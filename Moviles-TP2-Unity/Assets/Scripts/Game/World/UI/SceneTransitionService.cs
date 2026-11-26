using System.Collections;
using System.Diagnostics;
using Game.Audio;
using Game.World.Ads;
using Game.World.FabbriPlugin;
using Game.World.Visuals;
using TMPro;
using Tools.ServiceProvider;
using Tools.Tween;
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
        private GameObject _loadingRoot;
        private Image _progressBarFill;
        private TextMeshProUGUI _percentText;
        private Image _stateFadeImage;
        private Coroutine _stateFadeCoroutine;
        private bool _isLoading;

        public static bool IsLoading => _instance != null && _instance._isLoading;

        public static void PlayStateTransitionFade(float duration = 0.3f)
        {
            EnsureInstance();
            if (_instance._stateFadeCoroutine != null)
                _instance.StopCoroutine(_instance._stateFadeCoroutine);

            _instance._stateFadeCoroutine = _instance.StartCoroutine(_instance.StateTransitionFadeRoutine(duration));
        }

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
            _loadingRoot = bgGo;
            bgGo.transform.SetParent(transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = Palette.Shadow;

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
            titleTmp.color = Palette.CharacterSecondary;

            var barBgGo = new GameObject("ProgressBarBG");
            barBgGo.transform.SetParent(bgGo.transform, false);
            var barBgRect = barBgGo.AddComponent<RectTransform>();
            barBgRect.anchorMin = new Vector2(0.5f, 0.45f);
            barBgRect.anchorMax = new Vector2(0.5f, 0.45f);
            barBgRect.sizeDelta = new Vector2(800f, 44f);
            var barBgImg = barBgGo.AddComponent<Image>();
            barBgImg.color = Palette.Bounds;

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
            _progressBarFill.color = Palette.CharacterPrimary;
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
            _percentText.color = Palette.CharacterSecondary;

            var fadeGo = new GameObject("StateFadeOverlay");
            fadeGo.transform.SetParent(transform, false);
            var fadeRect = fadeGo.AddComponent<RectTransform>();
            fadeRect.anchorMin = Vector2.zero;
            fadeRect.anchorMax = Vector2.one;
            fadeRect.offsetMin = Vector2.zero;
            fadeRect.offsetMax = Vector2.zero;
            _stateFadeImage = fadeGo.AddComponent<Image>();
            Color fadeColor = Palette.Shadow;
            fadeColor.a = 0f;
            _stateFadeImage.color = fadeColor;
            _stateFadeImage.raycastTarget = false;
            _stateFadeImage.enabled = false;

            _loadingRoot.SetActive(false);
            _canvas.enabled = false;
        }

        private IEnumerator StateTransitionFadeRoutine(float duration)
        {
            if (_stateFadeImage == null)
                yield break;

            bool prevCanvasEnabled = _canvas.enabled;
            if (!_isLoading)
                _canvas.enabled = true;

            _stateFadeImage.enabled = true;
            Color baseColor = Palette.Shadow;

            yield return Ease.Tween(duration * 0.5f, t =>
            {
                baseColor.a = Mathf.Lerp(0f, 0.65f, t);
                _stateFadeImage.color = baseColor;
            }, Ease.OutCubic, unscaled: true);

            yield return Ease.Tween(duration * 0.5f, t =>
            {
                baseColor.a = Mathf.Lerp(0.65f, 0f, t);
                _stateFadeImage.color = baseColor;
            }, Ease.OutCubic, unscaled: true);

            _stateFadeImage.enabled = false;
            if (!_isLoading)
                _canvas.enabled = prevCanvasEnabled;

            _stateFadeCoroutine = null;
        }

        private IEnumerator LoadSceneRoutine(string scenePath)
        {
            _isLoading = true;
            _loadingRoot.SetActive(true);
            _canvas.enabled = true;
            UpdateProgress(0f);

            var sw = Stopwatch.StartNew();
            AsyncOperation op = SceneManager.LoadSceneAsync(scenePath);
            if (op == null)
            {
                Debug.LogError($"[SceneTransition] Failed to start async load for: {scenePath}");
                _loadingRoot.SetActive(false);
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

            if (!ServiceProvider.TryGetService(out IAdService adService) || !adService.IsAdShowing)
                Time.timeScale = 1f;
            GameAudioService.SetPausedDucking(false);

            sw.Stop();
            LogTransition(scenePath, sw.ElapsedMilliseconds);

            _loadingRoot.SetActive(false);
            if (_stateFadeCoroutine == null)
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
