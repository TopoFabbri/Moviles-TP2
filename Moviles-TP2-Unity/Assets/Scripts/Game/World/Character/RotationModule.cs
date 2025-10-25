using UnityEngine;

namespace Game.World.Character
{
    internal sealed class RotationModule
    {
        private readonly Transform dronePivot;
        private readonly Transform camPivot;
        private readonly float rotSpeed;
        private readonly float calibrateSpeed;
        private readonly float camCatchupSpeed;
        private Vector3 rotation = Vector3.zero;

        private const float MinAngle = -80f;
        private const float MaxAngle = 80f;

        public RotationModule(Transform dronePivot, Transform camPivot, float rotSpeed, float calibrateSpeed, float camCatchupSpeed)
        {
            this.dronePivot = dronePivot;
            this.camPivot = camPivot;
            this.rotSpeed = rotSpeed;
            this.calibrateSpeed = calibrateSpeed;
            this.camCatchupSpeed = camCatchupSpeed;
        }

        public void Rotate(Vector3 lookInput)
        {
            CalculateRotation(lookInput);

            dronePivot.rotation = Quaternion.Euler(rotation);
            camPivot.rotation = Quaternion.Slerp(camPivot.rotation, dronePivot.rotation, camCatchupSpeed * Time.deltaTime);
        }

        private void CalculateRotation(Vector3 lookInput)
        {
            lookInput = lookInput.normalized * (rotSpeed * Time.deltaTime);

            if (Mathf.Abs(lookInput.y) > float.Epsilon)
            {
                rotation.x += lookInput.y;
                rotation.x = Mathf.Clamp(rotation.x, MinAngle, MaxAngle);
            }
            else
            {
                float angle = Mathf.DeltaAngle(rotation.x, 0f);

                if (Mathf.Abs(angle) > Mathf.Abs(calibrateSpeed * Time.deltaTime))
                    angle = Mathf.Sign(angle) * calibrateSpeed * Time.deltaTime;

                rotation.x += angle;
            }

            if (Mathf.Abs(lookInput.z) > float.Epsilon)
            {
                rotation.z += lookInput.z;
                rotation.z = Mathf.Clamp(rotation.z, MinAngle, MaxAngle);
            }
            else
            {
                float angle = Mathf.DeltaAngle(rotation.z, 0f);

                if (Mathf.Abs(angle) > Mathf.Abs(calibrateSpeed * Time.deltaTime))
                    angle = Mathf.Sign(angle) * calibrateSpeed * Time.deltaTime;

                rotation.z += angle;
            }

            rotation.y += lookInput.x;
        }
    }
}