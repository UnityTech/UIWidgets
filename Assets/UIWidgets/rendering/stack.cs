using System;
using UIWidgets.ui;

namespace UIWidgets.rendering {
    public class RelativeRect : IEquatable<RelativeRect> {
        private RelativeRect(double left, double top, double right, double bottom) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public readonly double left;
        public readonly double top;
        public readonly double right;
        public readonly double bottom;

        public static RelativeRect fromLTRB(double left, double top, double right, double bottom) {
            return new RelativeRect(left, top, right, bottom);
        }

        public static RelativeRect fromSize(Rect rect, Size container) {
            return new RelativeRect(
                rect.left,
                rect.top,
                container.width - rect.right,
                container.height - rect.bottom);
        }

        public static RelativeRect fromRect(Rect rect, Rect container) {
            return RelativeRect.fromLTRB(
                rect.left - container.left,
                rect.top - container.top,
                container.right - rect.right,
                container.bottom - rect.bottom
            );
        }

        public static readonly RelativeRect fill = RelativeRect.fromLTRB(0.0, 0.0, 0.0, 0.0);

        public bool hasInsets {
            get { return this.left > 0.0 || this.top > 0.0 || this.right > 0.0 || this.bottom > 0.0; }
        }

        public RelativeRect shift(Offset offset) {
            return RelativeRect.fromLTRB(
                this.left + offset.dx,
                this.top + offset.dy,
                this.right - offset.dx,
                this.bottom - offset.dy);
        }

        public RelativeRect inflate(double delta) {
            return RelativeRect.fromLTRB(
                this.left - delta,
                this.top - delta,
                this.right - delta,
                this.bottom - delta);
        }

        public RelativeRect deflate(double delta) {
            return this.inflate(-delta);
        }

        public RelativeRect intersect(RelativeRect other) {
            return RelativeRect.fromLTRB(
                Math.Max(this.left, other.left),
                Math.Max(this.top, other.top),
                Math.Max(this.right, other.right),
                Math.Max(this.bottom, other.bottom)
            );
        }

        public Rect toRect(Rect container) {
            return Rect.fromLTRB(
                this.left + container.left,
                this.top + container.top,
                container.right - this.right,
                container.bottom - this.bottom);
        }

        public Rect toSize(Size container) {
            return Rect.fromLTRB(
                this.left,
                this.top,
                container.width - this.right,
                container.height - this.bottom);
        }

        public bool Equals(RelativeRect other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.left.Equals(other.left)
                   && this.top.Equals(other.top)
                   && this.right.Equals(other.right)
                   && this.bottom.Equals(other.bottom);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((RelativeRect) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.left.GetHashCode();
                hashCode = (hashCode * 397) ^ this.top.GetHashCode();
                hashCode = (hashCode * 397) ^ this.right.GetHashCode();
                hashCode = (hashCode * 397) ^ this.bottom.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(RelativeRect a, RelativeRect b) {
            return object.Equals(a, b);
        }

        public static bool operator !=(RelativeRect a, RelativeRect b) {
            return !(a == b);
        }
    }
}