using Tools.EventSystem;

namespace Game.Events
{
    internal sealed class VelocityDebugEvent : IEvent
    {
        internal float speed;
        internal float maxSpeed;
        
        public void Reset()
        {
            speed = 0f;
            maxSpeed = 0f;
        }

        public void Assign(params object[] parameters)
        {
            speed = (float)parameters[0];
            maxSpeed = (float)parameters[1];
        }
    }
}