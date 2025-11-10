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

        private void Awake()
        {
            EventSystem.Subscribe<ChangedStateEvent<States>>(OnChangedState);
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<ChangedStateEvent<States>>(OnChangedState);
        }

        private void OnChangedState(ChangedStateEvent<States> obj)
        {
            if (obj.state != States.Win) return;

            if (GameTime.Timer.TimeElapsed < aceTime)
            {
#if UNITY_ANDROID
                PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_great_time);
                PlayGamesPlatform.Instance.IncrementAchievement(GPGSIds.achievement_ace, 1, _ => { });
#endif
                GameSave.Credits += 10;
            }
            else if (GameTime.Timer.TimeElapsed < niceTime)
            {
                GameSave.Credits += 5;
            }
            else
            {
                GameSave.Credits += 2;
            }

            if (GameSave.CurrentLevel == 0)
            {
#if UNITY_ANDROID
                PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_tutorial);
#endif
            }
            else
            {
#if UNITY_ANDROID
                PlayGamesPlatform.Instance.IncrementAchievement(GPGSIds.achievement_levels_completed, 1, _ => { });
#endif
            }
        }
    }
}