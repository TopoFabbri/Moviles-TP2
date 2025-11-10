using System;
using System.Collections.Generic;
using Game.World.GameManager;
using UnityEngine.SceneManagement;

namespace Game.World.UI
{
    internal static class LevelList
    {
        private static readonly List<string> Levels = new();

        private static string MainMenu => SceneUtility.GetScenePathByBuildIndex(0);

        internal static List<string> GetLevels()
        {
            if (Levels.Count > 0) return Levels;
            
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                
                if (scenePath.Contains("Level", StringComparison.CurrentCultureIgnoreCase))
                    Levels.Add(scenePath);
            }

            return Levels;
        }

        internal static void LoadCurrentLevel()
        {
            if (Levels.Count == 0)
                GetLevels();

            SceneManager.LoadScene(Levels[GameSave.CurrentLevel]);
        }
        
        internal static void LoadNextLevel()
        {
            if (Levels.Count == 0)
                GetLevels();
            
            if (GameSave.CurrentLevel >= Levels.Count - 1)
            {
                GameSave.CurrentLevel = -1;
                SceneManager.LoadScene(MainMenu);
                return;
            }

            GameSave.CurrentLevel++;
            LoadCurrentLevel();
        }

        public static void LoadMainMenu(bool levelFinished)
        {
            if (levelFinished)
            {
                if (GameSave.CurrentLevel >= Levels.Count - 1)
                    GameSave.CurrentLevel = -1;
                else
                    GameSave.CurrentLevel++;
            }
            
            SceneManager.LoadScene(MainMenu);
        }
    }
}