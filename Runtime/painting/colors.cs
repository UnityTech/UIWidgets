using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class HSVColor {
        HSVColor(float alpha, float hue, float saturation, float value) {
            D.assert(this.alpha >= 0);
            D.assert(this.alpha <= 1);
            D.assert(hue >= 0);
            D.assert(hue <= 360);
            D.assert(saturation >= 0);
            D.assert(saturation <= 1);
            D.assert(value >= 0);
            D.assert(value <= 1);
            this.alpha = alpha;
            this.hue = hue;
            this.saturation = saturation;
            this.value = value;
        }

        public static HSVColor fromAHSV(float alpha, float hue, float saturation, float value) {
            return new HSVColor(alpha, hue, saturation, value);
        }

        public HSVColor withAlpha(float alpha) {
            return fromAHSV(alpha, this.hue, this.saturation, this.value);
        }

        public HSVColor withHue(float hue) {
            return fromAHSV(this.alpha, hue, this.saturation, this.value);
        }

        public HSVColor withSaturation(float saturation) {
            return fromAHSV(this.alpha, this.hue, saturation, this.value);
        }

        public HSVColor withValue(float value) {
            return fromAHSV(this.alpha, this.hue, this.saturation, value);
        }

        public Color toColor() {
            float chroma = this.saturation * this.value;
            float secondary = chroma * (1.0f - (((this.hue / 60.0f) % 2.0f) - 1.0f).abs());
            float match = this.value - chroma;

            return ColorUtils._colorFromHue(this.alpha, this.hue, chroma, secondary, match);
        }

        public readonly float alpha;
        public readonly float hue;
        public readonly float saturation;
        public readonly float value;
    }

    public class ColorSwatch<T> : Color {
        public ColorSwatch(
            long primary,
            Dictionary<T, Color> swatch) : base(primary) {
            this._swatch = swatch;
        }

        protected readonly Dictionary<T, Color> _swatch;

        public Color this[T index] {
            get { return this._swatch[index]; }
        }


        public bool Equals(ColorSwatch<T> other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.value == other.value && this._swatch == other._swatch;
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

            return this.Equals((ColorSwatch<T>) obj);
        }

        public static bool operator ==(ColorSwatch<T> left, ColorSwatch<T> right) {
            return Equals(left, right);
        }

        public static bool operator !=(ColorSwatch<T> left, ColorSwatch<T> right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) this.value;
                hashCode = (hashCode * 397) ^ this._swatch.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() {
            return this.GetType() + "(primary value: " + base.ToString() + ")";
        }
    }

    public static class ColorUtils {
        internal static Color _colorFromHue(
            float alpha,
            float hue,
            float chroma,
            float secondary,
            float match
        ) {
            float red;
            float green;
            float blue;
            if (hue < 60.0) {
                red = chroma;
                green = secondary;
                blue = 0.0f;
            }
            else if (hue < 120.0) {
                red = secondary;
                green = chroma;
                blue = 0.0f;
            }
            else if (hue < 180.0) {
                red = 0.0f;
                green = chroma;
                blue = secondary;
            }
            else if (hue < 240.0) {
                red = 0.0f;
                green = secondary;
                blue = chroma;
            }
            else if (hue < 300.0) {
                red = secondary;
                green = 0.0f;
                blue = chroma;
            }
            else {
                red = chroma;
                green = 0.0f;
                blue = secondary;
            }

            return Color.fromARGB((alpha * 0xFF).round(),
                ((red + match) * 0xFF).round(),
                ((green + match) * 0xFF).round(), ((blue + match) * 0xFF).round());
        }
    }
}