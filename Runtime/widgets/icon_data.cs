using System;

namespace Unity.UIWidgets.widgets {
    public class IconData : IEquatable<IconData> {
        public IconData(
            int codePoint,
            string fontFamily = null
        ) {
            this.codePoint = codePoint;
            this.fontFamily = fontFamily;
        }

        public readonly int codePoint;

        public readonly string fontFamily;

        public bool Equals(IconData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.codePoint == other.codePoint &&
                   string.Equals(this.fontFamily, other.fontFamily);
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

            return this.Equals((IconData) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.codePoint * 397) ^ (this.fontFamily != null ? this.fontFamily.GetHashCode() : 0);
            }
        }

        public static bool operator ==(IconData left, IconData right) {
            return Equals(left, right);
        }

        public static bool operator !=(IconData left, IconData right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return "IconData(U+" + this.codePoint.ToString("X5") + ")";
        }
    }
}