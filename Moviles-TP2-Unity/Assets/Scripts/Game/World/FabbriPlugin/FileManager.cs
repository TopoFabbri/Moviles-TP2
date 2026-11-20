using System;
using System.IO;
using Game.Events;
using Game.World.UI;
using Tools.EventSystem;
using Tools.ServiceProvider;
using UnityEngine;

namespace Game.World.FabbriPlugin
{
    public class FileManager : IService
    {
        private const string PackName = "com.fabbri2025.topologger";
        private const string LoggerClassName = "FileManager";
        private const int MaxLogBytes = 64 * 1024;
        private const string FallbackFileName = "logs.txt";

        private AndroidJavaClass fileManager = null;
        private AndroidJavaObject fileManagerInstance = null;

        private string FallbackFilePath => Path.Combine(Application.persistentDataPath, FallbackFileName);

        public FileManager()
        {
            Init();
        }

        public string ReadFile()
        {
            if (fileManagerInstance != null)
            {
                return fileManagerInstance.Call<string>("ReadFile") ?? string.Empty;
            }

            if (File.Exists(FallbackFilePath))
            {
                return File.ReadAllText(FallbackFilePath);
            }

            return string.Empty;
        }

        public void WriteFile(string data)
        {
            if (string.IsNullOrEmpty(data)) return;

            if (fileManagerInstance != null)
            {
                string existing = fileManagerInstance.Call<string>("ReadFile") ?? string.Empty;
                if (existing.Length + data.Length > MaxLogBytes)
                {
                    fileManagerInstance.Call("DeleteFiles");
                    string trimmed = TrimToTail(existing, MaxLogBytes / 2);
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        fileManagerInstance.Call("WriteFile", trimmed);
                    }
                }

                fileManagerInstance.Call("WriteFile", data + "\n");
                return;
            }

            try
            {
                if (File.Exists(FallbackFilePath))
                {
                    var info = new FileInfo(FallbackFilePath);
                    if (info.Length + data.Length > MaxLogBytes)
                    {
                        string existing = File.ReadAllText(FallbackFilePath);
                        File.WriteAllText(FallbackFilePath, TrimToTail(existing, MaxLogBytes / 2));
                    }
                }

                File.AppendAllText(FallbackFilePath, data + "\n");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FileManager] Failed to write fallback log: {ex.Message}");
            }
        }

        public void ClearFileImmediate()
        {
            if (fileManagerInstance != null)
            {
                fileManagerInstance.Call("DeleteFiles");
            }
            else if (File.Exists(FallbackFilePath))
            {
                File.Delete(FallbackFilePath);
            }
        }

        public void DeleteFile()
        {
            ServiceProvider.GetService<AndroidAlert>().ShowAlert(new[] { "Are you sure you want to delete logs?", "deleting logs.txt", "Delete", "Cancel" },
                (index) =>
                {
                    Debug.Log("Index of button: " + index);
                    if (index == -3)
                    {
                        ClearFileImmediate();
                        ServiceProvider.GetService<EventSystem>().Raise<LogsDeletedEvent>();
                    }
                });
        }

        private static string TrimToTail(string content, int maxChars)
        {
            if (string.IsNullOrEmpty(content) || content.Length <= maxChars)
            {
                return content;
            }

            int start = content.Length - maxChars;
            int newline = content.IndexOf('\n', start);
            if (newline >= 0 && newline + 1 < content.Length)
            {
                start = newline + 1;
            }

            return content.Substring(start);
        }

        private void Init()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                fileManager = new AndroidJavaClass(PackName + "." + LoggerClassName);
                AndroidJavaClass playerClass = new("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
                fileManager.SetStatic("mainAct", activity);

                fileManagerInstance = fileManager.CallStatic<AndroidJavaObject>("GetInstance");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FileManager] Android FileManager init failed, using persistentDataPath fallback: {ex.Message}");
                fileManagerInstance = null;
            }
#endif
        }
    }
}