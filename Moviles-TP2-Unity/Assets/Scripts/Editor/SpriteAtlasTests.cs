using NUnit.Framework;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Editor
{
    public class SpriteAtlasTests
    {
        [Test]
        public void HudAndMenusAtlases_PackForAndroid_WithCompressionOverride()
        {
            SpriteAtlasUtility.PackAllAtlases(BuildTarget.Android);

            var hudAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>("Assets/Art/UI/HUD.spriteatlas");
            var menusAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>("Assets/Art/UI/Menus.spriteatlas");

            Assert.IsNotNull(hudAtlas, "HUD.spriteatlas should exist");
            Assert.IsNotNull(menusAtlas, "Menus.spriteatlas should exist");
            Assert.Greater(hudAtlas.spriteCount, 0, "HUD.spriteatlas should contain packed sprites");
            Assert.Greater(menusAtlas.spriteCount, 0, "Menus.spriteatlas should contain packed sprites");

            var hudAndroid = hudAtlas.GetPlatformSettings("Android");
            var menusAndroid = menusAtlas.GetPlatformSettings("Android");

            Assert.IsTrue(hudAndroid.overridden, "HUD.spriteatlas should override Android platform settings");
            Assert.IsTrue(menusAndroid.overridden, "Menus.spriteatlas should override Android platform settings");
        }
    }
}
