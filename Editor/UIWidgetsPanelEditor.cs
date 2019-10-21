using Unity.UIWidgets.engine;
using UnityEditor;
using UnityEditor.UI;

namespace Unity.UIWidgets.Editor {
    [CustomEditor(typeof(UIWidgetsPanel), true)]
    [CanEditMultipleObjects]
    public class UIWidgetsPanelEditor : RawImageEditor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            var pixelRatioProperty = this.serializedObject.FindProperty("devicePixelRatioOverride");
            var antiAliasingProperty = this.serializedObject.FindProperty("hardwareAntiAliasing");
            EditorGUILayout.PropertyField(pixelRatioProperty);
            EditorGUILayout.PropertyField(antiAliasingProperty);
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}