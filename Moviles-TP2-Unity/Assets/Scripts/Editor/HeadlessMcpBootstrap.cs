using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Ensures MCP for Unity auto-starts when launching the Editor in external headless batchmode
    /// with UNITY_MCP_ALLOW_BATCH=1.
    /// </summary>
    [InitializeOnLoad]
    internal static class HeadlessMcpBootstrap
    {
        static HeadlessMcpBootstrap()
        {
            if (Application.isBatchMode &&
                Environment.GetEnvironmentVariable("UNITY_MCP_ALLOW_BATCH") == "1" &&
                !EditorPrefs.GetBool("MCPForUnity.AutoStartOnLoad", false))
            {
                EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);
                EditorUtility.RequestScriptReload();
            }
        }
    }
}
