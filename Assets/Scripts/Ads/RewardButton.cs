using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class RewardButton : MonoBehaviour
{
    [SerializeField] private Button _watchAdButton;
    [SerializeField] private TMP_Text _buttonText;
    [SerializeField] private int _rewardCoins = 100;

    private RewardedAdManager _adManager;

    [Inject]
    public void Construct(RewardedAdManager adManager)
    {
        _adManager = adManager;
    }

    private void Start()
    {
        if (_watchAdButton != null)
        {
            _watchAdButton.onClick.AddListener(OnWatchAdClicked);
        }
    }

    private void Update()
    {
        // Автоматично вмикаємо/вимикаємо кнопку залежно від того, чи завантажилась реклама
        if (_watchAdButton != null && _adManager != null)
        {
            bool isReady = _adManager.IsAdReady();
            _watchAdButton.interactable = isReady;

            if (_buttonText != null)
            {
                _buttonText.text = isReady ? $"Дивитися (+{_rewardCoins} монет)" : "Завантаження...";
            }
        }
    }

    private void OnWatchAdClicked()
    {
        if (_adManager != null)
        {
            // Передаємо метод GiveReward як колбек після перегляду
            _adManager.ShowRewardedAd(GiveReward);
        }
    }

    private void GiveReward()
    {
        // Нараховуємо монети гравцю
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        currentCoins += _rewardCoins;
        PlayerPrefs.SetInt("Coins", currentCoins);
        PlayerPrefs.Save();

        Debug.Log($"Успіх! Монети нараховано. Новий баланс: {currentCoins}");
    }
}