using System.Runtime.CompilerServices;

namespace Unity.UIWidgets.ui {
    public abstract class uiDrawCmd : PoolItem {
    }

    public class uiDrawSave : uiDrawCmd {
        public uiDrawSave() {
        }

        public static uiDrawSave create() {
            var drawSave = ItemPoolManager.alloc<uiDrawSave>();
            return drawSave;
        }
    }

    public class uiDrawSaveLayer : uiDrawCmd {
        public uiDrawSaveLayer() {
        }

        public static uiDrawSaveLayer create(uiRect? rect, uiPaint paint) {
            var drawSaveLayer = ItemPoolManager.alloc<uiDrawSaveLayer>();
            drawSaveLayer.rect = rect;
            drawSaveLayer.paint = paint;
            return drawSaveLayer;
        }

        public override void clear() {
            this.rect = null;
        }

        public uiRect? rect;
        public uiPaint paint;
    }

    public class uiDrawRestore : uiDrawCmd {
        public uiDrawRestore() {
        }

        public static uiDrawRestore create() {
            var drawRestore = ItemPoolManager.alloc<uiDrawRestore>();
            return drawRestore;
        }
    }

    public class uiDrawTranslate : uiDrawCmd {
        public uiDrawTranslate() {
        }

        public static uiDrawTranslate create(float dx, float dy) {
            var drawTranslate = ItemPoolManager.alloc<uiDrawTranslate>();
            drawTranslate.dx = dx;
            drawTranslate.dy = dy;
            return drawTranslate;
        }
        
        public float dx;
        public float dy;
    }

    public class uiDrawScale : uiDrawCmd {
        public uiDrawScale() {
            
        }

        public static uiDrawScale create(float sx, float? sy) {
            var drawScale = ItemPoolManager.alloc<uiDrawScale>();
            drawScale.sx = sx;
            drawScale.sy = sy;
            return drawScale;
        }
        
        public float sx;
        public float? sy;
    }

    public class uiDrawRotate : uiDrawCmd {
        public uiDrawRotate() {
            
        }

        public static uiDrawRotate create(float radians, uiOffset? offset) {
            var drawRotate = ItemPoolManager.alloc<uiDrawRotate>();
            drawRotate.radians = radians;
            drawRotate.offset = offset;
            return drawRotate;
        }
        
        public override void clear() {
            this.offset = null;
        }
        
        public float radians;
        public uiOffset? offset;
    }

    public class uiDrawSkew : uiDrawCmd {
        public uiDrawSkew() {
            
        }

        public static uiDrawSkew create(float sx, float sy) {
            var drawSkew = ItemPoolManager.alloc<uiDrawSkew>();
            drawSkew.sx = sx;
            drawSkew.sy = sy;
            return drawSkew;
        }
        
        public float sx;
        public float sy;
    }

    public class uiDrawConcat : uiDrawCmd {
        public uiDrawConcat() {
            
        }

        public static uiDrawConcat create(uiMatrix3? matrix) {
            var drawConcat = ItemPoolManager.alloc<uiDrawConcat>();
            drawConcat.matrix = matrix;
            return drawConcat;
        }
        
        public override void clear() {
            this.matrix = null;
        }
        
        public uiMatrix3? matrix;
    }

    public class uiDrawResetMatrix : uiDrawCmd {
        public uiDrawResetMatrix() {
            
        }

        public static uiDrawResetMatrix create() {
            var drawResetMatrix = ItemPoolManager.alloc<uiDrawResetMatrix>();
            return drawResetMatrix;
        }
    }

    public class uiDrawSetMatrix : uiDrawCmd {
        public uiDrawSetMatrix() {
            
        }

        public static uiDrawSetMatrix create(uiMatrix3? matrix) {
            var drawSetMatrix = ItemPoolManager.alloc<uiDrawSetMatrix>();
            drawSetMatrix.matrix = matrix;
            return drawSetMatrix;
        }
        
        public override void clear() {
            this.matrix = null;
        }
        
        public uiMatrix3? matrix;
    }

    public class uiDrawClipRect : uiDrawCmd {
        public uiDrawClipRect() {
            
        }

        public static uiDrawClipRect create(uiRect? rect) {
            var drawClipRect = ItemPoolManager.alloc<uiDrawClipRect>();
            drawClipRect.rect = rect;
            return drawClipRect;
        }
        
        public override void clear() {
            this.rect = null;
        }
        
        public uiRect? rect;
    }

    public class uiDrawClipRRect : uiDrawCmd {
        public uiDrawClipRRect() {
            
        }

        public static uiDrawClipRRect create(RRect rrect) {
            var drawClipRRect = ItemPoolManager.alloc<uiDrawClipRRect>();
            drawClipRRect.rrect = rrect;
            return drawClipRRect;
        }
        
        public override void clear() {
            this.rrect = null;
        }
        
        public RRect rrect;
    }

    public class uiDrawClipPath : uiDrawCmd {
        public uiDrawClipPath() {
            
        }

        public static uiDrawClipPath create(uiPath path) {
            var drawClipPath = ItemPoolManager.alloc<uiDrawClipPath>();
            drawClipPath.path = path;
            return drawClipPath;
        }

        public override void clear() {
            this.path.dispose();
            this.path = null;
        }

        public uiPath path;
    }

    public class uiDrawPath : uiDrawCmd {
        public uiDrawPath() {
            
        }

        public static uiDrawPath create(uiPath path, uiPaint paint) {
            var drawPath = ItemPoolManager.alloc<uiDrawPath>();
            drawPath.path = path;
            drawPath.paint = paint;
            return drawPath;
        }

        public override void clear() {
            this.path.dispose();
            this.path = null;
        }

        public uiPath path;
        public uiPaint paint;
    }

    public class uiDrawImage : uiDrawCmd {
        public uiDrawImage() {
            
        }

        public static uiDrawImage create(Image image, uiOffset? offset, uiPaint paint) {
            var drawImage = ItemPoolManager.alloc<uiDrawImage>();
            drawImage.image = image;
            drawImage.offset = offset;
            drawImage.paint = paint;
            return drawImage;
        }

        public override void clear() {
            this.image = null;
            this.offset = null;
        }
        
        public Image image;
        public uiOffset? offset;
        public uiPaint paint;
    }

    public class uiDrawImageRect : uiDrawCmd {
        public uiDrawImageRect() {
            
        }

        public static uiDrawImageRect create(Image image, uiRect? src, uiRect? dst, uiPaint paint) {
            var drawImageRect = ItemPoolManager.alloc<uiDrawImageRect>();
            drawImageRect.image = image;
            drawImageRect.src = src;
            drawImageRect.dst = dst;
            drawImageRect.paint = paint;
            return drawImageRect;
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
    }

    public class uiDrawImageNine : uiDrawCmd {
        public uiDrawImageNine() {
            
        }

        public static uiDrawImageNine create(Image image, uiRect? src, uiRect? center, uiRect? dst, uiPaint paint) {
            var drawImageNine = ItemPoolManager.alloc<uiDrawImageNine>();
            drawImageNine.image = image;
            drawImageNine.src = src;
            drawImageNine.center = center;
            drawImageNine.dst = dst;
            drawImageNine.paint = paint;
            return drawImageNine;
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
    }

    public class uiDrawPicture : uiDrawCmd {
        public uiDrawPicture() {
            
        }

        public static uiDrawPicture create(Picture picture) {
            var drawPicture = ItemPoolManager.alloc<uiDrawPicture>();
            drawPicture.picture = picture;
            return drawPicture;
        }

        public override void clear() {
            this.picture = null;
        }

        public Picture picture;
    }

    public class uiDrawTextBlob : uiDrawCmd {
        public uiDrawTextBlob() {
            
        }

        public static uiDrawTextBlob create(TextBlob textBlob, uiOffset? offset, uiPaint paint) {
            var drawTextBlob = ItemPoolManager.alloc<uiDrawTextBlob>();
            drawTextBlob.textBlob = textBlob;
            drawTextBlob.offset = offset;
            drawTextBlob.paint = paint;
            return drawTextBlob;
        }

        public override void clear() {
            this.textBlob = null;
            this.offset = null;
        }

        public TextBlob textBlob;
        public uiOffset? offset;
        public uiPaint paint;
    }
}