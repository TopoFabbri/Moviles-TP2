using System;
using Game.World.GameManager;
using TMPro;
using Tools.EventSystem;
using Tools.FSM;
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
        [SerializeField] private string titleText = "Time: ";
        [SerializeField] private string bestTimeText = "Best Time: ";

        private void Awake()
        {
            gameObject.SetActive(false);

            EventSystem.Subscribe<ChangedStateEvent<States>>(OnGameStateChanged);
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<ChangedStateEvent<States>>(OnGameStateChanged);
        }

        private void Update()
        {
            if (!UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject && PlayerInput.GetPlayerByIndex(0).currentControlScheme == "Gamepad")
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstSelected);
        }

        private void OnEnable()
        {
            float time = GameTime.Timer.TimeElapsed;
            
            if (GameSave.GetLevelTime(GameSave.CurrentLevel) > time || GameSave.GetLevelTime(GameSave.CurrentLevel) <= 0f)
                GameSave.SetLevelTime(GameSave.CurrentLevel, time);

            Time.ConvertToMinutes(time, out int minutes, out int seconds, out int centiseconds);
            title.text = titleText + $"{minutes:00}:{seconds:00}.{centiseconds:00}";
            
            Time.ConvertToMinutes(GameSave.GetLevelTime(GameSave.CurrentLevel), out minutes, out seconds, out centiseconds);
            bestTime.text = bestTimeText + $"{minutes:00}:{seconds:00}.{centiseconds:00}";
        }

        private void OnDisable()
        {
            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
        }

        private void OnGameStateChanged(ChangedStateEvent<States> obj)
        {
            gameObject.SetActive(obj.state == States.Win);
        }

        public void OnRestart()
        {
            EventSystem.Raise<RestartLevelEvent>();
        }

        public void OnMainMenu()
        {
            LevelList.LoadMainMenu(true);
        }

        public void OnNextLevel()
        {
            LevelList.LoadNextLevel();
        }
    }
}