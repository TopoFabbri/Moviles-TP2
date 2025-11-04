using Game.Events;
using Game.World.Targets;
using Game.World.UI;
using Tools.EventSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Time = Tools.Time.Time;

namespace Game.World.GameManager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private TargetSpawner targetSpawner;

        private const string TMPScene = "TmpScene";
        
        private void Awake()
        {
            EventSystem.Subscribe<DroneCollisionEvent>(OnDroneCollision);
            EventSystem.Subscribe<GameOverEvent>(OnGameOver);
            
            Application.targetFrameRate = 60;
            Time.Start();
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

        private void Update()
        {
            Time.Update();
        }

        private void OnDroneCollision(DroneCollisionEvent obj)
        {
            RestartLevel();
        }

        private static void OnGameOver(GameOverEvent obj)
        {
            LevelList.LoadNextLevel();
        }
        
        private static void RestartLevel()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(TMPScene);
            SceneManager.LoadScene(currentSceneIndex);
        }
    }
}
