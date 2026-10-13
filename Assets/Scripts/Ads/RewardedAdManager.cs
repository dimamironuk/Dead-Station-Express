using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class RewardedAdManager : MonoBehaviour
{
#if UNITY_ANDROID
    private const string AD_UNIT_ID = "ca-app-pub-4322918954975977/9813663415";
#else
    private const string AD_UNIT_ID = "unused";
#endif

    private RewardedAd _rewardedAd;

    private void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("[AdMob] SDK Ініціалізовано.");
            LoadRewardedAd();
        });
    }

    public void LoadRewardedAd()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        var adRequest = new AdRequest();

        RewardedAd.Load(AD_UNIT_ID, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError($"[AdMob] Помилка завантаження реклами: {error?.GetMessage()}");
                return;
            }

            _rewardedAd = ad;
            RegisterEventHandlers(_rewardedAd);
        });
    }

    public bool IsAdReady()
    {
        return _rewardedAd != null && _rewardedAd.CanShowAd();
    }

    public void ShowRewardedAd(Action onRewardEarned)
    {
        if (IsAdReady())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                onRewardEarned?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("[AdMob] Реклама ще не готова. Завантажуємо знову...");
            LoadRewardedAd();
        }
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            LoadRewardedAd();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            LoadRewardedAd();
        };
    }
}