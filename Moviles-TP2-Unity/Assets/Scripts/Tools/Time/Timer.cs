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
            started ? (float)(Time.DateTime - startTime).TotalSeconds - pausedTime - (paused ? (float)(Time.DateTime - pausedDateTime).TotalSeconds : 0f) : 0f;

        public Timer()
        {
            Restart();
        }

        public void Start()
        {
            if (started) return;
            started = true;

            startTime = Time.DateTime;
            pausedDateTime = Time.DateTime;
            pausedTime = 0f;
        }

        public void Restart()
        {
            started = false;
            pausedTime = 0f;
            paused = false;
            startTime = Time.DateTime;
            pausedDateTime = Time.DateTime;
        }
        
        public void Pause()
        {
            pausedDateTime = Time.DateTime;
            paused = true;
        }

        public void Resume()
        {
            paused = false;
            pausedTime += (float)(Time.DateTime - pausedDateTime).TotalSeconds;
        }
    }
}