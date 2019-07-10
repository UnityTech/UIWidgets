using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public abstract class ContainerLayer : Layer {
        readonly List<Layer> _layers = new List<Layer>();

        public List<Layer> layers {
            get { return this._layers; }
        }

        public void add(Layer layer) {
            layer.parent = this;
            this._layers.Add(layer);
        }

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            Rect childPaintBounds = Rect.zero;
            this.prerollChildren(context, matrix, ref childPaintBounds);
            this.paintBounds = childPaintBounds;
        }

        protected void prerollChildren(PrerollContext context, Matrix3 childMatrix, ref Rect childPaintBounds) {
            foreach (var layer in this._layers) {
                layer.preroll(context, childMatrix);

                if (childPaintBounds == null || childPaintBounds.isEmpty) {
                    childPaintBounds = layer.paintBounds;
                }
                else {
                    childPaintBounds = childPaintBounds.expandToInclude(layer.paintBounds);
                }
            }
        }

        protected void paintChildren(PaintContext context) {
            D.assert(this.needsPainting);

            foreach (var layer in this._layers) {
                if (layer.needsPainting) {
                    layer.paint(context);
                }
            }
        }
    }
}