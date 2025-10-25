using UnityEngine;

namespace Game.World.Character
{
    internal sealed class MoveModule
    {
        private readonly Transform dronePivot;
        private readonly Transform camPivot;
        private readonly VelocityModule velocityModule;
        private readonly float camCatchupSpeed;

        internal MoveModule(Transform dronePivot, Transform camPivot, float acceleration, float deceleration, float maxSpeed, float camCatchupSpeed)
        {
            this.dronePivot = dronePivot;
            this.camPivot = camPivot;
            velocityModule = new VelocityModule(acceleration, deceleration, maxSpeed);
            this.camCatchupSpeed = camCatchupSpeed;
        }

        internal void Move(Vector3 moveInput)
        {
            Vector3 velocity = CalculateVelocity(moveInput);

            dronePivot.position += velocity;
            camPivot.position = Vector3.Lerp(camPivot.position, dronePivot.position, camCatchupSpeed * Time.deltaTime);
        }

        private Vector3 CalculateVelocity(Vector3 moveInput)
        {
            if (moveInput.magnitude > float.Epsilon)
                velocityModule.Accelerate(moveInput, Time.deltaTime);
            else
                velocityModule.Decelerate(Time.deltaTime);

            Vector3 velocity = dronePivot.forward * velocityModule.GetVelocity().z + dronePivot.right * velocityModule.GetVelocity().x +
                               dronePivot.up * velocityModule.GetVelocity().y;
            return velocity;
        }
    }
}