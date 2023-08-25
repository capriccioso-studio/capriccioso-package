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
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = BuildService.Instance.Scenes;
            buildPlayerOptions.locationPathName = $"{BuildService.Instance.BuildPath}/Windows/Server/{BuildService.Instance.Name}-{BuildService.Instance.Version}-Windows-Server.exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.CompressWithLz4HC | BuildOptions.EnableHeadlessMode;

            CLogger.Instance.LogInfo("Building Server (Windows)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            CLogger.Instance.LogSuccess("Buildt Server (Windows)...");
        }

        [MenuItem("Build/Build Server (Linux)")]
        public static void BuildLinuxServer()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = BuildService.Instance.Scenes;
            buildPlayerOptions.locationPathName = $"{BuildService.Instance.BuildPath}/Linux/Server/{BuildService.Instance.Name}-{BuildService.Instance.Version}-Linux-Server.x86_64";
            buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
            buildPlayerOptions.options = BuildOptions.CompressWithLz4HC | BuildOptions.EnableHeadlessMode;

            CLogger.Instance.LogInfo("Building Server (Linux)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            CLogger.Instance.LogSuccess("Built Server (Linux).");
        }

        [MenuItem("Build/Build Client (Windows)")]
        public static void BuildWindowsClient()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = BuildService.Instance.Scenes;
            buildPlayerOptions.locationPathName = $"{BuildService.Instance.BuildPath}/Windows/Client/{BuildService.Instance.Name}-{BuildService.Instance.Version}-Windows-Client.exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.CompressWithLz4HC;

            CLogger.Instance.LogInfo("Building Client (Windows)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            CLogger.Instance.LogSuccess("Built Client (Windows).");
        }

        [MenuItem("Build/Build Client (HTML5)")]
        public static void BuildWebClient()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = BuildService.Instance.Scenes;
            buildPlayerOptions.locationPathName = $"{BuildService.Instance.BuildPath}/WebGL/Client/{BuildService.Instance.Name}-{BuildService.Instance.Version}-WebGL-Client";
            buildPlayerOptions.target = BuildTarget.WebGL;
            buildPlayerOptions.options = BuildOptions.CompressWithLz4HC;

            CLogger.Instance.LogInfo("Building Client (Web)...");
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            CLogger.Instance.LogSuccess("Built Client (Web).");
        }
    }
}
