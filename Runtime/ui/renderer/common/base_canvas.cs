using System;
using Unity.UIWidgets.flow;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class uiRecorderCanvas : Canvas {
        public uiRecorderCanvas(uiPictureRecorder recorder) {
            this._recorder = recorder;
        }

        protected readonly uiPictureRecorder _recorder;

        int _saveCount = 1;

        public void save() {
            this._saveCount++;
            this._recorder.addDrawCmd(uiDrawSave.create());
        }

        public void saveLayer(Rect rect, Paint paint) {
            this._saveCount++;
            this._recorder.addDrawCmd(uiDrawSaveLayer.create(
                rect: uiRectHelper.fromRect(rect),
                paint: uiPaint.fromPaint(paint)
            ));
        }

        public void restore() {
            this._saveCount--;
            this._recorder.addDrawCmd(uiDrawRestore.create());
        }

        public int getSaveCount() {
            return this._saveCount;
        }

        public void translate(float dx, float dy) {
            this._recorder.addDrawCmd(uiDrawTranslate.create(
                dx: dx,
                dy: dy
            ));
        }

        public void scale(float sx, float? sy = null) {
            this._recorder.addDrawCmd(uiDrawScale.create(
                sx: sx,
                sy: sy
            ));
        }

        public void rotate(float radians, Offset offset = null) {
            this._recorder.addDrawCmd(uiDrawRotate.create(
                radians: radians,
                offset: uiOffset.fromOffset(offset)
            ));
        }

        public void skew(float sx, float sy) {
            this._recorder.addDrawCmd(uiDrawSkew.create(
                sx: sx,
                sy: sy
            ));
        }

        public void concat(Matrix3 matrix) {
            this._recorder.addDrawCmd(uiDrawConcat.create(
                matrix: uiMatrix3.fromMatrix3(matrix)
            ));
        }

        readonly Matrix3 _totalMatrix = Matrix3.I();

        public Matrix3 getTotalMatrix() {
            var localMatrix = this._recorder.getTotalMatrix();
            this._totalMatrix.setAll(localMatrix.kMScaleX, localMatrix.kMSkewX, localMatrix.kMTransX,
                localMatrix.kMSkewY, localMatrix.kMScaleY, localMatrix.kMTransY,
                localMatrix.kMPersp0, localMatrix.kMPersp1, localMatrix.kMPersp2);

            return this._totalMatrix;
        }

        public void resetMatrix() {
            this._recorder.addDrawCmd(uiDrawResetMatrix.create(
            ));
        }

        public void setMatrix(Matrix3 matrix) {
            this._recorder.addDrawCmd(uiDrawSetMatrix.create(
                matrix: uiMatrix3.fromMatrix3(matrix)
            ));
        }

        public virtual float getDevicePixelRatio() {
            throw new Exception("not available in recorder");
        }

        public void clipRect(Rect rect) {
            this._recorder.addDrawCmd(uiDrawClipRect.create(
                rect: uiRectHelper.fromRect(rect)
            ));
        }

        public void clipRRect(RRect rrect) {
            this._recorder.addDrawCmd(uiDrawClipRRect.create(
                rrect: rrect
            ));
        }

        public void clipPath(Path path) {
            this._recorder.addDrawCmd(uiDrawClipPath.create(
                path: uiPath.fromPath(path)
            ));
        }

        public void drawLine(Offset from, Offset to, Paint paint) {
            var path = uiPath.create();
            path.moveTo(from.dx, from.dy);
            path.lineTo(to.dx, to.dy);

            this._recorder.addDrawCmd(uiDrawPath.create(
                path: path,
                paint: uiPaint.fromPaint(paint)
            ));
        }

        public void drawShadow(Path path, Color color, float elevation, bool transparentOccluder) {
            float dpr = Window.instance.devicePixelRatio;
            PhysicalShapeLayer.drawShadow(this, path, color, elevation, transparentOccluder, dpr);
        }

        public void drawRect(Rect rect, Paint paint) {
            if (rect.size.isEmpty) {
                return;
            }

            var path = uiPath.create();
            path.addRect(rect);

            this._recorder.addDrawCmd(uiDrawPath.create(
                path: path,
                paint: uiPaint.fromPaint(paint)
            ));
        }

        public void drawRRect(RRect rrect, Paint paint) {
            var path = uiPath.create();
            path.addRRect(rrect);
            this._recorder.addDrawCmd(uiDrawPath.create(
                path: path,
                paint: uiPaint.fromPaint(paint)
            ));
        }

        public void drawDRRect(RRect outer, RRect inner, Paint paint) {
            var path = uiPath.create();
            path.addRRect(outer);
            path.addRRect(inner);
            path.winding(PathWinding.clockwise);

            this._recorder.addDrawCmd(uiDrawPath.create(
                path: path,
                paint: uiPaint.fromPaint(paint)
            ));
        }

        public void drawOval(Rect rect, Paint paint) {
            var w = rect.width / 2;
            var h = rect.height / 2;
            var path = uiPath.create();
            path.addEllipse(rect.left + w, rect.top + h, w, h);

            this._recorder.addDrawCmd(uiDrawPath.create(
                path: path,
                paint: uiPaint.fromPaint(paint)
            ));
        }

        public void drawCircle(Offset c, float radius, Paint paint) {
            var path = uiPath.create();
            path.addCircle(c.dx, c.dy, radius);

            this._recorder.addDrawCmd(uiDrawPath.create(
                path: path,
                paint: uiPaint.fromPaint(paint)
            ));
        }

        public void drawArc(Rect rect, float startAngle, float sweepAngle, bool useCenter, Paint paint) {
            var path = uiPath.create();

            if (useCenter) {
                var center = rect.center;
                path.moveTo(center.dx, center.dy);
            }

            bool forceMoveTo = !useCenter;
            while (sweepAngle <= -Mathf.PI * 2) {
                path.arcTo(rect, startAngle, -Mathf.PI, forceMoveTo);
                startAngle -= Mathf.PI;
                path.arcTo(rect, startAngle, -Mathf.PI, false);
                startAngle -= Mathf.PI;
                forceMoveTo = false;
                sweepAngle += Mathf.PI * 2;
            }

            while (sweepAngle >= Mathf.PI * 2) {
                path.arcTo(rect, startAngle, Mathf.PI, forceMoveTo);
                startAngle += Mathf.PI;
                path.arcTo(rect, startAngle, Mathf.PI, false);
                startAngle += Mathf.PI;
                forceMoveTo = false;
                sweepAngle -= Mathf.PI * 2;
            }

            path.arcTo(rect, startAngle, sweepAngle, forceMoveTo);
            if (useCenter) {
                path.close();
            }

            this._recorder.addDrawCmd(uiDrawPath.create(
                path: path,
                paint: uiPaint.fromPaint(paint)
            ));
        }

        public void drawPath(Path path, Paint paint) {
            this._recorder.addDrawCmd(uiDrawPath.create(
                path: uiPath.fromPath(path),
                paint: uiPaint.fromPaint(paint)
            ));
        }

        public void drawImage(Image image, Offset offset, Paint paint) {
            this._recorder.addDrawCmd(uiDrawImage.create(
                image: image,
                offset: uiOffset.fromOffset(offset),
                paint: uiPaint.fromPaint(paint)
            ));
        }

        public void drawImageRect(Image image, Rect dst, Paint paint) {
            this._recorder.addDrawCmd(uiDrawImageRect.create(
                image: image,
                src: null,
                dst: uiRectHelper.fromRect(dst),
                paint: uiPaint.fromPaint(paint)
            ));
        }

        public void drawImageRect(Image image, Rect src, Rect dst, Paint paint) {
            this._recorder.addDrawCmd(uiDrawImageRect.create(
                image: image,
                src: uiRectHelper.fromRect(src),
                dst: uiRectHelper.fromRect(dst),
                paint: uiPaint.fromPaint(paint)
            ));
        }

        public void drawImageNine(Image image, Rect center, Rect dst, Paint paint) {
            this._recorder.addDrawCmd(uiDrawImageNine.create(
                image: image,
                src: null,
                center: uiRectHelper.fromRect(center),
                dst: uiRectHelper.fromRect(dst),
                paint: uiPaint.fromPaint(paint)
            ));
        }

        public void drawImageNine(Image image, Rect src, Rect center, Rect dst, Paint paint) {
            this._recorder.addDrawCmd(uiDrawImageNine.create(
                image: image,
                src: uiRectHelper.fromRect(src),
                center: uiRectHelper.fromRect(center),
                dst: uiRectHelper.fromRect(dst),
                paint: uiPaint.fromPaint(paint)
            ));
        }

        public void drawPicture(Picture picture) {
            this._recorder.addDrawCmd(uiDrawPicture.create(
                picture: picture
            ));
        }

        public void drawTextBlob(TextBlob textBlob, Offset offset, Paint paint) {
            this._recorder.addDrawCmd(uiDrawTextBlob.create(
                textBlob: textBlob,
                offset: uiOffset.fromOffset(offset),
                paint: uiPaint.fromPaint(paint)
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