using System;
using UIWidgets.ui;

namespace UIWidgets.painting {
    public class BorderSide : IEquatable<BorderSide> {
        public BorderSide(
            Color color = null,
            double width = 1.0
        ) {
            this.color = color ?? new Color(0xFF000000);
            this.width = width;
        }

        public static BorderSide merge(BorderSide a, BorderSide b) {
            return new BorderSide(
                color: a.color,
                width: a.width + b.width
            );
        }

        public readonly Color color;
        public readonly double width;

        public static readonly BorderSide none = new BorderSide(width: 0.0);

        public BorderSide copyWith(
            Color color = null,
            double? width = null
        ) {
            return new BorderSide(
                color: color ?? this.color,
                width: width ?? this.width
            );
        }

        public static bool canMerge(BorderSide a, BorderSide b) {
            return a.color == b.color;
        }

        public bool Equals(BorderSide other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return object.Equals(this.color, other.color) && this.width.Equals(other.width);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((BorderSide) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((this.color != null ? this.color.GetHashCode() : 0) * 397) ^ this.width.GetHashCode();
            }
        }

        public static bool operator ==(BorderSide lhs, BorderSide rhs) {
            return object.Equals(lhs, rhs);
        }

        public static bool operator !=(BorderSide lhs, BorderSide rhs) {
            return !(lhs == rhs);
        }
    }
}