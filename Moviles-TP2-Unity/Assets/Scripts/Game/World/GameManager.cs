using Game.World.Character;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.World
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string pcDefaultMap;
        [SerializeField] private string androidDefaultMap;
        
        private void Awake()
        {
            Application.targetFrameRate = 60;
            
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
            if (playerInput.currentActionMap.name == "Gyro")
            {
                if (Accelerometer.current == null || Accelerometer.current.enabled) return;

                InputSystem.EnableDevice(Accelerometer.current);
                GyroscopeHandler.Active = true;
            }
        }
    }
}
