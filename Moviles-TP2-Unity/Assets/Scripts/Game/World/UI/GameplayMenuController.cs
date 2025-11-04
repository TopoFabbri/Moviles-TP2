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
        
        private void Start()
        {
            rotationSensitivitySlider.value = GameSave.RotationSensitivity;
            controlSchemeText.text = InputListener.Schemes[GameSave.ControlScheme];
        }

        public void OnControllerSchemeChanged()
        {
            if (GameSave.ControlScheme + 1 < InputListener.Schemes.Count)
                GameSave.ControlScheme++;
            else
                GameSave.ControlScheme = 0;
            
            controlSchemeText.text = InputListener.Schemes[GameSave.ControlScheme];
        }

        public void OnRotationSensitivityChanged(float value)
        {
            GameSave.RotationSensitivity = value;
        }
    }
}
