using System.Collections.Generic;
using Game.World.GameManager;
using Game.World.Graphics;
using NUnit.Framework;
using Tools.ServiceProvider;

namespace Editor.Graphics
{
    [TestFixture]
    public class GraphicsSettingsTests
    {
        private sealed class FakeGraphicsApplier : IGraphicsApplier
        {
            public readonly List<GraphicsSettingsData> AppliedAll = new();
            public readonly List<GraphicsSettingsData> AppliedScene = new();
            public int RestoreCount;

            public void ApplyAll(GraphicsSettingsData data) => AppliedAll.Add(data);
            public void ApplySceneObjects(GraphicsSettingsData data) => AppliedScene.Add(data);
            public void RestoreDefaults() => RestoreCount++;
        }

        private PlayerPrefsGraphicsStorage _storage;
        private FakeGraphicsApplier _applier;

        [SetUp]
        public void SetUp()
        {
            ServiceProvider.Clear(true);
            _storage = new PlayerPrefsGraphicsStorage();
            _storage.Clear();
            _applier = new FakeGraphicsApplier();
        }

        [TearDown]
        public void TearDown()
        {
            _storage.Clear();
            ServiceProvider.Clear(true);
        }

        [Test]
        public void DefaultSettings_AreBalanced_AndDetectedAsBalancedPreset()
        {
            var service = new GraphicsSettingsService(_storage, _applier);

            Assert.AreEqual(GraphicsPreset.Balanced, service.CurrentPreset);
            Assert.AreEqual(60, service.CurrentSettings.TargetFps);
            Assert.AreEqual(0.75f, service.CurrentSettings.RenderScale, 0.001f);
            Assert.AreEqual(ShadowQualityMode.Hard, service.CurrentSettings.Shadows);
            Assert.IsFalse(service.CurrentSettings.PostProcessingEnabled);
            Assert.AreEqual(MsaaMode.Off, service.CurrentSettings.Msaa);
            Assert.AreEqual(TextureQualityMode.Half, service.CurrentSettings.TextureQuality);
            Assert.IsTrue(service.CurrentSettings.AdaptiveThermalControl);
        }

        [Test]
        public void ApplyPreset_ThenMutateSingleSetting_FlipsPresetToCustom()
        {
            var service = new GraphicsSettingsService(_storage, _applier);

            service.ApplyPreset(GraphicsPreset.High);
            Assert.AreEqual(GraphicsPreset.High, service.CurrentPreset);

            GraphicsSettingsData custom = service.CurrentSettings;
            custom.TargetFps = 30;
            service.UpdateSettings(custom);

            Assert.AreEqual(GraphicsPreset.Custom, service.CurrentPreset);
            Assert.AreEqual(2, _applier.AppliedAll.Count);
        }

        [Test]
        public void GraphicsSettings_PersistAcrossAccountSwitch_AndGameSaveReset()
        {
            var service = new GraphicsSettingsService(_storage, _applier);
            service.ApplyPreset(GraphicsPreset.BatterySaver);

            GameSave.SetActiveAccount("gpgs_player_123");
            GameSave.Reset();
            GameSave.SetActiveAccount(null);

            var reloaded = new GraphicsSettingsService(_storage, new FakeGraphicsApplier());
            Assert.AreEqual(GraphicsPreset.BatterySaver, reloaded.CurrentPreset);
            Assert.AreEqual(30, reloaded.CurrentSettings.TargetFps);
        }
    }
}
