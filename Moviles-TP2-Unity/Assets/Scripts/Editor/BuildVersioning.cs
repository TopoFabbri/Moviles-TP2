using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Game.Build
{
    /// <summary>
    /// This class handles automatic versioning after a build is completed.
    /// It increments the bundle version code and the patch number of the version name
    /// only when building an Android App Bundle.
    /// </summary>
    public class BuildVersioning : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            
            // Check if the build was for Android and if it was an App Bundle
            if (report.summary.platform == BuildTarget.Android && EditorUserBuildSettings.buildAppBundle)
            {
                // Increment the bundle version code
                PlayerSettings.Android.bundleVersionCode++;
                Debug.Log($"Incremented Bundle Version Code to: {PlayerSettings.Android.bundleVersionCode}");

                // Increment the patch number of the version name
                IncrementPatchVersion();
            }
        }

        private static void IncrementPatchVersion()
        {
            try
            {
                string[] versionParts = PlayerSettings.bundleVersion.Split('.');
                if (versionParts.Length == 3)
                {
                    if (int.TryParse(versionParts[2], out int patchNumber))
                    {
                        patchNumber++;
                        PlayerSettings.bundleVersion = $"{versionParts[0]}.{versionParts[1]}.{patchNumber}";
                        Debug.Log($"Incremented Version Number to: {PlayerSettings.bundleVersion}");
                    }
                    else
                    {
                        Debug.LogWarning("Could not parse the patch number of the version. Version not incremented.");
                    }
                }
                else
                {
                    Debug.LogWarning("Bundle version format is not a.b.c. Version not incremented.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"An error occurred while incrementing the version number: {e.Message}");
            }
        }
    }

    /// <summary>
    /// This class provides a custom editor menu to manually manage the version number.
    /// </summary>
    public static class VersionMenu
    {
        [MenuItem("Tools/Versioning/Increment Major")]
        private static void IncrementMajorVersion()
        {
            UpdateVersion(0);
        }

        [MenuItem("Tools/Versioning/Increment Minor")]
        private static void IncrementMinorVersion()
        {
            UpdateVersion(1);
        }

        private static void UpdateVersion(int indexToIncrement)
        {
            try
            {
                string[] versionParts = PlayerSettings.bundleVersion.Split('.');
                if (versionParts.Length == 3)
                {
                    if (int.TryParse(versionParts[indexToIncrement], out int versionNumber))
                    {
                        versionNumber++;
                        versionParts[indexToIncrement] = versionNumber.ToString();

                        // Reset subsequent numbers to zero
                        for (int i = indexToIncrement + 1; i < versionParts.Length; i++)
                        {
                            versionParts[i] = "0";
                        }

                        PlayerSettings.bundleVersion = string.Join(".", versionParts);
                        Debug.Log($"Updated Version Number to: {PlayerSettings.bundleVersion}");
                    }
                    else
                    {
                        Debug.LogWarning($"Could not parse the version number at index {indexToIncrement}.");
                    }
                }
                else
                {
                    Debug.LogWarning("Bundle version format is not a.b.c. Cannot update version.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"An error occurred while updating the version number: {e.Message}");
            }
        }
    }
}
