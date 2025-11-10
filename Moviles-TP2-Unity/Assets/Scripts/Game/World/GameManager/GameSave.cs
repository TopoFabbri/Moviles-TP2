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
        private static int _credits = -1;
        private static float _addedSpeed = -1f;
        
        private const string ControlSchemeKey = "ControlScheme";
        private const string RotationSensitivityKey = "RotationSensitivity";
        private const string InvertCamXKey = "InvertCamX";
        private const string InvertCamYKey = "InvertCamY";
        private const string CurrentLevelKey = "CurrentLevel";
        private const string CreditsKey = "Credits";
        private const string AddedSpeedKey = "AddedSpeed";

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

        public static int Credits
        {
            get
            { 
                if (_credits < 0)
                    _credits = PlayerPrefs.GetInt(CreditsKey, -1);
                
                if (_credits < 0)
                    Credits = 0;
                
                return _credits;
            }
            set
            {
                _credits = value;
                
                PlayerPrefs.SetInt(CreditsKey, _credits);
                PlayerPrefs.Save();
            }
        }

        public static float AddedSpeed
        {
            get
            {
                if (_addedSpeed <= 0f)
                    _addedSpeed = PlayerPrefs.GetFloat(AddedSpeedKey, 0f);
                
                if (_addedSpeed <= 0f)
                    return 0f;
                
                return _addedSpeed;
            }
            set
            {
                _addedSpeed = value;
                
                PlayerPrefs.SetFloat(AddedSpeedKey, _addedSpeed);
                PlayerPrefs.Save();
            }
        }
        
        public static void Reset()
        {
            _controlScheme = -1;
            _rotationSensitivity = -1f;
            _invertCamX = -1;
            _invertCamY = -1;
            _currentLevel = -1;
            _credits = -1;
            _addedSpeed = -1f;
            
            PlayerPrefs.DeleteAll();
        }
    }
}
