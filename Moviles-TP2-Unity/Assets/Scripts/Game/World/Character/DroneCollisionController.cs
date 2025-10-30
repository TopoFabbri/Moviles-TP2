using Game.Events;
using Game.World.Targets;
using Tools.EventSystem;
using UnityEngine;

namespace Game.World.Character
{
    public class DroneCollisionController : MonoBehaviour
    {
        private void OnCollisionEnter()
        {
            EventSystem.Raise<DroneCollisionEvent>();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out ObstacleScore target)) return;
            
            target.MarkDone();
        }
    }
}