using Tools.EventSystem;

namespace Game.World.Character
{
    internal sealed class VelocityDebugEvent : IEvent
    {
        public float speed;
        public float acceleration;
        public float deceleration;
        public float maxSpeed;
        
        public void Reset()
        {
            speed = 0f;
            acceleration = 0f;
            deceleration = 0f;
            maxSpeed = 0f;
        }

        public void Assign(params object[] parameters)
        {
            speed = (float)parameters[0];
            acceleration = (float)parameters[1];
            deceleration = (float)parameters[2];
            maxSpeed = (float)parameters[3];
        }
    }
}