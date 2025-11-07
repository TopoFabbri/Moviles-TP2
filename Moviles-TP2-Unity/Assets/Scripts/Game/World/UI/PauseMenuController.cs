using System.Collections.Generic;
using Game.Events;
using Game.Input;
using Game.World.GameManager;
using Tools.EventSystem;
using Tools.FSM;
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

        private void Start()
        {
            EventSystem.Subscribe<CancelAction>(OnCancel);
            EventSystem.Subscribe<ChangedStateEvent<States>>(OnChangedState);

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<CancelAction>(OnCancel);
            EventSystem.Unsubscribe<ChangedStateEvent<States>>(OnChangedState);
        }

        private void Update()
        {
            if (!UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject && PlayerInput.GetPlayerByIndex(0).currentControlScheme == "Gamepad")
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(pauseFirstSelected);
        }

        public void CloseAllSubMenus()
        {
            foreach (GameObject subMenu in subMenus)
                subMenu.SetActive(false);

            subMenuBg.SetActive(false);
        }

        private void OnChangedState(ChangedStateEvent<States> obj)
        {
            pauseMenu.SetActive(obj.state == States.Paused);
            
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

            CloseAllSubMenus();
        }

        public void OnResume()
        {
            EventSystem.Raise<PauseAction>();
        }
        
        public void OnRestart()
        {
            EventSystem.Raise<RestartLevelEvent>();
        }
        
        public void OnReturnToMenu()
        {
            LevelList.LoadMainMenu();
        }
        
        public void OnCancel(CancelAction obj)
        {
            if (subMenuBg.activeInHierarchy)
                CloseAllSubMenus();
        }
    }
}