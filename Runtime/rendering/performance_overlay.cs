using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public enum PerformanceOverlayOption {
        drawFPS, //default
        drawFrameCost
    }


    public class RenderPerformanceOverlay : RenderBox {
        public RenderPerformanceOverlay(
            int optionsMask = 0
        ) {
            this._optionMask = optionsMask;
        }

        public int optionsMask {
            get { return this._optionMask; }
            set {
                if (value == this._optionMask) {
                    return;
                }

                this._optionMask = value;
                this.markNeedsPaint();
            }
        }

        int _optionMask;

        protected override bool sizedByParent {
            get { return true; }
        }

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        protected override float computeMinIntrinsicWidth(float height) {
            return 0.0f;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return 0.0f;
        }

        float _intrinsicHeight {
            get {
                const float kDefaultGraphHeight = 80.0f;
                float result = 20f;

                if ((this.optionsMask | (1 << (int) PerformanceOverlayOption.drawFrameCost)) > 0) {
                    result += kDefaultGraphHeight;
                }

                return result;
            }
        }

        protected override float computeMinIntrinsicHeight(float width) {
            return this._intrinsicHeight;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return this._intrinsicHeight;
        }

        protected override void performResize() {
            this.size = this.constraints.constrain(new Size(float.PositiveInfinity, this._intrinsicHeight));
        }

        public override void paint(PaintingContext context, Offset offset) {
            D.assert(this.needsCompositing);
            context.addLayer(new PerformanceOverlayLayer(
                overlayRect: Rect.fromLTWH(offset.dx, offset.dy, this.size.width, this.size.height),
                optionsMask: this.optionsMask
            ));
        }
    }
}