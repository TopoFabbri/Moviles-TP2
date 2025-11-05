using System;
using Game.World.Character;
using Game.World.Targets;
using Tools.FSM;
using static Game.World.GameManager.GameManager;

namespace Game.World.GameManager.GameStates
{
    internal class GameOverState : State
    {
        public override Type[] OnExitParamTypes => new[] { typeof(TargetSpawner), typeof(DroneController) };

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () => flag.Invoke(Flags.Play));
            
            return actions;
        }

        public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
        {
            TargetSpawner targetSpawner = (TargetSpawner) parameters[0];
            DroneController drone = (DroneController) parameters[1];
            
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () => targetSpawner.Restart());
            actions.AddMainThreadBehaviour(0, () => drone.Restart());
            
            return actions;
        }
    }
}