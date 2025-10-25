using System;
using Game.Input;
using Model.Tools.EventSystem;
using UnityEngine;

namespace Game.World.Character
{
    internal sealed class DroneController : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float calibrationSpeed = 1f;
        [SerializeField] private float camRotationCatchupSpeed = 10f;
        [SerializeField] private float camPositionCatchupSpeed = 10f;
        [SerializeField] private float maxSpeed = 10f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 10f;
        [SerializeField] private Transform camPivot;
        [SerializeField] private Transform dronePivot;

        private RotationModule rotationModule;
        private MoveModule moveModule;

        private Vector3 moveInput = Vector3.zero;
        private Vector3 lookInput = Vector3.zero;

        private Action<MoveAction> moveAction;
        private Action<LookAction> lookAction;

        private void Awake()
        {
            moveAction = action => moveInput = action.input;
            lookAction = action => lookInput = action.input;

            EventSystem.Subscribe(moveAction);
            EventSystem.Subscribe(lookAction);

            rotationModule = new RotationModule(dronePivot, camPivot, rotationSpeed, calibrationSpeed, camRotationCatchupSpeed);
            moveModule = new MoveModule(dronePivot, camPivot, acceleration, deceleration, maxSpeed, camPositionCatchupSpeed);
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe(moveAction);
            EventSystem.Unsubscribe(lookAction);
        }

        private void Update()
        {
            rotationModule.Rotate(lookInput);
            moveModule.Move(moveInput);;
        }
    }
}