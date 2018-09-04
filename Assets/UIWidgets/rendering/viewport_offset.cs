using System;
using UIWidgets.foundation;

namespace UIWidgets.rendering {
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

            throw new Exception("unknown direction");
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

        public abstract ScrollDirection userScrollDirection { get; }
    }

    public class _FixedViewportOffset : ViewportOffset {
        public _FixedViewportOffset(double _pixels) {
            this._pixels = _pixels;
        }

        public new static _FixedViewportOffset zero() {
            return new _FixedViewportOffset(0.0);
        }

        public double _pixels;

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

        public override ScrollDirection userScrollDirection {
            get { return ScrollDirection.idle; }
        }
    }
}