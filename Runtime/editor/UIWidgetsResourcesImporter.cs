#if UNITY_EDITOR
using UnityEngine.Serialization;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Unity.UIWidgets.editor
{
    [System.Serializable]
    public class UIWidgetsResourcesImporter
    {
        bool m_UIWidgetsResourcesImported;

        public void OnDestroy()
        {
        }

        public void OnGUI()
        {
            string packageFullPath = GetPackageFullPath();
            m_UIWidgetsResourcesImported = Directory.Exists("Assets/UIWidgetsResources") ||
                                           Directory.Exists(packageFullPath + "/Runtime/Resources/fonts");
            // Display options to import Essential resources
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Label("UIWidgets Resources", EditorStyles.boldLabel);
                GUILayout.Label("We need to add resources to your project that are essential for using UIWidgets. " +
                                "These new resources will be placed at the root of your project in the \"UIWidgetsResources\" folder.",
                    new GUIStyle(EditorStyles.label) { wordWrap = true } );
                GUILayout.Space(5f);

                GUI.enabled = !m_UIWidgetsResourcesImported;
                if (GUILayout.Button("Import UIWidgets Resources"))
                {
                    AssetDatabase.importPackageCompleted += ImportCallback;

                    AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/UIWidgetsResources.unitypackage", false);
                }
                GUILayout.Space(5f);
                GUI.enabled = true;
            }
            GUILayout.EndVertical();
            GUILayout.Space(5f);
        }

        internal void RegisterResourceImportCallback()
        {
            AssetDatabase.importPackageCompleted += ImportCallback;
        }

        void ImportCallback(string packageName)
        {
            if (packageName == "UIWidgetsResources")
            {
                m_UIWidgetsResourcesImported = true;

                #if UNITY_2018_3_OR_NEWER
                SettingsService.NotifySettingsProviderChanged();
                #endif
            }

            Debug.Log("[" + packageName + "] have been imported.");

            AssetDatabase.importPackageCompleted -= ImportCallback;
        }

        static string GetPackageFullPath()
        {
            string packagePath = Path.GetFullPath("Packages/com.unity.uiwidgets");
            if (Directory.Exists(packagePath))
            {
                return packagePath;
            }
            
            packagePath = Path.GetFullPath("Packages/UIWidgets");
            if (Directory.Exists(packagePath))
            {
                return packagePath;
            }
            
            packagePath = Path.GetFullPath("Assets/UIWidgets");
            if (Directory.Exists(packagePath))
            {
                return packagePath;
            }

            return null;
        }
    }

    public class UIWidgetsResourcesImporterWindow : EditorWindow
    {
        [FormerlySerializedAs("m_ResourceImporter")] [SerializeField]
        UIWidgetsResourcesImporter resourcesImporter;

        public static void ShowResourcesImporterWindow()
        {
            var window = GetWindow<UIWidgetsResourcesImporterWindow>();
            window.titleContent = new GUIContent("UIWidgets Resources Importer");
            window.Focus();
        }

        void OnEnable()
        {
            SetEditorWindowSize();

            if (resourcesImporter == null)
                resourcesImporter = new UIWidgetsResourcesImporter();
            resourcesImporter.RegisterResourceImportCallback();
        }

        void OnDestroy()
        {
            resourcesImporter.OnDestroy();
        }

        void OnGUI()
        {
            resourcesImporter.OnGUI();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }
        
        void SetEditorWindowSize()
        {
            EditorWindow editorWindow = this;

            Vector2 windowSize = new Vector2(640, 110);
            editorWindow.minSize = windowSize;
            editorWindow.maxSize = windowSize;
        }
    }

}

#endif
