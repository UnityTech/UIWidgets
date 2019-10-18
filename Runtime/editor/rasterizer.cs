using System;
using Unity.UIWidgets.flow;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.editor {
    public class Rasterizer {
        Surface _surface;
        CompositorContext _compositorContext;
        LayerTree _lastLayerTree;
        Action _nextFrameCallback;

        public Rasterizer() {
            this._compositorContext = new CompositorContext();
        }

        public void setup(Surface surface) {
            this._surface = surface;
            this._compositorContext.onGrContextCreated(this._surface);
        }

        public void teardown() {
            this._compositorContext.onGrContextDestroyed();
            this._surface = null;
            this._lastLayerTree = null;
        }

        public LayerTree getLastLayerTree() {
            return this._lastLayerTree;
        }

        public void drawLastLayerTree() {
            if (this._lastLayerTree == null || this._surface == null) {
                return;
            }

            this._drawToSurface(this._lastLayerTree);
        }

        public void draw(LayerTree layerTree) {
            this._doDraw(layerTree);
        }

        public void setNextFrameCallback(Action callback) {
            this._nextFrameCallback = callback;
        }

        public CompositorContext getCompositorContext() {
            return this._compositorContext;
        }

        void _doDraw(LayerTree layerTree) {
            if (layerTree == null || this._surface == null) {
                return;
            }

            if (this._drawToSurface(layerTree)) {
                this._lastLayerTree = layerTree;
            }
        }

        bool _drawToSurface(LayerTree layerTree) {
            D.assert(this._surface != null);

            var frame = this._surface.acquireFrame(
                layerTree.frameSize, layerTree.devicePixelRatio, layerTree.antiAliasing);
            if (frame == null) {
                return false;
            }

            var canvas = frame.getCanvas();

            using (var compositorFrame = this._compositorContext.acquireFrame(canvas, true)) {
                if (compositorFrame != null && compositorFrame.raster(layerTree, false)) {
                    frame.submit();
                    this._fireNextFrameCallbackIfPresent();
                    return true;
                }
                return false;
            }
        }

        void _fireNextFrameCallbackIfPresent() {
            if (this._nextFrameCallback == null) {
                return;
            }

            var callback = this._nextFrameCallback;
            this._nextFrameCallback = null;
            callback();
        }
    }
}