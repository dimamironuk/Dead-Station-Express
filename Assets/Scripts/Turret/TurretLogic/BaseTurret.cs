using UnityEngine;

public class BaseTurret : MonoBehaviour
{
    [Header("Turret Base Info & Upgrades")]
    [SerializeField] private Sprite _icon;
    [SerializeField] protected string _turretName = "Turret";
    [SerializeField] protected string _statName = "DMG";
    [SerializeField] protected float _baseValue = 25f;
    [SerializeField] protected float _valuePerLevel = 5f;
    [SerializeField] protected string _unit = "";

    public string TurretName => _turretName;
    public string StatName => _statName;
    public string Unit => _unit;
    public Sprite TurretIcon => _icon;

    public int CurrentLevel { get; private set; } = 1;

    public virtual void SetLevel(int level)
    {
        CurrentLevel = level;
    }

    public virtual float GetStatForLevel(int level)
    {
        return _baseValue + (level - 1) * _valuePerLevel;
    }

    public float CurrentStat => GetStatForLevel(CurrentLevel);
}