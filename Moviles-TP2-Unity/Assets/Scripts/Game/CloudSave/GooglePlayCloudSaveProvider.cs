using System;
using System.Text;
using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
#endif

namespace Game.CloudSave
{
    public class GooglePlayCloudSaveProvider : ICloudSaveProvider
    {
        private const string SaveFileName = "DroneRacerSave";

        public bool IsAvailable
        {
            get
            {
#if UNITY_ANDROID
                return PlayGamesPlatform.Instance != null && PlayGamesPlatform.Instance.IsAuthenticated();
#else
                return false;
#endif
            }
        }

        public void LoadCloudSave(Action<bool, CloudSaveData> onComplete)
        {
#if UNITY_ANDROID
            if (!IsAvailable)
            {
                onComplete?.Invoke(false, null);
                return;
            }

            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.OpenWithAutomaticConflictResolution(
                SaveFileName,
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                (status, metadata) =>
                {
                    if (status != SavedGameRequestStatus.Success || metadata == null)
                    {
                        Debug.LogWarning($"[CloudSave] Failed to open saved game for load: {status}");
                        onComplete?.Invoke(false, null);
                        return;
                    }

                    savedGameClient.ReadBinaryData(metadata, (readStatus, bytes) =>
                    {
                        if (readStatus != SavedGameRequestStatus.Success)
                        {
                            Debug.LogWarning($"[CloudSave] Failed to read binary data: {readStatus}");
                            onComplete?.Invoke(false, null);
                            return;
                        }

                        if (bytes == null || bytes.Length == 0)
                        {
                            onComplete?.Invoke(true, null);
                            return;
                        }

                        string json = Encoding.UTF8.GetString(bytes);
                        CloudSaveData data = CloudSaveData.FromJson(json);
                        onComplete?.Invoke(true, data);
                    });
                });
#else
            onComplete?.Invoke(false, null);
#endif
        }

        public void SaveToCloud(CloudSaveData data, Action<bool> onComplete)
        {
#if UNITY_ANDROID
            if (!IsAvailable || data == null)
            {
                onComplete?.Invoke(false);
                return;
            }

            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.OpenWithAutomaticConflictResolution(
                SaveFileName,
                DataSource.ReadCacheOrNetwork,
                ConflictResolutionStrategy.UseLongestPlaytime,
                (status, metadata) =>
                {
                    if (status != SavedGameRequestStatus.Success || metadata == null)
                    {
                        Debug.LogWarning($"[CloudSave] Failed to open saved game for save: {status}");
                        onComplete?.Invoke(false);
                        return;
                    }

                    byte[] bytes = Encoding.UTF8.GetBytes(data.ToJson());
                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription($"Level {data.currentLevel} | Credits {data.credits}")
                        .Build();

                    savedGameClient.CommitUpdate(metadata, update, bytes, (commitStatus, _) =>
                    {
                        bool success = commitStatus == SavedGameRequestStatus.Success;
                        if (!success)
                            Debug.LogWarning($"[CloudSave] Failed to commit cloud save: {commitStatus}");
                        onComplete?.Invoke(success);
                    });
                });
#else
            onComplete?.Invoke(false);
#endif
        }
    }
}
