using System.Collections.Generic;
using Game.Events;
using Game.World.Character;
using Game.World.GameManager;
using Game.World.UI;
using Tools.EventSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    internal class InputListener : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string uiMap = "UI";
        [SerializeField] private List<string> schemes = new();

        internal static List<string> Schemes;
        
        private void Awake()
        {
            Schemes = schemes;
            
            playerInput.actions.FindActionMap(uiMap).Disable();
            
            EventSystem.Subscribe<PauseEvent>(OnPaused);
            EventSystem.Subscribe<ControlSchemeChangedEvent>(OnControlSchemeChanged);

            if (GameSave.ControlScheme < 0 || GameSave.ControlScheme >= schemes.Count)
                GameSave.ControlScheme = Application.platform == RuntimePlatform.WindowsPlayer ? 1 : 0;
            
            SwitchScheme(GameSave.ControlScheme);
        }

        private void OnControlSchemeChanged(ControlSchemeChangedEvent obj)
        {
            SwitchScheme(obj.schemeId);
        }

        private void SwitchScheme(int schemeId)
        {
            if (schemeId < 0 || schemeId > schemes.Count) return;
            
            foreach (string scheme in schemes)
                playerInput.actions.FindActionMap(scheme).Disable();
            
            playerInput.SwitchCurrentActionMap(schemes[schemeId]);
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<PauseEvent>(OnPaused);
            EventSystem.Unsubscribe<ControlSchemeChangedEvent>(OnControlSchemeChanged);
        }

        private void Update()
        {
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
            
            EventSystem.Raise<MoveAction>(inputVector);
        }
        
        public void OnLook(InputValue input)
        {
            Vector3 inputVector = input.Get<Vector3>();
            
            EventSystem.Raise<LookAction>(inputVector);
        }

        public void OnPause()
        {
            EventSystem.Raise<PauseAction>();
        }

        private void OnPaused(PauseEvent obj)
        {
            if (obj.paused)
                playerInput.actions.FindActionMap(uiMap).Disable();
            else
                playerInput.actions.FindActionMap(uiMap).Enable();
        }
    }
}
