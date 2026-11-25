using Game.Events;
using Game.World.GameManager;
using Game.World.Targets;
using GooglePlayGames;
using Tools.EventSystem;
using Tools.ServiceProvider;
using UnityEngine;

namespace Game.World.Character
{
    public class DroneCollisionController : MonoBehaviour
    {
        
        private void OnCollisionEnter(Collision collision)
        {
            if (GameSave.GodMode) return;
            string obstacleName = collision != null && collision.collider != null
                ? collision.collider.name
                : "unknown";
            Vector3 contactPoint = collision != null && collision.contactCount > 0
                ? collision.GetContact(0).point
                : transform.position;
            ServiceProvider.GetService<EventSystem>().Raise<DroneCollisionEvent>(obstacleName, contactPoint);

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