using System;
using System.Collections.Generic;
using Game.World.GameManager;
using Tools.ServiceProvider;
using UnityEngine.SceneManagement;

namespace Game.World.UI
{
    internal class LevelList : IService
    {
        private readonly List<string> levels = new();

        private static string MainMenu => SceneUtility.GetScenePathByBuildIndex(0);

        internal List<string> GetLevels()
        {
            if (levels.Count > 0) return levels;
            
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                
                if (scenePath.Contains("Level", StringComparison.CurrentCultureIgnoreCase))
                    levels.Add(scenePath);
            }

            return levels;
        }

        internal void LoadCurrentLevel()
        {
            if (levels.Count == 0)
                GetLevels();

            SceneTransitionService.LoadSceneAsync(levels[GameSave.CurrentLevel]);
        }
        
        internal void LoadNextLevel()
        {
            if (levels.Count == 0)
                GetLevels();
            
            if (GameSave.CurrentLevel >= levels.Count - 1)
            {
                GameSave.CurrentLevel = 0;
                SceneTransitionService.LoadSceneAsync(MainMenu);
                return;
            }

            GameSave.CurrentLevel++;
            LoadCurrentLevel();
        }

        public void LoadMainMenu(bool levelFinished)
        {
            if (levelFinished)
            {
                if (GameSave.CurrentLevel >= levels.Count - 1)
                    GameSave.CurrentLevel = 0;
                else
                    GameSave.CurrentLevel++;
            }
            
            SceneTransitionService.LoadSceneAsync(MainMenu);
        }
    }
}