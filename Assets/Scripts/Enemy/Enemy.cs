using UnityEngine;
using Zenject;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _speed = 3f;
    [SerializeField] private float _sideOffset = 2.5f;

    [Header("Behavior Settings")]
    [SerializeField] private float _activationDistance = 25f;
    [SerializeField] private float _despawnDistanceBehind = 15f;

    [Header("Attack Settings")]
    [SerializeField] private float _maxHealth = 60f;
    [SerializeField] protected float _currentHealth;
    [SerializeField] private float _attackRange = 0.8f;
    [SerializeField] private float _damage = 15f;
    [SerializeField] private float _attackInterval = 1f;

    [Header("Reward Settings")]
    [SerializeField] private int _nutsReward = 1;

    [Header("Target Info (Debug)")]
    [SerializeField] private Transform _target;
    [SerializeField] private bool _isActivated = false;

    protected float _lastAttackTime;
    protected float _targetSearchTimer;
    protected bool _isDead;

    protected CurrencyManager _currencyManager;
    protected RailLoopController _railLoopController;
    protected AnimationRobot _animationRobot;
    protected TrainEngine _trainEngine;

    [Inject]
    public void Construct(CurrencyManager currencyManager, RailLoopController railLoopController, TrainEngine trainEngine)
    {
        _currencyManager = currencyManager;
        _railLoopController = railLoopController;
        _trainEngine = trainEngine;
    }

    private void Awake()
    {
        _animationRobot = GetComponent<AnimationRobot>();
    }

    protected virtual void OnEnable()
    {
        _currentHealth = _maxHealth;
        _isActivated = false;
        _isDead = false;
        _target = null;
        _targetSearchTimer = 0f;

        if (_animationRobot != null)
        {
            _animationRobot.SetMoving(false);
        }
    }

    protected virtual void Update()
    {
        if (_isDead) return;

        float worldSpeed = _railLoopController != null ? _railLoopController.CurrentSpeed : 0f;
        transform.position += Vector3.back * (worldSpeed * Time.deltaTime);

        if (!_isActivated)
        {
            float distToEngine = _trainEngine != null ? Vector3.Distance(transform.position, _trainEngine.transform.position) : float.MaxValue;

            if (distToEngine <= _activationDistance)
            {
                _isActivated = true;
                SelectClosestTarget();
            }
            else
            {
                if (_animationRobot != null) _animationRobot.SetMoving(false);
                return;
            }
        }

        _targetSearchTimer += Time.deltaTime;
        if (_targetSearchTimer >= 0.25f || _target == null || !_target.gameObject.activeInHierarchy)
        {
            _targetSearchTimer = 0f;
            SelectClosestTarget();
        }

        if (_target == null) return;

        if (transform.position.z < _target.position.z - _despawnDistanceBehind)
        {
            Despawn();
            return;
        }

        Vector3 attackPosition = GetAttackPosition();
        float distanceToAttackPoint = Vector3.Distance(transform.position, attackPosition);

        if (distanceToAttackPoint > _attackRange)
        {
            if (_animationRobot != null) _animationRobot.SetMoving(true);

            transform.position = Vector3.MoveTowards(transform.position, attackPosition, _speed * Time.deltaTime);

            Vector3 moveDirection = attackPosition - transform.position;
            moveDirection.y = 0f;
            if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
        }
        else
        {
            if (_animationRobot != null) _animationRobot.SetMoving(false);

            Vector3 lookDirection = _target.position - transform.position;
            lookDirection.y = 0f;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }

            if (Time.time >= _lastAttackTime + _attackInterval)
            {
                AttackTarget();
                _lastAttackTime = Time.time;
            }
        }
    }

    private void SelectClosestTarget()
    {
        float minDistance = float.MaxValue;
        Transform closest = null;

        if (_trainEngine != null && _trainEngine.gameObject.activeInHierarchy)
        {
            minDistance = Vector3.Distance(transform.position, _trainEngine.transform.position);
            closest = _trainEngine.transform;
        }

        var wagons = FindObjectsOfType<Wagon>();
        foreach (var wagon in wagons)
        {
            if (!wagon.gameObject.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, wagon.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = wagon.transform;
            }
        }

        _target = closest;
    }

    private Vector3 GetAttackPosition()
    {
        Vector3 dirFromTarget = transform.position - _target.position;
        dirFromTarget.y = 0f;

        if (dirFromTarget == Vector3.zero)
            dirFromTarget = Vector3.forward;

        Vector3 attackPoint = _target.position + dirFromTarget.normalized * _sideOffset;
        attackPoint.y = transform.position.y;

        return attackPoint;
    }

    private void AttackTarget()
    {
        if (_target == null) return;

        if (_animationRobot != null)
        {
            _animationRobot.PlayAttackAnimation();
        }

        if (_target.TryGetComponent<Wagon>(out var wagon))
        {
            wagon.TakeDamage(_damage);
        }
        else if (_target.TryGetComponent<TrainEngine>(out var engine))
        {
            engine.TakeDamage(_damage);
        }
    }

    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (_currencyManager != null)
        {
            _currencyManager.AddNuts(_nutsReward);
        }

        if (_animationRobot != null)
        {
            _animationRobot.PlayDeathAnimation(Despawn);
        }
        else
        {
            Despawn();
        }
    }

    protected virtual void Despawn()
    {
        gameObject.SetActive(false);
    }
}