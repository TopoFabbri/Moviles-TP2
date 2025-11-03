using Game.Events;
using Game.World.Character;
using Game.World.UI;
using Tools.EventSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    internal class InputListener : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string analogsMap = "Analogs+Shoulders";
        [SerializeField] private string gyroMap = "Gyro";
        [SerializeField] private string uiMap = "UI";

        private void Awake()
        {
            playerInput.actions.FindActionMap(uiMap).Disable();

            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                playerInput.SwitchCurrentActionMap(analogsMap);
                playerInput.actions.FindActionMap(gyroMap).Disable();
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                playerInput.SwitchCurrentActionMap(gyroMap);
                playerInput.actions.FindActionMap(analogsMap).Disable();
            }
            
            EventSystem.Subscribe<PauseEvent>(OnPaused);
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<PauseEvent>(OnPaused);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (EditorApplication.isRemoteConnected)
            {
                playerInput.SwitchCurrentActionMap(gyroMap);
                playerInput.actions.FindActionMap(analogsMap).Disable();
            }
#endif
            
            if (playerInput.currentActionMap.name != "Gyro") return;
            
            if (Accelerometer.current != null && !Accelerometer.current.enabled)
                InputSystem.EnableDevice(Accelerometer.current);
                
            if (!AccelerometerHandler.Active)
                AccelerometerHandler.Active = true;
        }
        
        public void OnMove(InputValue input)
        {
            Vector3 inputVector = input.Get<Vector3>();
            
            EventSystem.Raise<MoveAction>(inputVector);
        }
        
        public void OnLook(InputValue input)
        {
            Vector3 inputVector = input.Get<Vector3>();
            
            EventSystem.Raise<LookAction>(inputVector);
        }

        public void OnPause()
        {
            EventSystem.Raise<PauseAction>();
        }

        private void OnPaused(PauseEvent obj)
        {
            if (obj.paused)
                playerInput.actions.FindActionMap(uiMap).Disable();
            else
                playerInput.actions.FindActionMap(uiMap).Enable();
        }
    }
}
