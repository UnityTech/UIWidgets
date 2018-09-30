using System;
using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.painting;
using UIWidgets.ui.painting.txt;
using UIWidgets.ui.txt;
using UnityEditor;
using UnityEngine;

namespace UIWidgets.ui {
    public class CanvasImpl : Canvas {
        static CanvasImpl() {
            var shader = Shader.Find("UIWidgets/2D Handles Lines");
            if (shader == null) {
                throw new Exception("UIWidgets/2D Handles Lines not found");
            }

            linesMat = new Material(shader);
            linesMat.hideFlags = HideFlags.HideAndDontSave;

            shader = Shader.Find("UIWidgets/GUIRoundedRect");
            if (shader == null) {
                throw new Exception("UIWidgets/GUIRoundedRect not found");
            }

            guiRoundedRectMat = new Material(shader);
            guiRoundedRectMat.hideFlags = HideFlags.HideAndDontSave;

            shader = Shader.Find("UIWidgets/GUITextureClip");
            if (shader == null) {
                throw new Exception("UIWidgets/GUITextureClip not found");
            }

            guiTextureClipMat = new Material(shader);
            guiTextureClipMat.hideFlags = HideFlags.HideAndDontSave;

            shader = Shader.Find("UIWidgets/ShadowRect");
            if (shader == null) {
                throw new Exception("UIWidgets/ShadowRect not found");
            }

            shadowRectMat = new Material(shader);
            shadowRectMat.hideFlags = HideFlags.HideAndDontSave;
        }

        private static readonly Material linesMat;
        private static readonly Material guiRoundedRectMat;
        private static readonly Material guiTextureClipMat;
        private static readonly Material shadowRectMat;

        private Matrix4x4 _transform;
        private ClipRec _clipRec;
        private LayerRec _layerRec;
        private Stack<CanvasRec> _stack;
        private RenderTexture _defaultTexture;

        private Stack<CanvasRec> stack {
            get { return this._stack ?? (this._stack = new Stack<CanvasRec>()); }
        }

        public CanvasImpl() {
            this._transform = Matrix4x4.identity;
            this._defaultTexture = RenderTexture.active;
        }

        public void drawPloygon4(Offset[] points, Paint paint) {
            var color = paint.color;
            if (color.alpha > 0) {
                Vector3[] verts = new Vector3 [points.Length];
                for (int i = 0; i < points.Length; i++) {
                    verts[i] = points[i].toVector();
                }

                this.prepareGL(linesMat);
                linesMat.SetPass(0);

                GL.Begin(GL.TRIANGLES);
                GL.Color(color.toColor());
                for (int index = 0; index < 2; ++index) {
                    GL.Vertex(verts[index * 2]);
                    GL.Vertex(verts[index * 2 + 1]);
                    GL.Vertex(verts[(index * 2 + 2) % 4]);
                    GL.Vertex(verts[index * 2]);
                    GL.Vertex(verts[(index * 2 + 2) % 4]);
                    GL.Vertex(verts[index * 2 + 1]);
                }

                GL.End();
            }
        }

        public void drawRect(Rect rect, BorderWidth borderWidth, BorderRadius borderRadius, Paint paint) {
            this.prepareGL(guiRoundedRectMat);

            guiRoundedRectMat.SetFloatArray("UIWidgets_BorderWidth",
                borderWidth == null ? new[] {0f, 0f, 0f, 0f} : borderWidth.toFloatArray());
            guiRoundedRectMat.SetFloatArray("UIWidgets_CornerRadius",
                borderRadius == null ? new[] {0f, 0f, 0f, 0f} : borderRadius.toFloatArray());

            Graphics.DrawTexture(rect.toRect(), EditorGUIUtility.whiteTexture,
                new UnityEngine.Rect(0.0f, 0.0f, 1f, 1f), 0, 0, 0, 0,
                paint.color.toColor(), guiRoundedRectMat);
        }


        public void drawRectShadow(Rect rect, Paint paint) {
            this.prepareGL(shadowRectMat);

            shadowRectMat.SetFloat("UIWidgets_sigma", (float) paint.blurSigma);

            Graphics.DrawTexture(rect.toRect(), EditorGUIUtility.whiteTexture,
                new UnityEngine.Rect(0.0f, 0.0f, 1f, 1f), 0, 0, 0, 0,
                paint.color.toColor(), shadowRectMat);
        }

        public void drawPicture(Picture picture) {
            this.save();

            int saveCount = 0;

            var drawCmds = picture.drawCmds;
            foreach (var drawCmd in drawCmds) {
                if (drawCmd is DrawPloygon4) {
                    var drawPloygon4 = (DrawPloygon4) drawCmd;
                    this.drawPloygon4(drawPloygon4.points, drawPloygon4.paint);
                } else if (drawCmd is DrawRect) {
                    var drawRect = (DrawRect) drawCmd;
                    this.drawRect(drawRect.rect, drawRect.borderWidth, drawRect.borderRadius, drawRect.paint);
                } else if (drawCmd is DrawLine) {
                    var drawLine = (DrawLine) drawCmd;
                    this.drawLine(drawLine.from, drawLine.to, drawLine.paint);
                } else if (drawCmd is DrawRectShadow) {
                    var drawRectShadow = (DrawRectShadow) drawCmd;
                    this.drawRectShadow(drawRectShadow.rect, drawRectShadow.paint);
                } else if (drawCmd is DrawPicture) {
                    var drawPicture = (DrawPicture) drawCmd;
                    this.drawPicture(drawPicture.picture);
                } else if (drawCmd is DrawConcat) {
                    var drawConcat = (DrawConcat) drawCmd;
                    this.concat(drawConcat.transform);
                } else if (drawCmd is DrawSetMatrix) {
                    var drawSetMatrix = (DrawSetMatrix) drawCmd;
                    this.setMatrix(drawSetMatrix.matrix);
                } else if (drawCmd is DrawSave) {
                    saveCount++;
                    this.save();
                } else if (drawCmd is DrawSaveLayer) {
                    saveCount++;
                    var drawSaveLayer = (DrawSaveLayer) drawCmd;
                    this.saveLayer(drawSaveLayer.rect, drawSaveLayer.paint);
                } else if (drawCmd is DrawRestore) {
                    saveCount--;
                    if (saveCount < 0) {
                        throw new Exception("unmatched save/restore in picture");
                    }

                    this.restore();
                } else if (drawCmd is DrawClipRect) {
                    var drawClipRect = (DrawClipRect) drawCmd;
                    this.clipRect(drawClipRect.rect);
                } else if (drawCmd is DrawClipRRect) {
                    var drawClipRRect = (DrawClipRRect) drawCmd;
                    this.clipRRect(drawClipRRect.rrect);
                } else if (drawCmd is DrawTextBlob) {
                    var drawTextBlob = (DrawTextBlob) drawCmd;
                    this.drawTextBlob(drawTextBlob.textBlob, drawTextBlob.offset);
                } else if (drawCmd is DrawImageRect) {
                    var drawImageRect = (DrawImageRect) drawCmd;
                    this.drawImageRect(drawImageRect.image, drawImageRect.dest, drawImageRect.src, drawImageRect.paint);
                } else {
                    throw new Exception("unknown drawCmd: " + drawCmd);
                }
            }

            if (saveCount != 0) {
                throw new Exception("unmatched save/restore in picture");
            }

            this.restore();
        }

        public void drawImageRect(Image image, Rect dest, Rect src = null, Paint paint = null) {
            D.assert(image != null);
            D.assert(dest != null);

            if (image.texture != null) {
                // convert src rect to Unity rect in normalized coordinates with (0,0) in the bottom-left corner.
                var textureHeight = image.texture.height;
                var textureWidth = image.texture.width;
                var srcRect = src == null
                    ? new UnityEngine.Rect(0, 0, 1, 1)
                    : new UnityEngine.Rect(
                        (float) (src.left / textureWidth),
                        (float) ((textureHeight - src.bottom) / textureHeight),
                        (float) (src.width / textureWidth),
                        (float) (src.height / textureHeight)
                    );

                this.prepareGL(guiTextureClipMat);

                Graphics.DrawTexture(dest.toRect(), image.texture,
                    srcRect, 0, 0, 0, 0,
                    paint != null && paint.color != null ? paint.color.toColor() : UnityEngine.Color.white,
                    guiTextureClipMat);
            }
        }

        public void drawLine(Offset from, Offset to, Paint paint) {
            var color = paint.color;
            Offset vect = to - from;
            var distance = vect.distance;

            if (color.alpha > 0 && distance > 0) {
                var halfWidth = paint.strokeWidth * 0.5;
                var diff = vect / distance * halfWidth;
                diff = new Offset(diff.dy, -diff.dx);
                this.prepareGL(linesMat);
                linesMat.SetPass(0);
                var points = new[] {
                    (from + diff).toVector(),
                    (from - diff).toVector(),
                    (to - diff).toVector(),
                    (to + diff).toVector(),
                };
                GL.Begin(GL.QUADS);
                GL.Color(color.toColor());
                for (int i = 0; i < points.Length; ++i) {
                    GL.Vertex(points[i]);
                }

                GL.End();
            }
        }

        public void concat(Matrix4x4 transform) {
            this._transform = transform * this._transform;
        }

        public void setMatrix(Matrix4x4 matrix) {
            this._transform = matrix;
        }

        public Matrix4x4 getMatrix() {
            return this._transform;
        }

        public void save() {
            var state = new CanvasRec {
                transform = this._transform,
                clipRect = this._clipRec,
                layerRec = this._layerRec,
            };
            this.stack.Push(state);
        }

        public void saveLayer(Rect bounds, Paint paint) {
            this.save();

            bounds = bounds.roundOut();

            var textureWidth = (int) Math.Ceiling(bounds.width * EditorGUIUtility.pixelsPerPoint);
            var textureHeight = (int) Math.Ceiling(bounds.height * EditorGUIUtility.pixelsPerPoint);

            var texture = RenderTexture.GetTemporary(
                textureWidth, textureHeight, 32,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

            RenderTexture.active = texture;

            GL.PushMatrix();
            GL.LoadPixelMatrix((float) bounds.left, (float) bounds.right, (float) bounds.bottom, (float) bounds.top);
            GL.Clear(true, true, new UnityEngine.Color(0, 0, 0, 0));

            this._layerRec = new LayerRec {
                bounds = bounds,
                paint = paint,
                texture = texture,
            };

            this._transform = Matrix4x4.identity;
            this._clipRec = null;
        }

        public void restore() {
            var layerRec = this._layerRec;

            var state = this._stack.Pop();
            this._transform = state.transform;
            this._clipRec = state.clipRect;
            this._layerRec = state.layerRec;

            if (layerRec != this._layerRec) {
                var targetTexture = this._layerRec != null ? this._layerRec.texture : this._defaultTexture;
                RenderTexture.active = targetTexture;

                GL.PopMatrix();

                this.prepareGL(guiTextureClipMat);

                Graphics.DrawTexture(layerRec.bounds.toRect(), layerRec.texture,
                    new UnityEngine.Rect(0.0f, 0.0f, 1f, 1f), 0, 0, 0, 0,
                    layerRec.paint.color.toColor(), guiTextureClipMat);

                RenderTexture.ReleaseTemporary(layerRec.texture);
                layerRec.texture = null;
            }
        }

        public int getSaveCount() {
            return this._stack.Count + 1;
        }

        public void clipRect(Rect rect, bool doAntiAlias = true) {
            if (rect.isInfinite) {
                return;
            }

            this.pushClipRect(rect, this._transform);
        }

        public void clipRRect(RRect rect, bool doAntiAlias = true) {
            if (rect.isInfinite) {
                return;
            }

            this.pushClipRRect(rect, this._transform);
        }

        public void drawTextBlob(TextBlob textBlob, Offset offset) {
            var mesh = MeshGenrator.generateMesh(textBlob);
            var font = FontManager.instance.getOrCreate(textBlob.style.fontFamily).font;
            prepareGL(font.material);
            font.material.SetPass(0);
            Matrix4x4 cameraMat = Matrix4x4.identity;

            if (Camera.current != null) // draw mesh will use camera matrix, set to identity before draw mesh
            {
                cameraMat = Camera.current.worldToCameraMatrix;
                Camera.current.worldToCameraMatrix = Matrix4x4.identity;
            }

            var textBlobOffset = textBlob.positions[textBlob.start];

            Graphics.DrawMeshNow(mesh, this._transform * Matrix4x4.Translate(
                                           new Vector3((float) Utils.PixelCorrectRound(offset.dx + textBlobOffset.x),
                                               (float) Utils.PixelCorrectRound(offset.dy + textBlobOffset.y), 0)));
            if (Camera.current != null) {
                Camera.current.worldToCameraMatrix = cameraMat;
                Camera.current.ResetWorldToCameraMatrix();
            }
        }

        private void pushClipRect(Rect clipRect, Matrix4x4 transform) {
            if (this._clipRec != null) {
                throw new Exception("already a clipRec, considering using saveLayer.");
            }

            this._clipRec = new ClipRec(transform, rect: clipRect);
        }

        private void pushClipRRect(RRect clipRRect, Matrix4x4 transform) {
            if (this._clipRec != null) {
                throw new Exception("already a clipRec, considering using saveLayer.");
            }

            this._clipRec = new ClipRec(transform, rrect: clipRRect);
        }

        private void prepareGL(Material mat) {
            if (this._clipRec != null) {
                mat.SetMatrix("UIWidgets_GUIClipMatrix", this._clipRec.transform.inverse);
                if (this._clipRec.rect != null) {
                    var rect = this._clipRec.rect;
                    mat.SetVector("UIWidgets_GUIClipRect", new Vector4(
                        (float) rect.left,
                        (float) rect.top,
                        (float) rect.width,
                        (float) rect.height));
                    mat.SetVector("UIWidgets_GUIClipRectRadius", new Vector4(0, 0, 0, 0));
                } else {
                    var rrect = this._clipRec.rrect;
                    var rect = rrect.outerRect;
                    mat.SetVector("UIWidgets_GUIClipRect", new Vector4(
                        (float) rect.left,
                        (float) rect.top,
                        (float) rect.width,
                        (float) rect.height));
                    mat.SetVector("UIWidgets_GUIClipRectRadius",
                        new Vector4(
                            (float) rrect.tlRadius,
                            (float) rrect.trRadius,
                            (float) rrect.brRadius,
                            (float) rrect.blRadius));
                }
            } else {
                mat.SetMatrix("UIWidgets_GUIClipMatrix", Matrix4x4.identity);
                var rect = Rect.largest;
                mat.SetVector("UIWidgets_GUIClipRect", new Vector4(
                    (float) rect.left,
                    (float) rect.top,
                    (float) rect.width,
                    (float) rect.height));
                mat.SetVector("UIWidgets_GUIClipRectRadius", new Vector4(0, 0, 0, 0));
            }

            GL.MultMatrix(this._transform);
        }

        private class ClipRec {
            public ClipRec(Matrix4x4 transform, Rect rect = null, RRect rrect = null) {
                this.transform = transform;
                this.rect = rect;
                this.rrect = rrect;
            }

            public readonly Matrix4x4 transform;

            public readonly Rect rect;

            public readonly RRect rrect;
        }

        private class LayerRec {
            public Rect bounds;
            public Paint paint;
            public RenderTexture texture;
        }

        private class CanvasRec {
            public Matrix4x4 transform;
            public ClipRec clipRect;
            public LayerRec layerRec;
        }
    }
}