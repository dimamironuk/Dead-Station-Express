using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class TurretUI : MonoBehaviour
{
    [Header("Turret Settings")]
    [SerializeField] private GameObject _turretPrefab;
    [SerializeField] private GameObject _customPreviewPrefab;
    [SerializeField] private int _price = 10;

    [Header("UI Components")]
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Image _icon;

    private TurretPlacementController _placementController;
    private CurrencyManager _currencyManager;
    private TurretData _turretData;

    [Inject]
    public void Construct(TurretPlacementController placementController, CurrencyManager currencyManager)
    {
        _placementController = placementController;
        _currencyManager = currencyManager;
    }

    private void Awake()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        _button.onClick.AddListener(OnClickAddTurretButton);
    }

    private void OnEnable()
    {
        if (_currencyManager != null)
        {
            _currencyManager.OnNutsChanged += UpdateButtonState;
            UpdateButtonState(_currencyManager.CurrentNuts);
        }
    }

    private void OnDisable()
    {
        if (_currencyManager != null)
        {
            _currencyManager.OnNutsChanged -= UpdateButtonState;
        }
    }

    private void Start()
    {
        if (_priceText != null)
        {
            _priceText.text = _price.ToString();
        }

        if (_currencyManager != null)
        {
            UpdateButtonState(_currencyManager.CurrentNuts);
        }
    }

    public void OnClickAddTurretButton()
    {
        if (_currencyManager == null || _placementController == null)
        {
            return;
        }

        if (_currencyManager.CurrentNuts >= _price)
        {
            if (_turretData == null)
            {
                _turretData = new TurretData
                {
                    Prefab = _turretPrefab,
                    PrefabGreen = _customPreviewPrefab,
                    Price = _price,
                    Name = _nameText != null ? _nameText.text : "",
                    Icon = _icon != null ? _icon.sprite : null
                };
            }

            _placementController.BeginPlacement(_turretData);
        }
    }

    private void UpdateButtonState(int currentNuts)
    {
        if (_button != null)
        {
            _button.interactable = currentNuts >= _price;
        }
    }

    public void Setup(TurretData data)
    {
        _turretData = data;
        if (data == null) return;

        _turretPrefab = data.Prefab;
        _customPreviewPrefab = data.PrefabGreen;
        _price = data.Price;

        if (_nameText != null)
        {
            _nameText.text = data.Name;
        }

        if (_icon != null)
        {
            _icon.sprite = data.Icon;
        }

        if (_priceText != null)
        {
            _priceText.text = _price.ToString();
        }

        if (_currencyManager != null)
        {
            UpdateButtonState(_currencyManager.CurrentNuts);
        }
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnClickAddTurretButton);
        }
    }
}