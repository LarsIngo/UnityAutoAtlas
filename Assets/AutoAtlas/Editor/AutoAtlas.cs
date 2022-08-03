namespace AutoAtlas.Editor
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEditor.U2D;
    using UnityEngine;
    using UnityEngine.U2D;

    /// <summary>
    /// Static class for generating sprite atlases.
    /// </summary>
    [InitializeOnLoad]
    public static class AutoAtlas
    {
        /// <summary>
        /// Static constructor.
        /// </summary>
        static AutoAtlas()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        /// <summary>
        /// Logs name of textures found in database.
        /// </summary>
        [MenuItem("AutoAtlas/Log/ReferencedTextures")]
        public static void LogTexturesDataBase()
        {
            List<string> paths = GetReferencedTextures();
            paths.Sort();
            foreach (string path in paths)
            {
                Object image = (Object)AssetDatabase.LoadAssetAtPath<Sprite>(path) ?? (Object)AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                Debug.Log(path + " (" + image.GetType().Name + ")", image);
            }
        }

        /// <summary>
        /// Deletes all sprite atlases.
        /// </summary>
        [MenuItem("AutoAtlas/Delete")]
        public static void DeleteAtlases()
        {
            string[] guids = AssetDatabase.FindAssets("t:spriteatlas");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.DeleteAsset(path);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        /// <summary>
        /// Generate sprite atlases.
        /// </summary>
        [MenuItem("AutoAtlas/Generate")]
        public static void GenerateAtlases()
        {
            // Delete existing atlases.
            DeleteAtlases();

            // Get textures.
            List<string> paths = GetReferencedTextures();

            // Store sprites and their folder path.
            Dictionary<string, Dictionary<Settings, List<Sprite>>> spriteDictionary = new Dictionary<string, Dictionary<Settings, List<Sprite>>>();
            foreach (string path in paths)
            {
                // Load sprite.
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                // Check whether directory contains resources path.
                if (path.Contains("/Resources/"))
                {
                    // Log warning and skip directory because atlases in resource path does not reduce memory size.
                    Debug.LogWarning(typeof(AutoAtlas).FullName + ".GenerateAtlases: Skipping sprite (" + path + ").", sprite);
                }
                else if (sprite.texture.width == sprite.texture.height && Mathf.IsPowerOfTwo(sprite.texture.width) && Mathf.IsPowerOfTwo(sprite.texture.height))
                {
                    // Log warning and skip sprite because texture is already squared and power of two.
                    Debug.LogWarning(typeof(AutoAtlas).FullName + ".GenerateAtlases: Skipping sprite (" + path + ").", sprite);
                }
                else
                {
                    // Check folder path.
                    string folderPath = path.Substring(0, path.LastIndexOf("/"));
                    if (!spriteDictionary.ContainsKey(folderPath))
                    {
                        spriteDictionary[folderPath] = new Dictionary<Settings, List<Sprite>>();
                    }

                    // Check whether format has alpha values.
                    //bool alpha = HasAlpha(sprite.texture.format);
                    Settings settings = new Settings(sprite);

                    if (!spriteDictionary[folderPath].ContainsKey(settings))
                    {
                        spriteDictionary[folderPath][settings] = new List<Sprite>();
                    }

                    // Check whether to add sprite.
                    List<Sprite> sprites = spriteDictionary[folderPath][settings];
                    if (!sprites.Contains(sprite))
                    {
                        sprites.Add(sprite);
                    }
                }
            }

            // Iterate directories and create sprite atlas.
            foreach (KeyValuePair<string, Dictionary<Settings, List<Sprite>>> pairA in spriteDictionary)
            {
                string folderPath = pairA.Key;

                // Iterate sprites to pack.
                foreach (KeyValuePair<Settings, List<Sprite>> pairB in pairA.Value)
                {
                    Settings settings = pairB.Key;
                    List<Sprite> sprites = pairB.Value;

                    const uint maxSize = 2048U;
                    uint finalSize = GetMinSize(sprites);
                    uint currentSize = finalSize;
                    uint itSize = currentSize;
                    uint itPixelCount = uint.MaxValue;
                    uint textureCount = 0U;

                    // Find optimal size.
                    SpriteAtlas spriteAtlas;
                    do
                    {
                        currentSize = itSize;

                        // Pack atlas using current size.
                        spriteAtlas = PackAtlas(folderPath, currentSize, sprites, settings);
                        List<Texture2D> textures = GetSpriteAtlasTextures(spriteAtlas);
                        textureCount = (uint)textures.Count;

                        // Count pixels in atlas.
                        uint pixelCount = 0;
                        foreach (Texture2D texture in textures)
                        {
                            pixelCount += (uint)(texture.width * texture.height);
                        }
                       
                        if (pixelCount < itPixelCount)
                        {
                            finalSize = currentSize;
                            itPixelCount = pixelCount;
                        }

                        // Increment current size.
                        itSize *= 2U;
                    }
                    while ((itSize <= maxSize) && (textureCount > 1));

                    // Repack atlas if needed.
                    if (currentSize != finalSize)
                    {
                        spriteAtlas = PackAtlas(folderPath, finalSize, sprites, settings);
                    }

                    Debug.Log(typeof(AutoAtlas).FullName + ".GenerateAtlases: Packed atlas (" + folderPath + ").", spriteAtlas);
                }
            }
        }

        /// <summary>
        /// Check whether mips should be generated. Return true if one sprite has mips.
        /// </summary>
        /// <param name="sprites">The sprites to check.</param>
        /// <returns>Whether to generate mips.</returns>
        private static bool GenerateMipMaps(List<Sprite> sprites)
        {
            foreach (Sprite sprite in sprites)
            {
                if (sprite.texture.mipmapCount > 1)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a sprite atlas.
        /// </summary>
        /// <param name="dir">Atlas path.</param>
        /// <param name="size">Max size of atlas texture.</param>
        /// <param name="sprites">Sprites to pack.</param>
        /// <param name="settings">Settings for the sprites.</param>
        /// <returns>The sprite atlas.</returns>
        private static SpriteAtlas PackAtlas(string dir, uint size, List<Sprite> sprites, Settings settings)
        {
            // Create atlas and add sprites.
            SpriteAtlas spriteAtlas = new SpriteAtlas();
            SpriteAtlasExtensions.Add(spriteAtlas, sprites.ToArray());

            // Set general settings.
            SpriteAtlasPackingSettings packingSettings = SpriteAtlasExtensions.GetPackingSettings(spriteAtlas);
            SpriteAtlasTextureSettings textureSettings = SpriteAtlasExtensions.GetTextureSettings(spriteAtlas);
            packingSettings.enableTightPacking = false;
            packingSettings.enableRotation = false;
            textureSettings.generateMipMaps = settings.Mips;
            SpriteAtlasExtensions.SetPackingSettings(spriteAtlas, packingSettings);
            SpriteAtlasExtensions.SetTextureSettings(spriteAtlas, textureSettings);
            SpriteAtlasExtensions.SetIncludeInBuild(spriteAtlas, true);

            // Set platform settings for Android.
            TextureImporterPlatformSettings platformSettings = SpriteAtlasExtensions.GetPlatformSettings(spriteAtlas, "Android");
            platformSettings.overridden = true;
            platformSettings.maxTextureSize = (int)size;
            platformSettings.format = settings.Alpha ? TextureImporterFormat.ETC2_RGBA8 : TextureImporterFormat.ETC2_RGB4;
            SpriteAtlasExtensions.SetPlatformSettings(spriteAtlas, platformSettings);

            // Set platform settings for iOS.
            platformSettings = SpriteAtlasExtensions.GetPlatformSettings(spriteAtlas, "iPhone");
            platformSettings.overridden = true;
            platformSettings.maxTextureSize = (int)size;
            platformSettings.format = settings.Alpha ? TextureImporterFormat.PVRTC_RGBA4 : TextureImporterFormat.PVRTC_RGB4;
            SpriteAtlasExtensions.SetPlatformSettings(spriteAtlas, platformSettings);

            // Get folder name.
            int index = dir.LastIndexOf("/") + 1;
            string folderName = dir.Substring(index, dir.Length - index);

            // Generate sprite atlas name and path.
            string name = folderName + "_AutoAtlas_" + (settings.Alpha ? "RGBA" : "RGB") + "_" + (settings.Mips ? "Mips" : "NoMips");
            string path = dir + "/" + name + ".spriteatlas";

            // Create sprite atlas.
            AssetDatabase.CreateAsset(spriteAtlas, path);

            // Pack atlas.
            SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget, false);

            // Refresh database.
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            return spriteAtlas;
        }

        /// <summary>
        /// Get minimum size needed by sprites.
        /// </summary>
        /// <param name="sprites">The sprites.</param>
        /// <returns>The minimum size.</returns>
        private static uint GetMinSize(List<Sprite> sprites)
        {
            // Find max size texture.
            int size = 0;
            foreach (Sprite sprite in sprites)
            {
                size = Mathf.Max(size, Mathf.NextPowerOfTwo(sprite.texture.width));
                size = Mathf.Max(size, Mathf.NextPowerOfTwo(sprite.texture.height));
            }

            return (uint)size;
        }

        /// <summary>
        /// Gets atlas textures.
        /// </summary>
        /// <param name="spriteAtlas">The atlas.</param>
        /// <returns>List of textures.</returns>
        private static List<Texture2D> GetSpriteAtlasTextures(SpriteAtlas spriteAtlas)
        {
            List<Texture2D> textures = new List<Texture2D>();

            Sprite[] sprites = new Sprite[spriteAtlas.spriteCount];
            spriteAtlas.GetSprites(sprites);
            foreach (Sprite sprite in sprites)
            {
                Texture2D texture = UnityEditor.Sprites.SpriteUtility.GetSpriteTexture(sprite, true);
                if (!textures.Contains(texture))
                {
                    textures.Add(texture);
                }
            }

            return textures;
        }

        /// <summary>
        /// Checks whether format has alpha values.
        /// </summary>
        /// <param name="format">The format to check.</param>
        /// <returns>Whether format has alpha values.</returns>
        private static bool HasAlpha(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.RGBA32:
                    return true;
                case TextureFormat.ETC2_RGBA8:
                    return true;
                case TextureFormat.ETC2_RGBA1:
                    return true;
                case TextureFormat.PVRTC_RGBA4:
                    return true;
                case TextureFormat.PVRTC_RGBA2:
                    return true;
                case TextureFormat.DXT5:
                    return true;
                case TextureFormat.DXT5Crunched:
                    return true;

                case TextureFormat.RGB24:
                    return false;
                case TextureFormat.ETC2_RGB:
                    return false;
                case TextureFormat.ETC_RGB4:
                    return false;
                case TextureFormat.PVRTC_RGB4:
                    return false;
                case TextureFormat.PVRTC_RGB2:
                    return false;
                case TextureFormat.DXT1:
                    return false;
                case TextureFormat.DXT1Crunched:
                    return false;

                default:
                    Debug.LogError(typeof(AutoAtlas).FullName + ".GenerateAtlases: Unsupported texture format (" + format.ToString() + ").");
                    return true;
            }
        }

        /// <summary>
        /// Find textures used by scenes and referenced by assets in Resource folders.
        /// </summary>
        /// <returns>A list of paths to textures.</returns>
        private static List<string> GetReferencedTextures()
        {
            List<string> depPaths = Dependencies.GetDepPaths();

            List<string> spritePaths = GetSpritePaths();

            List<string> referencedTextures = new List<string>();
            foreach (string spritePath in spritePaths)
            {
                if (depPaths.Contains(spritePath))
                {
                    Dependencies.AddUnique(spritePath, referencedTextures);
                }
            }

            return referencedTextures;
        }

        /// <summary>
        /// Gets all sprites in project.
        /// </summary>
        /// <returns>A list of paths to sprites.</returns>
        private static List<string> GetSpritePaths()
        {
            List<string> paths = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:sprite");
            foreach (string guid in guids)
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            return paths;
        }

        /// <summary>
        /// Iterate paths and make sure materials referencing atlases are set to null.
        /// </summary>
        /// <param name="materials">The materials to clear.</param>
        private static void ClearMaterials(List<Material> materials)
        {
            foreach (Material material in materials)
            {
                Texture mainTex = material.mainTexture;
                if (mainTex != null)
                {
                    if (mainTex.name.Contains("sactx-"))
                    {
                        material.mainTexture = null; // Because atlases are generated, serializeing a reference to an atlas will create broken references.
                        EditorUtility.SetDirty(material);
                    }
                }
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        /// <summary>
        /// Callback triggered when play mode is changed.
        /// </summary>
        /// <param name="state">The new state.</param>
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                List<Material> materials = new List<Material>();
                string[] guids = AssetDatabase.FindAssets("t:material");
                foreach (string guid in guids)
                {
                    materials.Add(AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)));
                }

                ClearMaterials(materials);
            }
        }

        /// <summary>
        /// Contains configuration settings for sprites.
        /// </summary>
        private struct Settings
        {
            /// <summary>
            /// Gets whether texture has alpha.
            /// </summary>
            public bool Alpha { get; private set; }

            /// <summary>
            /// Gets whether texture has mips.
            /// </summary>
            public bool Mips { get; private set; }
            
            /// <summary>
            /// The constructor.
            /// </summary>
            /// <param name="sprite">The sprite to create settings for.</param>
            public Settings(Sprite sprite)
            {
                this.Alpha = HasAlpha(sprite.texture.format);
                this.Mips = sprite.texture.mipmapCount > 1;
            }
        }

        /// <summary>
        /// Class containing interface which is triggered when assets are modified.
        /// </summary>
        private class AssetEvent : AssetModificationProcessor
        {
            /// <summary>
            /// Triggered when project is saved.
            /// </summary>
            /// <param name="paths">The paths to be saved.</param>
            /// <returns>The next paths.</returns>
            private static string[] OnWillSaveAssets(string[] paths)
            {
                List<Material> materials = new List<Material>();

                // Iterate paths and find materials.
                foreach (string path in paths)
                {
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (material != null)
                    {
                        materials.Add(material);
                    }
                }

                ClearMaterials(materials);

                return paths;
            }
        }

        /// <summary>
        /// Class containing interface which is triggered on build.
        /// </summary>
        private class BuildEvent : IPreprocessBuildWithReport, IPostprocessBuildWithReport
        {
            /// <summary>
            /// Gets the callback order.
            /// </summary>
            public int callbackOrder
            {
                get
                {
                    return 0;
                }
            }

            /// <summary>
            /// Generates sprite atlases.
            /// Triggered before build.
            /// </summary>
            /// <param name="report">Build report.</param>
            public void OnPreprocessBuild(BuildReport report)
            {
                GenerateAtlases();
            }

            /// <summary>
            /// Deletes sprite atlases.
            /// Triggered after build.
            /// </summary>
            /// <param name="report">Build report.</param>
            public void OnPostprocessBuild(BuildReport report)
            {
                DeleteAtlases();
            }
        }
    }
}
