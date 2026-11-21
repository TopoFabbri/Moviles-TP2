using System.Collections;
using System.Diagnostics;
using Game.World.FabbriPlugin;
using TMPro;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Game.World.UI
{
    public sealed class SceneTransitionService : MonoBehaviour
    {
        private const float MinDisplaySeconds = 0.4f;
        private const int OverlaySortingOrder = 32760;

        private static SceneTransitionService _instance;

        private Canvas _canvas;
        private Image _progressBarFill;
        private TextMeshProUGUI _percentText;
        private bool _isLoading;

        public static bool IsLoading => _instance != null && _instance._isLoading;

        public static void LoadSceneAsync(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("[SceneTransition] Cannot load null or empty scene path.");
                return;
            }

            EnsureInstance();
            if (_instance._isLoading)
                return;

            _instance.StartCoroutine(_instance.LoadSceneRoutine(scenePath));
        }

        private static void EnsureInstance()
        {
            if (_instance != null)
                return;

            var go = new GameObject(nameof(SceneTransitionService));
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<SceneTransitionService>();
            _instance.BuildOverlayUI();
        }

        private void BuildOverlayUI()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = OverlaySortingOrder;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            var bgGo = new GameObject("LoadingBG");
            bgGo.transform.SetParent(transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.06f, 0.09f, 1f);

            var titleGo = new GameObject("LoadingLabel");
            titleGo.transform.SetParent(bgGo.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.56f);
            titleRect.anchorMax = new Vector2(0.5f, 0.56f);
            titleRect.sizeDelta = new Vector2(600f, 90f);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "LOADING...";
            titleTmp.fontSize = 56f;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = Color.white;

            var barBgGo = new GameObject("ProgressBarBG");
            barBgGo.transform.SetParent(bgGo.transform, false);
            var barBgRect = barBgGo.AddComponent<RectTransform>();
            barBgRect.anchorMin = new Vector2(0.5f, 0.45f);
            barBgRect.anchorMax = new Vector2(0.5f, 0.45f);
            barBgRect.sizeDelta = new Vector2(800f, 44f);
            var barBgImg = barBgGo.AddComponent<Image>();
            barBgImg.color = new Color(0.18f, 0.2f, 0.25f, 1f);

            var barFillGo = new GameObject("ProgressBarFill");
            barFillGo.transform.SetParent(barBgGo.transform, false);
            var barFillRect = barFillGo.AddComponent<RectTransform>();
            barFillRect.anchorMin = Vector2.zero;
            barFillRect.anchorMax = Vector2.one;
            barFillRect.offsetMin = new Vector2(4f, 4f);
            barFillRect.offsetMax = new Vector2(-4f, -4f);

            _progressBarFill = barFillGo.AddComponent<Image>();
            _progressBarFill.sprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                new Vector2(0.5f, 0.5f));
            _progressBarFill.color = new Color(0.15f, 0.78f, 0.95f, 1f);
            _progressBarFill.type = Image.Type.Filled;
            _progressBarFill.fillMethod = Image.FillMethod.Horizontal;
            _progressBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            _progressBarFill.fillAmount = 0f;

            var pctGo = new GameObject("ProgressPercent");
            pctGo.transform.SetParent(bgGo.transform, false);
            var pctRect = pctGo.AddComponent<RectTransform>();
            pctRect.anchorMin = new Vector2(0.5f, 0.37f);
            pctRect.anchorMax = new Vector2(0.5f, 0.37f);
            pctRect.sizeDelta = new Vector2(300f, 60f);
            _percentText = pctGo.AddComponent<TextMeshProUGUI>();
            _percentText.text = "0%";
            _percentText.fontSize = 36f;
            _percentText.alignment = TextAlignmentOptions.Center;
            _percentText.color = new Color(0.8f, 0.9f, 1f, 1f);

            _canvas.enabled = false;
        }

        private IEnumerator LoadSceneRoutine(string scenePath)
        {
            _isLoading = true;
            _canvas.enabled = true;
            UpdateProgress(0f);

            var sw = Stopwatch.StartNew();
            AsyncOperation op = SceneManager.LoadSceneAsync(scenePath);
            if (op == null)
            {
                Debug.LogError($"[SceneTransition] Failed to start async load for: {scenePath}");
                _canvas.enabled = false;
                _isLoading = false;
                yield break;
            }

            op.allowSceneActivation = false;

            float elapsed = 0f;
            while (op.progress < 0.9f || elapsed < MinDisplaySeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float rawProgress = Mathf.Clamp01(op.progress / 0.9f);
                float timedProgress = Mathf.Clamp01(elapsed / MinDisplaySeconds);
                UpdateProgress(Mathf.Min(rawProgress, timedProgress));
                yield return null;
            }

            UpdateProgress(1f);
            op.allowSceneActivation = true;

            while (!op.isDone)
                yield return null;

            sw.Stop();
            LogTransition(scenePath, sw.ElapsedMilliseconds);

            _canvas.enabled = false;
            _isLoading = false;
        }

        private void UpdateProgress(float progress)
        {
            if (_progressBarFill != null)
                _progressBarFill.fillAmount = progress;

            if (_percentText != null)
                _percentText.text = $"{Mathf.RoundToInt(progress * 100f)}%";
        }

        private static void LogTransition(string scenePath, long elapsedMs)
        {
            string message = $"[SceneTransition] Async load completed: {scenePath} ({elapsedMs} ms)";
            Debug.Log(message);

            if (ServiceProvider.TryGetService(out IDiagnosticLogService logService))
                logService.RecordInfo("SceneTransition", $"Async load completed: {scenePath} ({elapsedMs} ms)");
        }
    }
}
