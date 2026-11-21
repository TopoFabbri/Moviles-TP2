using Game.Input;
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
        
        private void Awake()
        {
            EnsureGyroModeControl();
        }

        private void Start()
        {
            EnsureGyroModeControl();
            RefreshUI();
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
