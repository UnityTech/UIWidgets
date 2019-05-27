
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

    public struct uiOffset {
        public float dx;
        public float dy;

        public static uiOffset fromOffset(Offset offset) {
            return new uiOffset {
                dx = offset.dx,
                dy = offset.dy
            };
        }
    }

    public struct uiSize {
        public float width;
        public float height;

        public static uiSize fromSize(Size size) {
            return new uiSize {
                width = size.width,
                height = size.height
            };
        }
    }
}