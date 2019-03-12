using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public class TextureBox : RenderBox {

        public TextureBox(Texture texture = null) {
            D.assert(texture != null);
            this._texture = texture;
        }

        public Texture texture {
            get { return this._texture; }
            set {
                D.assert(value != null);
                if (value != this._texture) {
                    this._texture = value;
                    this.markNeedsPaint();
                }                
            }
        }
        
        Texture _texture;

        protected override bool sizedByParent {
            get { return true; }
        }

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        public override bool  isRepaintBoundary {
            get { return true; }
        }

        protected override void performResize() {
            this.size = this.constraints.biggest;
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this._texture == null) {
                return;
            }
            
            context.addLayer(new TextureLayer(
                rect: Rect.fromLTWH(offset.dx, offset.dy, this.size.width, this.size.height),
                texture: this._texture
            ));
        }
    }
}
