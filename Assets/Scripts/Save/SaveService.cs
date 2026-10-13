using UnityEngine;
using Zenject;

public class SaveService : MonoBehaviour
{
    private SaveManager _saveManager;
    private CurrencyManager _currencyManager;
    private TrainEngine _trainEngine;
    private TrainBuilder _trainBuilder;
    private TrainShopUIController _shopController;
    private CreateTurretUI _createTurretUI;

    [Inject]
    public void Construct(
        SaveManager saveManager,
        CurrencyManager currencyManager,
        TrainEngine trainEngine,
        TrainBuilder trainBuilder,
        [InjectOptional] TrainShopUIController shopController,
        [InjectOptional] CreateTurretUI createTurretUI)
    {
        _saveManager = saveManager;
        _currencyManager = currencyManager;
        _trainEngine = trainEngine;
        _trainBuilder = trainBuilder;
        _shopController = shopController;
        _createTurretUI = createTurretUI;
    }

    private void Start()
    {
        LoadGame();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        if (_currencyManager != null)
        {
            data.nuts = _currencyManager.CurrentNuts;
            data.nutsBonus = _currencyManager.NutsBonus;
        }

        if (_trainEngine != null)
        {
            data.engineCurrentHealth = _trainEngine.CurrentHealth;
            data.engineMaxHealth = _trainEngine.MaxHealth;
            data.engineAutoRepairRate = _trainEngine.AutoRepairRate;
            data.engineRamDamage = _trainEngine.RamDamage;
        }

        if (_shopController != null)
        {
            data.healthLvl = _shopController.HealthLvl;
            data.autoRepairLvl = _shopController.AutoRepairLvl;
            data.boltsBonusLvl = _shopController.BoltsBonusLvl;
        }

        if (_createTurretUI != null)
        {
            data.purchasedTurrets = _createTurretUI.GetPurchasedTurretNames();
        }

        if (_trainBuilder != null)
        {
            data.wagons.Clear();

            foreach (var wagonObj in _trainBuilder.SpawnedWagons)
            {
                if (wagonObj == null) continue;

                if (wagonObj.TryGetComponent<Wagon>(out var wagon))
                {
                    string turretName = "";

                    if (wagon.HasTurret && wagon.InstalledTurret != null)
                    {
                        turretName = wagon.InstalledTurret.name.Replace("(Clone)", "").Trim();
                        Debug.Log($"[SaveService] «бер≥гаЇмо вагон ≥з вежею: '{turretName}'");
                    }

                    WagonSaveData wData = new WagonSaveData
                    {
                        currentHealth = wagon.CurrentHealth,
                        maxHealth = wagon.MaxHealth,
                        wagonLevel = wagon.WagonLevel,
                        turretLevel = wagon.TurretLevel,
                        turretPrefabName = turretName,
                        turretPrice = wagon.InstalledTurretPrice
                    };

                    data.wagons.Add(wData);
                }
            }
        }

        _saveManager.Save(data);
        Debug.Log($"[SaveService] √ра усп≥шно збережена. ¬агон≥в у збереженн≥: {data.wagons.Count}");
    }

    public void LoadGame()
    {
        if (!_saveManager.HasSave()) return;

        SaveData data = _saveManager.Load();

        if (_currencyManager != null)
        {
            _currencyManager.LoadData(data.nuts, data.nutsBonus);
        }

        if (_trainEngine != null)
        {
            _trainEngine.LoadData(data.engineCurrentHealth, data.engineMaxHealth, data.engineAutoRepairRate, data.engineRamDamage);
        }

        if (_shopController != null)
        {
            _shopController.LoadData(data.healthLvl, data.autoRepairLvl, data.boltsBonusLvl);
        }

        if (_createTurretUI != null)
        {
            _createTurretUI.LoadPurchasedTurrets(data.purchasedTurrets);
        }

        if (_trainBuilder != null)
        {
            _trainBuilder.LoadWagons(data.wagons);
        }
    }
}