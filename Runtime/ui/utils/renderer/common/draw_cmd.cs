namespace Unity.UIWidgets.ui {
    public abstract class uiDrawCmd {
    }

    public class uiDrawSave : uiDrawCmd {
    }

    public class uiDrawSaveLayer : uiDrawCmd {
        public Rect rect;
        public Paint paint;
    }

    public class uiDrawRestore : uiDrawCmd {
    }

    public class uiDrawTranslate : uiDrawCmd {
        public float dx;
        public float dy;
    }

    public class uiDrawScale : uiDrawCmd {
        public float sx;
        public float? sy;
    }

    public class uiDrawRotate : uiDrawCmd {
        public float radians;
        public Offset offset;
    }

    public class uiDrawSkew : uiDrawCmd {
        public float sx;
        public float sy;
    }

    public class uiDrawConcat : uiDrawCmd {
        public Matrix3 matrix;
    }

    public class uiDrawResetMatrix : uiDrawCmd {
    }

    public class uiDrawSetMatrix : uiDrawCmd {
        public Matrix3 matrix;
    }

    public class uiDrawClipRect : uiDrawCmd {
        public Rect rect;
    }

    public class uiDrawClipRRect : uiDrawCmd {
        public RRect rrect;
    }

    public class uiDrawClipPath : uiDrawCmd {
        public Path path;
    }

    public class uiDrawPath : uiDrawCmd {
        public Path path;
        public Paint paint;
    }

    public class uiDrawImage : uiDrawCmd {
        public Image image;
        public Offset offset;
        public Paint paint;
    }

    public class uiDrawImageRect : uiDrawCmd {
        public Image image;
        public Rect src;
        public Rect dst;
        public Paint paint;
    }

    public class uiDrawImageNine : uiDrawCmd {
        public Image image;
        public Rect src;
        public Rect center;
        public Rect dst;
        public Paint paint;
    }

    public class uiDrawPicture : uiDrawCmd {
        public Picture picture;
    }

    public class uiDrawTextBlob : uiDrawCmd {
        public TextBlob textBlob;
        public Offset offset;
        public Paint paint;
    }
}