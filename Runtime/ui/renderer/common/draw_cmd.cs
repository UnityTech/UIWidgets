namespace Unity.UIWidgets.ui {
    public abstract class uiDrawCmd : PoolObject {
        public abstract void release();
        public abstract uiRect bounds(float margin);
    }

    public abstract class uiStateUpdateDrawCmd : uiDrawCmd {
        public override uiRect bounds(float margin) {
            return uiRectHelper.zero;
        }
    }

    public class uiDrawSave : uiStateUpdateDrawCmd {
        public uiDrawSave() {
        }

        public static uiDrawSave create() {
            var drawSave = ObjectPool<uiDrawSave>.alloc();
            return drawSave;
        }

        public override void release() {
            ObjectPool<uiDrawSave>.release(this);
        }
    }

    public class uiDrawSaveLayer : uiStateUpdateDrawCmd {
        public uiDrawSaveLayer() {
        }

        public static uiDrawSaveLayer create(uiRect? rect, uiPaint paint) {
            var drawSaveLayer = ObjectPool<uiDrawSaveLayer>.alloc();
            drawSaveLayer.rect = rect;
            drawSaveLayer.paint = paint;
            return drawSaveLayer;
        }

        public override void release() {
            ObjectPool<uiDrawSaveLayer>.release(this);
        }

        public override void clear() {
            this.rect = null;
        }

        public uiRect? rect;
        public uiPaint paint;
    }

    public class uiDrawRestore : uiStateUpdateDrawCmd {
        public uiDrawRestore() {
        }

        public static uiDrawRestore create() {
            var drawRestore = ObjectPool<uiDrawRestore>.alloc();
            return drawRestore;
        }

        public override void release() {
            ObjectPool<uiDrawRestore>.release(this);
        }
    }

    public class uiDrawTranslate : uiStateUpdateDrawCmd {
        public uiDrawTranslate() {
        }

        public static uiDrawTranslate create(float dx, float dy) {
            var drawTranslate = ObjectPool<uiDrawTranslate>.alloc();
            drawTranslate.dx = dx;
            drawTranslate.dy = dy;
            return drawTranslate;
        }

        public override void release() {
            ObjectPool<uiDrawTranslate>.release(this);
        }

        public float dx;
        public float dy;
    }

    public class uiDrawScale : uiStateUpdateDrawCmd {
        public uiDrawScale() {
        }

        public static uiDrawScale create(float sx, float? sy) {
            var drawScale = ObjectPool<uiDrawScale>.alloc();
            drawScale.sx = sx;
            drawScale.sy = sy;
            return drawScale;
        }

        public override void release() {
            ObjectPool<uiDrawScale>.release(this);
        }

        public float sx;
        public float? sy;
    }

    public class uiDrawRotate : uiStateUpdateDrawCmd {
        public uiDrawRotate() {
        }

        public static uiDrawRotate create(float radians, uiOffset? offset) {
            var drawRotate = ObjectPool<uiDrawRotate>.alloc();
            drawRotate.radians = radians;
            drawRotate.offset = offset;
            return drawRotate;
        }

        public override void release() {
            ObjectPool<uiDrawRotate>.release(this);
        }

        public override void clear() {
            this.offset = null;
        }

        public float radians;
        public uiOffset? offset;
    }

    public class uiDrawSkew : uiStateUpdateDrawCmd {
        public uiDrawSkew() {
        }

        public static uiDrawSkew create(float sx, float sy) {
            var drawSkew = ObjectPool<uiDrawSkew>.alloc();
            drawSkew.sx = sx;
            drawSkew.sy = sy;
            return drawSkew;
        }

        public override void release() {
            ObjectPool<uiDrawSkew>.release(this);
        }

        public float sx;
        public float sy;
    }

    public class uiDrawConcat : uiStateUpdateDrawCmd {
        public uiDrawConcat() {
        }

        public static uiDrawConcat create(uiMatrix3? matrix) {
            var drawConcat = ObjectPool<uiDrawConcat>.alloc();
            drawConcat.matrix = matrix;
            return drawConcat;
        }

        public override void release() {
            ObjectPool<uiDrawConcat>.release(this);
        }

        public override void clear() {
            this.matrix = null;
        }

        public uiMatrix3? matrix;
    }

    public class uiDrawResetMatrix : uiStateUpdateDrawCmd {
        public uiDrawResetMatrix() {
        }

        public static uiDrawResetMatrix create() {
            var drawResetMatrix = ObjectPool<uiDrawResetMatrix>.alloc();
            return drawResetMatrix;
        }

        public override void release() {
            ObjectPool<uiDrawResetMatrix>.release(this);
        }
    }

    public class uiDrawSetMatrix : uiStateUpdateDrawCmd {
        public uiDrawSetMatrix() {
        }

        public static uiDrawSetMatrix create(uiMatrix3? matrix) {
            var drawSetMatrix = ObjectPool<uiDrawSetMatrix>.alloc();
            drawSetMatrix.matrix = matrix;
            return drawSetMatrix;
        }

        public override void release() {
            ObjectPool<uiDrawSetMatrix>.release(this);
        }

        public override void clear() {
            this.matrix = null;
        }

        public uiMatrix3? matrix;
    }

    public class uiDrawClipRect : uiStateUpdateDrawCmd {
        public uiDrawClipRect() {
        }

        public static uiDrawClipRect create(uiRect? rect) {
            var drawClipRect = ObjectPool<uiDrawClipRect>.alloc();
            drawClipRect.rect = rect;
            return drawClipRect;
        }

        public override void release() {
            ObjectPool<uiDrawClipRect>.release(this);
        }

        public override void clear() {
            this.rect = null;
        }

        public uiRect? rect;
    }

    public class uiDrawClipRRect : uiStateUpdateDrawCmd {
        public uiDrawClipRRect() {
        }

        public static uiDrawClipRRect create(RRect rrect) {
            var drawClipRRect = ObjectPool<uiDrawClipRRect>.alloc();
            drawClipRRect.rrect = rrect;
            return drawClipRRect;
        }

        public override void release() {
            ObjectPool<uiDrawClipRRect>.release(this);
        }

        public override void clear() {
            this.rrect = null;
        }

        public RRect rrect;
    }

    public class uiDrawClipPath : uiStateUpdateDrawCmd {
        public uiDrawClipPath() {
        }

        public static uiDrawClipPath create(uiPath path) {
            var drawClipPath = ObjectPool<uiDrawClipPath>.alloc();
            drawClipPath.path = path;
            return drawClipPath;
        }

        public override void release() {
            ObjectPool<uiDrawClipPath>.release(this);
        }

        public override void clear() {
            //ObjectPool<uiPath>.release(this.path);
            uiPathCacheManager.putToCache(this.path);
            this.path = null;
        }

        public uiPath path;
    }

    public class uiDrawPath : uiDrawCmd {
        public uiDrawPath() {
        }

        public static uiDrawPath create(uiPath path, uiPaint paint) {
            var drawPath = ObjectPool<uiDrawPath>.alloc();
            drawPath.path = path;
            drawPath.paint = paint;
            return drawPath;
        }

        public override void release() {
            ObjectPool<uiDrawPath>.release(this);
        }

        public override void clear() {
            //ObjectPool<uiPath>.release(this.path);
            uiPathCacheManager.putToCache(this.path);
            this.path = null;
        }

        public uiPath path;
        public uiPaint paint;

        public override uiRect bounds(float margin) {
            return this.path.getBoundsWithMargin(margin);
        }
    }

    public class uiDrawImage : uiDrawCmd {
        public uiDrawImage() {
        }

        public static uiDrawImage create(Image image, uiOffset? offset, uiPaint paint) {
            var drawImage = ObjectPool<uiDrawImage>.alloc();
            drawImage.image = image;
            drawImage.offset = offset;
            drawImage.paint = paint;
            return drawImage;
        }

        public override void release() {
            ObjectPool<uiDrawImage>.release(this);
        }

        public override void clear() {
            this.image = null;
            this.offset = null;
        }

        public Image image;
        public uiOffset? offset;
        public uiPaint paint;
        
        
        // TODO: Should divide by device pixel ratio here, which is not available as
        //       this DrawCmd is created. This bounds should only used as an upper bound,
        //       assuming that device pixel ratio is always >= 1
        public override uiRect bounds(float margin) {
            return uiRectHelper.fromLTWH(
                this.offset.Value.dx - margin, this.offset.Value.dy - margin,
                this.image.width + 2 * margin,
                this.image.height + 2 * margin);
        }
    }

    public class uiDrawImageRect : uiDrawCmd {
        public uiDrawImageRect() {
        }

        public static uiDrawImageRect create(Image image, uiRect? src, uiRect? dst, uiPaint paint) {
            var drawImageRect = ObjectPool<uiDrawImageRect>.alloc();
            drawImageRect.image = image;
            drawImageRect.src = src;
            drawImageRect.dst = dst;
            drawImageRect.paint = paint;
            return drawImageRect;
        }

        public override void release() {
            ObjectPool<uiDrawImageRect>.release(this);
        }

        public override void clear() {
            this.image = null;
            this.src = null;
            this.dst = null;
        }

        public Image image;
        public uiRect? src;
        public uiRect? dst;
        public uiPaint paint;
        
        public override uiRect bounds(float margin) {
            return uiRectHelper.inflate(this.dst.Value, margin);
        }
    }

    public class uiDrawImageNine : uiDrawCmd {
        public uiDrawImageNine() {
        }

        public static uiDrawImageNine create(Image image, uiRect? src, uiRect? center, uiRect? dst, uiPaint paint) {
            var drawImageNine = ObjectPool<uiDrawImageNine>.alloc();
            drawImageNine.image = image;
            drawImageNine.src = src;
            drawImageNine.center = center;
            drawImageNine.dst = dst;
            drawImageNine.paint = paint;
            return drawImageNine;
        }

        public override void release() {
            ObjectPool<uiDrawImageNine>.release(this);
        }

        public override void clear() {
            this.image = null;
            this.src = null;
            this.center = null;
            this.dst = null;
        }

        public Image image;
        public uiRect? src;
        public uiRect? center;
        public uiRect? dst;
        public uiPaint paint;

        public override uiRect bounds(float margin) {
            return uiRectHelper.fromLTRB(
                this.dst.Value.left + ((this.center.Value.left - this.src.Value.left) * this.src.Value.width) - margin,
                this.dst.Value.top + ((this.center.Value.top - this.src.Value.top) * this.src.Value.height) - margin,
                this.dst.Value.right - ((this.src.Value.right - this.center.Value.right) * this.src.Value.width) + margin,
                this.dst.Value.bottom - ((this.src.Value.bottom - this.center.Value.bottom) * this.src.Value.height) + margin
            );
        }
    }

    public class uiDrawPicture : uiDrawCmd {
        public uiDrawPicture() {
        }

        public static uiDrawPicture create(Picture picture) {
            var drawPicture = ObjectPool<uiDrawPicture>.alloc();
            drawPicture.picture = picture;
            return drawPicture;
        }

        public override void release() {
            ObjectPool<uiDrawPicture>.release(this);
        }

        public override void clear() {
            this.picture = null;
        }

        public Picture picture;
        public override uiRect bounds(float margin) {
            return uiRectHelper.fromRect(this.picture.paintBounds.inflate(margin)).Value;
        }
    }

    public class uiDrawTextBlob : uiDrawCmd {
        public uiDrawTextBlob() {
        }

        public static uiDrawTextBlob create(TextBlob textBlob, uiOffset? offset, uiPaint paint) {
            var drawTextBlob = ObjectPool<uiDrawTextBlob>.alloc();
            drawTextBlob.textBlob = textBlob;
            drawTextBlob.offset = offset;
            drawTextBlob.paint = paint;
            return drawTextBlob;
        }

        public override void release() {
            ObjectPool<uiDrawTextBlob>.release(this);
        }

        public override void clear() {
            this.textBlob = null;
            this.offset = null;
        }

        public TextBlob? textBlob;
        public uiOffset? offset;
        public uiPaint paint;

        public override uiRect bounds(float margin) {
            return uiRectHelper.fromRect(this.textBlob.Value.boundsInText.translate(
                this.offset.Value.dx, this.offset.Value.dy).inflate(margin)).Value;
        }
    }
}