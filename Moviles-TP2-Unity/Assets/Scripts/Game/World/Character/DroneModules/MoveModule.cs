using UnityEngine;

namespace Game.World.Character.DroneModules
{
    internal sealed class MoveModule
    {
        private readonly Transform dronePivot;
        private readonly Transform camPivot;
        private readonly float camCatchupSpeed;
        internal VelocityModule VelocityModule { get; }

        internal MoveModule(Transform dronePivot, Transform camPivot, float acceleration, float deceleration, float maxSpeed, float camCatchupSpeed)
        {
            this.dronePivot = dronePivot;
            this.camPivot = camPivot;
            VelocityModule = new VelocityModule(acceleration, deceleration, maxSpeed);
            this.camCatchupSpeed = camCatchupSpeed;
        }

        internal void Move(Vector3 moveInput)
        {
            Vector3 velocity = CalculateVelocity(moveInput);

            dronePivot.position += velocity * Time.deltaTime;
            
            float t = 1f - Mathf.Exp(-camCatchupSpeed * Time.deltaTime);
            camPivot.position = Vector3.Lerp(camPivot.position, dronePivot.position, t);
        }

        private Vector3 CalculateVelocity(Vector3 moveInput)
        {
            VelocityModule.Axis decelerateAxes = VelocityModule.Axis.None;
            
            if (Mathf.Abs(moveInput.x) <= float.Epsilon)
                decelerateAxes |= VelocityModule.Axis.X;
            if (Mathf.Abs(moveInput.y) <= float.Epsilon)
                decelerateAxes |= VelocityModule.Axis.Y;
            if (Mathf.Abs(moveInput.z) <= float.Epsilon)
                decelerateAxes |= VelocityModule.Axis.Z;
            
            VelocityModule.Accelerate(moveInput, Time.deltaTime);
            VelocityModule.DecelerateAxes(Time.deltaTime, decelerateAxes);

            Vector3 velocity = dronePivot.forward * VelocityModule.GetVelocity().z + dronePivot.right * VelocityModule.GetVelocity().x +
                               dronePivot.up * VelocityModule.GetVelocity().y;
            return velocity;
        }
    }
}