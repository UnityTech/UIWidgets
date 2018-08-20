using UnityEngine;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.flow {
    public class ClipRectLayer : ContainerLayer {
        private Rect _clipRect;

        public Rect clipRect {
            set { this._clipRect = value; }
        }

        public override void preroll(PrerollContext context, Matrix4x4 matrix) {
            var childPaintBounds = Rect.zero;
            this.prerollChildren(context, matrix, ref childPaintBounds);
            childPaintBounds = childPaintBounds.intersect(this._clipRect);

            if (!childPaintBounds.isEmpty) {
                this.paintBounds = childPaintBounds;
            }
        }

        public override void paint(PaintContext context) {
            var canvas = context.canvas;

            canvas.save();
            try {
                canvas.clipRect(this.paintBounds);
                this.paintChildren(context);
            }
            finally {
                canvas.restore();
            }
        }
    }
}