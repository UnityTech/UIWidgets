using System;
using System.Linq;
using UIWidgets.painting;
using UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using Color = UIWidgets.ui.Color;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.Tests {
    public class CanvasAndLayers : EditorWindow {
        private readonly Action[] _options;

        private readonly string[] _optionStrings;

        private int _selected;

        CanvasAndLayers() {
            this._options = new Action[] {
                this.drawPloygon4,
                this.drawRect,
                this.drawRectShadow,
                this.drawPicture,
                this.clipRect,
                this.clipRRect,
                this.saveLayer,
            };
            this._optionStrings = this._options.Select(x => x.Method.Name).ToArray();
            this._selected = 0;

            this.titleContent = new GUIContent("CanvasAndLayers");
        }

        void OnGUI() {
            this._selected = EditorGUILayout.Popup("test case", this._selected, this._optionStrings);

            if (Event.current.type == EventType.Repaint) {
                this._options[this._selected]();
            }
        }

        void drawPloygon4() {
            var canvas = new CanvasImpl();

            var paint = new Paint {
                color = new Color(0xFFFF0000),
            };

            canvas.drawPloygon4(
                new[] {new Offset(10, 10), new Offset(10, 110), new Offset(110, 110), new Offset(110, 10)},
                paint);

            canvas.drawPloygon4(
                new[] {new Offset(10, 150), new Offset(10, 160), new Offset(140, 120), new Offset(110, 180)},
                paint);
        }

        void drawRect() {
            var canvas = new CanvasImpl();

            var paint = new Paint {
                color = new Color(0xFFFF0000),
            };

            canvas.drawRect(
                Rect.fromLTWH(10, 10, 100, 100),
                BorderWidth.only(2, 4, 6, 8),
                BorderRadius.only(0, 4, 8, 16),
                paint);

            paint = new Paint {
                color = new Color(0xFF00FF00),
            };

            canvas.drawRect(
                Rect.fromLTWH(10, 150, 100, 100),
                BorderWidth.only(),
                BorderRadius.only(0, 4, 8, 16),
                paint);

            canvas.drawRect(
                Rect.fromLTWH(150, 150, 100, 100),
                BorderWidth.only(10, 12, 14, 16),
                BorderRadius.only(),
                paint);
        }

        void drawRectShadow() {
            var canvas = new CanvasImpl();

            var paint = new Paint {
                color = new Color(0xFF00FF00),
                blurSigma = 3.0,
            };

            canvas.drawRectShadow(
                Rect.fromLTWH(10, 10, 100, 100),
                paint);

            paint = new Paint {
                color = new Color(0xFFFFFF00),
                blurSigma = 2.0,
            };

            canvas.drawRectShadow(
                Rect.fromLTWH(10, 150, 100, 100),
                paint);
        }

        void drawPicture() {
            var pictureRecorder = new PictureRecorder();
            var canvas = new RecorderCanvas(pictureRecorder);

            var paint = new Paint {
                color = new Color(0xFFFF0000),
            };

            canvas.drawPloygon4(
                new[] {new Offset(10, 10), new Offset(10, 110), new Offset(90, 110), new Offset(110, 10)},
                paint);

            paint = new Paint {
                color = new Color(0xFFFFFF00),
            };

            canvas.drawRect(
                Rect.fromLTWH(10, 150, 100, 100),
                BorderWidth.only(2, 4, 6, 8),
                BorderRadius.only(0, 4, 8, 16),
                paint);

            canvas.concat(Matrix4x4.Translate(new Vector2(-150, -150)));
            canvas.concat(Matrix4x4.Rotate(Quaternion.Euler(0, 0, -45)));
            canvas.concat(Matrix4x4.Translate(new Vector2(150, 150)));

            paint = new Paint {
                color = new Color(0xFF00FFFF),
                blurSigma = 3,
            };
            canvas.drawRectShadow(
                Rect.fromLTWH(150, 150, 110, 120),
                paint);

            var picture = pictureRecorder.endRecording();
            Debug.Log("picture.paintBounds: " + picture.paintBounds);

            var editorCanvas = new CanvasImpl();
            editorCanvas.drawPicture(picture);

            editorCanvas.concat(Matrix4x4.Rotate(Quaternion.Euler(0, 0, -15)));
            editorCanvas.concat(Matrix4x4.Translate(new Vector2(100, 100)));
            editorCanvas.drawPicture(picture);
        }

        void clipRect() {
            var pictureRecorder = new PictureRecorder();
            var canvas = new RecorderCanvas(pictureRecorder);

            var paint = new Paint {
                color = new Color(0xFFFF0000),
            };

            canvas.drawPloygon4(
                new[] {new Offset(10, 10), new Offset(10, 110), new Offset(90, 110), new Offset(110, 10)},
                paint);

            paint = new Paint {
                color = new Color(0xFFFFFF00),
            };

            canvas.drawRect(
                Rect.fromLTWH(10, 150, 100, 100),
                BorderWidth.only(2, 4, 6, 8),
                BorderRadius.only(0, 4, 8, 16),
                paint);

            canvas.concat(Matrix4x4.Translate(new Vector2(-150, -150)));
            canvas.concat(Matrix4x4.Rotate(Quaternion.Euler(0, 0, -45)));
            canvas.concat(Matrix4x4.Translate(new Vector2(150, 150)));

            paint = new Paint {
                color = new Color(0xFF00FFFF),
                blurSigma = 3,
            };
            canvas.drawRectShadow(
                Rect.fromLTWH(150, 150, 110, 120),
                paint);

            var picture = pictureRecorder.endRecording();
            Debug.Log("picture.paintBounds: " + picture.paintBounds);

            var editorCanvas = new CanvasImpl();
            editorCanvas.concat(Matrix4x4.Rotate(Quaternion.Euler(0, 0, -5)));
            editorCanvas.clipRect(Rect.fromLTWH(25, 15, 250, 250));
            editorCanvas.concat(Matrix4x4.Rotate(Quaternion.Euler(0, 0, 5)));

            editorCanvas.drawPicture(picture);

            editorCanvas.concat(Matrix4x4.Rotate(Quaternion.Euler(0, 0, -15)));
            editorCanvas.concat(Matrix4x4.Translate(new Vector2(100, 100)));
            editorCanvas.drawPicture(picture);
        }

        void clipRRect() {
            var pictureRecorder = new PictureRecorder();
            var canvas = new RecorderCanvas(pictureRecorder);

            var paint = new Paint {
                color = new Color(0xFFFF0000),
            };

            canvas.drawPloygon4(
                new[] {new Offset(10, 10), new Offset(10, 110), new Offset(90, 110), new Offset(110, 10)},
                paint);

            paint = new Paint {
                color = new Color(0xFFFFFF00),
            };

            canvas.drawRect(
                Rect.fromLTWH(10, 150, 100, 100),
                BorderWidth.only(2, 4, 6, 8),
                BorderRadius.only(0, 4, 8, 16),
                paint);

            canvas.concat(Matrix4x4.Translate(new Vector2(-150, -150)));
            canvas.concat(Matrix4x4.Rotate(Quaternion.Euler(0, 0, -45)));
            canvas.concat(Matrix4x4.Translate(new Vector2(150, 150)));

            paint = new Paint {
                color = new Color(0xFF00FFFF),
                blurSigma = 3,
            };
            canvas.drawRectShadow(
                Rect.fromLTWH(150, 150, 110, 120),
                paint);

            var picture = pictureRecorder.endRecording();
            Debug.Log("picture.paintBounds: " + picture.paintBounds);

            var editorCanvas = new CanvasImpl();
            editorCanvas.concat(Matrix4x4.Rotate(Quaternion.Euler(0, 0, -5)));
            editorCanvas.clipRRect(RRect.fromRectAndRadius(Rect.fromLTWH(25, 15, 250, 250), 50));
            editorCanvas.concat(Matrix4x4.Rotate(Quaternion.Euler(0, 0, 5)));

            editorCanvas.drawPicture(picture);

            editorCanvas.concat(Matrix4x4.Rotate(Quaternion.Euler(0, 0, -15)));
            editorCanvas.concat(Matrix4x4.Translate(new Vector2(100, 100)));
            editorCanvas.drawPicture(picture);
        }

        void saveLayer() {
            var pictureRecorder = new PictureRecorder();
            var canvas = new RecorderCanvas(pictureRecorder);

            var paint = new Paint {
                color = new Color(0xFFFF0000),
            };

            canvas.drawPloygon4(
                new[] {new Offset(10, 10), new Offset(10, 110), new Offset(90, 110), new Offset(110, 10)},
                paint);

            paint = new Paint {
                color = new Color(0xFFFFFF00),
            };

            canvas.drawRect(
                Rect.fromLTWH(10, 150, 100, 100),
                BorderWidth.only(2, 4, 6, 8),
                BorderRadius.only(0, 4, 8, 16),
                paint);

            canvas.concat(Matrix4x4.Translate(new Vector2(-150, -150)));
            canvas.concat(Matrix4x4.Rotate(Quaternion.Euler(0, 0, -45)));
            canvas.concat(Matrix4x4.Translate(new Vector2(150, 150)));

            canvas.clipRRect(RRect.fromRectAndRadius(Rect.fromLTWH(180, 150, 60, 120), 40));
            paint = new Paint {
                color = new Color(0xFF00FFFF),
                blurSigma = 3,
            };
            canvas.drawRectShadow(
                Rect.fromLTWH(150, 150, 110, 120),
                paint);

            var picture = pictureRecorder.endRecording();
            Debug.Log("picture.paintBounds: " + picture.paintBounds);

            var editorCanvas = new CanvasImpl();
            editorCanvas.concat(Matrix4x4.Rotate(Quaternion.Euler(0, 0, -5)));
            editorCanvas.clipRRect(RRect.fromRectAndRadius(Rect.fromLTWH(25, 15, 250, 250), 50));
            editorCanvas.concat(Matrix4x4.Rotate(Quaternion.Euler(0, 0, 5)));

            editorCanvas.saveLayer(picture.paintBounds, new Paint {color = new Color(0x7FFFFFFF)});
            editorCanvas.drawPicture(picture);

            editorCanvas.concat(Matrix4x4.Rotate(Quaternion.Euler(0, 0, -15)));
            editorCanvas.concat(Matrix4x4.Translate(new Vector2(100, 100)));
            editorCanvas.drawPicture(picture);
            editorCanvas.restore();
        }
    }
}