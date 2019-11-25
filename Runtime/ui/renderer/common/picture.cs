using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.Runtime.external;

namespace Unity.UIWidgets.ui {
    public class uiPicture : PoolObject {
        public uiPicture() {
        }

        public static uiPicture create(List<uiDrawCmd> drawCmds,
            uiRect paintBounds,
            BBoxHierarchy<IndexedRect> bbh = null,
            uiList<int> stateUpdateIndices = null) {
            var picture = ObjectPool<uiPicture>.alloc();
            picture.drawCmds = drawCmds;
            picture.paintBounds = paintBounds;
            picture.bbh = bbh;
            picture.stateUpdatesIndices = stateUpdateIndices;
            return picture;
        }

        public List<uiDrawCmd> drawCmds;
        public uiRect paintBounds;
        public BBoxHierarchy<IndexedRect> bbh; 
        public uiList<int> stateUpdatesIndices; 

        public override void clear() {
            //the recorder will dispose the draw commands
            this.drawCmds = null;
            ObjectPool<uiList<int>>.release(this.stateUpdatesIndices);
        }
    }

    public class uiPictureRecorder {
        readonly List<uiDrawCmd> _drawCmds = new List<uiDrawCmd>(128);

        readonly List<uiCanvasState> _states = new List<uiCanvasState>(32);
        
        readonly BBoxHierarchy<IndexedRect> _bbh = new RTree<IndexedRect>();

        readonly List<int> _stateUpdateIndices = new List<int>();

        public uiPictureRecorder() {
            this.reset();
        }

        uiCanvasState _getState() {
            D.assert(this._states.Count > 0);
            return this._states[this._states.Count - 1];
        }

        void _setState(uiCanvasState state) {
            this._states[this._states.Count - 1] = state;
        }

        public uiMatrix3 getTotalMatrix() {
            return this._getState().xform;
        }

        public void reset() {
            foreach (var drawCmd in this._drawCmds) {
                drawCmd.release();
            }

            this._drawCmds.Clear();
            this._states.Clear();
            this._states.Add(new uiCanvasState {
                xform = uiMatrix3.I(),
                scissor = null,
                saveLayer = false,
                layerOffset = null,
                paintBounds = uiRectHelper.zero
            });
            this._bbh.Clear();
            this._stateUpdateIndices.Clear();
        }

        public uiPicture endRecording() {
            if (this._states.Count > 1) {
                throw new Exception("unmatched save/restore commands");
            }
            
            uiList<int> stateUpdateIndices = ObjectPool<uiList<int>>.alloc();
            stateUpdateIndices.AddRange(this._stateUpdateIndices);
            var state = this._getState();
            return uiPicture.create(
                this._drawCmds,
                state.paintBounds,
                bbh: this._bbh,
                stateUpdateIndices: stateUpdateIndices);
        }

        public void addDrawCmd(uiDrawCmd drawCmd) {
            this._drawCmds.Add(drawCmd);

            switch (drawCmd) {
                case uiDrawSave _:
                    this._states.Add(this._getState().copy());
                    this._stateUpdateIndices.Add(this._drawCmds.Count - 1);
                    break;
                case uiDrawSaveLayer cmd: {
                    this._states.Add(new uiCanvasState {
                        xform = uiMatrix3.I(),
                        scissor = cmd.rect.Value.shift(-cmd.rect.Value.topLeft),
                        saveLayer = true,
                        layerOffset = cmd.rect.Value.topLeft,
                        paintBounds = uiRectHelper.zero
                    });
                    this._stateUpdateIndices.Add(this._drawCmds.Count - 1);
                    break;
                }
                case uiDrawRestore _: {
                    var stateToRestore = this._getState();
                    this._states.RemoveAt(this._states.Count - 1);
                    var state = this._getState();

                    if (!stateToRestore.saveLayer) {
                        state.paintBounds = stateToRestore.paintBounds;
                    }
                    else {
                        var paintBounds = stateToRestore.paintBounds.shift(stateToRestore.layerOffset.Value);
                        paintBounds = state.xform.mapRect(paintBounds);
                        this._addPaintBounds(paintBounds);
                    }

                    this._setState(state);
                    this._stateUpdateIndices.Add(this._drawCmds.Count - 1);
                    break;
                }
                case uiDrawTranslate cmd: {
                    var state = this._getState();
                    state.xform = new uiMatrix3(state.xform);
                    state.xform.preTranslate(cmd.dx, cmd.dy);
                    this._setState(state);
                    this._stateUpdateIndices.Add(this._drawCmds.Count - 1);
                    break;
                }
                case uiDrawScale cmd: {
                    var state = this._getState();
                    state.xform = new uiMatrix3(state.xform);
                    state.xform.preScale(cmd.sx, (cmd.sy ?? cmd.sx));
                    this._setState(state);
                    this._stateUpdateIndices.Add(this._drawCmds.Count - 1);
                    break;
                }
                case uiDrawRotate cmd: {
                    var state = this._getState();
                    state.xform = new uiMatrix3(state.xform);
                    if (cmd.offset == null) {
                        state.xform.preRotate(cmd.radians);
                    }
                    else {
                        state.xform.preRotate(cmd.radians,
                            cmd.offset.Value.dx,
                            cmd.offset.Value.dy);
                    }

                    this._setState(state);
                    this._stateUpdateIndices.Add(this._drawCmds.Count - 1);
                    break;
                }
                case uiDrawSkew cmd: {
                    var state = this._getState();
                    state.xform = new uiMatrix3(state.xform);
                    state.xform.preSkew(cmd.sx, cmd.sy);
                    this._setState(state);
                    this._stateUpdateIndices.Add(this._drawCmds.Count - 1);
                    break;
                }
                case uiDrawConcat cmd: {
                    var state = this._getState();
                    state.xform = new uiMatrix3(state.xform);
                    state.xform.preConcat(cmd.matrix.Value);
                    this._setState(state);
                    this._stateUpdateIndices.Add(this._drawCmds.Count - 1);
                    break;
                }
                case uiDrawResetMatrix _: {
                    var state = this._getState();
                    state.xform = uiMatrix3.I();
                    this._setState(state);
                    this._stateUpdateIndices.Add(this._drawCmds.Count - 1);
                    break;
                }
                case uiDrawSetMatrix cmd: {
                    var state = this._getState();
                    state.xform = new uiMatrix3(cmd.matrix.Value);
                    this._setState(state);
                    this._stateUpdateIndices.Add(this._drawCmds.Count - 1);
                    break;
                }
                case uiDrawClipRect cmd: {
                    var state = this._getState();

                    var rect = state.xform.mapRect(cmd.rect.Value);
                    state.scissor = state.scissor == null ? rect : state.scissor.Value.intersect(rect);
                    this._setState(state);
                    this._stateUpdateIndices.Add(this._drawCmds.Count - 1);
                    break;
                }
                case uiDrawClipRRect cmd: {
                    var state = this._getState();

                    var rect = state.xform.mapRect(uiRectHelper.fromRect(cmd.rrect.outerRect).Value);
                    state.scissor = state.scissor == null ? rect : state.scissor.Value.intersect(rect);
                    this._setState(state);
                    this._stateUpdateIndices.Add(this._drawCmds.Count - 1);
                    break;
                }
                case uiDrawClipPath cmd: {
                    var state = this._getState();
                    var scale = uiXformUtils.getScale(state.xform);

                    var rectPathCache = cmd.path.flatten(
                        scale * Window.instance.devicePixelRatio);
                    rectPathCache.computeFillMesh(0.0f, out _);
                    var rectMesh = rectPathCache.fillMesh;
                    var transformedMesh = rectMesh.transform(state.xform);
                    var rect = transformedMesh.bounds;
                    state.scissor = state.scissor == null ? rect : state.scissor.Value.intersect(rect);
                    this._setState(state);
                    ObjectPool<uiMeshMesh>.release(transformedMesh);
                    this._stateUpdateIndices.Add(this._drawCmds.Count - 1);
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
                        cache.computeFillMesh(0.0f, out _);
                        var fillMesh = cache.fillMesh;
                        mesh = fillMesh.transform(state.xform);
                    }
                    else {
                        float strokeWidth = (paint.strokeWidth * scale).clamp(0, 200.0f);
                        float fringeWidth = 1 / devicePixelRatio;

                        if (strokeWidth < fringeWidth) {
                            strokeWidth = fringeWidth;
                        }

                        var cache = path.flatten(scale * devicePixelRatio);
                        cache.computeStrokeMesh(
                            strokeWidth / scale * 0.5f,
                            0.0f,
                            paint.strokeCap,
                            paint.strokeJoin,
                            paint.strokeMiterLimit);
                        var strokenMesh = cache.strokeMesh;

                        mesh = strokenMesh.transform(state.xform);
                    }

                    if (paint.maskFilter != null && paint.maskFilter.Value.sigma != 0) {
                        float sigma = scale * paint.maskFilter.Value.sigma;
                        float sigma3 = 3 * sigma;
                        this._addPaintBounds(uiRectHelper.inflate(mesh.bounds, sigma3));
                        this._bbh.Insert(new IndexedRect(uiRectHelper.inflate(mesh.bounds, sigma3 + 5),
                            this._drawCmds.Count - 1));
                    }
                    else {
                        this._addPaintBounds(mesh.bounds);
                        this._bbh.Insert(new IndexedRect(uiRectHelper.inflate(mesh.bounds, 5),
                            this._drawCmds.Count - 1));
                    }

                    ObjectPool<uiMeshMesh>.release(mesh);
                    break;
                }
                case uiDrawImage cmd: {
                    var state = this._getState();
                    var rect = uiRectHelper.fromLTWH(cmd.offset.Value.dx, cmd.offset.Value.dy,
                        cmd.image.width, cmd.image.height);
                    rect = state.xform.mapRect(rect);
                    this._addPaintBounds(rect);
                    this._bbh.Insert(new IndexedRect(uiRectHelper.inflate(rect, 5),
                        this._drawCmds.Count - 1));
                    break;
                }
                case uiDrawImageRect cmd: {
                    var state = this._getState();
                    var rect = state.xform.mapRect(cmd.dst.Value);
                    this._addPaintBounds(rect);
                    this._bbh.Insert(new IndexedRect(uiRectHelper.inflate(rect, 5),
                        this._drawCmds.Count - 1));
                    break;
                }
                case uiDrawImageNine cmd: {
                    var state = this._getState();
                    var rect = state.xform.mapRect(cmd.dst.Value);
                    this._addPaintBounds(rect);
                    this._bbh.Insert(new IndexedRect(uiRectHelper.inflate(rect, 5),
                        this._drawCmds.Count - 1));
                    break;
                }
                case uiDrawPicture cmd: {
                    var state = this._getState();
                    var rect = state.xform.mapRect(uiRectHelper.fromRect(cmd.picture.paintBounds).Value);
                    this._addPaintBounds(rect);
                    this._bbh.Insert(new IndexedRect(uiRectHelper.inflate(rect, 5),
                        this._drawCmds.Count - 1));
                    break;
                }
                case uiDrawTextBlob cmd: {
                    var state = this._getState();
                    var scale = uiXformUtils.getScale(state.xform);
                    var rect = uiRectHelper.fromRect(
                        cmd.textBlob.Value.shiftedBoundsInText(cmd.offset.Value.dx, cmd.offset.Value.dy)).Value;
                    rect = state.xform.mapRect(rect);
                    this._bbh.Insert(new IndexedRect(uiRectHelper.inflate(rect, 5),
                        this._drawCmds.Count - 1));

                    var paint = cmd.paint;
                    if (paint.maskFilter != null && paint.maskFilter.Value.sigma != 0) {
                        float sigma = scale * paint.maskFilter.Value.sigma;
                        float sigma3 = 3 * sigma;
                        this._addPaintBounds(uiRectHelper.inflate(rect, sigma3));
                        this._bbh.Insert(new IndexedRect(uiRectHelper.inflate(rect, sigma3 + 5),
                            this._drawCmds.Count - 1));
                    }
                    else {
                        this._addPaintBounds(rect);
                        this._bbh.Insert(new IndexedRect(uiRectHelper.inflate(rect, 5),
                            this._drawCmds.Count - 1));
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
            }
            else {
                state.paintBounds = state.paintBounds.expandToInclude(paintBounds.Value);
            }

            this._setState(state);
        }

        struct uiCanvasState {
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
                    paintBounds = this.paintBounds
                };
            }
        }
    }
}