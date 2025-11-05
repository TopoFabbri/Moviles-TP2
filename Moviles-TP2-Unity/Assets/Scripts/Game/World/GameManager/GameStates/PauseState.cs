using Game.Input;
using Tools.EventSystem;
using Tools.FSM;

namespace Game.World.GameManager.GameStates
{
    internal class PauseState : State
    {
        private bool resume;
        
        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () => EventSystem.Subscribe<CancelAction>(OnCancelAction));
            actions.AddMainThreadBehaviour(0, () => EventSystem.Subscribe<PauseAction>(OnPauseAction));
            actions.AddMainThreadBehaviour(0, () => resume = false);
            
            return actions;
        }
        
        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();

            actions.AddMainThreadBehaviour(0, () =>
            {
                if (resume)
                    flag.Invoke(GameManager.Flags.Play);
            });

            return actions;
        }

        public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () => EventSystem.Unsubscribe<CancelAction>(OnCancelAction));
            actions.AddMainThreadBehaviour(0, () => EventSystem.Unsubscribe<PauseAction>(OnPauseAction));
            
            return actions;
        }

        private void OnCancelAction(CancelAction obj)
        {
            Resume();
        }

        private void OnPauseAction(PauseAction obj)
        {
            Resume();
        }

        private void Resume()
        {
            resume = true;
        }
    }
}