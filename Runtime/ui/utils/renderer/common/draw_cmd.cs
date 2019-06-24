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

        public static uiDrawSaveLayer create(Rect rect, Paint paint) {
            var drawSaveLayer = ItemPoolManager.alloc<uiDrawSaveLayer>();
            drawSaveLayer.rect = rect;
            drawSaveLayer.paint = paint;
            return drawSaveLayer;
        }
        
        public Rect rect;
        public Paint paint;
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

        public static uiDrawRotate create(float radians, Offset offset) {
            var drawRotate = ItemPoolManager.alloc<uiDrawRotate>();
            drawRotate.radians = radians;
            drawRotate.offset = offset;
            return drawRotate;
        }
        
        public float radians;
        public Offset offset;
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

        public static uiDrawConcat create(Matrix3 matrix) {
            var drawConcat = ItemPoolManager.alloc<uiDrawConcat>();
            drawConcat.matrix = matrix;
            return drawConcat;
        }
        
        public Matrix3 matrix;
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

        public static uiDrawSetMatrix create(Matrix3 matrix) {
            var drawSetMatrix = ItemPoolManager.alloc<uiDrawSetMatrix>();
            drawSetMatrix.matrix = matrix;
            return drawSetMatrix;
        }
        
        public Matrix3 matrix;
    }

    public class uiDrawClipRect : uiDrawCmd {
        public uiDrawClipRect() {
            
        }

        public static uiDrawClipRect create(Rect rect) {
            var drawClipRect = ItemPoolManager.alloc<uiDrawClipRect>();
            drawClipRect.rect = rect;
            return drawClipRect;
        }
        
        public Rect rect;
    }

    public class uiDrawClipRRect : uiDrawCmd {
        public uiDrawClipRRect() {
            
        }

        public static uiDrawClipRRect create(RRect rrect) {
            var drawClipRRect = ItemPoolManager.alloc<uiDrawClipRRect>();
            drawClipRRect.rrect = rrect;
            return drawClipRRect;
        }
        
        public RRect rrect;
    }

    public class uiDrawClipPath : uiDrawCmd {
        public uiDrawClipPath() {
            
        }

        public static uiDrawClipPath create(Path path) {
            var drawClipPath = ItemPoolManager.alloc<uiDrawClipPath>();
            drawClipPath.path = path;
            return drawClipPath;
        }
        
        public Path path;
    }

    public class uiDrawPath : uiDrawCmd {
        public uiDrawPath() {
            
        }

        public static uiDrawPath create(Path path, Paint paint) {
            var drawPath = ItemPoolManager.alloc<uiDrawPath>();
            drawPath.path = path;
            drawPath.paint = paint;
            return drawPath;
        }
        
        public Path path;
        public Paint paint;
    }

    public class uiDrawImage : uiDrawCmd {
        public uiDrawImage() {
            
        }

        public static uiDrawImage create(Image image, Offset offset, Paint paint) {
            var drawImage = ItemPoolManager.alloc<uiDrawImage>();
            drawImage.image = image;
            drawImage.offset = offset;
            drawImage.paint = paint;
            return drawImage;
        }
        
        public Image image;
        public Offset offset;
        public Paint paint;
    }

    public class uiDrawImageRect : uiDrawCmd {
        public uiDrawImageRect() {
            
        }

        public static uiDrawImageRect create(Image image, Rect src, Rect dst, Paint paint) {
            var drawImageRect = ItemPoolManager.alloc<uiDrawImageRect>();
            drawImageRect.image = image;
            drawImageRect.src = src;
            drawImageRect.dst = dst;
            drawImageRect.paint = paint;
            return drawImageRect;
        }
        
        public Image image;
        public Rect src;
        public Rect dst;
        public Paint paint;
    }

    public class uiDrawImageNine : uiDrawCmd {
        public uiDrawImageNine() {
            
        }

        public static uiDrawImageNine create(Image image, Rect src, Rect center, Rect dst, Paint paint) {
            var drawImageNine = ItemPoolManager.alloc<uiDrawImageNine>();
            drawImageNine.image = image;
            drawImageNine.src = src;
            drawImageNine.center = center;
            drawImageNine.dst = dst;
            drawImageNine.paint = paint;
            return drawImageNine;
        }
        
        public Image image;
        public Rect src;
        public Rect center;
        public Rect dst;
        public Paint paint;
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

        public static uiDrawTextBlob create(TextBlob textBlob, Offset offset, Paint paint) {
            var drawTextBlob = ItemPoolManager.alloc<uiDrawTextBlob>();
            drawTextBlob.textBlob = textBlob;
            drawTextBlob.offset = offset;
            drawTextBlob.paint = paint;
            return drawTextBlob;
        }

        public override void clear() {
            this.textBlob = null;
        }

        public TextBlob textBlob;
        public Offset offset;
        public Paint paint;
    }
}