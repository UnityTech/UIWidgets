using System.Collections.Generic;
using UIWidgetsGallery.gallery;
using UIWidgetsSample;
using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

using UnityEngine;


namespace UIWidgetsTheatre {

    class TheatreEntry {
        public string entryName;
        public Widget entryWidget;
    }
    
    class UIWidgetsTheatre : UIWidgetsSamplePanel {
        static readonly List<TheatreEntry> entries = new List<TheatreEntry> {
            new TheatreEntry{entryName = "UIWidget Gallery", entryWidget =  new GalleryApp()},
            new TheatreEntry{entryName = "Material App Bar", entryWidget = new MaterialAppBarWidget()},
            new TheatreEntry{entryName = "Material Tab Bar" , entryWidget = new MaterialTabBarWidget()},
            new TheatreEntry{entryName = "Asset Store", entryWidget = new AsScreenSample.AsScreenWidget()},
            new TheatreEntry{entryName = "ToDo App", entryWidget = new ToDoAppSample.ToDoListApp()}
        };

        public static string[] entryKeys {
            get {
                List<string> ret = new List<string>();
                foreach (var entry in entries) {
                    ret.Add(entry.entryName);
                }

                return ret.ToArray();
            }
        }
        
        [SerializeField] public int testCaseId;

        protected override Widget createWidget() {
            return new MaterialApp(
                showPerformanceOverlay: false,
                home: entries[this.testCaseId].entryWidget);
        }

        protected override void OnEnable() {
            FontManager.instance.addFont(Resources.Load<Font>("MaterialIcons-Regular"), "Material Icons");
            FontManager.instance.addFont(Resources.Load<Font>("GalleryIcons"), "GalleryIcons");
            base.OnEnable();
        }
    }
    
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(UIWidgetsTheatre), true)]
    [CanEditMultipleObjects]
    public class UIWidgetTheatreEditor : RawImageEditor {
        int _choiceIndex;
        
        public override void OnInspectorGUI() {
            var materialSample = this.target as UIWidgetsTheatre;
            this._choiceIndex = EditorGUILayout.Popup("Test Case", materialSample.testCaseId, UIWidgetsTheatre.entryKeys);
            materialSample.testCaseId = this._choiceIndex;
            EditorUtility.SetDirty(this.target);
        }
    }
    #endif
}