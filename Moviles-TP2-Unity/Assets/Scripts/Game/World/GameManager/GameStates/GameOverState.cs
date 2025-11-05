using Game.World.UI;
using Tools.EventSystem;
using Tools.FSM;
using static Game.World.GameManager.GameManager;

namespace Game.World.GameManager.GameStates
{
    internal class GameOverState : State
    {
        private bool restart;
        
        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () => restart = false);
            actions.AddMainThreadBehaviour(0, () => EventSystem.Subscribe<RestartLevelEvent>(OnRestart));
            
            return actions;
        }

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () =>
            {
                if (restart)
                    flag.Invoke(Flags.Restart);
            });
            
            return actions;
        }

        public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () => EventSystem.Unsubscribe<RestartLevelEvent>(OnRestart));
            
            return actions;
        }

        private void OnRestart(RestartLevelEvent _)
        {
            restart = true;
        }
    }
}