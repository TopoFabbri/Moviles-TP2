using System;
using Tools.ServiceProvider;
using UnityEngine;

namespace Game.World.FabbriPlugin
{
    internal class Logger : IService
    {
        internal enum LogLevel
        {
            Error,
            Warning,
            Info,
            Debug,
            Verbose
        }

        private const string LoggerClassName = "com.fabbri2025.topologger.TopoLogger";

        private bool isInitialized;

#if UNITY_ANDROID
        private AndroidJavaClass loggerClass;
#endif

        private void InitLogger()
        {
            isInitialized = true;

#if UNITY_ANDROID
            loggerClass = new AndroidJavaClass(LoggerClassName);
#endif
        }

        internal void Log(string message)
        {
            if (!isInitialized) InitLogger();

#if UNITY_ANDROID
            loggerClass?.CallStatic("SendLog", "LogSent", (int)LogLevel.Debug);
#endif
        }

        internal string GetLogs()
        {
            string logs = "";

#if UNITY_ANDROID
            logs = loggerClass?.CallStatic<string>("GetLogs");
#endif

            return logs;
        }
    }

    internal class AndroidAlert : IService
    {
        private const string PluginName = "com.fabbri2025.topologger";

#if UNITY_ANDROID
        private readonly AndroidJavaObject popupClassInstance;
        private const string PopupClassName = ".PopUp";
        private const string MainActivityFieldName = "mainAct";
#endif

        public AndroidAlert()
        {
#if UNITY_ANDROID
            AndroidJavaClass playerClass1 = new("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity1 = playerClass1.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaClass popupClass1 = new(PluginName + PopupClassName);
            popupClassInstance = popupClass1.CallStatic<AndroidJavaObject>("GetInstance");
            
            popupClass1.SetStatic(MainActivityFieldName, activity1);
#endif
        }
        
        public void ShowAlert(string[] strings, Action<int> alertHandler = null)
        {
#if UNITY_ANDROID
            if (strings.Length < 3)
            {
                Debug.LogError("Alert requires at least 3 strings");
                return;
            }

            popupClassInstance.Call("ShowAlertView", strings, new AlertViewCallback(alertHandler));
#endif
        }

#if UNITY_ANDROID
        private class AlertViewCallback : AndroidJavaProxy
        {
            private readonly Action<int> alertHandler;

            public AlertViewCallback(Action<int> alertHandlerIn) : base(PluginName + ".PopUp$AlertViewCallBack")
            {
                alertHandler = alertHandlerIn;
            }

            public void OnButtonTapped(int index)
            {
                alertHandler?.Invoke(index);
            }
        }
#endif
    }
}