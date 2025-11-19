using Tools.EventSystem;
using UnityEngine;

namespace Game.Events
{
    internal sealed class TriggerPopupEvent : IEvent
    {
        public GameObject popup;
        
        public void Restart()
        {
        }
        
        public void Assign(params object[] parameters)
        {
            popup = parameters[0] as GameObject;
        }
    }
}