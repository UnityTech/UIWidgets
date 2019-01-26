using System;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class CircleBorder : ShapeBorder, IEquatable<CircleBorder> {
        public CircleBorder(BorderSide side = null) {
            this.side = side ?? BorderSide.none;
        }

        public readonly BorderSide side;

        public override EdgeInsets dimensions {
            get { return EdgeInsets.all(this.side.width); }
        }

        public override ShapeBorder scale(double t) {
            return new CircleBorder(side: this.side.scale(t));
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, double t) {
            if (a is CircleBorder border) {
                return new CircleBorder(side: BorderSide.lerp(border.side, this.side, t));
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, double t) {
            if (b is CircleBorder border) {
                return new CircleBorder(side: BorderSide.lerp(this.side, border.side, t));
            }

            return base.lerpTo(b, t);
        }

        public override Path getInnerPath(Rect rect) {
            var path = new Path();
            path.addOval(Rect.fromCircle(
                center: rect.center,
                radius: Math.Max(0.0, rect.shortestSide / 2.0 - this.side.width)
            ));
            return path;
        }

        public override Path getOuterPath(Rect rect) {
            var path = new Path();
            path.addOval(Rect.fromCircle(
                center: rect.center,
                radius: rect.shortestSide / 2.0
            ));
            return path;
        }

        public override void paint(Canvas canvas, Rect rect) {
            switch (this.side.style) {
                case BorderStyle.none:
                    break;
                case BorderStyle.solid:
                    canvas.drawCircle(rect.center, (rect.shortestSide - this.side.width) / 2.0, this.side.toPaint());
                    break;
            }
        }

        public bool Equals(CircleBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.side, other.side);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((CircleBorder) obj);
        }

        public override int GetHashCode() {
            return (this.side != null ? this.side.GetHashCode() : 0);
        }

        public static bool operator ==(CircleBorder left, CircleBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(CircleBorder left, CircleBorder right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.side})";
        }
    }
}