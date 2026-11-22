using Game.World.Targets;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    public class OcclusionCullingTests
    {
        [Test]
        public void DronePrefab_Cameras_HaveOcclusionCullingEnabled()
        {
            GameObject drone = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Drone.prefab");
            Assert.IsNotNull(drone, "Drone.prefab should exist.");

            Camera[] cameras = drone.GetComponentsInChildren<Camera>(true);
            Assert.Greater(cameras.Length, 0, "Drone.prefab should contain at least one Camera.");

            foreach (Camera cam in cameras)
            {
                if (!cam.orthographic)
                    Assert.IsTrue(cam.useOcclusionCulling, $"Camera '{cam.name}' should have useOcclusionCulling enabled.");
            }
        }

        [Test]
        public void Prefabs_HaveExpectedStaticFlagsAndDynamicOccludee()
        {
            string[] ringPaths =
            {
                "Assets/Prefab/RingBig.prefab",
                "Assets/Prefab/RingMid.prefab",
                "Assets/Prefab/RingSmall.prefab"
            };

            foreach (string path in ringPaths)
            {
                GameObject ring = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.IsNotNull(ring, $"{path} should exist.");

                StaticEditorFlags rootFlags = GameObjectUtility.GetStaticEditorFlags(ring);
                Assert.AreEqual(0, (int)(rootFlags & StaticEditorFlags.OccluderStatic), $"{path} should not be OccluderStatic.");

                foreach (Renderer r in ring.GetComponentsInChildren<Renderer>(true))
                {
                    Assert.IsTrue(r.allowOcclusionWhenDynamic, $"Renderer '{r.name}' in {path} should allow occlusion when dynamic.");
                }
            }

            GameObject spawnPoint = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/SpawnPoint.prefab");
            Assert.IsNotNull(spawnPoint, "SpawnPoint.prefab should exist.");
            foreach (Transform t in spawnPoint.GetComponentsInChildren<Transform>(true))
            {
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(t.gameObject);
                Assert.AreEqual(0, (int)(flags & StaticEditorFlags.OccluderStatic), $"SpawnPoint child '{t.name}' should not be OccluderStatic.");
            }

            GameObject sceneDefaults = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/SceneDefaults.prefab");
            Assert.IsNotNull(sceneDefaults, "SceneDefaults.prefab should exist.");
            Transform floor = sceneDefaults.transform.Find("Map/Floor");
            Assert.IsNotNull(floor, "SceneDefaults.prefab should contain Map/Floor.");
            StaticEditorFlags floorFlags = GameObjectUtility.GetStaticEditorFlags(floor.gameObject);
            Assert.IsTrue((floorFlags & StaticEditorFlags.OccluderStatic) != 0, "Floor should be OccluderStatic.");
            Assert.IsTrue((floorFlags & StaticEditorFlags.OccludeeStatic) != 0, "Floor should be OccludeeStatic.");
        }

        [Test]
        public void LevelScenes_HaveOcclusionAreaConfigured()
        {
            string[] levelScenes =
            {
                "Assets/Scenes/Levels/Level Tutorial.unity",
                "Assets/Scenes/Levels/Level 1.unity",
                "Assets/Scenes/Levels/Level 2.unity",
                "Assets/Scenes/Levels/Level 3.unity",
                "Assets/Scenes/Levels/Level 4.unity"
            };

            foreach (string scenePath in levelScenes)
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Assert.IsTrue(scene.IsValid(), $"Scene '{scenePath}' should open.");

                OcclusionArea area = Object.FindFirstObjectByType<OcclusionArea>(FindObjectsInactive.Include);
                Assert.IsNotNull(area, $"Scene '{scenePath}' should have an OcclusionArea.");
                Assert.Greater(area.size.x, 0f, $"OcclusionArea in '{scenePath}' should have positive X size.");
                Assert.Greater(area.size.y, 0f, $"OcclusionArea in '{scenePath}' should have positive Y size.");
                Assert.Greater(area.size.z, 0f, $"OcclusionArea in '{scenePath}' should have positive Z size.");
            }
        }

        [Test]
        public void TargetCullingController_CullsOffFrustumAndBeyondMaxDistance_WhileKeepingCollidersActive()
        {
            var camGo = new GameObject("TestCam");
            Camera cam = camGo.AddComponent<Camera>();
            cam.transform.position = Vector3.zero;
            cam.transform.rotation = Quaternion.identity;
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;

            var controllerGo = new GameObject("TestCullingController");
            TargetCullingController controller = controllerGo.AddComponent<TargetCullingController>();
            controller.MaxRenderDistance = 200f;

            Target visibleTarget = CreateTestTarget("VisibleTarget", new Vector3(0f, 0f, 50f));
            Target farTarget = CreateTestTarget("FarTarget", new Vector3(0f, 0f, 350f));
            Target behindTarget = CreateTestTarget("BehindTarget", new Vector3(0f, 0f, -50f));

            try
            {
                controller.Initialize(new[] { visibleTarget, farTarget, behindTarget }, cam);

                Assert.IsFalse(visibleTarget.IsRenderCulled, "Target inside frustum and within max distance should be visible.");
                Assert.IsTrue(visibleTarget.GetComponent<MeshRenderer>().enabled);

                Assert.IsTrue(farTarget.IsRenderCulled, "Target beyond maxRenderDistance should be culled.");
                Assert.IsFalse(farTarget.GetComponent<MeshRenderer>().enabled);
                Assert.IsTrue(farTarget.GetComponent<BoxCollider>().enabled, "Collider must stay enabled when render-culled.");
                Assert.IsTrue(farTarget.gameObject.activeSelf, "GameObject must stay active when render-culled.");

                Assert.IsTrue(behindTarget.IsRenderCulled, "Target behind camera should be culled.");
                Assert.IsFalse(behindTarget.GetComponent<MeshRenderer>().enabled);
                Assert.IsTrue(behindTarget.GetComponent<BoxCollider>().enabled, "Collider must stay enabled when render-culled.");
                Assert.IsTrue(behindTarget.gameObject.activeSelf, "GameObject must stay active when render-culled.");

                controller.Clear();
                Assert.IsFalse(farTarget.IsRenderCulled);
                Assert.IsTrue(farTarget.GetComponent<MeshRenderer>().enabled);
                Assert.IsFalse(behindTarget.IsRenderCulled);
                Assert.IsTrue(behindTarget.GetComponent<MeshRenderer>().enabled);
            }
            finally
            {
                Object.DestroyImmediate(visibleTarget.gameObject);
                Object.DestroyImmediate(farTarget.gameObject);
                Object.DestroyImmediate(behindTarget.gameObject);
                Object.DestroyImmediate(controllerGo);
                Object.DestroyImmediate(camGo);
            }
        }

        private static Target CreateTestTarget(string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.AddComponent<BoxCollider>();
            return go.AddComponent<Target>();
        }
    }
}
