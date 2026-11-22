using System.IO;
using Game.World.Visuals;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Editor
{
    public static class CraftFloorsAssetSetup
    {
        [MenuItem("Tools/Craft Game Floors/Apply Materials & URP Settings")]
        public static void ApplyAll()
        {
            SetupSkyboxMaterial();
            ConvertMaterials();
            UpdateUrpAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CraftFloorsAssetSetup] Materials and URP assets updated.");
        }

        private static void SetupSkyboxMaterial()
        {
            const string path = "Assets/Art/Materials/CyberSkybox.mat";
            Shader skyShader = Shader.Find("Custom/PaletteGradientSkybox");
            if (skyShader == null)
            {
                Debug.LogError("[CraftFloorsAssetSetup] Shader Custom/PaletteGradientSkybox not found.");
                return;
            }

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(skyShader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = skyShader;
            }

            mat.SetColor("_ZenithColor", Palette.Bounds);
            mat.SetColor("_HorizonColor", Palette.Floor);
            mat.SetColor("_NadirColor", Palette.Shadow);
            mat.SetFloat("_HorizonSharpness", 2.2f);
            EditorUtility.SetDirty(mat);
        }

        private static void ConvertMaterials()
        {
            Shader toonShader = Shader.Find("Custom/CyberToon");
            if (toonShader == null)
            {
                Debug.LogError("[CraftFloorsAssetSetup] Shader Custom/CyberToon not found.");
                return;
            }

            ApplyToonMat("Assets/Art/Materials/Drone.mat", toonShader, Palette.CharacterPrimary, Palette.Bounds, Color.black, 0.0025f);
            ApplyToonMat("Assets/Art/Materials/DroneHelix.mat", toonShader, Palette.CharacterSecondary, Palette.Bounds, Color.black, 0.002f);
            ApplyToonMat("Assets/Art/Materials/Floor.mat", toonShader, Palette.Floor, Palette.Shadow, Color.black, 0f);
            ApplyToonMat("Assets/Art/Materials/RingA.mat", toonShader, Palette.RingPrimary, Palette.Shadow, Palette.RingPrimary * 1.4f, 0.0025f);
            ApplyToonMat("Assets/Art/Materials/RingB.mat", toonShader, Palette.RingSecondary, Palette.Shadow, Palette.RingSecondary * 1.4f, 0.0025f);

            Material trailMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Trail.mat");
            if (trailMat != null)
            {
                Shader trailShader = Shader.Find("Sprites/Default");
                if (trailShader != null)
                    trailMat.shader = trailShader;
                trailMat.color = Palette.CharacterPrimary;
                EditorUtility.SetDirty(trailMat);
            }

            string[] modelMats = Directory.GetFiles("Assets/Art/Models/Materials", "*.mat", SearchOption.TopDirectoryOnly);
            foreach (string rawPath in modelMats)
            {
                string assetPath = rawPath.Replace('\\', '/');
                string name = Path.GetFileNameWithoutExtension(assetPath);
                Color baseCol = name switch
                {
                    "XMat" => Palette.RingPrimary,
                    "YMat" => Palette.RingSecondary,
                    "ZMat" => Palette.CharacterPrimary,
                    "Center" => Palette.Accent,
                    _ => Palette.Accent
                };
                Color emission = name is "XMat" or "YMat" or "ZMat" ? Color.black : Palette.Accent * 0.8f;
                ApplyToonMat(assetPath, toonShader, baseCol, Palette.Shadow, emission, 0.002f);
            }
        }

        private static void ApplyToonMat(string path, Shader toonShader, Color baseColor, Color shadowTint, Color emission, float outlineWidth)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) return;

            mat.shader = toonShader;
            mat.SetColor("_BaseColor", baseColor);
            mat.SetColor("_ShadowTint", shadowTint);
            mat.SetFloat("_Bands", 3f);
            mat.SetFloat("_BandSoftness", 0.03f);
            mat.SetColor("_EmissionColor", emission);
            mat.SetColor("_OutlineColor", Palette.Shadow);
            mat.SetFloat("_OutlineWidth", outlineWidth);
            EditorUtility.SetDirty(mat);
        }

        private static void UpdateUrpAssets()
        {
            var mobileRp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
            if (mobileRp != null)
            {
                var so = new SerializedObject(mobileRp);
                var softShadows = so.FindProperty("m_SoftShadowsSupported");
                if (softShadows != null)
                {
                    softShadows.boolValue = true;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(mobileRp);
                }
            }

            var pcRenderer = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>("Assets/Settings/PC_Renderer.asset");
            if (pcRenderer != null)
            {
                foreach (var feature in pcRenderer.rendererFeatures)
                {
                    if (feature != null && feature.GetType().Name.Contains("AmbientOcclusion"))
                    {
                        feature.SetActive(false);
                        EditorUtility.SetDirty(feature);
                    }
                }
                EditorUtility.SetDirty(pcRenderer);
            }

            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Settings/SampleSceneProfile.asset");
            if (profile != null)
            {
                if (!profile.TryGet(out Tonemapping tonemapping))
                    tonemapping = profile.Add<Tonemapping>(true);
                tonemapping.mode.Override(TonemappingMode.Neutral);

                if (!profile.TryGet(out Bloom bloom))
                    bloom = profile.Add<Bloom>(true);
                bloom.intensity.Override(0.35f);
                bloom.threshold.Override(1.05f);

                if (!profile.TryGet(out Vignette vignette))
                    vignette = profile.Add<Vignette>(true);
                vignette.intensity.Override(0.22f);
                vignette.color.Override(Palette.Shadow);

                if (!profile.TryGet(out ColorAdjustments colorAdj))
                    colorAdj = profile.Add<ColorAdjustments>(true);
                colorAdj.saturation.Override(10f);
                colorAdj.contrast.Override(6f);

                EditorUtility.SetDirty(profile);
            }
        }
    }
}
