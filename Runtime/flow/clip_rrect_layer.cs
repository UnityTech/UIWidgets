using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class ClipRRectLayer : ContainerLayer {
        RRect _clipRRect;

        public RRect clipRRect {
            set { this._clipRRect = value; }
        }

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            var childPaintBounds = Rect.zero;
            this.prerollChildren(context, matrix, ref childPaintBounds);
            childPaintBounds = childPaintBounds.intersect(this._clipRRect.outerRect);

            if (!childPaintBounds.isEmpty) {
                this.paintBounds = childPaintBounds;
            }
        }

        public override void paint(PaintContext context) {
            var canvas = context.canvas;

            canvas.save();
            canvas.clipRRect(this._clipRRect);

            try {
                this.paintChildren(context);
            }
            finally {
                canvas.restore();
            }
        }
    }
}