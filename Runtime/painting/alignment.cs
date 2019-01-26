using System;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class Alignment : IEquatable<Alignment> {
        public Alignment(double x, double y) {
            this.x = x;
            this.y = y;
        }

        public readonly double x;

        public readonly double y;

        public static readonly Alignment topLeft = new Alignment(-1.0, -1.0);
        public static readonly Alignment topCenter = new Alignment(0, -1.0);
        public static readonly Alignment topRight = new Alignment(1.0, -1.0);
        public static readonly Alignment centerLeft = new Alignment(-1.0, 0.0);
        public static readonly Alignment center = new Alignment(0.0, 0.0);
        public static readonly Alignment centerRight = new Alignment(1.0, 0.0);
        public static readonly Alignment bottomLeft = new Alignment(-1.0, 1.0);
        public static readonly Alignment bottomCenter = new Alignment(0.0, 1.0);
        public static readonly Alignment bottomRight = new Alignment(1.0, 1.0);

        public static Alignment operator -(Alignment a, Alignment b) {
            return new Alignment(a.x - b.x, a.y - b.y);
        }

        public static Alignment operator +(Alignment a, Alignment b) {
            return new Alignment(a.x + b.x, a.y + b.y);
        }

        public static Alignment operator -(Alignment a) {
            return new Alignment(-a.x, -a.y);
        }

        public static Alignment operator *(Alignment a, double b) {
            return new Alignment(a.x * b, a.y * b);
        }

        public static Alignment operator /(Alignment a, double b) {
            return new Alignment(a.x / b, a.y / b);
        }

        public static Alignment operator %(Alignment a, double b) {
            return new Alignment(a.x % b, a.y % b);
        }

        public Offset alongOffset(Offset other) {
            double centerX = other.dx / 2.0;
            double centerY = other.dy / 2.0;
            return new Offset(centerX + this.x * centerX, centerY + this.y * centerY);
        }

        public Offset alongSize(Size other) {
            double centerX = other.width / 2.0;
            double centerY = other.height / 2.0;
            return new Offset(centerX + this.x * centerX, centerY + this.y * centerY);
        }

        public Offset withinRect(Rect rect) {
            double halfWidth = rect.width / 2.0;
            double halfHeight = rect.height / 2.0;
            return new Offset(
                rect.left + halfWidth + this.x * halfWidth,
                rect.top + halfHeight + this.y * halfHeight
            );
        }

        public Rect inscribe(Size size, Rect rect) {
            double halfWidthDelta = (rect.width - size.width) / 2.0;
            double halfHeightDelta = (rect.height - size.height) / 2.0;
            return Rect.fromLTWH(
                rect.left + halfWidthDelta + this.x * halfWidthDelta,
                rect.top + halfHeightDelta + this.y * halfHeightDelta,
                size.width,
                size.height
            );
        }

        public static Alignment lerp(Alignment a, Alignment b, double t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return new Alignment(MathUtils.lerpDouble(0.0, b.x, t), MathUtils.lerpDouble(0.0, b.y, t));
            }

            if (b == null) {
                return new Alignment(MathUtils.lerpDouble(a.x, 0.0, t), MathUtils.lerpDouble(a.y, 0.0, t));
            }

            return new Alignment(MathUtils.lerpDouble(a.x, b.x, t), MathUtils.lerpDouble(a.y, b.y, t));
        }

        public bool Equals(Alignment other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.x.Equals(other.x) && this.y.Equals(other.y);
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

            return this.Equals((Alignment) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.x.GetHashCode() * 397) ^ this.y.GetHashCode();
            }
        }

        public static bool operator ==(Alignment a, Alignment b) {
            return Equals(a, b);
        }

        public static bool operator !=(Alignment a, Alignment b) {
            return !(a == b);
        }

        public override string ToString() {
            if (this.x == -1.0 && this.y == -1.0) {
                return "topLeft";
            }

            if (this.x == 0.0 && this.y == -1.0) {
                return "topCenter";
            }

            if (this.x == 1.0 && this.y == -1.0) {
                return "topRight";
            }

            if (this.x == -1.0 && this.y == 0.0) {
                return "centerLeft";
            }

            if (this.x == 0.0 && this.y == 0.0) {
                return "center";
            }

            if (this.x == 1.0 && this.y == 0.0) {
                return "centerRight";
            }

            if (this.x == -1.0 && this.y == 1.0) {
                return "bottomLeft";
            }

            if (this.x == 0.0 && this.y == 1.0) {
                return "bottomCenter";
            }

            if (this.x == 1.0 && this.y == 1.0) {
                return "bottomRight";
            }

            return $"Alignment({this.x:F1}, {this.y:F1})";
        }
    }
}