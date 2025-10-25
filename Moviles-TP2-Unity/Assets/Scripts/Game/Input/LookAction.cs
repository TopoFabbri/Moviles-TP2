using Model.Tools.EventSystem;
using UnityEngine;

namespace Game.Input
{
    public class LookAction : IEvent
    {
        public Vector2 input;
        
        public void Reset()
        {
            input = Vector2.zero;
        }
        
        public void Assign(params object[] parameters)
        {
            input = (Vector2)parameters[0];
        }
    }
}