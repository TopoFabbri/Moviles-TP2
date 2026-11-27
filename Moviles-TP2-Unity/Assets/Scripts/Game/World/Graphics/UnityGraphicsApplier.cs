using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game.World.Graphics
{
    public sealed class UnityGraphicsApplier : IGraphicsApplier
    {
        private bool _hasCapturedOriginal;
        private RenderPipelineAsset _originalQualityRpAsset;
        private UniversalRenderPipelineAsset _runtimeRpAsset;

        private readonly List<AdaptivePerformanceScaler> _appliedScalers = new();
        private readonly List<AdaptivePerformanceScaler> _unappliedScalers = new();
        private readonly List<AdaptivePerformanceScaler> _disabledScalers = new();

        public void ApplyAll(GraphicsSettingsData data)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = data.TargetFps;
            QualitySettings.globalTextureMipmapLimit = data.TextureQuality == TextureQualityMode.Half ? 1 : 0;

            ApplyPipelineAssetSettings(data);
            ApplyAdaptivePerformance(data.AdaptiveThermalControl);
            ApplySceneObjects(data);
        }

        public void ApplySceneObjects(GraphicsSettingsData data)
        {
            LightShadows targetShadows = data.Shadows switch
            {
                ShadowQualityMode.Off => LightShadows.None,
                ShadowQualityMode.Hard => LightShadows.Hard,
                _ => LightShadows.Soft
            };

            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && lights[i].type == LightType.Directional)
                    lights[i].shadows = targetShadows;
            }

            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera cam = cameras[i];
                if (cam == null) continue;
                var uac = cam.GetUniversalAdditionalCameraData();
                if (uac != null)
                    uac.renderPostProcessing = data.PostProcessingEnabled;
            }
        }

        public void RestoreDefaults()
        {
            if (_hasCapturedOriginal)
            {
                QualitySettings.renderPipeline = _originalQualityRpAsset;
                _hasCapturedOriginal = false;
                _originalQualityRpAsset = null;
            }

            if (_runtimeRpAsset != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(_runtimeRpAsset);
                else
                    Object.DestroyImmediate(_runtimeRpAsset);

                _runtimeRpAsset = null;
            }
        }

        private void ApplyPipelineAssetSettings(GraphicsSettingsData data)
        {
            UniversalRenderPipelineAsset rp = EnsureRuntimePipelineClone();
            if (rp == null)
                return;

            rp.renderScale = data.RenderScale;
            rp.msaaSampleCount = (int)data.Msaa;
            rp.supportsHDR = data.PostProcessingEnabled;
            rp.shadowDistance = data.Shadows switch
            {
                ShadowQualityMode.Off => 0f,
                ShadowQualityMode.Hard => 25f,
                _ => 45f
            };
        }

        private UniversalRenderPipelineAsset EnsureRuntimePipelineClone()
        {
            var active = QualitySettings.renderPipeline as UniversalRenderPipelineAsset
                      ?? GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;

            if (active == null)
                return null;

            if (_runtimeRpAsset != null && active == _runtimeRpAsset)
                return _runtimeRpAsset;

            if (!_hasCapturedOriginal)
            {
                _originalQualityRpAsset = QualitySettings.renderPipeline;
                _hasCapturedOriginal = true;
            }

            _runtimeRpAsset = Object.Instantiate(active);
            _runtimeRpAsset.name = $"{active.name}_RuntimeClone";
            QualitySettings.renderPipeline = _runtimeRpAsset;
            return _runtimeRpAsset;
        }

        private void ApplyAdaptivePerformance(bool enabled)
        {
            var ap = Holder.Instance;
            if (ap == null || !ap.Active || ap.Indexer == null)
                return;

            var indexer = ap.Indexer;
            List<AdaptivePerformanceScaler> applied = _appliedScalers;
            List<AdaptivePerformanceScaler> unapplied = _unappliedScalers;
            List<AdaptivePerformanceScaler> disabled = _disabledScalers;

            indexer.GetAppliedScalers(ref applied);
            indexer.GetUnappliedScalers(ref unapplied);
            indexer.GetDisabledScalers(ref disabled);

            ToggleThermalScalers(applied, enabled);
            ToggleThermalScalers(unapplied, enabled);
            ToggleThermalScalers(disabled, enabled);
        }

        private static void ToggleThermalScalers(List<AdaptivePerformanceScaler> scalers, bool enabled)
        {
            if (scalers == null)
                return;

            for (int i = 0; i < scalers.Count; i++)
            {
                AdaptivePerformanceScaler scaler = scalers[i];
                if (scaler is AdaptiveFramerate || scaler is AdaptiveResolution)
                    scaler.Enabled = enabled;
            }
        }
    }
}
