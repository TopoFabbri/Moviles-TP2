using UnityEngine;

namespace Game.World.Character.DroneModules
{
    internal sealed class MoveModule
    {
        private readonly Transform dronePivot;
        private readonly Transform camPivot;
        private readonly Rigidbody rb;
        private readonly float camCatchupSpeed;
        internal VelocityModule VelocityModule { get; }

        internal MoveModule(Transform dronePivot, Transform camPivot, float acceleration, float maxSpeed, float camCatchupSpeed)
        {
            this.dronePivot = dronePivot;
            this.camPivot = camPivot;
            rb = dronePivot != null ? dronePivot.GetComponent<Rigidbody>() : null;
            VelocityModule = new VelocityModule(acceleration, maxSpeed);
            this.camCatchupSpeed = camCatchupSpeed;
        }

        internal void Move(Vector3 moveInput)
        {
            Vector3 velocity = CalculateVelocity(moveInput);

            if (rb != null && !rb.isKinematic)
                rb.linearVelocity = velocity;
            else
                dronePivot.position += velocity * Time.deltaTime;
            
            float t = 1f - Mathf.Exp(-camCatchupSpeed * Time.deltaTime);
            camPivot.position = Vector3.Lerp(camPivot.position, dronePivot.position, t);
        }

        internal void Stop()
        {
            if (rb != null && !rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        
        internal void Restart()
        {
            VelocityModule.Restart();
            Stop();
        }

        private Vector3 CalculateVelocity(Vector3 moveInput)
        {
            VelocityModule.Accelerate(moveInput, Time.deltaTime);

            Vector3 velocity = dronePivot.forward * VelocityModule.Velocity.z + dronePivot.right * VelocityModule.Velocity.x +
                               dronePivot.up * VelocityModule.Velocity.y;
            return velocity;
        }
    }
}