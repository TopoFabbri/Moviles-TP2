using System;
using Tools.EventSystem;

namespace Tools.FSM
{
    internal class ChangedStateEvent<TStateType> : IEvent where TStateType : Enum
    {
        internal TStateType state;
        
        public void Restart()
        {
        }

        public void Assign(params object[] parameters)
        {
            state = (TStateType)parameters[0];
        }
    }
}