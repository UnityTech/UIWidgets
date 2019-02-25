using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public enum RenderAnimatedSizeState {
        start,
        stable,
        changed,
        unstable
    }

    public class RenderAnimatedSize : RenderAligningShiftedBox {
        public RenderAnimatedSize(
            TickerProvider vsync = null,
            TimeSpan? duration = null,
            Curve curve = null,
            Alignment alignment = null,
            RenderBox child = null
        ) : base(child: child, alignment: alignment ?? Alignment.center) {
            D.assert(vsync != null);
            D.assert(duration != null);

            curve = curve ?? Curves.linear;
            this._vsync = vsync;
            this._controller = new AnimationController(
                vsync: this.vsync,
                duration: duration);
            this._controller.addListener(() => {
                if (this._controller.value != this._lastValue) {
                    this.markNeedsLayout();
                }
            });
            this._animation = new CurvedAnimation(
                parent: this._controller,
                curve: curve);
        }

        AnimationController _controller;
        CurvedAnimation _animation;
        readonly SizeTween _sizeTween = new SizeTween();
        bool _hasVisualOverflow;
        float _lastValue;

        public RenderAnimatedSizeState state {
            get { return this._state; }
        }

        RenderAnimatedSizeState _state = RenderAnimatedSizeState.start;

        public TimeSpan? duration {
            get { return this._controller.duration; }
            set {
                D.assert(value != null);
                if (value == this._controller.duration) {
                    return;
                }

                this._controller.duration = value;
            }
        }

        public Curve curve {
            get { return this._animation.curve; }
            set {
                D.assert(value != null);
                if (value == this._animation.curve) {
                    return;
                }

                this._animation.curve = value;
            }
        }

        public bool isAnimating {
            get { return this._controller.isAnimating; }
        }

        public TickerProvider vsync {
            get { return this._vsync; }
            set {
                D.assert(value != null);
                if (value == this._vsync) {
                    return;
                }

                this._vsync = value;
                this._controller.resync(this.vsync);
            }
        }

        TickerProvider _vsync;

        public override void detach() {
            this._controller.stop();
            base.detach();
        }

        Size _animatedSize {
            get { return this._sizeTween.evaluate(this._animation); }
        }

        protected override void performLayout() {
            this._lastValue = this._controller.value;
            this._hasVisualOverflow = false;

            if (this.child == null || this.constraints.isTight) {
                this._controller.stop();
                this.size = this._sizeTween.begin = this._sizeTween.end = this.constraints.smallest;
                this._state = RenderAnimatedSizeState.start;
                this.child?.layout(this.constraints);
                return;
            }

            this.child.layout(this.constraints, parentUsesSize: true);

            switch (this._state) {
                case RenderAnimatedSizeState.start:
                    this._layoutStart();
                    break;
                case RenderAnimatedSizeState.stable:
                    this._layoutStable();
                    break;
                case RenderAnimatedSizeState.changed:
                    this._layoutChanged();
                    break;
                case RenderAnimatedSizeState.unstable:
                    this._layoutUnstable();
                    break;
            }

            this.size = this.constraints.constrain(this._animatedSize);
            this.alignChild();

            if (this.size.width < this._sizeTween.end.width ||
                this.size.height < this._sizeTween.end.height) {
                this._hasVisualOverflow = true;
            }
        }

        void _restartAnimation() {
            this._lastValue = 0.0f;
            this._controller.forward(from: 0.0f);
        }

        void _layoutStart() {
            this._sizeTween.begin = this._sizeTween.end = this.debugAdoptSize(this.child.size);
            this._state = RenderAnimatedSizeState.stable;
        }

        void _layoutStable() {
            if (this._sizeTween.end != this.child.size) {
                this._sizeTween.begin = this.size;
                this._sizeTween.end = this.debugAdoptSize(this.child.size);
                this._restartAnimation();
                this._state = RenderAnimatedSizeState.changed;
            }
            else if (this._controller.value == this._controller.upperBound) {
                this._sizeTween.begin = this._sizeTween.end = this.debugAdoptSize(this.child.size);
            }
            else if (!this._controller.isAnimating) {
                this._controller.forward();
            }
        }

        void _layoutChanged() {
            if (this._sizeTween.end != this.child.size) {
                this._sizeTween.begin = this._sizeTween.end = this.debugAdoptSize(this.child.size);
                this._restartAnimation();
                this._state = RenderAnimatedSizeState.unstable;
            }
            else {
                this._state = RenderAnimatedSizeState.stable;
                if (!this._controller.isAnimating) {
                    this._controller.forward();
                }
            }
        }

        void _layoutUnstable() {
            if (this._sizeTween.end != this.child.size) {
                this._sizeTween.begin = this._sizeTween.end = this.debugAdoptSize(this.child.size);
                this._restartAnimation();
            }
            else {
                this._controller.stop();
                this._state = RenderAnimatedSizeState.stable;
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null && this._hasVisualOverflow) {
                Rect rect = Offset.zero & this.size;
                context.pushClipRect(this.needsCompositing, offset, rect, base.paint);
            }
            else {
                base.paint(context, offset);
            }
        }
    }
}