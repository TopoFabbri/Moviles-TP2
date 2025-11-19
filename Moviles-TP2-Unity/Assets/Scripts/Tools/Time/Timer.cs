using System;

namespace Tools.Time
{
    public sealed class Timer
    {
        private DateTime startTime;
        private DateTime pausedDateTime;

        private float pausedTime;
        private bool paused;
        private bool started;

        public float TimeElapsed =>
            started ? (float)(ServiceProvider.ServiceProvider.GetService<Time>().DateTime - startTime).TotalSeconds - pausedTime - (paused ? (float)(ServiceProvider.ServiceProvider.GetService<Time>().DateTime - pausedDateTime).TotalSeconds : 0f) : 0f;

        public Timer()
        {
            Restart();
        }

        public void Start()
        {
            if (started) return;
            started = true;

            startTime = ServiceProvider.ServiceProvider.GetService<Time>().DateTime;
            pausedDateTime = ServiceProvider.ServiceProvider.GetService<Time>().DateTime;
            pausedTime = 0f;
        }

        public void Restart()
        {
            started = false;
            pausedTime = 0f;
            paused = false;
            startTime = ServiceProvider.ServiceProvider.GetService<Time>().DateTime;
            pausedDateTime = ServiceProvider.ServiceProvider.GetService<Time>().DateTime;
        }
        
        public void Pause()
        {
            if (!started) return;
            
            pausedDateTime = ServiceProvider.ServiceProvider.GetService<Time>().DateTime;
            paused = true;
        }

        public void Resume()
        {
            if (!started) return;
            
            paused = false;
            pausedTime += (float)(ServiceProvider.ServiceProvider.GetService<Time>().DateTime - pausedDateTime).TotalSeconds;
        }
    }
}