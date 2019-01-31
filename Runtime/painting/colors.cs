using System.Collections.Generic;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
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
}