using Game.Events;
using Game.World.Character;
using Tools.EventSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    internal class InputListener : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string pcDefaultMap = "Analog+Shoulders";
        [SerializeField] private string androidDefaultMap = "Gyro";
        
        private void Awake()
        {
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                playerInput.SwitchCurrentActionMap(pcDefaultMap);
                playerInput.actions.FindActionMap(androidDefaultMap).Disable();
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                playerInput.SwitchCurrentActionMap(androidDefaultMap);
                playerInput.actions.FindActionMap(pcDefaultMap).Disable();
            }
        }
        
        private void Update()
        {
#if UNITY_EDITOR
            if (EditorApplication.isRemoteConnected)
            {
                playerInput.SwitchCurrentActionMap(androidDefaultMap);
                playerInput.actions.FindActionMap(pcDefaultMap).Disable();
            }
#endif
            
            if (playerInput.currentActionMap.name != "Gyro") return;
            
            if (Accelerometer.current != null && !Accelerometer.current.enabled)
                InputSystem.EnableDevice(Accelerometer.current);
                
            if (!GyroscopeHandler.Active)
                GyroscopeHandler.Active = true;
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
    }
}
