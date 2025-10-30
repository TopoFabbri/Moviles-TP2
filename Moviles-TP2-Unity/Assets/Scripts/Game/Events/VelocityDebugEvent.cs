using Tools.EventSystem;

namespace Game.Events
{
    internal sealed class VelocityDebugEvent : IEvent
    {
        public float speed;
        
        public void Reset()
        {
            speed = 0f;
        }

        public void Assign(params object[] parameters)
        {
            speed = (float)parameters[0];
        }
    }
}