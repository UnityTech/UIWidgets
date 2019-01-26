using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.rendering {
    public enum ScrollDirection {
        idle,
        forward,
        reverse,
    }

    public static class ScrollDirectionUtils {
        public static ScrollDirection flipScrollDirection(ScrollDirection direction) {
            switch (direction) {
                case ScrollDirection.idle:
                    return ScrollDirection.idle;
                case ScrollDirection.forward:
                    return ScrollDirection.reverse;
                case ScrollDirection.reverse:
                    return ScrollDirection.forward;
            }

            D.assert(false);
            return default(ScrollDirection);
        }
    }

    public abstract class ViewportOffset : ChangeNotifier {
        protected ViewportOffset() {
        }

        public static ViewportOffset @fixed(double value) {
            return new _FixedViewportOffset(value);
        }

        public static ViewportOffset zero() {
            return _FixedViewportOffset.zero();
        }

        public abstract double pixels { get; }
        public abstract bool applyViewportDimension(double viewportDimension);
        public abstract bool applyContentDimensions(double minScrollExtent, double maxScrollExtent);

        public abstract void correctBy(double correction);
        public abstract void jumpTo(double pixels);

        public abstract IPromise animateTo(double to, TimeSpan duration, Curve curve);

        public abstract ScrollDirection userScrollDirection { get; }

        public abstract bool allowImplicitScrolling { get; }

        public override string ToString() {
            var description = new List<string>();
            this.debugFillDescription(description);
            return Diagnostics.describeIdentity(this) + "(" + string.Join(", ", description.ToArray()) + ")";
        }

        protected virtual void debugFillDescription(List<string> description) {
            description.Add("offset: " + this.pixels.ToString("F1"));
        }
    }

    class _FixedViewportOffset : ViewportOffset {
        internal _FixedViewportOffset(double _pixels) {
            this._pixels = _pixels;
        }

        internal new static _FixedViewportOffset zero() {
            return new _FixedViewportOffset(0.0);
        }

        double _pixels;

        public override double pixels {
            get { return this._pixels; }
        }

        public override bool applyViewportDimension(double viewportDimension) {
            return true;
        }

        public override bool applyContentDimensions(double minScrollExtent, double maxScrollExtent) {
            return true;
        }

        public override void correctBy(double correction) {
            this._pixels += correction;
        }

        public override void jumpTo(double pixels) {
        }

        public override IPromise animateTo(double to, TimeSpan duration, Curve curve) {
            return Promise.Resolved();
        }

        public override ScrollDirection userScrollDirection {
            get { return ScrollDirection.idle; }
        }

        public override bool allowImplicitScrolling {
            get { return false; }
        }
    }
}