using System;
using System.Collections.Generic;
using System.Linq;
using UIWidgets.foundation;
using UnityEngine;

namespace UIWidgets.ui {
    public class Picture {
        public Picture(List<DrawCmd> drawCmds, Rect paintBounds) {
            this.drawCmds = drawCmds;
            this.paintBounds = paintBounds;
        }

        public readonly List<DrawCmd> drawCmds;
        public readonly Rect paintBounds;
    }

    public class PictureRecorder {
        readonly List<DrawCmd> _drawCmds = new List<DrawCmd>();

        readonly List<CanvasState> _states = new List<CanvasState>();

        public PictureRecorder() {
            this._states.Add(new CanvasState {
                xform = Matrix3.I(),
                scissor = null,
                saveLayer = false,
                layerOffset = null,
                paintBounds = Rect.zero,
            });
        }

        CanvasState _getState() {
            D.assert(this._states.Count > 0);
            return this._states.Last();
        }

        public Picture endRecording() {
            if (this._states.Count > 1) {
                throw new Exception("unmatched save/restore commands");
            }

            var state = this._getState();
            return new Picture(this._drawCmds, state.paintBounds);
        }

        public void addDrawCmd(DrawCmd drawCmd) {
            this._drawCmds.Add(drawCmd);

            if (drawCmd is DrawSave) {
                this._states.Add(this._getState().copy());
            } else if (drawCmd is DrawSaveLayer) {
                var drawSaveLayer = (DrawSaveLayer) drawCmd;

                this._states.Add(new CanvasState {
                    xform = Matrix3.I(),
                    scissor = drawSaveLayer.rect.shift(-drawSaveLayer.rect.topLeft),
                    saveLayer = true,
                    layerOffset = drawSaveLayer.rect.topLeft,
                    paintBounds = Rect.zero,
                });
            } else if (drawCmd is DrawRestore) {
                var stateToRestore = this._getState();
                this._states.RemoveAt(this._states.Count - 1);
                var state = this._getState();

                if (!stateToRestore.saveLayer) {
                    state.paintBounds = stateToRestore.paintBounds;
                } else {
                    var paintBounds = stateToRestore.paintBounds.shift(stateToRestore.layerOffset);
                    paintBounds = state.xform.mapRect(paintBounds);
                    this._addPaintBounds(paintBounds);
                }
            } else if (drawCmd is DrawTranslate) {
                var drawTranslate = (DrawTranslate) drawCmd;
                var state = this._getState();
                state.xform.preTranslate((float) drawTranslate.dx, (float) drawTranslate.dy);
            } else if (drawCmd is DrawScale) {
                var drawScale = (DrawScale) drawCmd;
                var state = this._getState();
                state.xform.preScale((float) drawScale.sx, (float) (drawScale.sy ?? drawScale.sx));
            } else if (drawCmd is DrawRotate) {
                var drawRotate = (DrawRotate) drawCmd;
                var state = this._getState();
                if (drawRotate.offset == null) {
                    state.xform.preRotate((float) drawRotate.radians * Mathf.PI);
                } else {
                    state.xform.preRotate((float) drawRotate.radians * Mathf.PI,
                        (float) drawRotate.offset.dx,
                        (float) drawRotate.offset.dy);
                }
            } else if (drawCmd is DrawSkew) {
                var drawSkew = (DrawSkew) drawCmd;
                var state = this._getState();
                state.xform.preSkew((float) drawSkew.sx, (float) drawSkew.sy);
            } else if (drawCmd is DrawConcat) {
                var drawConcat = (DrawConcat) drawCmd;
                var state = this._getState();
                state.xform.preConcat(drawConcat.matrix);
            } else if (drawCmd is DrawResetMatrix) {
                var state = this._getState();
                state.xform.reset();
            } else if (drawCmd is DrawSetMatrix) {
                var drawSetMatrix = (DrawSetMatrix) drawCmd;
                var state = this._getState();
                state.xform = new Matrix3(drawSetMatrix.matrix);
            } else if (drawCmd is DrawClipRect) {
                var drawClipRect = (DrawClipRect) drawCmd;
                var state = this._getState();

                var rect = state.xform.mapRect(drawClipRect.rect);
                state.scissor = state.scissor == null ? rect : state.scissor.intersect(rect);
            } else if (drawCmd is DrawClipRRect) {
                var drawClipRRect = (DrawClipRRect) drawCmd;
                var state = this._getState();

                var rect = state.xform.mapRect(drawClipRRect.rrect.outerRect);
                state.scissor = state.scissor == null ? rect : state.scissor.intersect(rect);
            } else if (drawCmd is DrawClipPath) {
                var drawClipPath = (DrawClipPath) drawCmd;
                var state = this._getState();

                bool convex;
                var rect = drawClipPath.path.flatten(
                    XformUtils.fromMatrix3(state.xform), (float) Window.instance.devicePixelRatio
                ).getFillMesh(out convex).getBounds();
                state.scissor = state.scissor == null ? rect : state.scissor.intersect(rect);
            } else if (drawCmd is DrawPath) {
                var drawPath = (DrawPath) drawCmd;
                var state = this._getState();
                var xform = XformUtils.fromMatrix3(state.xform);
                var path = drawPath.path;
                var paint = drawPath.paint;
                var devicePixelRatio = (float) Window.instance.devicePixelRatio;

                Mesh mesh;
                if (paint.style == PaintingStyle.fill) {
                    var cache = path.flatten(xform, devicePixelRatio);

                    bool convex;
                    mesh = cache.getFillMesh(out convex);
                } else {
                    float scale = XformUtils.getAverageScale(xform);
                    float strokeWidth = ((float) paint.strokeWidth * scale).clamp(0, 200.0f);
                    float fringeWidth = 1 / devicePixelRatio;

                    if (strokeWidth < fringeWidth) {
                        strokeWidth = fringeWidth;
                    }

                    var cache = path.flatten(xform, devicePixelRatio);
                    mesh = cache.getStrokeMesh(
                        strokeWidth * 0.5f,
                        paint.strokeCap,
                        paint.strokeJoin,
                        (float) paint.strokeMiterLimit);
                }

                this._addPaintBounds(mesh.getBounds());
            } else if (drawCmd is DrawImage) {
                var drawImage = (DrawImage) drawCmd;
                var state = this._getState();
                var rect = Rect.fromLTWH(drawImage.offset.dx, drawImage.offset.dy,
                    drawImage.image.width, drawImage.image.height);
                rect = state.xform.mapRect(rect);
                this._addPaintBounds(rect);
            } else if (drawCmd is DrawImageRect) {
                var drawImageRect = (DrawImageRect) drawCmd;
                var state = this._getState();
                var rect = state.xform.mapRect(drawImageRect.dst);
                this._addPaintBounds(rect);
            } else if (drawCmd is DrawImageNine) {
                var drawImageNine = (DrawImageNine) drawCmd;
                var state = this._getState();
                var rect = state.xform.mapRect(drawImageNine.dst);
                this._addPaintBounds(rect);
            } else if (drawCmd is DrawPicture) {
                var drawPicture = (DrawPicture) drawCmd;
                var state = this._getState();
                var rect = state.xform.mapRect(drawPicture.picture.paintBounds);
                this._addPaintBounds(rect);
            } else if (drawCmd is DrawTextBlob) {
                var drawTextBlob = (DrawTextBlob) drawCmd;
                var state = this._getState();
                var rect = drawTextBlob.textBlob.boundsInText.shift(drawTextBlob.offset);
                rect = state.xform.mapRect(rect);
                this._addPaintBounds(rect);
            } else {
                throw new Exception("unknown drawCmd: " + drawCmd);
            }
        }

        void _addPaintBounds(Rect paintBounds) {
            var state = this._getState();
            if (state.scissor != null) {
                paintBounds = paintBounds.intersect(state.scissor);
            }

            if (state.paintBounds.isEmpty) {
                state.paintBounds = paintBounds;
            } else {
                state.paintBounds = state.paintBounds.expandToInclude(paintBounds);
            }
        }

        class CanvasState {
            public Matrix3 xform;
            public Rect scissor;
            public bool saveLayer;
            public Offset layerOffset;
            public Rect paintBounds;

            public CanvasState copy() {
                return new CanvasState {
                    xform = this.xform,
                    scissor = this.scissor,
                    saveLayer = false,
                    layerOffset = null,
                    paintBounds = this.paintBounds,
                };
            }
        }
    }
}