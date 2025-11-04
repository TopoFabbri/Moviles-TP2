using Tools.EventSystem;

namespace Game.World.GameManager
{
    internal sealed class ControlSchemeChangedEvent : IEvent
    {
        internal int schemeId;
        
        public void Reset()
        {
            schemeId = -1;
        }

        public void Assign(params object[] parameters)
        {
            schemeId = (int) parameters[0];
        }
    }
}