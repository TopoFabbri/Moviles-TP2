using System;
using Tools.EventSystem;

namespace Tools.FSM
{
    public class ChangedStateEvent<TStateType> : IEvent where TStateType : Enum
    {
        public TStateType state;
        
        public void Restart()
        {
        }

        public void Assign(params object[] parameters)
        {
            state = (TStateType)parameters[0];
        }
    }
}