using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class TransformLayer : ContainerLayer {
        Matrix3 _transform;

        public Matrix3 transform {
            set { this._transform = value; }
        }

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            var childMatrix = Matrix3.concat(matrix, this._transform);

            var previousCullRect = context.cullRect;

            Matrix3 inverseTransform = Matrix3.I();
            if (this._transform.invert(inverseTransform)) {
                context.cullRect = inverseTransform.mapRect(context.cullRect);
            }
            else {
                context.cullRect = Rect.largest;
            }

            Rect childPaintBounds = Rect.zero;
            this.prerollChildren(context, childMatrix, ref childPaintBounds);

            childPaintBounds = this._transform.mapRect(childPaintBounds);
            this.paintBounds = childPaintBounds;

            context.cullRect = previousCullRect;
        }

        public override void paint(PaintContext context) {
            D.assert(this.needsPainting);

            var canvas = context.canvas;

            canvas.save();
            try {
                canvas.concat(this._transform);
                this.paintChildren(context);
            }
            finally {
                canvas.restore();
            }
        }
    }
}