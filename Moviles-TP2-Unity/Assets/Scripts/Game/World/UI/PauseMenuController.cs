using System.Collections.Generic;
using Game.Events;
using Game.World.GameManager;
using Tools.EventSystem;
using Tools.FSM;
using Tools.ServiceProvider;
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
            ServiceProvider.GetService<EventSystem>().Subscribe<CancelAction>(OnCancel);
            ServiceProvider.GetService<EventSystem>().Subscribe<ChangedStateEvent<States>>(OnChangedState);

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            ServiceProvider.GetService<EventSystem>().Unsubscribe<CancelAction>(OnCancel);
            ServiceProvider.GetService<EventSystem>().Unsubscribe<ChangedStateEvent<States>>(OnChangedState);
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
            ServiceProvider.GetService<EventSystem>().Raise<PauseAction>();
        }
        
        public void OnRestart()
        {
            ServiceProvider.GetService<EventSystem>().Raise<RestartLevelEvent>();
        }
        
        public void OnReturnToMenu()
        {
            ServiceProvider.GetService<LevelList>().LoadMainMenu(false);
        }
        
        public void OnCancel(CancelAction obj)
        {
            if (subMenuBg.activeInHierarchy)
                CloseAllSubMenus();
        }
    }
}