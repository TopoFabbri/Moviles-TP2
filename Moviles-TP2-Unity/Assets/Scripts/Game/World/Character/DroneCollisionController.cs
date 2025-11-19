using Game.Events;
using Game.World.GameManager;
using Game.World.Targets;
using GooglePlayGames.com.google.play.games.Runtime.Scripts.ISocialPlatform;
using Tools.EventSystem;
using Tools.ServiceProvider;
using UnityEngine;

namespace Game.World.Character
{
    public class DroneCollisionController : MonoBehaviour
    {
        
        private void OnCollisionEnter()
        {
            if (GameSave.GodMode) return;
            ServiceProvider.GetService<EventSystem>().Raise<DroneCollisionEvent>();

#if UNITY_ANDROID           
            PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_crash);
#endif
            ServiceProvider.GetService<AndroidVibrator>().Vibrate(100L);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out ObstacleScore target)) return;
            
            target.MarkDone();
            ServiceProvider.GetService<AndroidVibrator>().Vibrate(10L);
        }
    }
}