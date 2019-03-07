using System;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace UIWidgetsGallery.gallery {
    public class GalleryOptions : IEquatable<GalleryOptions> {
        public GalleryOptions(
            GalleryTheme theme = null,
            GalleryTextScaleValue textScaleFactor = null,
            float timeDilation = 1.0f,
            RuntimePlatform? platform = null,
            bool showOffscreenLayersCheckerboard = false,
            bool showRasterCacheImagesCheckerboard = false,
            bool showPerformanceOverlay = false
        ) {
            D.assert(theme != null);
            D.assert(textScaleFactor != null);

            this.theme = theme;
            this.textScaleFactor = textScaleFactor;
            this.timeDilation = timeDilation;
            this.platform = platform ?? Application.platform;
            this.showOffscreenLayersCheckerboard = showOffscreenLayersCheckerboard;
            this.showRasterCacheImagesCheckerboard = showRasterCacheImagesCheckerboard;
            this.showPerformanceOverlay = showPerformanceOverlay;
        }

        public readonly GalleryTheme theme;
        public readonly GalleryTextScaleValue textScaleFactor;
        public readonly float timeDilation;
        public readonly RuntimePlatform platform;
        public readonly bool showPerformanceOverlay;
        public readonly bool showRasterCacheImagesCheckerboard;
        public readonly bool showOffscreenLayersCheckerboard;

        public GalleryOptions copyWith(
            GalleryTheme theme = null,
            GalleryTextScaleValue textScaleFactor = null,
            float? timeDilation = null,
            RuntimePlatform? platform = null,
            bool? showPerformanceOverlay = null,
            bool? showRasterCacheImagesCheckerboard = null,
            bool? showOffscreenLayersCheckerboard = null
        ) {
            return new GalleryOptions(
                theme: theme ?? this.theme,
                textScaleFactor: textScaleFactor ?? this.textScaleFactor,
                timeDilation: timeDilation ?? this.timeDilation,
                platform: platform ?? this.platform,
                showPerformanceOverlay: showPerformanceOverlay ?? this.showPerformanceOverlay,
                showOffscreenLayersCheckerboard:
                showOffscreenLayersCheckerboard ?? this.showOffscreenLayersCheckerboard,
                showRasterCacheImagesCheckerboard: showRasterCacheImagesCheckerboard ??
                                                   this.showRasterCacheImagesCheckerboard
            );
        }

        public bool Equals(GalleryOptions other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return Equals(this.theme, other.theme) && Equals(this.textScaleFactor, other.textScaleFactor) &&
                   this.timeDilation.Equals(other.timeDilation) && this.platform == other.platform &&
                   this.showPerformanceOverlay == other.showPerformanceOverlay &&
                   this.showRasterCacheImagesCheckerboard == other.showRasterCacheImagesCheckerboard &&
                   this.showOffscreenLayersCheckerboard == other.showOffscreenLayersCheckerboard;
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
            return this.Equals((GalleryOptions) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.theme != null ? this.theme.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.textScaleFactor != null ? this.textScaleFactor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.timeDilation.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) this.platform;
                hashCode = (hashCode * 397) ^ this.showPerformanceOverlay.GetHashCode();
                hashCode = (hashCode * 397) ^ this.showRasterCacheImagesCheckerboard.GetHashCode();
                hashCode = (hashCode * 397) ^ this.showOffscreenLayersCheckerboard.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(GalleryOptions left, GalleryOptions right) {
            return Equals(left, right);
        }

        public static bool operator !=(GalleryOptions left, GalleryOptions right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.theme})";
        }
    }
}
