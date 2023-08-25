using UnityEditor;
using UnityEngine;
using System.IO;

namespace Capriccioso
{
    /// <summary>
    /// Used to automatically create folders
    /// </summary>
    public class CreateFolders : EditorWindow
    {
        public string gameFolderName = "CapricciosoGame";

        public Folder[] folders = new Folder[] {
            new Folder("_Animations"),
            new Folder("_Audio", new Folder[] { new Folder("Music"), new Folder("Sounds")}),
            new Folder("_Images", new Folder[]{ new Folder("Sprites"), new Folder("Textures")}),
            new Folder("_Materials"),
            new Folder("_Models"),
            new Folder("_Prefabs"),
            new Folder("_Scenes"),
            new Folder("_Scripts", new Folder[]{ new Folder("Managers"), new Folder("Utilities")}),
            new Folder("_Shaders")
        };

        [MenuItem("Tools/Create Folders")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<CreateFolders>("Create Folders");
        }
        /// <summary>
        /// Draws the Editor GUI
        /// </summary>
        void OnGUI()
        {
            ScriptableObject scriptableObject = this;
            SerializedObject serializedObject = new SerializedObject(scriptableObject);

            GUILayout.Space(30);
            EditorGUILayout.TextField(gameFolderName);
            EditorGUILayout.Space();

            SerializedProperty serializedFolders = serializedObject.FindProperty("folders");
            EditorGUILayout.PropertyField(serializedFolders, true);
            serializedObject.ApplyModifiedProperties();

            if(GUILayout.Button("Create"))
            {
                Create();
            }
        }
        /// <summary>
        /// Generates the folders
        /// </summary>
        void Create()
        {
            string path = Application.dataPath + "/" + gameFolderName + "/";
            
            for (int i = 0; i < folders.Length; i++)
            {
                Directory.CreateDirectory(path + folders[i].folderName);
                if (folders[i].subFolders.Length > 0)
                {
                    for (int k = 0; k < folders[i].subFolders.Length; k++)
                    {
                        Directory.CreateDirectory(path + folders[i].folderName + "/" + folders[i].subFolders[k]);
                    }
                }
            }
            AssetDatabase.Refresh();
        }
    }
}