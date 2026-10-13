using System;
using System.Collections.Generic;

[Serializable]
public class WagonSaveData
{
    public float currentHealth;
    public float maxHealth;
    public int wagonLevel = 1;
    public int turretLevel = 1;
    public string turretPrefabName;
    public int turretPrice;
}

[Serializable]
public class SaveData
{
    public int nuts;
    public int nutsBonus;

    public float engineCurrentHealth;
    public float engineMaxHealth;
    public float engineAutoRepairRate;
    public float engineRamDamage;

    public int healthLvl = 1;
    public int autoRepairLvl = 1;
    public int boltsBonusLvl = 1;

    public List<string> purchasedTurrets = new List<string>();
    public List<WagonSaveData> wagons = new List<WagonSaveData>();
}