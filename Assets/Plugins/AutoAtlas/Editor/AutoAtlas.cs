#if UNITY_EDITOR
namespace AutoAtlas.Editor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [InitializeOnLoad]
    public static class AutoAtlas
    {
        static AutoAtlas()
        {
        }

        [MenuItem("AutoAtlas/Log")]
        private static void Log()
        {
            Debug.Log("AutoAtlas.Log");
        }
    }
}

#endif
