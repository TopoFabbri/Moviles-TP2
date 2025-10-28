using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.World.Character
{
    internal sealed class GyroscopeHandler
    {
        // Store the raw accelerometer value at neutral pose
        private Vector3 origin; // components in [-1,1]
        private bool calibrated;

        // Simple smoothing storage
        private Vector3 smoothed;

        // Dead zone to avoid jitter
        private const float DeadZone = 0.1f;
        private const float SmoothSpeed = 2f;

        public Vector3 GetRotation()
        {
            if (Accelerometer.current == null || !Accelerometer.current.enabled)
                return Vector3.zero;

            // Auto-calibrate if not yet calibrated
            if (!calibrated)
                Calibrate();

            Vector3 a = Accelerometer.current.acceleration.value; // [-1,1] per axis

            // Normalise each axis around the calibrated origin so that origin -> 0, extremes -> +/-1
            float normY = NormalizeAxis(a.y, origin.y); // pitch
            float normX = NormalizeAxis(a.x, origin.x); // roll source (note: sign inverted below)

            if (Mathf.Abs(normY) < DeadZone) normY = 0f;
            if (Mathf.Abs(normX) < DeadZone) normX = 0f;

            // Map to look axes consistent with previous code
            Vector3 target = new(0f, normY, -normX);

            // Exponential smoothing for stability
            float t = 1f - Mathf.Exp(-SmoothSpeed * Time.deltaTime);
            smoothed = Vector3.Lerp(smoothed, target, t);
            return smoothed;
        }

        private static float NormalizeAxis(float value, float origin)
        {
            // shift so origin -> 0
            float delta = value - origin;
            // choose scaling based on direction to preserve full range
            float denom = delta >= 0f ? (1f - origin) : (1f + origin);
            if (denom < 1e-3f)
                return 0f;
            float v = delta / denom;
            return Mathf.Clamp(v, -1f, 1f);
        }

        public void Calibrate()
        {
            if (Accelerometer.current == null || !Accelerometer.current.enabled)
            {
                calibrated = false;
                return;
            }

            origin = Accelerometer.current.acceleration.value;
            origin.x = Mathf.Clamp(origin.x, -1f, 1f);
            origin.y = Mathf.Clamp(origin.y, -1f, 1f);
            origin.z = Mathf.Clamp(origin.z, -1f, 1f);

            calibrated = true;
            smoothed = Vector3.zero; // reset smoothing to avoid jump
        }
    }
}