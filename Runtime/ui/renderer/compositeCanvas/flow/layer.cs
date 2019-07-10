using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class PrerollContext {
        public RasterCache rasterCache;
        public float devicePixelRatio;
        public int antiAliasing;
        public Rect cullRect;
        public Stopwatch frameTime;
    }

    public class PaintContext {
        public Canvas canvas;
        public RasterCache rasterCache;
        public Stopwatch frameTime;
    }

    public abstract class Layer {
        ContainerLayer _parent;

        public ContainerLayer parent {
            get { return this._parent; }
            set { this._parent = value; }
        }

        Rect _paintBounds = Rect.zero;

        public Rect paintBounds {
            get { return this._paintBounds; }
            set { this._paintBounds = value ?? Rect.zero; }
        }

        public bool needsPainting {
            get { return !this._paintBounds.isEmpty; }
        }

        public virtual void preroll(PrerollContext context, Matrix3 matrix) {
        }

        public abstract void paint(PaintContext context);
    }

    static class LayerUtils {
        public static void alignToPixel(this Canvas canvas) {
            var matrix = canvas.getTotalMatrix();
            var devicePixelRatio = canvas.getDevicePixelRatio();
            var x = matrix[2].alignToPixel(devicePixelRatio);
            var y = matrix[5].alignToPixel(devicePixelRatio);
            if (x != matrix[2] || y != matrix[5]) {
                canvas.translate(x - matrix[2], y - matrix[5]);
            }
        }
    }
}