using UnityEngine;

public class TeslaTurret : BaseTurret
{
    [Header("Attack Settings")]
    [SerializeField] private float _range = 10f;
    [SerializeField] private float _fireRate = 1.5f;
    [SerializeField] private float _aoeRadius = 3.5f;
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Optimization Settings")]
    [SerializeField] private float _targetUpdateInterval = 0.25f;

    [Header("VFX & Audio")]
    [SerializeField] private ParticleSystem _lightningParticles;
    [SerializeField] private AudioSource _attackAudioSource;

    [Header("References")]
    [SerializeField] private Transform _partToRotate;

    [Header("Rotation Settings")]
    [SerializeField] private float _rotationSpeed = 12f;

    private Transform _targetEnemy;
    private float _fireCooldown = 0f;
    private float _targetUpdateTimer = 0f;

    private readonly Collider[] _hitColliders = new Collider[30];

    private void Update()
    {
        _targetUpdateTimer -= Time.deltaTime;
        if (_targetUpdateTimer <= 0f)
        {
            FindTarget();
            _targetUpdateTimer = _targetUpdateInterval;
        }

        if (_targetEnemy != null)
        {
            RotateToTarget();

            _fireCooldown -= Time.deltaTime;
            if (_fireCooldown <= 0f)
            {
                Attack();
                _fireCooldown = 1f / _fireRate;
            }
        }
    }

    private void FindTarget()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _range, _hitColliders, _enemyLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        for (int i = 0; i < hitCount; i++)
        {
            float distance = Vector3.Distance(transform.position, _hitColliders[i].transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = _hitColliders[i].transform;
            }
        }

        _targetEnemy = closestEnemy;
    }

    private void RotateToTarget()
    {
        Transform objToRotate = _partToRotate != null ? _partToRotate : transform;
        Vector3 direction = _targetEnemy.position - objToRotate.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            objToRotate.rotation = Quaternion.Slerp(objToRotate.rotation, lookRotation, Time.deltaTime * _rotationSpeed);
        }
    }

    private void Attack()
    {
        if (_lightningParticles != null) _lightningParticles.Play();
        if (_attackAudioSource != null) _attackAudioSource.Play();

        int hitCount = Physics.OverlapSphereNonAlloc(_targetEnemy.position, _aoeRadius, _hitColliders, _enemyLayer);
        for (int i = 0; i < hitCount; i++)
        {
            if (_hitColliders[i].TryGetComponent<Enemy>(out var enemy))
            {
                enemy.TakeDamage(CurrentStat);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _range);

        if (_targetEnemy != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_targetEnemy.position, _aoeRadius);
        }
    }
}