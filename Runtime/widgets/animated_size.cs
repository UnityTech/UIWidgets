using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;

namespace Unity.UIWidgets.widgets {
    public class AnimatedSize : SingleChildRenderObjectWidget {
        public AnimatedSize(
            Key key = null,
            Widget child = null,
            Alignment alignment = null,
            Curve curve = null,
            TimeSpan? duration = null,
            TickerProvider vsync = null) : base(key: key, child: child) {
            D.assert(duration != null);
            D.assert(vsync != null);
            this.alignment = alignment ?? Alignment.center;
            this.curve = curve ?? Curves.linear;
            this.duration = duration ?? TimeSpan.Zero;
            this.vsync = vsync;
        }

        public readonly Alignment alignment;

        public readonly Curve curve;

        public readonly TimeSpan duration;

        public readonly TickerProvider vsync;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderAnimatedSize(
                alignment: this.alignment,
                duration: this.duration,
                curve: this.curve,
                vsync: this.vsync);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            RenderAnimatedSize _renderObject = (RenderAnimatedSize) renderObject;
            _renderObject.alignment = this.alignment;
            _renderObject.duration = this.duration;
            _renderObject.curve = this.curve;
            _renderObject.vsync = this.vsync;
        }
    }
}