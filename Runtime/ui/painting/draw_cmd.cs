using UnityEngine;

namespace Unity.UIWidgets.ui {
    public abstract class DrawCmd {
        public abstract uiRect bounds(float margin);
    }

    public abstract class StateUpdateDrawCmd : DrawCmd {
        public override uiRect bounds(float margin) {
            return uiRectHelper.zero;
        }
    }

    public class DrawSave : StateUpdateDrawCmd {
    }

    public class DrawSaveLayer : StateUpdateDrawCmd {
        public Rect rect;
        public Paint paint;
    }

    public class DrawRestore : StateUpdateDrawCmd {
    }

    public class DrawTranslate : StateUpdateDrawCmd {
        public float dx;
        public float dy;
    }

    public class DrawScale : StateUpdateDrawCmd {
        public float sx;
        public float? sy;
    }

    public class DrawRotate : StateUpdateDrawCmd {
        public float radians;
        public Offset offset;
    }

    public class DrawSkew : StateUpdateDrawCmd {
        public float sx;
        public float sy;
    }

    public class DrawConcat : StateUpdateDrawCmd {
        public Matrix3 matrix;
    }

    public class DrawResetMatrix : StateUpdateDrawCmd {
    }

    public class DrawSetMatrix : StateUpdateDrawCmd {
        public Matrix3 matrix;
    }

    public class DrawClipRect : StateUpdateDrawCmd {
        public Rect rect;
    }

    public class DrawClipRRect : StateUpdateDrawCmd {
        public RRect rrect;
    }

    public class DrawClipPath : StateUpdateDrawCmd {
        public Path path;
    }

    public class DrawPath : DrawCmd {
        public Path path;
        public Paint paint;

        public override uiRect bounds(float margin) {
            return uiRectHelper.fromRect(this.path.getBoundsWithMargin(margin)).Value;
        }
    }

    public class DrawImage : DrawCmd {
        public Image image;
        public Offset offset;
        public Paint paint;

        // TODO: Should divide by device pixel ratio here, which is not available as
        //       this DrawCmd is created. This bounds should only used as an upper bound,
        //       assuming that device pixel ratio is always >= 1
        public override uiRect bounds(float margin) {
            return uiRectHelper.fromLTWH(
                this.offset.dx - margin, this.offset.dy - margin,
                this.image.width + 2 * margin,
                this.image.height + 2 * margin);
        }
    }

    public class DrawImageRect : DrawCmd {
        public Image image;
        public Rect src;
        public Rect dst;
        public Paint paint;
        
        public override uiRect bounds(float margin) {
            return uiRectHelper.fromRect(this.dst.inflate(margin)).Value;
        }
    }

    public class DrawImageNine : DrawCmd {
        public Image image;
        public Rect src;
        public Rect center;
        public Rect dst;
        public Paint paint;

        public override uiRect bounds(float margin) {
            return uiRectHelper.fromLTRB(
                this.dst.left + ((this.center.left - this.src.left) * this.src.width) - margin,
                this.dst.top + ((this.center.top - this.src.top) * this.src.height) - margin,
                this.dst.right - ((this.src.right - this.center.right) * this.src.width) + margin,
                this.dst.bottom - ((this.src.bottom - this.center.bottom) * this.src.height) + margin
            );
        }
    }

    public class DrawPicture : DrawCmd {
        public Picture picture;
        public override uiRect bounds(float margin) {
            return uiRectHelper.fromRect(this.picture.paintBounds.inflate(margin)).Value;
        }
    }

    public class DrawTextBlob : DrawCmd {
        public TextBlob? textBlob;
        public Offset offset;
        public Paint paint;

        public override uiRect bounds(float margin) {
            return uiRectHelper.fromRect(this.textBlob.Value.boundsInText
                .translate(this.offset.dx, this.offset.dy)
                .inflate(margin)).Value;
        }
    }
}