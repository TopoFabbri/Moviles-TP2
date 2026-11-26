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
            float effectiveMaxSpeed = maxSpeed + GameSave.AddedSpeed;
            Vector3 targetVel = throttle * effectiveMaxSpeed;
            targetVel = Vector3.ClampMagnitude(targetVel, effectiveMaxSpeed);

            float control = Mathf.Clamp01(GameSave.AddedControl);
            if (control >= 1f)
            {
                velocity = targetVel;
                return;
            }

            float effectiveAcceleration = acceleration / (1f - control);
            float maxDelta = effectiveAcceleration * dt;
            velocity.x = Mathf.MoveTowards(velocity.x, targetVel.x, maxDelta);
            velocity.y = Mathf.MoveTowards(velocity.y, targetVel.y, maxDelta);
            velocity.z = Mathf.MoveTowards(velocity.z, targetVel.z, maxDelta);
        }

        internal void Restart()
        {
            velocity = Vector3.zero;
        }
    }
}