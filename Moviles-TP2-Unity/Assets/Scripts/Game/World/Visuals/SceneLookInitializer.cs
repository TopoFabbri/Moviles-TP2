using Game.World.Graphics;
using Game.World.Visuals;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Game.World.Visuals
{
    public static class SceneLookInitializer
    {
        private static Material _runtimeSkybox;
        private static Texture2D _gridTexture;
        private static bool _hooked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!_hooked)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                _hooked = true;
            }

            ApplyToCurrentScene();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyToCurrentScene();
        }

        public static void ApplyToCurrentScene()
        {
            ApplySkyboxAndAmbient();
            ApplyFog();
            ApplyDirectionalLight();
            ApplyCameraAndPostProcessing();
            ApplyFloorGrid();

            if (ServiceProvider.TryGetService<IGraphicsSettingsService>(out var graphicsSettings))
                graphicsSettings.ApplyToActiveScene();
        }

        private static void ApplySkyboxAndAmbient()
        {
            Material currentSky = RenderSettings.skybox;
            bool hasCustomPaletteSky = currentSky != null && currentSky.shader != null &&
                                       currentSky.shader.name == "Custom/PaletteGradientSkybox";

            if (!hasCustomPaletteSky)
            {
                if (_runtimeSkybox == null)
                {
                    Shader skyShader = Shader.Find("Custom/PaletteGradientSkybox");
                    if (skyShader != null)
                    {
                        _runtimeSkybox = new Material(skyShader)
                        {
                            name = "CyberSkybox_Runtime"
                        };
                        _runtimeSkybox.SetColor("_ZenithColor", Palette.Bounds);
                        _runtimeSkybox.SetColor("_HorizonColor", Palette.Floor);
                        _runtimeSkybox.SetColor("_NadirColor", Palette.Shadow);
                        _runtimeSkybox.SetFloat("_HorizonSharpness", 2.2f);
                    }
                }

                if (_runtimeSkybox != null)
                    RenderSettings.skybox = _runtimeSkybox;
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Palette.Bounds;
            RenderSettings.ambientEquatorColor = Palette.EquatorAmbient;
            RenderSettings.ambientGroundColor = Palette.Shadow;
        }

        private static void ApplyFog()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Palette.Floor;
            RenderSettings.fogStartDistance = 45f;
            RenderSettings.fogEndDistance = 180f;
        }

        private static void ApplyDirectionalLight()
        {
            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            Light dirLight = null;
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].type == LightType.Directional)
                {
                    dirLight = lights[i];
                    break;
                }
            }

            if (dirLight == null)
                return;

            dirLight.color = Palette.KeyLight;
            dirLight.intensity = 1.15f;
            dirLight.shadows = ServiceProvider.TryGetService<IGraphicsSettingsService>(out var gfx)
                ? gfx.CurrentSettings.Shadows switch
                {
                    ShadowQualityMode.Off => LightShadows.None,
                    ShadowQualityMode.Hard => LightShadows.Hard,
                    _ => LightShadows.Soft
                }
                : LightShadows.Soft;
            dirLight.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
        }

        private static void ApplyCameraAndPostProcessing()
        {
            bool enablePostFx = !ServiceProvider.TryGetService<IGraphicsSettingsService>(out var gfx)
                             || gfx.CurrentSettings.PostProcessingEnabled;

            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera cam = cameras[i];
                if (cam == null) continue;
                if (!cam.orthographic)
                    cam.useOcclusionCulling = true;
                var uac = cam.GetUniversalAdditionalCameraData();
                if (uac != null)
                    uac.renderPostProcessing = enablePostFx;
            }

            Volume globalVolume = null;
            Volume[] volumes = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
            for (int i = 0; i < volumes.Length; i++)
            {
                if (volumes[i].isGlobal)
                {
                    globalVolume = volumes[i];
                    break;
                }
            }

            if (globalVolume == null)
                return;

            if (globalVolume.profile == null)
                globalVolume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

            VolumeProfile profile = globalVolume.profile;

            if (!profile.TryGet(out Tonemapping tonemapping))
                tonemapping = profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.Neutral);

            if (!profile.TryGet(out Bloom bloom))
                bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(0.35f);
            bloom.threshold.Override(1.05f);
            bloom.highQualityFiltering.Override(false);
            bloom.maxIterations.Override(4);

            if (!profile.TryGet(out Vignette vignette))
                vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.22f);
            vignette.color.Override(Palette.Shadow);

            if (!profile.TryGet(out ColorAdjustments colorAdj))
                colorAdj = profile.Add<ColorAdjustments>(true);
            colorAdj.saturation.Override(10f);
            colorAdj.contrast.Override(6f);
        }

        private static void ApplyFloorGrid()
        {
            GameObject floor = GameObject.Find("Floor");
            if (floor == null) return;

            Renderer rend = floor.GetComponent<Renderer>();
            if (rend == null || rend.sharedMaterial == null) return;

            if (_gridTexture == null)
            {
                const int size = 32;
                _gridTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Repeat,
                    name = "CyberFloorGrid"
                };

                Color baseCol = Color.white;
                Color lineCol = new Color(1.12f, 1.12f, 1.12f, 1f);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        bool isEdge = x == 0 || y == 0;
                        _gridTexture.SetPixel(x, y, isEdge ? lineCol : baseCol);
                    }
                }

                _gridTexture.Apply();
            }

            Material mat = rend.sharedMaterial;
            if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") == null)
            {
                rend.material.SetTexture("_BaseMap", _gridTexture);
                rend.material.SetTextureScale("_BaseMap", new Vector2(40f, 40f));
            }
        }
    }
}
