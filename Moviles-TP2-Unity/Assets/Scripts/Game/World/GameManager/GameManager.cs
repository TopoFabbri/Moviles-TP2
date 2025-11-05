using Game.World.Character;
using Game.World.GameManager.GameStates;
using Game.World.Targets;
using Tools.FSM;
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
            Play
        }

        [SerializeField] private TargetSpawner targetSpawner;
        [SerializeField] private DroneController drone;

        private FSM<States, Flags> fsm;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Time.Start();

            fsm = new FSM<States, Flags>(States.GameOver);

            fsm.AddState<GameOverState>(States.GameOver, onExitParameters: () => new object[] { targetSpawner, drone });
            fsm.AddState<PlayState>(States.Play, onEnterParameters: () => new object[] { targetSpawner }, onTickParameters: () => new object[] { drone });

            fsm.SetTransition(States.GameOver, Flags.Play, States.Play);
            fsm.SetTransition(States.Play, Flags.Crashed, States.GameOver);
        }

        private void Start()
        {
            fsm.Transition(Flags.Play);
        }

        private void Update()
        {
            Time.Update();
            fsm.Tick();
        }
    }
}