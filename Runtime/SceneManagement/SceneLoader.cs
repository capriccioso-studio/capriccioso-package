using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Capriccioso.Runtime.Core;
using Capriccioso.Runtime.Logging;

namespace Capriccioso.Runtime.SceneManagement
{
    /// <summary>
    /// Async scene loading utility with progress callbacks and transition support.
    /// Provides both coroutine and async/await patterns for scene management.
    /// </summary>
    /// <example>
    /// <code>
    /// // Simple scene load
    /// await SceneLoader.LoadSceneAsync("GameLevel");
    /// 
    /// // Load with progress callback
    /// await SceneLoader.LoadSceneAsync("GameLevel", progress => 
    /// {
    ///     loadingBar.fillAmount = progress;
    ///     percentText.text = $"{progress * 100:F0}%";
    /// });
    /// 
    /// // Load with transition (fade out, load, fade in)
    /// await SceneLoader.LoadSceneWithTransitionAsync(
    ///     "GameLevel",
    ///     onFadeOut: async () => await FadeCanvas(1f),
    ///     onFadeIn: async () => await FadeCanvas(0f),
    ///     onProgress: p => loadingBar.fillAmount = p
    /// );
    /// 
    /// // Coroutine-based loading (for MonoBehaviours)
    /// StartCoroutine(SceneLoader.LoadSceneCoroutine("GameLevel", 
    ///     onProgress: p => Debug.Log($"Loading: {p * 100}%"),
    ///     onComplete: () => Debug.Log("Scene loaded!")
    /// ));
    /// 
    /// // Additive scene loading
    /// await SceneLoader.LoadSceneAsync("UIOverlay", LoadSceneMode.Additive);
    /// 
    /// // Unload additive scene
    /// await SceneLoader.UnloadSceneAsync("UIOverlay");
    /// </code>
    /// </example>
    public static class SceneLoader
    {
        /// <summary>
        /// Loads a scene asynchronously with optional progress callback.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load.</param>
        /// <param name="onProgress">Called with progress value (0-1).</param>
        /// <param name="mode">Scene load mode (Single or Additive).</param>
        /// <returns>Task that completes when scene is loaded.</returns>
        public static async Task LoadSceneAsync(
            string sceneName, 
            Action<float> onProgress = null, 
            LoadSceneMode mode = LoadSceneMode.Single)
        {
            CLogger.LogInfo($"* Loading scene: {sceneName}");
            
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
            operation.allowSceneActivation = false;

            while (operation.progress < Constants.SceneLoadedThreshold)
            {
                onProgress?.Invoke(operation.progress / Constants.SceneLoadedThreshold);
                await Task.Yield();
            }

            onProgress?.Invoke(1f);
            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            CLogger.LogSuccess($"$ Scene loaded: {sceneName}");
        }

        /// <summary>
        /// Loads a scene with transition callbacks for fade effects.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load.</param>
        /// <param name="onFadeOut">Async callback to fade out (before loading).</param>
        /// <param name="onFadeIn">Async callback to fade in (after loading).</param>
        /// <param name="onProgress">Called with progress value (0-1).</param>
        /// <param name="mode">Scene load mode.</param>
        public static async Task LoadSceneWithTransitionAsync(
            string sceneName,
            Func<Task> onFadeOut = null,
            Func<Task> onFadeIn = null,
            Action<float> onProgress = null,
            LoadSceneMode mode = LoadSceneMode.Single)
        {
            CLogger.LogInfo($"* Loading scene with transition: {sceneName}");

            // Fade out
            if (onFadeOut != null)
            {
                await onFadeOut();
            }

            // Load scene
            await LoadSceneAsync(sceneName, onProgress, mode);

            // Small delay to ensure scene is fully initialized
            await Task.Delay(TimeSpan.FromSeconds(Constants.MinLoadingScreenTime));

            // Fade in
            if (onFadeIn != null)
            {
                await onFadeIn();
            }

            CLogger.LogSuccess($"$ Scene transition complete: {sceneName}");
        }

        /// <summary>
        /// Coroutine-based scene loading for use with StartCoroutine.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load.</param>
        /// <param name="onProgress">Called with progress value (0-1).</param>
        /// <param name="onComplete">Called when loading is complete.</param>
        /// <param name="mode">Scene load mode.</param>
        public static IEnumerator LoadSceneCoroutine(
            string sceneName,
            Action<float> onProgress = null,
            Action onComplete = null,
            LoadSceneMode mode = LoadSceneMode.Single)
        {
            CLogger.LogInfo($"* Loading scene (coroutine): {sceneName}");

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
            operation.allowSceneActivation = false;

            while (operation.progress < Constants.SceneLoadedThreshold)
            {
                onProgress?.Invoke(operation.progress / Constants.SceneLoadedThreshold);
                yield return null;
            }

            onProgress?.Invoke(1f);
            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                yield return null;
            }

            onComplete?.Invoke();
            CLogger.LogSuccess($"$ Scene loaded (coroutine): {sceneName}");
        }

        /// <summary>
        /// Unloads an additively loaded scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene to unload.</param>
        public static async Task UnloadSceneAsync(string sceneName)
        {
            CLogger.LogInfo($"* Unloading scene: {sceneName}");

            AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            CLogger.LogSuccess($"$ Scene unloaded: {sceneName}");
        }

        /// <summary>
        /// Reloads the current active scene.
        /// </summary>
        public static async Task ReloadCurrentSceneAsync(Action<float> onProgress = null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            await LoadSceneAsync(currentScene, onProgress);
        }

        /// <summary>
        /// Gets the name of the currently active scene.
        /// </summary>
        public static string CurrentSceneName => SceneManager.GetActiveScene().name;

        /// <summary>
        /// Gets the build index of the currently active scene.
        /// </summary>
        public static int CurrentSceneIndex => SceneManager.GetActiveScene().buildIndex;

        /// <summary>
        /// Checks if a scene is loaded.
        /// </summary>
        public static bool IsSceneLoaded(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene.isLoaded;
        }
    }
}
