using UnityEditor;
using UnityEngine;
using System.IO;

namespace Capriccioso.Editor
{
    /// <summary>
    /// Editor menu for creating Capriccioso project folder structure.
    /// </summary>
    public static class CreateFoldersMenu
    {
        [MenuItem("Assets/Create/Capriccioso/Project Folders", false, 0)]
        public static void CreateProjectFolders()
        {
            string rootPath = "Assets";
            
            string[] folders = new string[]
            {
                "_Animations",
                "_Prefabs",
                "_Resources",
                "_Scenes",
                "_Scripts",
                "_Scripts/Services",
                "_Scripts/Handlers",
                "_Scripts/Managers",
                "_Scripts/Events",
                "_Scripts/Classes",
                "_Scripts/Data",
                "_Sounds",
                "_Sprites",
                "_Materials",
                "_Fonts"
            };

            foreach (string folder in folders)
            {
                string fullPath = Path.Combine(rootPath, folder);
                if (!AssetDatabase.IsValidFolder(fullPath))
                {
                    string[] parts = folder.Split('/');
                    string currentPath = rootPath;
                    
                    foreach (string part in parts)
                    {
                        string nextPath = Path.Combine(currentPath, part);
                        if (!AssetDatabase.IsValidFolder(nextPath))
                        {
                            AssetDatabase.CreateFolder(currentPath, part);
                        }
                        currentPath = nextPath;
                    }
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Capriccioso project folders created successfully!");
        }

        [MenuItem("Assets/Create/Capriccioso/Resources Folder", false, 1)]
        public static void CreateResourcesFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.Refresh();
                Debug.Log("Resources folder created at Assets/Resources");
            }
            else
            {
                Debug.Log("Resources folder already exists.");
            }
        }
    }
}
