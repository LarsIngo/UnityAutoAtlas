#if UNITY_EDITOR
namespace AutoAtlas.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.U2D;
    using UnityEngine;
    using UnityEngine.U2D;

    public static class AutoAtlas
    {
        public static void DeleteAtlases()
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

        public static void CreateAtlases()
        {
            DeleteAtlases();

            Utility.CreateDirectory(Utility.GetPluginPath(), Utility.GetAtlasesFolderName());

            Dictionary<string, SpriteAtlas> atlasDictionary = GenerateAtlases();

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

        private static Dictionary<string, SpriteAtlas> GenerateAtlases()
        {
            Dictionary<string, SpriteAtlas> atlasDictionary = new Dictionary<string, SpriteAtlas>();

            Dictionary<string, List<Sprite>> spriteDictionary = FindSprites();

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

        private static Dictionary<string, List<Sprite>> FindSprites()
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
    }
}

#endif
