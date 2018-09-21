using System;
using System.Collections.Generic;
using UIWidgets.painting;
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
        private readonly List<DrawCmd> _drawCmds = new List<DrawCmd>();

        private Matrix4x4 _transform;
        private Rect _clipRect;
        private bool _isInLayer;
        private Rect _paintBounds;

        private Stack<CanvasRec> _stack;

        public PictureRecorder() {
            this._transform = Matrix4x4.identity;
            this._clipRect = null;
            this._isInLayer = false;
            this._paintBounds = null;
        }

        private Stack<CanvasRec> stack {
            get { return this._stack ?? (this._stack = new Stack<CanvasRec>()); }
        }

        public Picture endRecording() {
            if (this._stack != null && this._stack.Count > 0) {
                throw new Exception("unmatched save/restore commands");
            }

            return new Picture(this._drawCmds, this._paintBounds);
        }

        public void addDrawCmd(DrawCmd drawCmd) {
            this._drawCmds.Add(drawCmd);

            if (drawCmd is DrawPloygon4) {
                var drawPloygon4 = (DrawPloygon4) drawCmd;
                if (drawPloygon4.paint.color.alpha > 0) {
                    this.addPaintBounds(drawPloygon4.points);
                }
            } else if (drawCmd is DrawRect) {
                var drawRect = (DrawRect) drawCmd;
                if (drawRect.paint.color.alpha > 0) {
                    this.addPaintBounds(drawRect.rect);
                }
            } else if (drawCmd is DrawRectShadow) {
                var drawRectShadow = (DrawRectShadow) drawCmd;
                if (drawRectShadow.paint.color.alpha > 0) {
                    this.addPaintBounds(drawRectShadow.rect);
                }
            } else if (drawCmd is DrawPicture) {
                var drawPicture = (DrawPicture) drawCmd;
                this.addPaintBounds(drawPicture.picture.paintBounds);
            } else if (drawCmd is DrawConcat) {
                this._transform = ((DrawConcat) drawCmd).transform * this._transform;
            } else if (drawCmd is DrawSave) {
                this.stack.Push(new CanvasRec(
                    this._transform,
                    this._clipRect,
                    this._isInLayer,
                    null
                ));
            } else if (drawCmd is DrawLine)
            {
                var drawLine = (DrawLine) drawCmd;
                Offset vect = drawLine.to - drawLine.from;
                var distance = vect.distance;
                if (distance > 0)
                {
                    var halfWidth = drawLine.paint.strokeWidth * 0.5;
                    var diff = vect / distance * halfWidth;
                    diff = new Offset(diff.dy, -diff.dx);
                    var offsets = new Offset[]
                    {
                        drawLine.from + diff,
                        drawLine.from - diff,
                        drawLine.to + diff,
                        drawLine.to - diff,
                    };

                    var minX = offsets[0].dx;
                    var maxX = offsets[0].dx;
                    var minY = offsets[0].dy;
                    var maxY = offsets[0].dy;
                    for (int i = 1; i < offsets.Length; i++)
                    {
                        minX = Math.Min(minX, offsets[i].dx);
                        maxX = Math.Max(maxX, offsets[i].dx);
                        minY = Math.Min(minY, offsets[i].dy);
                        maxY = Math.Min(maxY, offsets[i].dy);
                    }
                    this.addPaintBounds(Rect.fromLTRB(minX, minY, maxX, maxY));
                }
            }
            else if (drawCmd is DrawSaveLayer) {
                this.stack.Push(new CanvasRec(
                    this._transform,
                    this._clipRect,
                    this._isInLayer,
                    this._paintBounds
                ));

                var drawSaveLayer = (DrawSaveLayer) drawCmd;
                this._transform = Matrix4x4.identity;
                this._clipRect = drawSaveLayer.rect;
                this._isInLayer = true;
                this._paintBounds = null;
            } else if (drawCmd is DrawRestore) {
                var isLayer = this._isInLayer;

                var state = this._stack.Pop();
                this._transform = state.transform;
                this._clipRect = state.clipRect;
                this._isInLayer = state.isInLayer;

                if (isLayer) {
                    var paintBounds = this._paintBounds;
                    this._paintBounds = state.paintBounds;
                    this.addPaintBounds(paintBounds);
                }
            } else if (drawCmd is DrawClipRect) {
                var drawClipRect = (DrawClipRect) drawCmd;
                this.addClipRect(drawClipRect.rect);
            } else if (drawCmd is DrawClipRRect) {
                var drawClipRRect = (DrawClipRRect) drawCmd;
                this.addClipRect(drawClipRRect.rrect.outerRect);
            } else if (drawCmd is DrawTextBlob)
            {
                var drawTextBlob = (DrawTextBlob) drawCmd;
                var bounds = drawTextBlob.textBlob.boundsInText.shift(new Offset(drawTextBlob.x, drawTextBlob.y));
                this.addPaintBounds(bounds);
            } else if (drawCmd is DrawImageRect) {
                var drawImageRect = (DrawImageRect) drawCmd;
                this.addPaintBounds(drawImageRect.src);
            } else {
                throw new Exception("unknown drawCmd: " + drawCmd);
            }
        }

        private void addClipRect(Rect rect) {
            if (rect.isInfinite) {
                return;
            }

            if (this._clipRect != null) {
                throw new Exception("already a clipRec, considering using saveLayer.");
            }
 
            this._clipRect = MatrixUtils.transformRect(this._transform, rect);
        }

        private void addPaintBounds(Rect paintBounds) {
            if (paintBounds == null) {
                return;
            }

            paintBounds = MatrixUtils.transformRect(this._transform, paintBounds);
            if (this._clipRect != null) {
                paintBounds = paintBounds.intersect(this._clipRect);
            }

            this._paintBounds = this._paintBounds == null
                ? paintBounds
                : this._paintBounds.expandToInclude(paintBounds);
        }

        private void addPaintBounds(Offset[] points) {
            var paintBounds = MatrixUtils.transformRect(this._transform, points);
            if (this._clipRect != null) {
                paintBounds = paintBounds.intersect(this._clipRect);
            }

            this._paintBounds = this._paintBounds == null
                ? paintBounds
                : this._paintBounds.expandToInclude(paintBounds);
        }

        private class CanvasRec {
            public CanvasRec(Matrix4x4 transform, Rect clipRect, bool isInLayer, Rect paintBounds) {
                this.transform = transform;
                this.clipRect = clipRect;
                this.isInLayer = isInLayer;
                this.paintBounds = paintBounds;
            }

            public readonly Matrix4x4 transform;
            public readonly Rect clipRect;
            public readonly bool isInLayer;
            public readonly Rect paintBounds;
        }
    }
}