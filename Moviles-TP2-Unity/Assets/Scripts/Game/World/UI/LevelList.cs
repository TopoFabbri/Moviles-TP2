using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Game.World.UI
{
    internal static class LevelList
    {
        private static readonly List<string> Levels = new();
        private static int _currentLevel = -1;

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
        
        internal static void LoadNextLevel()
        {
            if (Levels.Count == 0)
                GetLevels();
            
            if (_currentLevel >= Levels.Count - 1)
            {
                _currentLevel = -1;
                SceneManager.LoadScene(MainMenu);
                return;
            }

            _currentLevel++;
            SceneManager.LoadScene(Levels[_currentLevel]);
        }

        public static void LoadMainMenu()
        {
            SceneManager.LoadScene(MainMenu);
        }
    }
}