using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class JourneyUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _nutsText;
    [SerializeField] private TextMeshProUGUI _distanceText;
    [SerializeField] private Slider _progressBar;
    [SerializeField] private Button _startButton;
    [SerializeField] private GameObject _shopButton;

    [Header("Systems")]
    [SerializeField] private DialogController _dialogController;

    [Header("Distance Settings")]
    [SerializeField] private float _targetDistanceKm = 10f;

    private float _currentDistanceKm = 0f;

    private CurrencyManager _currencyManager;
    private JourneyManager _journeyManager;

    [Inject]
    public void Construct(CurrencyManager currencyManager, JourneyManager journeyManager)
    {
        _currencyManager = currencyManager;
        _journeyManager = journeyManager;
    }

    private void OnEnable()
    {
        if (_currencyManager != null)
            _currencyManager.OnNutsChanged += UpdateNutsUI;

        if (_journeyManager != null)
        {
            _journeyManager.OnProgressChanged += UpdateProgressFromManager;
            _journeyManager.OnStationDeparted += HandleDeparted;
            _journeyManager.OnStationReached += HandleReached;
        }

        if (_dialogController != null)
            _dialogController.OnDialogEnded += ShowStationUI;

        if (_startButton != null)
            _startButton.onClick.AddListener(OnStartButtonClicked);
    }

    private void OnDisable()
    {
        if (_currencyManager != null)
            _currencyManager.OnNutsChanged -= UpdateNutsUI;

        if (_journeyManager != null)
        {
            _journeyManager.OnProgressChanged -= UpdateProgressFromManager;
            _journeyManager.OnStationDeparted -= HandleDeparted;
            _journeyManager.OnStationReached -= HandleReached;
        }

        if (_dialogController != null)
            _dialogController.OnDialogEnded -= ShowStationUI;

        if (_startButton != null)
            _startButton.onClick.RemoveListener(OnStartButtonClicked);
    }

    private void Start()
    {
        UpdateNutsUI(_currencyManager != null ? _currencyManager.CurrentNuts : 0);
        UpdateDistanceUI(0f);
        ShowStationUI();
    }

    private void UpdateProgressFromManager(float progress)
    {
        _currentDistanceKm = progress * _targetDistanceKm;
        UpdateDistanceUI(progress);
    }

    private void OnStartButtonClicked()
    {
        if (_journeyManager != null)
            _journeyManager.DepartFromStation();
    }

    private void HandleDeparted()
    {
        HideStationUI();
    }

    private void HandleReached()
    {
    }

    private void ShowStationUI()
    {
        if (_startButton != null) _startButton.gameObject.SetActive(true);
        if (_shopButton != null) _shopButton.SetActive(true);
    }

    private void HideStationUI()
    {
        if (_startButton != null) _startButton.gameObject.SetActive(false);
        if (_shopButton != null) _shopButton.SetActive(false);
    }

    private void UpdateNutsUI(int nuts)
    {
        if (_nutsText != null)
            _nutsText.text = nuts.ToString("D4");
    }

    private void UpdateDistanceUI(float progress)
    {
        if (_distanceText != null)
            _distanceText.text = $"{_currentDistanceKm:F1} km";

        if (_progressBar != null)
            _progressBar.value = Mathf.Clamp01(progress);
    }
}