using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class PictureLayer : Layer {
        Offset _offset;

        public Offset offset {
            set { this._offset = value; }
        }

        Picture _picture;

        public Picture picture {
            set { this._picture = value; }
        }

        bool _isComplex = false;

        public bool isComplex {
            set { this._isComplex = value; }
        }

        bool _willChange = false;

        public bool willChange {
            set { this._willChange = value; }
        }

        RasterCacheResult _rasterCacheResult;

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            if (context.rasterCache != null) {
                Matrix3 ctm = Matrix3.makeTrans((float) this._offset.dx, (float) this._offset.dy);
                ctm.preConcat(matrix);
                ctm[6] = ctm[6].alignToPixel(context.devicePixelRatio);
                ctm[7] = ctm[7].alignToPixel(context.devicePixelRatio);

                this._rasterCacheResult = context.rasterCache.getPrerolledImage(
                    this._picture, ctm, context.devicePixelRatio, this._isComplex, this._willChange);
            } else {
                this._rasterCacheResult = null;
            }

            var bounds = this._picture.paintBounds.shift(this._offset);
            this.paintBounds = bounds;
        }

        public override void paint(PaintContext context) {
            var canvas = context.canvas;

            canvas.save();
            canvas.translate(this._offset.dx, this._offset.dy);

            // align to pixel
            var matrix = canvas.getTotalMatrix();
            var devicePixelRatio = context.canvas.getDevicePixelRatio();
            matrix[6] = matrix[6].alignToPixel(devicePixelRatio);
            matrix[7] = matrix[7].alignToPixel(devicePixelRatio);
            canvas.setMatrix(matrix);

            try {
                if (this._rasterCacheResult != null) {
                    this._rasterCacheResult.draw(canvas);
                } else {
                    canvas.drawPicture(this._picture);
                }
            } finally {
                canvas.restore();
            }
        }
    }
}
