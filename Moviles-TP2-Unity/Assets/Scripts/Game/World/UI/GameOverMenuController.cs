using Game.World.GameManager;
using Tools.EventSystem;
using Tools.FSM;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.World.UI
{
    internal sealed class GameOverMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject firstSelected;
        
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
            if (PlayerInput.GetPlayerByIndex(0).currentControlScheme == "Gamepad" && !UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstSelected);
        }

        private void OnGameStateChanged(ChangedStateEvent<States> obj)
        {
            gameObject.SetActive(obj.state == States.GameOver);
        }

        public void OnRestart()
        {
            EventSystem.Raise<RestartLevelEvent>();
        }
        
        public void OnMainMenu()
        {
            LevelList.LoadMainMenu();
        }
    }
}
