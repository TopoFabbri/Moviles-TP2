using System;
using Game.Input;
using Model.Tools.EventSystem;
using Model.Tools.FSM;
using UnityEngine;

namespace Game.World.Character
{
    internal sealed class DroneController : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float droneCatchupSpeed = .5f;
        [SerializeField] private float maxSpeed = 10f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 10f;
        [SerializeField] private Transform camPivot;
        [SerializeField] private Transform dronePivot;

        private VelocityModule velocityModule;

        private enum States
        {
            Idle,
            Move,
            Slow
        }

        internal enum Flags
        {
            Move,
            Break,
            Stopped
        }

        private FSM<States, Flags> fsm;

        private Vector2 moveInput = Vector2.zero;
        private Vector2 lookInput = Vector2.zero;

        private Action<MoveAction> moveAction;
        private Action<LookAction> lookAction;

        private void Awake()
        {
            moveAction = action => moveInput = action.input;
            lookAction = action => lookInput = action.input;

            EventSystem.Subscribe(moveAction);
            EventSystem.Subscribe(lookAction);

            velocityModule = new VelocityModule(acceleration, deceleration, maxSpeed);

            InitFsm();
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe(moveAction);
            EventSystem.Unsubscribe(lookAction);
        }

        private void Update()
        {
            fsm.Tick();
            velocityModule.RaiseDebugVelocity();
        }

        private void InitFsm()
        {
            fsm = new FSM<States, Flags>(States.Idle);

            fsm.AddState<IdleState>(States.Idle, onTickParameters: () => new object[] { moveInput, lookInput });
            fsm.AddState<MoveState>(States.Move,
                onTickParameters: () => new object[] { moveInput, lookInput, dronePivot, camPivot, rotationSpeed, droneCatchupSpeed, velocityModule });

            fsm.SetTransition(States.Idle, Flags.Move, States.Move);
        }
    }
}