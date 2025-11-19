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

        private AndroidJavaClass fileManager = null;
        private AndroidJavaObject fileManagerInstance = null;

        public FileManager()
        {
            Init();
        }
        
        public string ReadFile()
        {
            string txt = fileManagerInstance?.Call<string>("ReadFile");

            return txt;
        }

        public void WriteFile(string data)
        {
            fileManagerInstance?.Call("WriteFile", data + "\n");
        }

        public void DeleteFile()
        {
            ServiceProvider.GetService<AndroidAlert>().ShowAlert(new[] { "Are you sure you want to delete logs?", "deleting logs.txt", "Delete", "Cancel" },
                (index) =>
                {
                    Debug.Log("Index of button: " + index);
                    if (index == -3)
                    {
                        fileManagerInstance?.Call("DeleteFiles");
                        
                        ServiceProvider.GetService<EventSystem>().Raise<LogsDeletedEvent>();
                    }
                });
        }

        private void Init()
        {
#if UNITY_ANDROID
            fileManager = new AndroidJavaClass(PackName + "." + LoggerClassName);
            AndroidJavaClass playerClass = new("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            fileManager.SetStatic("mainAct", activity);

            fileManagerInstance = fileManager.CallStatic<AndroidJavaObject>("GetInstance");
#endif
        }
    }
}