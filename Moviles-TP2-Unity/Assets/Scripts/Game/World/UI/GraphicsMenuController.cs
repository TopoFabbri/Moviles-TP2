using System;
using System.Collections.Generic;
using Game.World.Graphics;
using TMPro;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.World.UI
{
    internal sealed class GraphicsMenuController : MonoBehaviour
    {
        private GameObject _mainScrollView;
        private GameObject _graphicsScrollView;
        private Button _openGraphicsBtn;
        private Button _backBtn;

        private TextMeshProUGUI _presetText;
        private TextMeshProUGUI _fpsText;
        private TextMeshProUGUI _renderScaleText;
        private TextMeshProUGUI _shadowsText;
        private Toggle _postFxToggle;
        private TextMeshProUGUI _msaaText;
        private TextMeshProUGUI _textureText;
        private Toggle _adaptiveToggle;

        private IGraphicsSettingsService _service;
        private bool _built;

        private void Awake()
        {
            ServiceProvider.TryGetService(out _service);
            EnsureUiBuilt();
        }

        private void OnEnable()
        {
            if (_service == null)
                ServiceProvider.TryGetService(out _service);

            if (_service != null)
                _service.OnSettingsChanged += OnSettingsChanged;

            ShowMainPanel();
            RefreshUI();
        }

        private void OnDisable()
        {
            if (_service != null)
                _service.OnSettingsChanged -= OnSettingsChanged;
        }

        public void OpenGraphicsPanel()
        {
            if (!_built) EnsureUiBuilt();
            if (_mainScrollView != null) _mainScrollView.SetActive(false);
            if (_graphicsScrollView != null) _graphicsScrollView.SetActive(true);
            RefreshUI();
            SelectIfGamepad(_backBtn != null ? _backBtn.gameObject : null);
        }

        public void ShowMainPanel()
        {
            if (_graphicsScrollView != null) _graphicsScrollView.SetActive(false);
            if (_mainScrollView != null) _mainScrollView.SetActive(true);
            SelectIfGamepad(_openGraphicsBtn != null ? _openGraphicsBtn.gameObject : null);
        }

        private void EnsureUiBuilt()
        {
            if (_built) return;

            Transform mainScroll = transform.Find("Scroll View");
            Transform mainContent = mainScroll != null ? mainScroll.Find("Viewport/Content") : null;
            if (mainScroll == null || mainContent == null)
                return;

            Transform labelTemplate = mainContent.Find("ControlSchemeTxt");
            Transform buttonTemplate = mainContent.Find("ControlSchemeBtn");
            Transform spaceTemplate = mainContent.Find("Space");
            Transform toggleTemplate = mainContent.Find("GodMode");
            if (labelTemplate == null || buttonTemplate == null || toggleTemplate == null)
                return;

            _mainScrollView = mainScroll.gameObject;

            _openGraphicsBtn = EnsureEntryButton(mainContent, spaceTemplate, buttonTemplate);

            Transform existingGraphics = transform.Find("GraphicsScrollView");
            _graphicsScrollView = existingGraphics != null
                ? existingGraphics.gameObject
                : Instantiate(_mainScrollView, transform);
            _graphicsScrollView.name = "GraphicsScrollView";

            Transform gfxContent = _graphicsScrollView.transform.Find("Viewport/Content");
            if (gfxContent == null)
                return;

            var toDestroy = new List<GameObject>(gfxContent.childCount);
            for (int i = 0; i < gfxContent.childCount; i++)
                toDestroy.Add(gfxContent.GetChild(i).gameObject);

            _backBtn = CreateButtonRow(gfxContent, buttonTemplate, "GraphicsBackBtn", "< Back", ShowMainPanel);

            AddSpacer(gfxContent, spaceTemplate, "GfxSpace0");
            _presetText = CreateCyclerRow(gfxContent, labelTemplate, buttonTemplate, "Preset", "Quality Preset:", OnCyclePreset);

            AddSpacer(gfxContent, spaceTemplate, "GfxSpace1");
            _fpsText = CreateCyclerRow(gfxContent, labelTemplate, buttonTemplate, "TargetFps", "Target FPS:", OnCycleTargetFps);

            AddSpacer(gfxContent, spaceTemplate, "GfxSpace2");
            _renderScaleText = CreateCyclerRow(gfxContent, labelTemplate, buttonTemplate, "RenderScale", "Resolution Scale:", OnCycleRenderScale);

            AddSpacer(gfxContent, spaceTemplate, "GfxSpace3");
            _shadowsText = CreateCyclerRow(gfxContent, labelTemplate, buttonTemplate, "Shadows", "Shadows:", OnCycleShadows);

            AddSpacer(gfxContent, spaceTemplate, "GfxSpace4");
            _postFxToggle = CreateToggleRow(gfxContent, toggleTemplate, "PostFxToggleRow", "Post-Processing (Bloom/HDR)", OnPostFxToggled);

            AddSpacer(gfxContent, spaceTemplate, "GfxSpace5");
            _msaaText = CreateCyclerRow(gfxContent, labelTemplate, buttonTemplate, "Msaa", "Anti-Aliasing (MSAA):", OnCycleMsaa);

            AddSpacer(gfxContent, spaceTemplate, "GfxSpace6");
            _textureText = CreateCyclerRow(gfxContent, labelTemplate, buttonTemplate, "TextureQuality", "Texture Quality:", OnCycleTextureQuality);

            AddSpacer(gfxContent, spaceTemplate, "GfxSpace7");
            _adaptiveToggle = CreateToggleRow(gfxContent, toggleTemplate, "AdaptiveThermalToggleRow", "Adaptive Thermal Control", OnAdaptiveThermalToggled);

            foreach (GameObject oldChild in toDestroy)
            {
                oldChild.SetActive(false);
                oldChild.transform.SetParent(null, false);
                Destroy(oldChild);
            }

            RebuildContentLayout(mainContent as RectTransform);
            RebuildContentLayout(gfxContent as RectTransform);

            _graphicsScrollView.SetActive(false);
            _built = true;
        }

        private Button EnsureEntryButton(Transform mainContent, Transform spaceTemplate, Transform buttonTemplate)
        {
            Transform existing = mainContent.Find("GraphicsSettingsBtn");
            if (existing != null)
            {
                Button existingBtn = existing.GetComponent<Button>();
                BindButton(existingBtn, OpenGraphicsPanel);
                return existingBtn;
            }

            AddSpacer(mainContent, spaceTemplate, "GraphicsEntrySpace");
            return CreateButtonRow(mainContent, buttonTemplate, "GraphicsSettingsBtn", "Graphics Settings", OpenGraphicsPanel);
        }

        private static void AddSpacer(Transform parent, Transform spaceTemplate, string name)
        {
            if (spaceTemplate == null) return;
            GameObject spacer = Instantiate(spaceTemplate.gameObject, parent);
            spacer.name = name;
        }

        private static Button CreateButtonRow(Transform parent, Transform buttonTemplate, string name, string text, UnityAction onClick)
        {
            GameObject btnObj = Instantiate(buttonTemplate.gameObject, parent);
            btnObj.name = name;
            TextMeshProUGUI tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null) tmp.text = text;
            Button btn = btnObj.GetComponent<Button>();
            BindButton(btn, onClick);
            return btn;
        }

        private static TextMeshProUGUI CreateCyclerRow(
            Transform parent,
            Transform labelTemplate,
            Transform buttonTemplate,
            string key,
            string labelText,
            UnityAction onClick)
        {
            GameObject labelObj = Instantiate(labelTemplate.gameObject, parent);
            labelObj.name = $"{key}Txt";
            TextMeshProUGUI labelTmp = labelObj.GetComponent<TextMeshProUGUI>();
            if (labelTmp != null) labelTmp.text = labelText;

            Button btn = CreateButtonRow(parent, buttonTemplate, $"{key}Btn", string.Empty, onClick);
            return btn != null ? btn.GetComponentInChildren<TextMeshProUGUI>(true) : null;
        }

        private static Toggle CreateToggleRow(Transform parent, Transform toggleTemplate, string rowName, string labelText, UnityAction<bool> onChanged)
        {
            GameObject rowObj = Instantiate(toggleTemplate.gameObject, parent);
            rowObj.name = rowName;
            Toggle toggle = rowObj.GetComponentInChildren<Toggle>(true);
            if (toggle != null)
            {
                TextMeshProUGUI label = toggle.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null) label.text = labelText;
                toggle.onValueChanged = new Toggle.ToggleEvent();
                toggle.onValueChanged.AddListener(onChanged);
            }
            return toggle;
        }

        private static void BindButton(Button btn, UnityAction onClick)
        {
            if (btn == null) return;
            btn.onClick = new Button.ButtonClickedEvent();
            btn.onClick.AddListener(onClick);
        }

        private static void RebuildContentLayout(RectTransform contentRect)
        {
            if (contentRect == null) return;
            VerticalLayoutGroup layoutGroup = contentRect.GetComponent<VerticalLayoutGroup>();
            float totalHeight = layoutGroup != null ? layoutGroup.padding.vertical : 20f;
            float spacing = layoutGroup != null ? layoutGroup.spacing : 10f;
            int activeChildren = 0;

            for (int i = 0; i < contentRect.childCount; i++)
            {
                RectTransform child = contentRect.GetChild(i) as RectTransform;
                if (child != null && child.gameObject.activeSelf)
                {
                    totalHeight += child.sizeDelta.y;
                    activeChildren++;
                }
            }

            if (activeChildren > 1)
                totalHeight += spacing * (activeChildren - 1);

            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, Mathf.Max(contentRect.sizeDelta.y, totalHeight));
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        private void OnSettingsChanged(GraphicsSettingsData _, GraphicsPreset __) => RefreshUI();

        private void RefreshUI()
        {
            if (_service == null) return;

            GraphicsSettingsData data = _service.CurrentSettings;
            GraphicsPreset preset = _service.CurrentPreset;

            if (_presetText != null)
            {
                _presetText.text = preset switch
                {
                    GraphicsPreset.BatterySaver => "Battery Saver",
                    GraphicsPreset.Balanced => "Balanced",
                    GraphicsPreset.High => "High",
                    _ => "Custom"
                };
            }

            if (_fpsText != null)
                _fpsText.text = $"{data.TargetFps} FPS";

            if (_renderScaleText != null)
                _renderScaleText.text = $"{Mathf.RoundToInt(data.RenderScale * 100f)}%";

            if (_shadowsText != null)
                _shadowsText.text = data.Shadows.ToString();

            if (_postFxToggle != null)
                _postFxToggle.SetIsOnWithoutNotify(data.PostProcessingEnabled);

            if (_msaaText != null)
                _msaaText.text = data.Msaa == MsaaMode.Off ? "Off" : $"{(int)data.Msaa}x";

            if (_textureText != null)
                _textureText.text = data.TextureQuality.ToString();

            if (_adaptiveToggle != null)
                _adaptiveToggle.SetIsOnWithoutNotify(data.AdaptiveThermalControl);
        }

        private void OnCyclePreset()
        {
            if (_service == null) return;
            GraphicsPreset next = _service.CurrentPreset switch
            {
                GraphicsPreset.BatterySaver => GraphicsPreset.Balanced,
                GraphicsPreset.Balanced => GraphicsPreset.High,
                _ => GraphicsPreset.BatterySaver
            };
            _service.ApplyPreset(next);
        }

        private void OnCycleTargetFps()
        {
            if (_service == null) return;
            GraphicsSettingsData data = _service.CurrentSettings;
            int idx = Array.IndexOf(GraphicsPresets.FpsOptions, data.TargetFps);
            data.TargetFps = GraphicsPresets.FpsOptions[(idx + 1) % GraphicsPresets.FpsOptions.Length];
            _service.UpdateSettings(data);
        }

        private void OnCycleRenderScale()
        {
            if (_service == null) return;
            GraphicsSettingsData data = _service.CurrentSettings;
            int idx = 0;
            for (int i = 0; i < GraphicsPresets.RenderScaleOptions.Length; i++)
            {
                if (Mathf.Abs(GraphicsPresets.RenderScaleOptions[i] - data.RenderScale) < 0.01f)
                {
                    idx = i;
                    break;
                }
            }
            data.RenderScale = GraphicsPresets.RenderScaleOptions[(idx + 1) % GraphicsPresets.RenderScaleOptions.Length];
            _service.UpdateSettings(data);
        }

        private void OnCycleShadows()
        {
            if (_service == null) return;
            GraphicsSettingsData data = _service.CurrentSettings;
            data.Shadows = data.Shadows switch
            {
                ShadowQualityMode.Off => ShadowQualityMode.Hard,
                ShadowQualityMode.Hard => ShadowQualityMode.Soft,
                _ => ShadowQualityMode.Off
            };
            _service.UpdateSettings(data);
        }

        private void OnPostFxToggled(bool isOn)
        {
            if (_service == null) return;
            GraphicsSettingsData data = _service.CurrentSettings;
            data.PostProcessingEnabled = isOn;
            _service.UpdateSettings(data);
        }

        private void OnCycleMsaa()
        {
            if (_service == null) return;
            GraphicsSettingsData data = _service.CurrentSettings;
            data.Msaa = data.Msaa switch
            {
                MsaaMode.Off => MsaaMode.X2,
                MsaaMode.X2 => MsaaMode.X4,
                _ => MsaaMode.Off
            };
            _service.UpdateSettings(data);
        }

        private void OnCycleTextureQuality()
        {
            if (_service == null) return;
            GraphicsSettingsData data = _service.CurrentSettings;
            data.TextureQuality = data.TextureQuality == TextureQualityMode.Half
                ? TextureQualityMode.Full
                : TextureQualityMode.Half;
            _service.UpdateSettings(data);
        }

        private void OnAdaptiveThermalToggled(bool isOn)
        {
            if (_service == null) return;
            GraphicsSettingsData data = _service.CurrentSettings;
            data.AdaptiveThermalControl = isOn;
            _service.UpdateSettings(data);
        }

        private static void SelectIfGamepad(GameObject target)
        {
            if (target == null) return;
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null) return;
            var pi = UnityEngine.InputSystem.PlayerInput.GetPlayerByIndex(0);
            if (pi != null && pi.currentControlScheme == "Gamepad")
                es.SetSelectedGameObject(target);
        }
    }
}
