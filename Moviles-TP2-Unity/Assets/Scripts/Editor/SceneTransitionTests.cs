using Game.World.UI;
using NUnit.Framework;
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
    }
}
