using System;
using Unity.UIWidgets.flow;
using UnityEngine;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.ui {
    public interface Canvas {
        void save();

        void saveLayer(Rect bounds, Paint paint = null);

        void restore();

        int getSaveCount();

        void translate(float dx, float dy);

        void scale(float sx, float? sy = null);

        void rotate(float radians, Offset offset = null);

        void skew(float sx, float sy);

        void concat(Matrix3 matrix);

        Matrix3 getTotalMatrix();

        void resetMatrix();

        void setMatrix(Matrix3 matrix);

        float getDevicePixelRatio();

        void clipRect(Rect rect);

        void clipRRect(RRect rrect);

        void clipPath(Path path);

        void drawLine(Offset from, Offset to, Paint paint);

        void drawRect(Rect rect, Paint paint);

        void drawShadow(Path path, Color color, float elevation, bool transparentOccluder);

        void drawRRect(RRect rect, Paint paint);

        void drawDRRect(RRect outer, RRect inner, Paint paint);

        void drawOval(Rect rect, Paint paint);

        void drawCircle(Offset c, float radius, Paint paint);

        void drawArc(Rect rect, float startAngle, float sweepAngle, bool useCenter, Paint paint);

        void drawPath(Path path, Paint paint);

        void drawImage(Image image, Offset offset, Paint paint);

        void drawImageRect(Image image, Rect dst, Paint paint);

        void drawImageRect(Image image, Rect src, Rect dst, Paint paint);

        void drawImageNine(Image image, Rect center, Rect dst, Paint paint);

        void drawImageNine(Image image, Rect src, Rect center, Rect dst, Paint paint);

        void drawPicture(Picture picture);

        void drawTextBlob(TextBlob textBlob, Offset offset, Paint paint);

        void drawParagraph(Paragraph paragraph, Offset offset);
        void flush();

        void reset();
    }

    public class RecorderCanvas : Canvas {
        public RecorderCanvas(PictureRecorder recorder) {
            this._recorder = recorder;
        }

        protected readonly PictureRecorder _recorder;

        int _saveCount = 1;

        public void save() {
            PathOptimizer.cmdNum++;
            this._saveCount++;
            this._recorder.addDrawCmd(DrawSave.createNew(
            ));
        }

        public void saveLayer(Rect rect, Paint paint) {
            PathOptimizer.cmdNum++;
            this._saveCount++;
            this._recorder.addDrawCmd(DrawSaveLayer.createNew(
                rect : rect,
                paint : new Paint(paint)
            ));
        }

        public void restore() {
            PathOptimizer.cmdNum++;
            this._saveCount--;
            this._recorder.addDrawCmd(DrawRestore.createNew(
            ));
        }

        public int getSaveCount() {
            return this._saveCount;
        }

        public void translate(float dx, float dy) {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawTranslate.createNew(
                dx : dx,
                dy : dy
            ));
        }

        public void scale(float sx, float? sy = null) {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawScale.createNew(
                sx : sx,
                sy : sy
            ));
        }

        public void rotate(float radians, Offset offset = null) {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawRotate.createNew(
                radians : radians,
                offset : offset
            ));
        }

        public void skew(float sx, float sy) {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawSkew.createNew(
                sx : sx,
                sy : sy
            ));
        }

        public void concat(Matrix3 matrix) {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawConcat.createNew(
                matrix = matrix
            ));
        }

        public Matrix3 getTotalMatrix() {
            return this._recorder.getTotalMatrix();
        }

        public void resetMatrix() {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawResetMatrix.createNew(
            ));
        }

        public void setMatrix(Matrix3 matrix) {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawSetMatrix.createNew(
                matrix = matrix
            ));
        }

        public virtual float getDevicePixelRatio() {
            throw new Exception("not available in recorder");
        }

        public void clipRect(Rect rect) {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawClipRect.createNew(
                rect : rect
            ));
        }

        public void clipRRect(RRect rrect) {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawClipRRect.createNew(
                rrect : rrect
            ));
        }

        public void clipPath(Path path) {
            Path newPath = new Path();
            newPath.copyFrom(path);
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawClipPath.createNew(
                path : newPath
            ));
        }

        public void drawLine(Offset from, Offset to, Paint paint) {
            var path = new Path();
            path.moveTo(from.dx, from.dy);
            path.lineTo(to.dx, to.dy);
            PathOptimizer.cmdNum++;

            this._recorder.addDrawCmd(DrawPath.createNew(
                path : path,
                paint : new Paint(paint)
            ));
        }

        public void drawShadow(Path path, Color color, float elevation, bool transparentOccluder) {
            float dpr = Window.instance.devicePixelRatio;
            Path newPath = new Path();
            newPath.copyFrom(path);
            PathOptimizer.cmdNum++;
            PhysicalShapeLayer.drawShadow(this, newPath, color, elevation, transparentOccluder, dpr);
        }

        public void drawRect(Rect rect, Paint paint) {
            if (rect.size.isEmpty) {
                return;
            }

            var path = new Path();
            path.addRect(rect);

            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawPath.createNew(
                path : path,
                paint : new Paint(paint)
            ));
        }

        public void drawRRect(RRect rrect, Paint paint) {
            var path = new Path();
            path.addRRect(rrect);
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawPath.createNew(
                path : path,
                paint : new Paint(paint)
            ));
        }

        public void drawDRRect(RRect outer, RRect inner, Paint paint) {
            var path = new Path();
            path.addRRect(outer);
            path.addRRect(inner);
            path.winding(PathWinding.clockwise);

            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawPath.createNew(
                path : path,
                paint : new Paint(paint)
            ));
        }

        public void drawOval(Rect rect, Paint paint) {
            var w = rect.width / 2;
            var h = rect.height / 2;
            var path = new Path();
            path.addEllipse(rect.left + w, rect.top + h, w, h);
            PathOptimizer.cmdNum++;

            this._recorder.addDrawCmd(DrawPath.createNew(
                path : path,
                paint : new Paint(paint)
            ));
        }

        public void drawCircle(Offset c, float radius, Paint paint) {
            var path = new Path();
            path.addCircle(c.dx, c.dy, radius);
            PathOptimizer.cmdNum++;

            this._recorder.addDrawCmd(DrawPath.createNew(
                path : path,
                paint : new Paint(paint)
            ));
        }

        public void drawArc(Rect rect, float startAngle, float sweepAngle, bool useCenter, Paint paint) {
            var path = new Path();

            if (useCenter) {
                var center = rect.center;
                path.moveTo(center.dx, center.dy);                
            }

            bool forceMoveTo = !useCenter;
            while (sweepAngle <= -Mathf.PI * 2) {
                path.addArc(rect, startAngle, -Mathf.PI, forceMoveTo);
                startAngle -= Mathf.PI;
                path.addArc(rect, startAngle, -Mathf.PI, false);
                startAngle -= Mathf.PI;
                forceMoveTo = false;
                sweepAngle += Mathf.PI * 2;
            }
            
            while (sweepAngle >= Mathf.PI * 2) {
                path.addArc(rect, startAngle, Mathf.PI, forceMoveTo);
                startAngle += Mathf.PI;
                path.addArc(rect, startAngle, Mathf.PI, false);
                startAngle += Mathf.PI;
                forceMoveTo = false;
                sweepAngle -= Mathf.PI * 2;
            }

            path.addArc(rect, startAngle, sweepAngle, forceMoveTo);
            if (useCenter) {
                path.close();
            }

            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawPath.createNew(
                path : path,
                paint : new Paint(paint)
            ));
        }

        public void drawPath(Path path, Paint paint) {
            Path newPath = new Path();
            newPath.copyFrom(path);
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawPath.createNew(
                path : newPath,
                paint : new Paint(paint)
            ));
        }

        public void drawImage(Image image, Offset offset, Paint paint) {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawImage.createNew(
                image : image,
                offset : offset,
                paint : new Paint(paint)
            ));
        }

        public void drawImageRect(Image image, Rect dst, Paint paint) {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawImageRect.createNew(
                image : image,
                src : null,
                dst : dst,
                paint : new Paint(paint)
            ));
        }

        public void drawImageRect(Image image, Rect src, Rect dst, Paint paint) {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawImageRect.createNew(
                image : image,
                src : src,
                dst : dst,
                paint : new Paint(paint)
            ));
        }

        public void drawImageNine(Image image, Rect center, Rect dst, Paint paint) {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawImageNine.createNew(
                image : image,
                src : null,
                center : center,
                dst : dst,
                paint : new Paint(paint)
            ));
        }

        public void drawImageNine(Image image, Rect src, Rect center, Rect dst, Paint paint) {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawImageNine.createNew(
                image : image,
                src : src,
                center : center,
                dst : dst,
                paint : new Paint(paint)
            ));
        }

        public void drawPicture(Picture picture) {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawPicture.createNew(
                picture : picture
            ));
        }

        public void drawTextBlob(TextBlob textBlob, Offset offset, Paint paint) {
            PathOptimizer.cmdNum++;
            this._recorder.addDrawCmd(DrawTextBlob.createNew(
                textBlob : textBlob,
                offset : offset,
                paint : new Paint(paint)
            ));
        }
        
        public void drawParagraph(Paragraph paragraph, Offset offset) {
            D.assert(paragraph != null);
            D.assert(PaintingUtils._offsetIsValid(offset));
            paragraph.paint(this, offset);
        }

        public virtual void flush() {
            throw new Exception("not available in recorder");
        }

        public void reset() {
            this._recorder.reset();
        }
    }
}