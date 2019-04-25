using System.Collections.Generic;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.utils {
    
    public static partial class GcCacheHelper {
        public static Rect CreateRect(float left = 0, float top = 0, float right = 0, float bottom = 0) {
            Rect ret = SimpleFlash<Rect>.instance.fetch();
            ret.left = left;
            ret.top = top;
            ret.right = right;
            ret.bottom = bottom;
            return ret;
        }
        
        public static Matrix3 CreateMatrix(Matrix3 other = null) {
            Matrix3 ret = SimpleFlash<Matrix3>.instance.fetch();
            if (other != null) {
                ret.copyFrom(other);
            }
            return ret;
        }
        
        public static DrawSave CreateDrawSaveCmd(){
            return ClearableSimpleFlash<DrawSave>.instance.fetch();
        }
        
        public static DrawSaveLayer CreateDrawSaveLayer(Rect rect, Paint paint) {
            var ret = ClearableSimpleFlash<DrawSaveLayer>.instance.fetch();
            ret.rect = rect;
            ret.paint = paint;
            return ret;
        }
        
        public static DrawRestore CreateDrawRestore() {
            return ClearableSimpleFlash<DrawRestore>.instance.fetch();
        }
        
        public static DrawTranslate CreateDrawTranslate(float dx, float dy) {
            var ret = ClearableSimpleFlash<DrawTranslate>.instance.fetch();
            ret.dx = dx;
            ret.dy = dy;
            return ret;
        }
        
        public static DrawScale CreateDrawScale(float sx, float? sy) {
            var ret = ClearableSimpleFlash<DrawScale>.instance.fetch();
            ret.sx = sx;
            ret.sy = sy;
            return ret;
        }
        
        
        public static DrawRotate CreateDrawRotate(float radians, Offset offset) {
            var ret = ClearableSimpleFlash<DrawRotate>.instance.fetch();
            ret.radians = radians;
            ret.offset = offset;
            return ret;
        }
        
        public static DrawSkew CreateDrawSkew(float sx, float sy) {
            var ret = ClearableSimpleFlash<DrawSkew>.instance.fetch();
            ret.sx = sx;
            ret.sy = sy;
            return ret;
        }
        
        public static DrawConcat CreateDrawConcat(Matrix3 matrix) {
            var ret = ClearableSimpleFlash<DrawConcat>.instance.fetch();
            ret.matrix = matrix;
            return ret;
        }

        public static DrawResetMatrix CreateDrawResetMatrix() {
            return ClearableSimpleFlash<DrawResetMatrix>.instance.fetch();
        }
        
        public static DrawSetMatrix CreateDrawSetMatrix(Matrix3 matrix) {
            var ret = ClearableSimpleFlash<DrawSetMatrix>.instance.fetch();
            ret.matrix = matrix;
            return ret;
        }
        
        public static DrawClipRect CreateDrawClipRect(Rect rect) {
            var ret = ClearableSimpleFlash<DrawClipRect>.instance.fetch();
            ret.rect = rect;
            return ret;
        }
        
        public static DrawClipRRect CreateDrawClipRRect(RRect rrect) {
            var ret = ClearableSimpleFlash<DrawClipRRect>.instance.fetch();
            ret.rrect = rrect;
            return ret;
        }

        public static DrawClipPath CreateDrawClipPath(Path path) {
            var ret = ClearableSimpleFlash<DrawClipPath>.instance.fetch();
            ret.path = path;
            return ret;
        }

        public static DrawPath CreateDrawPath(Path path, Paint paint) {
            var ret = ClearableSimpleFlash<DrawPath>.instance.fetch();
            ret.path = path;
            ret.paint = paint;
            return ret;
        }

        public static DrawImage CreateDrawImage(Image image, Offset offset, Paint paint) {
            var ret = ClearableSimpleFlash<DrawImage>.instance.fetch();
            ret.image = image;
            ret.offset = offset;
            ret.paint = paint;
            return ret;
        }
        
        public static DrawImageRect CreateDrawImageRect(Image image, Rect src, Rect dst, Paint paint) {
            var ret = ClearableSimpleFlash<DrawImageRect>.instance.fetch();
            ret.image = image;
            ret.src = src;
            ret.dst = dst;
            ret.paint = paint;
            return ret;
        }
        
        public static DrawImageNine CreateDrawImageNine(Image image, Rect src, Rect center, Rect dst, Paint paint) {
            var ret = ClearableSimpleFlash<DrawImageNine>.instance.fetch();
            ret.image = image;
            ret.src = src;
            ret.center = center;
            ret.dst = dst;
            ret.paint = paint;
            return ret;
        }

        public static DrawPicture CreateDrawPicture(Picture picture) {
            var ret = ClearableSimpleFlash<DrawPicture>.instance.fetch();
            ret.picture = picture;
            return ret;
        }

        public static DrawTextBlob CreateDrawTextBlob(TextBlob textBlob, Offset offset, Paint paint) {
            var ret = ClearableSimpleFlash<DrawTextBlob>.instance.fetch();
            ret.textBlob = textBlob;
            ret.offset = offset;
            ret.paint = paint;
            return ret;
        }

        public static List<T> CreateList<T>() {
            return Flash<T>.instance.fetch();
        }

        public static T Create<T>() where T : new() {
            return SimpleFlash<T>.instance.fetch();
        }

        public static T CreateRecyclable<T>() where T : GcRecyclable, new() {
            return ClearableSimpleFlash<T>.instance.fetch();
        }

        public static MaterialPropertyBlock CreateMaterialPropertyBlock() {
            return ClearableMaterialPropFlash.instance.fetch();
        }
    }
}