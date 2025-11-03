using System;

namespace Tools.Time
{
    public sealed class Timer
    {
        private readonly DateTime startTime;

        private float pausedTime;
        
        public float TimeElapsed => (float)(Time.DateTime - startTime).TotalSeconds - pausedTime;
        
        public Timer()
        {
            startTime = Time.DateTime;
        }
        
        public void Pause()
        {
            pausedTime += (float)(Time.DateTime - startTime).TotalSeconds;
        }
        
        public void Resume()
        {
            pausedTime -= (float)(Time.DateTime - startTime).TotalSeconds;
        }
    }
}