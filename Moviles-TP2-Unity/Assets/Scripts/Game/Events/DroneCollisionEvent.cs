using Tools.EventSystem;
using UnityEngine;

namespace Game.Events
{
    internal sealed class DroneCollisionEvent : IEvent
    {
        public string ObstacleName { get; private set; }
        public Vector3 ContactPoint { get; private set; }

        public void Restart()
        {
            ObstacleName = null;
            ContactPoint = Vector3.zero;
        }

        public void Assign(params object[] parameters)
        {
            if (parameters == null) return;
            if (parameters.Length > 0 && parameters[0] is string obstacleName)
                ObstacleName = obstacleName;
            if (parameters.Length > 1 && parameters[1] is Vector3 contactPoint)
                ContactPoint = contactPoint;
        }
    }
}