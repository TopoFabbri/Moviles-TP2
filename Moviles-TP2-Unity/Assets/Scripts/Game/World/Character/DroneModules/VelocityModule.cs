using Game.Events;
using Tools.EventSystem;
using UnityEngine;

namespace Game.World.Character.DroneModules
{
    internal sealed class VelocityModule
    {
        private Vector3 velocity;
        private readonly float acceleration;
        private readonly float maxSpeed;

        internal Vector3 Velocity => velocity;

        internal VelocityModule(float acceleration, float maxSpeed)
        {
            velocity = Vector3.zero;
            this.acceleration = acceleration;
            this.maxSpeed = maxSpeed;
        }

        internal void RaiseDebugVelocity()
        {
            EventSystem.Raise<VelocityDebugEvent>(velocity.magnitude, maxSpeed);
        }

        internal void Accelerate(Vector3 throttle, float dt)
        {
            Vector3 targetVel = throttle * maxSpeed;
            targetVel = Vector3.ClampMagnitude(targetVel, maxSpeed);
            
            velocity.x = Mathf.MoveTowards(velocity.x, targetVel.x, acceleration * dt);
            velocity.y = Mathf.MoveTowards(velocity.y, targetVel.y, acceleration * dt);
            velocity.z = Mathf.MoveTowards(velocity.z, targetVel.z, acceleration * dt);
        }

        internal void Restart()
        {
            velocity = Vector3.zero;
        }
        
        public float GetMaxSpeed()
        {
            return maxSpeed;
        }
    }
}