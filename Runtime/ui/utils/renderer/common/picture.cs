using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;


namespace Unity.UIWidgets.ui {
    public class uiPicture {
        public uiPicture(List<uiDrawCmd> drawCmds, uiRect paintBounds) {
            this.drawCmds = drawCmds;
            this.paintBounds = paintBounds;
        }

        public readonly List<uiDrawCmd> drawCmds;
        public readonly uiRect paintBounds;
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

        public uiMatrix3 getTotalMatrix() {
            return this._getState().xform;
        }

        public void reset() {
            this._drawCmds.Clear();
            this._states.Clear();
            this._states.Add(new uiCanvasState {
                xform = uiMatrix3.I(),
                scissor = null,
                saveLayer = false,
                layerOffset = null,
                paintBounds = uiRectHelper.zero
            });
        }

        public uiPicture endRecording() {
            if (this._states.Count > 1) {
                throw new Exception("unmatched save/restore commands");
            }

            var state = this._getState();            
            return new uiPicture(this._drawCmds, state.paintBounds);
        }

        public void addDrawCmd(uiDrawCmd drawCmd) {
            this._drawCmds.Add(drawCmd);

            switch (drawCmd) {
                case uiDrawSave _:
                    this._states.Add(this._getState().copy());
                    break;
                case uiDrawSaveLayer cmd: {
                    this._states.Add(new uiCanvasState {
                        xform = uiMatrix3.I(),
                        scissor = cmd.rect.Value.shift(-cmd.rect.Value.topLeft),
                        saveLayer = true,
                        layerOffset = cmd.rect.Value.topLeft,
                        paintBounds = uiRectHelper.zero
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
                        var paintBounds = stateToRestore.paintBounds.shift(stateToRestore.layerOffset.Value);
                        paintBounds = state.xform.mapRect(paintBounds);
                        this._addPaintBounds(paintBounds);
                    }
                    break;
                }
                case uiDrawTranslate cmd: {
                    var state = this._getState();
                    state.xform = new uiMatrix3(state.xform);
                    state.xform.preTranslate(cmd.dx, cmd.dy);
                    break;
                }
                case uiDrawScale cmd: {
                    var state = this._getState();
                    state.xform = new uiMatrix3(state.xform);
                    state.xform.preScale(cmd.sx, (cmd.sy ?? cmd.sx));
                    break;
                }
                case uiDrawRotate cmd: {
                    var state = this._getState();
                    state.xform = new uiMatrix3(state.xform);
                    if (cmd.offset == null) {
                        state.xform.preRotate(cmd.radians);
                    } else {
                        state.xform.preRotate(cmd.radians,
                            cmd.offset.Value.dx,
                            cmd.offset.Value.dy);
                    }
                    break;
                }
                case uiDrawSkew cmd: {
                    var state = this._getState();
                    state.xform = new uiMatrix3(state.xform);
                    state.xform.preSkew(cmd.sx, cmd.sy);
                    break;
                }
                case uiDrawConcat cmd: {
                    var state = this._getState();
                    state.xform = new uiMatrix3(state.xform);
                    state.xform.preConcat(cmd.matrix.Value);
                    break;
                }
                case uiDrawResetMatrix _: {
                    var state = this._getState();
                    state.xform = uiMatrix3.I();
                    break;
                }
                case uiDrawSetMatrix cmd: {
                    var state = this._getState();
                    state.xform = new uiMatrix3(cmd.matrix.Value);
                    break;
                }
                case uiDrawClipRect cmd: {
                    var state = this._getState();

                    var rect = state.xform.mapRect(cmd.rect.Value);
                    state.scissor = state.scissor == null ? rect : state.scissor.Value.intersect(rect);
                    break;
                }
                case uiDrawClipRRect cmd: {
                    var state = this._getState();

                    var rect = state.xform.mapRect(uiRectHelper.fromRect(cmd.rrect.outerRect));
                    state.scissor = state.scissor == null ? rect : state.scissor.Value.intersect(rect);
                    break;
                }
                case uiDrawClipPath cmd: {
                    var state = this._getState();
                    var scale = uiXformUtils.getScale(state.xform);

                    var rectPathCache = cmd.path.flatten(
                        scale * Window.instance.devicePixelRatio);
                    var rectMesh = rectPathCache.getFillMesh(out _);
                    var transformedMesh = rectMesh.transform(state.xform);
                    var rect = transformedMesh.bounds;
                    state.scissor = state.scissor == null ? rect : state.scissor.Value.intersect(rect);
                    rectPathCache.dispose();
                    rectMesh.dispose();
                    transformedMesh.dispose();
                    break;
                }
                case uiDrawPath cmd: {
                    var state = this._getState();
                    var scale = uiXformUtils.getScale(state.xform);
                    var path = cmd.path;
                    var paint = cmd.paint;
                    var devicePixelRatio = Window.instance.devicePixelRatio;

                    uiMeshMesh mesh;
                    if (paint.style == PaintingStyle.fill) {
                        var cache = path.flatten(scale * devicePixelRatio);
                        mesh = cache.getFillMesh(out _).transform(state.xform);
                        cache.dispose();
                    } else {
                        float strokeWidth = (paint.strokeWidth * scale).clamp(0, 200.0f);
                        float fringeWidth = 1 / devicePixelRatio;

                        if (strokeWidth < fringeWidth) {
                            strokeWidth = fringeWidth;
                        }

                        var cache = path.flatten(scale * devicePixelRatio);
                        var strokenMesh = cache.getStrokeMesh(
                            strokeWidth / scale * 0.5f,
                            paint.strokeCap,
                            paint.strokeJoin,
                            paint.strokeMiterLimit);
                        
                        mesh = strokenMesh.transform(state.xform);
                        cache.dispose();
                        strokenMesh.dispose();
                    }
                    
                    if (paint.maskFilter != null && paint.maskFilter.sigma != 0) {
                        float sigma = scale * paint.maskFilter.sigma;
                        float sigma3 = 3 * sigma;
                        this._addPaintBounds(uiRectHelper.inflate(mesh.bounds, sigma3));
                    } else {
                        this._addPaintBounds(mesh.bounds);
                    }
                    break;
                }
                case uiDrawImage cmd: {
                    var state = this._getState();
                    var rect = uiRectHelper.fromLTWH(cmd.offset.Value.dx, cmd.offset.Value.dy,
                        cmd.image.width, cmd.image.height);
                    rect = state.xform.mapRect(rect);
                    this._addPaintBounds(rect);
                    break;
                }
                case uiDrawImageRect cmd: {
                    var state = this._getState();
                    var rect = state.xform.mapRect(cmd.dst.Value);
                    this._addPaintBounds(rect);
                    break;
                }
                case uiDrawImageNine cmd: {
                    var state = this._getState();
                    var rect = state.xform.mapRect(cmd.dst.Value);
                    this._addPaintBounds(rect);
                    break;
                }
                case uiDrawPicture cmd: {
                    var state = this._getState();
                    var rect = state.xform.mapRect(uiRectHelper.fromRect(cmd.picture.paintBounds));
                    this._addPaintBounds(rect);
                    break;
                }
                case uiDrawTextBlob cmd: {
                    var state = this._getState();
                    var scale = uiXformUtils.getScale(state.xform);
                    var rect = uiRectHelper.fromRect(cmd.textBlob.boundsInText).shift(cmd.offset.Value);
                    rect = state.xform.mapRect(rect);
                    
                    var paint = cmd.paint;
                    if (paint.maskFilter != null && paint.maskFilter.sigma != 0) {
                        float sigma = scale * paint.maskFilter.sigma;
                        float sigma3 = 3 * sigma;
                        this._addPaintBounds(uiRectHelper.inflate(rect, sigma3));
                    } else {
                        this._addPaintBounds(rect);
                    }
                    
                    break;
                }
                default:
                    throw new Exception("unknown drawCmd: " + drawCmd);
            }
        }

        void _addPaintBounds(uiRect? paintBounds) {
            var state = this._getState();
            if (state.scissor != null) {
                paintBounds = paintBounds.Value.intersect(state.scissor.Value);
            }

            if (paintBounds == null || paintBounds.Value.isEmpty) {
                return;
            }

            if (state.paintBounds.isEmpty) {
                state.paintBounds = paintBounds.Value;
            } else {
                state.paintBounds = state.paintBounds.expandToInclude(paintBounds.Value);
            }
        }

        class uiCanvasState {
            public uiMatrix3 xform;
            public uiRect? scissor;
            public bool saveLayer;
            public uiOffset? layerOffset;
            public uiRect paintBounds;

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