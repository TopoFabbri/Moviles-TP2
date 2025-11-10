using System;

namespace Tools.Time
{
    public static class Time
    {
        public static float TimeElapsed
        {
            get
            {
                if (StartTime != default) return (float)(DateTime.UtcNow - StartTime).TotalSeconds;
                
                return -1f;
            }
        }

        public static void Start()
        {
            StartTime = DateTime.Now;
        }

        public static void Update()
        {
            TickTime = (float)(DateTime - _lastTickTime).TotalSeconds;
            _lastTickTime = DateTime;
        }

        public static DateTime DateTime => DateTime.UtcNow;
        public static DateTime DateTimeNow => DateTime.Now;
        public static DateTime StartTime { get; private set; }
        public static float TickTime { get; private set; }

        private static DateTime _lastTickTime;

        public static void ConvertToHours(float time, out int hours, out int minutes, out int seconds)
        {
            hours = (int)time / 3600;
            time -= hours * 3600;
            minutes = (int)time / 60;
            time -= minutes * 60;
            seconds = (int)time;
        }

        public static void ConvertToMinutes(float time, out int minutes, out int seconds, out int centiseconds)
        {
            minutes = (int)time / 60;
            time -= minutes * 60;
            seconds = (int)time;
            centiseconds = (int)((time - seconds) * 100);
        }
    }
}