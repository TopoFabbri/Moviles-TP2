using Tools.EventSystem;
using UnityEngine;

namespace Game.Events
{
    public class RingPassedEvent : IEvent
    {
        public Vector3 Position { get; private set; }
        public int RemainingRings { get; private set; }

        public void Assign(params object[] parameters)
        {
            if (parameters != null && parameters.Length > 0 && parameters[0] is Vector3 pos)
                Position = pos;
            if (parameters != null && parameters.Length > 1 && parameters[1] is int rem)
                RemainingRings = rem;
        }

        public void Restart()
        {
            Reset();
        }

        public void Reset()
        {
            Position = Vector3.zero;
            RemainingRings = 0;
        }
    }
}
