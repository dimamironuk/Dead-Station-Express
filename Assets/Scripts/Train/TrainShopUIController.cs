using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class TrainShopUIController : MonoBehaviour
{
    private const string AD_PREFS_KEY = "LastRepairAdTime";

    [Header("HP Bar")]
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private TMP_Text _hpText;

    [Header("Ad Repair")]
    [SerializeField] private Button _adRepairButton;
    [SerializeField] private TMP_Text _adRepairText;
    [SerializeField] private float _adCooldownMinutes = 10f;

    [Header("Bolts Repair")]
    [SerializeField] private Button _boltsRepairButton;
    [SerializeField] private TMP_Text _boltsRepairText;
    [SerializeField] private int _repairCost = 50;

    [Header("Train Health Upgrade")]
    [SerializeField] private Button _healthUpgradeButton;
    [SerializeField] private TMP_Text _healthLvlCostText;
    [SerializeField] private TMP_Text _healthInfoText;
    [SerializeField] private float _healthStep = 20f;
    [SerializeField] private int _healthBaseCost = 100;
    [SerializeField] private int _healthMaxLvl = 10;

    [Header("Auto-Repair Upgrade")]
    [SerializeField] private Button _autoRepairUpgradeButton;
    [SerializeField] private TMP_Text _autoRepairLvlCostText;
    [SerializeField] private TMP_Text _autoRepairInfoText;
    [SerializeField] private float _autoRepairStep = 1f;
    [SerializeField] private int _autoRepairBaseCost = 100;
    [SerializeField] private int _autoRepairMaxLvl = 10;

    [Header("Extra Bolts Upgrade")]
    [SerializeField] private Button _boltsBonusUpgradeButton;
    [SerializeField] private TMP_Text _boltsBonusLvlCostText;
    [SerializeField] private TMP_Text _boltsBonusInfoText;
    [SerializeField] private int _boltsBonusStep = 1;
    [SerializeField] private int _boltsBonusBaseCost = 100;
    [SerializeField] private int _boltsBonusMaxLvl = 10;

    private int _healthLvl = 1;
    private int _autoRepairLvl = 1;
    private int _boltsBonusLvl = 1;

    private TrainEngine _trainEngine;
    private CurrencyManager _currencyManager;
    private RewardedAdManager _adManager;

    public int HealthLvl => _healthLvl;
    public int AutoRepairLvl => _autoRepairLvl;
    public int BoltsBonusLvl => _boltsBonusLvl;

    [Inject]
    public void Construct(TrainEngine trainEngine, CurrencyManager currencyManager, [InjectOptional] RewardedAdManager adManager)
    {
        _trainEngine = trainEngine;
        _currencyManager = currencyManager;
        _adManager = adManager;
    }

    private void OnEnable()
    {
        if (_trainEngine != null)
            _trainEngine.OnHealthChanged += UpdateHPUI;

        if (_currencyManager != null)
            _currencyManager.OnNutsChanged += OnCurrencyChanged;

        UpdateAllUI();
    }

    private void OnDisable()
    {
        if (_trainEngine != null)
            _trainEngine.OnHealthChanged -= UpdateHPUI;

        if (_currencyManager != null)
            _currencyManager.OnNutsChanged -= OnCurrencyChanged;
    }

    private void Start()
    {
        if (_adRepairButton != null)
            _adRepairButton.onClick.AddListener(OnAdRepairClicked);

        if (_boltsRepairButton != null)
            _boltsRepairButton.onClick.AddListener(BuyBoltsRepair);

        if (_healthUpgradeButton != null)
            _healthUpgradeButton.onClick.AddListener(BuyHealthUpgrade);

        if (_autoRepairUpgradeButton != null)
            _autoRepairUpgradeButton.onClick.AddListener(BuyAutoRepairUpgrade);

        if (_boltsBonusUpgradeButton != null)
            _boltsBonusUpgradeButton.onClick.AddListener(BuyBoltsBonusUpgrade);

        UpdateAllUI();
    }

    private void Update()
    {
        UpdateAdRepairUI();
    }

    public void LoadData(int healthLvl, int autoRepairLvl, int boltsBonusLvl)
    {
        _healthLvl = Mathf.Max(1, healthLvl);
        _autoRepairLvl = Mathf.Max(1, autoRepairLvl);
        _boltsBonusLvl = Mathf.Max(1, boltsBonusLvl);

        UpdateAllUI();
    }

    private void OnAdRepairClicked()
    {
        if (_trainEngine == null || _adManager == null) return;

        if (GetRemainingAdCooldownSeconds() <= 0 && !_trainEngine.IsFullHealth && _adManager.IsAdReady())
        {
            _adManager.ShowRewardedAd(OnAdSuccess);
        }
    }

    private void OnAdSuccess()
    {
        _trainEngine.RepairFull();
        PlayerPrefs.SetString(AD_PREFS_KEY, DateTime.UtcNow.Ticks.ToString());
        PlayerPrefs.Save();
        UpdateAllUI();
    }

    private float GetRemainingAdCooldownSeconds()
    {
        if (!PlayerPrefs.HasKey(AD_PREFS_KEY)) return 0f;

        string savedTicks = PlayerPrefs.GetString(AD_PREFS_KEY);
        if (long.TryParse(savedTicks, out long ticks))
        {
            DateTime lastTime = new DateTime(ticks, DateTimeKind.Utc);
            double elapsedSeconds = (DateTime.UtcNow - lastTime).TotalSeconds;
            double cooldownSeconds = _adCooldownMinutes * 60f;

            if (elapsedSeconds < cooldownSeconds)
            {
                return (float)(cooldownSeconds - elapsedSeconds);
            }
        }

        return 0f;
    }

    private void UpdateAdRepairUI()
    {
        if (_adRepairButton == null || _trainEngine == null) return;

        if (_trainEngine.IsFullHealth)
        {
            _adRepairButton.interactable = false;
            if (_adRepairText != null)
                _adRepairText.text = "<b>Full Repair</b>\n<color=#808080>Full HP</color>";
            return;
        }

        float remainingSeconds = GetRemainingAdCooldownSeconds();
        if (remainingSeconds > 0)
        {
            _adRepairButton.interactable = false;
            if (_adRepairText != null)
            {
                TimeSpan time = TimeSpan.FromSeconds(remainingSeconds);
                _adRepairText.text = $"<b>Full Repair</b>\n<color=#FF5555>{time.Minutes:D2}:{time.Seconds:D2}</color>";
            }
        }
        else
        {
            bool isReady = _adManager != null && _adManager.IsAdReady();
            _adRepairButton.interactable = isReady;

            if (_adRepairText != null)
            {
                _adRepairText.text = isReady
                    ? "<b>Full Repair</b>\n<color=#80FF80>Watch Ad (Free)</color>"
                    : "<b>Full Repair</b>\n<color=#CCCCCC>Loading...</color>";
            }
        }
    }

    private void BuyBoltsRepair()
    {
        if (_trainEngine == null || _trainEngine.IsFullHealth) return;

        if (_currencyManager.TrySpendNuts(_repairCost))
        {
            _trainEngine.RepairFull();
            UpdateAllUI();
        }
    }

    private void BuyHealthUpgrade()
    {
        if (_healthLvl >= _healthMaxLvl) return;

        int cost = _healthBaseCost * _healthLvl;
        if (_currencyManager.TrySpendNuts(cost))
        {
            _trainEngine.AddMaxHealth(_healthStep);
            _healthLvl++;
            UpdateAllUI();
        }
    }

    private void BuyAutoRepairUpgrade()
    {
        if (_autoRepairLvl >= _autoRepairMaxLvl) return;

        int cost = _autoRepairBaseCost * _autoRepairLvl;
        if (_currencyManager.TrySpendNuts(cost))
        {
            _trainEngine.AddAutoRepairRate(_autoRepairStep);
            _autoRepairLvl++;
            UpdateAllUI();
        }
    }

    private void BuyBoltsBonusUpgrade()
    {
        if (_boltsBonusLvl >= _boltsBonusMaxLvl) return;

        int cost = _boltsBonusBaseCost * _boltsBonusLvl;
        if (_currencyManager.TrySpendNuts(cost))
        {
            _currencyManager.AddNutsBonus(_boltsBonusStep);
            _boltsBonusLvl++;
            UpdateAllUI();
        }
    }

    private void OnCurrencyChanged(int newNuts)
    {
        UpdateAllUI();
    }

    private void UpdateHPUI(float current, float max)
    {
        if (_hpSlider != null)
        {
            _hpSlider.maxValue = max;
            _hpSlider.value = current;
        }

        if (_hpText != null)
            _hpText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";

        if (_boltsRepairButton != null)
            _boltsRepairButton.interactable = !_trainEngine.IsFullHealth && _currencyManager.CurrentNuts >= _repairCost;
    }

    private void UpdateAllUI()
    {
        if (_trainEngine == null || _currencyManager == null) return;

        UpdateHPUI(_trainEngine.CurrentHealth, _trainEngine.MaxHealth);
        UpdateAdRepairUI();

        if (_boltsRepairText != null)
            _boltsRepairText.text = $"<b>Full Repair</b>\n<color=#FFCC00>{_repairCost}</color>";

        UpdateHealthUpgradeUI();
        UpdateAutoRepairUpgradeUI();
        UpdateBoltsBonusUpgradeUI();
    }

    private void UpdateHealthUpgradeUI()
    {
        if (_healthLvl >= _healthMaxLvl)
        {
            if (_healthLvlCostText != null)
                _healthLvlCostText.text = $"<b>MAX</b>\n<color=#808080>Lvl. {_healthLvl}</color>";

            if (_healthInfoText != null)
                _healthInfoText.text = $"<b>Train Health</b>\n<color=#CCCCCC>Current: {_trainEngine.MaxHealth} HP</color>\n<color=#FFCC00>MAX LEVEL</color>";

            if (_healthUpgradeButton != null)
                _healthUpgradeButton.interactable = false;
        }
        else
        {
            int healthCost = _healthBaseCost * _healthLvl;

            if (_healthLvlCostText != null)
                _healthLvlCostText.text = $"<b>Lvl. {_healthLvl}</b>\n<color=#FFCC00>{healthCost}</color>";

            if (_healthInfoText != null)
                _healthInfoText.text = $"<b>Train Health</b>\n<color=#CCCCCC>Current: {_trainEngine.MaxHealth} HP</color>\n<color=#80FF80>Next: {_trainEngine.MaxHealth + _healthStep} HP (+{_healthStep})</color>";

            if (_healthUpgradeButton != null)
                _healthUpgradeButton.interactable = _currencyManager.CurrentNuts >= healthCost;
        }
    }

    private void UpdateAutoRepairUpgradeUI()
    {
        if (_autoRepairLvl >= _autoRepairMaxLvl)
        {
            if (_autoRepairLvlCostText != null)
                _autoRepairLvlCostText.text = $"<b>MAX</b>\n<color=#808080>Lvl. {_autoRepairLvl}</color>";

            if (_autoRepairInfoText != null)
                _autoRepairInfoText.text = $"<b>Auto-Repair</b>\n<color=#CCCCCC>Current: {_trainEngine.AutoRepairRate} HP/sec</color>\n<color=#FFCC00>MAX LEVEL</color>";

            if (_autoRepairUpgradeButton != null)
                _autoRepairUpgradeButton.interactable = false;
        }
        else
        {
            int autoCost = _autoRepairBaseCost * _autoRepairLvl;

            if (_autoRepairLvlCostText != null)
                _autoRepairLvlCostText.text = $"<b>Lvl. {_autoRepairLvl}</b>\n<color=#FFCC00>{autoCost}</color>";

            if (_autoRepairInfoText != null)
                _autoRepairInfoText.text = $"<b>Auto-Repair</b>\n<color=#CCCCCC>Current: {_trainEngine.AutoRepairRate} HP/sec</color>\n<color=#80FF80>Next: {_trainEngine.AutoRepairRate + _autoRepairStep} HP/sec (+{_autoRepairStep})</color>";

            if (_autoRepairUpgradeButton != null)
                _autoRepairUpgradeButton.interactable = _currencyManager.CurrentNuts >= autoCost;
        }
    }

    private void UpdateBoltsBonusUpgradeUI()
    {
        if (_boltsBonusLvl >= _boltsBonusMaxLvl)
        {
            if (_boltsBonusLvlCostText != null)
                _boltsBonusLvlCostText.text = $"<b>MAX</b>\n<color=#808080>Lvl. {_boltsBonusLvl}</color>";

            if (_boltsBonusInfoText != null)
                _boltsBonusInfoText.text = $"<b>Extra Bolts</b>\n<color=#CCCCCC>Current: +{_currencyManager.NutsBonus} Bolts</color>\n<color=#FFCC00>MAX LEVEL</color>";

            if (_boltsBonusUpgradeButton != null)
                _boltsBonusUpgradeButton.interactable = false;
        }
        else
        {
            int boltsCost = _boltsBonusBaseCost * _boltsBonusLvl;

            if (_boltsBonusLvlCostText != null)
                _boltsBonusLvlCostText.text = $"<b>Lvl. {_boltsBonusLvl}</b>\n<color=#FFCC00>{boltsCost}</color>";

            if (_boltsBonusInfoText != null)
                _boltsBonusInfoText.text = $"<b>Extra Bolts</b>\n<color=#CCCCCC>Current: +{_currencyManager.NutsBonus} Bolts</color>\n<color=#80FF80>Next: +{_currencyManager.NutsBonus + _boltsBonusStep} Bolts (+{_boltsBonusStep})</color>";

            if (_boltsBonusUpgradeButton != null)
                _boltsBonusUpgradeButton.interactable = _currencyManager.CurrentNuts >= boltsCost;
        }
    }
}