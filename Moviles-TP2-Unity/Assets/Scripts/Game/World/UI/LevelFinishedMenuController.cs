using System;
using Game.World.GameManager;
using TMPro;
using Tools.EventSystem;
using Tools.FSM;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.World.UI
{
    internal sealed class LevelFinishedMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject firstSelected;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private string titleText = "Time: ";

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
            float seconds = GameTime.Timer.TimeElapsed;
            int minutes = (int)(seconds / 60f);
            seconds -= minutes * 60f;
            int centiseconds = Mathf.RoundToInt((seconds % 1f) * 100f);
            seconds %= 60f;

            title.text = titleText + $"{minutes:00}:{seconds:00}.{centiseconds:00}";
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