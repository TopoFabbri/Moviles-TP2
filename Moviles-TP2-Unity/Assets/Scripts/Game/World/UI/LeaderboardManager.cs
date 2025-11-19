
using GooglePlayGames.com.google.play.games.Runtime.Scripts.ISocialPlatform;
using Tools.ServiceProvider;
#if UNITY_ANDROID
using Game.World.GameManager;
#endif

namespace Game.World.UI
{
    internal class LeaderboardManager : IService
    {
        internal void ShowLeaderboard()
        {
#if UNITY_ANDROID
            PlayGamesPlatform.Instance.ShowLeaderboardUI();
#endif
        }

        internal void UpdateLeaderboard()
        {
            float score = 0f;
            
            for (int i = 0; i < ServiceProvider.GetService<LevelList>().GetLevels().Count; i++)
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