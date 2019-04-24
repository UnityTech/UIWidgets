using System.Runtime.ExceptionServices;
using Unity.UIWidgets.utils;

namespace Unity.UIWidgets.ui {
    public abstract class DrawCmd : Clearable {
        public virtual void clear() {
            
        }
    }

    public class DrawSave : DrawCmd {
        public static DrawSave createNew() {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawSave>.instance.fetch()
                : new DrawSave();

            return ret;
        }
    }

    public class DrawSaveLayer : DrawCmd {

        public static DrawSaveLayer createNew(Rect rect, Paint paint) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawSaveLayer>.instance.fetch()
                : new DrawSaveLayer();
            ret.rect = rect;
            ret.paint = paint;

            return ret;
        }

        public override void clear() {
            this.rect = null;
            this.paint = null;
        }
        
        public Rect rect;
        public Paint paint;
    }

    public class DrawRestore : DrawCmd {
        public static DrawRestore createNew() {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawRestore>.instance.fetch()
                : new DrawRestore();

            return ret;
        }
    }

    public class DrawTranslate : DrawCmd {
        
        public static DrawTranslate createNew(float dx, float dy) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawTranslate>.instance.fetch()
                : new DrawTranslate();

            ret.dx = dx;
            ret.dy = dy;

            return ret;
        }
        
        public float dx;
        public float dy;
    }

    public class DrawScale : DrawCmd {
        
        public static DrawScale createNew(float sx, float? sy) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawScale>.instance.fetch()
                : new DrawScale();

            ret.sx = sx;
            ret.sy = sy;

            return ret;
        }
        
        public float sx;
        public float? sy;
    }

    public class DrawRotate : DrawCmd {
        
        public static DrawRotate createNew(float radians, Offset offset) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawRotate>.instance.fetch()
                : new DrawRotate();

            ret.radians = radians;
            ret.offset = offset;

            return ret;
        }

        public override void clear() {
            this.offset = null;
        }
        
        public float radians;
        public Offset offset;
    }

    public class DrawSkew : DrawCmd {
        
        public static DrawSkew createNew(float sx, float sy) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawSkew>.instance.fetch()
                : new DrawSkew();

            ret.sx = sx;
            ret.sy = sy;

            return ret;
        }
        
        public float sx;
        public float sy;
    }

    public class DrawConcat : DrawCmd {
        public static DrawConcat createNew(Matrix3 matrix) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawConcat>.instance.fetch()
                : new DrawConcat();

            ret.matrix = matrix;
            return ret;
        }

        public override void clear() {
            this.matrix = null;
        }
        
        public Matrix3 matrix;
    }

    public class DrawResetMatrix : DrawCmd {
        public static DrawResetMatrix createNew() {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawResetMatrix>.instance.fetch()
                : new DrawResetMatrix();

            return ret;
        }
    }

    public class DrawSetMatrix : DrawCmd {
        public static DrawSetMatrix createNew(Matrix3 matrix) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawSetMatrix>.instance.fetch()
                : new DrawSetMatrix();

            ret.matrix = matrix;
            return ret;
        }

        public override void clear() {
            this.matrix = null;
        }
        
        public Matrix3 matrix;
    }

    public class DrawClipRect : DrawCmd {
        public static DrawClipRect createNew(Rect rect) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawClipRect>.instance.fetch()
                : new DrawClipRect();

            ret.rect = rect;
            return ret;
        }

        public override void clear() {
            this.rect = null;
        }
        
        public Rect rect;
    }

    public class DrawClipRRect : DrawCmd {
        public static DrawClipRRect createNew(RRect rrect) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawClipRRect>.instance.fetch()
                : new DrawClipRRect();

            ret.rrect = rrect;
            return ret;
        }

        public override void clear() {
            this.rrect = null;
        }
        
        public RRect rrect;
    }

    public class DrawClipPath : DrawCmd {
        public static DrawClipPath createNew(Path path) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawClipPath>.instance.fetch()
                : new DrawClipPath();

            ret.path = path;
            return ret;
        }

        public override void clear() {
            this.path = null;
        }
        public Path path;
    }

    public class DrawPath : DrawCmd {
        public static DrawPath createNew(Path path, Paint paint) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawPath>.instance.fetch()
                : new DrawPath();

            ret.path = path;
            ret.paint = paint;
            return ret;
        }

        public override void clear() {
            this.path = null;
            this.paint = null;
        }
        
        public Path path;
        public Paint paint;
    }

    public class DrawImage : DrawCmd {
        public static DrawImage createNew(Image image, Offset offset, Paint paint) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawImage>.instance.fetch()
                : new DrawImage();

            ret.image = image;
            ret.offset = offset;
            ret.paint = paint;
            return ret;
        }

        public override void clear() {
            this.image = null;
            this.offset = null;
            this.paint = null;
        }
        
        public Image image;
        public Offset offset;
        public Paint paint;
    }

    public class DrawImageRect : DrawCmd {
        
        public static DrawImageRect createNew(Image image, Rect src, Rect dst, Paint paint) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawImageRect>.instance.fetch()
                : new DrawImageRect();

            ret.image = image;
            ret.src = src;
            ret.dst = dst;
            ret.paint = paint;
            return ret;
        }

        public override void clear() {
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
        public static DrawImageNine createNew(Image image, Rect src, Rect center, Rect dst, Paint paint) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawImageNine>.instance.fetch()
                : new DrawImageNine();

            ret.image = image;
            ret.src = src;
            ret.center = center;
            ret.dst = dst;
            ret.paint = paint;
            return ret;
        }

        public override void clear() {
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
        public static DrawPicture createNew(Picture picture) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawPicture>.instance.fetch()
                : new DrawPicture();

            ret.picture = picture;
            return ret;
        }

        public override void clear() {
            this.picture = null;
        }
        public Picture picture;
    }

    public class DrawTextBlob : DrawCmd {
        public static DrawTextBlob createNew(TextBlob textBlob, Offset offset, Paint paint) {
            var ret = GcCacheHelper.optimizing
                ? ClearableSimpleFlash<DrawTextBlob>.instance.fetch()
                : new DrawTextBlob();

            ret.textBlob = textBlob;
            ret.offset = offset;
            ret.paint = paint;
            return ret;
        }

        public override void clear() {
            this.textBlob = null;
            this.offset = null;
            this.paint = null;
        }
        
        public TextBlob textBlob;
        public Offset offset;
        public Paint paint;
    }
}