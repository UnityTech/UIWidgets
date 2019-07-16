using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class BackdropFilterLayer : ContainerLayer {
        ImageFilter _filter;

        public ImageFilter filter {
            set { this._filter = value; }
        }

        public override void paint(PaintContext context) {
            D.assert(this.needsPainting);

            var canvas = context.canvas;
            canvas.saveLayer(this.paintBounds, new Paint {backdrop = this._filter});

            try {
                this.paintChildren(context);
            }
            finally {
                canvas.restore();
            }
        }
    }
}