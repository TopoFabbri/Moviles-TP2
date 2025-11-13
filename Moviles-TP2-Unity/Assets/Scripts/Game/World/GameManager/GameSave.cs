using System;
using System.Collections.Generic;
using Game.World.UI;
using GooglePlayGames;
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
        private static int _godMode = -1;
        private static readonly Dictionary<int, float> LevelTimes = new();
        private static readonly Dictionary<int, float> LevelTimesGodMode = new();

        private const string NoGooglePlayPlayerId = "defaultPlayer";
        private static string ControlSchemeKey => PlayerId + "ControlScheme";
        private static string RotationSensitivityKey => PlayerId + "RotationSensitivity";
        private static string InvertCamXKey => PlayerId + "InvertCamX";
        private static string InvertCamYKey => PlayerId + "InvertCamY";
        private static string CurrentLevelKey => PlayerId + "CurrentLevel";
        private static string CreditsKey => PlayerId + "Credits";
        private static string AddedSpeedKey => PlayerId + "AddedSpeed";
        private static string GodModeKey => PlayerId + "GodMode";
        private static string LevelTimesKey => PlayerId + "Level<level>Time";

        private static string _playerId;

        private static string PlayerId
        {
            get
            {
                _playerId ??= PlayGamesPlatform.Instance.IsAuthenticated() ? PlayGamesPlatform.Instance.GetUserId() : NoGooglePlayPlayerId;
                _playerId ??= NoGooglePlayPlayerId;

                return _playerId;
            }
        }
        
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
        
        public static bool GodMode
        {
            get
            {
                if (_godMode < 0)
                    _godMode = PlayerPrefs.GetInt(GodModeKey, -1);
                
                if (_godMode < 0)
                    GodMode = false;
                
                return _godMode > 0;
            }
            set
            {
                _godMode = value ? 1 : 0;
                
                PlayerPrefs.SetInt(GodModeKey, _godMode);
                PlayerPrefs.Save();
            }
        }

        public static void SetLevelTime(int level, float time)
        {
            LevelTimes.TryAdd(level, time);
            LevelTimes[level] = time;
            
            string key = LevelTimesKey.Replace("<level>", level.ToString()) + (GodMode ? "GodMode" : "");
            PlayerPrefs.SetFloat(key, time);
            PlayerPrefs.Save();
        }
        
        public static float GetLevelTime(int level)
        {
            if (!LevelTimes.ContainsKey(level))
                LevelTimes.TryAdd(level, -1f);
            
            string key = LevelTimesKey.Replace("<level>", level.ToString()) + (GodMode ? "GodMode" : "");
            LevelTimes[level] = PlayerPrefs.GetFloat(key, -1f);
            
            return LevelTimes[level];
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
            LevelTimes.Clear();
            
            PlayerPrefs.DeleteAll();
        }

        private static List<byte> Serialize()
        {
            List<byte> bytes = new();
            
            bytes.AddRange(BitConverter.GetBytes(ControlScheme));
            bytes.AddRange(BitConverter.GetBytes(RotationSensitivity));
            bytes.AddRange(BitConverter.GetBytes(InvertCamX));
            bytes.AddRange(BitConverter.GetBytes(InvertCamY));
            bytes.AddRange(BitConverter.GetBytes(CurrentLevel));
            bytes.AddRange(BitConverter.GetBytes(Credits));
            bytes.AddRange(BitConverter.GetBytes(AddedSpeed));
            
            bytes.AddRange(BitConverter.GetBytes(LevelList.GetLevels().Count));
            
            for (int i = 0; i < LevelList.GetLevels().Count; i++)
                bytes.AddRange(BitConverter.GetBytes(GetLevelTime(i)));
            
            return bytes;
        }
        
        private static void Deserialize(byte[] bytes)
        {
            int index = 0;
            
            ControlScheme = BitConverter.ToInt32(bytes, index);
            index += sizeof(int);
            
            RotationSensitivity = BitConverter.ToSingle(bytes, index);
            index += sizeof(float);
            
            _invertCamX = BitConverter.ToInt32(bytes, index);
            index += sizeof(int);
            
            _invertCamY = BitConverter.ToInt32(bytes, index);
            index += sizeof(int);
            
            CurrentLevel = BitConverter.ToInt32(bytes, index);
            index += sizeof(int);
            
            Credits = BitConverter.ToInt32(bytes, index);
            index += sizeof(int);
            
            AddedSpeed = BitConverter.ToSingle(bytes, index);
            index += sizeof(float);
            
            int levelsCount = BitConverter.ToInt32(bytes, index);
            index += sizeof(int);
            
            for (int i = 0; i < levelsCount; i++)
                SetLevelTime(i, BitConverter.ToSingle(bytes, index));
            
            index += sizeof(float) * levelsCount;
        }
    }
}
