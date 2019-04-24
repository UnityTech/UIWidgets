using Unity.UIWidgets.utils;

namespace Unity.UIWidgets.ui {
    public abstract class DrawCmd : GcRecyclable {
        public virtual void Recycle() {
            
        }
    }

    public class DrawSave : DrawCmd {
    }

    public class DrawSaveLayer : DrawCmd {
        public override void Recycle() {
            this.rect = null;
            this.paint = null;
        }
        
        public Rect rect;
        public Paint paint;
    }

    public class DrawRestore : DrawCmd {
    }

    public class DrawTranslate : DrawCmd {
        public float dx;
        public float dy;
    }

    public class DrawScale : DrawCmd {
        public float sx;
        public float? sy;
    }

    public class DrawRotate : DrawCmd {
        public override void Recycle() {
            this.offset = null;
        }
        
        public float radians;
        public Offset offset;
    }

    public class DrawSkew : DrawCmd {
        public float sx;
        public float sy;
    }

    public class DrawConcat : DrawCmd {
        public override void Recycle() {
            this.matrix = null;
        }
        
        public Matrix3 matrix;
    }

    public class DrawResetMatrix : DrawCmd {
    }

    public class DrawSetMatrix : DrawCmd {
        public override void Recycle() {
            this.matrix = null;
        }
        
        public Matrix3 matrix;
    }

    public class DrawClipRect : DrawCmd {
        public override void Recycle() {
            this.rect = null;
        }
        
        public Rect rect;
    }

    public class DrawClipRRect : DrawCmd {
        public override void Recycle() {
            this.rrect = null;
        }
        
        public RRect rrect;
    }

    public class DrawClipPath : DrawCmd {
        public override void Recycle() {
            this.path = null;
        }
        public Path path;
    }

    public class DrawPath : DrawCmd {
        public override void Recycle() {
            this.path = null;
            this.paint = null;
        }
        
        public Path path;
        public Paint paint;
    }

    public class DrawImage : DrawCmd {
        public override void Recycle() {
            this.image = null;
            this.offset = null;
            this.paint = null;
        }
        
        public Image image;
        public Offset offset;
        public Paint paint;
    }

    public class DrawImageRect : DrawCmd {
        public override void Recycle() {
            this.image = null;
            this.src = null;
            this.dst = null;
            this.paint = null;
        }
        
        public Image image;
        public Rect src;
        public Rect dst;
        public Paint paint;
    }

    public class DrawImageNine : DrawCmd {
        public override void Recycle() {
            this.image = null;
            this.src = null;
            this.center = null;
            this.dst = null;
            this.paint = null;
        }
        
        public Image image;
        public Rect src;
        public Rect center;
        public Rect dst;
        public Paint paint;
    }

    public class DrawPicture : DrawCmd {
        public override void Recycle() {
            this.picture = null;
        }
        public Picture picture;
    }

    public class DrawTextBlob : DrawCmd {
        public override void Recycle() {
            this.textBlob = null;
            this.offset = null;
            this.paint = null;
        }
        
        public TextBlob textBlob;
        public Offset offset;
        public Paint paint;
    }
}