using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Zenject;

public class WagonUpgradeUI : MonoBehaviour
{
    [Header("Window & Background Settings")]
    [SerializeField] private RectTransform _panelWindow;
    [SerializeField] private Button _overlayCloseButton;

    [Header("Turret UI Elements")]
    [SerializeField] private GameObject _bgTurret;
    [SerializeField] private Image _turretIconImage;

    [Header("Buttons")]
    [SerializeField] private Button _upgradeWagonButton;
    [SerializeField] private Button _upgradeTurretButton;

    [Header("Slider Text")]
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private TextMeshProUGUI _hpSliderText;

    [Header("Wagon Info & Cost Text")]
    [SerializeField] private TextMeshProUGUI _wagonCostText;
    [SerializeField] private TextMeshProUGUI _wagonInfoText;

    [Header("Turret Info & Cost Text")]
    [SerializeField] private TextMeshProUGUI _turretCostText;
    [SerializeField] private TextMeshProUGUI _turretInfoText;

    private Wagon _currentWagon;
    private Tween _animationTween;
    private CurrencyManager _currencyManager;

    [Inject]
    public void Construct([InjectOptional] CurrencyManager currencyManager)
    {
        _currencyManager = currencyManager;
    }

    private void Awake()
    {
        if (_overlayCloseButton != null) _overlayCloseButton.onClick.AddListener(Close);
        if (_upgradeWagonButton != null) _upgradeWagonButton.onClick.AddListener(OnUpgradeWagonClicked);
        if (_upgradeTurretButton != null) _upgradeTurretButton.onClick.AddListener(OnUpgradeTurretClicked);
    }

    public void Open(Wagon wagon)
    {
        _currentWagon = wagon;
        gameObject.SetActive(true);

        if (_panelWindow != null)
        {
            _panelWindow.gameObject.SetActive(true);
            _animationTween?.Kill();
            _panelWindow.localScale = Vector3.zero;
            _animationTween = _panelWindow.DOScale(Vector3.one, 0.35f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (_currentWagon == null) return;

        bool hasTurret = _currentWagon.HasTurret;

        if (_bgTurret != null) _bgTurret.SetActive(hasTurret);
        if (_upgradeTurretButton != null) _upgradeTurretButton.gameObject.SetActive(hasTurret);

        int currentHp = Mathf.CeilToInt(_currentWagon.CurrentHealth);
        int maxHp = Mathf.CeilToInt(_currentWagon.MaxHealth);
        int nextMaxHp = maxHp + 20;

        int wagonCost = _currentWagon.WagonLevel * 100;
        int turretCost = _currentWagon.TurretLevel * 150;

        if (_hpSlider != null)
        {
            _hpSlider.maxValue = _currentWagon.MaxHealth;
            _hpSlider.value = _currentWagon.CurrentHealth;
        }

        if (_hpSliderText != null)
        {
            _hpSliderText.text = $"{currentHp} / {maxHp}";
        }

        if (_wagonCostText != null) _wagonCostText.text = wagonCost.ToString();

        if (_wagonInfoText != null)
        {
            _wagonInfoText.text = $"<b>Wagon (Lvl {_currentWagon.WagonLevel})</b>\n" +
                                  $"Max HP: {maxHp}\n" +
                                  $"<color=#00FF00>Next: {nextMaxHp} HP (+20)</color>";
        }

        if (hasTurret)
        {
            UpdateTurretIcon();

            if (_turretCostText != null) _turretCostText.text = turretCost.ToString();

            if (_turretInfoText != null)
            {
                BaseTurret turret = _currentWagon.InstalledBaseTurret;

                if (turret != null)
                {
                    int lvl = _currentWagon.TurretLevel;
                    float currentStat = turret.CurrentStat;
                    float nextStat = turret.GetStatForLevel(lvl + 1);
                    float statDiff = nextStat - currentStat;

                    _turretInfoText.text = $"<b>{turret.TurretName} (Lvl {lvl})</b>\n" +
                                           $"{turret.StatName}: {currentStat:F0}{turret.Unit}\n" +
                                           $"<color=#00FF00>Next: {nextStat:F0}{turret.Unit} (+{statDiff:F0})</color>";
                }
                else
                {
                    _turretInfoText.text = $"<b>Turret (Lvl {_currentWagon.TurretLevel})</b>\n" +
                                           $"<color=#00FF00>Next: Lvl {_currentWagon.TurretLevel + 1}</color>";
                }
            }
        }
    }

    private void UpdateTurretIcon()
    {
        if (_turretIconImage == null || _currentWagon == null) return;

        Sprite iconToUse = null;

        if (_currentWagon.InstalledBaseTurret != null && _currentWagon.InstalledBaseTurret.TurretIcon != null)
        {
            iconToUse = _currentWagon.InstalledBaseTurret.TurretIcon;
        }
        else if (_currentWagon.InstalledTurretIcon != null)
        {
            iconToUse = _currentWagon.InstalledTurretIcon;
        }

        if (iconToUse != null)
        {
            _turretIconImage.sprite = iconToUse;
            _turretIconImage.gameObject.SetActive(true);
        }
    }

    private void OnUpgradeWagonClicked()
    {
        if (_currentWagon == null) return;

        int cost = _currentWagon.WagonLevel * 100;
        if (_currencyManager == null || _currencyManager.TrySpendNuts(cost))
        {
            _currentWagon.UpgradeWagon(20f);
            RefreshUI();
        }
    }

    private void OnUpgradeTurretClicked()
    {
        if (_currentWagon == null || !_currentWagon.HasTurret) return;

        int cost = _currentWagon.TurretLevel * 150;
        if (_currencyManager == null || _currencyManager.TrySpendNuts(cost))
        {
            _currentWagon.UpgradeTurret();
            RefreshUI();
        }
    }

    public void Close()
    {
        _animationTween?.Kill();
        if (_panelWindow != null)
        {
            _animationTween = _panelWindow.DOScale(Vector3.zero, 0.25f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _panelWindow.gameObject.SetActive(false);
                    gameObject.SetActive(false);
                    _currentWagon = null;
                });
        }
        else
        {
            gameObject.SetActive(false);
            _currentWagon = null;
        }
    }

    private void OnDestroy()
    {
        _animationTween?.Kill();
    }
}