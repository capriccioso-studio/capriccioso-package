using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Capriccioso.Runtime.Logging;

namespace Capriccioso.Runtime.Bootstrap
{
    /// <summary>
    /// Scene-agnostic initialization system that ensures required services are loaded
    /// regardless of which scene is started first. Essential for rapid iteration.
    /// </summary>
    /// <remarks>
    /// The BootstrapLoader automatically loads a "Bootstrap" scene containing your
    /// persistent managers when the game starts. This allows starting from any scene
    /// in the editor while ensuring all services are properly initialized.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Setup:
    /// // 1. Create a scene called "Bootstrap" (or configure the name in BootstrapSettings)
    /// // 2. Add all persistent managers/services to Bootstrap scene
    /// // 3. Create a BootstrapSettings asset: Create > Capriccioso > Bootstrap Settings
    /// // 4. Configure the bootstrap scene name and excluded scenes
    /// 
    /// // Your Bootstrap scene might contain:
    /// // - AudioService
    /// // - NetworkManager
    /// // - SaveManager
    /// // - EventSystem
    /// // - etc.
    /// 
    /// // The BootstrapLoader automatically runs before any scene loads.
    /// // If Bootstrap scene isn't loaded, it loads it additively.
    /// 
    /// // For scenes that shouldn't trigger bootstrap (like splash screens):
    /// // Add them to the "Excluded Scenes" list in BootstrapSettings
    /// </code>
    /// </example>
    public static class BootstrapLoader
    {
        private static bool s_hasBootstrapped = false;
        private static BootstrapSettings s_settings;

        /// <summary>
        /// The default bootstrap scene name if no settings are found.
        /// </summary>
        public const string DefaultBootstrapSceneName = "Bootstrap";

        /// <summary>
        /// Returns true if the bootstrap process has completed.
        /// </summary>
        public static bool HasBootstrapped => s_hasBootstrapped;

        /// <summary>
        /// Automatically called before any scene loads.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            s_hasBootstrapped = false;
            LoadSettings();
            
            if (s_settings != null && !s_settings.Enabled)
            {
                CLogger.LogInfo("BootstrapLoader is disabled in settings.");
                return;
            }

            string bootstrapSceneName = s_settings != null ? s_settings.BootstrapSceneName : DefaultBootstrapSceneName;
            
            // Check if current scene should trigger bootstrap
            string currentScene = SceneManager.GetActiveScene().name;
            if (IsExcludedScene(currentScene))
            {
                CLogger.LogInfo($"BootstrapLoader: Scene '{currentScene}' is excluded.");
                return;
            }

            // Check if bootstrap scene exists and isn't already loaded
            if (!IsBootstrapSceneLoaded(bootstrapSceneName))
            {
                LoadBootstrapScene(bootstrapSceneName);
            }
            else
            {
                s_hasBootstrapped = true;
            }
        }

        private static void LoadSettings()
        {
            s_settings = Resources.Load<BootstrapSettings>("BootstrapSettings");
            if (s_settings == null)
            {
                CLogger.Log("BootstrapSettings not found in Resources. Using defaults.");
            }
        }

        private static bool IsExcludedScene(string sceneName)
        {
            if (s_settings == null || s_settings.ExcludedScenes == null)
            {
                return false;
            }

            foreach (string excluded in s_settings.ExcludedScenes)
            {
                if (sceneName == excluded)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsBootstrapSceneLoaded(string bootstrapSceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == bootstrapSceneName)
                {
                    return true;
                }
            }
            return false;
        }

        private static void LoadBootstrapScene(string bootstrapSceneName)
        {
            // Check if the scene is in build settings
            if (!IsSceneInBuildSettings(bootstrapSceneName))
            {
                CLogger.LogWarning($"Bootstrap scene '{bootstrapSceneName}' not found in build settings. Skipping bootstrap.");
                s_hasBootstrapped = true;
                return;
            }

            CLogger.LogInfo($"BootstrapLoader: Loading '{bootstrapSceneName}' scene...");
            
            SceneManager.LoadScene(bootstrapSceneName, LoadSceneMode.Additive);
            s_hasBootstrapped = true;
            
            CLogger.LogSuccess($"BootstrapLoader: '{bootstrapSceneName}' loaded successfully.");
        }

        private static bool IsSceneInBuildSettings(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (name == sceneName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Manually triggers the bootstrap process. Useful for testing.
        /// </summary>
        public static void ForceBootstrap()
        {
            s_hasBootstrapped = false;
            string bootstrapSceneName = s_settings != null ? s_settings.BootstrapSceneName : DefaultBootstrapSceneName;
            
            if (!IsBootstrapSceneLoaded(bootstrapSceneName))
            {
                LoadBootstrapScene(bootstrapSceneName);
            }
        }
    }

    /// <summary>
    /// Settings for the BootstrapLoader system.
    /// Create via Assets > Create > Capriccioso > Bootstrap Settings.
    /// Must be placed in a Resources folder.
    /// </summary>
    [CreateAssetMenu(fileName = "BootstrapSettings", menuName = "Capriccioso/Bootstrap Settings")]
    public class BootstrapSettings : ScriptableObject
    {
        [Header("General")]
        [Tooltip("Enable or disable the bootstrap system.")]
        [SerializeField] private bool _enabled = true;

        [Tooltip("Name of the scene containing persistent managers.")]
        [SerializeField] private string _bootstrapSceneName = "Bootstrap";

        [Header("Exclusions")]
        [Tooltip("Scenes that should not trigger bootstrap loading (e.g., splash screens).")]
        [SerializeField] private List<string> _excludedScenes = new();

        public bool Enabled => _enabled;
        public string BootstrapSceneName => _bootstrapSceneName;
        public IReadOnlyList<string> ExcludedScenes => _excludedScenes;
    }
}
