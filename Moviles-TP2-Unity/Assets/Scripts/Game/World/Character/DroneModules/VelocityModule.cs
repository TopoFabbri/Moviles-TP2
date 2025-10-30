using System;
using Game.Events;
using Tools.EventSystem;
using UnityEngine;

namespace Game.World.Character.DroneModules
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
            EventSystem.Raise<VelocityDebugEvent>(velocity.magnitude);
        }

        internal void Accelerate(Vector3 throttle, float dt)
        {
            velocity += throttle * (acceleration * dt);
    
            if (Mathf.Abs(throttle.x) > float.Epsilon)
                velocity.x = Mathf.Clamp(velocity.x, -maxSpeed * Mathf.Abs(throttle.x), maxSpeed * Mathf.Abs(throttle.x));
            
            if (Mathf.Abs(throttle.y) > float.Epsilon)
                velocity.y = Mathf.Clamp(velocity.y, -maxSpeed * Mathf.Abs(throttle.y), maxSpeed * Mathf.Abs(throttle.y));
            
            if (Mathf.Abs(throttle.z) > float.Epsilon)
                velocity.z = Mathf.Clamp(velocity.z, -maxSpeed * Mathf.Abs(throttle.z), maxSpeed * Mathf.Abs(throttle.z));;
        }

        internal void DecelerateAxes(float dt, Axis axis)
        {
            if (axis.HasFlag(Axis.X))
                velocity.x = Mathf.MoveTowards(velocity.x, 0f, deceleration * dt);
            if (axis.HasFlag(Axis.Y))
                velocity.y = Mathf.MoveTowards(velocity.y, 0f, deceleration * dt);
            if (axis.HasFlag(Axis.Z))
                velocity.z = Mathf.MoveTowards(velocity.z, 0f, deceleration * dt);
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