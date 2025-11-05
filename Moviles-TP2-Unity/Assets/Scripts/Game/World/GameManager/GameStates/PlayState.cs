using System;
using Game.Events;
using Game.Input;
using Game.World.Character;
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
        
        public override Type[] OnTickParamTypes => new[] { typeof(DroneController) };

        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () => EventSystem.Subscribe<DroneCollisionEvent>(OnCrashed));
            actions.AddMainThreadBehaviour(0, () => EventSystem.Subscribe<GameOverEvent>(OnWon));
            actions.AddMainThreadBehaviour(0, () => EventSystem.Subscribe<PauseAction>(OnPause));
            actions.AddMultiThreadableBehaviour(0, () => GameTime.Timer.Resume());
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
            DroneController drone = (DroneController) parameters[0];
            
            BehaviourActions actions = Pool.Get<BehaviourActions>();

            actions.AddMainThreadBehaviour(0, () =>
            {
                if (crashed)
                    flag.Invoke(Flags.Crashed);
                else if (won)
                    flag.Invoke(Flags.Won);
                else if (paused)
                    flag.Invoke(Flags.Paused);
            });
            
            actions.AddMainThreadBehaviour(0, () =>
            {
                drone.OnUpdate();
            });
            
            return actions;
        }

        public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () => EventSystem.Unsubscribe<DroneCollisionEvent>(OnCrashed));
            actions.AddMainThreadBehaviour(0, () => EventSystem.Unsubscribe<GameOverEvent>(OnWon));
            actions.AddMainThreadBehaviour(0, () => EventSystem.Unsubscribe<PauseAction>(OnPause));
            actions.AddMultiThreadableBehaviour(0, () => GameTime.Timer.Pause());
            
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

        private void OnPause(PauseAction _)
        {
            paused = true;
        }
    }
}