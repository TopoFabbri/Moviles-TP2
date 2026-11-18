using System;
using Tools.ServiceProvider;

namespace Game.World.Ads
{
    /// <summary>
    /// Service interface for displaying interstitial and rewarded advertisements.
    /// </summary>
    public interface IAdService : IService
    {
        /// <summary>
        /// True if an advertisement is currently actively displaying.
        /// </summary>
        bool IsAdShowing { get; }

        /// <summary>
        /// Returns whether a rewarded ad can be shown, outputting remaining cooldown in seconds.
        /// </summary>
        bool CanShowRewarded(out float remainingCooldown);

        /// <summary>
        /// Seconds remaining until rewarded ad cooldown finishes.
        /// </summary>
        float RewardedCooldownRemaining { get; }

        /// <summary>
        /// Displays an interstitial ad (e.g. on level win or drone crash).
        /// </summary>
        /// <param name="placement">Identifier for placement analytics (e.g. "LevelWin", "GameOver").</param>
        /// <param name="onClosed">Optional callback when ad is closed.</param>
        void ShowInterstitial(string placement, Action onClosed = null);

        /// <summary>
        /// Displays a rewarded ad (e.g. in the store).
        /// </summary>
        /// <param name="placement">Identifier for placement analytics (e.g. "Store").</param>
        /// <param name="onRewardResult">Callback with true if user watched full ad and earned reward, false otherwise.</param>
        void ShowRewarded(string placement, Action<bool> onRewardResult);
    }
}
