using Tools.EventSystem;

namespace Tools.FSM
{
    public class DebugEvent : IEvent
    {
        public string Text { get; private set; }
        
        public void Restart()
        {
            Text = "";
        }

        public void Assign(params object[] parameters)
        {
            Text = (string) parameters[0];
        }
    }
}