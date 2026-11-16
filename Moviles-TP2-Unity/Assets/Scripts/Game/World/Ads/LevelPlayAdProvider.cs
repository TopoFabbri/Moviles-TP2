using System;
using UnityEngine;

#if ENABLE_LEVELPLAY
using Unity.Services.LevelPlay;
#endif

namespace Game.World.Ads
{
    /// <summary>
    /// LevelPlay (ironSource / Unity Ads Mediation) adapter.
    /// Active when ENABLE_LEVELPLAY symbol is defined and the com.unity.services.levelplay package is installed.
    /// </summary>
    public sealed class LevelPlayAdProvider
    {
#pragma warning disable CS0414
        [SerializeField] private string androidAppKey = "YOUR_ANDROID_APP_KEY";
        [SerializeField] private string interstitialAdUnitId = "YOUR_INTERSTITIAL_AD_UNIT_ID";
        [SerializeField] private string rewardedAdUnitId = "YOUR_REWARDED_AD_UNIT_ID";
#pragma warning restore CS0414

#if ENABLE_LEVELPLAY
        private LevelPlayInterstitialAd _interstitialAd;
        private LevelPlayRewardedAd _rewardedAd;
        private Action _onInterstitialClosedCallback;
        private Action<bool> _onRewardedResultCallback;
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;

        public void Initialize()
        {
            LevelPlay.OnInitSuccess += OnInitSuccess;
            LevelPlay.OnInitFailed += OnInitFailed;
            LevelPlay.Init(androidAppKey);
        }

        private void OnInitSuccess(LevelPlayConfiguration configuration)
        {
            _isInitialized = true;
            Debug.Log("[LevelPlayAdProvider] LevelPlay SDK initialized successfully.");

            // Create and load Interstitial Ad
            _interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);
            _interstitialAd.OnAdClosed += OnInterstitialClosed;
            _interstitialAd.OnAdDisplayFailed += (info, err) => OnInterstitialClosed(info);
            _interstitialAd.LoadAd();

            // Create and load Rewarded Ad
            _rewardedAd = new LevelPlayRewardedAd(rewardedAdUnitId);
            _rewardedAd.OnAdRewarded += OnRewardedEarned;
            _rewardedAd.OnAdClosed += OnRewardedClosed;
            _rewardedAd.OnAdDisplayFailed += (info, err) => { _onRewardedResultCallback?.Invoke(false); _onRewardedResultCallback = null; };
            _rewardedAd.LoadAd();
        }

        private void OnInitFailed(LevelPlayInitError error)
        {
            _isInitialized = false;
            Debug.LogError($"[LevelPlayAdProvider] Init failed: {error.ErrorMessage} (Code {error.ErrorCode})");
        }

        public bool IsInterstitialReady() => _interstitialAd != null && _interstitialAd.IsAdReady();

        public void ShowInterstitial(Action onClosed)
        {
            _onInterstitialClosedCallback = onClosed;
            if (IsInterstitialReady())
            {
                _interstitialAd.ShowAd();
            }
            else
            {
                _onInterstitialClosedCallback?.Invoke();
                _onInterstitialClosedCallback = null;
                _interstitialAd?.LoadAd();
            }
        }

        public bool IsRewardedReady() => _rewardedAd != null && _rewardedAd.IsAdReady();

        public void ShowRewarded(Action<bool> onResult)
        {
            _onRewardedResultCallback = onResult;
            if (IsRewardedReady())
            {
                _rewardedAd.ShowAd();
            }
            else
            {
                _onRewardedResultCallback?.Invoke(false);
                _onRewardedResultCallback = null;
                _rewardedAd?.LoadAd();
            }
        }

        private void OnInterstitialClosed(LevelPlayAdInfo info)
        {
            _onInterstitialClosedCallback?.Invoke();
            _onInterstitialClosedCallback = null;
            _interstitialAd?.LoadAd();
        }

        private void OnRewardedEarned(LevelPlayAdInfo info, LevelPlayReward reward)
        {
            _onRewardedResultCallback?.Invoke(true);
            _onRewardedResultCallback = null;
        }

        private void OnRewardedClosed(LevelPlayAdInfo info)
        {
            // If callback wasn't completed via OnRewardedEarned, treat as cancelled
            _onRewardedResultCallback?.Invoke(false);
            _onRewardedResultCallback = null;
            _rewardedAd?.LoadAd();
        }
#endif
    }
}
