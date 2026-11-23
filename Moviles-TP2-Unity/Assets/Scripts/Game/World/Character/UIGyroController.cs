using Game.World.Targets;
using UnityEngine;

namespace Game.World.Character
{
    public class UIGyroController : MonoBehaviour
    {
        [SerializeField] private Transform gyro;
        [SerializeField] private Transform pointer;
        [SerializeField] private Transform droneTransform;
        [SerializeField] private Camera overlayCam;
        [SerializeField] private Vector2 relativePos = new(0.8f, 0.8f);
        [SerializeField] private RectTransform targetRect;

        private Canvas _targetCanvas;

        private void Update()
        {
            gyro.rotation = Quaternion.identity;

            if (targetRect != null)
            {
                if (_targetCanvas == null)
                    _targetCanvas = targetRect.GetComponentInParent<Canvas>();

                Camera uiCam = (_targetCanvas != null && _targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                    ? _targetCanvas.worldCamera
                    : null;
                Vector3 worldCenter = targetRect.TransformPoint(targetRect.rect.center);
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(uiCam, worldCenter);
                gyro.position = overlayCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            }
            else
            {
                float posX = overlayCam.rect.x + overlayCam.rect.width * relativePos.x;
                float posY = overlayCam.rect.y + overlayCam.rect.height * relativePos.y;
                gyro.position = overlayCam.ViewportToWorldPoint(new Vector3(posX, posY, 10f));
            }
            
            if (ObstacleScore.Current)
                pointer.rotation = Quaternion.LookRotation(ObstacleScore.Current.transform.position - droneTransform.position, droneTransform.up);
        }
    }
}