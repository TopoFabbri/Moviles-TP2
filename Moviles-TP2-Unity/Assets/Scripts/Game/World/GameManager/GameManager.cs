using Game.Audio;
using Game.World.Ads;
using Game.World.Character;
using Game.World.GameManager.GameStates;
using Game.World.Targets;
using Tools.FSM;
using Tools.ServiceProvider;
using UnityEngine;
using Time = Tools.Time.Time;

namespace Game.World.GameManager
{
    internal sealed class GameManager : MonoBehaviour
    {
        internal enum Flags
        {
            Crashed,
            Won,
            Paused,
            Play,
            Restart,
            Popup
        }

        [SerializeField] private TargetSpawner targetSpawner;
        [SerializeField] private DroneController drone;
        
        private FSM<States, Flags> fsm;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            ResetTimeScaleIfNoAd();
            GameAudioService.SetPausedDucking(false);
            ServiceProvider.GetService<Time>().Start();

            InitializeFiniteStateMachine();
        }

        private void OnDestroy()
        {
            fsm?.Shutdown();
            ResetTimeScaleIfNoAd();
            GameAudioService.SetPausedDucking(false);
        }

        private static void ResetTimeScaleIfNoAd()
        {
            if (!ServiceProvider.TryGetService(out IAdService adService) || !adService.IsAdShowing)
                UnityEngine.Time.timeScale = 1f;
        }

        private void Update()
        {
            ServiceProvider.GetService<Time>().Update();
            fsm.Tick();
        }

        private void InitializeFiniteStateMachine()
        {
            fsm = new FSM<States, Flags>(States.Restart);

            fsm.AddState<RestartLevelState>(States.Restart, onTickParameters: () => new object[] { targetSpawner, drone });
            fsm.AddState<PlayState>(States.Play, onTickParameters: () => new object[] { drone });
            fsm.AddState<GameOverState>(States.GameOver);
            fsm.AddState<GameOverState>(States.Win);
            fsm.AddState<PauseState>(States.Paused);
            fsm.AddState<PopupState>(States.Popup);

            fsm.SetTransition(States.Restart, Flags.Play, States.Play);
            
            fsm.SetTransition(States.Play, Flags.Crashed, States.GameOver);
            fsm.SetTransition(States.Play, Flags.Paused, States.Paused);
            fsm.SetTransition(States.Play, Flags.Won, States.Win);
            fsm.SetTransition(States.Play, Flags.Popup, States.Popup);
            
            fsm.SetTransition(States.GameOver, Flags.Play, States.Play);
            fsm.SetTransition(States.GameOver, Flags.Restart, States.Restart);
            
            fsm.SetTransition(States.Paused, Flags.Play, States.Play);
            fsm.SetTransition(States.Paused, Flags.Restart, States.Restart);
            
            fsm.SetTransition(States.Popup, Flags.Play, States.Play);
            
            fsm.SetTransition(States.Win, Flags.Restart, States.Restart);
        }
    }
}