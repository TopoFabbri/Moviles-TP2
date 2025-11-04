using Game.World.GameManager;
using UnityEngine;

namespace Game.World.Character.DroneModules
{
    internal sealed class RotationModule
    {
        private readonly Transform dronePivot;
        private readonly Transform camPivot;
        private readonly float camCatchupSpeed;
        private Vector3 rotation = Vector3.zero;

        private float targetPitch;
        private float targetYaw;
            
        private const float MinAngle = -89f;
        private const float MaxAngle = 89f;

        public RotationModule(Transform dronePivot, Transform camPivot, float camCatchupSpeed)
        {
            this.dronePivot = dronePivot;
            this.camPivot = camPivot;
            this.camCatchupSpeed = camCatchupSpeed;
        }

        public void Rotate(Vector3 lookInput)
        {
            CalculateRotation(lookInput);

            dronePivot.localRotation = Quaternion.Euler(rotation);
            float t = 1f - Mathf.Exp(-camCatchupSpeed * Time.deltaTime);
            camPivot.rotation = Quaternion.Slerp(camPivot.rotation, dronePivot.rotation, t);
        }

        private void CalculateRotation(Vector3 lookInput)
        {
            targetPitch = Mathf.LerpAngle(MinAngle, MaxAngle, (lookInput.y + 1f) / 2f);
            targetYaw = Mathf.LerpAngle(MinAngle, MaxAngle, (lookInput.z + 1f) / 2f);

            if (Mathf.Abs(Mathf.DeltaAngle(rotation.x, targetPitch)) > GameSave.RotationSensitivity * Time.deltaTime)
                rotation.x += Mathf.Sign(Mathf.DeltaAngle(rotation.x, targetPitch)) * GameSave.RotationSensitivity * Time.deltaTime;
            else 
                rotation.x = targetPitch;
            
            if (Mathf.Abs(Mathf.DeltaAngle(rotation.z, targetYaw)) > GameSave.RotationSensitivity * Time.deltaTime)
                rotation.z += Mathf.Sign(Mathf.DeltaAngle(rotation.z, targetYaw)) * GameSave.RotationSensitivity * Time.deltaTime;
            else 
                rotation.z = targetYaw;
            
            rotation.y += lookInput.x * GameSave.RotationSensitivity * Time.deltaTime;
        }
    }
}