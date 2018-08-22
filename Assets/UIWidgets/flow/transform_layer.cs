using UIWidgets.painting;
using UIWidgets.ui;
using Matrix4x4 = UnityEngine.Matrix4x4;

namespace UIWidgets.flow {
    public class TransformLayer : ContainerLayer {
        private Matrix4x4 _tranform;

        public Matrix4x4 transform {
            set { this._tranform = value; }
        }

        public override void preroll(PrerollContext context, Matrix4x4 matrix) {
            var childMatrix = this._tranform * matrix;

            Rect childPaintBounds = Rect.zero;
            this.prerollChildren(context, childMatrix, ref childPaintBounds);

            childPaintBounds = MatrixUtils.transformRect(this._tranform, childPaintBounds);
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