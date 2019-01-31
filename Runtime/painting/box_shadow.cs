using System;
using System.Collections.Generic;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class BoxShadow : IEquatable<BoxShadow> {
        public BoxShadow(
            Color color = null,
            Offset offset = null,
            double blurRadius = 0.0,
            double spreadRadius = 0.0
        ) {
            this.color = color ?? Color.black;
            this.offset = offset ?? Offset.zero;
            this.blurRadius = blurRadius;
            this.spreadRadius = spreadRadius;
        }

        public readonly Color color;
        public readonly Offset offset;
        public readonly double blurRadius;
        public readonly double spreadRadius;

        public static double convertRadiusToSigma(double radius) {
            return radius * 0.57735 + 0.5;
        }

        public double blurSigma {
            get { return convertRadiusToSigma(this.blurRadius); }
        }

        public Paint toPaint() {
            return new Paint {
                color = this.color,
                maskFilter = MaskFilter.blur(BlurStyle.normal, this.blurSigma)
            };
        }

        public BoxShadow scale(double factor) {
            return new BoxShadow(
                color: this.color,
                offset: this.offset * factor,
                blurRadius: this.blurRadius * factor,
                spreadRadius: this.spreadRadius * factor
            );
        }

        public static BoxShadow lerp(BoxShadow a, BoxShadow b, double t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b.scale(t);
            }

            if (b == null) {
                return a.scale(1.0 - t);
            }

            return new BoxShadow(
                color: Color.lerp(a.color, b.color, t),
                offset: Offset.lerp(a.offset, b.offset, t),
                blurRadius: MathUtils.lerpDouble(a.blurRadius, b.blurRadius, t),
                spreadRadius: MathUtils.lerpDouble(a.spreadRadius, b.spreadRadius, t)
            );
        }

        public static List<BoxShadow> lerpList(List<BoxShadow> a, List<BoxShadow> b, double t) {
            if (a == null && b == null) {
                return null;
            }

            a = a ?? new List<BoxShadow>();
            b = b ?? new List<BoxShadow>();
            List<BoxShadow> result = new List<BoxShadow>();
            int commonLength = Math.Min(a.Count, b.Count);
            for (int i = 0; i < commonLength; i += 1) {
                result.Add(lerp(a[i], b[i], t));
            }

            for (int i = commonLength; i < a.Count; i += 1) {
                result.Add(a[i].scale(1.0 - t));
            }

            for (int i = commonLength; i < b.Count; i += 1) {
                result.Add(b[i].scale(t));
            }

            return result;
        }

        public bool Equals(BoxShadow other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.color, other.color)
                   && Equals(this.offset, other.offset)
                   && this.blurRadius.Equals(other.blurRadius)
                   && this.spreadRadius.Equals(other.spreadRadius);
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

            return this.Equals((BoxShadow) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.color != null ? this.color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.offset != null ? this.offset.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.blurRadius.GetHashCode();
                hashCode = (hashCode * 397) ^ this.spreadRadius.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(BoxShadow a, BoxShadow b) {
            return Equals(a, b);
        }

        public static bool operator !=(BoxShadow a, BoxShadow b) {
            return !(a == b);
        }

        public override string ToString() {
            return $"BoxShadow({this.color}, {this.offset}, {this.blurRadius}, {this.spreadRadius})";
        }
    }
}