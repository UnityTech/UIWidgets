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
                Matrix3 ctm = new Matrix3(matrix);
                ctm.postTranslate((float) this._offset.dx, (float) this._offset.dy);
                ctm[2] = ctm[2].alignToPixel(context.devicePixelRatio);
                ctm[5] = ctm[5].alignToPixel(context.devicePixelRatio);

                this._rasterCacheResult = context.rasterCache.getPrerolledImage(
                    this._picture, ctm, context.devicePixelRatio, this._isComplex, this._willChange);
            }
            else {
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
            matrix[2] = matrix[2].alignToPixel(devicePixelRatio);
            matrix[5] = matrix[5].alignToPixel(devicePixelRatio);
            canvas.setMatrix(matrix);

            try {
                if (this._rasterCacheResult != null) {
                    this._rasterCacheResult.draw(canvas);
                }
                else {
                    canvas.drawPicture(this._picture);
                }
            }
            finally {
                canvas.restore();
            }
        }
    }
}