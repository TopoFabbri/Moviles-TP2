using Game.World.FabbriPlugin;
using UnityEngine;

namespace Game.World.Analytics
{
    public interface IAnalyticsService : IService
    {
        bool IsInitialized { get; }
        bool IsDataCollectionEnabled { get; }

        void SetDataCollectionEnabled(bool enabled);

        void TrackLevelStarted(int levelIndex, string levelName, int totalRings, float addedSpeed, bool godMode, bool isRetry);
        void TrackLevelCompleted(int levelIndex, string levelName, float elapsedSeconds, bool isNewBest, bool aceAchieved, int creditsEarned, bool godMode);
        void TrackLevelFailed(int levelIndex, string levelName, float elapsedSeconds, int ringsPassed, int totalRings, int nextRingIndex, string crashObstacle, Vector3 crashPosition);
        void TrackLevelAbandoned(int levelIndex, string levelName, float elapsedSeconds, int ringsPassed, int totalRings, string fromState);
        void TrackInGamePurchase(string itemId, string currencyType, float amount, string currencyCode, int upgradeLevel, int balanceAfter);
        void TrackCurrencyEarned(string source, int amount, int levelIndex, int balanceAfter);
        void TrackAdInteraction(string adType, string placement, bool completed);
        void TrackRuntimeError(string errorType, string errorMessage, string sceneName);
        void Flush();
    }
}
