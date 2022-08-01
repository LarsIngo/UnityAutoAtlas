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

    public static class GUI
    {
        [MenuItem("Tools/AutoAtlas/DeleteAtlases")]
        private static void DeleteAtlases()
        {
            AutoAtlas.DeleteAtlases();
        }

        [MenuItem("Tools/AutoAtlas/CreateAtlases")]
        private static void CreateAtlases()
        {
            AutoAtlas.CreateAtlases();
        }

        [MenuItem("Tools/AutoAtlas/ToggleAutoAtlas")]
        private static void ToggleAutoAtlas()
        {
            Settings.Profile.AutoAtlasEnabled = !Settings.Profile.AutoAtlasEnabled;
            Settings.Profile.Apply();
        }

        [MenuItem("Tools/AutoAtlas/TogglePlayMode")]
        private static void TogglePlayMode()
        {
            Settings.Profile.PlayModeEnabled = !Settings.Profile.PlayModeEnabled;
            Settings.Profile.Apply();
        }

        //[MenuItem("Tools/AutoAtlas/ResetSetting")]
        //private static void ResetSetting()
        //{
        //    Settings.ResetSettings();
        //}

        [MenuItem("Assets/Create/AutoAtlas Settings", priority = 201)]
        private static void MenuCreateSettingsProfile()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateSettingsProfile>(), "AutoAtlasSettings.asset", EditorGUIUtility.FindTexture("ScriptableObject Icon"), null);
        }

        private class DoCreateSettingsProfile : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                Profile profile = Settings.CreateProfile(pathName);

                ProjectWindowUtil.ShowCreatedAsset(profile);
            }
        }
    }
}

#endif
