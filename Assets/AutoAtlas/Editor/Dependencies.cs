namespace AutoAtlas.Editor
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Static class with methods used to find dependencies in the unity project.
    /// </summary>
    public static class Dependencies
    {
        /// <summary>
        /// Static method which can be used to log other objects referencing the active object.
        /// </summary>
        /// <param name="targetPath">The target path to get dependencies for.</param>
        /// <returns>A list of paths which depends on target path.</returns>
        public static List<string> CollectDependencies(string targetPath)
        {
            // List of paths to check.
            List<string> checkPaths = GetCheckPaths();

            // Collect dependencies.
            List<string> allDepPaths = new List<string>();
            foreach (string checkPath in checkPaths)
            {
                string[] depPaths = UnityEditor.AssetDatabase.GetDependencies(checkPath);
                foreach (string depPath in depPaths)
                {
                    if (depPath.Contains(targetPath) && (depPath != checkPath))
                    {
                        AddUnique(checkPath, allDepPaths);
                    }
                }
            }

            return allDepPaths;
        }

        /// <summary>
        /// Add item to list if unique.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="item">The item.</param>
        /// <param name="list">The list.</param>
        public static void AddUnique<T>(T item, List<T> list)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }

        /// <summary>
        /// Add items to list if unique.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="itemContainer">The items.</param>
        /// <param name="list">The list.</param>
        public static void AddRangeUnique<T>(List<T> itemContainer, List<T> list)
        {
            foreach (T item in itemContainer)
            {
                if (!list.Contains(item))
                {
                    list.Add(item);
                }
            }
        }

        /// <summary>
        /// Add items to list if unique.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="itemContainer">The items.</param>
        /// <param name="list">The list.</param>
        public static void AddRangeUnique<T>(T[] itemContainer, List<T> list)
        {
            foreach (T item in itemContainer)
            {
                if (!list.Contains(item))
                {
                    list.Add(item);
                }
            }
        }

        /// <summary>
        /// Get paths to check for dependencies.
        /// Contains active scenes in build settings.
        /// Contains assets in resource folders.
        /// </summary>
        /// <returns>A list of paths to check for dependencies.</returns>
        public static List<string> GetCheckPaths()
        {
            // List of paths to check.
            List<string> checkPaths = new List<string>();

            // Add active build scenes as paths to check.
            UnityEditor.EditorBuildSettingsScene[] scenes = UnityEditor.EditorBuildSettings.scenes;
            foreach (UnityEditor.EditorBuildSettingsScene scene in scenes)
            {
                AddUnique(scene.path, checkPaths);
            }

            // Add resource folders as paths to check.
            List<string> resourceFolders = Dependencies.FindResourceFolders();
            foreach (string resourceFolder in resourceFolders)
            {
                AddRangeUnique(Dependencies.FindAssetsInFolder(resourceFolder), checkPaths);
            }

            return checkPaths;
        }

        /// <summary>
        /// Get paths to the unity project's dependencies.
        /// </summary>
        /// <returns>A list of paths used by the unity project.</returns>
        public static List<string> GetDepPaths()
        {
            // List of paths to check.
            List<string> checkPaths = GetCheckPaths();

            List<string> depsPaths = new List<string>();
            foreach (string checkPath in checkPaths)
            {
                AddRangeUnique(UnityEditor.AssetDatabase.GetDependencies(checkPath), depsPaths);
            }

            return depsPaths;
        }

        /// <summary>
        /// Static method which can be used to log other objects referencing the active object.
        /// </summary>
        [UnityEditor.MenuItem("Assets/PaperEngine/Log Collect Dependencies")]
        private static void CollectDependenciesLog()
        {
            UnityEngine.Object[] activeObjects = UnityEditor.Selection.objects;
            for (int i = 0; i < activeObjects.Length; ++i)
            {
                UnityEngine.Object activeObject = UnityEditor.Selection.objects[i];
                string targetPath = UnityEditor.AssetDatabase.GetAssetPath(activeObject);
                List<string> allDepPaths = CollectDependencies(targetPath);
                UnityEngine.Debug.Log(targetPath + ": Number of dependencies (" + allDepPaths.Count + "): ", activeObject);
                foreach (string depPath in allDepPaths)
                {
                    UnityEngine.Debug.Log(depPath, UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(depPath));
                }
            }
        }

        /// <summary>
        /// Finds all the resources folders.
        /// </summary>
        /// <returns>Array of resources folders.</returns>
        private static List<string> FindResourceFolders()
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:folder Resources");
            List<string> paths = new List<string>();
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                string folderName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (path.Contains("Assets/") && folderName == "Resources")
                {
                    paths.Add(path);
                }
            }

            return paths;
        }

        /// <summary>
        /// Finds all assets inside folder path. Includes sub-folders.
        /// </summary>
        /// <param name="inputFolderPath">The folder path.</param>
        /// <returns>An array of assets.</returns>
        private static List<string> FindAssetsInFolder(string inputFolderPath)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("", new string[] { inputFolderPath });
            List<string> paths = new List<string>();
            foreach (string guid in guids)
            {
                paths.Add(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
            }

            return paths;
        }
    }
}
