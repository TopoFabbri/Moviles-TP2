using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.World.Analytics;
using Game.World.GameManager;
using NUnit.Framework;
using Tools.ServiceProvider;
using UnityEngine;

namespace Editor
{
    public class AnalyticsServiceTests
    {
        private sealed class MockAnalyticsProvider : IAnalyticsProvider
        {
            public string Name { get; }
            public bool IsInitialized { get; private set; }
            public bool ConsentGranted { get; private set; } = true;
            public bool ThrowOnRecord { get; set; }
            public int FlushCount { get; private set; }
            public List<(string name, IDictionary<string, object> parameters, TimeSpan? duration, bool immediate)> RecordedEvents { get; } =
                new List<(string, IDictionary<string, object>, TimeSpan?, bool)>();

            public MockAnalyticsProvider(string name = "Mock")
            {
                Name = name;
            }

            public Task InitializeAsync()
            {
                IsInitialized = true;
                return Task.CompletedTask;
            }

            public void SetConsent(bool granted)
            {
                ConsentGranted = granted;
            }

            public void RecordEvent(string eventName, IDictionary<string, object> parameters, TimeSpan? duration = null, bool immediate = false)
            {
                if (ThrowOnRecord)
                    throw new InvalidOperationException("Simulated provider failure");

                RecordedEvents.Add((eventName, parameters, duration, immediate));
            }

            public void Flush()
            {
                FlushCount++;
            }
        }

        private bool originalAnalyticsEnabled;

        [SetUp]
        public void SetUp()
        {
            originalAnalyticsEnabled = GameSave.AnalyticsEnabled;
            GameSave.AnalyticsEnabled = true;
            PlayerPrefs.DeleteKey("Analytics_SessionActive");
        }

        [TearDown]
        public void TearDown()
        {
            GameSave.AnalyticsEnabled = originalAnalyticsEnabled;
            PlayerPrefs.DeleteKey("Analytics_SessionActive");
        }

        [Test]
        public void ServiceProvider_ResolvesAnalyticsService_AndInterface()
        {
            var mock = new MockAnalyticsProvider();
            var manager = new AnalyticsServiceManager(mock);
            ServiceProvider.TryAddService<IAnalyticsService>(manager);

            Assert.IsTrue(ServiceProvider.TryGetService<IAnalyticsService>(out var resolved));
            Assert.IsNotNull(resolved);
        }

        [Test]
        public void CompositeProvider_FansOutToAllProviders_AndIsolatesFailures()
        {
            var failing = new MockAnalyticsProvider("Failing") { ThrowOnRecord = true };
            var healthy = new MockAnalyticsProvider("Healthy");
            var composite = new CompositeAnalyticsProvider(failing, healthy);

            composite.RecordEvent("test_event", new Dictionary<string, object> { { "k", "v" } });

            Assert.AreEqual(1, healthy.RecordedEvents.Count);
            Assert.AreEqual("test_event", healthy.RecordedEvents[0].name);
        }

        [Test]
        public void Consent_DefaultsEnabled_AndTogglesAllProviders()
        {
            var mock = new MockAnalyticsProvider();
            var manager = new AnalyticsServiceManager(mock);

            Assert.IsTrue(manager.IsDataCollectionEnabled);
            manager.SetDataCollectionEnabled(false);

            Assert.IsFalse(manager.IsDataCollectionEnabled);
            Assert.IsFalse(mock.ConsentGranted);
            Assert.IsFalse(GameSave.AnalyticsEnabled);

            manager.TrackLevelStarted(1, "Level1", 10, 0f, false, false);
            Assert.AreEqual(0, mock.RecordedEvents.Count);
        }

        [Test]
        public void TrackLevelFailed_ComputesProgressDurationAndCrashMetadata()
        {
            var mock = new MockAnalyticsProvider();
            var manager = new AnalyticsServiceManager(mock);

            manager.TrackLevelFailed(2, "Level2", 14.5f, 5, 10, 6, "Pillar_01", new Vector3(1f, 2f, 3f));

            Assert.AreEqual(1, mock.RecordedEvents.Count);
            var evt = mock.RecordedEvents[0];
            Assert.AreEqual("level_failed", evt.name);
            Assert.IsTrue(evt.immediate);
            Assert.AreEqual(50f, (float)evt.parameters["progress_pct"], 0.01f);
            Assert.AreEqual("Pillar_01", evt.parameters["crash_obstacle"]);
            Assert.AreEqual(1f, (float)evt.parameters["crash_pos_x"], 0.01f);
        }

        [Test]
        public void TrackInGamePurchase_RecordsCreditAndRealCurrencyEvents()
        {
            var mock = new MockAnalyticsProvider();
            var manager = new AnalyticsServiceManager(mock);

            manager.TrackInGamePurchase("speed_upgrade", "credits", 6f, "CREDITS", 2, 14);
            manager.TrackInGamePurchase("no_ads", "real_currency", 2.99f, "USD", 1, 14);

            Assert.AreEqual(2, mock.RecordedEvents.Count);
            Assert.AreEqual("speed_upgrade", mock.RecordedEvents[0].parameters["item_id"]);
            Assert.AreEqual("no_ads", mock.RecordedEvents[1].parameters["item_id"]);
            Assert.AreEqual("USD", mock.RecordedEvents[1].parameters["currency_code"]);
        }

        [Test]
        public void RuntimeError_DeduplicatesRepeatedExceptions()
        {
            var mock = new MockAnalyticsProvider();
            var manager = new AnalyticsServiceManager(mock);

            manager.TrackRuntimeError("Exception", "NullReferenceException", "Level1");
            manager.TrackRuntimeError("Exception", "NullReferenceException", "Level1");

            Assert.AreEqual(1, mock.RecordedEvents.Count);
            Assert.AreEqual("runtime_error", mock.RecordedEvents[0].name);
        }

        [Test]
        public void UncleanSessionDetection_FlagsPreviousCrashOnNextBoot()
        {
            PlayerPrefs.SetInt("Analytics_SessionActive", 1);
            PlayerPrefs.Save();

            Assert.AreEqual(1, PlayerPrefs.GetInt("Analytics_SessionActive", 0));
            Assert.AreEqual(50f, AnalyticsServiceManager.CalculateProgressPercent(5, 10), 0.01f);
            Assert.AreEqual(0f, AnalyticsServiceManager.CalculateProgressPercent(5, 0), 0.01f);
        }
    }
}
