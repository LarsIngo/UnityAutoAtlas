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
            PlayModeEnabled
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

        private Container container = new Container();

        public Profile()
        {
            Selection.selectionChanged += () =>
            {
                if (Selection.activeObject == this)
                {
                    this.SaveBackup();
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

            this.Apply();
        }

        public void SaveBackup()
        {
            this.container.Save();
        }

        private void LoadBackup()
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
            private enum Status
            {
                Done,
                ShouldUpdate,
                IsUpdated,
            }

            private Status status = Status.Done;

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

                target.AutoAtlasEnabled = EditorGUILayout.Toggle("Enabled", target.AutoAtlasEnabled);

                EditorGUI.BeginDisabledGroup(!target.AutoAtlasEnabled);
                target.PlayModeEnabled = EditorGUILayout.Toggle("Play Mode", target.PlayModeEnabled);
                EditorGUI.EndDisabledGroup();

                if (editor.status == Status.ShouldUpdate)
                {
                    if (GUILayout.Button("Apply"))
                    {
                        target.SaveBackup();
                        target.Apply();
                        editor.status = Status.IsUpdated;
                    }
                    if (GUILayout.Button("Discard"))
                    {
                        target.LoadBackup();
                        editor.status = Status.IsUpdated;
                    }
                }

                if (GUILayout.Button("Reset"))
                {
                    target.ResetProfile();
                    editor.status = Status.IsUpdated;
                }
            }

            /// <summary>
            /// Unity method called when the inspector is drawn.
            /// </summary>
            public override void OnInspectorGUI()
            {
                EditorGUI.BeginChangeCheck();

                DrawDefaultInspector();

                DrawCustomInspector((Profile)this.target, this);

                if (EditorGUI.EndChangeCheck())
                {
                    if (this.status == Status.IsUpdated)
                    {
                        this.status = Status.Done;
                    }
                    else if (this.status == Status.Done)
                    {
                        this.status = Status.ShouldUpdate;
                    }
                }
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
