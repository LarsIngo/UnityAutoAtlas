#if UNITY_EDITOR
namespace AutoAtlas.Editor
{
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;

    [InitializeOnLoad]
    public static class EventListener
    {
        static EventListener()
        {
            EditorApplication.playModeStateChanged += OnPlayModeState;
        }

        private static void OnPlayModeState(PlayModeStateChange state)
        {
            if (Settings.Profile.AutoAtlasEnabled && Settings.Profile.PlayModeEnabled)
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

#endif
