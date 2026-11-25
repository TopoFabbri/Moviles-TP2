using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.World.Analytics
{
    public sealed class CompositeAnalyticsProvider : IAnalyticsProvider
    {
        private readonly List<IAnalyticsProvider> providers = new List<IAnalyticsProvider>();

        public string Name => "Composite";
        public bool IsInitialized { get; private set; }

        public CompositeAnalyticsProvider(params IAnalyticsProvider[] providers)
        {
            if (providers == null) return;
            foreach (var p in providers)
            {
                if (p != null)
                    this.providers.Add(p);
            }
        }

        public async Task InitializeAsync()
        {
            foreach (var provider in providers)
            {
                try
                {
                    await provider.InitializeAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Analytics] Provider '{provider.Name}' failed to initialize: {ex.Message}");
                }
            }

            IsInitialized = true;
        }

        public void SetConsent(bool granted)
        {
            foreach (var provider in providers)
            {
                try
                {
                    provider.SetConsent(granted);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Analytics] Provider '{provider.Name}' SetConsent failed: {ex.Message}");
                }
            }
        }

        public void RecordEvent(string eventName, IDictionary<string, object> parameters, TimeSpan? duration = null, bool immediate = false)
        {
            foreach (var provider in providers)
            {
                try
                {
                    provider.RecordEvent(eventName, parameters, duration, immediate);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Analytics] Provider '{provider.Name}' RecordEvent({eventName}) failed: {ex.Message}");
                }
            }
        }

        public void Flush()
        {
            foreach (var provider in providers)
            {
                try
                {
                    provider.Flush();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Analytics] Provider '{provider.Name}' Flush failed: {ex.Message}");
                }
            }
        }
    }
}
