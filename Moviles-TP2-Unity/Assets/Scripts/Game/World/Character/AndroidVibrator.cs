using UnityEngine;

namespace Game.World.Character
{
    public static class AndroidVibrator
    {
#if UNITY_ANDROID
        private static readonly AndroidJavaClass UnityPlayer = new("com.unity3d.player.UnityPlayer");
        private static readonly AndroidJavaObject CurrentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        private static AndroidJavaObject vibrator = CurrentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#endif
            
        public static void Vibrate(long milliseconds)
        {
            bool vibrated = false;
#if UNITY_ANDROID
            Stop();
            vibrator.Call("vibrate", milliseconds);
            vibrated = true;
#endif
            if (!vibrated)
              Handheld.Vibrate();
        }

        public static void Stop()
        {
#if UNITY_ANDROID
            vibrator.Call("cancel");
#endif
        }

    }
}