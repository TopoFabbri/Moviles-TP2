using Game.World.GameManager;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.World.Character
{
    internal class AccelerometerHandler : IService
    {
        private enum SensorMode
        {
            None,
            Attitude,
            Gravity,
            Accelerometer
        }

        private SensorMode activeMode = SensorMode.None;
        private Quaternion originAttitude = Quaternion.identity;
        private Vector3 originGravity = Vector3.down;
        private bool calibrated;

        private Vector3 smoothed;

        // Full deflection reached at +/- MaxTiltAngle degrees from calibrated pose
        private const float MaxTiltAngle = 45f;
        private const float DeadZone = 0.06f;
        private const float SmoothSpeed = 12f;

        public Vector3 GetRotation()
        {
            SensorMode currentMode = DetectActiveSensor();
            if (currentMode == SensorMode.None)
                return Vector3.zero;

            if (currentMode != activeMode)
            {
                activeMode = currentMode;
                calibrated = false;
            }

            if (!calibrated)
            {
                Calibrate();
                if (!calibrated)
                    return Vector3.zero;
            }

            Vector2 tilt = activeMode switch
            {
                SensorMode.Attitude => ComputeTiltFromAttitude(originAttitude, AttitudeSensor.current.attitude.ReadValue(), MaxTiltAngle),
                SensorMode.Gravity => ComputeTiltFromGravity(originGravity, GravitySensor.current.gravity.ReadValue(), MaxTiltAngle),
                SensorMode.Accelerometer => ComputeTiltFromGravity(originGravity, Accelerometer.current.acceleration.ReadValue(), MaxTiltAngle),
                _ => Vector2.zero
            };

            float normX = ApplyDeadZone(tilt.x, DeadZone);
            float normY = ApplyDeadZone(tilt.y, DeadZone);

            normX *= GameSave.InvertCamX ? -1f : 1f;
            normY *= GameSave.InvertCamY ? -1f : 1f;

            // Map to look axes: y = pitch, z = roll (inverted to match Unity Z-forward roll)
            Vector3 target = new(0f, normY, -normX);

            float t = 1f - Mathf.Exp(-SmoothSpeed * Time.deltaTime);
            smoothed = Vector3.Lerp(smoothed, target, t);
            return smoothed;
        }

        internal void Calibrate()
        {
            SensorMode currentMode = DetectActiveSensor();
            activeMode = currentMode;

            switch (currentMode)
            {
                case SensorMode.Attitude:
                {
                    Quaternion q = AttitudeSensor.current.attitude.ReadValue();
                    if (!IsValidQuaternion(q))
                    {
                        calibrated = false;
                        return;
                    }

                    originAttitude = q.normalized;
                    calibrated = true;
                    smoothed = Vector3.zero;
                    break;
                }
                case SensorMode.Gravity:
                {
                    Vector3 g = GravitySensor.current.gravity.ReadValue();
                    if (g.sqrMagnitude < 1e-4f)
                    {
                        calibrated = false;
                        return;
                    }

                    originGravity = g.normalized;
                    calibrated = true;
                    smoothed = Vector3.zero;
                    break;
                }
                case SensorMode.Accelerometer:
                {
                    Vector3 a = Accelerometer.current.acceleration.ReadValue();
                    if (a.sqrMagnitude < 1e-4f)
                    {
                        calibrated = false;
                        return;
                    }

                    originGravity = a.normalized;
                    calibrated = true;
                    smoothed = Vector3.zero;
                    break;
                }
                default:
                    calibrated = false;
                    break;
            }
        }

        private static SensorMode DetectActiveSensor()
        {
            if (AttitudeSensor.current != null && AttitudeSensor.current.enabled)
                return SensorMode.Attitude;
            if (GravitySensor.current != null && GravitySensor.current.enabled)
                return SensorMode.Gravity;
            if (Accelerometer.current != null && Accelerometer.current.enabled)
                return SensorMode.Accelerometer;
            return SensorMode.None;
        }

        internal static bool IsValidQuaternion(Quaternion q)
        {
            float sqrMag = q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
            return sqrMag > 1e-6f && !float.IsNaN(sqrMag) && !float.IsInfinity(sqrMag);
        }

        /// <summary>
        /// Computes normalized (roll, pitch) in [-1, 1] from AttitudeSensor quaternions.
        /// Uses basis-vector Atan2 on the relative quaternion so full [-180, 180] range is preserved
        /// on both pitch and roll without Euler gimbal lock or 90-degree bounce.
        /// </summary>
        internal static Vector2 ComputeTiltFromAttitude(Quaternion origin, Quaternion current, float maxTiltAngle)
        {
            if (!IsValidQuaternion(origin) || !IsValidQuaternion(current) || maxTiltAngle <= 0f)
                return Vector2.zero;

            Quaternion delta = Quaternion.Inverse(origin.normalized) * current.normalized;
            return ExtractTiltFromDelta(delta, maxTiltAngle);
        }

        /// <summary>
        /// Computes normalized (roll, pitch) in [-1, 1] from GravitySensor / Accelerometer vectors.
        /// Converts the gravity delta into a relative device rotation to avoid cross-axis 180-degree flips
        /// and 90-degree bounce.
        /// </summary>
        internal static Vector2 ComputeTiltFromGravity(Vector3 originGravity, Vector3 currentGravity, float maxTiltAngle)
        {
            if (originGravity.sqrMagnitude < 1e-4f || currentGravity.sqrMagnitude < 1e-4f || maxTiltAngle <= 0f)
                return Vector2.zero;

            Vector3 g0 = originGravity.normalized;
            Vector3 g1 = currentGravity.normalized;

            // Device rotation is the inverse of the gravity vector's rotation in device space
            Quaternion delta = Quaternion.FromToRotation(g1, g0);
            return ExtractTiltFromDelta(delta, maxTiltAngle);
        }

        private static Vector2 ExtractTiltFromDelta(Quaternion delta, float maxTiltAngle)
        {
            // Pitch: rotation of local +Z in the YZ plane (negative X rotation tilts +Z toward +Y -> positive pitch)
            Vector3 fwd = delta * Vector3.forward;
            float pitchDeg = (fwd.y * fwd.y + fwd.z * fwd.z > 1e-6f)
                ? Mathf.Atan2(fwd.y, fwd.z) * Mathf.Rad2Deg
                : 0f;

            // Roll: rotation of local +X in the XY plane (negative Z rotation tilts +X toward -Y -> positive roll)
            Vector3 right = delta * Vector3.right;
            float rollDeg = (right.x * right.x + right.y * right.y > 1e-6f)
                ? Mathf.Atan2(-right.y, right.x) * Mathf.Rad2Deg
                : 0f;

            float normX = Mathf.Clamp(rollDeg / maxTiltAngle, -1f, 1f);
            float normY = Mathf.Clamp(pitchDeg / maxTiltAngle, -1f, 1f);
            return new Vector2(normX, normY);
        }

        internal static float ApplyDeadZone(float value, float deadZone)
        {
            float abs = Mathf.Abs(value);
            if (abs <= deadZone)
                return 0f;
            if (deadZone >= 1f)
                return 0f;

            float rescaled = (abs - deadZone) / (1f - deadZone);
            return Mathf.Clamp01(rescaled) * Mathf.Sign(value);
        }
    }
}
