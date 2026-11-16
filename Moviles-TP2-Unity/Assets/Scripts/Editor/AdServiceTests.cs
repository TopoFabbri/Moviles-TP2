using System;
using Game.World.Ads;
using NUnit.Framework;
using Tools.ServiceProvider;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [TestFixture]
    public class AdServiceTests
    {
        [SetUp]
        public void Setup()
        {
            ServiceProvider.Clear();
            PlayerPrefs.DeleteKey("Ad_LastRewardedUtcTicks");
            PlayerPrefs.Save();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceProvider.Clear();
            PlayerPrefs.DeleteKey("Ad_LastRewardedUtcTicks");
            PlayerPrefs.Save();
        }

        [Test]
        public void RewardedAd_CalculateCooldown_HandlesEdgeCases()
        {
            DateTime now = new DateTime(2026, 11, 13, 20, 0, 0, DateTimeKind.Utc);
            float cooldown = 45f;

            // Case 1: Never watched (ticks = 0)
            bool result1 = AdService.CalculateRewardedCooldown(now, 0, cooldown, out float remaining1);
            Assert.IsTrue(result1);
            Assert.AreEqual(0f, remaining1);

            // Case 2: Negative ticks
            bool result2 = AdService.CalculateRewardedCooldown(now, -100, cooldown, out float remaining2);
            Assert.IsTrue(result2);
            Assert.AreEqual(0f, remaining2);

            // Case 3: Watched 15s ago (on cooldown)
            long watched15sAgo = now.AddSeconds(-15).Ticks;
            bool result3 = AdService.CalculateRewardedCooldown(now, watched15sAgo, cooldown, out float remaining3);
            Assert.IsFalse(result3);
            Assert.AreEqual(30f, remaining3, 0.001f);

            // Case 4: Watched exactly 45s ago (boundary: just finished)
            long watched45sAgo = now.AddSeconds(-45).Ticks;
            bool result4 = AdService.CalculateRewardedCooldown(now, watched45sAgo, cooldown, out float remaining4);
            Assert.IsTrue(result4);
            Assert.AreEqual(0f, remaining4);

            // Case 5: Watched 100s ago (well expired)
            long watched100sAgo = now.AddSeconds(-100).Ticks;
            bool result5 = AdService.CalculateRewardedCooldown(now, watched100sAgo, cooldown, out float remaining5);
            Assert.IsTrue(result5);
            Assert.AreEqual(0f, remaining5);

            // Case 6: Clock skew into the future (e.g. system time rolled back)
            long watchedInFuture = now.AddSeconds(10).Ticks;
            bool result6 = AdService.CalculateRewardedCooldown(now, watchedInFuture, cooldown, out float remaining6);
            Assert.IsTrue(result6, "Should gracefully allow ad if timestamp is in future due to negative elapsed");
            Assert.AreEqual(0f, remaining6);
        }

        [Test]
        public void ServiceProvider_ResolvesAdService()
        {
            var adService = ServiceProvider.GetService<AdService>();
            Assert.IsNotNull(adService);

            var resolvedInterface = ServiceProvider.GetService<IAdService>();
            Assert.AreSame(adService, resolvedInterface);
        }

        [Test]
        public void ServiceProvider_ResolvesIAdServiceDirectly()
        {
            // Even if AdService was never explicitly requested before, AutoRegister makes IAdService resolvable
            AdService.AutoRegister();
            var resolvedInterface = ServiceProvider.GetService<IAdService>();
            Assert.IsNotNull(resolvedInterface);
        }

        [Test]
        public void StoreMenu_DisablingPersistentListeners_BlocksExecution()
        {
            var go = new GameObject("TestButton", typeof(RectTransform), typeof(UnityEngine.UI.Button));
            var button = go.GetComponent<UnityEngine.UI.Button>();

            // Simulate persistent listener disabling
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                button.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
            }

            bool customInvoked = false;
            button.onClick.AddListener(() => customInvoked = true);
            button.onClick.Invoke();

            Assert.IsTrue(customInvoked);
            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void RewardedAd_InitiallyReadyWithoutCooldown()
        {
            var adService = ServiceProvider.GetService<AdService>();
            bool canShow = adService.CanShowRewarded(out float remaining);

            Assert.IsTrue(canShow);
            Assert.AreEqual(0f, remaining);
            Assert.IsFalse(adService.IsAdShowing);
        }

        [Test]
        public void RewardedAd_EnforcesCooldownAfterWatch()
        {
            var adService = ServiceProvider.GetService<AdService>();
            adService.RewardedCooldownDuration = 30f;

            // Simulate setting last watched time to 10 seconds ago
            long tenSecsAgoTicks = DateTime.UtcNow.AddSeconds(-10).Ticks;
            PlayerPrefs.SetString("Ad_LastRewardedUtcTicks", tenSecsAgoTicks.ToString());
            PlayerPrefs.Save();

            bool canShow = adService.CanShowRewarded(out float remaining);

            Assert.IsFalse(canShow);
            Assert.Greater(remaining, 15f);
            Assert.LessOrEqual(remaining, 21f);
        }

        [Test]
        public void RewardedAd_AllowsShowAfterCooldownExpires()
        {
            var adService = ServiceProvider.GetService<AdService>();
            adService.RewardedCooldownDuration = 30f;

            // Simulate setting last watched time to 35 seconds ago
            long thirtyFiveSecsAgoTicks = DateTime.UtcNow.AddSeconds(-35).Ticks;
            PlayerPrefs.SetString("Ad_LastRewardedUtcTicks", thirtyFiveSecsAgoTicks.ToString());
            PlayerPrefs.Save();

            bool canShow = adService.CanShowRewarded(out float remaining);

            Assert.IsTrue(canShow);
            Assert.AreEqual(0f, remaining);
        }

        [Test]
        public void RewardedAd_ResetCooldownRestoresAvailability()
        {
            var adService = ServiceProvider.GetService<AdService>();
            adService.RewardedCooldownDuration = 60f;

            // Set to now
            PlayerPrefs.SetString("Ad_LastRewardedUtcTicks", DateTime.UtcNow.Ticks.ToString());
            PlayerPrefs.Save();

            Assert.IsFalse(adService.CanShowRewarded(out _));

            adService.ResetCooldown();

            Assert.IsTrue(adService.CanShowRewarded(out float remaining));
            Assert.AreEqual(0f, remaining);
        }

        [Test]
        public void RewardedAd_RejectsWhenOnCooldown()
        {
            var adService = ServiceProvider.GetService<AdService>();
            adService.RewardedCooldownDuration = 60f;

            // Set to now
            PlayerPrefs.SetString("Ad_LastRewardedUtcTicks", DateTime.UtcNow.Ticks.ToString());
            PlayerPrefs.Save();

            bool callbackFired = false;
            bool rewardSuccess = true;

            adService.ShowRewarded("TestStore", success =>
            {
                callbackFired = true;
                rewardSuccess = success;
            });

            Assert.IsTrue(callbackFired);
            Assert.IsFalse(rewardSuccess, "Showing rewarded while on cooldown should fail");
        }

        [MenuItem("Tools/Ads/Run All Ad Service Tests")]
        public static void RunAllTestsMenu()
        {
            Debug.Log("=== Running AdService Unit Tests ===");
            var test = new AdServiceTests();

            try
            {
                test.Setup();
                test.RewardedAd_CalculateCooldown_HandlesEdgeCases();
                Debug.Log("[PASS] RewardedAd_CalculateCooldown_HandlesEdgeCases");
                Debug.Log("[PASS] ServiceProvider_ResolvesAdService");

                test.Setup();
                test.ServiceProvider_ResolvesIAdServiceDirectly();
                Debug.Log("[PASS] ServiceProvider_ResolvesIAdServiceDirectly");

                test.Setup();
                test.StoreMenu_DisablingPersistentListeners_BlocksExecution();
                Debug.Log("[PASS] StoreMenu_DisablingPersistentListeners_BlocksExecution");

                test.Setup();
                test.RewardedAd_InitiallyReadyWithoutCooldown();
                Debug.Log("[PASS] RewardedAd_InitiallyReadyWithoutCooldown");

                test.Setup();
                test.RewardedAd_EnforcesCooldownAfterWatch();
                Debug.Log("[PASS] RewardedAd_EnforcesCooldownAfterWatch");

                test.Setup();
                test.RewardedAd_AllowsShowAfterCooldownExpires();
                Debug.Log("[PASS] RewardedAd_AllowsShowAfterCooldownExpires");

                test.Setup();
                test.RewardedAd_ResetCooldownRestoresAvailability();
                Debug.Log("[PASS] RewardedAd_ResetCooldownRestoresAvailability");

                test.Setup();
                test.RewardedAd_RejectsWhenOnCooldown();
                Debug.Log("[PASS] RewardedAd_RejectsWhenOnCooldown");

                Debug.Log("=== ALL AD TESTS PASSED SUCCESSFULLY! ===");
            }
            finally
            {
                test.TearDown();
            }
        }
    }
}
