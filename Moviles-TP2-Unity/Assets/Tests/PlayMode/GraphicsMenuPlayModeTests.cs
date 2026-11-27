using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests.PlayMode
{
    public class GraphicsMenuPlayModeTests
    {
        [UnityTest]
        public IEnumerator GameplaySubmenuPrefab_BuildsAndTogglesGraphicsPanel()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/GameplaySubmenu.prefab");
            Assert.IsNotNull(prefab);

            var canvasGo = new GameObject("TestCanvas", typeof(Canvas), typeof(CanvasScaler));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var canvasRect = canvasGo.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1920f, 1080f);

            GameObject instance = Object.Instantiate(prefab, canvasGo.transform);
            try
            {
                yield return null;

                Transform mainScroll = instance.transform.Find("Scroll View");
                Transform gfxScroll = instance.transform.Find("GraphicsScrollView");
                Transform entryBtnTransform = instance.transform.Find("Scroll View/Viewport/Content/GraphicsSettingsBtn");

                Assert.IsNotNull(mainScroll);
                Assert.IsNotNull(gfxScroll);
                Assert.IsNotNull(entryBtnTransform);
                Assert.IsFalse(gfxScroll.gameObject.activeSelf);

                entryBtnTransform.GetComponent<Button>().onClick.Invoke();
                Assert.IsFalse(mainScroll.gameObject.activeSelf);
                Assert.IsTrue(gfxScroll.gameObject.activeSelf);

                yield return null;
                Canvas.ForceUpdateCanvases();

                AssertToggleDoesNotOverlap(gfxScroll, "Viewport/Content/PostFxToggleRow");
                AssertToggleDoesNotOverlap(gfxScroll, "Viewport/Content/AdaptiveThermalToggleRow");
            }
            finally
            {
                Object.Destroy(canvasGo);
            }
        }

        private static void AssertToggleDoesNotOverlap(Transform gfxScroll, string relativePath)
        {
            Transform row = gfxScroll.Find(relativePath);
            Assert.IsNotNull(row, $"Missing toggle row {relativePath}");

            Toggle toggle = row.GetComponentInChildren<Toggle>(true);
            Assert.IsNotNull(toggle);
            Assert.IsNotNull(toggle.targetGraphic);

            TextMeshProUGUI label = toggle.GetComponentInChildren<TextMeshProUGUI>(true);
            Assert.IsNotNull(label);
            Assert.IsTrue(label.enableAutoSizing);

            var labelCorners = new Vector3[4];
            var boxCorners = new Vector3[4];
            label.rectTransform.GetWorldCorners(labelCorners);
            toggle.targetGraphic.rectTransform.GetWorldCorners(boxCorners);

            Assert.Greater(label.rectTransform.rect.width, 100f, $"{relativePath} label width collapsed");
            Assert.LessOrEqual(labelCorners[2].x, boxCorners[0].x + 0.5f, $"{relativePath} label overlaps checkbox");
        }
    }
}
