using Game.Audio;
using Game.Events;
using Game.World.UI;
using Tools.EventSystem;
using Tools.FSM;
using Tools.ServiceProvider;

namespace Game.World.GameManager.GameStates
{
    internal class PauseState : State
    {
        private bool resume;
        private bool restart;
        
        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () =>
            {
                UnityEngine.Time.timeScale = 0f;
                GameAudioService.SetPausedDucking(true);
            });
            actions.AddMainThreadBehaviour(0, () => ServiceProvider.GetService<EventSystem>().Subscribe<CancelAction>(OnCancelAction));
            actions.AddMainThreadBehaviour(0, () => ServiceProvider.GetService<EventSystem>().Subscribe<PauseAction>(OnPauseAction));
            actions.AddMainThreadBehaviour(0, () => ServiceProvider.GetService<EventSystem>().Subscribe<RestartLevelEvent>(OnRestart));
            actions.AddMultiThreadableBehaviour(0, () => resume = false);
            actions.AddMultiThreadableBehaviour(0, () => restart = false);
            
            return actions;
        }

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();

            actions.AddMainThreadBehaviour(0, () =>
            {
                if (resume)
                    flag.Invoke(GameManager.Flags.Play);
                else if (restart)
                    flag.Invoke(GameManager.Flags.Restart);
            });

            return actions;
        }

        public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
        {
            BehaviourActions actions = Pool.Get<BehaviourActions>();
            
            actions.AddMainThreadBehaviour(0, () =>
            {
                UnityEngine.Time.timeScale = 1f;
                GameAudioService.SetPausedDucking(false);
            });
            actions.AddMainThreadBehaviour(0, () => ServiceProvider.GetService<EventSystem>().Unsubscribe<CancelAction>(OnCancelAction));
            actions.AddMainThreadBehaviour(0, () => ServiceProvider.GetService<EventSystem>().Unsubscribe<PauseAction>(OnPauseAction));
            actions.AddMainThreadBehaviour(0, () => ServiceProvider.GetService<EventSystem>().Unsubscribe<RestartLevelEvent>(OnRestart));
            
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

        private void OnRestart(RestartLevelEvent obj)
        {
            restart = true;
        }
    }
}