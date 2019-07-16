using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class ClipRectLayer : ContainerLayer {
        Rect _clipRect;

        public Rect clipRect {
            set { this._clipRect = value; }
        }

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            var previousCullRect = context.cullRect;

            context.cullRect = context.cullRect.intersect(this._clipRect);

            this.paintBounds = Rect.zero;
            if (!context.cullRect.isEmpty) {
                var childPaintBounds = Rect.zero;
                this.prerollChildren(context, matrix, ref childPaintBounds);
                childPaintBounds = childPaintBounds.intersect(this._clipRect);

                if (!childPaintBounds.isEmpty) {
                    this.paintBounds = childPaintBounds;
                }
            }

            context.cullRect = previousCullRect;
        }

        public override void paint(PaintContext context) {
            D.assert(this.needsPainting);

            var canvas = context.canvas;

            canvas.save();
            canvas.clipRect(this.paintBounds);

            try {
                this.paintChildren(context);
            }
            finally {
                canvas.restore();
            }
        }
    }
}