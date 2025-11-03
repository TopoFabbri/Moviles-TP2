using Tools.EventSystem;

namespace Game.World.UI
{
    internal sealed class PauseEvent : IEvent
    {
        public bool paused;
        
        public void Reset()
        {
            paused = false;
        }

        public void Assign(params object[] parameters)
        {
            paused = (bool) parameters[0];
        }
    }
}