using System;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class CompositorContext {
        public class ScopedFrame : IDisposable {
            readonly CompositorContext _context;
            readonly Canvas _canvas;

            public ScopedFrame(CompositorContext context, Canvas canvas) {
                this._context = context;
                this._canvas = canvas;

                this._context._beginFrame(this);
            }

            public CompositorContext context() {
                return this._context;
            }

            public Canvas canvas() {
                return this._canvas;
            }

            public bool raster(LayerTree layerTree, bool ignoreRasterCache) {
                layerTree.preroll(this, ignoreRasterCache);
                layerTree.paint(this);
                return true;
            }

            public void Dispose() {
                this._context._endFrame(this);
            }
        }

        readonly RasterCache _rasterCache;

        public CompositorContext() {
            this._rasterCache = new RasterCache();
        }

        public ScopedFrame acquireFrame(Canvas canvas) {
            return new ScopedFrame(this, canvas);
        }

        public void onGrContextCreated(Surface surface) {
            this._rasterCache.clear();
            this._rasterCache.meshPool = surface.getMeshPool();
        }

        public void onGrContextDestroyed() {
            this._rasterCache.clear();
        }

        public RasterCache rasterCache() {
            return this._rasterCache;
        }

        void _beginFrame(ScopedFrame frame) {
        }

        void _endFrame(ScopedFrame frame) {
            this._rasterCache.sweepAfterFrame();
        }
    }
}