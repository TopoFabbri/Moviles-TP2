using System;
using Game.Events;
using Game.World.Character.DroneModules;
using Game.World.GameManager;
using Tools.EventSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.World.Character
{
    internal sealed class DroneController : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float camRotationCatchupSpeed = 10f;
        [SerializeField] private float camPositionCatchupSpeed = 10f;
        [SerializeField] private float maxSpeed = 10f;
        [SerializeField] private float acceleration = 10f;
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
        }

        private void Start()
        {
            if (GameSave.RotationSensitivity <= 0f)
                GameSave.RotationSensitivity = rotationSpeed;

            rotationModule = new RotationModule(dronePivot, camPivot, rotationSpeed, camRotationCatchupSpeed);
            moveModule = new MoveModule(dronePivot, camPivot, acceleration, maxSpeed, camPositionCatchupSpeed);
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe(moveAction);
            EventSystem.Unsubscribe(lookAction);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
                Calibrate();

            ReadAccelerometer();

            rotationModule.Rotate(lookInput);
            moveModule.Move(moveInput);

            moveModule.VelocityModule.RaiseDebugVelocity();
        }

        private void ReadAccelerometer()
        {
            if (GameSave.ControlScheme != 0)
                return;

            Vector3 rot = AccelerometerHandler.GetRotation();

            lookInput.y = rot.y * GameSave.RotationSensitivity / 100f;
            lookInput.z = rot.z * GameSave.RotationSensitivity / 100f;
        }

        private static void Calibrate()
        {
            AccelerometerHandler.Calibrate();
        }
    }
}