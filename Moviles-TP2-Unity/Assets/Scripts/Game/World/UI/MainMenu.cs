using System.Collections.Generic;
using Game.CloudSave;
using Game.Events;
using Game.World.GameManager;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TMPro;
using Tools.EventSystem;
using Tools.ServiceProvider;
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
        
        private void Start()
        {
            if (versionTxt)
                versionTxt.text = $"v{Application.version}";
            
            foreach (InputAction inputAction in PlayerInput.GetPlayerByIndex(0).actions)
                inputAction.Disable();

            if (PlayerInput.all.Count > 0)
                PlayerInput.GetPlayerByIndex(0)?.actions.FindActionMap("UI").Enable();

            ServiceProvider.GetService<EventSystem>().Subscribe<CancelAction>(OnCancel);

            CloseAllSubMenus();

            PlayLogin();
        }

        private void OnDestroy()
        {
            ServiceProvider.GetService<EventSystem>().Unsubscribe<CancelAction>(OnCancel);
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

        public void OnLogger()
        {
        }
        
        public void OnPlay()
        {
            ServiceProvider.GetService<LevelList>().LoadCurrentLevel();
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

        [ContextMenu("Simulate Fresh Install (Cloud Save)")]
        private void SimulateFreshInstallCloudSave()
        {
            GameSave.Reset();
            MockCloudSaveProvider.SeedMockCloudSave(new CloudSaveData
            {
                currentLevel = 3,
                credits = 500,
                addedSpeed = 15f,
                savedAtUtcTicks = System.DateTime.UtcNow.Ticks
            });
            Debug.Log("[CloudSave] Simulated fresh install with mock cloud save (Level 3, 500 Credits, +15 Speed). Enter Play Mode to test.");
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
            CloudSaveService.Instance.OnPlayServicesAuthenticated();
        }
#endif
    }
}