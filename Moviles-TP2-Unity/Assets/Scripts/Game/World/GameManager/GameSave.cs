using System;
using System.Collections.Generic;
using Game.CloudSave;
using Game.Events;
using Game.World.UI;
using GooglePlayGames;
using Tools.EventSystem;
using Tools.ServiceProvider;
using UnityEngine;

namespace Game.World.GameManager
{
    public enum GyroMode
    {
        BankToTurn = 0,
        FpvAcro = 1
    }

    public static class GameSave
    {
        private static int _controlScheme = -1;
        private static int _gyroMode = -1;
        private static float _rotationSensitivity = -1f;
        private static int _invertCamX = -1;
        private static int _invertCamY = -1;
        private static int _currentLevel = -1;
        private static int _credits = -1;
        private static float _addedSpeed = -1f;
        private static float _addedControl = -1f;
        private static int _godMode = -1;
        private static int _hasNoAds = -1;
        private static int _adsEnabled = -1;
        private static int _analyticsEnabled = -1;
        private static readonly Dictionary<int, float> LevelTimes = new();
        private static readonly Dictionary<int, float> LevelTimesGodMode = new();

        private const string NoGooglePlayPlayerId = "defaultPlayer";
        public const float DefaultRotationSensitivity = 100f;

        private static string CloudDecisionKey => PlayerId + "CloudSaveDecisionMade";
        private static string LastSavedUtcTicksKey => PlayerId + "LastSavedUtcTicks";
        private static string ControlSchemeKey => PlayerId + "ControlScheme";
        private static string GyroModeKey => PlayerId + "GyroMode";
        private static string RotationSensitivityKey => PlayerId + "RotationSensitivity";
        private static string InvertCamXKey => PlayerId + "InvertCamX";
        private static string InvertCamYKey => PlayerId + "InvertCamY";
        private static string MusicVolumeKey => PlayerId + "MusicVolume";
        private static string SfxVolumeKey => PlayerId + "SfxVolume";
        private static string CurrentLevelKey => PlayerId + "CurrentLevel";
        private static string CreditsKey => PlayerId + "Credits";
        private static string AddedSpeedKey => PlayerId + "AddedSpeed";
        private static string AddedControlKey => PlayerId + "AddedControl";
        private static string GodModeKey => PlayerId + "GodMode";
        private static string HasNoAdsKey => PlayerId + "HasNoAds";
        private static string AdsEnabledKey => PlayerId + "AdsEnabled";
        private static string AnalyticsEnabledKey => PlayerId + "AnalyticsEnabled";
        private static string LevelTimesKey => PlayerId + "Level<level>Time";

        private static string _activePlayerId = NoGooglePlayPlayerId;

        public static string ActivePlayerId => _activePlayerId;

        private static string PlayerId => _activePlayerId;

        public static event Action OnSaveDataChanged;

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f);
            set
            {
                PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
                PlayerPrefs.Save();
            }
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(SfxVolumeKey, 1.0f);
            set
            {
                PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
                PlayerPrefs.Save();
            }
        }

        public static bool SetActiveAccount(string playerId)
        {
            string normalized = string.IsNullOrEmpty(playerId) || playerId == "0"
                ? NoGooglePlayPlayerId
                : playerId;

            if (string.Equals(_activePlayerId, normalized, StringComparison.Ordinal))
                return false;

            _activePlayerId = normalized;
            InvalidateCache();
            NotifySaveDataChanged();
            return true;
        }

        private static void InvalidateCache()
        {
            _controlScheme = -1;
            _gyroMode = -1;
            _rotationSensitivity = -1f;
            _invertCamX = -1;
            _invertCamY = -1;
            _currentLevel = -1;
            _credits = -1;
            _addedSpeed = -1f;
            _addedControl = -1f;
            _godMode = -1;
            _hasNoAds = -1;
            _adsEnabled = -1;
            LevelTimes.Clear();
            LevelTimesGodMode.Clear();
        }

        private static void NotifySaveDataChanged()
        {
            OnSaveDataChanged?.Invoke();

            if (ServiceProvider.TryGetService<EventSystem>(out var eventSystem))
            {
                eventSystem.Raise<SaveDataChangedEvent>();
                eventSystem.Raise<ControlSchemeChangedEvent>(ControlScheme);
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
                
                ServiceProvider.GetService<EventSystem>().Raise<ControlSchemeChangedEvent>(_controlScheme);
            }
        }

        public static GyroMode GyroMode
        {
            get
            {
                if (_gyroMode < 0)
                    _gyroMode = PlayerPrefs.GetInt(GyroModeKey, (int)GyroMode.BankToTurn);

                if (_gyroMode != (int)GyroMode.BankToTurn && _gyroMode != (int)GyroMode.FpvAcro)
                    _gyroMode = (int)GyroMode.BankToTurn;

                return (GyroMode)_gyroMode;
            }
            set
            {
                _gyroMode = (int)value;

                PlayerPrefs.SetInt(GyroModeKey, _gyroMode);
                PlayerPrefs.Save();
            }
        }
        
        public static float RotationSensitivity
        {
            get
            {
                if (_rotationSensitivity <= 0f)
                    _rotationSensitivity = PlayerPrefs.GetFloat(RotationSensitivityKey, DefaultRotationSensitivity);

                if (_rotationSensitivity <= 0f)
                    _rotationSensitivity = DefaultRotationSensitivity;

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

        public static bool HasNoAds
        {
            get
            {
                if (_hasNoAds < 0)
                    _hasNoAds = PlayerPrefs.GetInt(HasNoAdsKey, -1);

                if (_hasNoAds < 0)
                    HasNoAds = false;

                return _hasNoAds > 0;
            }
            set
            {
                _hasNoAds = value ? 1 : 0;

                PlayerPrefs.SetInt(HasNoAdsKey, _hasNoAds);
                PlayerPrefs.Save();
            }
        }

        public static bool AdsEnabled
        {
            get
            {
                if (_adsEnabled < 0)
                    _adsEnabled = PlayerPrefs.GetInt(AdsEnabledKey, -1);

                if (_adsEnabled < 0)
                    AdsEnabled = true;

                return _adsEnabled > 0;
            }
            set
            {
                _adsEnabled = value ? 1 : 0;

                PlayerPrefs.SetInt(AdsEnabledKey, _adsEnabled);
                PlayerPrefs.Save();
            }
        }

        public static bool AnalyticsEnabled
        {
            get
            {
                if (_analyticsEnabled < 0)
                    _analyticsEnabled = PlayerPrefs.GetInt(AnalyticsEnabledKey, -1);

                if (_analyticsEnabled < 0)
                    AnalyticsEnabled = true;

                return _analyticsEnabled > 0;
            }
            set
            {
                _analyticsEnabled = value ? 1 : 0;

                PlayerPrefs.SetInt(AnalyticsEnabledKey, _analyticsEnabled);
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

        public static float AddedControl
        {
            get
            {
                if (_addedControl < 0f)
                    _addedControl = Mathf.Clamp01(PlayerPrefs.GetFloat(AddedControlKey, 0f));

                return _addedControl;
            }
            set
            {
                _addedControl = Mathf.Clamp01(value);

                PlayerPrefs.SetFloat(AddedControlKey, _addedControl);
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
            Dictionary<int, float> cache = GodMode ? LevelTimesGodMode : LevelTimes;
            cache[level] = time;

            string key = LevelTimesKey.Replace("<level>", level.ToString()) + (GodMode ? "GodMode" : "");
            PlayerPrefs.SetFloat(key, time);
            PlayerPrefs.Save();
        }

        public static float GetLevelTime(int level)
        {
            Dictionary<int, float> cache = GodMode ? LevelTimesGodMode : LevelTimes;
            if (cache.TryGetValue(level, out float cachedTime))
                return cachedTime;

            string key = LevelTimesKey.Replace("<level>", level.ToString()) + (GodMode ? "GodMode" : "");
            float time = PlayerPrefs.GetFloat(key, -1f);
            cache[level] = time;
            return time;
        }

        public static long LastSavedUtcTicks
        {
            get
            {
                string raw = PlayerPrefs.GetString(LastSavedUtcTicksKey, "0");
                return long.TryParse(raw, out long ticks) ? ticks : 0L;
            }
            set
            {
                PlayerPrefs.SetString(LastSavedUtcTicksKey, value.ToString());
                PlayerPrefs.Save();
            }
        }

        public static bool HasCloudDecisionBeenMade
        {
            get => PlayerPrefs.GetInt(CloudDecisionKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(CloudDecisionKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static CloudSaveData ExportSnapshot()
        {
            CloudSaveData data = new()
            {
                controlScheme = ControlScheme,
                gyroMode = (int)GyroMode,
                rotationSensitivity = RotationSensitivity,
                invertCamX = InvertCamX,
                invertCamY = InvertCamY,
                currentLevel = CurrentLevel,
                credits = Credits,
                addedSpeed = AddedSpeed,
                addedControl = AddedControl,
                godMode = GodMode,
                hasNoAds = HasNoAds,
                adsEnabled = AdsEnabled,
                savedAtUtcTicks = DateTime.UtcNow.Ticks
            };

            LastSavedUtcTicks = data.savedAtUtcTicks;

            LevelList levelList = ServiceProvider.GetService<LevelList>();
            int levelsCount = levelList != null ? levelList.GetLevels().Count : 0;

            for (int i = 0; i < levelsCount; i++)
            {
                string normalKey = LevelTimesKey.Replace("<level>", i.ToString());
                float normalTime = PlayerPrefs.GetFloat(normalKey, -1f);
                if (normalTime >= 0f)
                    data.levelTimes.Add(new LevelTimeEntry { level = i, time = normalTime });

                string godKey = normalKey + "GodMode";
                float godTime = PlayerPrefs.GetFloat(godKey, -1f);
                if (godTime >= 0f)
                    data.levelTimesGodMode.Add(new LevelTimeEntry { level = i, time = godTime });
            }

            return data;
        }

        public static void ImportSnapshot(CloudSaveData data)
        {
            if (data == null)
                return;

            ControlScheme = data.controlScheme;
            GyroMode = (GyroMode)data.gyroMode;
            RotationSensitivity = data.rotationSensitivity;
            InvertCamX = data.invertCamX;
            InvertCamY = data.invertCamY;
            CurrentLevel = data.currentLevel;
            Credits = data.credits;
            AddedSpeed = data.addedSpeed;
            AddedControl = data.addedControl;
            GodMode = data.godMode;
            HasNoAds = data.hasNoAds;
            AdsEnabled = data.adsEnabled;

            LevelTimes.Clear();
            LevelTimesGodMode.Clear();

            if (data.levelTimes != null)
            {
                foreach (LevelTimeEntry entry in data.levelTimes)
                {
                    LevelTimes[entry.level] = entry.time;
                    string key = LevelTimesKey.Replace("<level>", entry.level.ToString());
                    PlayerPrefs.SetFloat(key, entry.time);
                }
            }

            if (data.levelTimesGodMode != null)
            {
                foreach (LevelTimeEntry entry in data.levelTimesGodMode)
                {
                    LevelTimesGodMode[entry.level] = entry.time;
                    string key = LevelTimesKey.Replace("<level>", entry.level.ToString()) + "GodMode";
                    PlayerPrefs.SetFloat(key, entry.time);
                }
            }

            LastSavedUtcTicks = data.savedAtUtcTicks > 0 ? data.savedAtUtcTicks : DateTime.UtcNow.Ticks;
            PlayerPrefs.Save();
            NotifySaveDataChanged();
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(ControlSchemeKey);
            PlayerPrefs.DeleteKey(GyroModeKey);
            PlayerPrefs.DeleteKey(RotationSensitivityKey);
            PlayerPrefs.DeleteKey(InvertCamXKey);
            PlayerPrefs.DeleteKey(InvertCamYKey);
            PlayerPrefs.DeleteKey(CurrentLevelKey);
            PlayerPrefs.DeleteKey(CreditsKey);
            PlayerPrefs.DeleteKey(AddedSpeedKey);
            PlayerPrefs.DeleteKey(AddedControlKey);
            PlayerPrefs.DeleteKey(GodModeKey);
            PlayerPrefs.DeleteKey(HasNoAdsKey);
            PlayerPrefs.DeleteKey(AdsEnabledKey);
            PlayerPrefs.DeleteKey(CloudDecisionKey);
            PlayerPrefs.DeleteKey(LastSavedUtcTicksKey);

            int levelsCount = 0;
            if (ServiceProvider.TryGetService<LevelList>(out var levelList))
                levelsCount = levelList.GetLevels().Count;

            int maxLevelKeys = Math.Max(levelsCount, 32);
            for (int i = 0; i < maxLevelKeys; i++)
            {
                string normalKey = LevelTimesKey.Replace("<level>", i.ToString());
                PlayerPrefs.DeleteKey(normalKey);
                PlayerPrefs.DeleteKey(normalKey + "GodMode");
            }

            InvalidateCache();
            PlayerPrefs.Save();
            NotifySaveDataChanged();
        }
    }
}
