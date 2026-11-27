using System;
using Tools.ServiceProvider;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.World.Graphics
{
    public sealed class GraphicsSettingsService : IGraphicsSettingsService
    {
        private readonly IGraphicsStorage _storage;
        private readonly IGraphicsApplier _applier;

        public GraphicsSettingsData CurrentSettings { get; private set; }
        public GraphicsPreset CurrentPreset => GraphicsPresets.DetectPreset(CurrentSettings);

        public event Action<GraphicsSettingsData, GraphicsPreset> OnSettingsChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoRegister()
        {
            if (ServiceProvider.TryGetService<IGraphicsSettingsService>(out _))
                return;

            var service = new GraphicsSettingsService(
                new PlayerPrefsGraphicsStorage(),
                new UnityGraphicsApplier());

            ServiceProvider.TryAddService<GraphicsSettingsService>(service);
            ServiceProvider.TryAddService<IGraphicsSettingsService>(service);
            service.ApplyCurrentSettings();

            Application.quitting += service.OnShutdown;
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                    service.OnShutdown();
            };
#endif
        }

        public GraphicsSettingsService()
            : this(new PlayerPrefsGraphicsStorage(), new UnityGraphicsApplier())
        {
        }

        public GraphicsSettingsService(IGraphicsStorage storage, IGraphicsApplier applier)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _applier = applier ?? throw new ArgumentNullException(nameof(applier));
            CurrentSettings = GraphicsPresets.Normalize(_storage.Load());
        }

        public void ApplyPreset(GraphicsPreset preset)
        {
            if (preset == GraphicsPreset.Custom)
                return;

            UpdateSettings(GraphicsPresets.GetPreset(preset));
        }

        public void UpdateSettings(GraphicsSettingsData newSettings)
        {
            CurrentSettings = GraphicsPresets.Normalize(newSettings);
            _storage.Save(CurrentSettings);
            _applier.ApplyAll(CurrentSettings);
            OnSettingsChanged?.Invoke(CurrentSettings, CurrentPreset);
        }

        public void ApplyCurrentSettings()
        {
            _applier.ApplyAll(CurrentSettings);
        }

        public void ApplyToActiveScene()
        {
            _applier.ApplySceneObjects(CurrentSettings);
        }

        public void ResetToDefaults()
        {
            UpdateSettings(GraphicsPresets.Balanced);
        }

        private void OnShutdown()
        {
            _applier.RestoreDefaults();
        }
    }
}
