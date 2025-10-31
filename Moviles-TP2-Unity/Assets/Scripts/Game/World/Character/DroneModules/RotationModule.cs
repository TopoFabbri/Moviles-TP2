using Unity.Mathematics;
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
            CalculateRotation(lookInput);
            LerpCamera();
        }

        private void CalculateRotation(Vector3 lookInput)
        {
            dronePivot.rotation = Quaternion.AngleAxis(lookInput.x * rotSpeed * Time.deltaTime, dronePivot.up) * dronePivot.rotation;
            dronePivot.rotation = Quaternion.AngleAxis(lookInput.y * rotSpeed * Time.deltaTime, dronePivot.right) * dronePivot.rotation;

            // after yaw/pitch are applied, keep the current forward
            Vector3 fwd = dronePivot.forward;
            float targetRoll = Mathf.Lerp(MinAngle, MaxAngle, (lookInput.z + 1f) * 0.5f);
            // Build an up vector by rotating a reference around current forward to the desired roll
            Vector3 refUp = Vector3.up; // or cache an application-specific "neutral" up
            Quaternion qRoll = Quaternion.AngleAxis(targetRoll, fwd);
            Vector3 up = qRoll * refUp;

            if (!Mathf.Approximately(Vector3.Dot(fwd, Vector3.up), 1f))
                dronePivot.rotation = Quaternion.LookRotation(fwd, up);

            if (Mathf.Approximately(lookInput.y, float.Epsilon))
            {
                Quaternion stableRot = Quaternion.Euler(new Vector3(0f, dronePivot.rotation.eulerAngles.y, 0f));
                dronePivot.rotation = Quaternion.RotateTowards(dronePivot.rotation, stableRot, rotSpeed * Time.deltaTime);
            }
        }

        private void LerpCamera()
        {
            float t = 1f - Mathf.Exp(-camCatchupSpeed * Time.deltaTime);
            camPivot.rotation = Quaternion.Slerp(camPivot.rotation, dronePivot.rotation, t);
        }
    }
}