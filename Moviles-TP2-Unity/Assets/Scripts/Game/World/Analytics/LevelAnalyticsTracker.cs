using Game.Events;
using Game.World.GameManager;
using Game.World.Targets;
using Tools.EventSystem;
using Tools.FSM;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.World.Analytics
{
    public sealed class LevelAnalyticsTracker : MonoBehaviour
    {
        private bool levelStartedThisAttempt;
        private bool isRetryAttempt;
        private States previousState = States.Restart;
        private string lastCrashObstacle = "unknown";
        private Vector3 lastCrashPosition = Vector3.zero;
        private float levelStartTime;

        private void OnEnable()
        {
            if (ServiceProvider.TryGetService<EventSystem>(out var eventSystem))
            {
                eventSystem.Subscribe<ChangedStateEvent<States>>(OnStateChanged);
                eventSystem.Subscribe<DroneCollisionEvent>(OnDroneCollision);
            }
        }

        private void OnDisable()
        {
            if (ServiceProvider.TryGetService<EventSystem>(out var eventSystem))
            {
                eventSystem.Unsubscribe<ChangedStateEvent<States>>(OnStateChanged);
                eventSystem.Unsubscribe<DroneCollisionEvent>(OnDroneCollision);
            }
        }

        private void OnDroneCollision(DroneCollisionEvent evt)
        {
            lastCrashObstacle = string.IsNullOrEmpty(evt.ObstacleName) ? "unknown" : evt.ObstacleName;
            lastCrashPosition = evt.ContactPoint;
        }

        private void OnStateChanged(ChangedStateEvent<States> evt)
        {
            if (previousState == States.Restart && evt.state == States.Play)
            {
                levelStartedThisAttempt = true;
                levelStartTime = Time.time;
                lastCrashObstacle = "unknown";
                lastCrashPosition = Vector3.zero;

                if (ServiceProvider.TryGetService<IAnalyticsService>(out var analytics))
                {
                    int totalRings = GetTotalRings();
                    analytics.TrackLevelStarted(
                        GameSave.CurrentLevel,
                        SceneManager.GetActiveScene().name,
                        totalRings,
                        GameSave.AddedSpeed,
                        GameSave.GodMode,
                        isRetryAttempt);
                }

                isRetryAttempt = true;
            }
            else if (evt.state == States.GameOver && levelStartedThisAttempt)
            {
                levelStartedThisAttempt = false;
                if (ServiceProvider.TryGetService<IAnalyticsService>(out var analytics))
                {
                    int totalRings = GetTotalRings();
                    int remaining = ObstacleScore.RemainingCount;
                    int ringsPassed = Mathf.Max(0, totalRings - remaining);
                    float elapsed = Mathf.Max(0f, Time.time - levelStartTime);

                    analytics.TrackLevelFailed(
                        GameSave.CurrentLevel,
                        SceneManager.GetActiveScene().name,
                        elapsed,
                        ringsPassed,
                        totalRings,
                        ObstacleScore.CurrentOrder,
                        lastCrashObstacle,
                        lastCrashPosition);
                }
            }

            previousState = evt.state;
        }

        public void NotifyLevelAbandoned(string fromState)
        {
            if (!levelStartedThisAttempt) return;
            levelStartedThisAttempt = false;

            if (ServiceProvider.TryGetService<IAnalyticsService>(out var analytics))
            {
                int totalRings = GetTotalRings();
                int remaining = ObstacleScore.RemainingCount;
                int ringsPassed = Mathf.Max(0, totalRings - remaining);
                float elapsed = Mathf.Max(0f, Time.time - levelStartTime);

                analytics.TrackLevelAbandoned(
                    GameSave.CurrentLevel,
                    SceneManager.GetActiveScene().name,
                    elapsed,
                    ringsPassed,
                    totalRings,
                    fromState);
            }
        }

        private static int GetTotalRings()
        {
            var spawner = FindFirstObjectByType<TargetSpawner>();
            return spawner != null ? spawner.TotalRingCount : 0;
        }
    }
}
