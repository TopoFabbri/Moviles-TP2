using System.Collections.Generic;
using Game.Input;
using Tools.EventSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.World.UI
{
    internal sealed class MainMenu : MonoBehaviour
    {
        [SerializeField] private List<GameObject> subMenus;
        [SerializeField] private GameObject subMenuBg;
        [SerializeField] private GameObject firstSelected;

        private void Start()
        {
            foreach (InputAction inputAction in PlayerInput.GetPlayerByIndex(0).actions)
                inputAction.Disable();

            PlayerInput.GetPlayerByIndex(0).actions.FindActionMap("UI").Enable();

            EventSystem.Subscribe<CancelAction>(OnCancel);
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<CancelAction>(OnCancel);
        }

        private void Update()
        {
            if (PlayerInput.GetPlayerByIndex(0).currentControlScheme == "Gamepad")
            {
                if (!UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject)
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstSelected);
            }
            else
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void CloseAllSubMenus()
        {
            foreach (GameObject subMenu in subMenus)
                subMenu.SetActive(false);

            subMenuBg.SetActive(false);
        }

        private void OnCancel(CancelAction obj)
        {
            if (subMenuBg.activeInHierarchy)
                CloseAllSubMenus();
        }

        public void OnPlay()
        {
            LevelList.LoadCurrentLevel();
        }

        public void OnExit()
        {
            Application.Quit();

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
    }
}