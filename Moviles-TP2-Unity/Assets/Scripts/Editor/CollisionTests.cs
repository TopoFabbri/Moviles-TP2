using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    public class CollisionTests
    {
        [Test]
        public void EnvironmentPrefabs_ColliderBoundsMatchVisualRendererBounds()
        {
            string[] paths =
            {
                "Assets/Prefab/Environment/EnvArchway.prefab",
                "Assets/Prefab/Environment/EnvBarrier.prefab",
                "Assets/Prefab/Environment/EnvPlatform.prefab",
                "Assets/Prefab/Environment/EnvPylon.prefab"
            };

            foreach (string path in paths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.IsNotNull(prefab, $"{path} should exist.");

                GameObject instance = Object.Instantiate(prefab);
                try
                {
                    Renderer renderer = instance.GetComponentInChildren<Renderer>(true);
                    Assert.IsNotNull(renderer, $"{path} should have a Renderer.");

                    Collider[] colliders = instance.GetComponentsInChildren<Collider>(true);
                    Assert.Greater(colliders.Length, 0, $"{path} should have at least one Collider.");

                    Bounds combined = colliders[0].bounds;
                    for (int i = 1; i < colliders.Length; i++)
                    {
                        combined.Encapsulate(colliders[i].bounds);
                    }

                    Bounds visual = renderer.bounds;
                    Assert.AreEqual(visual.center.x, combined.center.x, 0.5f, $"{path} collider center X mismatch.");
                    Assert.AreEqual(visual.center.y, combined.center.y, 0.5f, $"{path} collider center Y mismatch.");
                    Assert.AreEqual(visual.center.z, combined.center.z, 0.5f, $"{path} collider center Z mismatch.");
                    Assert.AreEqual(visual.size.x, combined.size.x, 2.0f, $"{path} collider size X mismatch.");
                    Assert.AreEqual(visual.size.y, combined.size.y, 1.0f, $"{path} collider size Y mismatch.");
                    Assert.AreEqual(visual.size.z, combined.size.z, 0.5f, $"{path} collider size Z mismatch.");
                }
                finally
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        [Test]
        public void EnvArchway_CenterOpeningIsClearForDrone()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Environment/EnvArchway.prefab");
            GameObject instance = Object.Instantiate(prefab);
            try
            {
                Physics.SyncTransforms();
                Collider[] colliders = instance.GetComponentsInChildren<Collider>(true);
                Vector3 centerOpening = new Vector3(0f, 5f, 0f);
                foreach (Collider c in colliders)
                {
                    Assert.IsFalse(c.bounds.Contains(centerOpening),
                        $"EnvArchway collider should not block center opening at {centerOpening}, but {c.bounds} does.");
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void DronePrefab_BoxColliderCoversDroneBodyMesh()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Drone.prefab");
            Assert.IsNotNull(prefab);
            GameObject instance = Object.Instantiate(prefab);
            try
            {
                Transform pivot = instance.transform.Find("DronePivot");
                Assert.IsNotNull(pivot, "DronePivot should exist.");

                BoxCollider box = pivot.GetComponent<BoxCollider>();
                Assert.IsNotNull(box, "DronePivot should have a BoxCollider.");
                Assert.IsFalse(box.isTrigger);
                Assert.IsTrue(box.enabled);

                Rigidbody rb = pivot.GetComponent<Rigidbody>();
                Assert.IsNotNull(rb, "DronePivot should have a Rigidbody.");
                Assert.IsFalse(rb.isKinematic);
                Assert.AreEqual(RigidbodyConstraints.FreezeRotation, rb.constraints,
                    "DronePivot Rigidbody should freeze rotation only so MoveModule can drive linearVelocity.");
                Assert.AreEqual(CollisionDetectionMode.Continuous, rb.collisionDetectionMode,
                    "DronePivot Rigidbody should use Continuous collision detection to prevent tunneling.");
                Assert.AreEqual(RigidbodyInterpolation.Interpolate, rb.interpolation,
                    "DronePivot Rigidbody should use Interpolate for smooth motion between FixedUpdate steps.");

                MeshRenderer bodyRenderer = null;
                foreach (MeshRenderer mr in pivot.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (mr.name == "Drone")
                    {
                        bodyRenderer = mr;
                        break;
                    }
                }
                Assert.IsNotNull(bodyRenderer, "Drone body MeshRenderer should exist.");

                Assert.GreaterOrEqual(box.bounds.size.x, bodyRenderer.bounds.size.x - 0.1f, "Drone BoxCollider width should cover body.");
                Assert.GreaterOrEqual(box.bounds.size.y, bodyRenderer.bounds.size.y - 0.1f, "Drone BoxCollider height should cover body.");
                Assert.GreaterOrEqual(box.bounds.size.z, bodyRenderer.bounds.size.z - 0.1f, "Drone BoxCollider depth should cover body.");
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void RingPrefabs_HaveValidFrameAndTriggerColliders()
        {
            string[] paths =
            {
                "Assets/Prefab/RingBig.prefab",
                "Assets/Prefab/RingMid.prefab",
                "Assets/Prefab/RingSmall.prefab"
            };

            foreach (string path in paths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.IsNotNull(prefab, $"{path} should exist.");
                GameObject instance = Object.Instantiate(prefab);
                try
                {
                    MeshCollider frameCollider = instance.GetComponent<MeshCollider>();
                    Assert.IsNotNull(frameCollider, $"{path} root should have MeshCollider.");
                    Assert.IsFalse(frameCollider.isTrigger, $"{path} frame MeshCollider should not be trigger.");
                    Assert.IsFalse(frameCollider.convex, $"{path} static ring frame MeshCollider should be non-convex.");

                    Transform scoreChild = instance.transform.Find("ScoreCollider");
                    Assert.IsNotNull(scoreChild, $"{path} should have ScoreCollider child.");
                    MeshCollider scoreCollider = scoreChild.GetComponent<MeshCollider>();
                    Assert.IsNotNull(scoreCollider, $"{path} ScoreCollider should have MeshCollider.");
                    Assert.IsTrue(scoreCollider.isTrigger, $"{path} ScoreCollider must be trigger.");
                    Assert.IsTrue(scoreCollider.convex, $"{path} ScoreCollider must be convex.");
                }
                finally
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        [Test]
        public void LevelScenes_HaveNoInvalidMeshColliders()
        {
            string[] scenes =
            {
                "Assets/Scenes/Levels/Level Tutorial.unity",
                "Assets/Scenes/Levels/Level 1.unity",
                "Assets/Scenes/Levels/Level 2.unity",
                "Assets/Scenes/Levels/Level 3.unity",
                "Assets/Scenes/Levels/Level 4.unity"
            };

            foreach (string scenePath in scenes)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                MeshCollider[] meshColliders = Object.FindObjectsByType<MeshCollider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (MeshCollider mc in meshColliders)
                {
                    if (!mc.convex)
                    {
                        Assert.IsFalse(mc.isTrigger, $"Non-convex MeshCollider '{mc.name}' in '{scenePath}' cannot be a trigger.");
                        Rigidbody rb = mc.GetComponentInParent<Rigidbody>();
                        if (rb != null)
                        {
                            Assert.IsTrue(rb.isKinematic, $"Non-convex MeshCollider '{mc.name}' in '{scenePath}' cannot be on dynamic Rigidbody '{rb.name}'.");
                        }
                    }
                }
            }
        }
    }
}
