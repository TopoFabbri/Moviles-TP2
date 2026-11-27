using System.Collections;
using NUnit.Framework;
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

            GameObject instance = Object.Instantiate(prefab);
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
            }
            finally
            {
                Object.Destroy(instance);
            }
        }
    }
}
