using Game.Audio;
using Game.World.GameManager;
using Game.World.GameManager.GameStates;
using Game.World.UI;
using NUnit.Framework;
using Tools.FSM;
using UnityEngine;
using UnityEngine.TestTools;

namespace Editor
{
    public class SceneTransitionTests
    {
        [Test]
        public void LoadSceneAsync_IgnoresNullOrEmptyPath()
        {
            LogAssert.Expect(LogType.Error, "[SceneTransition] Cannot load null or empty scene path.");
            SceneTransitionService.LoadSceneAsync(string.Empty);
            Assert.IsFalse(SceneTransitionService.IsLoading);
        }

        [Test]
        public void LevelList_GetLevels_ReturnsConfiguredLevelScenes()
        {
            var levelList = new LevelList();
            var levels = levelList.GetLevels();

            Assert.IsNotNull(levels);
            Assert.Greater(levels.Count, 0);
            foreach (string path in levels)
                StringAssert.Contains("Level", path);
        }

        [Test]
        public void FsmShutdown_FromPauseState_RestoresTimeScaleAndAudioDucking()
        {
            float prevTimeScale = Time.timeScale;
            bool prevDucking = GameAudioService.IsPausedDucking;

            try
            {
                var fsm = new FSM<States, GameManager.Flags>(States.Restart);
                fsm.AddState<GameOverState>(States.Restart);
                fsm.AddState<PauseState>(States.Paused);
                fsm.SetTransition(States.Restart, GameManager.Flags.Paused, States.Paused);

                fsm.Transition(GameManager.Flags.Paused);
                Assert.AreEqual(0f, Time.timeScale, 0.0001f);
                Assert.IsTrue(GameAudioService.IsPausedDucking);

                fsm.Shutdown();
                Assert.AreEqual(1f, Time.timeScale, 0.0001f);
                Assert.IsFalse(GameAudioService.IsPausedDucking);

                Assert.DoesNotThrow(() => fsm.Tick());
            }
            finally
            {
                Time.timeScale = prevTimeScale;
                GameAudioService.SetPausedDucking(prevDucking);
            }
        }
    }
}
