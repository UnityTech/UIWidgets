using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class LayerTree {
        Layer _rootLayer;

        public Layer rootLayer {
            get { return this._rootLayer; }
            set { this._rootLayer = value; }
        }

        Size _frameSize;

        public Size frameSize {
            get { return this._frameSize; }
            set { this._frameSize = value; }
        }

        float _devicePixelRatio;

        public float devicePixelRatio {
            get { return this._devicePixelRatio; }
            set { this._devicePixelRatio = value; }
        }
        
        int _antiAliasing;

        public int antiAliasing {
            get { return this._antiAliasing; }
            set { this._antiAliasing = value; }
        }

        static readonly Matrix3 _identityMatrix = Matrix3.I();

        public void preroll(CompositorContext.ScopedFrame frame, bool ignoreRasterCache = false) {
            var prerollContext = new PrerollContext {
                rasterCache = ignoreRasterCache ? null : frame.context().rasterCache(),
                devicePixelRatio = frame.canvas().getDevicePixelRatio(),
                antiAliasing = this.antiAliasing,
                cullRect = Rect.largest,
                frameTime = frame.context().frameTime()
            };

            this._rootLayer.preroll(prerollContext, _identityMatrix);
        }

        public void paint(CompositorContext.ScopedFrame frame, bool ignoreRasterCache = false) {
            var paintContext = new PaintContext {
                canvas = frame.canvas(),
                rasterCache = ignoreRasterCache ? null : frame.context().rasterCache(),
                frameTime = frame.context().frameTime()
            };

            if (this._rootLayer.needsPainting) {
                this._rootLayer.paint(paintContext);
            }
        }
    }
}