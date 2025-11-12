using System;
using System.Collections.Generic;
using Game.Events;
using Game.World.GameManager;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TMPro;
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
        [SerializeField] private TextMeshProUGUI versionTxt;
        
        private const string LoggerClassName = "com.fabbri2025.topologger.TopoLogger";

#if UNITY_ANDROID
        private AndroidJavaClass loggerClass;
#endif

        private void Start()
        {
            if (versionTxt)
                versionTxt.text = $"v{Application.version}";
            
            InitLogger();

            foreach (InputAction inputAction in PlayerInput.GetPlayerByIndex(0).actions)
                inputAction.Disable();

            if (PlayerInput.all.Count > 0)
                PlayerInput.GetPlayerByIndex(0)?.actions.FindActionMap("UI").Enable();

            EventSystem.Subscribe<CancelAction>(OnCancel);

            CloseAllSubMenus();

            PlayLogin();
        }

        private void InitLogger()
        {
#if UNITY_ANDROID
            try
            {
                loggerClass = new AndroidJavaClass(LoggerClassName);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
#endif
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<CancelAction>(OnCancel);
        }

        private void Update()
        {
            if (PlayerInput.GetPlayerByIndex(0)?.currentControlScheme == "Gamepad")
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

        public void OnLogger()
        {
#if UNITY_ANDROID
            loggerClass?.CallStatic<AndroidJavaClass>("SendLog", "LogSent", 1);
#endif
        }

        public void OnExit()
        {
            Application.Quit();

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }

        [ContextMenu("Reset All Saves")]
        private void ResetAllSaves()
        {
            GameSave.Reset();
        }

        private static void PlayLogin()
        {
#if UNITY_ANDROID
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
            PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
#endif
        }

#if UNITY_ANDROID
        private static void ProcessAuthentication(SignInStatus status)
        {
            if (status != SignInStatus.Success) return;

            PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_first_flight);
        }
#endif
    }
}