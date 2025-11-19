using Game.Events;
using Tools.EventSystem;
using Tools.FSM;
using Tools.ServiceProvider;
using UnityEngine;

namespace Game.World.GameManager.GameStates
{
    internal class PopupState : State
    {
        private bool resume;
        private GameObject popup;

        public PopupState()
        {
            ServiceProvider.GetService<EventSystem>().Subscribe<TriggerPopupEvent>(OnTriggerPopup);
        }
        
        ~PopupState()
        {
            ServiceProvider.GetService<EventSystem>().Unsubscribe<TriggerPopupEvent>(OnTriggerPopup);
        }

        private void OnTriggerPopup(TriggerPopupEvent obj)
        {
            popup = obj.popup;
        }

        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();

            actions.AddMainThreadBehaviour(0, () => ServiceProvider.GetService<EventSystem>().Subscribe<CancelAction>(OnCancelAction));
            actions.AddMainThreadBehaviour(0, () => ServiceProvider.GetService<EventSystem>().Subscribe<PauseAction>(OnPauseAction));
            actions.AddMainThreadBehaviour(0, () => resume = false);

            return actions;
        }

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();

            actions.AddMainThreadBehaviour(0, () =>
            {
                if (resume) flag.Invoke(GameManager.Flags.Play);
            });

            return actions;
        }

        public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () => popup.SetActive(false));
            
            return actions;
        }

        private void OnCancelAction(CancelAction obj)
        {
            Resume();
        }

        private void OnPauseAction(PauseAction obj)
        {
            Resume();
        }

        private void Resume()
        {
            resume = true;
        }
    }
}