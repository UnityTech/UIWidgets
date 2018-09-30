using UIWidgets.ui;
using UnityEngine;

namespace UIWidgets.flow {
    public class PictureLayer : Layer {
        private Offset _offset;

        public Offset offset {
            set { this._offset = value; }
        }

        private Picture _picture;

        public Picture picture {
            set { this._picture = value; }
        }

        private bool _isComplex = false;

        public bool isComplex {
            set { this._isComplex = value; }
        }

        private bool _willChange = false;

        public bool willChange {
            set { this._willChange = value; }
        }

        private RasterCacheResult _rasterCacheResult;

        public override void preroll(PrerollContext context, Matrix4x4 matrix) {
            if (context.rasterCache != null) {
                Matrix4x4 ctm = matrix;
                ctm = Matrix4x4.Translate(this._offset.toVector()) * ctm;
                ctm.m03 = Mathf.Round(ctm.m03);
                ctm.m13 = Mathf.Round(ctm.m13);

                this._rasterCacheResult = context.rasterCache.getPrerolledImage(
                    this._picture, ref ctm, this._isComplex, this._willChange);
            } else {
                this._rasterCacheResult = null;
            }

            var bounds = this._picture.paintBounds.shift(this._offset);
            this.paintBounds = bounds;
        }

        public override void paint(PaintContext context) {
            var canvas = context.canvas;

            canvas.save();
            canvas.concat(Matrix4x4.Translate(this._offset.toVector()));
            var matrix = canvas.getMatrix();
            matrix.m03 = Mathf.Round(matrix.m03);
            matrix.m13 = Mathf.Round(matrix.m13);
            canvas.setMatrix(matrix);

            try {
                if (this._rasterCacheResult != null) {
                    this._rasterCacheResult.draw(canvas);
                } else {
                    canvas.drawPicture(this._picture);
                }
            }
            finally {
                canvas.restore();
            }
        }
    }
}