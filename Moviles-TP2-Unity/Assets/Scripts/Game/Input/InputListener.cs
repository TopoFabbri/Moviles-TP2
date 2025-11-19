using System.Collections.Generic;
using Game.Events;
using Game.World.GameManager;
using Tools.EventSystem;
using Tools.FSM;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    internal class InputListener : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string uiMap = "UI";
        [SerializeField] private List<string> actionMaps = new();
        [SerializeField] private bool inMenu;
        
        internal static List<string> ActionMaps;

        private void Awake()
        {
            if (ActionMaps == null)
                ActionMaps = actionMaps;
            else
                actionMaps = ActionMaps;

            if (inMenu) return;

            ServiceProvider.GetService<EventSystem>().Subscribe<ChangedStateEvent<States>>(OnStateChanged);
            ServiceProvider.GetService<EventSystem>().Subscribe<ControlSchemeChangedEvent>(OnControlSchemeChanged);

            if (GameSave.ControlScheme < 0 || GameSave.ControlScheme >= actionMaps.Count)
                GameSave.ControlScheme = Application.platform == RuntimePlatform.WindowsPlayer ? 1 : 0;

            SwitchScheme(GameSave.ControlScheme);
        }

        private void OnDestroy()
        {
            if (inMenu) return;
            
            ServiceProvider.GetService<EventSystem>().Unsubscribe<ChangedStateEvent<States>>(OnStateChanged);
            ServiceProvider.GetService<EventSystem>().Unsubscribe<ControlSchemeChangedEvent>(OnControlSchemeChanged);
        }

        private void OnControlSchemeChanged(ControlSchemeChangedEvent obj)
        {
            SwitchScheme(obj.schemeId);
        }

        private void SwitchScheme(int schemeId)
        {
            if (schemeId < 0 || schemeId > actionMaps.Count) return;

            foreach (string scheme in actionMaps)
                playerInput.actions.FindActionMap(scheme).Disable();

            playerInput.SwitchCurrentActionMap(actionMaps[schemeId]);
        }

        private void Update()
        {
            if (inMenu) return;
            
            if (Accelerometer.current == null) return;

            if (GameSave.ControlScheme == 0)
            {
                if (!Accelerometer.current.enabled)
                    InputSystem.EnableDevice(Accelerometer.current);
            }
            else
            {
                if (Accelerometer.current.enabled)
                    InputSystem.DisableDevice(Accelerometer.current);
            }
        }

        public void OnMove(InputValue input)
        {
            Vector3 inputVector = input.Get<Vector3>();

            ServiceProvider.GetService<EventSystem>().Raise<MoveAction>(inputVector);
        }

        public void OnLook(InputValue input)
        {
            Vector3 inputVector = input.Get<Vector3>();

            inputVector.x *= GameSave.InvertCamX ? -1 : 1;
            inputVector.y *= GameSave.InvertCamY ? 1 : -1;
            inputVector *= GameSave.RotationSensitivity / 100f;

            ServiceProvider.GetService<EventSystem>().Raise<LookAction>(inputVector);
        }

        public void OnCalibrate()
        {
            ServiceProvider.GetService<EventSystem>().Raise<CalibrateAction>();
        }

        public void OnPause()
        {
            ServiceProvider.GetService<EventSystem>().Raise<PauseAction>();
        }

        public void OnCancel()
        {
            ServiceProvider.GetService<EventSystem>().Raise<CancelAction>();
        }

        private void OnStateChanged(ChangedStateEvent<States> obj)
        {
            if (obj.state == States.Play)
            {
                playerInput.actions.FindActionMap(actionMaps[GameSave.ControlScheme]).Enable();
                playerInput.actions.FindActionMap(uiMap).Disable();
            }
            else
            {
                playerInput.actions.FindActionMap(actionMaps[GameSave.ControlScheme]).Disable();
                playerInput.actions.FindActionMap(uiMap).Enable();
            }
        }
    }
}