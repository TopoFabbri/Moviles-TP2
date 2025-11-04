using System.Collections.Generic;
using Game.Input;
using Tools.EventSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.World.UI
{
    internal sealed class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private List<GameObject> subMenus;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject subMenuBg;
        [SerializeField] private GameObject pauseFirstSelected;

        private bool paused;

        private void Start()
        {
            EventSystem.Subscribe<PauseAction>(OnPause);

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<PauseAction>(OnPause);
        }

        public void CloseAllSubMenus()
        {
            foreach (GameObject subMenu in subMenus)
                subMenu.SetActive(false);

            subMenuBg.SetActive(false);
        }

        public void TogglePause()
        {
            paused = !paused;

            pauseMenu.SetActive(paused);

            CloseAllSubMenus();
            
            if (PlayerInput.GetPlayerByIndex(0).currentControlScheme == "Gamepad")
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(pauseFirstSelected);

            EventSystem.Raise<PauseEvent>(paused);
        }

        private void OnPause(PauseAction obj)
        {
            TogglePause();
        }
    }
}