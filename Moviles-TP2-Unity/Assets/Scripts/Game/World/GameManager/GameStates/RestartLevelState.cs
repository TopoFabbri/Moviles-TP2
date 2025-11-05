using System;
using Game.World.Character;
using Game.World.Targets;
using Tools.FSM;

namespace Game.World.GameManager.GameStates
{
    internal sealed class RestartLevelState : State
    {
        public override Type[] OnTickParamTypes => new[] { typeof(TargetSpawner), typeof(DroneController) };

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            TargetSpawner targetSpawner = (TargetSpawner) parameters[0];
            DroneController drone = (DroneController) parameters[1];
            
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () => targetSpawner.Restart());
            actions.AddMainThreadBehaviour(0, () => targetSpawner.Spawn());
            actions.AddMainThreadBehaviour(0, () => drone.Restart());
            actions.AddMainThreadBehaviour(0, () => flag.Invoke(GameManager.Flags.Play));
            actions.AddMultiThreadableBehaviour(0, () => GameTime.Timer.Restart());
            
            return actions;
        }
    }
}