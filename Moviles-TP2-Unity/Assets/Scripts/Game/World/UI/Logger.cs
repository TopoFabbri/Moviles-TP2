using System;
using UnityEngine;

namespace Game.World.UI
{
    internal static class Logger
    {
        internal enum LogLevel
        {
            Error,
            Warning,
            Info,
            Debug,
            Verbose
        }

        const string LoggerClassName = "com.fabbri2025.topologger.TopoLogger";

        private static bool isInitialized;

#if UNITY_ANDROID
        private static AndroidJavaClass loggerClass;
#endif

        private static void InitLogger()
        {
            isInitialized = true;

#if UNITY_ANDROID
            loggerClass = new AndroidJavaClass(LoggerClassName);
#endif
        }

        internal static void Log(string message)
        {
            if (!isInitialized) InitLogger();

#if UNITY_ANDROID
            loggerClass?.CallStatic("SendLog", "LogSent", (int)LogLevel.Debug);
#endif
        }

        internal static string GetLogs()
        {
            string logs = "";

#if UNITY_ANDROID
            logs = loggerClass?.CallStatic<string>("GetLogs");
#endif

            return logs;
        }
    }

    internal static class AndroidAlert
    {
        private const string PluginName = "com.fabbri2025.topologger";

#if UNITY_ANDROID
        private static AndroidJavaClass playerClass = new("com.unity3d.player.UnityPlayer");
        private static AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");

        private static AndroidJavaClass popupClass = new(PluginName + ".PopUp");
        private static AndroidJavaObject popupClassInstance = popupClass.CallStatic<AndroidJavaObject>("GetInstance");
#endif

        public static void ShowAlert(string[] strings, Action<int> alertHandler = null)
        {
#if UNITY_ANDROID
            popupClass.SetStatic("mainAct", activity);

            if (strings.Length < 3)
            {
                Debug.LogError("Alert requires at least 3 strings");
                return;
            }

            popupClassInstance.Call("ShowAlertView", strings, new AlertViewCallback(alertHandler));
#endif
        }

#if UNITY_ANDROID
        class AlertViewCallback : AndroidJavaProxy
        {
            private Action<int> alertHandler;

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