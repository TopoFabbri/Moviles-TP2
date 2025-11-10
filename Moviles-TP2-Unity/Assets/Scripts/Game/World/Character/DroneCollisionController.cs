using Game.Events;
using Game.World.Targets;
using GooglePlayGames;
using Tools.EventSystem;
using UnityEngine;

namespace Game.World.Character
{
    public class DroneCollisionController : MonoBehaviour
    {
        
        private void OnCollisionEnter(Collision collision)
        {
            EventSystem.Raise<DroneCollisionEvent>();

#if UNITY_ANDROID           
            PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_crash);
#endif
            AndroidVibrator.Vibrate(100L);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out ObstacleScore target)) return;
            
            target.MarkDone();
            AndroidVibrator.Vibrate(10L);
        }
    }
}