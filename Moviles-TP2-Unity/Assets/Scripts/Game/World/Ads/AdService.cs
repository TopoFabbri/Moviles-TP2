using System;
using Game.World.FabbriPlugin;
using Game.World.GameManager;
using Tools.ServiceProvider;
using UnityEngine;

namespace Game.World.Ads
{
    /// <summary>
    /// Core ad management service registered with ServiceProvider.
    /// Manages presentation of Interstitial and Rewarded ads, enforces cooldown timers,
    /// and logs ad events.
    /// </summary>
    public class AdService : IAdService, IService
    {
        private const string PrefsLastRewardedUtcKey = "Ad_LastRewardedUtcTicks";

#if ENABLE_LEVELPLAY
        private readonly LevelPlayAdProvider _levelPlayProvider = new();
#endif

        /// <summary>
        /// Cooldown time in seconds between rewarded ad views.
        /// </summary>
        public float RewardedCooldownDuration { get; set; } = 45f;

        /// <summary>
        /// True if an ad is currently playing on screen.
        /// </summary>
        public bool IsAdShowing { get; private set; }

        public float RewardedCooldownRemaining
        {
            get
            {
                CanShowRewarded(out float remaining);
                return remaining;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void AutoRegister()
        {
            if (!ServiceProvider.TryGetService(out AdService service))
            {
                service = new AdService();
            }
            ServiceProvider.TryAddService<IAdService>(service);
        }

        public AdService()
        {
            ServiceProvider.TryAddService<AdService>(this);
            ServiceProvider.TryAddService<IAdService>(this);

#if ENABLE_LEVELPLAY
            try
            {
                _levelPlayProvider.Initialize();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AdService] LevelPlay initialization exception: {ex.Message}");
            }
#endif
        }

        public bool CanShowRewarded(out float remainingCooldown)
        {
            if (IsAdShowing)
            {
                remainingCooldown = RewardedCooldownDuration;
                return false;
            }

            string storedTicksStr = PlayerPrefs.GetString(PrefsLastRewardedUtcKey, "0");
            long.TryParse(storedTicksStr, out long storedTicks);

            return CalculateRewardedCooldown(DateTime.UtcNow, storedTicks, RewardedCooldownDuration, out remainingCooldown);
        }

        /// <summary>
        /// Pure calculation of remaining cooldown and availability for testability and deterministic checks.
        /// </summary>
        public static bool CalculateRewardedCooldown(DateTime utcNow, long lastWatchedTicks, float cooldownDuration, out float remainingSeconds)
        {
            if (lastWatchedTicks <= 0)
            {
                remainingSeconds = 0f;
                return true;
            }

            DateTime lastWatched = new DateTime(lastWatchedTicks, DateTimeKind.Utc);
            double elapsed = (utcNow - lastWatched).TotalSeconds;

            if (elapsed >= 0 && elapsed < cooldownDuration)
            {
                remainingSeconds = Mathf.Max(0f, (float)(cooldownDuration - elapsed));
                return false;
            }

            remainingSeconds = 0f;
            return true;
        }

        public void ShowInterstitial(string placement, Action onClosed = null)
        {
            if (GameSave.HasNoAds && !GameSave.AdsEnabled)
            {
                LogAdEvent($"Skipping Interstitial Ad (NoAds active, Placement: {placement})");
                onClosed?.Invoke();
                return;
            }

            if (IsAdShowing)
            {
                Debug.LogWarning($"[AdService] Ad already displaying. Ignoring interstitial request for '{placement}'.");
                onClosed?.Invoke();
                return;
            }

            IsAdShowing = true;
            LogAdEvent($"Showing Interstitial Ad (Placement: {placement})");

#if ENABLE_LEVELPLAY
            if (_levelPlayProvider.IsInitialized && _levelPlayProvider.IsInterstitialReady())
            {
                _levelPlayProvider.ShowInterstitial(() =>
                {
                    IsAdShowing = false;
                    LogAdEvent($"LevelPlay Interstitial Ad Closed (Placement: {placement})");
                    onClosed?.Invoke();
                });
                return;
            }
            _levelPlayProvider.EnsureInterstitialLoaded();
#endif

            AdUIOverlay.Instance.ShowInterstitialAd(placement, () =>
            {
                IsAdShowing = false;
                LogAdEvent($"Interstitial Ad Closed (Placement: {placement})");
                onClosed?.Invoke();
            });
        }

        public void ShowRewarded(string placement, Action<bool> onRewardResult)
        {
            if (IsAdShowing)
            {
                Debug.LogWarning($"[AdService] Ad already displaying. Ignoring rewarded request for '{placement}'.");
                onRewardResult?.Invoke(false);
                return;
            }

            if (!CanShowRewarded(out float remaining))
            {
                Debug.LogWarning($"[AdService] Rewarded ad is on cooldown ({remaining:F1}s remaining).");
                onRewardResult?.Invoke(false);
                return;
            }

            IsAdShowing = true;
            LogAdEvent($"Showing Rewarded Ad (Placement: {placement})");

#if ENABLE_LEVELPLAY
            if (_levelPlayProvider.IsInitialized && _levelPlayProvider.IsRewardedReady())
            {
                _levelPlayProvider.ShowRewarded(success =>
                {
                    IsAdShowing = false;
                    if (success)
                    {
                        PlayerPrefs.SetString(PrefsLastRewardedUtcKey, DateTime.UtcNow.Ticks.ToString());
                        PlayerPrefs.Save();
                        LogAdEvent($"LevelPlay Rewarded Ad Completed - Reward Granted! (Placement: {placement})");
                    }
                    else
                    {
                        LogAdEvent($"LevelPlay Rewarded Ad Skipped/Cancelled (Placement: {placement})");
                    }

                    onRewardResult?.Invoke(success);
                });
                return;
            }
            _levelPlayProvider.EnsureRewardedLoaded();
#endif

            AdUIOverlay.Instance.ShowRewardedAd(placement, success =>
            {
                IsAdShowing = false;
                if (success)
                {
                    PlayerPrefs.SetString(PrefsLastRewardedUtcKey, DateTime.UtcNow.Ticks.ToString());
                    PlayerPrefs.Save();
                    LogAdEvent($"Rewarded Ad Completed - Reward Granted! (Placement: {placement})");
                }
                else
                {
                    LogAdEvent($"Rewarded Ad Skipped/Cancelled (Placement: {placement})");
                }

                onRewardResult?.Invoke(success);
            });
        }

        /// <summary>
        /// Resets the rewarded ad cooldown timer (useful for testing and reset saves).
        /// </summary>
        public void ResetCooldown()
        {
            PlayerPrefs.DeleteKey(PrefsLastRewardedUtcKey);
            PlayerPrefs.Save();
        }

        private static void LogAdEvent(string message)
        {
            Debug.Log($"[AdService] {message}");
            try
            {
                if (ServiceProvider.TryGetService(out Game.World.FabbriPlugin.Logger logger))
                {
                    logger?.Log($"[AdService] {message}");
                }
            }
            catch
            {
                // Service resolution may fail during early bootstrap; safe to ignore
            }
        }
    }
}
