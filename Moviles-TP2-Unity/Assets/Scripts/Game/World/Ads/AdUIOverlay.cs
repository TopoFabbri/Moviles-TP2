using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World.Ads
{
    /// <summary>
    /// Persistent full-screen UI overlay that visually renders simulated interstitial
    /// and rewarded ads in the Editor and on devices without live ad network fill.
    /// Matches the game's neon sci-fi low-poly aesthetic.
    /// </summary>
    public sealed class AdUIOverlay : MonoBehaviour
    {
        private static AdUIOverlay _instance;

        public static AdUIOverlay Instance
        {
            get
            {
                if (_instance == null)
                    EnsureInstance();
                return _instance;
            }
        }

        private GameObject _overlayRoot;
        private TextMeshProUGUI _badgeText;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _subtitleText;
        private TextMeshProUGUI _countdownText;
        private Image _progressBarFill;
        private Button _actionButton;
        private TextMeshProUGUI _actionButtonText;
        private Image _videoFramePulse;

        private Coroutine _activeAdCoroutine;
        private float _previousTimeScale = 1f;
        private bool _isPausedForAd;

        public static void EnsureInstance()
        {
            if (_instance != null) return;

            var existing = FindFirstObjectByType<AdUIOverlay>();
            if (existing != null)
            {
                _instance = existing;
                return;
            }

            GameObject go = new GameObject("AdUIOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<AdUIOverlay>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            BuildUIHierarchy();
        }

        public void ShowInterstitialAd(string placement, Action onClosed)
        {
            if (_activeAdCoroutine != null)
                StopCoroutine(_activeAdCoroutine);

            _activeAdCoroutine = StartCoroutine(RunInterstitialRoutine(placement, onClosed));
        }

        public void ShowRewardedAd(string placement, Action<bool> onRewardResult)
        {
            if (_activeAdCoroutine != null)
                StopCoroutine(_activeAdCoroutine);

            _activeAdCoroutine = StartCoroutine(RunRewardedRoutine(placement, onRewardResult));
        }

        private IEnumerator RunInterstitialRoutine(string placement, Action onClosed)
        {
            PauseGame();
            _overlayRoot.SetActive(true);

            _badgeText.text = "[ SPONSORED ADVERTISEMENT ]";
            _titleText.text = "INTERSTITIAL AD";
            _subtitleText.text = $"Placement: {placement}\nCalibrating telemetry systems...";
            _actionButton.gameObject.SetActive(false);
            _actionButton.interactable = false;
            _progressBarFill.fillAmount = 0f;

            const float duration = 3.0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                _progressBarFill.fillAmount = progress;

                int remaining = Mathf.CeilToInt(duration - elapsed);
                _countdownText.text = $"Ad finishes in {remaining}s...";

                if (_videoFramePulse != null)
                {
                    float pulse = 0.5f + 0.5f * Mathf.Sin(elapsed * 4f);
                    _videoFramePulse.color = Color.Lerp(new Color(0f, 0.8f, 1f, 0.15f), new Color(0f, 1f, 0.7f, 0.35f), pulse);
                }

                yield return null;
            }

            _progressBarFill.fillAmount = 1f;
            _countdownText.text = "Ad completed!";
            _actionButton.gameObject.SetActive(true);
            _actionButton.interactable = true;
            _actionButtonText.text = "CLOSE [X]";

            if (UnityEngine.EventSystems.EventSystem.current != null)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(_actionButton.gameObject);

            bool userClosed = false;
            _actionButton.onClick.RemoveAllListeners();
            _actionButton.onClick.AddListener(() => userClosed = true);

            // Give up to 3.0s auto-close or until user clicks
            float autoCloseElapsed = 0f;
            while (!userClosed && autoCloseElapsed < 3.0f)
            {
                autoCloseElapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            CloseOverlay();
            onClosed?.Invoke();
        }

        private IEnumerator RunRewardedRoutine(string placement, Action<bool> onRewardResult)
        {
            PauseGame();
            _overlayRoot.SetActive(true);

            _badgeText.text = "[ REWARDED SPONSOR ]";
            _titleText.text = "REWARDED AD";
            _subtitleText.text = $"Watch sponsored clip to earn free Drone Credits!\n({placement})";
            _actionButton.gameObject.SetActive(false);
            _actionButton.interactable = false;
            _progressBarFill.fillAmount = 0f;

            const float duration = 5.0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                _progressBarFill.fillAmount = progress;

                int remaining = Mathf.CeilToInt(duration - elapsed);
                _countdownText.text = $"Reward in {remaining}s (do not close)...";

                if (_videoFramePulse != null)
                {
                    float pulse = 0.5f + 0.5f * Mathf.Sin(elapsed * 5f);
                    _videoFramePulse.color = Color.Lerp(new Color(0f, 1f, 0.4f, 0.15f), new Color(1f, 0.9f, 0f, 0.35f), pulse);
                }

                yield return null;
            }

            _progressBarFill.fillAmount = 1f;
            _countdownText.text = "SUCCESS! Reward Granted!";
            _actionButton.gameObject.SetActive(true);
            _actionButton.interactable = true;
            _actionButtonText.text = "CLAIM REWARD";

            if (UnityEngine.EventSystems.EventSystem.current != null)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(_actionButton.gameObject);

            bool userClaimed = false;
            _actionButton.onClick.RemoveAllListeners();
            _actionButton.onClick.AddListener(() => userClaimed = true);

            // Wait for user to click claim or auto-claim after 3.5 seconds
            float claimTimer = 0f;
            while (!userClaimed && claimTimer < 3.5f)
            {
                claimTimer += Time.unscaledDeltaTime;
                yield return null;
            }

            CloseOverlay();
            onRewardResult?.Invoke(true);
        }

        private void CloseOverlay()
        {
            _overlayRoot.SetActive(false);
            _activeAdCoroutine = null;

            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == _actionButton.gameObject)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }

            ResumeGame();
        }

        private void PauseGame()
        {
            if (!_isPausedForAd)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                _isPausedForAd = true;
            }
        }

        private void ResumeGame()
        {
            if (_isPausedForAd)
            {
                Time.timeScale = _previousTimeScale > 0f ? _previousTimeScale : 1f;
                _isPausedForAd = false;
            }
        }

        private void OnDisable()
        {
            if (_isPausedForAd)
            {
                ResumeGame();
            }
        }

        private void OnDestroy()
        {
            if (_isPausedForAd)
            {
                ResumeGame();
            }
        }

        private void BuildUIHierarchy()
        {
            // Canvas setup
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            // Overlay dark backdrop
            _overlayRoot = new GameObject("OverlayRoot", typeof(RectTransform));
            _overlayRoot.transform.SetParent(transform, false);

            RectTransform rootRect = _overlayRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;

            Image backdropImage = _overlayRoot.AddComponent<Image>();
            backdropImage.color = new Color(0.02f, 0.03f, 0.07f, 0.95f);

            // Dialog container
            GameObject dialog = new GameObject("DialogBox", typeof(RectTransform), typeof(Image));
            dialog.transform.SetParent(_overlayRoot.transform, false);
            RectTransform dialogRect = dialog.GetComponent<RectTransform>();
            dialogRect.sizeDelta = new Vector2(760, 520);
            dialogRect.anchoredPosition = Vector2.zero;

            Image dialogBg = dialog.GetComponent<Image>();
            dialogBg.color = new Color(0.07f, 0.10f, 0.18f, 0.98f);

            // Dialog neon outline
            Outline dialogOutline = dialog.AddComponent<Outline>();
            dialogOutline.effectColor = new Color(0f, 1f, 1f, 0.8f);
            dialogOutline.effectDistance = new Vector2(3, -3);

            // Badge text ("[ ADVERTISEMENT ]")
            _badgeText = CreateTMP(dialog.transform, "Badge", new Vector2(0, 205), new Vector2(400, 35), 18, FontStyles.Bold, new Color(1f, 0.85f, 0.2f, 1f));

            // Title text ("INTERSTITIAL AD" / "REWARDED AD")
            _titleText = CreateTMP(dialog.transform, "Title", new Vector2(0, 160), new Vector2(650, 48), 34, FontStyles.Bold, Color.white);

            // Video simulation container
            GameObject videoBox = new GameObject("VideoSimulation", typeof(RectTransform), typeof(Image));
            videoBox.transform.SetParent(dialog.transform, false);
            RectTransform videoRect = videoBox.GetComponent<RectTransform>();
            videoRect.sizeDelta = new Vector2(640, 160);
            videoRect.anchoredPosition = new Vector2(0, 40);
            _videoFramePulse = videoBox.GetComponent<Image>();
            _videoFramePulse.color = new Color(0f, 0.7f, 1f, 0.25f);

            Outline videoOutline = videoBox.AddComponent<Outline>();
            videoOutline.effectColor = new Color(0f, 0.8f, 1f, 0.4f);
            videoOutline.effectDistance = new Vector2(2, -2);

            // Subtitle text inside or below video container
            _subtitleText = CreateTMP(videoBox.transform, "Subtitle", Vector2.zero, new Vector2(600, 120), 22, FontStyles.Normal, new Color(0.85f, 0.95f, 1f, 1f));

            // Progress bar background
            GameObject progressBg = new GameObject("ProgressBarBg", typeof(RectTransform), typeof(Image));
            progressBg.transform.SetParent(dialog.transform, false);
            RectTransform progBgRect = progressBg.GetComponent<RectTransform>();
            progBgRect.sizeDelta = new Vector2(640, 16);
            progBgRect.anchoredPosition = new Vector2(0, -75);
            Image progBgImage = progressBg.GetComponent<Image>();
            progBgImage.color = new Color(0.15f, 0.20f, 0.30f, 1f);

            // Progress bar fill
            GameObject progressFill = new GameObject("ProgressBarFill", typeof(RectTransform), typeof(Image));
            progressFill.transform.SetParent(progressBg.transform, false);
            RectTransform progFillRect = progressFill.GetComponent<RectTransform>();
            progFillRect.anchorMin = Vector2.zero;
            progFillRect.anchorMax = Vector2.one;
            progFillRect.sizeDelta = Vector2.zero;
            _progressBarFill = progressFill.GetComponent<Image>();
            _progressBarFill.color = new Color(0f, 1f, 0.7f, 1f);
            _progressBarFill.type = Image.Type.Filled;
            _progressBarFill.fillMethod = Image.FillMethod.Horizontal;
            _progressBarFill.fillAmount = 0f;

            // Countdown text
            _countdownText = CreateTMP(dialog.transform, "Countdown", new Vector2(0, -115), new Vector2(500, 36), 22, FontStyles.Bold, new Color(0f, 1f, 1f, 1f));

            // Action / Close button
            GameObject buttonObj = new GameObject("ActionButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObj.transform.SetParent(dialog.transform, false);
            RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(300, 56);
            btnRect.anchoredPosition = new Vector2(0, -180);

            Image btnImage = buttonObj.GetComponent<Image>();
            btnImage.color = new Color(0f, 0.8f, 0.9f, 1f);

            _actionButton = buttonObj.GetComponent<Button>();
            ColorBlock colors = _actionButton.colors;
            colors.normalColor = new Color(0f, 0.8f, 0.9f, 1f);
            colors.highlightedColor = new Color(0f, 1f, 0.8f, 1f);
            colors.pressedColor = new Color(1f, 0.5f, 0.2f, 1f);
            colors.disabledColor = new Color(0.3f, 0.4f, 0.5f, 0.5f);
            _actionButton.colors = colors;

            _actionButtonText = CreateTMP(buttonObj.transform, "BtnText", Vector2.zero, new Vector2(280, 48), 24, FontStyles.Bold, Color.black);
            _actionButtonText.text = "CLOSE [X]";

            _overlayRoot.SetActive(false);
        }

        private TextMeshProUGUI CreateTMP(Transform parent, string name, Vector2 pos, Vector2 size, float fontSize, FontStyles style, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = name;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return tmp;
        }
    }
}
