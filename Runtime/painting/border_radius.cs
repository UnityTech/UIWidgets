using System;
using System.Text;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class BorderRadius : IEquatable<BorderRadius> {
        BorderRadius(
            Radius topLeft,
            Radius topRight,
            Radius bottomRight,
            Radius bottomLeft) {
            this.topLeft = topLeft ?? Radius.zero;
            this.topRight = topRight ?? Radius.zero;
            this.bottomRight = bottomRight ?? Radius.zero;
            this.bottomLeft = bottomLeft ?? Radius.zero;
        }

        public static BorderRadius all(Radius radius) {
            return only(radius, radius, radius, radius);
        }

        public static BorderRadius all(float radius) {
            return only(radius, radius, radius, radius);
        }

        public static BorderRadius circular(float radius) {
            return all(Radius.circular(radius));
        }

        public static BorderRadius vertical(Radius top = null, Radius bottom = null) {
            return only(top, top, bottom, bottom);
        }

        public static BorderRadius vertical(float? top = null, float? bottom = null) {
            return only(top, top, bottom, bottom);
        }

        public static BorderRadius horizontal(Radius left = null, Radius right = null) {
            return only(left, right, right, left);
        }

        public static BorderRadius horizontal(float? left = null, float? right = null) {
            return only(left, right, right, left);
        }

        public static BorderRadius only(
            Radius topLeft = null, Radius topRight = null,
            Radius bottomRight = null, Radius bottomLeft = null) {
            return new BorderRadius(topLeft, topRight, bottomRight, bottomLeft);
        }

        public static BorderRadius only(
            float? topLeft = null, float? topRight = null,
            float? bottomRight = null, float? bottomLeft = null) {
            var tlRadius = topLeft != null ? Radius.circular(topLeft.Value) : null;
            var trRadius = topRight != null ? Radius.circular(topRight.Value) : null;
            var brRadius = bottomRight != null ? Radius.circular(bottomRight.Value) : null;
            var blRadius = bottomLeft != null ? Radius.circular(bottomLeft.Value) : null;

            return new BorderRadius(tlRadius, trRadius, brRadius, blRadius);
        }

        public static readonly BorderRadius zero = all(Radius.zero);

        public readonly Radius topLeft;
        public readonly Radius topRight;
        public readonly Radius bottomRight;
        public readonly Radius bottomLeft;

        public RRect toRRect(Rect rect) {
            return RRect.fromRectAndCorners(
                rect,
                topLeft: this.topLeft,
                topRight: this.topRight,
                bottomRight: this.bottomRight,
                bottomLeft: this.bottomLeft
            );
        }

        public static BorderRadius operator -(BorderRadius it, BorderRadius other) {
            return only(
                topLeft: it.topLeft - other.topLeft,
                topRight: it.topRight - other.topRight,
                bottomLeft: it.bottomLeft - other.bottomLeft,
                bottomRight: it.bottomRight - other.bottomRight
            );
        }

        public static BorderRadius operator +(BorderRadius it, BorderRadius other) {
            return only(
                topLeft: it.topLeft + other.topLeft,
                topRight: it.topRight + other.topRight,
                bottomLeft: it.bottomLeft + other.bottomLeft,
                bottomRight: it.bottomRight + other.bottomRight
            );
        }

        public static BorderRadius operator -(BorderRadius it) {
            return only(
                topLeft: -it.topLeft,
                topRight: -it.topRight,
                bottomLeft: -it.bottomLeft,
                bottomRight: -it.bottomRight
            );
        }

        public static BorderRadius operator *(BorderRadius it, float other) {
            return only(
                topLeft: it.topLeft * other,
                topRight: it.topRight * other,
                bottomLeft: it.bottomLeft * other,
                bottomRight: it.bottomRight * other
            );
        }

        public static BorderRadius operator /(BorderRadius it, float other) {
            return only(
                topLeft: it.topLeft / other,
                topRight: it.topRight / other,
                bottomLeft: it.bottomLeft / other,
                bottomRight: it.bottomRight / other
            );
        }

        public static BorderRadius operator %(BorderRadius it, float other) {
            return only(
                topLeft: it.topLeft % other,
                topRight: it.topRight % other,
                bottomLeft: it.bottomLeft % other,
                bottomRight: it.bottomRight % other
            );
        }

        public static BorderRadius lerp(BorderRadius a, BorderRadius b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b * t;
            }

            if (b == null) {
                return a * (1.0f - t);
            }

            return only(
                topLeft: Radius.lerp(a.topLeft, b.topLeft, t),
                topRight: Radius.lerp(a.topRight, b.topRight, t),
                bottomLeft: Radius.lerp(a.bottomLeft, b.bottomLeft, t),
                bottomRight: Radius.lerp(a.bottomRight, b.bottomRight, t)
            );
        }

        public bool Equals(BorderRadius other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.topLeft.Equals(other.topLeft)
                   && this.topRight.Equals(other.topRight)
                   && this.bottomRight.Equals(other.bottomRight)
                   && this.bottomLeft.Equals(other.bottomLeft);
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

        public static bool operator ==(BorderRadius a, BorderRadius b) {
            return Equals(a, b);
        }

        public static bool operator !=(BorderRadius a, BorderRadius b) {
            return !Equals(a, b);
        }

        public override string ToString() {
            string visual = null;
            if (this.topLeft == this.topRight &&
                this.topRight == this.bottomLeft &&
                this.bottomLeft == this.bottomRight) {
                if (this.topLeft != Radius.zero) {
                    if (this.topLeft.x == this.topLeft.y) {
                        visual = $"BorderRadius.circular({this.topLeft.x:F1})";
                    }
                    else {
                        visual = $"BorderRadius.all({this.topLeft})";
                    }
                }
            }
            else {
                var result = new StringBuilder();
                result.Append("BorderRadius.only(");
                bool comma = false;
                if (this.topLeft != Radius.zero) {
                    result.Append($"topLeft: {this.topLeft}");
                    comma = true;
                }

                if (this.topRight != Radius.zero) {
                    if (comma) {
                        result.Append(", ");
                    }

                    result.Append($"topRight: {this.topRight}");
                    comma = true;
                }

                if (this.bottomLeft != Radius.zero) {
                    if (comma) {
                        result.Append(", ");
                    }

                    result.Append($"bottomLeft: {this.bottomLeft}");
                    comma = true;
                }

                if (this.bottomRight != Radius.zero) {
                    if (comma) {
                        result.Append(", ");
                    }

                    result.Append($"bottomRight: {this.bottomRight}");
                }

                result.Append(")");
                visual = result.ToString();
            }

            if (visual != null) {
                return visual;
            }

            return "BorderRadius.zero";
        }
    }
}