using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = UnityEngine.Rect;

namespace UIWidgets.Tests {
    public class CanvasAndLayers : EditorWindow {
        static Material _guiTextureMat;

        internal static Material _getGUITextureMat() {
            if (_guiTextureMat) {
                return _guiTextureMat;
            }

            var guiTextureShader = Shader.Find("UIWidgets/GUITexture");
            if (guiTextureShader == null) {
                throw new Exception("UIWidgets/GUITexture not found");
            }

            _guiTextureMat = new Material(guiTextureShader);
            _guiTextureMat.hideFlags = HideFlags.HideAndDontSave;
            return _guiTextureMat;
        }

        readonly Action[] _options;

        readonly string[] _optionStrings;

        int _selected;

        ImageStream _stream;

        RenderTexture _renderTexture;

        WindowAdapter _windowAdapter;

        MeshPool _meshPool;

        CanvasAndLayers() {
            this._options = new Action[] {
                this.drawPloygon4,
                this.drawRect,
                this.drawRectShadow,
                this.drawImageRect,
                this.drawPicture,
                this.clipRect,
                this.clipRRect,
                this.saveLayer,
                this.drawLine,
            };
            this._optionStrings = this._options.Select(x => x.Method.Name).ToArray();
            this._selected = 0;

            this.titleContent = new GUIContent("CanvasAndLayers");
            this.SetAntiAliasing(4);
        }

        void OnGUI() {
            this._selected = EditorGUILayout.Popup("test case", this._selected, this._optionStrings);
            if (this._selected == 3) {
                using (this._windowAdapter.getScope()) {
                    if (GUI.Button(new Rect(20, 50, 100, 20), "Image 1")) {
                        this.LoadImage(
                            "http://a.hiphotos.baidu.com/image/h%3D300/sign=10b374237f0e0cf3bff748fb3a47f23d/adaf2edda3cc7cd90df1ede83401213fb80e9127.jpg");
                    }

                    if (GUI.Button(new Rect(20, 150, 100, 20), "Image 2")) {
                        this.LoadImage(
                            "http://a.hiphotos.baidu.com/image/pic/item/cf1b9d16fdfaaf519b4aa960875494eef11f7a47.jpg");
                    }

                    if (GUI.Button(new Rect(20, 250, 100, 20), "Image 3")) {
                        this.LoadImage(
                            "http://a.hiphotos.baidu.com/image/pic/item/2f738bd4b31c8701c1e721dd2a7f9e2f0708ffbc.jpg");
                    }
                }
            }

            this._windowAdapter.OnGUI();

            if (Event.current.type == EventType.Repaint || Event.current.type == EventType.MouseDown) {
                this.createRenderTexture();

                Window.instance = this._windowAdapter;

                this._options[this._selected]();

                Window.instance = null;

                Graphics.DrawTexture(new Rect(0, 0, this.position.width, this.position.height),
                    this._renderTexture, _getGUITextureMat());
            }
        }

        void Update() {
            this._windowAdapter.Update();
        }

        void OnEnable() {
            this._windowAdapter = new EditorWindowAdapter(this);
            this._windowAdapter.OnEnable();
            this._meshPool = new MeshPool();
        }

        void OnDisable() {
            this._meshPool.Dispose();
            this._meshPool = null;
        }

        void createRenderTexture() {
            var width = (int) (this.position.width * EditorGUIUtility.pixelsPerPoint);
            var height = (int) (this.position.height * EditorGUIUtility.pixelsPerPoint);
            if (this._renderTexture == null ||
                this._renderTexture.width != width ||
                this._renderTexture.height != height) {
                var desc = new RenderTextureDescriptor(
                    width,
                    height,
                    RenderTextureFormat.Default, 24) {
                    msaaSamples = QualitySettings.antiAliasing,
                    useMipMap = false,
                    autoGenerateMips = false,
                };

                this._renderTexture = RenderTexture.GetTemporary(desc);
            }
        }

        void LoadImage(string url) {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            NetworkImage networkImage = new NetworkImage(url, headers: headers);
            ImageConfiguration imageConfig = new ImageConfiguration();
            this._stream = networkImage.resolve(imageConfig);
        }

        void drawPloygon4() {
            var canvas = new CommandBufferCanvas(this._renderTexture, (float) Window.instance.devicePixelRatio, this._meshPool);

            var paint = new Paint {
                color = new Color(0xFFFF0000),
            };

//            canvas.drawRect(
//                Unity.UIWidgets.ui.Rect.fromLTRB(10, 10, 110, 110),
//                paint);

            var path = new Path();
            path.moveTo(10, 150);
            path.lineTo(10, 160);
            path.lineTo(140, 120);
            path.lineTo(110, 180);
            path.winding(PathWinding.clockwise);
            path.close();
            path.addRect(Unity.UIWidgets.ui.Rect.fromLTWH(0, 100, 100, 100));
            path.addRect(Unity.UIWidgets.ui.Rect.fromLTWH(200, 0, 100, 100));
            path.addRRect(RRect.fromRectAndRadius(Unity.UIWidgets.ui.Rect.fromLTWH(150, 100, 30, 30), 10));
            path.addOval(Unity.UIWidgets.ui.Rect.fromLTWH(150, 50, 100, 100));
            path.winding(PathWinding.clockwise);

            if (Event.current.type == EventType.MouseDown) {
                var pos = new Offset(
                    Event.current.mousePosition.x,
                    Event.current.mousePosition.y
                );
                
                Debug.Log(pos + ": " + path.contains(pos));
            }

            canvas.drawPath(path, paint);
            canvas.flush();
        }

        void drawLine() {
            var canvas = new CommandBufferCanvas(this._renderTexture, (float) Window.instance.devicePixelRatio, this._meshPool);

            var paint = new Paint {
                color = new Color(0xFFFF0000),
                style = PaintingStyle.stroke,
                strokeWidth = 10,
            };

            canvas.drawLine(
                new Offset(10, 20),
                new Offset(50, 20),
                paint);

            canvas.drawLine(
                new Offset(10, 10),
                new Offset(100, 100),
                paint);

            canvas.drawLine(
                new Offset(10, 10),
                new Offset(10, 50),
                paint);

            canvas.drawLine(
                new Offset(40, 10),
                new Offset(90, 10),
                paint);

            var widthPaint = new Paint {
                color = new Color(0xFFFF0000),
                strokeWidth = 4,
            };

            canvas.drawLine(
                new Offset(40, 20),
                new Offset(120, 190),
                widthPaint);

            canvas.flush();
        }

        void drawRect() {
            var canvas = new CommandBufferCanvas(this._renderTexture, (float) Window.instance.devicePixelRatio, this._meshPool);

            var paint = new Paint {
                color = new Color(0xFFFF0000),
            };

            canvas.rotate(15 * Mathf.PI / 180);
            var rect = Unity.UIWidgets.ui.Rect.fromLTWH(10, 10, 100, 100);
            var rrect = RRect.fromRectAndCorners(rect, 0, 4, 8, 16);

            canvas.drawRRect(rrect, paint);

            paint = new Paint {
                color = new Color(0xFF00FF00),
            };

            rect = Unity.UIWidgets.ui.Rect.fromLTWH(10, 150, 100, 100);
            rrect = RRect.fromRectAndCorners(rect, 0, 4, 8, 16);
            canvas.drawRRect(rrect, paint);

            rect = Unity.UIWidgets.ui.Rect.fromLTWH(150, 150, 100, 100);
            rrect = RRect.fromRectAndCorners(rect, 10, 12, 14, 16);
            var rect1 = Unity.UIWidgets.ui.Rect.fromLTWH(160, 160, 80, 80);
            var rrect1 = RRect.fromRectAndCorners(rect1, 5, 6, 7, 8);

            canvas.drawDRRect(rrect, rrect1, paint);

            canvas.flush();
        }

        void drawRectShadow() {
//            var canvas = new CanvasImpl();
//
//            var paint = new Paint {
//                color = new Color(0xFF00FF00),
//                blurSigma = 3.0,
//            };
//
//            canvas.drawRectShadow(
//                Rect.fromLTWH(10, 10, 100, 100),
//                paint);
//
//            paint = new Paint {
//                color = new Color(0xFFFFFF00),
//                blurSigma = 2.0,
//            };
//
//            canvas.drawRectShadow(
//                Rect.fromLTWH(10, 150, 100, 100),
//                paint);
        }

        void drawPicture() {
            var pictureRecorder = new PictureRecorder();
            var canvas = new RecorderCanvas(pictureRecorder);

            var paint = new Paint {
                color = new Color(0xFFFF0000),
            };

            var path = new Path();
            path.moveTo(10, 10);
            path.lineTo(10, 110);
            path.lineTo(90, 110);
            path.lineTo(100, 10);
            path.close();
            canvas.drawPath(path, paint);

            paint = new Paint {
                color = new Color(0xFFFFFF00),
            };

            var rect = Unity.UIWidgets.ui.Rect.fromLTWH(10, 150, 100, 100);
            var rrect = RRect.fromRectAndCorners(rect, 0, 4, 8, 16);
            var rect1 = Unity.UIWidgets.ui.Rect.fromLTWH(18, 152, 88, 92);
            var rrect1 = RRect.fromRectAndCorners(rect1, 0, 4, 8, 16);

            canvas.drawDRRect(rrect, rrect1, paint);

            canvas.rotate(-45 * Mathf.PI / 180, new Offset(150, 150));

            paint = new Paint {
                color = new Color(0xFF00FFFF),
                blurSigma = 3,
            };
            canvas.drawRect(
                Unity.UIWidgets.ui.Rect.fromLTWH(150, 150, 110, 120),
                paint);

            var picture = pictureRecorder.endRecording();
            Debug.Log("picture.paintBounds: " + picture.paintBounds);

            var editorCanvas = new CommandBufferCanvas(this._renderTexture, (float) Window.instance.devicePixelRatio, this._meshPool);
            editorCanvas.drawPicture(picture);

            editorCanvas.rotate(-15 * Mathf.PI / 180);
            editorCanvas.translate(100, 100);
            editorCanvas.drawPicture(picture);
            editorCanvas.flush();
        }

        void drawImageRect() {
            if (this._stream == null || this._stream.completer == null || this._stream.completer.currentImage == null) {
                return;
            }

            var canvas = new CommandBufferCanvas(this._renderTexture, (float) Window.instance.devicePixelRatio, this._meshPool);

            var paint = new Paint {
                color = new Color(0x7FFF0000),
            };

            canvas.drawImageRect(this._stream.completer.currentImage.image,
                Unity.UIWidgets.ui.Rect.fromLTWH(100, 50, 250, 250),
                paint
            );
            canvas.flush();
        }

        void clipRect() {
            var pictureRecorder = new PictureRecorder();
            var canvas = new RecorderCanvas(pictureRecorder);

            var paint = new Paint {
                color = new Color(0xFFFF0000),
            };

            var path = new Path();
            path.moveTo(10, 10);
            path.lineTo(10, 110);
            path.lineTo(90, 110);
            path.lineTo(110, 10);
            path.close();

            canvas.drawPath(path, paint);

            paint = new Paint {
                color = new Color(0xFFFFFF00),
            };

            var rect = Unity.UIWidgets.ui.Rect.fromLTWH(10, 150, 100, 100);
            var rrect = RRect.fromRectAndCorners(rect, 0, 4, 8, 16);
            var rect1 = Unity.UIWidgets.ui.Rect.fromLTWH(18, 152, 88, 92);
            var rrect1 = RRect.fromRectAndCorners(rect1, 0, 4, 8, 16);
            canvas.drawDRRect(rrect, rrect1, paint);

            canvas.rotate(-45 * Mathf.PI / 180.0, new Offset(150, 150));

//            paint = new Paint {
//                color = new Color(0xFF00FFFF),
//                blurSigma = 3,
//            };
//            canvas.drawRectShadow(
//                Rect.fromLTWH(150, 150, 110, 120),
//                paint);

            var picture = pictureRecorder.endRecording();
            Debug.Log("picture.paintBounds: " + picture.paintBounds);

            var editorCanvas = new CommandBufferCanvas(this._renderTexture, (float) Window.instance.devicePixelRatio, this._meshPool);
            editorCanvas.rotate(-5 * Mathf.PI / 180);
            editorCanvas.clipRect(Unity.UIWidgets.ui.Rect.fromLTWH(25, 15, 250, 250));
            editorCanvas.rotate(5 * Mathf.PI / 180);

            editorCanvas.drawPicture(picture);

            editorCanvas.rotate(-15 * Mathf.PI / 180);
            editorCanvas.translate(100, 100);

            editorCanvas.drawPicture(picture);

            editorCanvas.flush();
        }

        void clipRRect() {
            var pictureRecorder = new PictureRecorder();
            var canvas = new RecorderCanvas(pictureRecorder);

            var paint = new Paint {
                color = new Color(0xFFFF0000),
            };

            var path = new Path();
            path.moveTo(10, 10);
            path.lineTo(10, 110);
            path.lineTo(90, 110);
            path.lineTo(110, 10);
            path.close();

            canvas.drawPath(path, paint);

            paint = new Paint {
                color = new Color(0xFFFFFF00),
            };

            var rect = Unity.UIWidgets.ui.Rect.fromLTWH(10, 150, 100, 100);
            var rrect = RRect.fromRectAndCorners(rect, 0, 4, 8, 16);
            var rect1 = Unity.UIWidgets.ui.Rect.fromLTWH(18, 152, 88, 92);
            var rrect1 = RRect.fromRectAndCorners(rect1, 0, 4, 8, 16);
            canvas.drawDRRect(rrect, rrect1, paint);

            canvas.rotate(-45 * Mathf.PI / 180.0, new Offset(150, 150));

//            paint = new Paint {
//                color = new Color(0xFF00FFFF),
//                blurSigma = 3,
//            };
//            canvas.drawRectShadow(
//                Rect.fromLTWH(150, 150, 110, 120),
//                paint);

            var picture = pictureRecorder.endRecording();
            Debug.Log("picture.paintBounds: " + picture.paintBounds);

            var editorCanvas = new CommandBufferCanvas(this._renderTexture, (float) Window.instance.devicePixelRatio, this._meshPool);
            editorCanvas.rotate(-5 * Mathf.PI / 180);
            editorCanvas.clipRRect(RRect.fromRectAndRadius(Unity.UIWidgets.ui.Rect.fromLTWH(25, 15, 250, 250), 50));
            editorCanvas.rotate(5 * Mathf.PI / 180);

            editorCanvas.drawPicture(picture);

            editorCanvas.rotate(-15 * Mathf.PI / 180);
            editorCanvas.translate(100, 100);

            editorCanvas.drawPicture(picture);

            editorCanvas.flush();
        }

        void saveLayer() {
            var pictureRecorder = new PictureRecorder();
            var canvas = new RecorderCanvas(pictureRecorder);

            var paint = new Paint {
                color = new Color(0xFFFF0000),
            };

            var path = new Path();
            path.moveTo(10, 10);
            path.lineTo(10, 110);
            path.lineTo(90, 110);
            path.lineTo(110, 10);
            path.close();

            canvas.drawPath(path, paint);

            paint = new Paint {
                color = new Color(0xFFFFFF00),
            };

            var rect = Unity.UIWidgets.ui.Rect.fromLTWH(10, 150, 100, 100);
            var rrect = RRect.fromRectAndCorners(rect, 0, 4, 8, 16);
            var rect1 = Unity.UIWidgets.ui.Rect.fromLTWH(18, 152, 88, 92);
            var rrect1 = RRect.fromRectAndCorners(rect1, 0, 4, 8, 16);
            canvas.drawDRRect(rrect, rrect1, paint);

            canvas.rotate(-45 * Mathf.PI / 180.0, new Offset(150, 150));

//            paint = new Paint {
//                color = new Color(0xFF00FFFF),
//                blurSigma = 3,
//            };
//            canvas.drawRectShadow(
//                Rect.fromLTWH(150, 150, 110, 120),
//                paint);

            var picture = pictureRecorder.endRecording();
            Debug.Log("picture.paintBounds: " + picture.paintBounds);

            var editorCanvas = new CommandBufferCanvas(this._renderTexture, (float) Window.instance.devicePixelRatio, this._meshPool);

            editorCanvas.saveLayer(picture.paintBounds, new Paint {color = new Color(0x7FFFFFFF)});
            editorCanvas.drawPicture(picture);
            editorCanvas.restore();

            editorCanvas.translate(150, 0);
            editorCanvas.saveLayer(picture.paintBounds, new Paint {color = new Color(0xFFFFFFFF)});
            editorCanvas.drawPicture(picture);
            editorCanvas.restore();

            editorCanvas.flush();
        }
    }
}
