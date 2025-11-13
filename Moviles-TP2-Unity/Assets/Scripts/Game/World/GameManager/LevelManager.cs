using GooglePlayGames;
using Tools.EventSystem;
using Tools.FSM;
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
            EventSystem.Subscribe<ChangedStateEvent<States>>(OnChangedState);
            AceTime = aceTime;
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<ChangedStateEvent<States>>(OnChangedState);
        }

        private void OnChangedState(ChangedStateEvent<States> obj)
        {
            if (obj.state != States.Win) return;

            GrantTimeRewards();
            GrantLevelCompletedRewards();
        }

        private void GrantTimeRewards()
        {
            float time = GameTime.Timer.TimeElapsed;
            float bestTime = GameSave.GetLevelTime(GameSave.CurrentLevel);

            if (time < aceTime)
            {
#if UNITY_ANDROID
                PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_great_time);
                
                if (time < bestTime && bestTime > aceTime && bestTime > 0f)
                    PlayGamesPlatform.Instance.IncrementAchievement(GPGSIds.achievement_ace, 1, _ => { });
#endif
                GameSave.Credits += 10;
            }
            else if (time < niceTime)
            {
                GameSave.Credits += 5;
            }
            else
            {
                GameSave.Credits += 2;
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