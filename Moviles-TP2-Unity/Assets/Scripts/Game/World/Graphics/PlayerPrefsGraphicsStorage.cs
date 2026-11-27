using UnityEngine;

namespace Game.World.Graphics
{
    public sealed class PlayerPrefsGraphicsStorage : IGraphicsStorage
    {
        public const string TargetFpsKey = "Graphics_TargetFps";
        public const string RenderScaleKey = "Graphics_RenderScale";
        public const string ShadowsKey = "Graphics_Shadows";
        public const string PostProcessingKey = "Graphics_PostProcessing";
        public const string MsaaKey = "Graphics_Msaa";
        public const string TextureQualityKey = "Graphics_TextureQuality";
        public const string AdaptiveThermalKey = "Graphics_AdaptiveThermal";

        public GraphicsSettingsData Load()
        {
            GraphicsSettingsData defaults = GraphicsPresets.Balanced;
            var loaded = new GraphicsSettingsData
            {
                TargetFps = PlayerPrefs.GetInt(TargetFpsKey, defaults.TargetFps),
                RenderScale = PlayerPrefs.GetFloat(RenderScaleKey, defaults.RenderScale),
                Shadows = (ShadowQualityMode)PlayerPrefs.GetInt(ShadowsKey, (int)defaults.Shadows),
                PostProcessingEnabled = PlayerPrefs.GetInt(PostProcessingKey, defaults.PostProcessingEnabled ? 1 : 0) > 0,
                Msaa = (MsaaMode)PlayerPrefs.GetInt(MsaaKey, (int)defaults.Msaa),
                TextureQuality = (TextureQualityMode)PlayerPrefs.GetInt(TextureQualityKey, (int)defaults.TextureQuality),
                AdaptiveThermalControl = PlayerPrefs.GetInt(AdaptiveThermalKey, defaults.AdaptiveThermalControl ? 1 : 0) > 0
            };

            return GraphicsPresets.Normalize(loaded);
        }

        public void Save(GraphicsSettingsData data)
        {
            GraphicsSettingsData normalized = GraphicsPresets.Normalize(data);
            PlayerPrefs.SetInt(TargetFpsKey, normalized.TargetFps);
            PlayerPrefs.SetFloat(RenderScaleKey, normalized.RenderScale);
            PlayerPrefs.SetInt(ShadowsKey, (int)normalized.Shadows);
            PlayerPrefs.SetInt(PostProcessingKey, normalized.PostProcessingEnabled ? 1 : 0);
            PlayerPrefs.SetInt(MsaaKey, (int)normalized.Msaa);
            PlayerPrefs.SetInt(TextureQualityKey, (int)normalized.TextureQuality);
            PlayerPrefs.SetInt(AdaptiveThermalKey, normalized.AdaptiveThermalControl ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(TargetFpsKey);
            PlayerPrefs.DeleteKey(RenderScaleKey);
            PlayerPrefs.DeleteKey(ShadowsKey);
            PlayerPrefs.DeleteKey(PostProcessingKey);
            PlayerPrefs.DeleteKey(MsaaKey);
            PlayerPrefs.DeleteKey(TextureQualityKey);
            PlayerPrefs.DeleteKey(AdaptiveThermalKey);
            PlayerPrefs.Save();
        }
    }
}
