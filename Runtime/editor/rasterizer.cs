using System;
using Unity.UIWidgets.flow;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

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

            PathOptimizer.cmdNum = 0;

            var frame = this._surface.acquireFrame(
                layerTree.frameSize, layerTree.devicePixelRatio, layerTree.antiAliasing);
            if (frame == null) {
                return false;
            }

            var canvas = frame.getCanvas();

            using (var compositorFrame = this._compositorContext.acquireFrame(canvas, true)) {

                PathOptimizer.optimizing = true;
                if (compositorFrame != null && compositorFrame.raster(layerTree, false)) {
                    
                    
                    frame.submit();

                    PathOptimizer.optimizing = false;
                    Flash<PathPath>.instance.clearAll();
                    Flash<PathPoint>.instance.clearAll();
                    Flash<Vector3>.instance.clearAll();
                    Flash<int>.instance.clearAll();
                    Flash<float>.instance.clearAll();
                    SimpleFlash<PathPoint>.instance.clearAll();
                    SimpleFlash<Matrix3>.instance.clearAll();
                    SimpleFlash<CanvasState>.instance.clearAll();
                    SimpleFlash<PathPath>.instance.clearAll();
                    SimpleFlash<Rect>.instance.clearAll();
                    ClearableSimpleFlash<PictureFlusher.CmdDraw>.instance.clearAll();
                    ClearableMaterialPropFlash.instance.clearAll();
                    Flash<object>.instance.clearAll();
                    Flash<PictureFlusher.RenderLayer>.instance.clearAll();
                    Flash<PictureFlusher.State>.instance.clearAll();
                    SimpleFlash<PictureFlusher.State>.instance.clearAll();
                    
                    this._fireNextFrameCallbackIfPresent();
                    
                    //Debug.Log(PathOptimizer.cmdNum);

                    return true;
                }

                PathOptimizer.optimizing = false;
                Flash<PathPath>.instance.clearAll();
                Flash<PathPoint>.instance.clearAll();
                Flash<Vector3>.instance.clearAll();
                Flash<int>.instance.clearAll();
                Flash<float>.instance.clearAll();
                SimpleFlash<PathPoint>.instance.clearAll();
                SimpleFlash<Matrix3>.instance.clearAll();
                SimpleFlash<CanvasState>.instance.clearAll();
                SimpleFlash<PathPath>.instance.clearAll();
                SimpleFlash<Rect>.instance.clearAll();
                ClearableSimpleFlash<PictureFlusher.CmdDraw>.instance.clearAll();
                ClearableMaterialPropFlash.instance.clearAll();
                Flash<object>.instance.clearAll();
                Flash<PictureFlusher.RenderLayer>.instance.clearAll();
                Flash<PictureFlusher.State>.instance.clearAll();
                SimpleFlash<PictureFlusher.State>.instance.clearAll();
                
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