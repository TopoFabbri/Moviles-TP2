using Game.Events;
using Tools.EventSystem;
using UnityEngine;

namespace Game.World.UI
{
    public static class FileManager
    {
        private const string packName = "com.fabbri2025.topologger";
        private const string loggerClassName = "FileManager";

        private static AndroidJavaClass fileManager = null;
        private static AndroidJavaObject fileManagerInstance = null;

        public static string ReadFile()
        {
            if (fileManagerInstance == null)
                Init();
            
            string txt = fileManagerInstance?.Call<string>("ReadFile");

            return txt;
        }

        public static void WriteFile(string data)
        {
            if (fileManagerInstance == null)
            {
                Init();
            }
            fileManagerInstance?.Call("WriteFile", data + "\n");
        }

        public static void DeleteFile()
        {
            AndroidAlert.ShowAlert(new string[] { "Are you sure you want to delete logs?", "deleting logs.txt", "Delete", "Cancel" },
                (index) =>
                {
                    Debug.Log("Index of button: " + index);
                    if (index == -3)
                    {
                        if (fileManagerInstance == null)
                        {
                            Init();
                        }
                        fileManagerInstance?.Call("DeleteFiles");
                        
                        EventSystem.Raise<LogsDeletedEvent>();
                    }
                });
        }

        private static void Init()
        {
#if UNITY_ANDROID
            fileManager = new AndroidJavaClass(packName + "." + loggerClassName);
            AndroidJavaClass playerClass = new("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            fileManager.SetStatic("mainAct", activity);

            fileManagerInstance = fileManager.CallStatic<AndroidJavaObject>("GetInstance");
#endif
        }
    }
}