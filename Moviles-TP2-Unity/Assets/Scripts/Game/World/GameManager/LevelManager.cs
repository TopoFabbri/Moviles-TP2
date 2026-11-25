using GooglePlayGames;
using Tools.EventSystem;
using Tools.FSM;
using Tools.ServiceProvider;
using UnityEngine;

namespace Game.World.GameManager
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private float aceTime = 40f;
        [SerializeField] private float niceTime = 60f;

        public static float AceTime { get; private set; }
        
        private void Awake()
        {
            ServiceProvider.GetService<EventSystem>().Subscribe<ChangedStateEvent<States>>(OnChangedState);
            AceTime = aceTime;
        }

        private void OnDestroy()
        {
            ServiceProvider.GetService<EventSystem>().Unsubscribe<ChangedStateEvent<States>>(OnChangedState);
        }

        private void OnChangedState(ChangedStateEvent<States> obj)
        {
            if (obj.state != States.Win) return;

            GrantTimeRewards();
            GrantLevelCompletedRewards();
        }

        private void GrantTimeRewards()
        {
            float time = ServiceProvider.GetService<GameTime>().Timer.TimeElapsed;
            float bestTime = GameSave.GetLevelTime(GameSave.CurrentLevel);
            bool isNewBest = bestTime < 0f || time < bestTime;
            bool aceAchieved = time < aceTime;
            int creditsEarned;

            if (aceAchieved)
            {
#if UNITY_ANDROID
                PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_great_time);
                
                if (time < aceTime && bestTime > aceTime && bestTime > 0f)
                    PlayGamesPlatform.Instance.IncrementAchievement(GPGSIds.achievement_ace, 1, _ => { });
#endif
                creditsEarned = 10;
                GameSave.Credits += creditsEarned;
            }
            else if (time < niceTime)
            {
                creditsEarned = 5;
                GameSave.Credits += creditsEarned;
            }
            else
            {
                creditsEarned = 2;
                GameSave.Credits += creditsEarned;
            }

            if (ServiceProvider.TryGetService<Analytics.IAnalyticsService>(out var analytics))
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                analytics.TrackLevelCompleted(GameSave.CurrentLevel, sceneName, time, isNewBest, aceAchieved, creditsEarned, GameSave.GodMode);
                analytics.TrackCurrencyEarned("level_win", creditsEarned, GameSave.CurrentLevel, GameSave.Credits);
            }
        }

        private static void GrantLevelCompletedRewards()
        {
            if (GameSave.CurrentLevel == 0)
            {
#if UNITY_ANDROID
                PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_tutorial);
#endif
            }
            else
            {
#if UNITY_ANDROID
                if (GameSave.GetLevelTime(GameSave.CurrentLevel) < 0)
                    PlayGamesPlatform.Instance.IncrementAchievement(GPGSIds.achievement_levels_completed, 1, _ => { });
#endif
            }
        }
    }
}