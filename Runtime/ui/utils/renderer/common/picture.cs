using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;


namespace Unity.UIWidgets.ui {
    public class uiPicture {
        public uiPicture(List<uiDrawCmd> drawCmds, Rect paintBounds) {
            this.drawCmds = drawCmds;
            this.paintBounds = paintBounds;
        }

        public readonly List<uiDrawCmd> drawCmds;
        public readonly Rect paintBounds;
    }

    public class uiPictureRecorder {
        readonly List<uiDrawCmd> _drawCmds = new List<uiDrawCmd>();

        readonly List<uiCanvasState> _states = new List<uiCanvasState>();

        public uiPictureRecorder() {
            this.reset();
        }

        uiCanvasState _getState() {
            D.assert(this._states.Count > 0);
            return this._states[this._states.Count - 1];
        }

        public Matrix3 getTotalMatrix() {
            return this._getState().xform;
        }

        public void reset() {
            this._drawCmds.Clear();
            this._states.Clear();
            this._states.Add(new uiCanvasState {
                xform = Matrix3.I(),
                scissor = null,
                saveLayer = false,
                layerOffset = null,
                paintBounds = Rect.zero,
            });
        }

        public uiPicture endRecording() {
            if (this._states.Count > 1) {
                throw new Exception("unmatched save/restore commands");
            }

            var state = this._getState();            
            return new uiPicture(new List<uiDrawCmd>(this._drawCmds), state.paintBounds);
        }

        public void addDrawCmd(uiDrawCmd drawCmd) {
            this._drawCmds.Add(drawCmd);

            switch (drawCmd) {
                case uiDrawSave _:
                    this._states.Add(this._getState().copy());
                    break;
                case uiDrawSaveLayer cmd: {
                    this._states.Add(new uiCanvasState {
                        xform = Matrix3.I(),
                        scissor = cmd.rect.shift(-cmd.rect.topLeft),
                        saveLayer = true,
                        layerOffset = cmd.rect.topLeft,
                        paintBounds = Rect.zero,
                    });
                    break;
                }
                case uiDrawRestore _: {
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
                    break;
                }
                case uiDrawTranslate cmd: {
                    var state = this._getState();
                    state.xform = new Matrix3(state.xform);
                    state.xform.preTranslate(cmd.dx, cmd.dy);
                    break;
                }
                case uiDrawScale cmd: {
                    var state = this._getState();
                    state.xform = new Matrix3(state.xform);
                    state.xform.preScale(cmd.sx, (cmd.sy ?? cmd.sx));
                    break;
                }
                case uiDrawRotate cmd: {
                    var state = this._getState();
                    state.xform = new Matrix3(state.xform);
                    if (cmd.offset == null) {
                        state.xform.preRotate(cmd.radians);
                    } else {
                        state.xform.preRotate(cmd.radians,
                            cmd.offset.dx,
                            cmd.offset.dy);
                    }
                    break;
                }
                case uiDrawSkew cmd: {
                    var state = this._getState();
                    state.xform = new Matrix3(state.xform);
                    state.xform.preSkew(cmd.sx, cmd.sy);
                    break;
                }
                case uiDrawConcat cmd: {
                    var state = this._getState();
                    state.xform = new Matrix3(state.xform);
                    state.xform.preConcat(cmd.matrix);
                    break;
                }
                case uiDrawResetMatrix _: {
                    var state = this._getState();
                    state.xform = Matrix3.I();
                    break;
                }
                case uiDrawSetMatrix cmd: {
                    var state = this._getState();
                    state.xform = new Matrix3(cmd.matrix);
                    break;
                }
                case uiDrawClipRect cmd: {
                    var state = this._getState();

                    var rect = state.xform.mapRect(cmd.rect);
                    state.scissor = state.scissor == null ? rect : state.scissor.intersect(rect);
                    break;
                }
                case uiDrawClipRRect cmd: {
                    var state = this._getState();

                    var rect = state.xform.mapRect(cmd.rrect.outerRect);
                    state.scissor = state.scissor == null ? rect : state.scissor.intersect(rect);
                    break;
                }
                case uiDrawClipPath cmd: {
                    var state = this._getState();
                    var scale = XformUtils.getScale(state.xform);

                    var rect = cmd.path.flatten(
                        scale * Window.instance.devicePixelRatio
                    ).getFillMesh(out _).transform(state.xform).bounds;
                    state.scissor = state.scissor == null ? rect : state.scissor.intersect(rect);
                    break;
                }
                case uiDrawPath cmd: {
                    var state = this._getState();
                    var scale = XformUtils.getScale(state.xform);
                    var path = cmd.path;
                    var paint = cmd.paint;
                    var devicePixelRatio = Window.instance.devicePixelRatio;

                    MeshMesh mesh;
                    if (paint.style == PaintingStyle.fill) {
                        var cache = path.flatten(scale * devicePixelRatio);
                        mesh = cache.getFillMesh(out _).transform(state.xform);
                    } else {
                        float strokeWidth = (paint.strokeWidth * scale).clamp(0, 200.0f);
                        float fringeWidth = 1 / devicePixelRatio;

                        if (strokeWidth < fringeWidth) {
                            strokeWidth = fringeWidth;
                        }

                        var cache = path.flatten(scale * devicePixelRatio);
                        mesh = cache.getStrokeMesh(
                            strokeWidth / scale * 0.5f,
                            paint.strokeCap,
                            paint.strokeJoin,
                            paint.strokeMiterLimit).transform(state.xform);
                    }
                    
                    if (paint.maskFilter != null && paint.maskFilter.sigma != 0) {
                        float sigma = scale * paint.maskFilter.sigma;
                        float sigma3 = 3 * sigma;
                        this._addPaintBounds(mesh.bounds.inflate(sigma3));
                    } else {
                        this._addPaintBounds(mesh.bounds);
                    }
                    break;
                }
                case uiDrawImage cmd: {
                    var state = this._getState();
                    var rect = Rect.fromLTWH(cmd.offset.dx, cmd.offset.dy,
                        cmd.image.width, cmd.image.height);
                    rect = state.xform.mapRect(rect);
                    this._addPaintBounds(rect);
                    break;
                }
                case uiDrawImageRect cmd: {
                    var state = this._getState();
                    var rect = state.xform.mapRect(cmd.dst);
                    this._addPaintBounds(rect);
                    break;
                }
                case uiDrawImageNine cmd: {
                    var state = this._getState();
                    var rect = state.xform.mapRect(cmd.dst);
                    this._addPaintBounds(rect);
                    break;
                }
                case uiDrawPicture cmd: {
                    var state = this._getState();
                    var rect = state.xform.mapRect(cmd.picture.paintBounds);
                    this._addPaintBounds(rect);
                    break;
                }
                case uiDrawTextBlob cmd: {
                    var state = this._getState();
                    var scale = XformUtils.getScale(state.xform);
                    var rect = cmd.textBlob.boundsInText.shift(cmd.offset);
                    rect = state.xform.mapRect(rect);
                    
                    var paint = cmd.paint;
                    if (paint.maskFilter != null && paint.maskFilter.sigma != 0) {
                        float sigma = scale * paint.maskFilter.sigma;
                        float sigma3 = 3 * sigma;
                        this._addPaintBounds(rect.inflate(sigma3));
                    } else {
                        this._addPaintBounds(rect);
                    }
                    
                    break;
                }
                default:
                    throw new Exception("unknown drawCmd: " + drawCmd);
            }
        }

        void _addPaintBounds(Rect paintBounds) {
            var state = this._getState();
            if (state.scissor != null) {
                paintBounds = paintBounds.intersect(state.scissor);
            }

            if (paintBounds == null || paintBounds.isEmpty) {
                return;
            }

            if (state.paintBounds.isEmpty) {
                state.paintBounds = paintBounds;
            } else {
                state.paintBounds = state.paintBounds.expandToInclude(paintBounds);
            }
        }

        class uiCanvasState {
            public Matrix3 xform;
            public Rect scissor;
            public bool saveLayer;
            public Offset layerOffset;
            public Rect paintBounds;

            public uiCanvasState copy() {
                return new uiCanvasState {
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