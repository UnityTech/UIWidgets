using System;
using UIWidgets.foundation;
using UIWidgets.painting;
using UIWidgets.ui.txt;
using UnityEngine;

namespace UIWidgets.ui {
    public interface Canvas {
        void drawPloygon4(Offset[] points, Paint paint = null);

        void drawRect(Rect rect, BorderWidth borderWidth = null, BorderRadius borderRadius = null, Paint paint = null);

        void drawRectShadow(Rect rect, Paint paint = null);

        void drawPicture(Picture picture);

        void drawImageRect(Image image, Rect dest, Rect src = null, Paint paint = null);

        void drawLine(Offset from, Offset to, Paint paint = null);

        void concat(Matrix4x4 transform);

        void setMatrix(Matrix4x4 matrix);

        Matrix4x4 getMatrix();

        void save();

        void saveLayer(Rect rect, Paint paint = null);

        void restore();

        int getSaveCount();

        void clipRect(Rect rect, bool doAntiAlias = true);

        void clipRRect(RRect rrect, bool doAntiAlias = true);

        void drawTextBlob(TextBlob textBlob, Offset offset);
    }

    public class RecorderCanvas : Canvas {
        public RecorderCanvas(PictureRecorder recorder) {
            this._recorder = recorder;
        }

        readonly PictureRecorder _recorder;

        int _saveCount = 1;

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

        public void drawImageRect(Image image, Rect dest, Rect src, Paint paint) {
            this._recorder.addDrawCmd(new DrawImageRect {
                image = image,
                dest = dest,
                src = src,
                paint = paint,
            });
        }

        public void drawLine(Offset from, Offset to, Paint paint) {
            this._recorder.addDrawCmd(new DrawLine {
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

        public void setMatrix(Matrix4x4 matrix) {
            this._recorder.addDrawCmd(new DrawSetMatrix {
                matrix =  matrix,
            });
        }

        public Matrix4x4 getMatrix() {
            throw new Exception("not available in recorder");            
        }

        public void save() {
            this._saveCount++;
            this._recorder.addDrawCmd(new DrawSave {
            });
        }

        public void saveLayer(Rect rect, Paint paint) {
            this._saveCount++;
            this._recorder.addDrawCmd(new DrawSaveLayer {
                rect = rect,
                paint = paint,
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

        public void clipRect(Rect rect, bool doAntiAlias = true) {
            this._recorder.addDrawCmd(new DrawClipRect {
                rect = rect,
            });
        }

        public void clipRRect(RRect rrect, bool doAntiAlias = true) {
            this._recorder.addDrawCmd(new DrawClipRRect {
                rrect = rrect,
            });
        }

        public void drawTextBlob(TextBlob textBlob, Offset offset) {
            this._recorder.addDrawCmd(new DrawTextBlob {
                textBlob = textBlob,
                offset = offset
            });
        }
    }
}