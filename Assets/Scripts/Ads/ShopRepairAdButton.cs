using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class ShopRepairAdButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _buttonText;
    [SerializeField] private float _cooldownMinutes = 10f;

    private const string PREFS_KEY = "LastRepairAdTime";

    private RewardedAdManager _adManager;
    private TrainEngine _trainEngine;

    [Inject]
    public void Construct(RewardedAdManager adManager, TrainEngine trainEngine)
    {
        _adManager = adManager;
        _trainEngine = trainEngine;
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
        if (_button == null || _adManager == null || _trainEngine == null) return;

        // 1. Перевірка: Якщо HP вже повне
        if (_trainEngine.IsFullHealth)
        {
            _button.interactable = false;
            if (_buttonText != null) _buttonText.text = "HP Повне";
            return;
        }

        // 2. Перевірка: Таймер кулдауну (10 хвилин)
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
            // 3. Якщо кулдаун минув — перевіряємо готовність реклами
            bool isReady = _adManager.IsAdReady();
            _button.interactable = isReady;

            if (_buttonText != null)
            {
                _buttonText.text = isReady ? "Полагодити поїзд (Реклама)" : "Завантаження...";
            }
        }
    }

    private void OnClicked()
    {
        if (GetRemainingCooldownSeconds() <= 0 && !_trainEngine.IsFullHealth && _adManager.IsAdReady())
        {
            _adManager.ShowRewardedAd(OnAdSuccess);
        }
    }

    private void OnAdSuccess()
    {
        // Відновлюємо HP
        _trainEngine.RepairFull();

        // Фіксуємо час перегляду
        PlayerPrefs.SetString(PREFS_KEY, DateTime.UtcNow.Ticks.ToString());
        PlayerPrefs.Save();

        Debug.Log("[Shop] Поїзд по лагоджено, кулдаун запущено!");
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