using System.Collections.Generic;
using Game.Events;
using Tools.EventSystem;
using Tools.ServiceProvider;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Game.World.UI
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> popups;
        
        private void Awake()
        {
            foreach (GameObject popup in popups)
            {
                popup.SetActive(false);
            }
        }

        public void OnResume()
        {
            ServiceProvider.GetService<EventSystem>().Raise<CancelAction>();
        }

        private void Update()
        {
            if (Touchscreen.current == null) return;

            GameObject activePopup = null;
            foreach (GameObject popup in popups)
            {
                if (popup != null && popup.activeInHierarchy)
                {
                    activePopup = popup;
                    break;
                }
            }

            if (activePopup == null) return;

            RectTransform rectTransform = activePopup.GetComponent<RectTransform>();
            Canvas canvas = activePopup.GetComponentInParent<Canvas>();
            Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? canvas.worldCamera
                : null;

            foreach (TouchControl touch in Touchscreen.current.touches)
            {
                if (!touch.press.wasPressedThisFrame) continue;

                Vector2 pos = touch.position.ReadValue();
                if (rectTransform == null || RectTransformUtility.RectangleContainsScreenPoint(rectTransform, pos, cam))
                {
                    OnResume();
                    break;
                }
            }
        }
    }
}
