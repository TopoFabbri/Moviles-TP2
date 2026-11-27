using System;
using Tools.ServiceProvider;

namespace Game.World.Graphics
{
    public interface IGraphicsSettingsService : IService
    {
        GraphicsSettingsData CurrentSettings { get; }
        GraphicsPreset CurrentPreset { get; }
        event Action<GraphicsSettingsData, GraphicsPreset> OnSettingsChanged;

        void ApplyPreset(GraphicsPreset preset);
        void UpdateSettings(GraphicsSettingsData newSettings);
        void ApplyCurrentSettings();
        void ApplyToActiveScene();
        void ResetToDefaults();
    }
}
