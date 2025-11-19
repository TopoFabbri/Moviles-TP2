using System;
using Game.Events;
using Game.World.Character.DroneModules;
using Game.World.GameManager;
using Game.World.UI;
using Tools.EventSystem;
using Tools.ServiceProvider;
using UnityEngine;

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
        [SerializeField] private HUDController hudController;
        
        private Vector3 initPos;
        private Quaternion initRot;
        
        private RotationModule rotationModule;
        private MoveModule moveModule;
        
        private Vector3 moveInput = Vector3.zero;
        private Vector3 lookInput = Vector3.zero;

        private Action<MoveAction> moveAction;
        private Action<LookAction> lookAction;

        private void Awake()
        {
            moveAction = action =>
            {
                moveInput = action.input;
                ServiceProvider.GetService<GameTime>().Timer.Start();
            };
            
            lookAction = action => lookInput = action.input;

            ServiceProvider.GetService<EventSystem>().Subscribe(moveAction);
            ServiceProvider.GetService<EventSystem>().Subscribe(lookAction);
            ServiceProvider.GetService<EventSystem>().Subscribe<CalibrateAction>(Calibrate);

            rotationModule = new RotationModule(dronePivot, camPivot, rotationSpeed, camRotationCatchupSpeed);
            moveModule = new MoveModule(dronePivot, camPivot, acceleration, maxSpeed, camPositionCatchupSpeed);
            
            initPos = dronePivot.position;
            initRot = dronePivot.rotation;
        }

        private void Start()
        {
            if (GameSave.RotationSensitivity <= 0f)
                GameSave.RotationSensitivity = rotationSpeed;
        }

        private void OnDestroy()
        {
            ServiceProvider.GetService<EventSystem>().Unsubscribe(moveAction);
            ServiceProvider.GetService<EventSystem>().Unsubscribe(lookAction);
            ServiceProvider.GetService<EventSystem>().Unsubscribe<CalibrateAction>(Calibrate);
        }

        internal void OnUpdate()
        {
            ReadAccelerometer();

            rotationModule.Rotate(lookInput);
            moveModule.Move(moveInput);

            moveModule.VelocityModule.RaiseDebugVelocity();
        }

        internal void Restart()
        {
            dronePivot.position = initPos;
            dronePivot.rotation = initRot;

            rotationModule.Restart();
            moveModule.Restart();
        }
        
        private void ReadAccelerometer()
        {
            if (GameSave.ControlScheme != 0)
                return;

            Vector3 rot = ServiceProvider.GetService<AccelerometerHandler>().GetRotation();
            
            lookInput.y = rot.y * GameSave.RotationSensitivity / 100f;
            lookInput.z = rot.z * GameSave.RotationSensitivity / 100f;
        }

        private static void Calibrate(CalibrateAction _)
        {
            ServiceProvider.GetService<AccelerometerHandler>().Calibrate();
        }
    }
}