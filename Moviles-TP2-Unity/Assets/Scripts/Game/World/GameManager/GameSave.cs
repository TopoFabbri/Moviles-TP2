using Tools.EventSystem;
using UnityEngine;

namespace Game.World.GameManager
{
    public static class GameSave
    {
        private static int _controlScheme = -1;
        private static float _rotationSensitivity = -1f;
        private static int _invertCamX = -1;
        private static int _invertCamY = -1;
        private static int _currentLevel = -1;
        
        private const string ControlSchemeKey = "ControlScheme";
        private const string RotationSensitivityKey = "RotationSensitivity";
        private const string InvertCamXKey = "InvertCamX";
        private const string InvertCamYKey = "InvertCamY";
        private const string CurrentLevelKey = "CurrentLevel";

        public static int ControlScheme
        {
            get
            {
                if (_controlScheme < 0)
                    _controlScheme = PlayerPrefs.GetInt(ControlSchemeKey, -1);
                
                if (_controlScheme < 0)
                    return -1;
                
                return _controlScheme;
            }
            set
            {
                _controlScheme = value;
                
                PlayerPrefs.SetInt(ControlSchemeKey, _controlScheme);
                PlayerPrefs.Save();
                
                EventSystem.Raise<ControlSchemeChangedEvent>(_controlScheme);
            }
        }
        
        public static float RotationSensitivity
        {
            get
            {
                if (_rotationSensitivity <= 0f)
                    _rotationSensitivity = PlayerPrefs.GetFloat(RotationSensitivityKey, 0f);
                
                if (_rotationSensitivity <= 0f)
                    return 0f;
                
                return _rotationSensitivity;
            }
            
            set
            {
                _rotationSensitivity = value;
                
                PlayerPrefs.SetFloat(RotationSensitivityKey, _rotationSensitivity);
                PlayerPrefs.Save();
            }
        }
        
        public static bool InvertCamX
        {
            get
            {
                if (_invertCamX < 0)
                    _invertCamX = PlayerPrefs.GetInt(InvertCamXKey, -1);
                
                if (_invertCamX < 0)
                    InvertCamX = false;
                
                return _invertCamX > 0;
            }
            set
            {
                _invertCamX = value ? 1 : 0;
                
                PlayerPrefs.SetInt(InvertCamXKey, _invertCamX);
                PlayerPrefs.Save();
            }
        }
        
        public static bool InvertCamY
        {
            get
            {
                if (_invertCamY < 0)
                    _invertCamY = PlayerPrefs.GetInt(InvertCamYKey, -1);
                
                if (_invertCamY < 0)
                    InvertCamY = true;
                
                return _invertCamY > 0;
            }
            set
            {
                _invertCamY = value ? 1 : 0;
                
                PlayerPrefs.SetInt(InvertCamYKey, _invertCamY);
                PlayerPrefs.Save();
            }
        }

        public static int CurrentLevel
        {
            get
            {
                if (_currentLevel < 0)
                    _currentLevel = PlayerPrefs.GetInt(CurrentLevelKey, -1);
                
                if (_currentLevel < 0)
                    CurrentLevel = 0;
                
                return _currentLevel;
            }
            set
            {
                _currentLevel = value;
                
                PlayerPrefs.SetInt(CurrentLevelKey, _currentLevel);
                PlayerPrefs.Save();
                
                EventSystem.Raise<ControlSchemeChangedEvent>(_currentLevel);
            }
        }
    }
}
