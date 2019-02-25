using System;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class Alignment : IEquatable<Alignment> {
        public Alignment(float x, float y) {
            this.x = x;
            this.y = y;
        }

        public readonly float x;

        public readonly float y;

        public static readonly Alignment topLeft = new Alignment(-1.0f, -1.0f);
        public static readonly Alignment topCenter = new Alignment(0, -1.0f);
        public static readonly Alignment topRight = new Alignment(1.0f, -1.0f);
        public static readonly Alignment centerLeft = new Alignment(-1.0f, 0.0f);
        public static readonly Alignment center = new Alignment(0.0f, 0.0f);
        public static readonly Alignment centerRight = new Alignment(1.0f, 0.0f);
        public static readonly Alignment bottomLeft = new Alignment(-1.0f, 1.0f);
        public static readonly Alignment bottomCenter = new Alignment(0.0f, 1.0f);
        public static readonly Alignment bottomRight = new Alignment(1.0f, 1.0f);

        public static Alignment operator -(Alignment a, Alignment b) {
            return new Alignment(a.x - b.x, a.y - b.y);
        }

        public static Alignment operator +(Alignment a, Alignment b) {
            return new Alignment(a.x + b.x, a.y + b.y);
        }

        public static Alignment operator -(Alignment a) {
            return new Alignment(-a.x, -a.y);
        }

        public static Alignment operator *(Alignment a, float b) {
            return new Alignment(a.x * b, a.y * b);
        }

        public static Alignment operator /(Alignment a, float b) {
            return new Alignment(a.x / b, a.y / b);
        }

        public static Alignment operator %(Alignment a, float b) {
            return new Alignment(a.x % b, a.y % b);
        }

        public Offset alongOffset(Offset other) {
            float centerX = other.dx / 2.0f;
            float centerY = other.dy / 2.0f;
            return new Offset(centerX + this.x * centerX, centerY + this.y * centerY);
        }

        public Offset alongSize(Size other) {
            float centerX = other.width / 2.0f;
            float centerY = other.height / 2.0f;
            return new Offset(centerX + this.x * centerX, centerY + this.y * centerY);
        }

        public Offset withinRect(Rect rect) {
            float halfWidth = rect.width / 2.0f;
            float halfHeight = rect.height / 2.0f;
            return new Offset(
                rect.left + halfWidth + this.x * halfWidth,
                rect.top + halfHeight + this.y * halfHeight
            );
        }

        public Rect inscribe(Size size, Rect rect) {
            float halfWidthDelta = (rect.width - size.width) / 2.0f;
            float halfHeightDelta = (rect.height - size.height) / 2.0f;
            return Rect.fromLTWH(
                rect.left + halfWidthDelta + this.x * halfWidthDelta,
                rect.top + halfHeightDelta + this.y * halfHeightDelta,
                size.width,
                size.height
            );
        }

        public static Alignment lerp(Alignment a, Alignment b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return new Alignment(MathUtils.lerpFloat(0.0f, b.x, t), MathUtils.lerpFloat(0.0f, b.y, t));
            }

            if (b == null) {
                return new Alignment(MathUtils.lerpFloat(a.x, 0.0f, t), MathUtils.lerpFloat(a.y, 0.0f, t));
            }

            return new Alignment(MathUtils.lerpFloat(a.x, b.x, t), MathUtils.lerpFloat(a.y, b.y, t));
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
            if (this.x == -1.0f && this.y == -1.0f) {
                return "topLeft";
            }

            if (this.x == 0.0f && this.y == -1.0f) {
                return "topCenter";
            }

            if (this.x == 1.0f && this.y == -1.0f) {
                return "topRight";
            }

            if (this.x == -1.0f && this.y == 0.0f) {
                return "centerLeft";
            }

            if (this.x == 0.0f && this.y == 0.0f) {
                return "center";
            }

            if (this.x == 1.0f && this.y == 0.0f) {
                return "centerRight";
            }

            if (this.x == -1.0f && this.y == 1.0f) {
                return "bottomLeft";
            }

            if (this.x == 0.0f && this.y == 1.0f) {
                return "bottomCenter";
            }

            if (this.x == 1.0f && this.y == 1.0f) {
                return "bottomRight";
            }

            return $"Alignment({this.x:F1}, {this.y:F1})";
        }
    }
}