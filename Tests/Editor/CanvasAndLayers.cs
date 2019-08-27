using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Gradient = Unity.UIWidgets.ui.Gradient;
using Material = UnityEngine.Material;
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

        static Texture2D texture6;

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
                this.drawParagraph,
            };
            this._optionStrings = this._options.Select(x => x.Method.Name).ToArray();
            this._selected = 0;

            this.titleContent = new GUIContent("CanvasAndLayers");
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

                if (Event.current.type == EventType.MouseDown) {
                    Promise.Delayed(new TimeSpan(0, 0, 5)).Then(() => { Debug.Log("Promise.Delayed: 5s"); });
                }

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

            texture6 = Resources.Load<Texture2D>("6");
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
            var canvas = new CommandBufferCanvas(this._renderTexture, Window.instance.devicePixelRatio,
                this._meshPool);

            var paint = new Paint {
                color = new Color(0xFFFF0000),
                shader = Gradient.linear(new Offset(80, 80), new Offset(180, 180), new List<Color>() {
                    Colors.red, Colors.black, Colors.green
                }, null, TileMode.clamp)
            };

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

            canvas.rotate(Mathf.PI * 15 / 180);

            canvas.translate(100, 100);

            paint.shader = Gradient.radial(new Offset(80, 80), 100, new List<Color>() {
                Colors.red, Colors.black, Colors.green
            }, null, TileMode.clamp);
            canvas.drawPath(path, paint);


            canvas.translate(100, 100);
            paint.shader = Gradient.sweep(new Offset(120, 100), new List<Color>() {
                    Colors.red, Colors.black, Colors.green, Colors.red,
                }, null, TileMode.clamp, 10 * Mathf.PI / 180, 135 * Mathf.PI / 180);
            canvas.drawPath(path, paint);


            canvas.translate(100, 100);
            //paint.maskFilter = MaskFilter.blur(BlurStyle.normal, 5);
            paint.shader = new ImageShader(new Image(texture6, true), TileMode.mirror);
            canvas.drawPath(path, paint);

            canvas.flush();
        }

        void drawLine() {
            var canvas = new CommandBufferCanvas(this._renderTexture, Window.instance.devicePixelRatio,
                this._meshPool);

            var paint = new Paint {
                color = new Color(0xFFFF0000),
                style = PaintingStyle.stroke,
                strokeWidth = 10,
                shader = Gradient.linear(new Offset(10, 10), new Offset(180, 180), new List<Color>() {
                    Colors.red, Colors.green, Colors.yellow
                }, null, TileMode.clamp)
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


            canvas.drawArc(Unity.UIWidgets.ui.Rect.fromLTWH(200, 200, 100, 100), Mathf.PI / 4,
                -Mathf.PI / 2 + Mathf.PI * 4 - 1, true, paint);

            paint.maskFilter = MaskFilter.blur(BlurStyle.normal, 1);
            paint.strokeWidth = 4;

            canvas.drawLine(
                new Offset(40, 20),
                new Offset(120, 190),
                paint);

            canvas.scale(3);
            TextBlobBuilder builder = new TextBlobBuilder();
            string text = "This is a text blob";
            builder.setBounds(new Rect(-10, -20, 200, 50));
            builder.setPositionXs(new float[] {
                10, 20, 30, 40, 50, 60, 70, 80, 90, 100,
                110, 120, 130, 140, 150, 160, 170, 180, 190
            });
            builder.allocRunPos(new TextStyle(), text, 0, text.Length);

            var textBlob = builder.make();
            canvas.drawTextBlob(textBlob, new Offset(100, 100), new Paint {
                color = Colors.black,
                maskFilter = MaskFilter.blur(BlurStyle.normal, 5),
            });
            canvas.drawTextBlob(textBlob, new Offset(100, 100), paint);

            canvas.drawLine(
                new Offset(10, 30),
                new Offset(10, 60),
                new Paint() {style = PaintingStyle.stroke, strokeWidth = 0.1f});

            canvas.drawLine(
                new Offset(20, 30),
                new Offset(20, 60),
                new Paint() {style = PaintingStyle.stroke, strokeWidth = 0.333f});

            canvas.flush();
        }

        void drawRect() {
            var canvas = new CommandBufferCanvas(this._renderTexture, Window.instance.devicePixelRatio,
                this._meshPool);

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
            var canvas = new CommandBufferCanvas(this._renderTexture, Window.instance.devicePixelRatio,
                this._meshPool);

            var paint = new Paint {
                color = new Color(0xFF00FF00),
                maskFilter = MaskFilter.blur(BlurStyle.normal, 3),
            };

            canvas.clipRect(Unity.UIWidgets.ui.Rect.fromLTWH(25, 25, 300, 300));

            canvas.rotate(-Mathf.PI / 8.0f);

            canvas.drawRect(
                Unity.UIWidgets.ui.Rect.fromLTWH(10, 10, 100, 100),
                paint);

            paint = new Paint {
                color = new Color(0xFFFFFF00),
                maskFilter = MaskFilter.blur(BlurStyle.normal, 5),
                style = PaintingStyle.stroke,
                strokeWidth = 55,
                shader = Gradient.linear(new Offset(10, 10), new Offset(180, 180), new List<Color>() {
                    Colors.red, Colors.green, Colors.yellow
                }, null, TileMode.clamp)
            };

            canvas.drawRect(
                Unity.UIWidgets.ui.Rect.fromLTWH(10, 150, 200, 200),
                paint);

            canvas.drawImage(new Image(texture6, true),
                new Offset(50, 150),
                paint);

            canvas.flush();
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
                maskFilter = MaskFilter.blur(BlurStyle.normal, 3),
            };
            canvas.drawRect(
                Unity.UIWidgets.ui.Rect.fromLTWH(150, 150, 110, 120),
                paint);

            var picture = pictureRecorder.endRecording();
            Debug.Log("picture.paintBounds: " + picture.paintBounds);

            var editorCanvas = new CommandBufferCanvas(this._renderTexture, Window.instance.devicePixelRatio,
                this._meshPool);
            editorCanvas.drawPicture(picture);

            editorCanvas.rotate(-15 * Mathf.PI / 180);
            editorCanvas.translate(100, 100);
            editorCanvas.drawPicture(picture);
            editorCanvas.flush();
        }

        void drawParagraph() {
            var pb = new ParagraphBuilder(new ParagraphStyle{});
            pb.addText("Hello drawParagraph");
            var paragraph = pb.build();
            paragraph.layout(new ParagraphConstraints(width:300));
            var canvas = new CommandBufferCanvas(this._renderTexture, Window.instance.devicePixelRatio,
                this._meshPool);
            canvas.drawParagraph(paragraph, new Offset(10f, 100f));
            canvas.flush();
            Unity.UIWidgets.ui.Paragraph.release(ref paragraph);
        }
        
        void drawImageRect() {
            if (this._stream == null || this._stream.completer == null || this._stream.completer.currentImage == null) {
                return;
            }

            var canvas = new CommandBufferCanvas(this._renderTexture, Window.instance.devicePixelRatio,
                this._meshPool);

            var paint = new Paint {
                // color = new Color(0x7FFF0000),
                shader = Gradient.linear(new Offset(100, 100), new Offset(280, 280), new List<Color>() {
                    Colors.red, Colors.black, Colors.green
                }, null, TileMode.clamp)
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

            canvas.rotate(-45 * Mathf.PI / 180.0f, new Offset(150, 150));

//            paint = new Paint {
//                color = new Color(0xFF00FFFF),
//                blurSigma = 3,
//            };
//            canvas.drawRectShadow(
//                Rect.fromLTWH(150, 150, 110, 120),
//                paint);

            var picture = pictureRecorder.endRecording();
            Debug.Log("picture.paintBounds: " + picture.paintBounds);

            var editorCanvas = new CommandBufferCanvas(this._renderTexture, Window.instance.devicePixelRatio,
                this._meshPool);
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

            canvas.rotate(-45 * Mathf.PI / 180.0f, new Offset(150, 150));

//            paint = new Paint {
//                color = new Color(0xFF00FFFF),
//                blurSigma = 3,
//            };
//            canvas.drawRectShadow(
//                Rect.fromLTWH(150, 150, 110, 120),
//                paint);

            var picture = pictureRecorder.endRecording();
            Debug.Log("picture.paintBounds: " + picture.paintBounds);

            var editorCanvas = new CommandBufferCanvas(this._renderTexture, Window.instance.devicePixelRatio,
                this._meshPool);
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
            var paint1 = new Paint {
                color = new Color(0xFFFFFFFF),
            };

            var path1 = new Path();
            path1.moveTo(0, 0);
            path1.lineTo(0, 90);
            path1.lineTo(90, 90);
            path1.lineTo(90, 0);
            path1.close();
            canvas.drawPath(path1, paint1);

            
            var paint = new Paint {
                color = new Color(0xFFFF0000),
            };

            var path = new Path();
            path.moveTo(20, 20);
            path.lineTo(20, 70);
            path.lineTo(70, 70);
            path.lineTo(70, 20);
            path.close();

            canvas.drawPath(path, paint);

            var paint2 = new Paint {
                color = new Color(0xFFFFFF00),
            };

            var path2 = new Path();
            path2.moveTo(30, 30);
            path2.lineTo(30, 60);
            path2.lineTo(60, 60);
            path2.lineTo(60, 30);
            path2.close();

            canvas.drawPath(path2, paint2);

            var picture = pictureRecorder.endRecording();

            var editorCanvas = new CommandBufferCanvas(this._renderTexture, Window.instance.devicePixelRatio,
                this._meshPool);

            editorCanvas.saveLayer(
                picture.paintBounds, new Paint {
                    color = new Color(0xFFFFFFFF),
                });
            editorCanvas.drawPicture(picture);
            editorCanvas.restore();

            editorCanvas.saveLayer(Unity.UIWidgets.ui.Rect.fromLTWH(45, 45, 90, 90), new Paint {
                color = new Color(0xFFFFFFFF),
                backdrop = ImageFilter.blur(3f, 3f)
            });
            editorCanvas.restore();

            editorCanvas.flush();
        }
    }
}