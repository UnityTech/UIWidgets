using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.flow {
    public class PictureLayer : Layer {
        Offset _offset = Offset.zero;

        public Offset offset {
            set { this._offset = value ?? Offset.zero; }
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
                ctm.preTranslate(this._offset.dx,
                    this._offset.dy); // TOOD: pre or post? https://github.com/flutter/engine/pull/7945
                ctm[2] = ctm[2].alignToPixel(context.devicePixelRatio);
                ctm[5] = ctm[5].alignToPixel(context.devicePixelRatio);

                this._rasterCacheResult = context.rasterCache.getPrerolledImage(
                    this._picture, ctm, context.devicePixelRatio, context.antiAliasing, this._isComplex,
                    this._willChange);
            }
            else {
                this._rasterCacheResult = null;
            }

            var bounds = this._picture.paintBounds.shift(this._offset);
            this.paintBounds = bounds;
        }

        public override void paint(PaintContext context) {
            D.assert(this._picture != null);
            D.assert(this.needsPainting);

            var canvas = context.canvas;

            canvas.save();
            canvas.translate(this._offset.dx, this._offset.dy);

            canvas.alignToPixel();

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