using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.World.GameManager;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.World.Analytics
{
    public sealed class AnalyticsServiceManager : IAnalyticsService
    {
        private const string SessionActiveKey = "Analytics_SessionActive";
        private const int MaxRuntimeErrorsPerSession = 20;
        private const float ErrorDedupeWindowSeconds = 5f;

        private readonly IAnalyticsProvider provider;
        private readonly ConcurrentQueue<(string type, string message, string scene)> pendingErrors =
            new ConcurrentQueue<(string, string, string)>();
        private readonly Dictionary<string, float> recentErrorTimestamps = new Dictionary<string, float>();

        private int errorsRecordedThisSession;
        private float sessionStartRealtime;
        private bool sessionEnded;

        public bool IsInitialized { get; private set; }
        public bool IsDataCollectionEnabled { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoRegister()
        {
            var manager = new AnalyticsServiceManager();
            if (!ServiceProvider.TryAddService<IAnalyticsService>(manager))
                return;

            manager.Initialize();
        }

        public AnalyticsServiceManager() : this(new CompositeAnalyticsProvider(
            new GooglePlayGamesAnalyticsProvider(),
            new UnityAnalyticsProvider()))
        {
        }

        public AnalyticsServiceManager(IAnalyticsProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            IsDataCollectionEnabled = GameSave.AnalyticsEnabled;
        }

        public void Initialize()
        {
            if (IsInitialized) return;

            sessionStartRealtime = Time.realtimeSinceStartup;
            IsDataCollectionEnabled = GameSave.AnalyticsEnabled;
            provider.SetConsent(IsDataCollectionEnabled);

            bool previousUnclean = PlayerPrefs.GetInt(SessionActiveKey, 0) == 1;
            PlayerPrefs.SetInt(SessionActiveKey, 1);
            PlayerPrefs.Save();

            CreateLifecycleRunner();
            _ = InitializeProviderAndStartSessionAsync(previousUnclean);
        }

        private async Task InitializeProviderAndStartSessionAsync(bool previousUnclean)
        {
            try
            {
                await provider.InitializeAsync();
                IsInitialized = true;
                provider.SetConsent(IsDataCollectionEnabled);

                if (IsDataCollectionEnabled)
                {
                    RecordSessionStarted(previousUnclean);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Analytics] Initialization failed: {ex.Message}");
            }
        }

        private void CreateLifecycleRunner()
        {
            var go = new GameObject("AnalyticsLifecycleRunner");
            UnityEngine.Object.DontDestroyOnLoad(go);
            var runner = go.AddComponent<AnalyticsLifecycleRunner>();
            runner.Bind(this);
        }

        public void SetDataCollectionEnabled(bool enabled)
        {
            IsDataCollectionEnabled = enabled;
            GameSave.AnalyticsEnabled = enabled;
            provider.SetConsent(enabled);
        }

        private void RecordSessionStarted(bool previousUnclean)
        {
            var parameters = new Dictionary<string, object>
            {
                { "device_model", SystemInfo.deviceModel ?? "unknown" },
                { "os_version", SystemInfo.operatingSystem ?? "unknown" },
                { "ram_mb", SystemInfo.systemMemorySize },
                { "screen_resolution", $"{Screen.width}x{Screen.height}" },
                { "app_version", Application.version ?? "0.0.0" },
                { "control_scheme", GameSave.ControlScheme.ToString() },
                { "previous_session_unclean", previousUnclean }
            };

            provider.RecordEvent("session_started", parameters);
        }

        internal void RecordSessionEnded(bool cleanExit)
        {
            if (sessionEnded) return;
            sessionEnded = true;

            if (cleanExit)
            {
                PlayerPrefs.SetInt(SessionActiveKey, 0);
                PlayerPrefs.Save();
            }

            if (!IsDataCollectionEnabled) return;

            float durationSeconds = Mathf.Max(0f, Time.realtimeSinceStartup - sessionStartRealtime);
            var parameters = new Dictionary<string, object>
            {
                { "duration_seconds", durationSeconds },
                { "clean_exit", cleanExit }
            };

            provider.RecordEvent("session_ended", parameters, TimeSpan.FromSeconds(durationSeconds), immediate: true);
            provider.Flush();
        }

        internal void OnApplicationResumed()
        {
            sessionEnded = false;
            PlayerPrefs.SetInt(SessionActiveKey, 1);
            PlayerPrefs.Save();
        }

        internal void EnqueueThreadedError(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Assert)
                return;

            string sceneName = "unknown";
            pendingErrors.Enqueue((type.ToString(), condition ?? string.Empty, sceneName));
        }

        internal void DrainPendingErrors()
        {
            while (pendingErrors.TryDequeue(out var err))
            {
                string scene = SceneManager.GetActiveScene().name;
                TrackRuntimeError(err.type, err.message, string.IsNullOrEmpty(scene) ? err.scene : scene);
            }
        }

        public void TrackLevelStarted(int levelIndex, string levelName, int totalRings, float addedSpeed, bool godMode, bool isRetry)
        {
            if (!IsDataCollectionEnabled) return;

            var parameters = new Dictionary<string, object>
            {
                { "level_index", levelIndex },
                { "level_name", levelName ?? string.Empty },
                { "total_rings", totalRings },
                { "added_speed", addedSpeed },
                { "god_mode", godMode },
                { "is_retry", isRetry }
            };

            provider.RecordEvent("level_started", parameters);
        }

        public void TrackLevelCompleted(int levelIndex, string levelName, float elapsedSeconds, bool isNewBest, bool aceAchieved, int creditsEarned, bool godMode)
        {
            if (!IsDataCollectionEnabled) return;

            var parameters = new Dictionary<string, object>
            {
                { "level_index", levelIndex },
                { "level_name", levelName ?? string.Empty },
                { "elapsed_seconds", elapsedSeconds },
                { "is_new_best", isNewBest },
                { "ace_achieved", aceAchieved },
                { "credits_earned", creditsEarned },
                { "god_mode", godMode }
            };

            provider.RecordEvent("level_completed", parameters, TimeSpan.FromSeconds(Mathf.Max(0f, elapsedSeconds)), immediate: true);
        }

        public void TrackLevelFailed(int levelIndex, string levelName, float elapsedSeconds, int ringsPassed, int totalRings, int nextRingIndex, string crashObstacle, Vector3 crashPosition)
        {
            if (!IsDataCollectionEnabled) return;

            float progressPct = CalculateProgressPercent(ringsPassed, totalRings);
            var parameters = new Dictionary<string, object>
            {
                { "level_index", levelIndex },
                { "level_name", levelName ?? string.Empty },
                { "elapsed_seconds", elapsedSeconds },
                { "rings_passed", ringsPassed },
                { "total_rings", totalRings },
                { "progress_pct", progressPct },
                { "next_ring_index", nextRingIndex },
                { "crash_obstacle", string.IsNullOrEmpty(crashObstacle) ? "unknown" : crashObstacle },
                { "crash_pos_x", crashPosition.x },
                { "crash_pos_y", crashPosition.y },
                { "crash_pos_z", crashPosition.z }
            };

            provider.RecordEvent("level_failed", parameters, TimeSpan.FromSeconds(Mathf.Max(0f, elapsedSeconds)), immediate: true);
        }

        public void TrackLevelAbandoned(int levelIndex, string levelName, float elapsedSeconds, int ringsPassed, int totalRings, string fromState)
        {
            if (!IsDataCollectionEnabled) return;

            float progressPct = CalculateProgressPercent(ringsPassed, totalRings);
            var parameters = new Dictionary<string, object>
            {
                { "level_index", levelIndex },
                { "level_name", levelName ?? string.Empty },
                { "elapsed_seconds", elapsedSeconds },
                { "rings_passed", ringsPassed },
                { "total_rings", totalRings },
                { "progress_pct", progressPct },
                { "from_state", fromState ?? "unknown" }
            };

            provider.RecordEvent("level_abandoned", parameters, TimeSpan.FromSeconds(Mathf.Max(0f, elapsedSeconds)));
        }

        public void TrackInGamePurchase(string itemId, string currencyType, float amount, string currencyCode, int upgradeLevel, int balanceAfter)
        {
            if (!IsDataCollectionEnabled) return;

            var parameters = new Dictionary<string, object>
            {
                { "item_id", itemId ?? string.Empty },
                { "currency_type", currencyType ?? string.Empty },
                { "amount", amount },
                { "currency_code", currencyCode ?? string.Empty },
                { "upgrade_level", upgradeLevel },
                { "balance_after", balanceAfter }
            };

            provider.RecordEvent("in_game_purchase", parameters, immediate: true);
        }

        public void TrackCurrencyEarned(string source, int amount, int levelIndex, int balanceAfter)
        {
            if (!IsDataCollectionEnabled) return;

            var parameters = new Dictionary<string, object>
            {
                { "source", source ?? string.Empty },
                { "amount", amount },
                { "level_index", levelIndex },
                { "balance_after", balanceAfter }
            };

            provider.RecordEvent("currency_earned", parameters);
        }

        public void TrackAdInteraction(string adType, string placement, bool completed)
        {
            if (!IsDataCollectionEnabled) return;

            var parameters = new Dictionary<string, object>
            {
                { "ad_type", adType ?? string.Empty },
                { "placement", placement ?? string.Empty },
                { "completed", completed }
            };

            provider.RecordEvent("ad_interaction", parameters);
        }

        public void TrackRuntimeError(string errorType, string errorMessage, string sceneName)
        {
            if (!IsDataCollectionEnabled) return;
            if (errorsRecordedThisSession >= MaxRuntimeErrorsPerSession) return;

            string key = $"{errorType}:{errorMessage}";
            float now = Time.realtimeSinceStartup;
            if (recentErrorTimestamps.TryGetValue(key, out float lastTime) &&
                now - lastTime < ErrorDedupeWindowSeconds)
            {
                return;
            }

            recentErrorTimestamps[key] = now;
            errorsRecordedThisSession++;

            string truncated = string.IsNullOrEmpty(errorMessage)
                ? string.Empty
                : (errorMessage.Length > 256 ? errorMessage.Substring(0, 256) : errorMessage);

            var parameters = new Dictionary<string, object>
            {
                { "error_type", errorType ?? "Exception" },
                { "error_message", truncated },
                { "scene_name", sceneName ?? string.Empty }
            };

            provider.RecordEvent("runtime_error", parameters, immediate: true);
        }

        public void Flush()
        {
            provider.Flush();
        }

        public static float CalculateProgressPercent(int ringsPassed, int totalRings)
        {
            if (totalRings <= 0) return 0f;
            float pct = (float)ringsPassed / totalRings * 100f;
            return Mathf.Clamp(pct, 0f, 100f);
        }

        private sealed class AnalyticsLifecycleRunner : MonoBehaviour
        {
            private AnalyticsServiceManager owner;

            public void Bind(AnalyticsServiceManager manager)
            {
                owner = manager;
                Application.logMessageReceivedThreaded += HandleLog;
            }

            private void HandleLog(string condition, string stackTrace, LogType type)
            {
                owner?.EnqueueThreadedError(condition, stackTrace, type);
            }

            private void Update()
            {
                owner?.DrainPendingErrors();
            }

            private void OnApplicationPause(bool pauseStatus)
            {
                if (pauseStatus)
                    owner?.RecordSessionEnded(cleanExit: true);
                else
                    owner?.OnApplicationResumed();
            }

            private void OnApplicationQuit()
            {
                owner?.RecordSessionEnded(cleanExit: true);
            }

            private void OnDestroy()
            {
                Application.logMessageReceivedThreaded -= HandleLog;
            }
        }
    }
}
