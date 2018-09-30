using UIWidgets.painting;
using UIWidgets.ui.txt;
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

    public class DrawImageRect : DrawCmd {
        public Paint paint;
        public Image image;
        public Rect src;
        public Rect dest;
    }

    public class DrawConcat : DrawCmd {
        public Matrix4x4 transform;
    }
    
    public class DrawSetMatrix : DrawCmd {
        public Matrix4x4 matrix;
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
    
    public class DrawTextBlob : DrawCmd
    {
        public TextBlob textBlob;
        public Offset offset;
    }

    public class DrawLine : DrawCmd
    {
        public Offset from;
        public Offset to;
        public Paint paint;
    }

}

