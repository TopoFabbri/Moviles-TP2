using System.Collections;
using Game.Audio;
using Game.Events;
using Game.World.Character;
using TMPro;
using Tools.EventSystem;
using Tools.ServiceProvider;
using Tools.Tween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.World.Visuals
{
    public class GameFeelDirector : MonoBehaviour
    {
        private static GameFeelDirector _instance;
        private Image _flashOverlay;
        private bool _subscribed;

        public static GameFeelDirector Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(GameFeelDirector));
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<GameFeelDirector>();
                    _instance.BuildFlashOverlay();
                }

                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            SceneManager.sceneLoaded += (_, _) => Instance.TrySubscribe();
            Instance.TrySubscribe();
        }

        private void TrySubscribe()
        {
            if (_subscribed) return;
            if (ServiceProvider.TryGetService<EventSystem>(out var eventSystem))
            {
                eventSystem.Subscribe<RingPassedEvent>(OnRingPassed);
                _subscribed = true;
            }
        }

        private void BuildFlashOverlay()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32700;

            var flashGo = new GameObject("MilestoneFlash");
            flashGo.transform.SetParent(transform, false);
            var rect = flashGo.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _flashOverlay = flashGo.AddComponent<Image>();
            Color c = Palette.Accent;
            c.a = 0f;
            _flashOverlay.color = c;
            _flashOverlay.raycastTarget = false;
            _flashOverlay.enabled = false;
        }

        private void OnRingPassed(RingPassedEvent evt)
        {
            if (DroneProceduralMotion.Instance != null)
            {
                DroneProceduralMotion.Instance.TriggerScalePop();
                DroneProceduralMotion.Instance.AddCameraTrauma(0.28f);
            }

            VfxPoolService.Instance.PlayRingPassBurst(evt.Position);
            Transform followTarget = DroneProceduralMotion.Instance != null ? DroneProceduralMotion.Instance.transform : null;
            VfxPoolService.Instance.SpawnFloatingText(evt.Position, "+RING", followTarget);
            GameAudioService.PlayPickupBlip();
        }

        public void TriggerCrashImpact(Vector3 position)
        {
            if (DroneProceduralMotion.Instance != null)
                DroneProceduralMotion.Instance.AddCameraTrauma(0.75f);

            VfxPoolService.Instance.PlayCrashBurst(position);
            GameAudioService.PlayCrashSound();
            StartCoroutine(HitStopRoutine());
        }

        private IEnumerator HitStopRoutine()
        {
            if (Time.timeScale <= 0.1f) yield break;
            float prevScale = Time.timeScale;

            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(0.06f);
            if (Mathf.Approximately(Time.timeScale, 0.05f))
                Time.timeScale = prevScale;
        }

        public void PlayWinMilestoneSequence(Transform winPanel, TextMeshProUGUI timeText = null, float finalTime = 0f)
        {
            StartCoroutine(WinMilestoneRoutine(winPanel, timeText, finalTime));
        }

        private IEnumerator WinMilestoneRoutine(Transform winPanel, TextMeshProUGUI timeText, float finalTime)
        {
            GameAudioService.PlayMilestoneSting();
            StartCoroutine(FlashRoutine());

            yield return new WaitForSecondsRealtime(0.05f);

            Vector3 confettiPos = DroneProceduralMotion.Instance != null
                ? DroneProceduralMotion.Instance.transform.position
                : Vector3.up * 2f;
            VfxPoolService.Instance.PlayMilestoneConfetti(confettiPos);

            if (winPanel != null)
            {
                winPanel.localScale = Vector3.zero;
                yield return Ease.Tween(0.35f, t =>
                {
                    if (winPanel != null)
                        winPanel.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, t);
                }, Ease.OutBack, unscaled: true);
            }

            if (timeText != null && finalTime > 0f)
            {
                yield return Ease.Tween(0.3f, t =>
                {
                    if (timeText != null)
                        timeText.text = $"{(finalTime * t):F2}s";
                }, Ease.OutCubic, unscaled: true);
            }
        }

        private IEnumerator FlashRoutine()
        {
            if (_flashOverlay == null) yield break;
            _flashOverlay.enabled = true;
            Color c = Palette.Accent;

            yield return Ease.Tween(0.1f, t =>
            {
                c.a = Mathf.Lerp(0.6f, 0f, t);
                _flashOverlay.color = c;
            }, Ease.OutCubic, unscaled: true);

            _flashOverlay.enabled = false;
        }
    }
}
