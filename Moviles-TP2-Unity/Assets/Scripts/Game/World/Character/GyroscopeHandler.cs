using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.World.Character
{
    internal static class GyroscopeHandler
    {
        public static bool Active { get; set; } = false;

        // Store the raw accelerometer value at neutral pose
        private static Vector3 _origin; // components in [-1,1]
        private static bool _calibrated;

        // Simple smoothing storage
        private static Vector3 _smoothed;

        // Dead zone to avoid jitter
        private const float DeadZone = 0.1f;
        private const float SmoothSpeed = 2f;

        public static Vector3 GetRotation()
        {
            if (!Active || Accelerometer.current == null || !Accelerometer.current.enabled)
                return Vector3.zero;

            // Auto-calibrate if not yet calibrated
            if (!_calibrated)
                Calibrate();

            Vector3 a = Accelerometer.current.acceleration.value; // [-1,1] per axis

            // Normalise each axis around the calibrated origin so that origin -> 0, extremes -> +/-1
            float normY = NormalizeAxis(a.y, _origin.y); // pitch
            float normX = NormalizeAxis(a.x, _origin.x); // roll source (note: sign inverted below)

            if (Mathf.Abs(normY) < DeadZone) normY = 0f;
            if (Mathf.Abs(normX) < DeadZone) normX = 0f;

            // Map to look axes consistent with previous code
            Vector3 target = new(0f, normY, -normX);

            // Exponential smoothing for stability
            float t = 1f - Mathf.Exp(-SmoothSpeed * Time.deltaTime);
            _smoothed = Vector3.Lerp(_smoothed, target, t);
            return _smoothed;
        }

        private static float NormalizeAxis(float value, float origin)
        {
            // shift so origin -> 0
            float delta = value - origin;
            // choose scaling based on direction to preserve full range
            float denom = delta >= 0f ? 1f - origin : 1f + origin;
            if (denom < 1e-3f)
                return 0f;
            float v = delta / denom;
            return Mathf.Clamp(v, -1f, 1f);
        }

        internal static void Calibrate()
        {
            if (!Active || Accelerometer.current == null || !Accelerometer.current.enabled)
            {
                _calibrated = false;
                return;
            }

            _origin = Accelerometer.current.acceleration.value;
            _origin.x = Mathf.Clamp(_origin.x, -1f, 1f);
            _origin.y = Mathf.Clamp(_origin.y, -1f, 1f);
            _origin.z = Mathf.Clamp(_origin.z, -1f, 1f);

            _calibrated = true;
            _smoothed = Vector3.zero; // reset smoothing to avoid jump
        }
    }
}