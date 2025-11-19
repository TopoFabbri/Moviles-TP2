using Game.Events;
using Game.World.GameManager;
using Tools.EventSystem;
using Tools.ServiceProvider;
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
            ServiceProvider.GetService<EventSystem>().Raise<VelocityDebugEvent>(velocity.magnitude, maxSpeed + GameSave.AddedSpeed);;
        }

        internal void Accelerate(Vector3 throttle, float dt)
        {
            Vector3 targetVel = throttle * (maxSpeed + GameSave.AddedSpeed);
            targetVel = Vector3.ClampMagnitude(targetVel, maxSpeed + GameSave.AddedSpeed);
            
            velocity.x = Mathf.MoveTowards(velocity.x, targetVel.x, acceleration * dt);
            velocity.y = Mathf.MoveTowards(velocity.y, targetVel.y, acceleration * dt);
            velocity.z = Mathf.MoveTowards(velocity.z, targetVel.z, acceleration * dt);
        }

        internal void Restart()
        {
            velocity = Vector3.zero;
        }
    }
}