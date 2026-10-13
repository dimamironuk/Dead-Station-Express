using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class ShopBoltsAdButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _buttonText;
    [SerializeField] private int _rewardBolts = 100;
    [SerializeField] private float _cooldownMinutes = 10f;

    private const string PREFS_KEY = "LastBoltsAdTime";

    private RewardedAdManager _adManager;
    private CurrencyManager _currencyManager;

    [Inject]
    public void Construct(RewardedAdManager adManager, CurrencyManager currencyManager)
    {
        _adManager = adManager;
        _currencyManager = currencyManager;
    }

    private void Start()
    {
        if (_button != null)
        {
            _button.onClick.AddListener(OnClicked);
        }
    }

    private void Update()
    {
        if (_button == null || _adManager == null) return;

        float remainingSeconds = GetRemainingCooldownSeconds();

        if (remainingSeconds > 0)
        {
            _button.interactable = false;
            if (_buttonText != null)
            {
                TimeSpan time = TimeSpan.FromSeconds(remainingSeconds);
                _buttonText.text = $"{time.Minutes:D2}:{time.Seconds:D2}";
            }
        }
        else
        {
            bool isReady = _adManager.IsAdReady();
            _button.interactable = isReady;

            if (_buttonText != null)
            {
                _buttonText.text = isReady
                    ? $"<b>+{_rewardBolts} Bolts</b>\n<color=#80FF80>Watch Ad (Free)</color>"
                    : "Loading...";
            }
        }
    }

    private void OnClicked()
    {
        if (GetRemainingCooldownSeconds() <= 0 && _adManager.IsAdReady())
        {
            _adManager.ShowRewardedAd(OnAdSuccess);
        }
    }

    private void OnAdSuccess()
    {
        _currencyManager.AddNuts(_rewardBolts);

        PlayerPrefs.SetString(PREFS_KEY, DateTime.UtcNow.Ticks.ToString());
        PlayerPrefs.Save();

        Debug.Log($"[Shop] Успішно додано {_rewardBolts} болтів!");
    }

    private float GetRemainingCooldownSeconds()
    {
        if (!PlayerPrefs.HasKey(PREFS_KEY)) return 0f;

        string savedTicks = PlayerPrefs.GetString(PREFS_KEY);
        if (long.TryParse(savedTicks, out long ticks))
        {
            DateTime lastTime = new DateTime(ticks, DateTimeKind.Utc);
            double elapsedSeconds = (DateTime.UtcNow - lastTime).TotalSeconds;
            double cooldownSeconds = _cooldownMinutes * 60f;

            if (elapsedSeconds < cooldownSeconds)
            {
                return (float)(cooldownSeconds - elapsedSeconds);
            }
        }

        return 0f;
    }
}