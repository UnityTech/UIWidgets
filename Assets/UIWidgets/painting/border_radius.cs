using System;
using UIWidgets.ui;

namespace UIWidgets.painting {
    public class BorderRadius : IEquatable<BorderRadius> {
        private BorderRadius(
            double topLeft,
            double topRight,
            double bottomRight,
            double bottomLeft) {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomRight = bottomRight;
            this.bottomLeft = bottomLeft;
        }

        public static BorderRadius all(double radius) {
            return BorderRadius.only(radius, radius, radius, radius);
        }

        public static BorderRadius vertical(double top, double bottom) {
            return BorderRadius.only(top, top, bottom, bottom);
        }

        public static BorderRadius horizontal(double left, double right) {
            return BorderRadius.only(left, right, right, left);
        }

        public static BorderRadius only(
            double topLeft = 0.0, double topRight = 0.0,
            double bottomRight = 0.0, double bottomLeft = 0.0) {
            return new BorderRadius(topLeft, topRight, bottomRight, bottomLeft);
        }

        public static readonly BorderRadius zero = BorderRadius.all(0);

        public readonly double topLeft;
        public readonly double topRight;
        public readonly double bottomRight;
        public readonly double bottomLeft;

        public RRect toRRect(Rect rect) {
            return RRect.fromRectAndCorners(
                rect,
                topLeft: this.topLeft,
                topRight: this.topRight,
                bottomLeft: this.bottomLeft,
                bottomRight: this.bottomRight
            );
        }
        
        public bool Equals(BorderRadius other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.topLeft.Equals(other.topLeft)
                   && this.topRight.Equals(other.topRight)
                   && this.bottomRight.Equals(other.bottomRight)
                   && this.bottomLeft.Equals(other.bottomLeft);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((BorderRadius) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.topLeft.GetHashCode();
                hashCode = (hashCode * 397) ^ this.topRight.GetHashCode();
                hashCode = (hashCode * 397) ^ this.bottomRight.GetHashCode();
                hashCode = (hashCode * 397) ^ this.bottomLeft.GetHashCode();
                return hashCode;
            }
        }
    }

    public class BorderWidth : IEquatable<BorderWidth> {
        private BorderWidth(
            double top,
            double right,
            double bottom,
            double left) {
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.left = left;
        }

        public static BorderWidth only(
            double top = 0, double right = 0,
            double bottom = 0, double left = 0) {
            return new BorderWidth(top, right, bottom, left);
        }

        public static BorderWidth all(double width) {
            return BorderWidth.only(width, width, width, width);
        }

        public static readonly BorderWidth zero = BorderWidth.only();

        public readonly double top;
        public readonly double right;
        public readonly double bottom;
        public readonly double left;

        public bool Equals(BorderWidth other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.top.Equals(other.top)
                   && this.right.Equals(other.right)
                   && this.bottom.Equals(other.bottom)
                   && this.left.Equals(other.left);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((BorderWidth) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.top.GetHashCode();
                hashCode = (hashCode * 397) ^ this.right.GetHashCode();
                hashCode = (hashCode * 397) ^ this.bottom.GetHashCode();
                hashCode = (hashCode * 397) ^ this.left.GetHashCode();
                return hashCode;
            }
        }
    }
}