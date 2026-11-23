using UnityEngine;

namespace Game.World.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaHandler : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea = Rect.zero;
        private Vector2Int lastScreenSize = Vector2Int.zero;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void OnEnable()
        {
            ApplySafeArea();
        }

        private void Update()
        {
            if (Screen.safeArea != lastSafeArea ||
                Screen.width != lastScreenSize.x ||
                Screen.height != lastScreenSize.y)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            if (rectTransform == null || Screen.width <= 0 || Screen.height <= 0)
                return;

            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            float horizontalMargin = Mathf.Max(safeArea.xMin, Screen.width - safeArea.xMax);
            float verticalMargin = Mathf.Max(safeArea.yMin, Screen.height - safeArea.yMax);

            Vector2 anchorMin = new Vector2(horizontalMargin / Screen.width, verticalMargin / Screen.height);
            Vector2 anchorMax = new Vector2((Screen.width - horizontalMargin) / Screen.width, (Screen.height - verticalMargin) / Screen.height);

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
