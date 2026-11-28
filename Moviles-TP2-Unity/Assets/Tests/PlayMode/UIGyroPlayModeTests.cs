using System.Collections;
using Game.Events;
using Game.World.Character;
using Game.World.Targets;
using NUnit.Framework;
using Tools.EventSystem;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    public class UIGyroPlayModeTests
    {
        private static readonly string[] LevelScenes =
        {
            "Level Tutorial",
            "Level 1",
            "Level 2",
            "Level 3",
            "Level 4"
        };

        [UnityTest]
        public IEnumerator Pointer_InAllLevels_PointsTowardCurrentTarget([ValueSource(nameof(LevelScenes))] string sceneName)
        {
            LogAssert.ignoreFailingMessages = true;
            try
            {
                AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                while (!load.isDone) yield return null;

                yield return null;
                yield return null;
                yield return null;

                var gyroCtrl = Object.FindFirstObjectByType<UIGyroController>();
                Assert.IsNotNull(gyroCtrl, $"UIGyroController should exist in {sceneName}.");

                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var pointer = typeof(UIGyroController).GetField("pointer", flags)?.GetValue(gyroCtrl) as Transform;
                var droneTransform = typeof(UIGyroController).GetField("droneTransform", flags)?.GetValue(gyroCtrl) as Transform;

                Assert.IsNotNull(pointer, "pointer should be assigned.");
                Assert.IsNotNull(droneTransform, "droneTransform should be assigned.");
                Assert.IsNotNull(ObstacleScore.Current, $"ObstacleScore.Current should not be null in {sceneName}.");

                AssertPointerPointsAtTarget(pointer, droneTransform, $"{sceneName} initial");

                // Simulate rotating and moving the drone, then passing the first ring
                var events = ServiceProvider.GetService<EventSystem>();
                events.Raise<LookAction>(new Vector3(1f, 0.5f, 0f));
                events.Raise<MoveAction>(Vector3.forward);
                for (int i = 0; i < 15; i++) yield return null;

                AssertPointerPointsAtTarget(pointer, droneTransform, $"{sceneName} after move+rotate");

                if (ObstacleScore.Current != null)
                {
                    ObstacleScore.Current.MarkDone();
                    yield return null;
                    yield return null;
                    if (ObstacleScore.Current != null)
                        AssertPointerPointsAtTarget(pointer, droneTransform, $"{sceneName} after MarkDone");
                }
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        private static void AssertPointerPointsAtTarget(Transform pointer, Transform droneTransform, string context)
        {
            Vector3 expectedDir = (ObstacleScore.Current.transform.position - droneTransform.position).normalized;
            Vector3 actualForward = pointer.forward.normalized;
            float dot = Vector3.Dot(expectedDir, actualForward);
            Assert.Greater(dot, 0.99f,
                $"[{context}] Pointer forward {actualForward} does not match expected direction {expectedDir} (dot={dot:F3})!");
        }
    }
}
