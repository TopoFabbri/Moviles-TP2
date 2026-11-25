using System;
using System.Collections.Generic;
using System.Text;
using Game.CloudSave;
using Game.World.Ads;
using Game.World.GameManager;
using Game.World.IAP;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_ANDROID
using GooglePlayGames;
#endif

namespace Game.World.FabbriPlugin
{
    public class DiagnosticLogService : IDiagnosticLogService
    {
        private const float DeduplicateWindowSeconds = 3f;

        private readonly Dictionary<string, float> _recentKeys = new();
        private bool _isHooked;

        public event Action<string, string, string> OnFailureRecorded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoRegister()
        {
            if (ServiceProvider.TryGetService<IDiagnosticLogService>(out _))
                return;

            var service = new DiagnosticLogService();
            ServiceProvider.TryAddService<DiagnosticLogService>(service);
            ServiceProvider.TryAddService<IDiagnosticLogService>(service);
        }

        public DiagnosticLogService()
        {
            HookUnityLogs();
        }

        public void RecordFailure(string category, string action, string reason, string details = null)
        {
            string safeCategory = string.IsNullOrEmpty(category) ? "GENERAL" : category;
            string safeAction = string.IsNullOrEmpty(action) ? "UnknownAction" : action;
            string safeReason = string.IsNullOrEmpty(reason) ? "UnknownReason" : reason;

            string key = $"{safeCategory}|{safeAction}|{safeReason}";
            float now = Time.realtimeSinceStartup;
            if (_recentKeys.TryGetValue(key, out float lastTime) && (now - lastTime) < DeduplicateWindowSeconds)
            {
                return;
            }
            _recentKeys[key] = now;

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string entry = string.IsNullOrEmpty(details)
                ? $"[{timestamp}] [FAIL] [{safeCategory}] {safeAction}: {safeReason}"
                : $"[{timestamp}] [FAIL] [{safeCategory}] {safeAction}: {safeReason} | {details}";

            WriteEntry(entry);
            OnFailureRecorded?.Invoke(safeCategory, safeAction, safeReason);
        }

        public void RecordInfo(string category, string message)
        {
            string safeCategory = string.IsNullOrEmpty(category) ? "GENERAL" : category;
            string safeMessage = string.IsNullOrEmpty(message) ? string.Empty : message;
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            WriteEntry($"[{timestamp}] [INFO] [{safeCategory}] {safeMessage}");
        }

        public string BuildDiagnosticReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== DRONERACER DIAGNOSTIC REPORT ===");
            sb.AppendLine($"Generated (UTC): {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
            sb.AppendLine($"App Version: {Application.version} | Unity: {Application.unityVersion} | Platform: {Application.platform}");
            sb.AppendLine($"Device: {SystemInfo.deviceModel} | OS: {SystemInfo.operatingSystem}");
            sb.AppendLine($"RAM: {SystemInfo.systemMemorySize} MB | GPU: {SystemInfo.graphicsDeviceName}");
            sb.AppendLine($"Active Scene: {SceneManager.GetActiveScene().name}");
            sb.AppendLine("--- SERVICES & SAVE STATE ---");
            sb.AppendLine($"GPGS Authenticated: {GetGpgsStatus()} | ActivePlayerId: {GameSave.ActivePlayerId}");
            sb.AppendLine($"CloudSave Instance: {CloudSaveService.HasInstance} | PendingCloudData: {(CloudSaveService.HasInstance && CloudSaveService.Instance.PendingCloudData != null)} | CloudDecisionMade: {GameSave.HasCloudDecisionBeenMade}");

            if (ServiceProvider.TryGetService<IAdService>(out var adService))
            {
                sb.AppendLine($"Ads: Enabled={GameSave.AdsEnabled}, HasNoAds={GameSave.HasNoAds}, IsAdShowing={adService.IsAdShowing}, RewardedCooldown={adService.RewardedCooldownRemaining:F1}s");
            }
            else
            {
                sb.AppendLine($"Ads: ServiceNotRegistered | Enabled={GameSave.AdsEnabled}, HasNoAds={GameSave.HasNoAds}");
            }

            if (ServiceProvider.TryGetService<IIAPService>(out var iapService))
            {
                sb.AppendLine($"IAP: Initialized={iapService.IsInitialized}, NoAdsPrice={iapService.NoAdsPriceString}");
            }
            else
            {
                sb.AppendLine("IAP: ServiceNotRegistered");
            }

            if (ServiceProvider.TryGetService<Game.World.Analytics.IAnalyticsService>(out var analyticsService))
            {
                sb.AppendLine($"Analytics: Initialized={analyticsService.IsInitialized}, Enabled={analyticsService.IsDataCollectionEnabled}");
            }
            else
            {
                sb.AppendLine($"Analytics: ServiceNotRegistered | Enabled={GameSave.AnalyticsEnabled}");
            }

            sb.AppendLine($"Save: Level={GameSave.CurrentLevel}, Credits={GameSave.Credits}, AddedSpeed={GameSave.AddedSpeed}, GodMode={GameSave.GodMode}, Control={GameSave.ControlScheme}, Gyro={GameSave.GyroMode}");
            sb.AppendLine("=== LOGS ===");

            string logs = ServiceProvider.GetService<FileManager>().ReadFile();
            sb.Append(string.IsNullOrEmpty(logs) ? "(no log entries recorded)" : logs);
            return sb.ToString();
        }

        public static string FormatRichText(string plainReport)
        {
            if (string.IsNullOrEmpty(plainReport))
                return string.Empty;

            var lines = plainReport.Replace("\r\n", "\n").Split('\n');
            var sb = new StringBuilder(plainReport.Length * 2);
            bool inLogsSection = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (i > 0) sb.Append('\n');

                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith("==="))
                {
                    inLogsSection = line.Contains("LOGS");
                    sb.Append($"<b><color=#4FC3F7>{line}</color></b>");
                    continue;
                }

                if (line.StartsWith("---"))
                {
                    sb.Append($"<b><color=#FFD54F>{line}</color></b>");
                    continue;
                }

                if (!inLogsSection)
                {
                    int colonIndex = line.IndexOf(':');
                    if (colonIndex > 0)
                    {
                        string label = line.Substring(0, colonIndex + 1);
                        string value = line.Substring(colonIndex + 1);
                        sb.Append($"<indent=16><color=#90CAF9><b>{label}</b></color>{value}</indent>");
                    }
                    else
                    {
                        sb.Append($"<indent=16>{line}</indent>");
                    }
                    continue;
                }

                if (line == "(no log entries recorded)")
                {
                    sb.Append("<indent=16><color=#9E9E9E><i>(no log entries recorded)</i></color></indent>");
                    continue;
                }

                if (TryFormatLogLine(line, out string formattedLog))
                {
                    sb.Append($"<indent=12>{formattedLog}</indent>");
                }
                else
                {
                    sb.Append($"<indent=24><color=#B0BEC5>{line}</color></indent>");
                }
            }

            return sb.ToString();
        }

        private static bool TryFormatLogLine(string line, out string formatted)
        {
            formatted = null;
            if (!line.StartsWith("[")) return false;

            int tsEnd = line.IndexOf(']');
            if (tsEnd <= 1) return false;

            string timestamp = line.Substring(0, tsEnd + 1);
            string rest = line.Substring(tsEnd + 1).TrimStart();

            string levelTag;
            string levelColor;
            if (rest.StartsWith("[FAIL]"))
            {
                levelTag = "[FAIL]";
                levelColor = "#FF5252";
            }
            else if (rest.StartsWith("[INFO]"))
            {
                levelTag = "[INFO]";
                levelColor = "#69F0AE";
            }
            else
            {
                return false;
            }

            rest = rest.Substring(levelTag.Length).TrimStart();

            string categoryPart = string.Empty;
            if (rest.StartsWith("["))
            {
                int catEnd = rest.IndexOf(']');
                if (catEnd > 1)
                {
                    string cat = rest.Substring(0, catEnd + 1);
                    categoryPart = $"<color=#FFB74D>{cat}</color> ";
                    rest = rest.Substring(catEnd + 1).TrimStart();
                }
            }

            formatted = $"<color=#9E9E9E>{timestamp}</color>\t<b><color={levelColor}>{levelTag}</color></b>\t{categoryPart}{rest}";
            return true;
        }

        public void ShareReport(string subject = "DroneRacer Diagnostic Report")
        {
            string report = BuildDiagnosticReport();
            GUIUtility.systemCopyBuffer = report;

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var intentClass = new AndroidJavaClass("android.content.Intent");
                using var intent = new AndroidJavaObject("android.content.Intent", intentClass.GetStatic<string>("ACTION_SEND"));
                intent.Call<AndroidJavaObject>("setType", "text/plain");
                intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subject);
                intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), report);

                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intent, subject);
                currentActivity.Call("startActivity", chooser);
            }
            catch (Exception ex)
            {
                RecordFailure("DIAGNOSTICS", "ShareReport", ex.Message);
            }
#else
            Debug.Log($"[DiagnosticLogService] Report copied to clipboard ({report.Length} chars).");
#endif
        }

        public void ClearLogs()
        {
            _recentKeys.Clear();
            ServiceProvider.GetService<FileManager>().ClearFileImmediate();
        }

        private void HookUnityLogs()
        {
            if (_isHooked) return;
            _isHooked = true;
            Application.logMessageReceivedThreaded += OnUnityLogReceived;
        }

        private void OnUnityLogReceived(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
                return;

            string firstLine = string.IsNullOrEmpty(stackTrace)
                ? string.Empty
                : stackTrace.Split('\n')[0].Trim();

            RecordFailure("UNITY", type.ToString(), condition, firstLine);
        }

        private static string GetGpgsStatus()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                return PlayGamesPlatform.Instance != null && PlayGamesPlatform.Instance.IsAuthenticated()
                    ? "True"
                    : "False";
            }
            catch (Exception ex)
            {
                return $"Error ({ex.Message})";
            }
#else
            return "N/A (Editor/Non-Android)";
#endif
        }

        private static void WriteEntry(string entry)
        {
            ServiceProvider.GetService<FileManager>().WriteFile(entry);
        }
    }
}
