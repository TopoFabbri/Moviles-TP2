using Game.Events;
using Game.World.Ads;
using Game.World.GameManager;
using Tools.EventSystem;
using Tools.FSM;
using Tools.ServiceProvider;
using Tools.Tween;
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
        
        private void OnEnable()
        {
            SceneTransitionService.PlayStateTransitionFade(0.3f);
            transform.localScale = Vector3.zero;
            StartCoroutine(Ease.Tween(0.28f, t => transform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, t), Ease.OutBack, unscaled: true));
        }

        private void OnDestroy()
        {
            ServiceProvider.GetService<EventSystem>().Unsubscribe<ChangedStateEvent<States>>(OnGameStateChanged);
        }

        private void Update()
        {
            if (ServiceProvider.TryGetService(out IAdService adService) && adService.IsAdShowing)
                return;

            if (PlayerInput.GetPlayerByIndex(0).currentControlScheme == "Gamepad" && !UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(firstSelected);
        }

        private void OnGameStateChanged(ChangedStateEvent<States> obj)
        {
            bool isGameOver = obj.state == States.GameOver;
            gameObject.SetActive(isGameOver);

            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

            if (isGameOver)
            {
                ServiceProvider.GetService<IAdService>().ShowInterstitial("GameOver");
            }
        }

        public void OnRestart()
        {
            ServiceProvider.GetService<EventSystem>().Raise<RestartLevelEvent>();
        }
        
        public void OnMainMenu()
        {
            FindFirstObjectByType<Analytics.LevelAnalyticsTracker>()?.NotifyLevelAbandoned("GameOver");
            ServiceProvider.GetService<LevelList>().LoadMainMenu(false);
        }
    }
}
