using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UnityConsent;

namespace Game.World.Analytics
{
    public sealed class UnityAnalyticsProvider : IAnalyticsProvider
    {
        private bool consentGranted = true;

        public string Name => "UnityAnalytics";
        public bool IsInitialized { get; private set; }

        public async Task InitializeAsync()
        {
            if (IsInitialized) return;

            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            ApplyConsent(consentGranted);
            IsInitialized = true;
        }

        public void SetConsent(bool granted)
        {
            consentGranted = granted;
            if (IsInitialized)
            {
                ApplyConsent(granted);
            }
        }

        private static void ApplyConsent(bool granted)
        {
            var status = granted ? ConsentStatus.Granted : ConsentStatus.Denied;
            EndUserConsent.SetConsentState(new ConsentState
            {
                AnalyticsIntent = status
            });
        }

        public void RecordEvent(string eventName, IDictionary<string, object> parameters, TimeSpan? duration = null, bool immediate = false)
        {
            if (!IsInitialized || !consentGranted || string.IsNullOrEmpty(eventName))
                return;

            var customEvent = new CustomEvent(eventName);
            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    if (string.IsNullOrEmpty(kvp.Key) || kvp.Value == null) continue;
                    customEvent[kvp.Key] = kvp.Value;
                }
            }

            if (duration.HasValue && duration.Value > TimeSpan.Zero)
            {
                customEvent["duration_ms"] = (long)duration.Value.TotalMilliseconds;
            }

            AnalyticsService.Instance.RecordEvent(customEvent);

            if (immediate)
            {
                AnalyticsService.Instance.Flush();
            }
        }

        public void Flush()
        {
            if (!IsInitialized || !consentGranted)
                return;

            AnalyticsService.Instance.Flush();
        }
    }
}
