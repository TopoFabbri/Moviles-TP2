using Game.Events;
using Game.World.Ads;
using Game.World.GameManager;
using TMPro;
using Tools.EventSystem;
using Tools.FSM;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.InputSystem;
using Time = Tools.Time.Time;

namespace Game.World.UI
{
    internal sealed class LevelFinishedMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject firstSelected;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI bestTime;
        [SerializeField] private TextMeshProUGUI aceTime;
        [SerializeField] private string titleText = "Time: ";
        [SerializeField] private string bestTimeText = "Best Time: ";
        [SerializeField] private string aceTimeText = "Ace Time: ";

        private void Awake()
        {
            gameObject.SetActive(false);

            ServiceProvider.GetService<EventSystem>().Subscribe<ChangedStateEvent<States>>(OnGameStateChanged);
        }

        private void OnDestroy()
        {
            ServiceProvider.GetService<EventSystem>().Unsubscribe<ChangedStateEvent<States>>(OnGameStateChanged);
        }

        private void Update()
        {
            if (ServiceProvider.TryGetService(out IAdService adService) && adService.IsAdShowing)
                return;

            if (!UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject && PlayerInput.GetPlayerByIndex(0).currentControlScheme == "Gamepad")
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstSelected);
        }

        private void OnEnable()
        {
            float time = ServiceProvider.GetService<GameTime>().Timer.TimeElapsed;
            
            if (GameSave.GetLevelTime(GameSave.CurrentLevel) > time || GameSave.GetLevelTime(GameSave.CurrentLevel) <= 0f)
                GameSave.SetLevelTime(GameSave.CurrentLevel, time);

            ServiceProvider.GetService<Time>().ConvertToMinutes(time, out int minutes, out int seconds, out int centiseconds);
            title.text = titleText + $"{minutes:00}:{seconds:00}.{centiseconds:00}";
            
            ServiceProvider.GetService<Time>().ConvertToMinutes(GameSave.GetLevelTime(GameSave.CurrentLevel), out minutes, out seconds, out centiseconds);
            bestTime.text = bestTimeText + $"{minutes:00}:{seconds:00}.{centiseconds:00}";
            
            ServiceProvider.GetService<Time>().ConvertToMinutes(LevelManager.AceTime, out minutes, out seconds, out centiseconds);
            aceTime.text = aceTimeText + $"{minutes:00}:{seconds:00}.{centiseconds:00}";
            
            ServiceProvider.GetService<LeaderboardManager>().UpdateLeaderboard();
        }

        private void OnDisable()
        {
            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
        }

        private void OnGameStateChanged(ChangedStateEvent<States> obj)
        {
            bool isWin = obj.state == States.Win;
            gameObject.SetActive(isWin);

            if (isWin)
            {
                ServiceProvider.GetService<IAdService>().ShowInterstitial("LevelWin");
            }
        }

        public void OnRestart()
        {
            ServiceProvider.GetService<EventSystem>().Raise<RestartLevelEvent>();
        }

        public void OnMainMenu()
        {
            ServiceProvider.GetService<LevelList>().LoadMainMenu(true);
        }

        public void OnNextLevel()
        {
            ServiceProvider.GetService<LevelList>().LoadNextLevel();
        }
    }
}