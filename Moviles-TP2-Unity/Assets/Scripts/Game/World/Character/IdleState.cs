using System;
using Model.Tools.FSM;
using UnityEngine;

namespace Game.World.Character
{
    internal sealed class IdleState : State
    {
        public override Type[] OnTickParamTypes => new[] {typeof(Vector2), typeof(Vector2)};

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            Vector2 moveInput = (Vector2) parameters[0];
            Vector2 lookInput = (Vector2) parameters[1];
            
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () =>
            {
                if (moveInput.magnitude > float.Epsilon || lookInput.magnitude > float.Epsilon)
                    flag.Invoke(DroneController.Flags.Move);
            });
            
            return actions;
        }
    }
}