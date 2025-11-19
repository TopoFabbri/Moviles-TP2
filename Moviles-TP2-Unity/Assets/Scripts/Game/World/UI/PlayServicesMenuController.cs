using GooglePlayGames.com.google.play.games.Runtime.Scripts.ISocialPlatform;
using Tools.ServiceProvider;
using UnityEngine;

namespace Game.World.UI
{
    internal sealed class PlayServicesMenuController : MonoBehaviour
    {
        public void ShowLeaderboard()
        {
            ServiceProvider.GetService<LeaderboardManager>().ShowLeaderboard();
        }

        public void ShowAchievements()
        {
#if UNITY_ANDROID
            PlayGamesPlatform.Instance.ShowAchievementsUI();
#endif
        }
    }
}