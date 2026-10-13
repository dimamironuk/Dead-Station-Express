using UnityEngine;

public class Flamethrower : BaseTurret
{
    [Header("Flamethrower Settings")]
    [SerializeField] private float _range = 8f;
    [SerializeField] private float _rotationSpeed = 8f;
    [SerializeField] private Vector3 _rotationOffset = new Vector3(0f, -90f, 0f);

    [Header("Optimization Settings")]
    [SerializeField] private float _targetUpdateInterval = 0.25f;
    [SerializeField] private float _burnTickInterval = 0.1f;

    [Header("VFX & Audio")]
    [SerializeField] private ParticleSystem _flameParticle;
    [SerializeField] private AudioSource _flameAudioSource;

    [Header("References")]
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private Transform _partToRotate;

    private bool _isFiring;
    private Transform _targetEnemy;
    private float _targetUpdateTimer;
    private float _burnTickTimer;

    private readonly Collider[] _hitColliders = new Collider[30];

    private void Update()
    {
        _targetUpdateTimer -= Time.deltaTime;
        if (_targetUpdateTimer <= 0f)
        {
            FindNearestEnemy();
            _targetUpdateTimer = _targetUpdateInterval;
        }

        if (_targetEnemy == null)
        {
            StopFiring();
            return;
        }

        bool isAimingAtTarget = RotateTowards(_targetEnemy.position);

        if (isAimingAtTarget)
        {
            StartFiring();

            _burnTickTimer -= Time.deltaTime;
            if (_burnTickTimer <= 0f)
            {
                BurnEnemiesInArea();
                _burnTickTimer = _burnTickInterval;
            }
        }
        else
        {
            StopFiring();
        }
    }

    private void FindNearestEnemy()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _range, _hitColliders, _enemyLayer);

        Transform nearest = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _hitColliders[i];
            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = hit.transform;
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

    private void StartFiring()
    {
        if (_isFiring) return;

        _isFiring = true;

        if (_flameParticle != null && !_flameParticle.isPlaying)
        {
            _flameParticle.Play();
        }

        if (_flameAudioSource != null && !_flameAudioSource.isPlaying)
        {
            _flameAudioSource.Play();
        }
    }

    private void StopFiring()
    {
        if (!_isFiring) return;

        _isFiring = false;

        if (_flameParticle != null && _flameParticle.isPlaying)
        {
            _flameParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (_flameAudioSource != null && _flameAudioSource.isPlaying)
        {
            _flameAudioSource.Stop();
        }
    }

    private void BurnEnemiesInArea()
    {
        float damageToApply = CurrentStat * _burnTickInterval;
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _range, _hitColliders, _enemyLayer);

        for (int i = 0; i < hitCount; i++)
        {
            if (_hitColliders[i].TryGetComponent<Enemy>(out var enemy))
            {
                enemy.TakeDamage(damageToApply);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _range);
    }
}