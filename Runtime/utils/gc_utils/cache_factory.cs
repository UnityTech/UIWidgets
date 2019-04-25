using System.Collections.Generic;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.utils {
    
    public static partial class GcCacheHelper {
        public static Rect CreateRect(float left = 0, float top = 0, float right = 0, float bottom = 0) {
            Rect ret = ObjectCache<Rect>.instance.Fetch();
            ret.left = left;
            ret.top = top;
            ret.right = right;
            ret.bottom = bottom;
            return ret;
        }
        
        public static Matrix3 CreateMatrix(Matrix3 other = null) {
            Matrix3 ret = ObjectCache<Matrix3>.instance.Fetch();
            if (other != null) {
                ret.copyFrom(other);
            }
            return ret;
        }
        
        public static DrawSave CreateDrawSaveCmd(){
            return RecyclableObjectCache<DrawSave>.instance.Fetch();
        }
        
        public static DrawSaveLayer CreateDrawSaveLayer(Rect rect, Paint paint) {
            var ret = RecyclableObjectCache<DrawSaveLayer>.instance.Fetch();
            ret.rect = rect;
            ret.paint = paint;
            return ret;
        }
        
        public static DrawRestore CreateDrawRestore() {
            return RecyclableObjectCache<DrawRestore>.instance.Fetch();
        }
        
        public static DrawTranslate CreateDrawTranslate(float dx, float dy) {
            var ret = RecyclableObjectCache<DrawTranslate>.instance.Fetch();
            ret.dx = dx;
            ret.dy = dy;
            return ret;
        }
        
        public static DrawScale CreateDrawScale(float sx, float? sy) {
            var ret = RecyclableObjectCache<DrawScale>.instance.Fetch();
            ret.sx = sx;
            ret.sy = sy;
            return ret;
        }
        
        
        public static DrawRotate CreateDrawRotate(float radians, Offset offset) {
            var ret = RecyclableObjectCache<DrawRotate>.instance.Fetch();
            ret.radians = radians;
            ret.offset = offset;
            return ret;
        }
        
        public static DrawSkew CreateDrawSkew(float sx, float sy) {
            var ret = RecyclableObjectCache<DrawSkew>.instance.Fetch();
            ret.sx = sx;
            ret.sy = sy;
            return ret;
        }
        
        public static DrawConcat CreateDrawConcat(Matrix3 matrix) {
            var ret = RecyclableObjectCache<DrawConcat>.instance.Fetch();
            ret.matrix = matrix;
            return ret;
        }

        public static DrawResetMatrix CreateDrawResetMatrix() {
            return RecyclableObjectCache<DrawResetMatrix>.instance.Fetch();
        }
        
        public static DrawSetMatrix CreateDrawSetMatrix(Matrix3 matrix) {
            var ret = RecyclableObjectCache<DrawSetMatrix>.instance.Fetch();
            ret.matrix = matrix;
            return ret;
        }
        
        public static DrawClipRect CreateDrawClipRect(Rect rect) {
            var ret = RecyclableObjectCache<DrawClipRect>.instance.Fetch();
            ret.rect = rect;
            return ret;
        }
        
        public static DrawClipRRect CreateDrawClipRRect(RRect rrect) {
            var ret = RecyclableObjectCache<DrawClipRRect>.instance.Fetch();
            ret.rrect = rrect;
            return ret;
        }

        public static DrawClipPath CreateDrawClipPath(Path path) {
            var ret = RecyclableObjectCache<DrawClipPath>.instance.Fetch();
            ret.path = path;
            return ret;
        }

        public static DrawPath CreateDrawPath(Path path, Paint paint) {
            var ret = RecyclableObjectCache<DrawPath>.instance.Fetch();
            ret.path = path;
            ret.paint = paint;
            return ret;
        }

        public static DrawImage CreateDrawImage(Image image, Offset offset, Paint paint) {
            var ret = RecyclableObjectCache<DrawImage>.instance.Fetch();
            ret.image = image;
            ret.offset = offset;
            ret.paint = paint;
            return ret;
        }
        
        public static DrawImageRect CreateDrawImageRect(Image image, Rect src, Rect dst, Paint paint) {
            var ret = RecyclableObjectCache<DrawImageRect>.instance.Fetch();
            ret.image = image;
            ret.src = src;
            ret.dst = dst;
            ret.paint = paint;
            return ret;
        }
        
        public static DrawImageNine CreateDrawImageNine(Image image, Rect src, Rect center, Rect dst, Paint paint) {
            var ret = RecyclableObjectCache<DrawImageNine>.instance.Fetch();
            ret.image = image;
            ret.src = src;
            ret.center = center;
            ret.dst = dst;
            ret.paint = paint;
            return ret;
        }

        public static DrawPicture CreateDrawPicture(Picture picture) {
            var ret = RecyclableObjectCache<DrawPicture>.instance.Fetch();
            ret.picture = picture;
            return ret;
        }

        public static DrawTextBlob CreateDrawTextBlob(TextBlob textBlob, Offset offset, Paint paint) {
            var ret = RecyclableObjectCache<DrawTextBlob>.instance.Fetch();
            ret.textBlob = textBlob;
            ret.offset = offset;
            ret.paint = paint;
            return ret;
        }

        public static List<T> CreateList<T>(int len = 0) {
            return ListCache<T>.instance.Fetch(len);
        }

        public static T Create<T>() where T : new() {
            return ObjectCache<T>.instance.Fetch();
        }

        public static T CreateRecyclable<T>() where T : GcRecyclable, new() {
            return RecyclableObjectCache<T>.instance.Fetch();
        }

        public static MaterialPropertyBlock CreateMaterialPropertyBlock() {
            return MaterialPropCache.instance.Fetch();
        }

        public static T CreateFromPool<T>() where T : new() {
            return ObjectPool<T>.instance.Fetch();
        }

        public static void RecycleToPool<T>(T obj) where T : new() {
            ObjectPool<T>.instance.Recycle(obj);
        }
    }
}