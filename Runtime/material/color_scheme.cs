using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.material {
    public class ColorScheme : Diagnosticable, IEquatable<ColorScheme> {
        public ColorScheme(
            Color primary,
            Color primaryVariant,
            Color secondary,
            Color secondaryVariant,
            Color surface,
            Color background,
            Color error,
            Color onPrimary,
            Color onSecondary,
            Color onSurface,
            Color onBackground,
            Color onError,
            Brightness brightness) {
            D.assert(primary != null);
            D.assert(primaryVariant != null);
            D.assert(secondary != null);
            D.assert(secondaryVariant != null);
            D.assert(surface != null);
            D.assert(background != null);
            D.assert(error != null);
            D.assert(onPrimary != null);
            D.assert(onSecondary != null);
            D.assert(onSurface != null);
            D.assert(onBackground != null);
            D.assert(onError != null);

            this.primary = primary;
            this.primaryVariant = primaryVariant;
            this.secondary = secondary;
            this.secondaryVariant = secondaryVariant;
            this.surface = surface;
            this.background = background;
            this.error = error;
            this.onPrimary = onPrimary;
            this.onSecondary = onSecondary;
            this.onSurface = onSurface;
            this.onBackground = onBackground;
            this.onError = onError;
            this.brightness = brightness;
        }

        public static ColorScheme light(
            Color primary = null,
            Color primaryVariant = null,
            Color secondary = null,
            Color secondaryVariant = null,
            Color surface = null,
            Color background = null,
            Color error = null,
            Color onPrimary = null,
            Color onSecondary = null,
            Color onSurface = null,
            Color onBackground = null,
            Color onError = null,
            Brightness brightness = Brightness.light
        ) {
            primary = primary ?? new Color(0xFF6200EE);
            primaryVariant = primaryVariant ?? new Color(0xFF3700B3);
            secondary = secondary ?? new Color(0xFF03DAC6);
            secondaryVariant = secondaryVariant ?? new Color(0xFF018786);
            surface = surface ?? Colors.white;
            background = background ?? Colors.white;
            error = error ?? new Color(0xFFB00020);
            onPrimary = onPrimary ?? Colors.white;
            onSecondary = onSecondary ?? Colors.black;
            onSurface = onSurface ?? Colors.black;
            onBackground = onBackground ?? Colors.black;
            onError = onError ?? Colors.white;

            return new ColorScheme(
                primary: primary,
                primaryVariant: primaryVariant,
                secondary: secondary,
                secondaryVariant: secondaryVariant,
                surface: surface,
                background: background,
                error: error,
                onPrimary: onPrimary,
                onSecondary: onSecondary,
                onSurface: onSurface,
                onBackground: onBackground,
                onError: onError,
                brightness: brightness
            );
        }

        public static ColorScheme dark(
            Color primary = null,
            Color primaryVariant = null,
            Color secondary = null,
            Color secondaryVariant = null,
            Color surface = null,
            Color background = null,
            Color error = null,
            Color onPrimary = null,
            Color onSecondary = null,
            Color onSurface = null,
            Color onBackground = null,
            Color onError = null,
            Brightness brightness = Brightness.dark
        ) {
            primary = primary ?? new Color(0xFFBB86FC);
            primaryVariant = primaryVariant ?? new Color(0xFF4B01D0);
            secondary = secondary ?? new Color(0xFF03DAC6);
            secondaryVariant = secondaryVariant ?? new Color(0xFF03DAC6);
            surface = surface ?? Colors.black;
            background = background ?? Colors.black;
            error = error ?? new Color(0xFFB00020);
            onPrimary = onPrimary ?? Colors.black;
            onSecondary = onSecondary ?? Colors.black;
            onSurface = onSurface ?? Colors.white;
            onBackground = onBackground ?? Colors.white;
            onError = onError ?? Colors.black;

            return new ColorScheme(
                primary: primary,
                primaryVariant: primaryVariant,
                secondary: secondary,
                secondaryVariant: secondaryVariant,
                surface: surface,
                background: background,
                error: error,
                onPrimary: onPrimary,
                onSecondary: onSecondary,
                onSurface: onSurface,
                onBackground: onBackground,
                onError: onError,
                brightness: brightness
            );
        }

        public static ColorScheme fromSwatch(
            MaterialColor primarySwatch = null,
            Color primaryColorDark = null,
            Color accentColor = null,
            Color cardColor = null,
            Color backgroundColor = null,
            Color errorColor = null,
            Brightness? brightness = Brightness.light) {
            D.assert(brightness != null);
            primarySwatch = primarySwatch ?? Colors.blue;

            bool isDark = brightness == Brightness.dark;
            bool primaryIsDark = _brightnessFor(primarySwatch) == Brightness.dark;
            Color secondary = accentColor ?? (isDark ? Colors.tealAccent[200] : primarySwatch);
            bool secondaryIsDark = _brightnessFor(secondary) == Brightness.dark;

            return new ColorScheme(
                primary: primarySwatch,
                primaryVariant: primaryColorDark ?? (isDark ? Colors.black : primarySwatch[700]),
                secondary: secondary,
                secondaryVariant: isDark ? Colors.tealAccent[700] : primarySwatch[700],
                surface: cardColor ?? (isDark ? Colors.grey[800] : Colors.white),
                background: backgroundColor ?? (isDark ? Colors.grey[700] : primarySwatch[200]),
                error: errorColor ?? Colors.red[700],
                onPrimary: primaryIsDark ? Colors.white : Colors.black,
                onSecondary: secondaryIsDark ? Colors.white : Colors.black,
                onSurface: isDark ? Colors.white : Colors.black,
                onBackground: primaryIsDark ? Colors.white : Colors.black,
                onError: isDark ? Colors.black : Colors.white,
                brightness: brightness ?? Brightness.light
            );
        }


        static Brightness _brightnessFor(Color color) {
            return ThemeData.estimateBrightnessForColor(color);
        }


        public readonly Color primary;

        public readonly Color primaryVariant;

        public readonly Color secondary;

        public readonly Color secondaryVariant;

        public readonly Color surface;

        public readonly Color background;

        public readonly Color error;

        public readonly Color onPrimary;

        public readonly Color onSecondary;

        public readonly Color onSurface;

        public readonly Color onBackground;

        public readonly Color onError;

        public readonly Brightness brightness;

        public ColorScheme copyWith(
            Color primary = null,
            Color primaryVariant = null,
            Color secondary = null,
            Color secondaryVariant = null,
            Color surface = null,
            Color background = null,
            Color error = null,
            Color onPrimary = null,
            Color onSecondary = null,
            Color onSurface = null,
            Color onBackground = null,
            Color onError = null,
            Brightness? brightness = null) {
            return new ColorScheme(
                primary: primary ?? this.primary,
                primaryVariant: primaryVariant ?? this.primaryVariant,
                secondary: secondary ?? this.secondary,
                secondaryVariant: secondaryVariant ?? this.secondaryVariant,
                surface: surface ?? this.surface,
                background: background ?? this.background,
                error: error ?? this.error,
                onPrimary: onPrimary ?? this.onPrimary,
                onSecondary: onSecondary ?? this.onSecondary,
                onSurface: onSurface ?? this.onSurface,
                onBackground: onBackground ?? this.onBackground,
                onError: onError ?? this.onError,
                brightness: brightness ?? this.brightness
            );
        }

        public static ColorScheme lerp(ColorScheme a, ColorScheme b, float t) {
            return new ColorScheme(
                primary: Color.lerp(a.primary, b.primary, t),
                primaryVariant: Color.lerp(a.primaryVariant, b.primaryVariant, t),
                secondary: Color.lerp(a.secondary, b.secondary, t),
                secondaryVariant: Color.lerp(a.secondaryVariant, b.secondaryVariant, t),
                surface: Color.lerp(a.surface, b.surface, t),
                background: Color.lerp(a.background, b.background, t),
                error: Color.lerp(a.error, b.error, t),
                onPrimary: Color.lerp(a.onPrimary, b.onPrimary, t),
                onSecondary: Color.lerp(a.onSecondary, b.onSecondary, t),
                onSurface: Color.lerp(a.onSurface, b.onSurface, t),
                onBackground: Color.lerp(a.onBackground, b.onBackground, t),
                onError: Color.lerp(a.onError, b.onError, t),
                brightness: t < 0.5 ? a.brightness : b.brightness
            );
        }


        public bool Equals(ColorScheme other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return other.primary == this.primary
                   && other.primaryVariant == this.primaryVariant
                   && other.secondary == this.secondary
                   && other.secondaryVariant == this.secondaryVariant
                   && other.surface == this.surface
                   && other.background == this.background
                   && other.error == this.error
                   && other.onPrimary == this.onPrimary
                   && other.onSecondary == this.onSecondary
                   && other.onSurface == this.onSurface
                   && other.onBackground == this.onBackground
                   && other.onError == this.onError
                   && other.brightness == this.brightness;
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

            return this.Equals((ColorScheme) obj);
        }

        public static bool operator ==(ColorScheme left, ColorScheme right) {
            return Equals(left, right);
        }

        public static bool operator !=(ColorScheme left, ColorScheme right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.primary.GetHashCode();
                hashCode = (hashCode * 397) ^ this.primaryVariant.GetHashCode();
                hashCode = (hashCode * 397) ^ this.secondary.GetHashCode();
                hashCode = (hashCode * 397) ^ this.secondaryVariant.GetHashCode();
                hashCode = (hashCode * 397) ^ this.surface.GetHashCode();
                hashCode = (hashCode * 397) ^ this.background.GetHashCode();
                hashCode = (hashCode * 397) ^ this.error.GetHashCode();
                hashCode = (hashCode * 397) ^ this.onPrimary.GetHashCode();
                hashCode = (hashCode * 397) ^ this.onSecondary.GetHashCode();
                hashCode = (hashCode * 397) ^ this.onSurface.GetHashCode();
                hashCode = (hashCode * 397) ^ this.onBackground.GetHashCode();
                hashCode = (hashCode * 397) ^ this.onError.GetHashCode();
                hashCode = (hashCode * 397) ^ this.brightness.GetHashCode();
                return hashCode;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            ColorScheme defaultScheme = light();
            properties.add(new DiagnosticsProperty<Color>("primary", this.primary,
                defaultValue: defaultScheme.primary));
            properties.add(new DiagnosticsProperty<Color>("primaryVariant", this.primaryVariant,
                defaultValue: defaultScheme.primaryVariant));
            properties.add(new DiagnosticsProperty<Color>("secondary", this.secondary,
                defaultValue: defaultScheme.secondary));
            properties.add(new DiagnosticsProperty<Color>("secondaryVariant", this.secondaryVariant,
                defaultValue: defaultScheme.secondaryVariant));
            properties.add(new DiagnosticsProperty<Color>("surface", this.surface,
                defaultValue: defaultScheme.surface));
            properties.add(new DiagnosticsProperty<Color>("background", this.background,
                defaultValue: defaultScheme.background));
            properties.add(new DiagnosticsProperty<Color>("error", this.error, defaultValue: defaultScheme.error));
            properties.add(new DiagnosticsProperty<Color>("onPrimary", this.onPrimary,
                defaultValue: defaultScheme.onPrimary));
            properties.add(new DiagnosticsProperty<Color>("onSecondary", this.onSecondary,
                defaultValue: defaultScheme.onSecondary));
            properties.add(new DiagnosticsProperty<Color>("onSurface", this.onSurface,
                defaultValue: defaultScheme.onSurface));
            properties.add(new DiagnosticsProperty<Color>("onBackground", this.onBackground,
                defaultValue: defaultScheme.onBackground));
            properties.add(new DiagnosticsProperty<Color>("onError", this.onError,
                defaultValue: defaultScheme.onError));
            properties.add(new DiagnosticsProperty<Brightness>("brightness", this.brightness,
                defaultValue: defaultScheme.brightness));
        }
    }
}