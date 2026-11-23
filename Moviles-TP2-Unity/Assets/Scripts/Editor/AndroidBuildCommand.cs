using System;
using System.IO;
using System.Linq;
using Unity.Pipeline.Commands;
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

            BuildAabCore(outputPath, ksPass, kaPass);
        }

        [CliCommand("build_aab", "Build signed Android App Bundle (.aab)")]
        public static string BuildAabCli(
            [CliArg("output", "Output .aab path")] string output = "Builds/DroneRacer.aab",
            [CliArg("keystore-pass", "Android keystore password")] string keystorePass = "",
            [CliArg("keyalias-pass", "Android keyalias password")] string keyaliasPass = "")
        {
            if (EditorApplication.isPlaying)
            {
                throw new InvalidOperationException("Cannot build AAB while Editor is in Play Mode. Exit Play Mode first.");
            }

            if (EditorApplication.isCompiling)
            {
                throw new InvalidOperationException("Cannot build AAB while scripts are compiling. Wait for compilation to finish.");
            }

            string ksPass = !string.IsNullOrEmpty(keystorePass)
                ? keystorePass
                : Environment.GetEnvironmentVariable("UNITY_KEYSTORE_PASSWORD");
            string kaPass = !string.IsNullOrEmpty(keyaliasPass)
                ? keyaliasPass
                : Environment.GetEnvironmentVariable("UNITY_KEYALIAS_PASSWORD");

            BuildAabCore(output, ksPass, kaPass);
            return $"Built {output} (version {PlayerSettings.bundleVersion}, versionCode {PlayerSettings.Android.bundleVersionCode})";
        }

        private static void BuildAabCore(string outputPath, string ksPass, string kaPass)
        {
            if (string.IsNullOrEmpty(kaPass)) kaPass = ksPass;
            if (string.IsNullOrEmpty(ksPass))
            {
                throw new Exception("Keystore password missing. Pass --keystore-pass or set UNITY_KEYSTORE_PASSWORD.");
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
