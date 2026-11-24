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
            UpdateRotation();
            moveModule.Move(moveInput);

            moveModule.VelocityModule.RaiseDebugVelocity();
        }

        internal void StopMovement()
        {
            moveModule.Stop();
        }

        internal void Restart()
        {
            dronePivot.position = initPos;
            dronePivot.rotation = initRot;
            if (dronePivot.TryGetComponent(out Rigidbody rb) && !rb.isKinematic)
            {
                rb.position = initPos;
                rb.rotation = initRot;
            }

            rotationModule.Restart();
            moveModule.Restart();
        }
        
        private void UpdateRotation()
        {
            if (GameSave.ControlScheme != 0)
            {
                rotationModule.Rotate(lookInput);
                return;
            }

            float sensitivityScale = GameSave.RotationSensitivity / 100f;
            AccelerometerHandler handler = ServiceProvider.GetService<AccelerometerHandler>();

            if (GameSave.GyroMode == GyroMode.BankToTurn)
            {
                Quaternion swing = handler.GetSwingRotation();
                rotationModule.RotateBankToTurn(swing, lookInput.x, sensitivityScale);
            }
            else
            {
                Vector3 rot = handler.GetRotation();
                Vector3 acroInput = new(lookInput.x, rot.y * sensitivityScale, rot.z * sensitivityScale);
                rotationModule.RotateFpvAcro(acroInput);
            }
        }

        private static void Calibrate(CalibrateAction _)
        {
            ServiceProvider.GetService<AccelerometerHandler>().Calibrate();
        }
    }
}