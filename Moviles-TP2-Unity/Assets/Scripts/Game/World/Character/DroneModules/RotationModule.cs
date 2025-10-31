using UnityEngine;

namespace Game.World.Character.DroneModules
{
    internal sealed class RotationModule
    {
        private readonly Transform dronePivot;
        private readonly Transform camPivot;
        private readonly float rotSpeed;
        private readonly float camCatchupSpeed;
        
        private const float MinAngle = -89f;
        private const float MaxAngle = 89f;

        public RotationModule(Transform dronePivot, Transform camPivot, float rotSpeed, float camCatchupSpeed)
        {
            this.dronePivot = dronePivot;
            this.camPivot = camPivot;
            this.rotSpeed = rotSpeed;
            this.camCatchupSpeed = camCatchupSpeed;
        }

        public void Rotate(Vector3 lookInput)
        {
            dronePivot.rotation = Quaternion.AngleAxis(lookInput.x * rotSpeed * Time.deltaTime, dronePivot.up) * dronePivot.rotation;
            dronePivot.rotation = Quaternion.AngleAxis(lookInput.y * rotSpeed * Time.deltaTime, dronePivot.right) * dronePivot.rotation;
            dronePivot.rotation = Quaternion.AngleAxis(lookInput.z * rotSpeed * Time.deltaTime, dronePivot.forward) * dronePivot.rotation;

            if (Mathf.Abs(lookInput.y) + Mathf.Abs(lookInput.z) <= float.Epsilon)
            {
                Quaternion stableRot = Quaternion.Euler(new Vector3(0f, dronePivot.rotation.eulerAngles.y, 0f));
                dronePivot.rotation = Quaternion.RotateTowards(dronePivot.rotation, stableRot, rotSpeed * Time.deltaTime);
            }
            
            float t = 1f - Mathf.Exp(-camCatchupSpeed * Time.deltaTime);
            camPivot.rotation = Quaternion.Slerp(camPivot.rotation, dronePivot.rotation, t);
        }

        private void CalculateRotation(Vector3 lookInput)
        {
        }
    }
}