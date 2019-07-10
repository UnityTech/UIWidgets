using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.flow {
    public class TextureLayer : Layer {
        Offset _offset = Offset.zero;

        public Offset offset {
            set { this._offset = value ?? Offset.zero; }
        }

        Size _size;

        public Size size {
            set { this._size = value; }
        }

        Texture _texture;

        public Texture texture {
            set { this._texture = value; }
        }

        bool _freeze = false;

        public bool freeze {
            set { this._freeze = value; }
        }

        public override void preroll(PrerollContext context, Matrix3 matrix) {
            this.paintBounds = Rect.fromLTWH(
                this._offset.dx, this._offset.dy, this._size.width, this._size.height);
        }

        public override void paint(PaintContext context) {
            D.assert(this.needsPainting);

            if (this._texture == null) {
                return;
            }

            var image = new Image(this._texture, noDispose: true);

            var canvas = context.canvas;
            canvas.drawImageRect(image, this.paintBounds, new Paint());
        }
    }
}