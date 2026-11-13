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
        [SerializeField] private Toggle invertCamXToggle;
        [SerializeField] private Toggle invertCamYToggle;
        [SerializeField] private Toggle godModeToggle;
        
        private void Start()
        {
            rotationSensitivitySlider.value = GameSave.RotationSensitivity;
            if (InputListener.ActionMaps != null && InputListener.ActionMaps.Count > 0)
            {
                if (GameSave.ControlScheme < 0 || GameSave.ControlScheme >= InputListener.ActionMaps.Count)
                    GameSave.ControlScheme = 0;
                controlSchemeText.text = InputListener.ActionMaps[GameSave.ControlScheme];
            }
            invertCamXToggle.isOn = GameSave.InvertCamX;
            invertCamYToggle.isOn = GameSave.InvertCamY;
            godModeToggle.isOn = GameSave.GodMode;
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
