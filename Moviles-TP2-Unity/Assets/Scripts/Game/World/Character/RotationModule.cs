using UnityEngine;

namespace Game.World.Character
{
    internal sealed class RotationModule
    {
        private readonly Transform dronePivot;
        private readonly Transform camPivot;
        private readonly float rotSpeed;
        private readonly float camCatchupSpeed;
        private Vector3 rotation = Vector3.zero;

        private float targetPitch;
        private float targetYaw;
            
        private const float MinAngle = -80f;
        private const float MaxAngle = 80f;

        public RotationModule(Transform dronePivot, Transform camPivot, float rotSpeed, float camCatchupSpeed)
        {
            this.dronePivot = dronePivot;
            this.camPivot = camPivot;
            this.rotSpeed = rotSpeed;
            this.camCatchupSpeed = camCatchupSpeed;
        }

        public void Rotate(Vector3 lookInput)
        {
            CalculateRotation(lookInput);

            dronePivot.rotation = Quaternion.Euler(rotation);
            float t = 1f - Mathf.Exp(-camCatchupSpeed * Time.deltaTime);
            camPivot.rotation = Quaternion.Slerp(camPivot.rotation, dronePivot.rotation, t);
        }

        private void CalculateRotation(Vector3 lookInput)
        {
            targetPitch = Mathf.LerpAngle(MinAngle, MaxAngle, (lookInput.y + 1f) / 2f);
            targetYaw = Mathf.LerpAngle(MinAngle, MaxAngle, (lookInput.z + 1f) / 2f);

            if (Mathf.Abs(Mathf.DeltaAngle(rotation.x, targetPitch)) > rotSpeed * Time.deltaTime)
                rotation.x += Mathf.Sign(Mathf.DeltaAngle(rotation.x, targetPitch)) * rotSpeed * Time.deltaTime;
            else 
                rotation.x = targetPitch;
            
            if (Mathf.Abs(Mathf.DeltaAngle(rotation.z, targetYaw)) > rotSpeed * Time.deltaTime)
                rotation.z += Mathf.Sign(Mathf.DeltaAngle(rotation.z, targetYaw)) * rotSpeed * Time.deltaTime;
            else 
                rotation.z = targetYaw;
            
            rotation.y += lookInput.x * rotSpeed * Time.deltaTime;
        }
    }
}