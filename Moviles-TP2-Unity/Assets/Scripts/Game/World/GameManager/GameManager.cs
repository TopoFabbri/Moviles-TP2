using Game.Events;
using Game.World.Targets;
using Tools.EventSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.World.GameManager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private TargetSpawner targetSpawner;
        
        private void Awake()
        {
            EventSystem.Subscribe<DroneCollisionEvent>(OnDroneCollision);
            EventSystem.Subscribe<GameOverEvent>(OnGameOver);
            
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            targetSpawner.Spawn();
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
        
        private static void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
