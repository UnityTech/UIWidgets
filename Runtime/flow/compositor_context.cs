using System;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class CompositorContext {
        public class ScopedFrame : IDisposable {
            readonly CompositorContext _context;
            readonly Canvas _canvas;
            readonly bool _instrumentation_enabled;

            public ScopedFrame(CompositorContext context, Canvas canvas, bool instrumentation_enabled) {
                this._context = context;
                this._canvas = canvas;
                this._instrumentation_enabled = instrumentation_enabled;
                this._context._beginFrame(this, this._instrumentation_enabled);
            }

            public CompositorContext context() {
                return this._context;
            }

            public Canvas canvas() {
                return this._canvas;
            }

            public bool raster(LayerTree layerTree, bool ignoreRasterCache) {
                layerTree.preroll(this, ignoreRasterCache);
                layerTree.paint(this, ignoreRasterCache);
                return true;
            }

            public void Dispose() {
                this._context._endFrame(this, this._instrumentation_enabled);
            }
        }

        readonly RasterCache _rasterCache;
        readonly Stopwatch _frameTime;

        public CompositorContext() {
            this._rasterCache = new RasterCache();
            this._frameTime = new Stopwatch();
        }

        public ScopedFrame acquireFrame(Canvas canvas, bool instrumentation_enabled) {
            return new ScopedFrame(this, canvas, instrumentation_enabled);
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

        public Stopwatch frameTime() {
            return this._frameTime;
        }

        void _beginFrame(ScopedFrame frame, bool enable_instrumentation) {
            if (enable_instrumentation) {
                this._frameTime.start();
            }
        }

        void _endFrame(ScopedFrame frame, bool enable_instrumentation) {
            this._rasterCache.sweepAfterFrame();
            if (enable_instrumentation) {
                this._frameTime.stop();
            }
        }
    }
}