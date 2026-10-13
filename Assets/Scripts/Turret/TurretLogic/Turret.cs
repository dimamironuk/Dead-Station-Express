using UnityEngine;
using DG.Tweening;

public class Turret : BaseTurret
{
    [Header("Turret Settings")]
    [SerializeField] private float _range = 10f;
    [SerializeField] private float _fireRate = 2f;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private Vector3 _rotationOffset = new Vector3(0f, -90f, 0f);

    [Header("Optimization Settings")]
    [SerializeField] private float _targetUpdateInterval = 0.25f;

    [Header("Recoil Settings")]
    [SerializeField] private float _recoilDistance = 0.25f;
    [SerializeField] private float _recoilDuration = 0.12f;
    [SerializeField] private Vector3 _recoilDirection = new Vector3(-1f, 0f, 0f);

    [Header("VFX & Audio")]
    [SerializeField] private ParticleSystem _shootParticle;
    [SerializeField] private AudioSource _shootAudioSource;

    [Header("References")]
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private Transform _partToRotate;
    [SerializeField] private Transform _muzzle;

    private float _fireCooldown;
    private float _targetUpdateTimer;
    private Enemy _targetEnemy;
    private Vector3 _initialMuzzleLocalPos;

    private readonly Collider[] _hitColliders = new Collider[30];

    private void Start()
    {
        if (_muzzle != null)
        {
            _initialMuzzleLocalPos = _muzzle.localPosition;
        }
    }

    private void Update()
    {
        _fireCooldown -= Time.deltaTime;
        _targetUpdateTimer -= Time.deltaTime;

        if (_targetUpdateTimer <= 0f)
        {
            FindNearestEnemy();
            _targetUpdateTimer = _targetUpdateInterval;
        }

        if (_targetEnemy == null) return;

        bool isAimingAtTarget = RotateTowards(_targetEnemy.transform.position);

        if (isAimingAtTarget && _fireCooldown <= 0f)
        {
            Shoot(_targetEnemy);
            _fireCooldown = 1f / _fireRate;
        }
    }

    private void FindNearestEnemy()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _range, _hitColliders, _enemyLayer);
        Enemy nearest = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            if (_hitColliders[i].TryGetComponent<Enemy>(out var enemy))
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = enemy;
                }
            }
        }

        _targetEnemy = nearest;
    }

    private bool RotateTowards(Vector3 targetPosition)
    {
        if (_partToRotate == null) return true;

        Vector3 direction = targetPosition - _partToRotate.position;
        direction.y = 0f;

        if (direction == Vector3.zero) return true;

        Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(_rotationOffset);
        _partToRotate.rotation = Quaternion.Slerp(_partToRotate.rotation, targetRotation, Time.deltaTime * _rotationSpeed);

        return Quaternion.Angle(_partToRotate.rotation, targetRotation) < 15f;
    }

    private void Shoot(Enemy enemy)
    {
        enemy.TakeDamage(CurrentStat);

        if (_shootParticle != null) _shootParticle.Play();
        if (_shootAudioSource != null) _shootAudioSource.Play();

        AnimateRecoil();
    }

    private void AnimateRecoil()
    {
        if (_muzzle == null) return;

        _muzzle.DOKill(true);
        _muzzle.localPosition = _initialMuzzleLocalPos;

        Vector3 targetRecoilPos = _initialMuzzleLocalPos + (_recoilDirection.normalized * _recoilDistance);

        Sequence recoilSequence = DOTween.Sequence();
        recoilSequence.Append(_muzzle.DOLocalMove(targetRecoilPos, _recoilDuration * 0.15f).SetEase(Ease.OutExpo));
        recoilSequence.Append(_muzzle.DOLocalMove(_initialMuzzleLocalPos, _recoilDuration * 0.85f).SetEase(Ease.OutBack));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _range);
    }
}