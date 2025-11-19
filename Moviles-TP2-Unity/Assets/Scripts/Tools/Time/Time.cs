using System;
using Tools.ServiceProvider;

namespace Tools.Time
{
    public class Time : IService
    {
        public float TimeElapsed
        {
            get
            {
                if (StartTime != default) return (float)(DateTime.UtcNow - StartTime).TotalSeconds;
                
                return -1f;
            }
        }

        public void Start()
        {
            StartTime = DateTime.Now;
        }

        public void Update()
        {
            TickTime = (float)(DateTime - _lastTickTime).TotalSeconds;
            _lastTickTime = DateTime;
        }

        public DateTime DateTime => DateTime.UtcNow;
        public DateTime DateTimeNow => DateTime.Now;
        public DateTime StartTime { get; private set; }
        public float TickTime { get; private set; }

        private DateTime _lastTickTime;

        public void ConvertToHours(float time, out int hours, out int minutes, out int seconds)
        {
            hours = (int)time / 3600;
            time -= hours * 3600;
            minutes = (int)time / 60;
            time -= minutes * 60;
            seconds = (int)time;
        }

        public void ConvertToMinutes(float time, out int minutes, out int seconds, out int centiseconds)
        {
            minutes = (int)time / 60;
            time -= minutes * 60;
            seconds = (int)time;
            centiseconds = (int)((time - seconds) * 100);
        }
    }
}