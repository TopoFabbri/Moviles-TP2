using Tools.ServiceProvider;
using UnityEngine;

namespace Game.World.Character
{
    public class AndroidVibrator : IService
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private readonly AndroidJavaObject vibrator;
#endif

        public AndroidVibrator()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#endif
        }
        
        public void Vibrate(long milliseconds)
        {
            bool vibrated = false;
#if UNITY_ANDROID && !UNITY_EDITOR
            Stop();
            vibrator?.Call("vibrate", milliseconds);
            vibrated = true;
#endif
            if (!vibrated)
              Handheld.Vibrate();
        }

        public void Stop()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            vibrator?.Call("cancel");
#endif
        }

    }
}