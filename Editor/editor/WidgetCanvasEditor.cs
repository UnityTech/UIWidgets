using Unity.UIWidgets.engine;
using UnityEditor;
using UnityEditor.UI;

namespace Unity.UIWidgets.Editor {
    [CustomEditor(typeof(WidgetCanvas), true)]
    [CanEditMultipleObjects]
    public class WidgetCanvasEditor : RawImageEditor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            var pixelRatioProperty = this.serializedObject.FindProperty("devicePixelRatioOverride");
            EditorGUILayout.PropertyField(pixelRatioProperty);
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}
