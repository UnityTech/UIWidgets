using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {
    public class PerformanceOverlay : LeafRenderObjectWidget {
        public PerformanceOverlay(
            Key key = null,
            int optionsMask = 0
        ) : base(key: key) {
            this.optionsMask = optionsMask;
        }

        public readonly int optionsMask;

        public static PerformanceOverlay allEnabled(
            Key key = null
        ) {
            return new PerformanceOverlay(
                optionsMask: (1 << (int) PerformanceOverlayOption.drawFPS) |
                             (1 << (int) PerformanceOverlayOption.drawFrameCost)
            );
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderPerformanceOverlay(
                optionsMask: this.optionsMask);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderPerformanceOverlay _renderObject = (RenderPerformanceOverlay) renderObject;
            _renderObject.optionsMask = this.optionsMask;
        }
    }
}