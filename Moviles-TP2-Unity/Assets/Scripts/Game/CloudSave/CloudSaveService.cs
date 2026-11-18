using System;
using System.Collections;
using Game.World.GameManager;
using UnityEngine;

namespace Game.CloudSave
{
    public class CloudSaveService : MonoBehaviour
    {
        private const float SaveIntervalSeconds = 60f;

        private static CloudSaveService _instance;
        public static bool HasInstance => _instance != null;
        public static CloudSaveService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CloudSaveService>();
                    if (_instance == null)
                    {
                        GameObject go = new(nameof(CloudSaveService));
                        _instance = go.AddComponent<CloudSaveService>();
                    }
                }

                return _instance;
            }
        }

        public event Action<CloudSaveData> OnCloudConflictDetected;

        public CloudSaveData PendingCloudData { get; private set; }

        private ICloudSaveProvider _provider;
        private Coroutine _periodicSaveCoroutine;
        private bool _hasCheckedInitialSave;
        private bool _isCheckingInitialSave;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID && !UNITY_EDITOR
            _provider = new GooglePlayCloudSaveProvider();
#else
            _provider = new MockCloudSaveProvider();
#endif
        }

        private void Start()
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            OnPlayServicesAuthenticated();
#endif
        }

        public void OnPlayServicesAuthenticated()
        {
            if (_hasCheckedInitialSave || _isCheckingInitialSave)
                return;

            if (_provider == null || !_provider.IsAvailable)
                return;

            _isCheckingInitialSave = true;

            _provider.LoadCloudSave((success, cloudData) =>
            {
                _isCheckingInitialSave = false;

                if (!success)
                {
                    Debug.LogWarning("[CloudSave] Initial cloud save check failed.");
                    return;
                }

                _hasCheckedInitialSave = true;

                if (cloudData != null)
                {
                    bool shouldPrompt = !GameSave.HasCloudDecisionBeenMade ||
                                        (cloudData.savedAtUtcTicks > 0 && cloudData.savedAtUtcTicks > GameSave.LastSavedUtcTicks);

                    if (shouldPrompt)
                    {
                        PendingCloudData = cloudData;
                        OnCloudConflictDetected?.Invoke(cloudData);

                        if (FindFirstObjectByType<Game.World.UI.CloudSavePopupController>() == null)
                            ShowFallbackPopup(cloudData);
                    }
                    else
                    {
                        StartPeriodicSave();
                    }
                }
                else
                {
                    GameSave.HasCloudDecisionBeenMade = true;
                    StartPeriodicSave();
                }
            });
        }

        public void ResolveUseCloud()
        {
            if (PendingCloudData != null)
                GameSave.ImportSnapshot(PendingCloudData);

            PendingCloudData = null;
            GameSave.HasCloudDecisionBeenMade = true;
            StartPeriodicSave();
        }

        public void ResolveKeepLocal()
        {
            PendingCloudData = null;
            GameSave.HasCloudDecisionBeenMade = true;
            SaveNow();
            StartPeriodicSave();
        }

        public void SaveNow()
        {
            if (_provider == null || !_provider.IsAvailable)
                return;

            if (!GameSave.HasCloudDecisionBeenMade || !_hasCheckedInitialSave || PendingCloudData != null)
                return;

            CloudSaveData snapshot = GameSave.ExportSnapshot();
            _provider.SaveToCloud(snapshot, success =>
            {
                if (!success)
                    Debug.LogWarning("[CloudSave] Failed to save snapshot to cloud.");
            });
        }

        private void StartPeriodicSave()
        {
            if (_periodicSaveCoroutine != null)
                StopCoroutine(_periodicSaveCoroutine);

            _periodicSaveCoroutine = StartCoroutine(PeriodicSaveRoutine());
        }

        private IEnumerator PeriodicSaveRoutine()
        {
            WaitForSecondsRealtime wait = new(SaveIntervalSeconds);
            while (true)
            {
                yield return wait;
                SaveNow();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && GameSave.HasCloudDecisionBeenMade)
                SaveNow();
        }

        private void OnApplicationQuit()
        {
            if (GameSave.HasCloudDecisionBeenMade)
                SaveNow();
        }

        private void ShowFallbackPopup(CloudSaveData cloudData)
        {
            GameObject canvasGo = new("CloudSaveFallbackCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            UnityEngine.UI.CanvasScaler scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            GameObject backdrop = new("Backdrop");
            backdrop.transform.SetParent(canvasGo.transform, false);
            UnityEngine.UI.Image backdropImg = backdrop.AddComponent<UnityEngine.UI.Image>();
            backdropImg.color = new Color(0f, 0f, 0f, 0.75f);
            RectTransform backdropRt = backdrop.GetComponent<RectTransform>();
            backdropRt.anchorMin = Vector2.zero;
            backdropRt.anchorMax = Vector2.one;
            backdropRt.offsetMin = Vector2.zero;
            backdropRt.offsetMax = Vector2.zero;

            GameObject panel = new("Panel");
            panel.transform.SetParent(backdrop.transform, false);
            UnityEngine.UI.Image panelImg = panel.AddComponent<UnityEngine.UI.Image>();
            panelImg.color = new Color(0.12f, 0.14f, 0.18f, 1f);
            RectTransform panelRt = panel.GetComponent<RectTransform>();
            panelRt.sizeDelta = new Vector2(900f, 520f);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            CreateFallbackText(panel.transform, font, "Cloud Save Found", 40, FontStyle.Bold,
                new Vector2(0f, 190f), new Vector2(820f, 70f));

            string localText = $"LOCAL SAVE\nLevel: {GameSave.CurrentLevel}\nCredits: {GameSave.Credits}\nAdded Speed: {GameSave.AddedSpeed:F1}";
            CreateFallbackText(panel.transform, font, localText, 28, FontStyle.Normal,
                new Vector2(-210f, 20f), new Vector2(380f, 220f));

            string cloudText = $"CLOUD SAVE\nLevel: {cloudData.currentLevel}\nCredits: {cloudData.credits}\nAdded Speed: {cloudData.addedSpeed:F1}";
            CreateFallbackText(panel.transform, font, cloudText, 28, FontStyle.Normal,
                new Vector2(210f, 20f), new Vector2(380f, 220f));

            CreateFallbackButton(panel.transform, font, "Use Cloud", new Color(0.18f, 0.55f, 0.9f, 1f),
                new Vector2(-180f, -170f), () =>
                {
                    ResolveUseCloud();
                    Destroy(canvasGo);
                });

            CreateFallbackButton(panel.transform, font, "Keep Local", new Color(0.35f, 0.38f, 0.44f, 1f),
                new Vector2(180f, -170f), () =>
                {
                    ResolveKeepLocal();
                    Destroy(canvasGo);
                });
        }

        private static void CreateFallbackText(Transform parent, Font font, string content, int fontSize, FontStyle style, Vector2 anchoredPos, Vector2 size)
        {
            GameObject go = new("Text");
            go.transform.SetParent(parent, false);
            UnityEngine.UI.Text text = go.AddComponent<UnityEngine.UI.Text>();
            text.font = font;
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }

        private static void CreateFallbackButton(Transform parent, Font font, string label, Color bgColor, Vector2 anchoredPos, Action onClick)
        {
            GameObject go = new(label + "Button");
            go.transform.SetParent(parent, false);
            UnityEngine.UI.Image img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = bgColor;
            UnityEngine.UI.Button btn = go.AddComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(280f, 80f);

            CreateFallbackText(go.transform, font, label, 28, FontStyle.Bold, Vector2.zero, new Vector2(260f, 70f));
        }
    }
}
