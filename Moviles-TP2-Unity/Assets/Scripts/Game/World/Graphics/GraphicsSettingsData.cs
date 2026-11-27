using System;
using UnityEngine;

namespace Game.World.Graphics
{
    public enum GraphicsPreset
    {
        BatterySaver = 0,
        Balanced = 1,
        High = 2,
        Custom = 3
    }

    public enum ShadowQualityMode
    {
        Off = 0,
        Hard = 1,
        Soft = 2
    }

    public enum MsaaMode
    {
        Off = 1,
        X2 = 2,
        X4 = 4
    }

    public enum TextureQualityMode
    {
        Half = 0,
        Full = 1
    }

    [Serializable]
    public struct GraphicsSettingsData : IEquatable<GraphicsSettingsData>
    {
        public int TargetFps;
        public float RenderScale;
        public ShadowQualityMode Shadows;
        public bool PostProcessingEnabled;
        public MsaaMode Msaa;
        public TextureQualityMode TextureQuality;
        public bool AdaptiveThermalControl;

        public bool Equals(GraphicsSettingsData other)
        {
            return TargetFps == other.TargetFps
                && Mathf.Abs(RenderScale - other.RenderScale) < 0.01f
                && Shadows == other.Shadows
                && PostProcessingEnabled == other.PostProcessingEnabled
                && Msaa == other.Msaa
                && TextureQuality == other.TextureQuality
                && AdaptiveThermalControl == other.AdaptiveThermalControl;
        }

        public override bool Equals(object obj) => obj is GraphicsSettingsData other && Equals(other);

        public override int GetHashCode()
        {
            return HashCode.Combine(
                TargetFps,
                Mathf.RoundToInt(RenderScale * 100f),
                Shadows,
                PostProcessingEnabled,
                Msaa,
                TextureQuality,
                AdaptiveThermalControl);
        }
    }

    public static class GraphicsPresets
    {
        public static readonly int[] FpsOptions = { 30, 45, 60 };
        public static readonly float[] RenderScaleOptions = { 0.60f, 0.75f, 0.90f, 1.00f };

        public static GraphicsSettingsData BatterySaver => new()
        {
            TargetFps = 30,
            RenderScale = 0.60f,
            Shadows = ShadowQualityMode.Off,
            PostProcessingEnabled = false,
            Msaa = MsaaMode.Off,
            TextureQuality = TextureQualityMode.Half,
            AdaptiveThermalControl = true
        };

        public static GraphicsSettingsData Balanced => new()
        {
            TargetFps = 60,
            RenderScale = 0.75f,
            Shadows = ShadowQualityMode.Hard,
            PostProcessingEnabled = false,
            Msaa = MsaaMode.Off,
            TextureQuality = TextureQualityMode.Half,
            AdaptiveThermalControl = true
        };

        public static GraphicsSettingsData High => new()
        {
            TargetFps = 60,
            RenderScale = 0.90f,
            Shadows = ShadowQualityMode.Soft,
            PostProcessingEnabled = true,
            Msaa = MsaaMode.X2,
            TextureQuality = TextureQualityMode.Full,
            AdaptiveThermalControl = false
        };

        public static GraphicsSettingsData GetPreset(GraphicsPreset preset)
        {
            return preset switch
            {
                GraphicsPreset.BatterySaver => BatterySaver,
                GraphicsPreset.High => High,
                _ => Balanced
            };
        }

        public static GraphicsPreset DetectPreset(GraphicsSettingsData data)
        {
            GraphicsSettingsData normalized = Normalize(data);
            if (normalized.Equals(BatterySaver)) return GraphicsPreset.BatterySaver;
            if (normalized.Equals(Balanced)) return GraphicsPreset.Balanced;
            if (normalized.Equals(High)) return GraphicsPreset.High;
            return GraphicsPreset.Custom;
        }

        public static GraphicsSettingsData Normalize(GraphicsSettingsData data)
        {
            data.TargetFps = SnapNearest(data.TargetFps, FpsOptions);
            data.RenderScale = SnapNearest(data.RenderScale, RenderScaleOptions);

            if (!Enum.IsDefined(typeof(ShadowQualityMode), data.Shadows))
                data.Shadows = Balanced.Shadows;
            if (!Enum.IsDefined(typeof(MsaaMode), data.Msaa))
                data.Msaa = Balanced.Msaa;
            if (!Enum.IsDefined(typeof(TextureQualityMode), data.TextureQuality))
                data.TextureQuality = Balanced.TextureQuality;

            return data;
        }

        private static int SnapNearest(int value, int[] options)
        {
            int best = options[0];
            int bestDiff = Math.Abs(value - best);
            for (int i = 1; i < options.Length; i++)
            {
                int diff = Math.Abs(value - options[i]);
                if (diff < bestDiff)
                {
                    best = options[i];
                    bestDiff = diff;
                }
            }
            return best;
        }

        private static float SnapNearest(float value, float[] options)
        {
            float best = options[0];
            float bestDiff = Mathf.Abs(value - best);
            for (int i = 1; i < options.Length; i++)
            {
                float diff = Mathf.Abs(value - options[i]);
                if (diff < bestDiff)
                {
                    best = options[i];
                    bestDiff = diff;
                }
            }
            return best;
        }
    }
}
