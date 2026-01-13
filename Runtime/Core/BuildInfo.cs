using System;
using UnityEngine;

namespace Capriccioso.Runtime.Core
{
    /// <summary>
    /// Provides build version and timestamp information at runtime.
    /// Useful for displaying version info in-game and for debugging.
    /// </summary>
    /// <remarks>
    /// BuildInfo can be populated automatically via a pre-build script,
    /// or manually via the BuildInfoSettings asset.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Display version in UI
    /// versionText.text = BuildInfo.FullVersion;
    /// // Output: "1.2.3 (Build 456)"
    /// 
    /// // Access individual components
    /// Debug.Log($"Version: {BuildInfo.Version}");
    /// Debug.Log($"Build: {BuildInfo.BuildNumber}");
    /// Debug.Log($"Built: {BuildInfo.BuildDate}");
    /// Debug.Log($"Branch: {BuildInfo.GitBranch}");
    /// Debug.Log($"Commit: {BuildInfo.GitCommit}");
    /// 
    /// // Check build type
    /// if (BuildInfo.IsDebugBuild)
    /// {
    ///     EnableDebugFeatures();
    /// }
    /// 
    /// // Log all build info
    /// BuildInfo.LogBuildInfo();
    /// </code>
    /// </example>
    public static class BuildInfo
    {
        private static BuildInfoSettings s_settings;
        private static bool s_initialized = false;

        /// <summary>
        /// Semantic version string (e.g., "1.2.3").
        /// </summary>
        public static string Version => GetSettings()?.Version ?? Application.version;

        /// <summary>
        /// Build number, typically incremented by CI/CD.
        /// </summary>
        public static int BuildNumber => GetSettings()?.BuildNumber ?? 0;

        /// <summary>
        /// Full version string with build number (e.g., "1.2.3 (Build 456)").
        /// </summary>
        public static string FullVersion
        {
            get
            {
                int build = BuildNumber;
                return build > 0 ? $"{Version} (Build {build})" : Version;
            }
        }

        /// <summary>
        /// Date and time when the build was created.
        /// </summary>
        public static string BuildDate => GetSettings()?.BuildDate ?? "Unknown";

        /// <summary>
        /// Git branch name at build time.
        /// </summary>
        public static string GitBranch => GetSettings()?.GitBranch ?? "Unknown";

        /// <summary>
        /// Git commit hash (short) at build time.
        /// </summary>
        public static string GitCommit => GetSettings()?.GitCommit ?? "Unknown";

        /// <summary>
        /// Returns true if this is a debug/development build.
        /// </summary>
        public static bool IsDebugBuild => Debug.isDebugBuild;

        /// <summary>
        /// Returns the Unity version used to build.
        /// </summary>
        public static string UnityVersion => Application.unityVersion;

        /// <summary>
        /// Returns the target platform.
        /// </summary>
        public static RuntimePlatform Platform => Application.platform;

        /// <summary>
        /// Company name from Player Settings.
        /// </summary>
        public static string CompanyName => Application.companyName;

        /// <summary>
        /// Product name from Player Settings.
        /// </summary>
        public static string ProductName => Application.productName;

        private static BuildInfoSettings GetSettings()
        {
            if (!s_initialized)
            {
                s_settings = Resources.Load<BuildInfoSettings>("BuildInfoSettings");
                s_initialized = true;
            }
            return s_settings;
        }

        /// <summary>
        /// Logs all build information to the console.
        /// </summary>
        public static void LogBuildInfo()
        {
            CLogger.LogMajorAction("BUILD INFO");
            CLogger.LogInfo($"Product: {ProductName} by {CompanyName}");
            CLogger.LogInfo($"Version: {FullVersion}");
            CLogger.LogInfo($"Build Date: {BuildDate}");
            CLogger.LogInfo($"Unity: {UnityVersion}");
            CLogger.LogInfo($"Platform: {Platform}");
            CLogger.LogInfo($"Git: {GitBranch} @ {GitCommit}");
            CLogger.LogInfo($"Debug Build: {IsDebugBuild}");
        }

        /// <summary>
        /// Returns a formatted multi-line string with all build info.
        /// </summary>
        public static string GetFormattedBuildInfo()
        {
            return $"{ProductName} v{FullVersion}\n" +
                   $"Built: {BuildDate}\n" +
                   $"Unity: {UnityVersion}\n" +
                   $"Platform: {Platform}\n" +
                   $"Branch: {GitBranch}\n" +
                   $"Commit: {GitCommit}";
        }

        /// <summary>
        /// Resets cached settings. Call if settings are changed at runtime.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            s_initialized = false;
            s_settings = null;
        }
    }

    /// <summary>
    /// ScriptableObject containing build information.
    /// Create via Assets > Create > Capriccioso > Build Info Settings.
    /// Must be placed in a Resources folder.
    /// </summary>
    /// <remarks>
    /// This can be auto-populated by a pre-build script in your CI/CD pipeline.
    /// </remarks>
    [CreateAssetMenu(fileName = "BuildInfoSettings", menuName = "Capriccioso/Build Info Settings")]
    public class BuildInfoSettings : ScriptableObject
    {
        [Header("Version")]
        [Tooltip("Semantic version (MAJOR.MINOR.PATCH).")]
        [SerializeField] private string _version = "0.0.1";

        [Tooltip("Build number, typically set by CI/CD.")]
        [SerializeField] private int _buildNumber = 0;

        [Header("Build Details")]
        [Tooltip("Date/time of build.")]
        [SerializeField] private string _buildDate = "";

        [Header("Source Control")]
        [Tooltip("Git branch name.")]
        [SerializeField] private string _gitBranch = "";

        [Tooltip("Git commit hash (short).")]
        [SerializeField] private string _gitCommit = "";

        public string Version => _version;
        public int BuildNumber => _buildNumber;
        public string BuildDate => _buildDate;
        public string GitBranch => _gitBranch;
        public string GitCommit => _gitCommit;

        /// <summary>
        /// Sets build information programmatically (for CI/CD scripts).
        /// </summary>
        public void SetBuildInfo(string version, int buildNumber, string buildDate, string branch, string commit)
        {
            _version = version;
            _buildNumber = buildNumber;
            _buildDate = buildDate;
            _gitBranch = branch;
            _gitCommit = commit;
        }

        /// <summary>
        /// Sets the build date to the current time.
        /// </summary>
        public void SetBuildDateNow()
        {
            _buildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
