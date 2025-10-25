using System;
using Model.Tools.EventSystem;
using UnityEngine;

namespace Game.World.Character
{
    internal sealed class VelocityModule
    {
        [Flags]
        internal enum Axis
        {
            None = 0,
            X = 1,
            Y = 2,
            Z = 4,
        }
        
        private Vector3 velocity;
        private readonly float acceleration;
        private readonly float deceleration;
        private readonly float maxSpeed;

        internal VelocityModule(float acceleration, float deceleration, float maxSpeed)
        {
            velocity = Vector3.zero;
            this.acceleration = acceleration;
            this.deceleration = deceleration;
            this.maxSpeed = maxSpeed;
        }

        internal void RaiseDebugVelocity()
        {
            EventSystem.Raise<VelocityDebugEvent>(velocity.magnitude, acceleration, deceleration, maxSpeed);
        }

        internal void Accelerate(Vector3 throttle, float dt)
        {
            velocity += throttle * (acceleration * dt);
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        }

        internal void DecelerateAxes(float dt, Axis axis)
        {
            if (axis.HasFlag(Axis.X))
                velocity.x = MathF.Max(velocity.x - deceleration * Mathf.Sign(velocity.x) * dt, 0f);
            if (axis.HasFlag(Axis.Y))
                velocity.y = MathF.Max(velocity.y - deceleration * Mathf.Sign(velocity.y) * dt, 0f);
            if (axis.HasFlag(Axis.Z))
                velocity.z = MathF.Max(velocity.z - deceleration * Mathf.Sign(velocity.z) * dt, 0f);
        }

        internal Vector3 GetVelocity()
        {
            return velocity;
        }

        public float GetMaxSpeed()
        {
            return maxSpeed;
        }
    }
}