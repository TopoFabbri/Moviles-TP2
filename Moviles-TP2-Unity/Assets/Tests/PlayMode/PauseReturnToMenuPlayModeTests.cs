using System.Collections;
using Game.Audio;
using Game.Events;
using Game.World.Character;
using Game.World.GameManager;
using Game.World.UI;
using NUnit.Framework;
using Tools.EventSystem;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.PlayMode
{
    public class PauseReturnToMenuPlayModeTests
    {
        [UnityTest]
        public IEnumerator ReturningToMenuFromPause_ThenPressingPlay_RestoresTimeScaleAndMovement()
        {
            LogAssert.ignoreFailingMessages = true;
            int previousLevel = GameSave.CurrentLevel;

            try
            {
                var levelList = ServiceProvider.GetService<LevelList>();
                var levels = levelList.GetLevels();
                int level1Index = levels.FindIndex(path => path.Contains("Level 1"));
                Assert.GreaterOrEqual(level1Index, 0, "Level 1 scene should exist in LevelList.");
                string level1Path = levels[level1Index];

                AsyncOperation load = SceneManager.LoadSceneAsync(level1Path, LoadSceneMode.Single);
                while (!load.isDone) yield return null;
                yield return null;
                yield return null;

                Assert.AreEqual(1f, Time.timeScale, 0.0001f);

                // Pause the level and return to menu via PauseMenuController.
                ServiceProvider.GetService<EventSystem>().Raise<PauseAction>();
                yield return null;
                yield return null;
                Assert.AreEqual(0f, Time.timeScale, 0.0001f, "PauseState should set Time.timeScale to 0.");

                PauseMenuController pauseMenu = Object.FindFirstObjectByType<PauseMenuController>(FindObjectsInactive.Include);
                Assert.IsNotNull(pauseMenu, "PauseMenuController should exist in Level 1.");
                pauseMenu.OnReturnToMenu();

                while (SceneTransitionService.IsLoading) yield return null;
                yield return null;

                Assert.AreEqual(1f, Time.timeScale, 0.0001f, "MainMenu should restore Time.timeScale to 1.");
                Assert.IsFalse(GameAudioService.IsPausedDucking, "MainMenu should clear pause audio ducking.");

                MainMenu mainMenu = Object.FindFirstObjectByType<MainMenu>();
                Assert.IsNotNull(mainMenu, "MainMenu should be loaded.");
                GameSave.CurrentLevel = level1Index;
                mainMenu.OnPlay();

                while (SceneTransitionService.IsLoading) yield return null;
                yield return null;
                yield return null;

                Assert.AreEqual(1f, Time.timeScale, 0.0001f, "Reloaded level should run at Time.timeScale == 1.");
                Assert.IsFalse(GameAudioService.IsPausedDucking, "Reloaded level should not have pause audio ducking.");

                DroneCollisionController pivotCtrl = Object.FindFirstObjectByType<DroneCollisionController>();
                Assert.IsNotNull(pivotCtrl, "DroneCollisionController should exist in Level 1.");
                Vector3 initialPos = pivotCtrl.transform.position;
                float startScaledTime = Time.time;

                ServiceProvider.GetService<EventSystem>().Raise<MoveAction>(Vector3.forward);
                float elapsed = 0f;
                while (elapsed < 0.35f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }

                Assert.Greater(Time.time - startScaledTime, 0.1f, "Scaled Time.time should advance so drone animations play.");
                Assert.Greater(
                    Vector3.Distance(initialPos, pivotCtrl.transform.position),
                    0.05f,
                    "Drone should move normally after returning to menu from pause and pressing Play.");
            }
            finally
            {
                GameSave.CurrentLevel = previousLevel;
                Time.timeScale = 1f;
                GameAudioService.SetPausedDucking(false);
                LogAssert.ignoreFailingMessages = false;
            }
        }
    }
}
