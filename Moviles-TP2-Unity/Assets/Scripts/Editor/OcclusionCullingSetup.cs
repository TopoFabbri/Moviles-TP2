using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    public static class OcclusionCullingSetup
    {
        private const StaticEditorFlags StaticEnvironmentFlags =
            StaticEditorFlags.OccluderStatic |
            StaticEditorFlags.OccludeeStatic |
            StaticEditorFlags.BatchingStatic |
            StaticEditorFlags.ContributeGI |
            StaticEditorFlags.ReflectionProbeStatic;

        [MenuItem("Tools/Occlusion Culling/Bake All Scenes")]
        public static void SetupAndBakeAllScenes()
        {
            ConfigurePrefabs();
            BakeAllBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[OcclusionCullingSetup] Prefabs configured and all build scenes baked.");
        }

        private static void ConfigurePrefabs()
        {
            // Dynamic pooled rings: no static flags, allow occlusion when dynamic
            string[] ringPrefabs =
            {
                "Assets/Prefab/RingBig.prefab",
                "Assets/Prefab/RingMid.prefab",
                "Assets/Prefab/RingSmall.prefab"
            };

            foreach (string path in ringPrefabs)
            {
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                if (root == null) continue;

                try
                {
                    SetStaticFlagsRecursive(root.transform, 0);
                    foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
                    {
                        r.allowOcclusionWhenDynamic = true;
                    }
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            // SpawnPoint: disabled at runtime, must NOT bake as static occluder
            const string spawnPointPath = "Assets/Prefab/SpawnPoint.prefab";
            GameObject spawnRoot = PrefabUtility.LoadPrefabContents(spawnPointPath);
            if (spawnRoot != null)
            {
                try
                {
                    SetStaticFlagsRecursive(spawnRoot.transform, 0);
                    PrefabUtility.SaveAsPrefabAsset(spawnRoot, spawnPointPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(spawnRoot);
                }
            }

            // SceneDefaults: Floor is static occluder/occludee; logic objects are non-static
            const string sceneDefaultsPath = "Assets/Prefab/SceneDefaults.prefab";
            GameObject defaultsRoot = PrefabUtility.LoadPrefabContents(sceneDefaultsPath);
            if (defaultsRoot != null)
            {
                try
                {
                    SetStaticFlagsRecursive(defaultsRoot.transform, 0);
                    Transform floor = FindChildRecursive(defaultsRoot.transform, "Floor");
                    if (floor != null)
                        GameObjectUtility.SetStaticEditorFlags(floor.gameObject, StaticEnvironmentFlags);

                    PrefabUtility.SaveAsPrefabAsset(defaultsRoot, sceneDefaultsPath);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(defaultsRoot);
                }
            }
        }

        private static void BakeAllBuildScenes()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (!scenes[i].enabled) continue;
                string scenePath = scenes[i].path;
                if (string.IsNullOrEmpty(scenePath)) continue;

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                GameObject floor = GameObject.Find("Floor");
                if (floor == null)
                {
                    Debug.Log($"[OcclusionCullingSetup] Skipping '{scenePath}' (no Floor / 3D level geometry).");
                    continue;
                }

                foreach (Camera cam in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (!cam.orthographic)
                        cam.useOcclusionCulling = true;
                }

                // Clear static flags on any SpawnPoint instances in the scene
                foreach (GameObject rootGo in scene.GetRootGameObjects())
                {
                    foreach (Transform t in rootGo.GetComponentsInChildren<Transform>(true))
                    {
                        if (t.name.StartsWith("SpawnPoint", StringComparison.Ordinal))
                            SetStaticFlagsRecursive(t, 0);
                    }
                }

                // Ensure Floor is marked static occluder/occludee
                GameObjectUtility.SetStaticEditorFlags(floor, StaticEnvironmentFlags);

                ConfigureOcclusionArea(scene);

                StaticOcclusionCulling.smallestOccluder = 30f;
                StaticOcclusionCulling.smallestHole = 3f;
                StaticOcclusionCulling.backfaceThreshold = 100f;

                if (!Application.isBatchMode)
                {
                    bool computed = StaticOcclusionCulling.Compute();
                    Debug.Log($"[OcclusionCullingSetup] Scene '{scenePath}' Compute={computed}, umbraDataSize={StaticOcclusionCulling.umbraDataSize}");
                }
                else
                {
                    Debug.Log($"[OcclusionCullingSetup] Scene '{scenePath}' configured for Umbra (Compute skipped in batchmode).");
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        private static void ConfigureOcclusionArea(Scene scene)
        {
            OcclusionArea area = UnityEngine.Object.FindFirstObjectByType<OcclusionArea>(FindObjectsInactive.Include);
            if (area == null)
            {
                var go = new GameObject("OcclusionArea");
                SceneManager.MoveGameObjectToScene(go, scene);
                area = go.AddComponent<OcclusionArea>();
            }

            Bounds bounds = new Bounds(Vector3.zero, new Vector3(50f, 50f, 50f));
            foreach (GameObject rootGo in scene.GetRootGameObjects())
            {
                foreach (Transform t in rootGo.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.StartsWith("SpawnPoint", StringComparison.Ordinal))
                        bounds.Encapsulate(t.position);
                }
            }

            bounds.Expand(new Vector3(60f, 60f, 60f));
            area.transform.position = Vector3.zero;
            area.center = bounds.center;
            area.size = bounds.size;

            var so = new SerializedObject(area);
            SerializedProperty isViewVolume = so.FindProperty("m_IsViewVolume");
            if (isViewVolume != null)
            {
                isViewVolume.boolValue = true;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetStaticFlagsRecursive(Transform root, StaticEditorFlags flags)
        {
            GameObjectUtility.SetStaticEditorFlags(root.gameObject, flags);
            for (int i = 0; i < root.childCount; i++)
                SetStaticFlagsRecursive(root.GetChild(i), flags);
        }

        private static Transform FindChildRecursive(Transform root, string targetName)
        {
            if (root.name == targetName) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildRecursive(root.GetChild(i), targetName);
                if (found != null) return found;
            }
            return null;
        }
    }
}
