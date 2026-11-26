using Game.Input;
using Game.Audio;
using Game.World.GameManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World.UI
{
    internal sealed class GameplayMenuController : MonoBehaviour
    {
        [SerializeField] private Slider rotationSensitivitySlider;
        [SerializeField] private TextMeshProUGUI controlSchemeText;
        [SerializeField] private TextMeshProUGUI gyroModeText;
        [SerializeField] private Toggle invertCamXToggle;
        [SerializeField] private Toggle invertCamYToggle;
        [SerializeField] private Toggle godModeToggle;
        
        [SerializeField] private Slider _musicVolSlider;
        [SerializeField] private Slider _sfxVolSlider;

        private void Awake()
        {
            EnsureGyroModeControl();
            EnsureVolumeControls();
        }

        private void Start()
        {
            EnsureGyroModeControl();
            EnsureVolumeControls();
            WireBakedControlListeners();
            RefreshUI();
        }

        private void WireBakedControlListeners()
        {
            if (gyroModeText != null)
            {
                Button gyroBtn = gyroModeText.GetComponentInParent<Button>(true);
                if (gyroBtn != null)
                {
                    gyroBtn.onClick.RemoveListener(OnGyroModeChanged);
                    gyroBtn.onClick.AddListener(OnGyroModeChanged);
                }
            }

            if (_musicVolSlider != null)
            {
                _musicVolSlider.SetValueWithoutNotify(GameSave.MusicVolume);
                _musicVolSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
                _musicVolSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (_sfxVolSlider != null)
            {
                _sfxVolSlider.SetValueWithoutNotify(GameSave.SfxVolume);
                _sfxVolSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
                _sfxVolSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            }
        }

        private static void OnMusicVolumeChanged(float v)
        {
            GameSave.MusicVolume = v;
            GameAudioService.SetMusicVolume(v);
        }

        private static void OnSfxVolumeChanged(float v)
        {
            GameSave.SfxVolume = v;
            GameAudioService.SetSfxVolume(v);
        }

        private void OnEnable()
        {
            GameSave.OnSaveDataChanged += RefreshUI;
            RefreshUI();
        }

        private void OnDisable()
        {
            GameSave.OnSaveDataChanged -= RefreshUI;
        }

        public void RefreshUI()
        {
            if (rotationSensitivitySlider != null)
                rotationSensitivitySlider.SetValueWithoutNotify(GameSave.RotationSensitivity);
            if (controlSchemeText != null && InputListener.ActionMaps != null && InputListener.ActionMaps.Count > 0)
            {
                if (GameSave.ControlScheme < 0 || GameSave.ControlScheme >= InputListener.ActionMaps.Count)
                    GameSave.ControlScheme = 0;
                controlSchemeText.text = InputListener.ActionMaps[GameSave.ControlScheme];
            }

            UpdateGyroModeText();

            if (invertCamXToggle != null)
                invertCamXToggle.SetIsOnWithoutNotify(GameSave.InvertCamX);
            if (invertCamYToggle != null)
                invertCamYToggle.SetIsOnWithoutNotify(GameSave.InvertCamY);
            if (godModeToggle != null)
                godModeToggle.SetIsOnWithoutNotify(GameSave.GodMode);
        }

        public void OnControllerSchemeChanged()
        {
            if (InputListener.ActionMaps == null || InputListener.ActionMaps.Count == 0) return;

            if (GameSave.ControlScheme + 1 < InputListener.ActionMaps.Count)
                GameSave.ControlScheme++;
            else
                GameSave.ControlScheme = 0;
            
            controlSchemeText.text = InputListener.ActionMaps[GameSave.ControlScheme];
        }

        public void OnGyroModeChanged()
        {
            GameSave.GyroMode = GameSave.GyroMode == GyroMode.BankToTurn
                ? GyroMode.FpvAcro
                : GyroMode.BankToTurn;

            UpdateGyroModeText();
        }

        private void EnsureVolumeControls()
        {
            if (_musicVolSlider != null && _sfxVolSlider != null)
                return;

            Slider template = GetComponentInChildren<Slider>(true);
            if (template == null)
                return;

            GameAudioService.SetMusicVolume(GameSave.MusicVolume);
            GameAudioService.SetSfxVolume(GameSave.SfxVolume);

            if (_musicVolSlider == null)
            {
                _musicVolSlider = CreateVolumeSlider(template, "MusicVol", GameSave.MusicVolume, val =>
                {
                    GameSave.MusicVolume = val;
                    GameAudioService.SetMusicVolume(val);
                });
            }

            if (_sfxVolSlider == null)
            {
                _sfxVolSlider = CreateVolumeSlider(template, "SFXVol", GameSave.SfxVolume, val =>
                {
                    GameSave.SfxVolume = val;
                    GameAudioService.SetSfxVolume(val);
                });
            }
        }

        private static Slider CreateVolumeSlider(Slider template, string sliderName, float initialValue, UnityEngine.Events.UnityAction<float> onChanged)
        {
            Transform existing = template.transform.parent != null ? template.transform.parent.Find(sliderName) : null;
            Slider slider = existing != null ? existing.GetComponent<Slider>() : Instantiate(template, template.transform.parent);
            slider.gameObject.name = sliderName;
            slider.onValueChanged = new Slider.SliderEvent();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.SetValueWithoutNotify(initialValue);
            slider.onValueChanged.AddListener(onChanged);
            return slider;
        }

        private void EnsureGyroModeControl()
        {
            if (gyroModeText != null || controlSchemeText == null)
                return;

            Transform btnTransform = controlSchemeText.transform.parent;
            Transform contentTransform = btnTransform != null ? btnTransform.parent : null;
            if (btnTransform == null || contentTransform == null)
                return;

            Transform existingBtnTransform = contentTransform.Find("GyroModeBtn");
            if (existingBtnTransform != null)
            {
                gyroModeText = existingBtnTransform.GetComponentInChildren<TextMeshProUGUI>();
                UnityEngine.UI.Button existingBtn = existingBtnTransform.GetComponent<UnityEngine.UI.Button>();
                if (existingBtn != null)
                {
                    existingBtn.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                    existingBtn.onClick.AddListener(OnGyroModeChanged);
                }
                return;
            }

            VerticalLayoutGroup layoutGroup = contentTransform.GetComponent<VerticalLayoutGroup>();
            int btnIndex = btnTransform.GetSiblingIndex();
            Transform labelTransform = null;
            Transform spaceTransform = null;

            for (int i = 0; i < contentTransform.childCount; i++)
            {
                Transform child = contentTransform.GetChild(i);
                if (child.name == "ControlSchemeTxt")
                    labelTransform = child;
                else if (child.name.StartsWith("Space") && spaceTransform == null)
                    spaceTransform = child;
            }

            int insertIndex = btnIndex + 1;

            if (spaceTransform != null)
            {
                GameObject gyroSpace = Instantiate(spaceTransform.gameObject, contentTransform);
                gyroSpace.name = "GyroModeSpace";
                gyroSpace.transform.SetSiblingIndex(insertIndex++);
            }

            if (labelTransform != null)
            {
                GameObject gyroLabel = Instantiate(labelTransform.gameObject, contentTransform);
                gyroLabel.name = "GyroModeTxt";
                gyroLabel.transform.SetSiblingIndex(insertIndex++);
                TextMeshProUGUI labelTmp = gyroLabel.GetComponent<TextMeshProUGUI>();
                if (labelTmp != null)
                    labelTmp.text = "Gyro Mode:";
            }

            GameObject gyroBtnObj = Instantiate(btnTransform.gameObject, contentTransform);
            gyroBtnObj.name = "GyroModeBtn";
            gyroBtnObj.transform.SetSiblingIndex(insertIndex);

            UnityEngine.UI.Button gyroBtn = gyroBtnObj.GetComponent<UnityEngine.UI.Button>();
            if (gyroBtn != null)
            {
                gyroBtn.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                gyroBtn.onClick.AddListener(OnGyroModeChanged);
                gyroModeText = gyroBtnObj.GetComponentInChildren<TextMeshProUGUI>();
            }

            RectTransform contentRect = contentTransform as RectTransform;
            if (contentRect != null)
            {
                float totalHeight = layoutGroup != null ? layoutGroup.padding.vertical : 20f;
                float spacing = layoutGroup != null ? layoutGroup.spacing : 10f;
                int activeChildren = 0;
                for (int i = 0; i < contentRect.childCount; i++)
                {
                    RectTransform childRect = contentRect.GetChild(i) as RectTransform;
                    if (childRect != null && childRect.gameObject.activeSelf)
                    {
                        totalHeight += childRect.sizeDelta.y;
                        activeChildren++;
                    }
                }
                if (activeChildren > 1)
                    totalHeight += spacing * (activeChildren - 1);

                if (contentRect.sizeDelta.y < totalHeight)
                    contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);

                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            }
        }

        private void UpdateGyroModeText()
        {
            if (gyroModeText == null)
                return;

            gyroModeText.text = GameSave.GyroMode == GyroMode.BankToTurn
                ? "Bank-to-Turn"
                : "FPV Acro";
        }

        public void OnRotationSensitivityChanged(float value)
        {
            if (!gameObject.activeInHierarchy)
                return;
            GameSave.RotationSensitivity = value;
        }
        
        public void OnInvertCamXChanged(bool value)
        {
            GameSave.InvertCamX = value;
        }
        
        public void OnInvertCamYChanged(bool value)
        {
            GameSave.InvertCamY = value;
        }
        
        public void OnGodModeChanged(bool value)
        {
            GameSave.GodMode = value;
        }
    }
}
