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

        public override void preroll(PrerollContext context, Matrix4x4 matrix) {
            var bounds = this._picture.cullRect().shift(this._offset);
            this.paintBounds = bounds;
        }

        public override void paint(PaintContext context) {
            var canvas = context.canvas;

            canvas.save();

            try {
                canvas.concat(Matrix4x4.Translate(new Vector2((float) this._offset.dx, (float) this._offset.dy)));
                canvas.drawPicture(this._picture);
            }
            finally {
                canvas.restore();
            }
        }
    }
}