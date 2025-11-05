using System;
using Game.World.Character;
using Game.World.Targets;
using Game.World.UI;
using Tools.EventSystem;
using Tools.FSM;
using static Game.World.GameManager.GameManager;

namespace Game.World.GameManager.GameStates
{
    internal class GameOverState : State
    {
        private bool restart;
        
        public override Type[] OnExitParamTypes => new[] { typeof(TargetSpawner), typeof(DroneController) };

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
                    flag.Invoke(Flags.Play);
            });
            
            return actions;
        }

        public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
        {
            TargetSpawner targetSpawner = (TargetSpawner) parameters[0];
            DroneController drone = (DroneController) parameters[1];
            
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () => targetSpawner.Restart());
            actions.AddMainThreadBehaviour(0, () => targetSpawner.Spawn());
            actions.AddMainThreadBehaviour(0, () => drone.Restart());
            actions.AddMainThreadBehaviour(0, () => EventSystem.Unsubscribe<RestartLevelEvent>(OnRestart));
            
            return actions;
        }

        private void OnRestart(RestartLevelEvent _)
        {
            restart = true;
        }
    }
}