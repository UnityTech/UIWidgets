using System;
using System.Collections.Generic;

namespace UIWidgetsGallery.gallery {
    public class GalleryTextScaleValue : IEquatable<GalleryTextScaleValue> {
        public GalleryTextScaleValue(float? scale = null, string label = null) {
            this.scale = scale;
            this.label = label;
        }

        public readonly float? scale;
        public readonly string label;

        public bool Equals(GalleryTextScaleValue other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return this.scale.Equals(other.scale) && string.Equals(this.label, other.label);
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
            return this.Equals((GalleryTextScaleValue) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.scale.GetHashCode() * 397) ^ (this.label != null ? this.label.GetHashCode() : 0);
            }
        }

        public static bool operator ==(GalleryTextScaleValue left, GalleryTextScaleValue right) {
            return Equals(left, right);
        }

        public static bool operator !=(GalleryTextScaleValue left, GalleryTextScaleValue right) {
            return !Equals(left, right);
        }


        public override string ToString() {
            return $"{this.GetType()}({this.label})";
        }
        
        public static readonly List<GalleryTextScaleValue> kAllGalleryTextScaleValues = new List<GalleryTextScaleValue> {
            new GalleryTextScaleValue(null, "System Default"),
            new GalleryTextScaleValue(0.8f, "Small"),
            new GalleryTextScaleValue(1.0f, "Normal"),
            new GalleryTextScaleValue(1.3f, "Large"),
            new GalleryTextScaleValue(2.0f, "Huge"),
        };
    }
}
