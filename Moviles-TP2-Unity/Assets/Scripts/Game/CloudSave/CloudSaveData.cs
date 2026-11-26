using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.CloudSave
{
    [Serializable]
    public class LevelTimeEntry
    {
        public int level;
        public float time;
    }

    [Serializable]
    public class CloudSaveData
    {
        public int controlScheme;
        public int gyroMode;
        public float rotationSensitivity;
        public bool invertCamX;
        public bool invertCamY;
        public int currentLevel;
        public int credits;
        public float addedSpeed;
        public float addedControl;
        public bool godMode;
        public bool hasNoAds;
        public bool adsEnabled = true;
        public List<LevelTimeEntry> levelTimes = new();
        public List<LevelTimeEntry> levelTimesGodMode = new();
        public long savedAtUtcTicks;

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static CloudSaveData FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonUtility.FromJson<CloudSaveData>(json);
        }
    }
}
