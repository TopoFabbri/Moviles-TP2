using Model.Tools.EventSystem;
using UnityEngine;

namespace Game.Input
{
    public class LookAction : IEvent
    {
        public Vector3 input;
        
        public void Reset()
        {
            input = Vector3.zero;
        }
        
        public void Assign(params object[] parameters)
        {
            input = (Vector3)parameters[0];
        }
    }
}