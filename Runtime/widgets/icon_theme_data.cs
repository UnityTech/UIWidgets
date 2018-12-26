using System;
using UIWidgets.foundation;
using UIWidgets.ui;

namespace UIWidgets.widgets {
    public class IconThemeData : Diagnosticable, IEquatable<IconThemeData> {
        public IconThemeData(
            Color color = null,
            double? opacity = null,
            double? size = null) {
            this.color = color;
            this._opacity = opacity;
            this.size = size;
        }

        public static IconThemeData fallback() {
            return new IconThemeData(
                color: new Color(0xFF000000),
                opacity: 1.0,
                size: 24.0);
        }

        public IconThemeData copyWith(
            Color color = null,
            double? opacity = null,
            double? size = null) {
            return new IconThemeData(
                color: color ?? this.color,
                opacity: opacity ?? this.opacity,
                size: size ?? this.size
            );
        }

        public IconThemeData merge(IconThemeData other) {
            if (other == null) {
                return this;
            }

            return this.copyWith(
                color: other.color,
                opacity: other.opacity,
                size: other.size
            );
        }

        public bool isConcrete {
            get { return this.color != null && this.opacity != null && this.size != null; }
        }

        public readonly Color color;

        public double? opacity {
            get { return this._opacity == null ? (double?) null : this._opacity.Value.clamp(0.0, 1.0); }
        }

        readonly double? _opacity;

        public readonly double? size;


        public bool Equals(IconThemeData other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return object.Equals(this.color, other.color) &&
                   this._opacity.Equals(other._opacity) &&
                   this.size.Equals(other.size);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((IconThemeData) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.color != null ? this.color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this._opacity.GetHashCode();
                hashCode = (hashCode * 397) ^ this.size.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(IconThemeData left, IconThemeData right) {
            return object.Equals(left, right);
        }

        public static bool operator !=(IconThemeData left, IconThemeData right) {
            return !object.Equals(left, right);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Color>("color", this.color,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DoubleProperty("opacity", this.opacity,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DoubleProperty("size", this.size,
                defaultValue: Diagnostics.kNullDefaultValue));
        }
    }
}