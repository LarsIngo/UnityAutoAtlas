namespace AutoAtlas.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEditor.U2D;
    using UnityEngine;
    using UnityEngine.U2D;

    public static class AutoAtlas
    {
        static AutoAtlas()
        {
            EditorApplication.playModeStateChanged += OnPlayModeState;
        }

        private static void OnPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                AutoAtlas.CreateAtlases();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                AutoAtlas.DeleteAtlases();
            }
        }

        public static void DeleteAtlases(Profile profile)
        {
            string outputPath = Utility.GetOutputPath();

            if (System.IO.Directory.Exists(outputPath))
            {
                string[] paths = Utility.FindAssetPaths("t:spriteatlas", outputPath);

                for (int pathIndex = 0; pathIndex < paths.Length; ++pathIndex)
                {
                    AssetDatabase.DeleteAsset(paths[pathIndex]);
                }

                Utility.DeleteDirectory(outputPath);

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }

        [MenuItem("AutoAtlas/CreateAtlases")]
        public static void CreateAtlases()
        {
            List<Profile> profiles = Utility.FindAssetsByType<Profile>();
            foreach (Profile profile in profiles)
            {
                AutoAtlas.CreateAtlases(profile);
            }
        }

        [MenuItem("AutoAtlas/DeleteAtlases")]
        public static void DeleteAtlases()
        {
            List<Profile> profiles = Utility.FindAssetsByType<Profile>();
            foreach (Profile profile in profiles)
            {
                AutoAtlas.DeleteAtlases(profile);
            }
        }

        public static void CreateAtlases(Profile profile)
        {
            DeleteAtlases(profile);

            if (!profile.AutoAtlasEnabled) return;

            Dictionary<string, SpriteAtlas> atlasDictionary = GenerateAtlases(profile);

            foreach (KeyValuePair<string, SpriteAtlas> pair in atlasDictionary)
            {
                string directory = pair.Key;
                SpriteAtlas atlas = pair.Value;

                string name = directory.Replace('/','_');

                string assetPath = Utility.GetOutputPath() + "/" + name + ".spriteatlas";

                AssetDatabase.CreateAsset(atlas, assetPath);
            }

            SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static Dictionary<string, SpriteAtlas> GenerateAtlases(Profile profile)
        {
            Dictionary<string, SpriteAtlas> atlasDictionary = new Dictionary<string, SpriteAtlas>();

            Dictionary<string, List<Sprite>> spriteDictionary = FindSprites(profile);

            foreach (KeyValuePair<string, List<Sprite>> pair in spriteDictionary)
            {
                string directory = pair.Key;
                List<Sprite> sprites = pair.Value;

                SpriteAtlas atlas = new SpriteAtlas();

                SpriteAtlasExtensions.Add(atlas, sprites.ToArray());

                atlasDictionary[directory] = atlas;
            }

            return atlasDictionary;
        }

        private static Dictionary<string, List<Sprite>> FindSprites(Profile profile)
        {
            Dictionary<string, List<Sprite>> dictionary = new Dictionary<string, List<Sprite>>();

            string[] scenePaths = Utility.GetBuildScenePaths();

            for (int sceneIndex = 0; sceneIndex < scenePaths.Length; ++sceneIndex)
            {
                string[] dependenciePaths = AssetDatabase.GetDependencies(scenePaths[sceneIndex]);
                for (int dependencieIndex = 0; dependencieIndex < dependenciePaths.Length; ++dependencieIndex)
                {
                    string dependenciePath = dependenciePaths[dependencieIndex];
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(dependenciePath);

                    if (sprite)
                    {
                        string directoryPath = Utility.GetDirectory(dependenciePath);

                        if (!dictionary.ContainsKey(directoryPath))
                        {
                            dictionary[directoryPath] = new List<Sprite>();
                        }

                        List<Sprite> sprites = dictionary[directoryPath];
                        if (!sprites.Contains(sprite))
                        {
                            sprites.Add(sprite);
                        }
                    }
                }
            }

            return dictionary;
        }

        private class BuildEvents : IPreprocessBuildWithReport, IPostprocessBuildWithReport
        {
            public int callbackOrder
            {
                get
                {
                    return 0;
                }
            }

            public void OnPreprocessBuild(BuildReport report)
            {
                AutoAtlas.CreateAtlases();
            }

            public void OnPostprocessBuild(BuildReport report)
            {
                AutoAtlas.DeleteAtlases();
            }
        }
    }
}
