using Game.CloudSave;
using Game.World.GameManager;
using Game.World.UI;
using NUnit.Framework;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.UI;

namespace Editor
{
    [TestFixture]
    public class GameSaveAccountSwitchTests
    {
        private const string AccountA = "test_account_A";
        private const string AccountB = "test_account_B";
        private const string UnrelatedKey = "Ad_LastRewardedUtcTicks";

        [SetUp]
        public void SetUp()
        {
            ServiceProvider.Clear();
            CleanTestAccounts();
        }

        [TearDown]
        public void TearDown()
        {
            CleanTestAccounts();
            ServiceProvider.Clear();
        }

        private static void CleanTestAccounts()
        {
            GameSave.SetActiveAccount(AccountA);
            GameSave.Reset();
            GameSave.SetActiveAccount(AccountB);
            GameSave.Reset();
            GameSave.SetActiveAccount(null);
            GameSave.Reset();
            PlayerPrefs.DeleteKey(UnrelatedKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void StartupRaceCondition_SetActiveAccountAfterEarlyRead_SwitchesKeysAndReloads()
        {
            // Simulate early read before GPGS finishes authenticating
            Assert.AreEqual("defaultPlayer", GameSave.ActivePlayerId);
            GameSave.Credits = 999;

            // GPGS finishes authenticating as AccountA
            bool changed = GameSave.SetActiveAccount(AccountA);
            Assert.IsTrue(changed);
            Assert.AreEqual(AccountA, GameSave.ActivePlayerId);
            Assert.AreEqual(0, GameSave.Credits, "AccountA should not inherit defaultPlayer credits.");
        }

        [Test]
        public void AccountSwitch_IsolatesSettingsAndProgress()
        {
            GameSave.SetActiveAccount(AccountA);
            GameSave.RotationSensitivity = 250f;
            GameSave.GyroMode = GyroMode.FpvAcro;
            GameSave.InvertCamX = true;
            GameSave.InvertCamY = false;
            GameSave.GodMode = true;
            GameSave.ControlScheme = 1;
            GameSave.Credits = 500;
            GameSave.AddedSpeed = 12f;
            GameSave.CurrentLevel = 3;
            GameSave.HasNoAds = true;
            GameSave.AdsEnabled = false;

            // Switch to AccountB
            GameSave.SetActiveAccount(AccountB);
            Assert.AreEqual(GameSave.DefaultRotationSensitivity, GameSave.RotationSensitivity);
            Assert.AreEqual(GyroMode.BankToTurn, GameSave.GyroMode);
            Assert.IsFalse(GameSave.InvertCamX);
            Assert.IsTrue(GameSave.InvertCamY);
            Assert.IsFalse(GameSave.GodMode);
            Assert.AreEqual(-1, GameSave.ControlScheme);
            Assert.AreEqual(0, GameSave.Credits);
            Assert.AreEqual(0f, GameSave.AddedSpeed);
            Assert.AreEqual(0, GameSave.CurrentLevel);
            Assert.IsFalse(GameSave.HasNoAds);
            Assert.IsTrue(GameSave.AdsEnabled);

            // Switch back to AccountA and verify values are restored
            GameSave.SetActiveAccount(AccountA);
            Assert.AreEqual(250f, GameSave.RotationSensitivity);
            Assert.AreEqual(GyroMode.FpvAcro, GameSave.GyroMode);
            Assert.IsTrue(GameSave.InvertCamX);
            Assert.IsFalse(GameSave.InvertCamY);
            Assert.IsTrue(GameSave.GodMode);
            Assert.AreEqual(1, GameSave.ControlScheme);
            Assert.AreEqual(500, GameSave.Credits);
            Assert.AreEqual(12f, GameSave.AddedSpeed);
            Assert.AreEqual(3, GameSave.CurrentLevel);
            Assert.IsTrue(GameSave.HasNoAds);
            Assert.IsFalse(GameSave.AdsEnabled);
        }

        [Test]
        public void CloudMetadata_IsScopedPerAccount()
        {
            GameSave.SetActiveAccount(AccountA);
            GameSave.HasCloudDecisionBeenMade = true;
            GameSave.LastSavedUtcTicks = 123456789L;

            GameSave.SetActiveAccount(AccountB);
            Assert.IsFalse(GameSave.HasCloudDecisionBeenMade, "AccountB must not inherit AccountA's cloud decision.");
            Assert.AreEqual(0L, GameSave.LastSavedUtcTicks, "AccountB must not inherit AccountA's LastSavedUtcTicks.");
        }

        [Test]
        public void Reset_OnlyWipesActiveAccount_PreservesOtherAccountsAndUnrelatedPrefs()
        {
            PlayerPrefs.SetString(UnrelatedKey, "99999");
            PlayerPrefs.Save();

            GameSave.SetActiveAccount(AccountA);
            GameSave.Credits = 250;

            GameSave.SetActiveAccount(AccountB);
            GameSave.Credits = 750;

            // Reset AccountB
            GameSave.Reset();
            Assert.AreEqual(0, GameSave.Credits);

            // Unrelated key must remain intact
            Assert.AreEqual("99999", PlayerPrefs.GetString(UnrelatedKey, ""));

            // AccountA must remain intact
            GameSave.SetActiveAccount(AccountA);
            Assert.AreEqual(250, GameSave.Credits);
        }

        [Test]
        public void LevelTimes_AreIsolatedBetweenNormalAndGodMode()
        {
            GameSave.SetActiveAccount(AccountA);

            GameSave.GodMode = false;
            GameSave.SetLevelTime(0, 42.5f);

            GameSave.GodMode = true;
            GameSave.SetLevelTime(0, 15.0f);

            GameSave.GodMode = false;
            Assert.AreEqual(42.5f, GameSave.GetLevelTime(0), 0.001f);

            GameSave.GodMode = true;
            Assert.AreEqual(15.0f, GameSave.GetLevelTime(0), 0.001f);
        }

        [Test]
        public void GameplayMenuController_RefreshUI_DoesNotTriggerOnValueChanged_AndReactsToAccountSwitch()
        {
            var root = new GameObject("GameplayMenuTest");
            try
            {
                var sliderGo = new GameObject("Slider", typeof(Slider));
                sliderGo.transform.SetParent(root.transform);
                var slider = sliderGo.GetComponent<Slider>();
                slider.minValue = 10f;
                slider.maxValue = 300f;

                var toggleGo = new GameObject("GodModeToggle", typeof(Toggle));
                toggleGo.transform.SetParent(root.transform);
                var toggle = toggleGo.GetComponent<Toggle>();

                var controller = root.AddComponent<GameplayMenuController>();
                var type = typeof(GameplayMenuController);
                type.GetField("rotationSensitivitySlider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, slider);
                type.GetField("godModeToggle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, toggle);

                int callbackCount = 0;
                slider.onValueChanged.AddListener(_ => callbackCount++);
                toggle.onValueChanged.AddListener(_ => callbackCount++);

                GameSave.SetActiveAccount(AccountA);
                GameSave.RotationSensitivity = 220f;
                GameSave.GodMode = true;

                controller.RefreshUI();
                Assert.AreEqual(0, callbackCount, "RefreshUI must use SetValueWithoutNotify / SetIsOnWithoutNotify.");
                Assert.AreEqual(220f, slider.value, 0.001f);
                Assert.IsTrue(toggle.isOn);

                // Switching account fires OnSaveDataChanged and updates UI if enabled
                GameSave.SetActiveAccount(AccountB);
                controller.RefreshUI();
                Assert.AreEqual(GameSave.DefaultRotationSensitivity, slider.value, 0.001f);
                Assert.IsFalse(toggle.isOn);
                Assert.AreEqual(0, callbackCount);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
