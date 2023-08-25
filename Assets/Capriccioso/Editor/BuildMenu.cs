using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Capriccioso
{
    public class BuildScript
    {

        [MenuItem("Build/Build All")]
        public static void BuildAll()
        {
            BuildWindowsServer();
            BuildLinuxServer();
            BuildWindowsClient();
            BuildWebClient();
        }

        [MenuItem("Build/Build Server (Windows)")]
        public static void BuildWindowsServer()
        {
            string buildPath = "Builds";
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/_Scenes/Offline.unity" , "Assets/_Scenes/Lobby.unity"};
            buildPlayerOptions.locationPathName = buildPath + "/Windows/Server/HaphAngel-Server.exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.CompressWithLz4HC | BuildOptions.EnableHeadlessMode;

            CLogger.Instance.LogInfo("Building Server (Windows)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            CLogger.Instance.LogSuccess("Buildt Server (Windows)...");
        }

        [MenuItem("Build/Build Server (Linux)")]
        public static void BuildLinuxServer()
        {
            string buildPath = "Builds";
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/_Scenes/Offline.unity" , "Assets/_Scenes/Lobby.unity"};
            buildPlayerOptions.locationPathName = buildPath + "/Linux/Server/haphazard_linux_server.x86_64";
            buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
            buildPlayerOptions.options = BuildOptions.CompressWithLz4HC | BuildOptions.EnableHeadlessMode;

            CLogger.Instance.LogInfo("Building Server (Linux)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            CLogger.Instance.LogSuccess("Built Server (Linux).");
        }

        [MenuItem("Build/Build Client (Windows)")]
        public static void BuildWindowsClient()
        {
            string buildPath = "Builds";
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/_Scenes/Offline.unity", "Assets/_Scenes/Lobby.unity", "Assets/_Scenes/main.unity" };
            buildPlayerOptions.locationPathName = buildPath + "/Windows/Client/haphazard_windows_client.exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.CompressWithLz4HC;

            CLogger.Instance.LogInfo("Building Client (Windows)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            CLogger.Instance.LogSuccess("Built Client (Windows).");
        }

        [MenuItem("Build/Build Client (HTML5)")]
        public static void BuildWebClient()
        {
            string buildPath = "Builds";
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] { "Assets/_Scenes/Offline.unity", "Assets/_Scenes/Lobby.unity", "Assets/_Scenes/main.unity" };
            buildPlayerOptions.locationPathName = buildPath + "/WebGL/Client/HaphAngel-WebGL-Client";
            buildPlayerOptions.target = BuildTarget.WebGL;
            buildPlayerOptions.options = BuildOptions.CompressWithLz4HC;

            CLogger.Instance.LogInfo("Building Client (Web)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            CLogger.Instance.LogSuccess("Built Client (Web).");
        }
    }
}
