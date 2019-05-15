using UnityEditor.Experimental.UIElements;

namespace Unity.UIWidgets.ui {
    
    public struct uiRect {

        public float top;
        public float left;
        public float width;
        public float height;

        public static uiRect fromRect(Rect rect) {
            return new uiRect {
                top = rect.top,
                left = rect.left,
                width = rect.width,
                height = rect.height
            };
        }
    }
}