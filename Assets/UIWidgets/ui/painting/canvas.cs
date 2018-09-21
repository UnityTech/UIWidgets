using UIWidgets.painting;
using UIWidgets.ui.txt;
using UnityEngine;

namespace UIWidgets.ui {
    public interface Canvas {
        void drawPloygon4(Offset[] points, Paint paint);

        void drawRect(Rect rect, BorderWidth borderWidth, BorderRadius borderRadius, Paint paint);

        void drawRectShadow(Rect rect, Paint paint);

        void drawPicture(Picture picture);

        void drawImageRect(Rect src, Rect dst, Paint paint, Image image);

        void drawLine(Offset from, Offset to, Paint paint);

        void concat(Matrix4x4 transform);

        void save();

        void saveLayer(Rect rect, Paint paint);

        void restore();

        void clipRect(Rect rect);

        void clipRRect(RRect rrect);

        void drawTextBlob(TextBlob textBlob, double x, double y);
    }

    public class RecorderCanvas : Canvas {
        public RecorderCanvas(PictureRecorder recorder) {
            this._recorder = recorder;
        }

        private readonly PictureRecorder _recorder;

        public void drawPloygon4(Offset[] points, Paint paint) {
            this._recorder.addDrawCmd(new DrawPloygon4 {
                points = points,
                paint = paint,
            });
        }

        public void drawRect(Rect rect, BorderWidth borderWidth, BorderRadius borderRadius, Paint paint) {
            this._recorder.addDrawCmd(new DrawRect {
                rect = rect,
                borderWidth = borderWidth,
                borderRadius = borderRadius,
                paint = paint,
            });
        }

        public void drawRectShadow(Rect rect, Paint paint) {
            this._recorder.addDrawCmd(new DrawRectShadow {
                rect = rect,
                paint = paint,
            });
        }

        public void drawPicture(Picture picture) {
            this._recorder.addDrawCmd(new DrawPicture {
                picture = picture,
            });
        }

        public void drawImageRect(Rect src, Rect dst, Paint paint, Image image) {
            this._recorder.addDrawCmd(new DrawImageRect
            {
                paint = paint,
                image = image,
                src = src,
                dst = dst,
            });
        }

        public void drawLine(Offset from, Offset to, Paint paint)
        {
            this._recorder.addDrawCmd(new DrawLine
            {
                from = from,
                to = to,
                paint = paint,
            });
        }

        public void concat(Matrix4x4 transform) {
            this._recorder.addDrawCmd(new DrawConcat {
                transform = transform,
            });
        }

        public void save() {
            this._recorder.addDrawCmd(new DrawSave {
            });
        }

        public void saveLayer(Rect rect, Paint paint) {
            this._recorder.addDrawCmd(new DrawSaveLayer {
                rect = rect,
                paint = paint,
            });
        }

        public void restore() {
            this._recorder.addDrawCmd(new DrawRestore {
            });
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

        public void drawTextBlob(TextBlob textBlob, double x, double y)
        {
            this._recorder.addDrawCmd(new DrawTextBlob() {
                textBlob = textBlob,
                x = x,
                y = y,
            });
        }
    }
}
