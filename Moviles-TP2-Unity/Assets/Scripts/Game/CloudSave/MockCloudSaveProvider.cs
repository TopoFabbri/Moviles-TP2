using System;
using System.IO;
using Game.World.GameManager;
using UnityEngine;

namespace Game.CloudSave
{
    public class MockCloudSaveProvider : ICloudSaveProvider
    {
        private static string MockFilePath
        {
            get
            {
                string safeId = string.IsNullOrEmpty(GameSave.ActivePlayerId) ? "defaultPlayer" : GameSave.ActivePlayerId;
                foreach (char c in Path.GetInvalidFileNameChars())
                    safeId = safeId.Replace(c, '_');
                return Path.Combine(Application.persistentDataPath, $"mock_cloud_save_{safeId}.json");
            }
        }

        public bool IsAvailable => true;

        public void LoadCloudSave(Action<bool, CloudSaveData> onComplete)
        {
            if (!File.Exists(MockFilePath))
            {
                onComplete?.Invoke(true, null);
                return;
            }

            try
            {
                string json = File.ReadAllText(MockFilePath);
                CloudSaveData data = CloudSaveData.FromJson(json);
                onComplete?.Invoke(true, data);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MockCloudSave] Failed to read mock cloud save: {e.Message}");
                onComplete?.Invoke(false, null);
            }
        }

        public void SaveToCloud(CloudSaveData data, Action<bool> onComplete)
        {
            if (data == null)
            {
                onComplete?.Invoke(false);
                return;
            }

            try
            {
                File.WriteAllText(MockFilePath, data.ToJson());
                onComplete?.Invoke(true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MockCloudSave] Failed to write mock cloud save: {e.Message}");
                onComplete?.Invoke(false);
            }
        }

        public static void SeedMockCloudSave(CloudSaveData data)
        {
            if (data == null)
                return;

            File.WriteAllText(MockFilePath, data.ToJson());
        }

        public static void ClearMockCloudSave()
        {
            if (File.Exists(MockFilePath))
                File.Delete(MockFilePath);
        }
    }
}
