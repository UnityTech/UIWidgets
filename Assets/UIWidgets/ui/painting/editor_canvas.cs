using System;
using System.Collections.Generic;
using UIWidgets.painting;
using UnityEditor;
using UnityEngine;

namespace UIWidgets.ui {
    public class EditorCanvas : Canvas {
        static EditorCanvas() {
            var shader = Shader.Find("UIWidgets/2D Handles Lines");
            if (shader == null) {
                throw new Exception("UIWidgets/2D Handles Lines not found");
            }

            EditorCanvas.linesMat = new Material(shader);
            EditorCanvas.linesMat.hideFlags = HideFlags.HideAndDontSave;

            shader = Shader.Find("UIWidgets/GUIRoundedRect");
            if (shader == null) {
                throw new Exception("UIWidgets/GUIRoundedRect not found");
            }

            EditorCanvas.guiRoundedRectMat = new Material(shader);
            EditorCanvas.guiRoundedRectMat.hideFlags = HideFlags.HideAndDontSave;

            shader = Shader.Find("UIWidgets/GUITextureClip");
            if (shader == null) {
                throw new Exception("UIWidgets/GUITextureClip not found");
            }

            EditorCanvas.guiTextureClipMat = new Material(shader);
            EditorCanvas.guiTextureClipMat.hideFlags = HideFlags.HideAndDontSave;

            shader = Shader.Find("UIWidgets/ShadowRect");
            if (shader == null) {
                throw new Exception("UIWidgets/ShadowRect not found");
            }

            EditorCanvas.shadowRectMat = new Material(shader);
            EditorCanvas.shadowRectMat.hideFlags = HideFlags.HideAndDontSave;
        }

        private static readonly Material linesMat;
        private static readonly Material guiRoundedRectMat;
        private static readonly Material guiTextureClipMat;
        private static readonly Material shadowRectMat;

        private Matrix4x4 _transform;
        private ClipRec _clipRec;
        private LayerRec _layerRec;
        private Stack<CanvasRec> _stack;

        private Stack<CanvasRec> stack {
            get { return this._stack ?? (this._stack = new Stack<CanvasRec>()); }
        }

        public EditorCanvas() {
            this._transform = Matrix4x4.identity;
        }

        public void drawPloygon4(Offset[] points, Paint paint) {
            var color = paint.color;
            if (color.alpha > 0) {
                Vector3[] verts = new Vector3 [points.Length];
                for (int i = 0; i < points.Length; i++) {
                    verts[i] = points[i].toVector();
                }

                this.prepareGL(EditorCanvas.linesMat);
                EditorCanvas.linesMat.SetPass(0);

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
            this.prepareGL(EditorCanvas.guiRoundedRectMat);

            EditorCanvas.guiRoundedRectMat.SetFloatArray("UIWidgets_BorderWidth",
                borderWidth == null ? new[] {0f, 0f, 0f, 0f} : borderWidth.toFloatArray());
            EditorCanvas.guiRoundedRectMat.SetFloatArray("UIWidgets_CornerRadius",
                borderRadius == null ? new[] {0f, 0f, 0f, 0f} : borderRadius.toFloatArray());

            Graphics.DrawTexture(rect.toRect(), EditorGUIUtility.whiteTexture,
                new UnityEngine.Rect(0.0f, 0.0f, 1f, 1f), 0, 0, 0, 0,
                paint.color.toColor(), EditorCanvas.guiRoundedRectMat);
        }

        public void drawRectShadow(Rect rect, Paint paint) {
            this.prepareGL(EditorCanvas.shadowRectMat);

            EditorCanvas.shadowRectMat.SetFloat("UIWidgets_sigma", (float) paint.blurSigma);

            Graphics.DrawTexture(rect.toRect(), EditorGUIUtility.whiteTexture,
                new UnityEngine.Rect(0.0f, 0.0f, 1f, 1f), 0, 0, 0, 0,
                paint.color.toColor(), EditorCanvas.shadowRectMat);
        }

        public void drawPicture(Picture picture) {
            var drawCmds = picture.drawCmds;
            foreach (var drawCmd in drawCmds) {
                if (drawCmd is DrawPloygon4) {
                    var drawPloygon4 = (DrawPloygon4) drawCmd;
                    this.drawPloygon4(drawPloygon4.points, drawPloygon4.paint);
                } else if (drawCmd is DrawRect) {
                    var drawRect = (DrawRect) drawCmd;
                    this.drawRect(drawRect.rect, drawRect.borderWidth, drawRect.borderRadius, drawRect.paint);
                } else if (drawCmd is DrawRectShadow) {
                    var drawRectShadow = (DrawRectShadow) drawCmd;
                    this.drawRectShadow(drawRectShadow.rect, drawRectShadow.paint);
                } else if (drawCmd is DrawPicture) {
                    var drawPicture = (DrawPicture) drawCmd;
                    this.drawPicture(drawPicture.picture);
                } else if (drawCmd is DrawConcat) {
                    this.concat(((DrawConcat) drawCmd).transform);
                } else if (drawCmd is DrawSave) {
                    this.save();
                } else if (drawCmd is DrawSaveLayer) {
                    var drawSaveLayer = (DrawSaveLayer) drawCmd;
                    this.saveLayer(drawSaveLayer.rect, drawSaveLayer.paint);
                } else if (drawCmd is DrawRestore) {
                    this.restore();
                } else if (drawCmd is DrawClipRect) {
                    var drawClipRect = (DrawClipRect) drawCmd;
                    this.clipRect(drawClipRect.rect);
                } else if (drawCmd is DrawClipRRect) {
                    var drawClipRRect = (DrawClipRRect) drawCmd;
                    this.clipRRect(drawClipRRect.rrect);
                } else {
                    throw new Exception("unknown drawCmd: " + drawCmd);
                }
            }
        }

        public void concat(Matrix4x4 transform) {
            this._transform = transform * this._transform;
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

            var textureWidth = (int) Math.Round(bounds.width * EditorGUIUtility.pixelsPerPoint);
            var textureHeight = (int) Math.Round(bounds.height * EditorGUIUtility.pixelsPerPoint);

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
            var layerRect = this._layerRec;

            var state = this._stack.Pop();
            this._transform = state.transform;
            this._clipRec = state.clipRect;
            this._layerRec = state.layerRec;

            RenderTexture.active = this._layerRec != null ? this._layerRec.texture : null;
            GL.PopMatrix();

            if (layerRect != null) {
                this.prepareGL(EditorCanvas.guiTextureClipMat);

                Graphics.DrawTexture(layerRect.bounds.toRect(), layerRect.texture,
                    new UnityEngine.Rect(0.0f, 0.0f, 1f, 1f), 0, 0, 0, 0,
                    layerRect.paint.color.toColor(), EditorCanvas.guiTextureClipMat);

                RenderTexture.ReleaseTemporary(layerRect.texture);
                layerRect.texture = null;
            }
        }

        public void clipRect(Rect rect) {
            if (rect.isInfinite) {
                return;
            }

            this.pushClipRect(rect, this._transform);
        }

        public void clipRRect(RRect rect) {
            if (rect.isInfinite) {
                return;
            }

            this.pushClipRRect(rect, this._transform);
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