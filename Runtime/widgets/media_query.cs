using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public enum Orientation {
        portrait,
        landscape
    }

    public class MediaQueryData : IEquatable<MediaQueryData> {
        public MediaQueryData(
            Size size = null,
            float devicePixelRatio = 1.0f,
            float textScaleFactor = 1.0f,
            Brightness platformBrightness = Brightness.light,
            EdgeInsets viewInsets = null,
            EdgeInsets padding = null,
            bool alwaysUse24HourFormat = false,
            bool accessibleNavigation = false,
            bool invertColors = false,
            bool disableAnimations = false,
            bool boldText = false
        ) {
            this.size = size ?? Size.zero;
            this.devicePixelRatio = devicePixelRatio;
            this.textScaleFactor = textScaleFactor;
            this.platformBrightness = platformBrightness;
            this.viewInsets = viewInsets ?? EdgeInsets.zero;
            this.padding = padding ?? EdgeInsets.zero;
            this.alwaysUse24HourFormat = alwaysUse24HourFormat;
            this.accessibleNavigation = accessibleNavigation;
            this.invertColors = invertColors;
            this.disableAnimations = disableAnimations;
            this.boldText = boldText;
        }

        public static MediaQueryData fromWindow(Window window) {
            return new MediaQueryData(
                size: window.physicalSize / window.devicePixelRatio,
                devicePixelRatio: window.devicePixelRatio,
                textScaleFactor: window.textScaleFactor,
                // platformBrightness: window.platformBrightness, // TODO: remove comment when window.platformBrightness is ready
                viewInsets: EdgeInsets.fromWindowPadding(window.viewInsets, window.devicePixelRatio),
                padding: EdgeInsets.fromWindowPadding(window.padding, window.devicePixelRatio)
//                accessibleNavigation: window.accessibilityFeatures.accessibleNavigation,
//                invertColors: window.accessibilityFeatures.invertColors,
//                disableAnimations: window.accessibilityFeatures.disableAnimations,
//                boldText: window.accessibilityFeatures.boldText,
//                alwaysUse24HourFormat: window.alwaysUse24HourFormat
            );
        }

        public readonly Size size;

        public readonly float devicePixelRatio;

        public readonly float textScaleFactor;

        public readonly Brightness platformBrightness;

        public readonly EdgeInsets viewInsets;

        public readonly EdgeInsets padding;

        public readonly bool alwaysUse24HourFormat;

        public readonly bool accessibleNavigation;

        public readonly bool invertColors;

        public readonly bool disableAnimations;

        public readonly bool boldText;

        public Orientation orientation {
            get { return this.size.width > this.size.height ? Orientation.landscape : Orientation.portrait; }
        }

        public MediaQueryData copyWith(
            Size size = null,
            float? devicePixelRatio = null,
            float? textScaleFactor = null,
            Brightness? platformBrightness = null,
            EdgeInsets viewInsets = null,
            EdgeInsets padding = null,
            bool? alwaysUse24HourFormat = null,
            bool? accessibleNavigation = null,
            bool? invertColors = null,
            bool? disableAnimations = null,
            bool? boldText = null
        ) {
            return new MediaQueryData(
                size: size ?? this.size,
                devicePixelRatio: devicePixelRatio ?? this.devicePixelRatio,
                textScaleFactor: textScaleFactor ?? this.textScaleFactor,
                platformBrightness: platformBrightness ?? this.platformBrightness,
                viewInsets: viewInsets ?? this.viewInsets,
                padding: padding ?? this.padding,
                alwaysUse24HourFormat: alwaysUse24HourFormat ?? this.alwaysUse24HourFormat,
                accessibleNavigation: accessibleNavigation ?? this.accessibleNavigation,
                invertColors: invertColors ?? this.invertColors,
                disableAnimations: disableAnimations ?? this.disableAnimations,
                boldText: boldText ?? this.boldText
            );
        }

        public MediaQueryData removePadding(
            bool removeLeft = false,
            bool removeTop = false,
            bool removeRight = false,
            bool removeBottom = false
        ) {
            if (!(removeLeft || removeTop || removeRight || removeBottom)) {
                return this;
            }

            return new MediaQueryData(
                size: this.size,
                devicePixelRatio: this.devicePixelRatio,
                textScaleFactor: this.textScaleFactor,
                platformBrightness: this.platformBrightness,
                padding: this.padding.copyWith(
                    left: removeLeft ? (float?) 0.0 : null,
                    top: removeTop ? (float?) 0.0 : null,
                    right: removeRight ? (float?) 0.0 : null,
                    bottom: removeBottom ? (float?) 0.0 : null
                ),
                viewInsets: this.viewInsets,
                alwaysUse24HourFormat: this.alwaysUse24HourFormat,
                disableAnimations: this.disableAnimations,
                invertColors: this.invertColors,
                accessibleNavigation: this.accessibleNavigation,
                boldText: this.boldText
            );
        }

        public MediaQueryData removeViewInsets(
            bool removeLeft = false,
            bool removeTop = false,
            bool removeRight = false,
            bool removeBottom = false
        ) {
            if (!(removeLeft || removeTop || removeRight || removeBottom)) {
                return this;
            }

            return new MediaQueryData(
                size: this.size,
                devicePixelRatio: this.devicePixelRatio,
                textScaleFactor: this.textScaleFactor,
                platformBrightness: this.platformBrightness,
                padding: this.padding,
                viewInsets: this.viewInsets.copyWith(
                    left: removeLeft ? (float?) 0.0 : null,
                    top: removeTop ? (float?) 0.0 : null,
                    right: removeRight ? (float?) 0.0 : null,
                    bottom: removeBottom ? (float?) 0.0 : null
                ),
                alwaysUse24HourFormat: this.alwaysUse24HourFormat,
                disableAnimations: this.disableAnimations,
                invertColors: this.invertColors,
                accessibleNavigation: this.accessibleNavigation,
                boldText: this.boldText
            );
        }

        public bool Equals(MediaQueryData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.size, other.size) && this.devicePixelRatio.Equals(other.devicePixelRatio) &&
                   this.textScaleFactor.Equals(other.textScaleFactor) &&
                   Equals(this.platformBrightness, other.platformBrightness) &&
                   Equals(this.viewInsets, other.viewInsets) &&
                   Equals(this.padding, other.padding) &&
                   this.alwaysUse24HourFormat == other.alwaysUse24HourFormat &&
                   this.accessibleNavigation == other.accessibleNavigation && this.invertColors == other.invertColors &&
                   this.disableAnimations == other.disableAnimations && this.boldText == other.boldText;
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

            return this.Equals((MediaQueryData) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.size != null ? this.size.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.devicePixelRatio.GetHashCode();
                hashCode = (hashCode * 397) ^ this.textScaleFactor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.platformBrightness.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.viewInsets != null ? this.viewInsets.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.padding != null ? this.padding.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.alwaysUse24HourFormat.GetHashCode();
                hashCode = (hashCode * 397) ^ this.accessibleNavigation.GetHashCode();
                hashCode = (hashCode * 397) ^ this.invertColors.GetHashCode();
                hashCode = (hashCode * 397) ^ this.disableAnimations.GetHashCode();
                hashCode = (hashCode * 397) ^ this.boldText.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(MediaQueryData left, MediaQueryData right) {
            return Equals(left, right);
        }

        public static bool operator !=(MediaQueryData left, MediaQueryData right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}(" +
                   $"size: {this.size}, " +
                   $"devicePixelRatio: {this.devicePixelRatio:F1}, " +
                   $"textScaleFactor: {this.textScaleFactor:F1}, " +
                   $"platformBrightness: {this.platformBrightness}, " +
                   $"padding: {this.padding}, " +
                   $"viewInsets: {this.viewInsets}, " +
                   $"alwaysUse24HourFormat: {this.alwaysUse24HourFormat}, " +
                   $"accessibleNavigation: {this.accessibleNavigation}" +
                   $"disableAnimations: {this.disableAnimations}" +
                   $"invertColors: {this.invertColors}" +
                   $"boldText: {this.boldText}" +
                   ")";
        }
    }

    public class MediaQuery : InheritedWidget {
        public MediaQuery(
            Key key = null,
            MediaQueryData data = null,
            Widget child = null
        ) : base(key, child) {
            D.assert(child != null);
            D.assert(data != null);
            this.data = data;
        }

        public static MediaQuery removePadding(
            Key key = null,
            BuildContext context = null,
            bool removeLeft = false,
            bool removeTop = false,
            bool removeRight = false,
            bool removeBottom = false,
            Widget child = null
        ) {
            D.assert(context != null);
            return new MediaQuery(
                key: key,
                data: of(context).removePadding(
                    removeLeft: removeLeft,
                    removeTop: removeTop,
                    removeRight: removeRight,
                    removeBottom: removeBottom
                ),
                child: child
            );
        }

        public static MediaQuery removeViewInsets(
            Key key = null,
            BuildContext context = null,
            bool removeLeft = false,
            bool removeTop = false,
            bool removeRight = false,
            bool removeBottom = false,
            Widget child = null
        ) {
            D.assert(context != null);
            return new MediaQuery(
                key: key,
                data: of(context).removeViewInsets(
                    removeLeft: removeLeft,
                    removeTop: removeTop,
                    removeRight: removeRight,
                    removeBottom: removeBottom
                ),
                child: child
            );
        }

        public readonly MediaQueryData data;

        public static MediaQueryData of(BuildContext context, bool nullOk = false) {
            D.assert(context != null);
            MediaQuery query = (MediaQuery) context.inheritFromWidgetOfExactType(typeof(MediaQuery));
            if (query != null) {
                return query.data;
            }

            if (nullOk) {
                return null;
            }

            throw new UIWidgetsError(
                "MediaQuery.of() called with a context that does not contain a MediaQuery.\n" +
                "No MediaQuery ancestor could be found starting from the context that was passed " +
                "to MediaQuery.of(). This can happen because you do not have a WidgetsApp or " +
                "MaterialApp widget (those widgets introduce a MediaQuery), or it can happen " +
                "if the context you use comes from a widget above those widgets.\n" +
                "The context used was:\n" +
                $"  {context}");
        }

        public static float textScaleFactorOf(BuildContext context) {
            return of(context, nullOk: true)?.textScaleFactor ?? 1.0f;
        }

        public static Brightness platformBrightnessOf(BuildContext context) {
            return of(context, nullOk: true)?.platformBrightness ?? Brightness.light;
        }

        public static bool boldTextOverride(BuildContext context) {
            return of(context, nullOk: true)?.boldText ?? false;
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return this.data != ((MediaQuery) oldWidget).data;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<MediaQueryData>("data", this.data, showName: false));
        }
    }
}