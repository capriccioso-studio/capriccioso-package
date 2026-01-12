using UnityEditor;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;

namespace Capriccioso.Editor
{
    /// <summary>
    /// Automatically updates BuildInfoSettings before building.
    /// </summary>
    public class BuildInfoUpdater : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            UpdateBuildInfo();
        }

        [MenuItem("Window/Capriccioso/Update Build Info")]
        public static void UpdateBuildInfo()
        {
            // Find or create BuildInfoSettings
            BuildInfoSettings settings = GetOrCreateSettings();
            if (settings == null) return;

            // Update build info
            settings.SetBuildDateNow();
            
            // Try to get git info
            string branch = GetGitBranch();
            string commit = GetGitCommitShort();
            
            SerializedObject so = new SerializedObject(settings);
            so.FindProperty("_buildDate").stringValue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            so.FindProperty("_version").stringValue = Application.version;
            so.FindProperty("_buildNumber").intValue = so.FindProperty("_buildNumber").intValue + 1;
            
            if (!string.IsNullOrEmpty(branch))
            {
                so.FindProperty("_gitBranch").stringValue = branch;
            }
            
            if (!string.IsNullOrEmpty(commit))
            {
                so.FindProperty("_gitCommit").stringValue = commit;
            }
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            
            UnityEngine.Debug.Log($"BuildInfo updated: v{Application.version} Build #{so.FindProperty("_buildNumber").intValue}");
        }

        private static BuildInfoSettings GetOrCreateSettings()
        {
            // Try to find existing settings
            string[] guids = AssetDatabase.FindAssets("t:BuildInfoSettings");
            
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<BuildInfoSettings>(path);
            }
            
            // Create new settings in Resources folder
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            
            BuildInfoSettings settings = ScriptableObject.CreateInstance<BuildInfoSettings>();
            AssetDatabase.CreateAsset(settings, "Assets/Resources/BuildInfoSettings.asset");
            AssetDatabase.SaveAssets();
            
            UnityEngine.Debug.Log("Created BuildInfoSettings at Assets/Resources/BuildInfoSettings.asset");
            return settings;
        }

        private static string GetGitBranch()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "rev-parse --abbrev-ref HEAD",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Directory.GetParent(Application.dataPath).FullName
                };

                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();
                    return process.ExitCode == 0 ? output : "";
                }
            }
            catch
            {
                return "";
            }
        }

        private static string GetGitCommitShort()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "rev-parse --short HEAD",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Directory.GetParent(Application.dataPath).FullName
                };

                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();
                    return process.ExitCode == 0 ? output : "";
                }
            }
            catch
            {
                return "";
            }
        }
    }
}
