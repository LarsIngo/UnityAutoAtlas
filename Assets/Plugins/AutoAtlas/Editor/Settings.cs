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

    [InitializeOnLoad]
    public static class Settings
    {
        private static Profile profile;

        public static Profile Profile
        {
            get
            {
                return profile;
            }
        }

        static Settings()
        {
            profile = LoadProfile();
        }

        public static Profile CreateProfile(string path)
        {
            Profile profile = ScriptableObject.CreateInstance<Profile>();
            profile.name = System.IO.Path.GetFileName(path);
            profile.ResetProfile();

            AssetDatabase.CreateAsset(profile, path);

            return profile;
        }

        private static Profile LoadProfile()
        {
            string path = Utility.GetSettingsPath();

            Profile profile;
            if (string.IsNullOrEmpty(path))
            {
                profile = CreateProfile(Utility.GetSettingsPath());
            }
            else
            {
                profile = AssetDatabase.LoadAssetAtPath<Profile>(path);
            }

            profile.LoadBackup();

            return profile;
        }
    }
}

#endif
