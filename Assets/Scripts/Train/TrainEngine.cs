using System;
using UnityEngine;
using Zenject;

public class TrainEngine : MonoBehaviour
{
    public event Action<float, float> OnHealthChanged;

    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _currentHealth;

    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public bool IsFullHealth => _currentHealth >= _maxHealth;

    public float AutoRepairRate { get; private set; } = 0f; 
    public float RamDamage { get; private set; } = 10f;     

    private GameOverPanel _gameOverPanel;
    private RailLoopController _railLoopController;

    [Inject]
    public void Construct(RailLoopController railLoopController, GameOverPanel gameOverPanel)
    {
        _railLoopController = railLoopController;
        _gameOverPanel = gameOverPanel;
    }

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    private void Start()
    {
        NotifyHealthChanged();
    }

    private void Update()
    {
        if (AutoRepairRate > 0 && _currentHealth < _maxHealth)
        {
            _currentHealth += AutoRepairRate * Time.deltaTime;
            _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
            NotifyHealthChanged();
        }
    }

    public void RepairFull()
    {
        _currentHealth = _maxHealth;
        NotifyHealthChanged();
    }

    public void AddMaxHealth(float amount)
    {
        _maxHealth += amount;
        _currentHealth += amount; 
        NotifyHealthChanged();
    }

    public void AddAutoRepairRate(float amount)
    {
        AutoRepairRate += amount;
    }

    public void AddRamDamage(float amount)
    {
        RamDamage += amount;
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);
        NotifyHealthChanged();

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    private void Die()
    {
        float traveledDistance = _railLoopController != null ? _railLoopController.TraveledDistance : 0f;

        if (_gameOverPanel != null)
        {
            _gameOverPanel.ShowGameOver(traveledDistance);
        }
    }
    public void LoadData(float currentHP, float maxHP, float autoRepair, float ramDamage)
    {
        _maxHealth = maxHP;
        _currentHealth = currentHP;
        AutoRepairRate = autoRepair;
        RamDamage = ramDamage;
        NotifyHealthChanged();
    }
}