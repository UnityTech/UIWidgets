using UIWidgets.painting;
using UnityEngine;

namespace UIWidgets.ui {
    public interface DrawCmd {
    }

    public class DrawPloygon4 : DrawCmd {
        public Offset[] points;
        public Paint paint;
    }

    public class DrawRect : DrawCmd {
        public Rect rect;
        public BorderWidth borderWidth;
        public BorderRadius borderRadius;
        public Paint paint;
    }

    public class DrawRectShadow : DrawCmd {
        public Rect rect;
        public Paint paint;
    }

    public class DrawPicture : DrawCmd {
        public Picture picture;
    }

    public class DrawConcat : DrawCmd {
        public Matrix4x4 transform;
    }

    public class DrawSave : DrawCmd {
    }

    public class DrawSaveLayer : DrawCmd {
        public Rect rect;
        public Paint paint;
    }

    public class DrawRestore : DrawCmd {
    }

    public class DrawClipRect : DrawCmd {
        public Rect rect;
    }

    public class DrawClipRRect : DrawCmd {
        public RRect rrect;
    }
    
    public class DrawMesh : DrawCmd {
        public Mesh mesh;
    }
}