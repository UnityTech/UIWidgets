using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class OpacityLayer : ContainerLayer {
        Offset _offset;

        public Offset offset {
            set { this._offset = value; }
        }

        int _alpha;

        public int alpha {
            set { this._alpha = value; }
        }

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            var childMatrix = new Matrix3(matrix);
            childMatrix.preTranslate(this._offset.dx,
                this._offset.dy); // TOOD: pre or post? https://github.com/flutter/engine/pull/7945

            base.preroll(context, childMatrix);

            var bounds = this.paintBounds.shift(this._offset);
            this.paintBounds = bounds;
        }

        public override void paint(PaintContext context) {
            D.assert(this.needsPainting);

            var canvas = context.canvas;

            canvas.save();
            canvas.translate(this._offset.dx, this._offset.dy);

            canvas.alignToPixel();

            var saveLayerBounds = this.paintBounds.shift(-this._offset).roundOut();
            var paint = new Paint {color = Color.fromARGB(this._alpha, 255, 255, 255)};
            canvas.saveLayer(saveLayerBounds, paint);

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