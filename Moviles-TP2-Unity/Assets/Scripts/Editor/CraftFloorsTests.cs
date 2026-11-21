using Game.Audio;
using Game.World.Visuals;
using NUnit.Framework;
using Tools.Tween;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class CraftFloorsTests
    {
        [Test]
        public void Ease_Curves_ReturnExpectedEndpoints()
        {
            Assert.AreEqual(0f, Ease.OutCubic(0f), 0.001f);
            Assert.AreEqual(1f, Ease.OutCubic(1f), 0.001f);
            Assert.Greater(Ease.OutCubic(0.5f), 0.5f);

            Assert.AreEqual(0f, Ease.OutBack(0f), 0.001f);
            Assert.AreEqual(1f, Ease.OutBack(1f), 0.001f);
            Assert.Greater(Ease.OutBack(0.7f), 1f);

            Assert.AreEqual(0f, Ease.OutElastic(0f), 0.001f);
            Assert.AreEqual(1f, Ease.OutElastic(1f), 0.001f);
        }

        [Test]
        public void GameAudioService_SynthesizesNonEmptyPcmClips()
        {
            AudioClip pickup = GameAudioService.CreatePickupBlipClip();
            AudioClip milestone = GameAudioService.CreateMilestoneStingClip();
            AudioClip crash = GameAudioService.CreateCrashClip();
            AudioClip click = GameAudioService.CreateUiClickClip();
            AudioClip ambient = GameAudioService.CreateAmbientBedClip();

            Assert.IsNotNull(pickup);
            Assert.Greater(pickup.samples, 0);
            Assert.IsNotNull(milestone);
            Assert.Greater(milestone.samples, 0);
            Assert.IsNotNull(crash);
            Assert.Greater(crash.samples, 0);
            Assert.IsNotNull(click);
            Assert.Greater(click.samples, 0);
            Assert.IsNotNull(ambient);
            Assert.Greater(ambient.samples, 0);

            Object.DestroyImmediate(pickup);
            Object.DestroyImmediate(milestone);
            Object.DestroyImmediate(crash);
            Object.DestroyImmediate(click);
            Object.DestroyImmediate(ambient);
        }

        [Test]
        public void GameAudioService_GainLayersAndPauseDucking()
        {
            GameAudioService.SetMusicVolume(0.8f);
            GameAudioService.SetSfxVolume(0.5f);
            GameAudioService.SetPausedDucking(false);

            Assert.AreEqual(0.8f, GameAudioService.EffectiveMusicVolume, 0.001f);
            Assert.AreEqual(0.5f, GameAudioService.EffectiveSfxVolume, 0.001f);

            GameAudioService.SetPausedDucking(true);
            Assert.AreEqual(0.4f, GameAudioService.EffectiveMusicVolume, 0.001f);
            Assert.AreEqual(0.25f, GameAudioService.EffectiveSfxVolume, 0.001f);

            GameAudioService.SetPausedDucking(false);
        }

        [Test]
        public void Palette_And_CyberToonMaterials_AreConfigured()
        {
            Assert.AreNotEqual(Color.black, Palette.Shadow);
            Shader toon = Shader.Find("Custom/CyberToon");
            Assert.IsNotNull(toon, "Custom/CyberToon shader should compile and be found.");

            Material droneMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Drone.mat");
            Assert.IsNotNull(droneMat);
            Assert.AreEqual(toon, droneMat.shader);

            Material skyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/CyberSkybox.mat");
            Assert.IsNotNull(skyMat);
            Assert.AreEqual(Shader.Find("Custom/PaletteGradientSkybox"), skyMat.shader);
        }
    }
}
