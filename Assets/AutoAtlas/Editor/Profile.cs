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

    public class Profile : ScriptableObject
    {
        private enum PropertyType
        {
            AutoAtlasEnabled,
            PlayModeEnabled,
            IsReadable,
            MipMap,
            sRGB,
            FilterMode,
            MaxSize
        }

        public bool AutoAtlasEnabled
        {
            get
            {
                string key = PropertyType.AutoAtlasEnabled.ToString();
                bool value = false;
                if (!this.container.GetBool(key, ref value))
                {
                    Debug.LogWarning(this.GetType().FullName + ": Unable to find key (" + key + ").");
                }

                return value;
            }
            set
            {
                string key = PropertyType.AutoAtlasEnabled.ToString();
                this.container.SetBool(key, value);
            }
        }

        public bool PlayModeEnabled
        {
            get
            {
                string key = PropertyType.PlayModeEnabled.ToString();
                bool value = false;
                if (!this.container.GetBool(key, ref value))
                {
                    Debug.LogWarning(this.GetType().FullName + ": Unable to find key (" + key + ").");
                }

                return value;
            }
            set
            {
                string key = PropertyType.PlayModeEnabled.ToString();
                this.container.SetBool(key, value);
            }
        }

        public bool IsReadable
        {
            get
            {
                string key = PropertyType.IsReadable.ToString();
                bool value = false;
                if (!this.container.GetBool(key, ref value))
                {
                    Debug.LogWarning(this.GetType().FullName + ": Unable to find key (" + key + ").");
                }

                return value;
            }
            set
            {
                string key = PropertyType.IsReadable.ToString();
                this.container.SetBool(key, value);
            }
        }

        public bool MipMap
        {
            get
            {
                string key = PropertyType.MipMap.ToString();
                bool value = false;
                if (!this.container.GetBool(key, ref value))
                {
                    Debug.LogWarning(this.GetType().FullName + ": Unable to find key (" + key + ").");
                }

                return value;
            }
            set
            {
                string key = PropertyType.MipMap.ToString();
                this.container.SetBool(key, value);
            }
        }

        public bool sRGB
        {
            get
            {
                string key = PropertyType.sRGB.ToString();
                bool value = false;
                if (!this.container.GetBool(key, ref value))
                {
                    Debug.LogWarning(this.GetType().FullName + ": Unable to find key (" + key + ").");
                }

                return value;
            }
            set
            {
                string key = PropertyType.sRGB.ToString();
                this.container.SetBool(key, value);
            }
        }

        public FilterMode FilterMode
        {
            get
            {
                string key = PropertyType.FilterMode.ToString();
                int value = 0;
                if (!this.container.GetInt(key, ref value))
                {
                    Debug.LogWarning(this.GetType().FullName + ": Unable to find key (" + key + ").");
                }

                return (FilterMode)value;
            }
            set
            {
                string key = PropertyType.FilterMode.ToString();
                this.container.SetInt(key, (int)value);
            }
        }

        public int MaxSize
        {
            get
            {
                string key = PropertyType.MaxSize.ToString();
                int value = 0;
                if (!this.container.GetInt(key, ref value))
                {
                    Debug.LogWarning(this.GetType().FullName + ": Unable to find key (" + key + ").");
                }

                return value;
            }
            set
            {
                string key = PropertyType.MaxSize.ToString();
                this.container.SetInt(key, value);
            }
        }

        private Container container = new Container();

        private bool selected = false;

        public Profile()
        {
            Selection.selectionChanged += () =>
            {
                if (Selection.activeObject == this && !this.selected)
                {
                    this.selected = true;
                    this.SaveBackup();
                }
                else if (this.selected)
                {
                    this.selected = false;
                    if (!this.container.Empty())
                    {
                        this.LoadBackup();
                    }
                }
            };
        }

        public void Apply()
        {
            if (this.AutoAtlasEnabled)
            {
                EditorSettings.spritePackerMode = this.PlayModeEnabled ? SpritePackerMode.AlwaysOnAtlas : SpritePackerMode.BuildTimeOnlyAtlas;
            }
            else
            {

            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        public void ResetProfile()
        {
            this.container.Clear();

            this.AutoAtlasEnabled = true;
            this.PlayModeEnabled = false;
            this.IsReadable = false;
            this.MipMap = false;
            this.sRGB = true;
            this.FilterMode = FilterMode.Bilinear;
            this.MaxSize = 4096;

            this.Apply();
        }

        public void SaveBackup()
        {
            this.container.Save();
        }

        public void LoadBackup()
        {
            this.container.Load();
        }

        /// <summary>
        /// Class which creates buttons in the inspector for this class.
        /// </summary>
        [InitializeOnLoad]
        [CustomEditor(typeof(Profile))]
        public class InspectorEditor : Editor
        {
            private enum FoldoutStatus
            {
                Standalone = 1 << 0,
                Android = 1 << 1,
                iPhone = 1 << 2
            }

            private bool shouldUpdate = false;

            private FoldoutStatus foldoutStatus = 0;

            /// <summary>
            /// Draws the custom inspector.
            /// </summary>
            /// <param name="target">Target object.</param>
            public static void DrawCustomInspector(Profile target, InspectorEditor editor)
            {
                if (target.container.Empty())
                {
                    target.container.Load();
                }

                EditorGUILayout.LabelField("General settings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                target.AutoAtlasEnabled = EditorGUILayout.Toggle("Enabled", target.AutoAtlasEnabled);
                editor.shouldUpdate = EditorGUI.EndChangeCheck() ? true : editor.shouldUpdate;

                EditorGUI.BeginDisabledGroup(!target.AutoAtlasEnabled);

                EditorGUI.BeginChangeCheck();
                target.PlayModeEnabled = EditorGUILayout.Toggle("Play Mode", target.PlayModeEnabled);
                editor.shouldUpdate = EditorGUI.EndChangeCheck() ? true : editor.shouldUpdate;

                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField("Texture Settings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                target.IsReadable = EditorGUILayout.Toggle("Read/Write Enabled", target.IsReadable);
                editor.shouldUpdate = EditorGUI.EndChangeCheck() ? true : editor.shouldUpdate;

                EditorGUI.BeginChangeCheck();
                target.MipMap = EditorGUILayout.Toggle("Generate Mip Maps", target.MipMap);
                editor.shouldUpdate = EditorGUI.EndChangeCheck() ? true : editor.shouldUpdate;

                EditorGUI.BeginChangeCheck();
                target.sRGB = EditorGUILayout.Toggle("sRGB", target.sRGB);
                editor.shouldUpdate = EditorGUI.EndChangeCheck() ? true : editor.shouldUpdate;

                EditorGUI.BeginChangeCheck();
                target.FilterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", target.FilterMode);
                editor.shouldUpdate = EditorGUI.EndChangeCheck() ? true : editor.shouldUpdate;

                EditorGUI.BeginChangeCheck();
                target.MaxSize = EditorGUILayout.IntPopup("Max Size", target.MaxSize, new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" }, new int[] { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 });
                editor.shouldUpdate = EditorGUI.EndChangeCheck() ? true : editor.shouldUpdate;

                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField("Platform Settings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                if (EditorGUILayout.Foldout((editor.foldoutStatus & FoldoutStatus.Standalone) == FoldoutStatus.Standalone, "PC, Mac & Linus Standalone", true))
                {
                    editor.foldoutStatus |= FoldoutStatus.Standalone;

                    EditorGUI.BeginChangeCheck();
                    target.MaxSize = EditorGUILayout.IntPopup("Max Size", target.MaxSize, new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" }, new int[] { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 });
                    editor.shouldUpdate = EditorGUI.EndChangeCheck() ? true : editor.shouldUpdate;
                }
                else
                {
                    editor.foldoutStatus &= ~FoldoutStatus.Standalone;
                }

                if (EditorGUILayout.Foldout((editor.foldoutStatus & FoldoutStatus.iPhone) == FoldoutStatus.iPhone, "iOS", true))
                {
                    editor.foldoutStatus |= FoldoutStatus.iPhone;

                    EditorGUI.BeginChangeCheck();
                    target.MaxSize = EditorGUILayout.IntPopup("Max Size", target.MaxSize, new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" }, new int[] { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 });
                    editor.shouldUpdate = EditorGUI.EndChangeCheck() ? true : editor.shouldUpdate;
                }
                else
                {
                    editor.foldoutStatus &= ~FoldoutStatus.iPhone;
                }

                if (EditorGUILayout.Foldout((editor.foldoutStatus & FoldoutStatus.Android) == FoldoutStatus.Android, "Android", true))
                {
                    editor.foldoutStatus |= FoldoutStatus.Android;

                    EditorGUI.BeginChangeCheck();
                    target.MaxSize = EditorGUILayout.IntPopup("Max Size", target.MaxSize, new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" }, new int[] { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 });
                    editor.shouldUpdate = EditorGUI.EndChangeCheck() ? true : editor.shouldUpdate;
                }
                else
                {
                    editor.foldoutStatus &= ~FoldoutStatus.Android;
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();

                EditorGUI.BeginDisabledGroup(!editor.shouldUpdate);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Apply"))
                {
                    target.SaveBackup();
                    target.Apply();
                    editor.shouldUpdate = false;
                }

                if (GUILayout.Button("Discard"))
                {
                    target.LoadBackup();
                    editor.shouldUpdate = false;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("Reset"))
                {
                    target.ResetProfile();
                    editor.shouldUpdate = false;
                }
            }

            /// <summary>
            /// Unity method called when the inspector is drawn.
            /// </summary>
            public override void OnInspectorGUI()
            {
                Profile target = (Profile)this.target;

                if (target.container.Empty())
                {
                    target.LoadBackup();
                }

                DrawDefaultInspector();

                DrawCustomInspector(target, this);
            }
        }

        private class Container
        {
            private Dictionary<string, string> dictionary = new Dictionary<string, string>();

            public Container()
            {
            }

            public bool ContainsKey(string key)
            {
                return this.dictionary.ContainsKey(key);
            }

            public bool GetBool(string key, ref bool value)
            {
                int v = value ? 1 : 0;
                if (this.GetInt(key, ref v))
                {
                    value = v == 1;
                    return true;
                }

                return false;
            }

            public void SetBool(string key, bool value)
            {
                this.SetInt(key, value ? 1 : 0);
            }

            public bool GetInt(string key, ref int value)
            {
                if (this.ContainsKey(key))
                {
                    return int.TryParse(this.dictionary[key], out value);
                }

                return false;
            }

            public void SetInt(string key, int value)
            {
                this.dictionary[key] = value.ToString();
            }

            public bool GetString(string key, ref string value)
            {
                if (this.ContainsKey(key))
                {
                    value = this.dictionary[key];
                    return true;
                }

                return false;
            }

            public void SetString(string key, string value)
            {
                this.dictionary[key] = value;
            }

            public void Clear()
            {
                this.dictionary.Clear();
            }

            public void Save()
            {
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                System.Xml.XmlNode rootNode = xmlDoc.CreateElement(Utility.GetSettingsName());
                xmlDoc.AppendChild(rootNode);

                foreach (KeyValuePair<string, string> pair in this.dictionary)
                {
                    System.Xml.XmlNode node = xmlDoc.CreateElement(pair.Key);
                    node.InnerText = pair.Value;
                    rootNode.AppendChild(node);
                }

                using (var stringWriter = new System.IO.StringWriter())
                {
                    using (var xmlTextWriter = System.Xml.XmlWriter.Create(stringWriter))
                    {
                        xmlDoc.WriteTo(xmlTextWriter);
                        xmlTextWriter.Flush();
                        EditorPrefs.SetString(Utility.GetSettingsName(), stringWriter.GetStringBuilder().ToString());
                    }
                }
            }

            public void Load()
            {
                this.Clear();

                string xml = EditorPrefs.GetString(Utility.GetSettingsName(), null);
                if (!string.IsNullOrEmpty(xml))
                {
                    System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                    xmlDoc.LoadXml(xml);

                    foreach (System.Xml.XmlNode node in xmlDoc.DocumentElement)
                    {
                        this.dictionary[node.Name] = node.InnerText;
                    }
                }
            }

            public bool Empty()
            {
                return this.dictionary.Count == 0;
            }
        }
    }
}

#endif
