using System;

namespace Unity.UIWidgets.ui {
    public interface Canvas {
        void save();

        void saveLayer(Rect bounds, Paint paint = null);

        void restore();

        int getSaveCount();

        void translate(double dx, double dy);

        void scale(double sx, double? sy = null);

        void rotate(double radians, Offset offset = null);

        void skew(double sx, double sy);

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

        void drawRRect(RRect rect, Paint paint);

        void drawDRRect(RRect outer, RRect inner, Paint paint);

        void drawOval(Rect rect, Paint paint);

        void drawCircle(Offset c, double radius, Paint paint);

        void drawArc(Rect rect, double startAngle, double sweepAngle, bool useCenter, Paint paint);

        void drawPath(Path path, Paint paint);

        void drawImage(Image image, Offset offset, Paint paint);

        void drawImageRect(Image image, Rect dst, Paint paint);

        void drawImageRect(Image image, Rect src, Rect dst, Paint paint);

        void drawImageNine(Image image, Rect center, Rect dst, Paint paint);

        void drawImageNine(Image image, Rect src, Rect center, Rect dst, Paint paint);

        void drawPicture(Picture picture);

        void drawTextBlob(TextBlob textBlob, Offset offset, Paint paint);

        void flush();

        void reset();
    }

    public class RecorderCanvas : Canvas {
        public RecorderCanvas(PictureRecorder recorder) {
            this._recorder = recorder;
        }

        readonly PictureRecorder _recorder;

        int _saveCount = 1;

        public void save() {
            this._saveCount++;
            this._recorder.addDrawCmd(new DrawSave {
            });
        }

        public void saveLayer(Rect rect, Paint paint) {
            this._saveCount++;
            this._recorder.addDrawCmd(new DrawSaveLayer {
                rect = rect,
                paint = new Paint(paint),
            });
        }

        public void restore() {
            this._saveCount--;
            this._recorder.addDrawCmd(new DrawRestore {
            });
        }

        public int getSaveCount() {
            return this._saveCount;
        }

        public void translate(double dx, double dy) {
            this._recorder.addDrawCmd(new DrawTranslate {
                dx = dx,
                dy = dy,
            });
        }

        public void scale(double sx, double? sy = null) {
            this._recorder.addDrawCmd(new DrawScale {
                sx = sx,
                sy = sy,
            });
        }

        public void rotate(double radians, Offset offset = null) {
            this._recorder.addDrawCmd(new DrawRotate {
                radians = radians,
                offset = offset,
            });
        }

        public void skew(double sx, double sy) {
            this._recorder.addDrawCmd(new DrawSkew {
                sx = sx,
                sy = sy,
            });
        }

        public void concat(Matrix3 matrix) {
            this._recorder.addDrawCmd(new DrawConcat {
                matrix = matrix,
            });
        }

        public Matrix3 getTotalMatrix() {
            throw new Exception("not available in recorder");
        }

        public void resetMatrix() {
            this._recorder.addDrawCmd(new DrawResetMatrix {
            });
        }

        public void setMatrix(Matrix3 matrix) {
            this._recorder.addDrawCmd(new DrawSetMatrix {
                matrix = matrix,
            });
        }

        public float getDevicePixelRatio() {
            throw new Exception("not available in recorder");
        }

        public void clipRect(Rect rect) {
            this._recorder.addDrawCmd(new DrawClipRect {
                rect = rect,
            });
        }

        public void clipRRect(RRect rrect) {
            this._recorder.addDrawCmd(new DrawClipRRect {
                rrect = rrect,
            });
        }

        public void clipPath(Path path) {
            this._recorder.addDrawCmd(new DrawClipPath {
                path = path,
            });
        }

        public void drawLine(Offset from, Offset to, Paint paint) {
            var path = new Path();
            path.moveTo(from.dx, from.dy);
            path.lineTo(to.dx, to.dy);

            this._recorder.addDrawCmd(new DrawPath {
                path = path,
                paint = new Paint(paint),
            });
        }

        public void drawRect(Rect rect, Paint paint) {
            if (rect.size.isEmpty) {
                return;
            }
            var path = new Path();
            path.addRect(rect);

            this._recorder.addDrawCmd(new DrawPath {
                path = path,
                paint = new Paint(paint),
            });
        }

        public void drawRRect(RRect rrect, Paint paint) {
            var path = new Path();
            path.addRRect(rrect);
            this._recorder.addDrawCmd(new DrawPath {
                path = path,
                paint = new Paint(paint),
            });
        }

        public void drawDRRect(RRect outer, RRect inner, Paint paint) {
            var path = new Path();
            path.addRRect(outer);
            path.addRRect(inner);
            path.winding(PathWinding.clockwise);

            this._recorder.addDrawCmd(new DrawPath {
                path = path,
                paint = new Paint(paint),
            });
        }

        public void drawOval(Rect rect, Paint paint) {
            var w = rect.width / 2;
            var h = rect.height / 2;
            var path = new Path();
            path.addEllipse(rect.left + w, rect.top + h, w, h);

            this._recorder.addDrawCmd(new DrawPath {
                path = path,
                paint = new Paint(paint),
            });
        }

        public void drawCircle(Offset c, double radius, Paint paint) {
            var path = new Path();
            path.addCircle(c.dx, c.dy, radius);

            this._recorder.addDrawCmd(new DrawPath {
                path = path,
                paint = new Paint(paint),
            });
        }

        public void drawArc(Rect rect, double startAngle, double sweepAngle, bool useCenter, Paint paint) {
            var path = new Path();
            //path.(c.dx, c.dy, radius);

            this._recorder.addDrawCmd(new DrawPath {
                path = path,
                paint = new Paint(paint),
            });
        }

        public void drawPath(Path path, Paint paint) {
            this._recorder.addDrawCmd(new DrawPath {
                path = path,
                paint = new Paint(paint),
            });
        }

        public void drawImage(Image image, Offset offset, Paint paint) {
            this._recorder.addDrawCmd(new DrawImage {
                image = image,
                offset = offset,
                paint = new Paint(paint),
            });
        }

        public void drawImageRect(Image image, Rect dst, Paint paint) {
            this._recorder.addDrawCmd(new DrawImageRect {
                image = image,
                dst = dst,
                paint = new Paint(paint),
            });
        }

        public void drawImageRect(Image image, Rect src, Rect dst, Paint paint) {
            this._recorder.addDrawCmd(new DrawImageRect {
                image = image,
                src = src,
                dst = dst,
                paint = new Paint(paint),
            });
        }

        public void drawImageNine(Image image, Rect center, Rect dst, Paint paint) {
            this._recorder.addDrawCmd(new DrawImageNine {
                image = image,
                center = center,
                dst = dst,
                paint = new Paint(paint),
            });
        }

        public void drawImageNine(Image image, Rect src, Rect center, Rect dst, Paint paint) {
            this._recorder.addDrawCmd(new DrawImageNine {
                image = image,
                src = src,
                center = center,
                dst = dst,
                paint = new Paint(paint),
            });
        }

        public void drawPicture(Picture picture) {
            this._recorder.addDrawCmd(new DrawPicture {
                picture = picture,
            });
        }

        public void drawTextBlob(TextBlob textBlob, Offset offset, Paint paint) {
            this._recorder.addDrawCmd(new DrawTextBlob {
                textBlob = textBlob,
                offset = offset,
                paint = new Paint(paint),
            });
        }

        public void flush() {
            throw new Exception("not available in recorder");
        }

        public void reset() {
            throw new Exception("not available in recorder");
        }
    }
}