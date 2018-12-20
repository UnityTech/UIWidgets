#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace UIWidgets.editor {


    public class EditorWindowAdapter : WindowAdapter
    {
        public readonly EditorWindow editorWindow;
        
        public EditorWindowAdapter(EditorWindow editorWindow):base(editorWindow.position, EditorGUIUtility.pixelsPerPoint)
        {
            this.editorWindow = editorWindow;
            editorWindow.wantsMouseMove = true;
            editorWindow.wantsMouseEnterLeaveWindow = true;
        }
        
        public override void scheduleFrame() {
            if (this.editorWindow != null) {
                this.editorWindow.Repaint();
            }
        }
        
        public override GUIContent titleContent
        {
            get { return editorWindow.titleContent; }
        }

        protected override void getWindowMetrics(out double devicePixelRatio, out Rect position)
        {
            devicePixelRatio = EditorGUIUtility.pixelsPerPoint;
            position = this.editorWindow.position;
        }
    }

}
#endif