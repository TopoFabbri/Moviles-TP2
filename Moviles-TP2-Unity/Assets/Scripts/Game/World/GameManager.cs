using Game.Events;
using Game.World.Character;
using Tools.EventSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game.World
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string pcDefaultMap;
        [SerializeField] private string androidDefaultMap;
        
        private void Awake()
        {
            EventSystem.Subscribe<DroneCollisionEvent>(OnDroneCollision);
            EventSystem.Subscribe<GameOverEvent>(OnGameOver);
            
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

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<DroneCollisionEvent>(OnDroneCollision);
            EventSystem.Unsubscribe<GameOverEvent>(OnGameOver);
        }

        private void OnDroneCollision(DroneCollisionEvent obj)
        {
            RestartLevel();
        }

        private void OnGameOver(GameOverEvent obj)
        {
            RestartLevel();
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
        
        private static void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
