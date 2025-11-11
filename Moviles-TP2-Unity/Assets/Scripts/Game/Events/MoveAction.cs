using Tools.EventSystem;
using UnityEngine;

namespace Game.Events
{
    public class MoveAction : IEvent
    {
        public Vector3 input;
        
        public void Restart()
        {
            input = Vector3.zero;
        }

        public void Assign(params object[] parameters)
        {
            input = (Vector3)parameters[0];
        }
    }
}