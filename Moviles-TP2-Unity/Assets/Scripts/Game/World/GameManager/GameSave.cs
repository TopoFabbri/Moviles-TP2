using Tools.EventSystem;
using UnityEngine;

namespace Game.World.GameManager
{
    public static class GameSave
    {
        private static int _controlScheme = -1;
        private static float _rotationSensitivity = -1f;
        
        private const string ControlSchemeKey = "ControlScheme";
        private const string RotationSensitivityKey = "RotationSensitivity";
        
        public static int ControlScheme
        {
            get
            {
                if (_controlScheme < 0)
                    PlayerPrefs.GetInt(ControlSchemeKey, -1);
                
                if (_controlScheme < 0)
                    return -1;
                
                return _controlScheme;
            }
            set
            {
                _controlScheme = value;
                
                PlayerPrefs.SetInt(ControlSchemeKey, value);
                
                EventSystem.Raise<ControlSchemeChangedEvent>(value);
            }
        }
        
        public static float RotationSensitivity
        {
            get
            {
                if (_rotationSensitivity <= 0f)
                    PlayerPrefs.GetFloat(RotationSensitivityKey, 0f);
                
                if (_rotationSensitivity <= 0f)
                    return 0f;
                
                return _rotationSensitivity;
            }
            
            set
            {
                _rotationSensitivity = value;
                
                PlayerPrefs.SetFloat(RotationSensitivityKey, value);
            }
        }
    }
}
