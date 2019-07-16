using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class ClipRRectLayer : ContainerLayer {
        RRect _clipRRect;

        public RRect clipRRect {
            set { this._clipRRect = value; }
        }

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            var previousCullRect = context.cullRect;

            var clipPathBounds = this._clipRRect.outerRect;
            context.cullRect = context.cullRect.intersect(clipPathBounds);

            this.paintBounds = Rect.zero;
            if (!context.cullRect.isEmpty) {
                var childPaintBounds = Rect.zero;
                this.prerollChildren(context, matrix, ref childPaintBounds);
                childPaintBounds = childPaintBounds.intersect(clipPathBounds);

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