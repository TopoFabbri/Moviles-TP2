using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

namespace Game.World.Analytics
{
    public sealed class GooglePlayGamesAnalyticsProvider : IAnalyticsProvider
    {
        private bool consentGranted = true;

        public string Name => "GooglePlayGames";
        public bool IsInitialized { get; private set; }

        public Task InitializeAsync()
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        public void SetConsent(bool granted)
        {
            consentGranted = granted;
        }

        public void RecordEvent(string eventName, IDictionary<string, object> parameters, TimeSpan? duration = null, bool immediate = false)
        {
            if (!consentGranted || string.IsNullOrEmpty(eventName))
                return;

#if UNITY_ANDROID && !UNITY_EDITOR
            var platform = PlayGamesPlatform.Instance;
            if (platform == null || !platform.IsAuthenticated())
                return;

            var builder = new PlayerGameEvent.Builder(eventName);
            if (parameters != null)
            {
                int propCount = 0;
                foreach (var kvp in parameters)
                {
                    if (propCount >= 25) break;
                    if (string.IsNullOrEmpty(kvp.Key) || kvp.Value == null) continue;

                    switch (kvp.Value)
                    {
                        case string s:
                            builder.AddProperty(kvp.Key, s.Length > 1024 ? s.Substring(0, 1024) : s);
                            propCount++;
                            break;
                        case int i:
                            builder.AddProperty(kvp.Key, i);
                            propCount++;
                            break;
                        case long l:
                            builder.AddProperty(kvp.Key, l);
                            propCount++;
                            break;
                        case float f:
                            builder.AddProperty(kvp.Key, f);
                            propCount++;
                            break;
                        case double d:
                            builder.AddProperty(kvp.Key, d);
                            propCount++;
                            break;
                        case bool b:
                            builder.AddProperty(kvp.Key, b);
                            propCount++;
                            break;
                    }
                }
            }

            if (duration.HasValue && duration.Value > TimeSpan.Zero)
            {
                builder.AddProperty("duration_ms", (long)duration.Value.TotalMilliseconds);
            }

            var gameEvent = builder.Build();
            if (immediate)
            {
                platform.RecordEventImmediate(gameEvent);
            }
            else
            {
                platform.RecordEvent(gameEvent);
            }
#endif
        }

        public void Flush()
        {
            if (!consentGranted)
                return;

#if UNITY_ANDROID && !UNITY_EDITOR
            var platform = PlayGamesPlatform.Instance;
            if (platform != null && platform.IsAuthenticated())
            {
                platform.RequestEventsUpload();
            }
#endif
        }
    }
}
