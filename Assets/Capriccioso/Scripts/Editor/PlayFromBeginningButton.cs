using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class PlayFromBeginningButton : Editor
{
    static PlayFromBeginningButton()
    {
        EditorApplication.playModeStateChanged += CheckPlayModeState;
    }

    private static void CheckPlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            LoadAndPlayScene();
        }
    }

    [MenuItem("Tools/Play From Beginning")]
    private static void PlayFromBeginning()
    {
        if (!EditorApplication.isPlaying)
        {
            LoadAndPlayScene();
        }
    }

    private static void LoadAndPlayScene()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(EditorSceneManager.GetSceneByBuildIndex(0).path);
            EditorApplication.isPlaying = true;
        }
    }
}
