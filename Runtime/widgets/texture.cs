using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {
    public class Texture : LeafRenderObjectWidget {
        public Texture(Key key, UnityEngine.Texture texture) : base(key: key) {
            D.assert(texture != null);
            this.texture = texture;
        }

        public readonly UnityEngine.Texture texture;

        public override RenderObject createRenderObject(BuildContext context) {
            return new TextureBox(texture: this.texture);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((TextureBox) renderObject).texture = this.texture;
        }
    }
}
