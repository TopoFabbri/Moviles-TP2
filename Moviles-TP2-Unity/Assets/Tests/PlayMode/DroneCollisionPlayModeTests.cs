using System.Collections;
using Game.Events;
using Game.World.Character;
using NUnit.Framework;
using Tools.EventSystem;
using Tools.ServiceProvider;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    public class DroneCollisionPlayModeTests
    {
        private static IEnumerator LoadLevel1AndGetPivot(System.Action<Transform, Rigidbody> onReady)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Level 1", LoadSceneMode.Single);
            while (!load.isDone) yield return null;

            yield return null;
            yield return null;
            yield return null;

            DroneCollisionController collisionCtrl = Object.FindFirstObjectByType<DroneCollisionController>();
            Assert.IsNotNull(collisionCtrl, "DroneCollisionController (DronePivot) should exist in Level 1.");
            Transform pivot = collisionCtrl.transform;
            Rigidbody rb = pivot.GetComponent<Rigidbody>();
            Assert.IsNotNull(rb, "DronePivot should have a Rigidbody.");
            onReady(pivot, rb);
        }

        [UnityTest]
        public IEnumerator Drone_AtMaxSpeed_HitsEnvBarrier_TriggersCrash()
        {
            LogAssert.ignoreFailingMessages = true;
            int prevCaptureFramerate = Time.captureFramerate;
            Time.captureFramerate = 30;

            try
            {
                Transform pivot = null;
                Rigidbody rb = null;
                yield return LoadLevel1AndGetPivot((p, r) => { pivot = p; rb = r; });

                GameObject barrierPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Environment/EnvBarrier.prefab");
                Assert.IsNotNull(barrierPrefab, "EnvBarrier.prefab should exist.");

                Vector3 barrierPos = pivot.position + pivot.forward * 25f - Vector3.up * 0.65f;
                GameObject barrier = Object.Instantiate(barrierPrefab);
                barrier.transform.position = barrierPos;
                barrier.transform.rotation = pivot.rotation * barrierPrefab.transform.rotation;

                try
                {
                    EventSystem events = ServiceProvider.GetService<EventSystem>();
                    events.Raise<MoveAction>(Vector3.forward);

                    float startTime = Time.time;
                    bool tunneled = false;
                    bool crashed = false;

                    while (Time.time - startTime < 2.0f)
                    {
                        yield return null;
                        float distAlongForward = Vector3.Dot(pivot.position - barrierPos, pivot.forward);
                        if (distAlongForward > 2.0f)
                        {
                            tunneled = true;
                            break;
                        }

                        if (GameObject.Find("GameOverMenu") != null)
                        {
                            crashed = true;
                            break;
                        }
                    }

                    Assert.IsFalse(tunneled, "Drone tunneled straight through EnvBarrier at 30 FPS without crashing!");
                    Assert.IsTrue(crashed, "Drone should have crashed into EnvBarrier (GameOverMenu active).");
                }
                finally
                {
                    Object.Destroy(barrier);
                }
            }
            finally
            {
                Time.captureFramerate = prevCaptureFramerate;
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [UnityTest]
        public IEnumerator Drone_AtMaxSpeed_FliesThroughRingSmall_TriggersRingPassed()
        {
            LogAssert.ignoreFailingMessages = true;
            int prevCaptureFramerate = Time.captureFramerate;
            Time.captureFramerate = 30;

            try
            {
                Transform pivot = null;
                Rigidbody rb = null;
                yield return LoadLevel1AndGetPivot((p, r) => { pivot = p; rb = r; });

                GameObject ringSmallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/RingSmall.prefab");
                Assert.IsNotNull(ringSmallPrefab, "RingSmall.prefab should exist.");

                Vector3 ringPos = pivot.position + pivot.forward * 25f;
                GameObject ring = Object.Instantiate(ringSmallPrefab, ringPos, pivot.rotation);

                try
                {
                    TriggerSpy spy = pivot.gameObject.AddComponent<TriggerSpy>();

                    EventSystem events = ServiceProvider.GetService<EventSystem>();
                    events.Raise<MoveAction>(Vector3.forward);

                    float startTime = Time.time;
                    while (Time.time - startTime < 2.0f && spy.Exits == 0)
                    {
                        yield return null;
                    }

                    Assert.IsNull(GameObject.Find("GameOverMenu"), "Drone flying through center of RingSmall should not crash into frame.");
                    Assert.GreaterOrEqual(spy.Enters, 1, "Drone should enter RingSmall ScoreCollider at 30 FPS.");
                    Assert.GreaterOrEqual(spy.Exits, 1, "Drone should exit RingSmall ScoreCollider at 30 FPS (not tunnel through).");
                    Assert.AreEqual("ScoreCollider", spy.LastExit);
                }
                finally
                {
                    Object.Destroy(ring);
                }
            }
            finally
            {
                Time.captureFramerate = prevCaptureFramerate;
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [UnityTest]
        public IEnumerator Drone_FliesThroughEnvArchwayCenter_DoesNotCrash()
        {
            LogAssert.ignoreFailingMessages = true;
            try
            {
                Transform pivot = null;
                Rigidbody rb = null;
                yield return LoadLevel1AndGetPivot((p, r) => { pivot = p; rb = r; });

                GameObject archwayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Environment/EnvArchway.prefab");
                Assert.IsNotNull(archwayPrefab);

                Vector3 archwayPos = pivot.position + pivot.forward * 20f - Vector3.up * 5f;
                GameObject archway = Object.Instantiate(archwayPrefab);
                archway.transform.position = archwayPos;
                archway.transform.rotation = pivot.rotation * archwayPrefab.transform.rotation;

                try
                {
                    EventSystem events = ServiceProvider.GetService<EventSystem>();
                    events.Raise<MoveAction>(Vector3.forward);

                    float startTime = Time.time;
                    bool cleared = false;
                    while (Time.time - startTime < 2.0f)
                    {
                        yield return null;
                        Assert.IsNull(GameObject.Find("GameOverMenu"), "Drone should NOT crash when flying through EnvArchway center opening.");
                        if (Vector3.Dot(pivot.position - archwayPos, pivot.forward) > 5.0f)
                        {
                            cleared = true;
                            break;
                        }
                    }

                    Assert.IsTrue(cleared, "Drone should pass cleanly through EnvArchway center opening.");
                }
                finally
                {
                    Object.Destroy(archway);
                }
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [UnityTest]
        public IEnumerator Drone_HitsEnvPylon_TriggersCrash()
        {
            LogAssert.ignoreFailingMessages = true;
            try
            {
                Transform pivot = null;
                Rigidbody rb = null;
                yield return LoadLevel1AndGetPivot((p, r) => { pivot = p; rb = r; });

                GameObject pylonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Environment/EnvPylon.prefab");
                Assert.IsNotNull(pylonPrefab);

                Vector3 pylonPos = pivot.position + pivot.forward * 20f - Vector3.up * 5f;
                GameObject pylon = Object.Instantiate(pylonPrefab);
                pylon.transform.position = pylonPos;
                pylon.transform.rotation = pivot.rotation * pylonPrefab.transform.rotation;

                try
                {
                    EventSystem events = ServiceProvider.GetService<EventSystem>();
                    events.Raise<MoveAction>(Vector3.forward);

                    float startTime = Time.time;
                    bool crashed = false;
                    while (Time.time - startTime < 2.0f)
                    {
                        yield return null;
                        if (GameObject.Find("GameOverMenu") != null)
                        {
                            crashed = true;
                            break;
                        }
                    }

                    Assert.IsTrue(crashed, "Drone should crash when hitting EnvPylon.");
                }
                finally
                {
                    Object.Destroy(pylon);
                }
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        private sealed class TriggerSpy : MonoBehaviour
        {
            public int Enters;
            public int Exits;
            public string LastExit = "none";

            private void OnTriggerEnter(Collider other) => Enters++;

            private void OnTriggerExit(Collider other)
            {
                Exits++;
                LastExit = other.name;
            }
        }
    }
}
