using Game.Events;
using Game.World.GameManager;
using Tools.EventSystem;
using Tools.FSM;
using Tools.ServiceProvider;
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
            
            ServiceProvider.GetService<EventSystem>().Subscribe<ChangedStateEvent<States>>(OnGameStateChanged);
        }
        
        private void OnDestroy()
        {
            ServiceProvider.GetService<EventSystem>().Unsubscribe<ChangedStateEvent<States>>(OnGameStateChanged);
        }

        private void Update()
        {
            if (PlayerInput.GetPlayerByIndex(0).currentControlScheme == "Gamepad" && !UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstSelected);
        }

        private void OnGameStateChanged(ChangedStateEvent<States> obj)
        {
            gameObject.SetActive(obj.state == States.GameOver);

            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        public void OnRestart()
        {
            ServiceProvider.GetService<EventSystem>().Raise<RestartLevelEvent>();
        }
        
        public void OnMainMenu()
        {
            ServiceProvider.GetService<LevelList>().LoadMainMenu(false);
        }
    }
}
