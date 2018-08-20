using System.Collections.Generic;
using UIWidgets.ui;
using Matrix4x4 = UnityEngine.Matrix4x4;

namespace UIWidgets.flow {
    public abstract class ContainerLayer : Layer {
        private readonly List<Layer> _layers = new List<Layer>();

        public List<Layer> layers {
            get { return this._layers; }
        }

        public void add(Layer layer) {
            layer.parent = this;
            this._layers.Add(layer);
        }

        public override void preroll(PrerollContext context, Matrix4x4 matrix) {
            Rect childPaintBounds = Rect.zero;
            this.prerollChildren(context, matrix, ref childPaintBounds);
            this.paintBounds = childPaintBounds;
        }

        protected void prerollChildren(PrerollContext context, Matrix4x4 childMatrix, ref Rect childPaintBounds) {
            foreach (var layer in this._layers) {
                layer.preroll(context, childMatrix);
                childPaintBounds = childPaintBounds.expandToInclude(layer.paintBounds);
            }
        }

        protected void paintChildren(PaintContext context) {
            foreach (var layer in this._layers) {
                if (layer.needsPainting) {
                    layer.paint(context);
                }
            }
        }
    }
}