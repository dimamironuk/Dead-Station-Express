using UnityEngine;
using DG.Tweening;

public class RailgunTurret : BaseTurret
{
    [Header("Turret Settings")]
    [SerializeField] private float _range = 15f;
    [SerializeField] private float _fireRate = 1f;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private Vector3 _rotationOffset = new Vector3(0f, -90f, 0f);

    [Header("Optimization Settings")]
    [SerializeField] private float _targetUpdateInterval = 0.25f;

    [Header("Laser Visuals")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _laserDuration = 0.12f;

    [Header("Recoil Settings")]
    [SerializeField] private Transform _gunTransform;
    [SerializeField] private float _recoilDistance = 0.6f;
    [SerializeField] private float _recoilDuration = 0.25f;
    [SerializeField] private Vector3 _recoilAxis = new Vector3(-1f, 0f, 0f);

    [Header("VFX & Audio")]
    [SerializeField] private ParticleSystem _shootParticle;
    [SerializeField] private AudioSource _shootAudioSource;

    [Header("References")]
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private Transform _partToRotate;
    [SerializeField] private Transform _muzzle;

    private float _fireCooldown;
    private float _targetUpdateTimer;
    private float _laserTimer;
    private Transform _targetEnemy;
    private Vector3 _initialGunLocalPos;

    private readonly Collider[] _hitColliders = new Collider[30];
    private readonly RaycastHit[] _raycastHits = new RaycastHit[30];

    private void Start()
    {
        if (_gunTransform != null) _initialGunLocalPos = _gunTransform.localPosition;
        if (_lineRenderer != null) _lineRenderer.enabled = false;
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

        if (_laserTimer > 0f)
        {
            _laserTimer -= Time.deltaTime;
            if (_laserTimer <= 0f && _lineRenderer != null)
            {
                _lineRenderer.enabled = false;
            }
        }

        if (_targetEnemy == null) return;

        bool isAimingAtTarget = RotateTowards(_targetEnemy.position);

        if (isAimingAtTarget && _fireCooldown <= 0f)
        {
            Shoot();
            _fireCooldown = 1f / _fireRate;
        }
    }

    private void FindNearestEnemy()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _range, _hitColliders, _enemyLayer);
        Transform nearest = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            float distance = Vector3.Distance(transform.position, _hitColliders[i].transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = _hitColliders[i].transform;
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

    private void Shoot()
    {
        Vector3 fireOrigin = _muzzle != null ? _muzzle.position : transform.position;
        Quaternion currentRotation = _partToRotate != null ? _partToRotate.rotation : transform.rotation;
        Vector3 fireDirection = currentRotation * Quaternion.Inverse(Quaternion.Euler(_rotationOffset)) * Vector3.forward;
        Vector3 endPosition = fireOrigin + fireDirection * _range;

        int hitCount = Physics.RaycastNonAlloc(fireOrigin, fireDirection, _raycastHits, _range, _enemyLayer);

        for (int i = 0; i < hitCount; i++)
        {
            if (_raycastHits[i].collider.TryGetComponent<Enemy>(out var enemy))
            {
                enemy.TakeDamage(CurrentStat);
            }
        }

        if (_lineRenderer != null)
        {
            _lineRenderer.SetPosition(0, fireOrigin);
            _lineRenderer.SetPosition(1, endPosition);
            _lineRenderer.enabled = true;
            _laserTimer = _laserDuration;
        }

        if (_shootParticle != null) _shootParticle.Play();
        if (_shootAudioSource != null) _shootAudioSource.Play();

        AnimateRecoil();
    }

    private void AnimateRecoil()
    {
        if (_gunTransform == null) return;

        _gunTransform.DOKill(true);
        _gunTransform.localPosition = _initialGunLocalPos;

        Vector3 targetRecoilPos = _initialGunLocalPos + (_recoilAxis.normalized * _recoilDistance);

        Sequence recoilSequence = DOTween.Sequence();
        recoilSequence.Append(_gunTransform.DOLocalMove(targetRecoilPos, _recoilDuration * 0.15f).SetEase(Ease.OutExpo));
        recoilSequence.Append(_gunTransform.DOLocalMove(_initialGunLocalPos, _recoilDuration * 0.85f).SetEase(Ease.OutBack));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _range);
    }
}