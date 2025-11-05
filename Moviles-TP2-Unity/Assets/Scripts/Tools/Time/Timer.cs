using System;

namespace Tools.Time
{
    public sealed class Timer
    {
        private readonly DateTime startTime;
        private DateTime pausedDateTime;

        private float pausedTime;
        private bool paused;
        
        public float TimeElapsed => (float)(Time.DateTime - startTime).TotalSeconds - pausedTime - (paused ? (float)(Time.DateTime - pausedDateTime).TotalSeconds : 0f);
        
        public Timer()
        {
            startTime = Time.DateTime;
            pausedDateTime = Time.DateTime;
            pausedTime = 0f;
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