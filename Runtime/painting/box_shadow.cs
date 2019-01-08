using System;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class BoxShadow : IEquatable<BoxShadow> {
        public BoxShadow(
            Color color = null,
            Offset offset = null,
            double blurRadius = 0.0,
            double spreadRadius = 0.0
        ) {
            this.color = color ?? new Color(0xFF000000);
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
                //blurSigma = this.blurSigma
            };
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
    }
}
