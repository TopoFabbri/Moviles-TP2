using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Editor
{
    public static class AndroidBuildCommand
    {
        public static void BuildAab()
        {
            var args = Environment.GetCommandLineArgs();
            string outputPath = "Builds/DroneRacer.aab";
            string ksPass = Environment.GetEnvironmentVariable("UNITY_KEYSTORE_PASSWORD");
            string kaPass = Environment.GetEnvironmentVariable("UNITY_KEYALIAS_PASSWORD");

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-buildOutput") outputPath = args[i + 1];
                else if (args[i] == "-androidKeystorePass" && string.IsNullOrEmpty(ksPass)) ksPass = args[i + 1];
                else if (args[i] == "-androidKeyaliasPass" && string.IsNullOrEmpty(kaPass)) kaPass = args[i + 1];
            }

            if (string.IsNullOrEmpty(kaPass)) kaPass = ksPass;
            if (string.IsNullOrEmpty(ksPass))
            {
                throw new Exception("Keystore password missing. Set UNITY_KEYSTORE_PASSWORD.");
            }

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = "KeyStore/DroneRacer.keystore";
            PlayerSettings.Android.keystorePass = ksPass;
            PlayerSettings.Android.keyaliasName = "droneracer2025";
            PlayerSettings.Android.keyaliasPass = kaPass;

            string dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            EditorUserBuildSettings.buildAppBundle = true;

            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            BuildReport report = BuildPipeline.BuildPlayer(
                scenes,
                outputPath,
                BuildTarget.Android,
                BuildOptions.None
            );

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"Android AAB build failed: {report.summary.result} (errors: {report.summary.totalErrors})");
            }

            AssetDatabase.SaveAssets();
        }
    }
}
