using System.Collections;
using UnityEngine;
using Zenject;

public class SpiderBossAI : Enemy
{
    private enum BossPhase { Behind, SideLeft, SideRight, Charge }

    [Header("Phase Settings")]
    [SerializeField] private float _phaseDuration = 7f;
    [SerializeField] private float _sideOffsetDistance = 8f;
    [SerializeField] private float _chargeDamage = 35f;

    [Header("Movement Settings")]
    [SerializeField] private float _keepDistanceBehind = 14f;
    [SerializeField] private float _bossMoveSpeed = 7f;
    [SerializeField] private float _bossRotationSpeed = 6f;
    [SerializeField] private float _modelRotationOffset = 0f;

    [Header("References & Visuals")]
    [SerializeField] private SpiderLegAnimator _legAnimator;
    [SerializeField] private Transform _turretBase;
    [SerializeField] private Transform _minigunBarrels;
    [SerializeField] private Vector3 _barrelSpinAxis = Vector3.forward;
    [SerializeField] private float _barrelSpinSpeed = 1200f;
    [SerializeField] private float _turretYawOffset = 0f;
    [SerializeField] private ParticleSystem _muzzleFlash;
    [SerializeField] private float _fireRate = 0.08f;
    [SerializeField] private float _bossDamage = 2f;

    private JourneyManager _journeyManager;
    private Transform _targetWagonTransform;
    private BossPhase _currentPhase = BossPhase.Behind;
    private float _nextFireTime;
    private float _phaseTimer;
    private bool _isCharging = false;

    [Inject]
    public void Construct(JourneyManager journeyManager)
    {
        _journeyManager = journeyManager;
    }

    private void OnEnable()
    {
        if (_journeyManager != null)
        {
            _journeyManager.IsBossActive = true;
        }
        _phaseTimer = _phaseDuration;
    }

    protected override void Update()
    {
        if (_isDead)
        {
            StopMuzzleFlash();
            return;
        }

        FindLastWagonTarget();
        HandlePhaseSwitching();
        HandleBossMovement();
        HandleWeaponAiming();
        HandleBossShooting();
    }

    private void HandlePhaseSwitching()
    {
        if (_isCharging) return;

        _phaseTimer -= Time.deltaTime;
        if (_phaseTimer <= 0)
        {
            _phaseTimer = _phaseDuration;
            // Випадковий вибір наступної фази
            _currentPhase = (BossPhase)Random.Range(0, 4);

            if (_currentPhase == BossPhase.Charge)
            {
                StartCoroutine(ChargeAttackRoutine());
            }
        }
    }

    private void HandleBossMovement()
    {
        if (_targetWagonTransform == null || _isCharging) return;

        Vector3 targetPosition = CalculateTargetPosition();
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget > 0.4f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, _bossMoveSpeed * Time.deltaTime);

            Vector3 moveDir = (targetPosition - transform.position);
            moveDir.y = 0f;
            if (moveDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir) * Quaternion.Euler(0, _modelRotationOffset, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * _bossRotationSpeed);
            }

            if (_legAnimator != null) _legAnimator.StartWalking();
        }
        else
        {
            if (_legAnimator != null) _legAnimator.StopWalking();
        }
    }

    private Vector3 CalculateTargetPosition()
    {
        Vector3 wagonPos = _targetWagonTransform.position;

        switch (_currentPhase)
        {
            case BossPhase.SideLeft:
                return new Vector3(wagonPos.x - _sideOffsetDistance, transform.position.y, wagonPos.z);

            case BossPhase.SideRight:
                return new Vector3(wagonPos.x + _sideOffsetDistance, transform.position.y, wagonPos.z);

            case BossPhase.Behind:
            default:
                return new Vector3(wagonPos.x, transform.position.y, wagonPos.z - _keepDistanceBehind);
        }
    }

    private IEnumerator ChargeAttackRoutine()
    {
        _isCharging = true;
        StopMuzzleFlash();

        // 1. Невелика затримка (бос прицілюється/готується до ривка)
        yield return new WaitForSeconds(0.8f);

        // 2. Стрімкий ривок убік вагона
        float chargeSpeed = _bossMoveSpeed * 2.5f;
        while (_targetWagonTransform != null)
        {
            Vector3 ramPos = _targetWagonTransform.position + new Vector3(0f, 0f, -2f);
            ramPos.y = transform.position.y;

            transform.position = Vector3.MoveTowards(transform.position, ramPos, chargeSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, ramPos) <= 1.2f)
            {
                // Нанесення шкоди при ударі
                ApplyDamageToTarget(_chargeDamage);
                break;
            }
            yield return null;
        }

        // 3. Відкат назад
        _currentPhase = BossPhase.Behind;
        _isCharging = false;
    }

    private void HandleWeaponAiming()
    {
        if (_targetWagonTransform == null || _turretBase == null || _isCharging) return;

        Vector3 dirToTarget = _targetWagonTransform.position - _turretBase.position;
        dirToTarget.y = 0f;

        if (dirToTarget != Vector3.zero)
        {
            Quaternion targetYaw = Quaternion.LookRotation(dirToTarget) * Quaternion.Euler(0, _turretYawOffset, 0);
            _turretBase.rotation = Quaternion.Slerp(_turretBase.rotation, targetYaw, Time.deltaTime * (_bossRotationSpeed * 2.5f));
        }
    }

    private void HandleBossShooting()
    {
        if (_targetWagonTransform == null || _isCharging)
        {
            StopMuzzleFlash();
            return;
        }

        if (_minigunBarrels != null)
        {
            _minigunBarrels.Rotate(_barrelSpinAxis * (_barrelSpinSpeed * Time.deltaTime), Space.Self);
        }

        if (_muzzleFlash != null && !_muzzleFlash.isPlaying)
        {
            _muzzleFlash.Play();
        }

        if (Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + _fireRate;
            ApplyDamageToTarget(_bossDamage);
        }
    }

    private void ApplyDamageToTarget(float damageAmount)
    {
        if (_targetWagonTransform == null) return;

        if (_targetWagonTransform.TryGetComponent<Wagon>(out var wagon))
        {
            wagon.TakeDamage(damageAmount);
        }
        else if (_targetWagonTransform.TryGetComponent<TrainEngine>(out var engine))
        {
            engine.TakeDamage(damageAmount);
        }
    }

    private void FindLastWagonTarget()
    {
        Wagon[] wagons = FindObjectsOfType<Wagon>();
        float minZ = float.MaxValue;
        Transform lastWagon = null;

        foreach (var wagon in wagons)
        {
            if (!wagon.gameObject.activeInHierarchy) continue;

            if (wagon.transform.position.z < minZ)
            {
                minZ = wagon.transform.position.z;
                lastWagon = wagon.transform;
            }
        }

        if (lastWagon == null && _trainEngine != null && _trainEngine.gameObject.activeInHierarchy)
        {
            lastWagon = _trainEngine.transform;
        }

        _targetWagonTransform = lastWagon;
    }

    private void StopMuzzleFlash()
    {
        if (_muzzleFlash != null && _muzzleFlash.isPlaying)
        {
            _muzzleFlash.Stop();
        }
    }

    protected override void Despawn()
    {
        if (_journeyManager != null)
        {
            _journeyManager.IsBossActive = false;
        }
        StopMuzzleFlash();
        base.Despawn();
    }
}