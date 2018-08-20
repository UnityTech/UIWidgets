using UnityEngine;
using Rect = UIWidgets.ui.Rect;
using Canvas = UIWidgets.ui.Canvas;

namespace UIWidgets.flow {
    public class PrerollContext {
    }

    public class PaintContext {
        public Canvas canvas;
    }

    public abstract class Layer {
        private ContainerLayer _parent;

        public ContainerLayer parent {
            get { return this._parent; }
            set { this._parent = value; }
        }

        private Rect _paintBounds;

        public Rect paintBounds {
            get { return this._paintBounds; }
            set { this._paintBounds = value; }
        }

        public bool needsPainting {
            get { return !this._paintBounds.isEmpty; }
        }

        public virtual void preroll(PrerollContext context, Matrix4x4 matrix) {
        }

        public abstract void paint(PaintContext context);
    }
}