using System.Collections;
using Tools.Tween;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.World.Character
{
    public class DroneProceduralMotion : MonoBehaviour
    {
        public static DroneProceduralMotion Instance { get; private set; }

        private Vector3 _baseLocalPos;
        private Quaternion _baseLocalRot;
        private Vector3 _baseLocalScale;
        private bool _capturedBaseline;

        private Rigidbody _rb;
        private Vector3 _prevVel;
        private float _currentPitch;
        private float _currentRoll;

        private Coroutine _popCoroutine;
        private float _trauma;
        private Vector3 _camBaseLocalPos;
        private Camera _mainCam;
        private bool _capturedCamBaseline;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void HookSceneLoad()
        {
            SceneManager.sceneLoaded += (_, _) => EnsureAttached();
            EnsureAttached();
        }

        public static void EnsureAttached()
        {
            var controller = FindFirstObjectByType<DroneController>();
            if (controller == null) return;

            Transform visualTarget = null;
            Renderer[] renderers = controller.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Transform t = renderers[i].transform;
                if (t == controller.transform) continue;
                if (t.name.Contains("CamPivot")) continue;
                visualTarget = t;
                break;
            }

            if (visualTarget == null)
                visualTarget = controller.transform;

            if (visualTarget.GetComponent<DroneProceduralMotion>() == null)
                visualTarget.gameObject.AddComponent<DroneProceduralMotion>();
        }

        private void Awake()
        {
            Instance = this;
            CaptureBaseline();
            _rb = GetComponentInParent<Rigidbody>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void CaptureBaseline()
        {
            if (_capturedBaseline) return;
            _baseLocalPos = transform.localPosition;
            _baseLocalRot = transform.localRotation;
            _baseLocalScale = transform.localScale;
            _capturedBaseline = true;
        }

        private void Update()
        {
            if (!_capturedBaseline)
                CaptureBaseline();

            float t = Time.time;
            float bob = Mathf.Sin(t * 0.75f * Mathf.PI * 2f) * 0.08f
                      + Mathf.Sin(t * 1.5f * Mathf.PI * 2f) * 0.02f;
            float idleRoll = Mathf.Sin(t * 0.38f * Mathf.PI * 2f) * 1.8f;

            float targetPitch = 0f;
            float targetRoll = idleRoll;

            if (_rb != null)
            {
                Vector3 vel = _rb.linearVelocity;
                Vector3 localVel = transform.parent != null
                    ? transform.parent.InverseTransformDirection(vel)
                    : vel;

                float speedNorm = Mathf.Clamp(localVel.z / 20f, -1f, 1f);
                float lateralNorm = Mathf.Clamp(localVel.x / 12f, -1f, 1f);

                targetPitch = speedNorm * 8f;
                targetRoll = idleRoll - lateralNorm * 6f;
                _prevVel = vel;
            }

            float lerpRate = Time.deltaTime / 0.3f;
            _currentPitch = Mathf.Lerp(_currentPitch, targetPitch, lerpRate);
            _currentRoll = Mathf.Lerp(_currentRoll, targetRoll, lerpRate);

            transform.localPosition = _baseLocalPos + new Vector3(0f, bob, 0f);
            transform.localRotation = _baseLocalRot * Quaternion.Euler(_currentPitch, 0f, _currentRoll);

            UpdateCameraShake();
        }

        public void TriggerScalePop()
        {
            if (!_capturedBaseline)
                CaptureBaseline();

            if (_popCoroutine != null)
            {
                StopCoroutine(_popCoroutine);
                transform.localScale = _baseLocalScale;
            }

            _popCoroutine = StartCoroutine(ScalePopRoutine());
        }

        private IEnumerator ScalePopRoutine()
        {
            Vector3 peakScale = _baseLocalScale * 1.25f;

            yield return Ease.Tween(0.06f, p =>
            {
                transform.localScale = Vector3.LerpUnclamped(_baseLocalScale, peakScale, p);
            }, Ease.OutBack, unscaled: false);

            yield return Ease.Tween(0.09f, p =>
            {
                transform.localScale = Vector3.LerpUnclamped(peakScale, _baseLocalScale, p);
            }, Ease.OutCubic, unscaled: false);

            transform.localScale = _baseLocalScale;
            _popCoroutine = null;
        }

        public void AddCameraTrauma(float amount)
        {
            _trauma = Mathf.Clamp01(_trauma + amount);
        }

        private void UpdateCameraShake()
        {
            if (_mainCam == null)
            {
                _mainCam = Camera.main;
                if (_mainCam != null && !_capturedCamBaseline)
                {
                    _camBaseLocalPos = _mainCam.transform.localPosition;
                    _capturedCamBaseline = true;
                }
            }

            if (_mainCam == null || !_capturedCamBaseline)
                return;

            if (_trauma <= 0.001f)
            {
                _trauma = 0f;
                _mainCam.transform.localPosition = _camBaseLocalPos;
                return;
            }

            float shake = _trauma * _trauma * 0.35f;
            float offsetX = (Mathf.PerlinNoise(Time.unscaledTime * 28f, 0f) * 2f - 1f) * shake;
            float offsetY = (Mathf.PerlinNoise(0f, Time.unscaledTime * 28f) * 2f - 1f) * shake;
            _mainCam.transform.localPosition = _camBaseLocalPos + new Vector3(offsetX, offsetY, 0f);

            _trauma = Mathf.Max(0f, _trauma - Time.unscaledDeltaTime * 2.2f);
        }
    }
}
