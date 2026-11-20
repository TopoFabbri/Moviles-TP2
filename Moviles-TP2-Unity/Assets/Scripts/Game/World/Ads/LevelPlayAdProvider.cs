using System;
using System.Threading;
using System.Threading.Tasks;
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
        [SerializeField] private string androidAppKey = "299f0d335";
        [SerializeField] private string interstitialAdUnitId = "od2vuy9qld2ve46v";
        [SerializeField] private string rewardedAdUnitId = "y6pgbn4vjxrewodl";
#pragma warning restore CS0414

#if ENABLE_LEVELPLAY
        private const int MaxRetryExponent = 6; // Cap backoff at 64s

        private LevelPlayInterstitialAd _interstitialAd;
        private LevelPlayRewardedAd _rewardedAd;
        private Action _onInterstitialClosedCallback;
        private Action<bool> _onRewardedResultCallback;
        private SynchronizationContext _unityContext;
        private bool _isInitialized;
        private bool _isInterstitialLoading;
        private bool _isRewardedLoading;
        private bool _rewardedEarned;
        private int _interstitialRetryAttempt;
        private int _rewardedRetryAttempt;

        public bool IsInitialized => _isInitialized;

        public void Initialize()
        {
            _unityContext = SynchronizationContext.Current;
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
            _interstitialAd.OnAdLoaded += OnInterstitialLoaded;
            _interstitialAd.OnAdLoadFailed += OnInterstitialLoadFailed;
            _interstitialAd.OnAdClosed += OnInterstitialClosed;
            _interstitialAd.OnAdDisplayFailed += (info, err) =>
            {
                Debug.LogWarning($"[LevelPlayAdProvider] Interstitial display failed: {err?.ErrorMessage} (Code {err?.ErrorCode})");
                if (Tools.ServiceProvider.ServiceProvider.TryGetService<Game.World.FabbriPlugin.IDiagnosticLogService>(out var diag))
                    diag.RecordFailure("ADS", "InterstitialDisplay", $"{err?.ErrorMessage} (Code {err?.ErrorCode})");
                OnInterstitialClosed(info);
            };
            LoadInterstitial();

            // Create and load Rewarded Ad
            _rewardedAd = new LevelPlayRewardedAd(rewardedAdUnitId);
            _rewardedAd.OnAdLoaded += OnRewardedLoaded;
            _rewardedAd.OnAdLoadFailed += OnRewardedLoadFailed;
            _rewardedAd.OnAdRewarded += OnRewardedEarned;
            _rewardedAd.OnAdClosed += OnRewardedClosed;
            _rewardedAd.OnAdDisplayFailed += (info, err) =>
            {
                Debug.LogWarning($"[LevelPlayAdProvider] Rewarded display failed: {err?.ErrorMessage} (Code {err?.ErrorCode})");
                if (Tools.ServiceProvider.ServiceProvider.TryGetService<Game.World.FabbriPlugin.IDiagnosticLogService>(out var diag))
                    diag.RecordFailure("ADS", "RewardedDisplay", $"{err?.ErrorMessage} (Code {err?.ErrorCode})");
                _onRewardedResultCallback?.Invoke(false);
                _onRewardedResultCallback = null;
                LoadRewarded();
            };
            LoadRewarded();
        }

        private void OnInitFailed(LevelPlayInitError error)
        {
            _isInitialized = false;
            Debug.LogError($"[LevelPlayAdProvider] Init failed: {error.ErrorMessage} (Code {error.ErrorCode})");
            if (Tools.ServiceProvider.ServiceProvider.TryGetService<Game.World.FabbriPlugin.IDiagnosticLogService>(out var diag))
                diag.RecordFailure("ADS", "Init", $"{error.ErrorMessage} (Code {error.ErrorCode})");
        }

        public bool IsInterstitialReady() => _interstitialAd != null && _interstitialAd.IsAdReady();

        public void EnsureInterstitialLoaded()
        {
            if (_isInitialized && !IsInterstitialReady() && !_isInterstitialLoading)
            {
                LoadInterstitial();
            }
        }

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
                EnsureInterstitialLoaded();
            }
        }

        public bool IsRewardedReady() => _rewardedAd != null && _rewardedAd.IsAdReady();

        public void EnsureRewardedLoaded()
        {
            if (_isInitialized && !IsRewardedReady() && !_isRewardedLoading)
            {
                LoadRewarded();
            }
        }

        public void ShowRewarded(Action<bool> onResult)
        {
            _rewardedEarned = false;
            _onRewardedResultCallback = onResult;
            if (IsRewardedReady())
            {
                _rewardedAd.ShowAd();
            }
            else
            {
                _onRewardedResultCallback?.Invoke(false);
                _onRewardedResultCallback = null;
                EnsureRewardedLoaded();
            }
        }

        private void LoadInterstitial()
        {
            if (_interstitialAd == null || _isInterstitialLoading) return;
            _isInterstitialLoading = true;
            _interstitialAd.LoadAd();
        }

        private void LoadRewarded()
        {
            if (_rewardedAd == null || _isRewardedLoading) return;
            _isRewardedLoading = true;
            _rewardedAd.LoadAd();
        }

        private void OnInterstitialLoaded(LevelPlayAdInfo info)
        {
            _isInterstitialLoading = false;
            _interstitialRetryAttempt = 0;
            Debug.Log("[LevelPlayAdProvider] Interstitial ad loaded.");
        }

        private void OnInterstitialLoadFailed(LevelPlayAdError error)
        {
            _isInterstitialLoading = false;
            _interstitialRetryAttempt = Math.Min(_interstitialRetryAttempt + 1, MaxRetryExponent);
            int delaySeconds = (int)Math.Pow(2, _interstitialRetryAttempt);
            Debug.LogWarning($"[LevelPlayAdProvider] Interstitial load failed: {error?.ErrorMessage} (Code {error?.ErrorCode}). Retrying in {delaySeconds}s.");
            if (Tools.ServiceProvider.ServiceProvider.TryGetService<Game.World.FabbriPlugin.IDiagnosticLogService>(out var diag))
                diag.RecordFailure("ADS", "InterstitialLoad", $"{error?.ErrorMessage} (Code {error?.ErrorCode})");
            ScheduleOnMainThread(delaySeconds * 1000, LoadInterstitial);
        }

        private void OnRewardedLoaded(LevelPlayAdInfo info)
        {
            _isRewardedLoading = false;
            _rewardedRetryAttempt = 0;
            Debug.Log("[LevelPlayAdProvider] Rewarded ad loaded.");
        }

        private void OnRewardedLoadFailed(LevelPlayAdError error)
        {
            _isRewardedLoading = false;
            _rewardedRetryAttempt = Math.Min(_rewardedRetryAttempt + 1, MaxRetryExponent);
            int delaySeconds = (int)Math.Pow(2, _rewardedRetryAttempt);
            Debug.LogWarning($"[LevelPlayAdProvider] Rewarded load failed: {error?.ErrorMessage} (Code {error?.ErrorCode}). Retrying in {delaySeconds}s.");
            if (Tools.ServiceProvider.ServiceProvider.TryGetService<Game.World.FabbriPlugin.IDiagnosticLogService>(out var diag))
                diag.RecordFailure("ADS", "RewardedLoad", $"{error?.ErrorMessage} (Code {error?.ErrorCode})");
            ScheduleOnMainThread(delaySeconds * 1000, LoadRewarded);
        }

        private void OnInterstitialClosed(LevelPlayAdInfo info)
        {
            _onInterstitialClosedCallback?.Invoke();
            _onInterstitialClosedCallback = null;
            LoadInterstitial();
        }

        private void OnRewardedEarned(LevelPlayAdInfo info, LevelPlayReward reward)
        {
            _rewardedEarned = true;
            _onRewardedResultCallback?.Invoke(true);
            _onRewardedResultCallback = null;
        }

        private void OnRewardedClosed(LevelPlayAdInfo info)
        {
            // Defer cancellation slightly so adapters that fire OnAdClosed before OnAdRewarded still grant the reward
            if (_onRewardedResultCallback != null && !_rewardedEarned)
            {
                ScheduleOnMainThread(250, () =>
                {
                    if (!_rewardedEarned && _onRewardedResultCallback != null)
                    {
                        _onRewardedResultCallback.Invoke(false);
                        _onRewardedResultCallback = null;
                    }
                });
            }
            LoadRewarded();
        }

        private void ScheduleOnMainThread(int delayMs, Action action)
        {
            Task.Delay(delayMs).ContinueWith(_ =>
            {
                if (_unityContext != null)
                {
                    _unityContext.Post(_2 => action(), null);
                }
                else
                {
                    action();
                }
            });
        }
#endif
    }
}
