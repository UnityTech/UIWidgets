using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public partial class PictureFlusher {
        internal class RenderLayer : PoolObject {
            public int rtID;
            public int width;
            public int height;
            public FilterMode filterMode = FilterMode.Bilinear;
            public bool noMSAA = false;
            public uiRect layerBounds;
            public uiPaint? layerPaint;
            public readonly List<RenderCmd> draws = new List<RenderCmd>(128);
            public readonly List<RenderLayer> layers = new List<RenderLayer>(16);
            public readonly List<State> states = new List<State>(16);
            public State currentState;
            public ClipStack clipStack;
            public uint lastClipGenId;
            public uiRect lastClipBounds;
            public bool ignoreClip = true;

            Vector4? _viewport;

            public Vector4 viewport {
                get {
                    if (!this._viewport.HasValue) {
                        this._viewport = new Vector4(
                            this.layerBounds.left,
                            this.layerBounds.top,
                            this.layerBounds.width,
                            this.layerBounds.height);
                    }

                    return this._viewport.Value;
                }
            }

            public static RenderLayer create(int rtID = 0, int width = 0, int height = 0,
                FilterMode filterMode = FilterMode.Bilinear,
                bool noMSAA = false, 
                uiRect? layerBounds = null, uiPaint? layerPaint = null, bool ignoreClip = true) {
                D.assert(layerBounds != null);
                var newLayer = ObjectPool<RenderLayer>.alloc();
                newLayer.rtID = rtID;
                newLayer.width = width;
                newLayer.height = height;
                newLayer.filterMode = filterMode;
                newLayer.noMSAA = noMSAA;
                newLayer.layerBounds = layerBounds.Value;
                newLayer.layerPaint = layerPaint;
                newLayer.ignoreClip = ignoreClip;
                newLayer.currentState = State.create();
                newLayer.states.Add(newLayer.currentState);
                newLayer.clipStack = ClipStack.create();

                return newLayer;
            }

            public void addLayer(RenderLayer layer) {
                this.layers.Add(layer);
                this.draws.Add(CmdLayer.create(layer: layer));
            }

            public override void clear() {
                //these two list should have been cleared in PictureFlusher._clearLayer
                D.assert(this.draws.Count == 0);
                D.assert(this.layers.Count == 0);
                this.draws.Clear();
                this.layers.Clear();

                foreach (var state in this.states) {
                    ObjectPool<State>.release(state);
                }

                this.states.Clear();
                ObjectPool<ClipStack>.release(this.clipStack);
                this._viewport = null;
            }
        }

        internal class State : PoolObject {
            public State() {
            }

            static readonly uiMatrix3 _id = uiMatrix3.I();

            uiMatrix3? _matrix;
            float? _scale;
            uiMatrix3? _invMatrix;

            public static State create(uiMatrix3? matrix = null, float? scale = null, uiMatrix3? invMatrix = null) {
                State newState = ObjectPool<State>.alloc();
                newState._matrix = matrix ?? _id;
                newState._scale = scale;
                newState._invMatrix = invMatrix;

                return newState;
            }

            public override void clear() {
                this._matrix = null;
                this._scale = null;
                this._invMatrix = null;
            }

            public uiMatrix3? matrix {
                get { return this._matrix; }
                set {
                    this._matrix = value ?? _id;
                    this._scale = null;
                    this._invMatrix = null;
                }
            }

            public float scale {
                get {
                    if (this._scale == null) {
                        this._scale = uiXformUtils.getScale(this._matrix.Value);
                    }

                    return this._scale.Value;
                }
            }

            public uiMatrix3 invMatrix {
                get {
                    if (this._invMatrix == null) {
                        this._invMatrix = this._matrix.Value.invert();
                    }

                    return this._invMatrix.Value;
                }
            }

            public State copy() {
                return create(this._matrix, this._scale, this._invMatrix);
            }
        }
    }
}