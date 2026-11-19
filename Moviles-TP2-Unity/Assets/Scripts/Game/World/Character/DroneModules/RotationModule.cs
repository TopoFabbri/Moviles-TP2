using UnityEngine;

namespace Game.World.Character.DroneModules
{
    internal sealed class RotationModule
    {
        private enum RotationMode
        {
            None,
            Standard,
            BankToTurn,
            FpvAcro
        }

        private readonly Transform dronePivot;
        private readonly Transform camPivot;
        private readonly float rotSpeed;
        private readonly float camCatchupSpeed;
        private Vector3 rotation = Vector3.zero;
        private float bankYaw;
        private Quaternion acroRotation = Quaternion.identity;
        private RotationMode activeMode = RotationMode.None;

        private float targetPitch;
        private float targetYaw;
            
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
            if (activeMode != RotationMode.None && activeMode != RotationMode.Standard)
                rotation = dronePivot.localRotation.eulerAngles;
            activeMode = RotationMode.Standard;

            CalculateRotation(lookInput);

            dronePivot.localRotation = Quaternion.Euler(rotation);
            UpdateCameraRotation();
        }

        public void RotateBankToTurn(Quaternion tiltSwing, float manualYawInput, float sensitivityScale)
        {
            if (activeMode != RotationMode.None && activeMode != RotationMode.BankToTurn)
                bankYaw = dronePivot.localRotation.eulerAngles.y;
            activeMode = RotationMode.BankToTurn;

            dronePivot.localRotation = StepBankToTurn(ref bankYaw, tiltSwing, manualYawInput, rotSpeed, sensitivityScale, Time.deltaTime);
            UpdateCameraRotation();
        }

        public void RotateFpvAcro(Vector3 lookInput)
        {
            if (activeMode != RotationMode.None && activeMode != RotationMode.FpvAcro)
                acroRotation = dronePivot.localRotation;
            activeMode = RotationMode.FpvAcro;

            acroRotation = StepFpvAcro(acroRotation, lookInput, rotSpeed, Time.deltaTime);
            dronePivot.localRotation = acroRotation;
            UpdateCameraRotation();
        }

        internal static Quaternion StepBankToTurn(
            ref float currentYaw,
            Quaternion tiltSwing,
            float manualYawInput,
            float rotSpeed,
            float sensitivityScale,
            float deltaTime)
        {
            float bankYawRate = AccelerometerHandler.ComputeBankYawRate(tiltSwing);
            currentYaw += (bankYawRate * sensitivityScale + manualYawInput) * rotSpeed * deltaTime;
            return Quaternion.Euler(0f, currentYaw, 0f) * tiltSwing;
        }

        internal static Quaternion StepFpvAcro(Quaternion currentRotation, Vector3 lookInput, float rotSpeed, float deltaTime)
        {
            float pitchDelta = lookInput.y * rotSpeed * deltaTime;
            float yawDelta = lookInput.x * rotSpeed * deltaTime;
            float rollDelta = lookInput.z * rotSpeed * deltaTime;

            Quaternion localDelta = Quaternion.Euler(pitchDelta, yawDelta, rollDelta);
            return (currentRotation * localDelta).normalized;
        }

        internal void Restart()
        {
            rotation = Vector3.zero;
            bankYaw = 0f;
            acroRotation = Quaternion.identity;
            activeMode = RotationMode.None;
        }

        private void UpdateCameraRotation()
        {
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