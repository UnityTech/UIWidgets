using System.Collections.Generic;
using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;


namespace UIWidgetsSample {

    class TheatreEntry {
        public string entryName;
        public Widget entryWidget;
    }
    
    class UIWidgetsTheatre : UIWidgetsSamplePanel {
        static readonly List<TheatreEntry> entries = new List<TheatreEntry> {
            new TheatreEntry{entryName = "Material App Bar", entryWidget = new MaterialAppBarWidget()},
            new TheatreEntry{entryName = "Material Tab Bar" , entryWidget = new MaterialTabBarWidget()}
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
            base.OnEnable();
            FontManager.instance.addFont(Resources.Load<Font>(path: "MaterialIcons-Regular"));
        }
    }
    
    
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
}