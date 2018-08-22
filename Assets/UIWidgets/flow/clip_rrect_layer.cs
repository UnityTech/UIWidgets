using UIWidgets.ui;
using UnityEngine;
using Rect = UIWidgets.ui.Rect;
using Color = UIWidgets.ui.Color;

namespace UIWidgets.flow {
    public class ClipRRectLayer : ContainerLayer {
        private RRect _clipRRect;

        public RRect clipRRect {
            set { this._clipRRect = value; }
        }

        public override void preroll(PrerollContext context, Matrix4x4 matrix) {
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
            var paint = new Paint {color = new Color(0xFFFFFFFF)};
            canvas.saveLayer(this.paintBounds, paint);
            try {
                this.paintChildren(context);
            }
            finally {
                canvas.restore();
                canvas.restore();
            }
        }
    }
}