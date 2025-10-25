using System;
using Model.Tools.EventSystem;
using UnityEngine;

namespace Game.World.Character
{
    internal sealed class VelocityModule
    {
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
            velocity += acceleration * throttle * dt;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        }

        internal void Decelerate(float dt)
        {
            float newSpeed = MathF.Max(velocity.magnitude - deceleration * dt, 0f);
            
            velocity = velocity.normalized * newSpeed;
        }

        internal Vector3 GetVelocity()
        {
            return velocity;
        }
    }
}