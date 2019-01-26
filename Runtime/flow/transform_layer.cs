using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class TransformLayer : ContainerLayer {
        Matrix3 _tranform;

        public Matrix3 transform {
            set { this._tranform = value; }
        }

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            var childMatrix = Matrix3.concat(this._tranform, matrix);

            Rect childPaintBounds = Rect.zero;
            this.prerollChildren(context, childMatrix, ref childPaintBounds);

            childPaintBounds = this._tranform.mapRect(childPaintBounds);
            this.paintBounds = childPaintBounds;
        }

        public override void paint(PaintContext context) {
            var canvas = context.canvas;

            canvas.save();
            try {
                canvas.concat(this._tranform);
                this.paintChildren(context);
            }
            finally {
                canvas.restore();
            }
        }
    }
}