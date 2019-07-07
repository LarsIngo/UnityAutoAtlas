#if UNITY_EDITOR
namespace AutoAtlas.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.U2D;

    public static class Utility
    {
        public static string[] GetBuildScenePaths()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            string[] scenePaths = new string[scenes.Length];
            for (int sceneIndex = 0; sceneIndex < scenes.Length; ++sceneIndex)
            {
                scenePaths[sceneIndex] = scenes[sceneIndex].path;
            }

            return scenePaths;
        }

        public static string GetDirectory(string path)
        {
            return path.Substring(0, path.LastIndexOf("/"));
        }

        public static string GetPluginPath()
        {
            string[] guids = AssetDatabase.FindAssets("AutoAtlas");

            return AssetDatabase.GUIDToAssetPath(guids[0]);
        }

        public static string GetSettingsName(bool includeExtension = false)
        {
            string str = "AutoAtlasSettings";
            if (includeExtension)
            {
                str += ".asset";
            }

            return str;
        }

        public static string GetSettingsPath()
        {
            return Utility.FindAssetPath(GetSettingsName() + " t:scriptableobject");
        }

        public static string GetTempFolderName()
        {
            return "Temp";
        }

        public static string GetTempPath(bool includeSubFolder = true)
        {
            string path = Utility.GetPluginPath();
            if (includeSubFolder)
            {
                path += "/" + GetTempFolderName();
            }

            return path;
        }

        public static string GetAtlasesFolderName()
        {
            return "Atlases";
        }

        public static string GetOutputPath()
        {
            return GetPluginPath() + "/" + GetAtlasesFolderName();
        }

        public static void CreateDirectory(string parentFolder, string newFolderName)
        {
            string path = parentFolder + "/" + newFolderName;
            if (!System.IO.Directory.Exists(path))
            {
                AssetDatabase.CreateFolder(parentFolder, newFolderName);

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }

        public static void DeleteDirectory(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }

        public static string FindAssetPath(string filter)
        {
            string[] paths = FindAssetPaths(filter);
            return paths.Length > 0 ? paths[0] : null;
        }

        public static string[] FindAssetPaths(string filter, string directory = null)
        {
            string[] guids = string.IsNullOrEmpty(directory) ? AssetDatabase.FindAssets(filter) : AssetDatabase.FindAssets(filter, new string[] { directory });

            string[] paths = new string[guids.Length];

            for (int guidIndex = 0; guidIndex < guids.Length; ++guidIndex)
            {
                paths[guidIndex] = AssetDatabase.GUIDToAssetPath(guids[guidIndex]);
            }

            return paths;
        }
    }
}

#endif
