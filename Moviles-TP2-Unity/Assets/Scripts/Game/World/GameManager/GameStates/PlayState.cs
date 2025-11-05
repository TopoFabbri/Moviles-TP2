using System;
using Game.Events;
using Game.World.Targets;
using Game.World.UI;
using Tools.EventSystem;
using Tools.FSM;
using Flags = Game.World.GameManager.GameManager.Flags;

namespace Game.World.GameManager.GameStates
{
    internal class PlayState : State
    {
        private bool crashed;
        private bool won;
        private bool paused;
        
        public override Type[] OnEnterParamTypes => new[] { typeof(TargetSpawner) };

        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            TargetSpawner targetSpawner = (TargetSpawner) parameters[0];
            
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () => EventSystem.Subscribe<DroneCollisionEvent>(OnCrashed));
            actions.AddMainThreadBehaviour(0, () => EventSystem.Subscribe<GameOverEvent>(OnWon));
            actions.AddMainThreadBehaviour(0, () => EventSystem.Subscribe<PauseEvent>(OnPause));
            actions.AddMainThreadBehaviour(0, () => targetSpawner.Spawn());
            actions.AddMainThreadBehaviour(0, () =>
            {
                crashed = false;
                won = false;
                paused = false;
            });
            
            return actions;
        }

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();

            actions.AddMainThreadBehaviour(0, () =>
            {
                if (crashed)
                    flag.Invoke(Flags.Crashed);
            });

            actions.AddMainThreadBehaviour(0, () =>
            {
                if (won)
                    flag.Invoke(Flags.Won);
            });
            
            actions.AddMainThreadBehaviour(0, () =>
            {
                if (paused)
                    flag.Invoke(Flags.Paused);
            });
            
            return actions;
        }

        public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () => EventSystem.Unsubscribe<DroneCollisionEvent>(OnCrashed));
            actions.AddMainThreadBehaviour(0, () => EventSystem.Unsubscribe<GameOverEvent>(OnWon));
            actions.AddMainThreadBehaviour(0, () => EventSystem.Unsubscribe<PauseEvent>(OnPause));
            
            return actions;
        }

        private void OnCrashed(DroneCollisionEvent _)
        {
            crashed = true;
        }
        
        private void OnWon(GameOverEvent _)
        {
            won = true;
        }

        private void OnPause(PauseEvent _)
        {
            paused = true;
        }
    }
}