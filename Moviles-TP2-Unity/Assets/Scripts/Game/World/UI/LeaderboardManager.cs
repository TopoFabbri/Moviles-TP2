#if UNITY_ANDROID
using Game.World.GameManager;
using GooglePlayGames;
#endif

namespace Game.World.UI
{
    internal static class LeaderboardManager
    {
        internal static void ShowLeaderboard()
        {
#if UNITY_ANDROID
            PlayGamesPlatform.Instance.ShowLeaderboardUI();
#endif
        }

        internal static void UpdateLeaderboard()
        {
            float score = 0f;
            
            for (int i = 0; i < LevelList.GetLevels().Count; i++)
            {
                float currentLevelTime = GameSave.GetLevelTime(i);
                
                if (currentLevelTime < 0f)
                    return;
                
                score += GameSave.GetLevelTime(i);
            }
            
#if UNITY_ANDROID
            PlayGamesPlatform.Instance.ReportScore((long)score, GameSave.GodMode ? GPGSIds.leaderboard_total_time_godmode : GPGSIds.leaderboard_total_time, _ => { });
#endif
        }
    }
}