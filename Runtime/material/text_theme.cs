using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class TextTheme : Diagnosticable, IEquatable<TextTheme> {
        public TextTheme(
            TextStyle display4 = null,
            TextStyle display3 = null,
            TextStyle display2 = null,
            TextStyle display1 = null,
            TextStyle headline = null,
            TextStyle title = null,
            TextStyle subhead = null,
            TextStyle body2 = null,
            TextStyle body1 = null,
            TextStyle caption = null,
            TextStyle button = null,
            TextStyle subtitle = null,
            TextStyle overline = null
        ) {
            this.display4 = display4;
            this.display3 = display3;
            this.display2 = display2;
            this.display1 = display1;
            this.headline = headline;
            this.title = title;
            this.subhead = subhead;
            this.body2 = body2;
            this.body1 = body1;
            this.caption = caption;
            this.button = button;
            this.subtitle = subtitle;
            this.overline = overline;
        }


        public readonly TextStyle display4;

        public readonly TextStyle display3;

        public readonly TextStyle display2;

        public readonly TextStyle display1;

        public readonly TextStyle headline;

        public readonly TextStyle title;

        public readonly TextStyle subhead;

        public readonly TextStyle body2;

        public readonly TextStyle body1;

        public readonly TextStyle caption;

        public readonly TextStyle button;

        public readonly TextStyle subtitle;

        public readonly TextStyle overline;


        public TextTheme copyWith(
            TextStyle display4 = null,
            TextStyle display3 = null,
            TextStyle display2 = null,
            TextStyle display1 = null,
            TextStyle headline = null,
            TextStyle title = null,
            TextStyle subhead = null,
            TextStyle body2 = null,
            TextStyle body1 = null,
            TextStyle caption = null,
            TextStyle button = null,
            TextStyle subtitle = null,
            TextStyle overline = null
        ) {
            return new TextTheme(
                display4: display4 ?? this.display4,
                display3: display3 ?? this.display3,
                display2: display2 ?? this.display2,
                display1: display1 ?? this.display1,
                headline: headline ?? this.headline,
                title: title ?? this.title,
                subhead: subhead ?? this.subhead,
                body2: body2 ?? this.body2,
                body1: body1 ?? this.body1,
                caption: caption ?? this.caption,
                button: button ?? this.button,
                subtitle: subtitle ?? this.subtitle,
                overline: overline ?? this.overline
            );
        }

        public TextTheme merge(TextTheme other) {
            if (other == null) {
                return this;
            }

            return this.copyWith(
                display4: this.display4?.merge(other.display4) ?? other.display4,
                display3: this.display3?.merge(other.display3) ?? other.display3,
                display2: this.display2?.merge(other.display2) ?? other.display2,
                display1: this.display1?.merge(other.display1) ?? other.display1,
                headline: this.headline?.merge(other.headline) ?? other.headline,
                title: this.title?.merge(other.title) ?? other.title,
                subhead: this.subhead?.merge(other.subhead) ?? other.subhead,
                body2: this.body2?.merge(other.body2) ?? other.body2,
                body1: this.body1?.merge(other.body1) ?? other.body1,
                caption: this.caption?.merge(other.caption) ?? other.caption,
                button: this.button?.merge(other.button) ?? other.button,
                subtitle: this.subtitle?.merge(other.subtitle) ?? other.subtitle,
                overline: this.overline?.merge(other.overline) ?? other.overline
            );
        }


        public TextTheme apply(
            string fontFamily = null,
            float fontSizeFactor = 1.0f,
            float fontSizeDelta = 0.0f,
            Color displayColor = null,
            Color bodyColor = null,
            TextDecoration decoration = null,
            Color decorationColor = null,
            TextDecorationStyle? decorationStyle = null
        ) {
            return new TextTheme(
                display4: this.display4?.apply(
                    color: displayColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                display3: this.display3?.apply(
                    color: displayColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                display2: this.display2?.apply(
                    color: displayColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                display1: this.display1?.apply(
                    color: displayColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                headline: this.headline?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                title: this.title?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                subhead: this.subhead?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                body2: this.body2?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                body1: this.body1?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                caption: this.caption?.apply(
                    color: displayColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                button: this.button?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                subtitle: this.subtitle?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                ),
                overline: this.overline?.apply(
                    color: bodyColor,
                    decoration: decoration,
                    decorationColor: decorationColor,
                    decorationStyle: decorationStyle,
                    fontFamily: fontFamily,
                    fontSizeFactor: fontSizeFactor,
                    fontSizeDelta: fontSizeDelta
                )
            );
        }

        public static TextTheme lerp(TextTheme a, TextTheme b, float t) {
            return new TextTheme(
                display4: TextStyle.lerp(a?.display4, b?.display4, t),
                display3: TextStyle.lerp(a?.display3, b?.display3, t),
                display2: TextStyle.lerp(a?.display2, b?.display2, t),
                display1: TextStyle.lerp(a?.display1, b?.display1, t),
                headline: TextStyle.lerp(a?.headline, b?.headline, t),
                title: TextStyle.lerp(a?.title, b?.title, t),
                subhead: TextStyle.lerp(a?.subhead, b?.subhead, t),
                body2: TextStyle.lerp(a?.body2, b?.body2, t),
                body1: TextStyle.lerp(a?.body1, b?.body1, t),
                caption: TextStyle.lerp(a?.caption, b?.caption, t),
                button: TextStyle.lerp(a?.button, b?.button, t),
                subtitle: TextStyle.lerp(a?.subtitle, b?.subtitle, t),
                overline: TextStyle.lerp(a?.overline, b?.overline, t)
            );
        }

        public bool Equals(TextTheme other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.display4 == other.display4
                   && this.display3 == other.display3
                   && this.display2 == other.display2
                   && this.display1 == other.display1
                   && this.headline == other.headline
                   && this.title == other.title
                   && this.subhead == other.subhead
                   && this.body2 == other.body2
                   && this.body1 == other.body1
                   && this.caption == other.caption
                   && this.button == other.button
                   && this.subtitle == other.subtitle
                   && this.overline == other.overline;
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

            return this.Equals((TextTheme) obj);
        }

        public static bool operator ==(TextTheme left, TextTheme right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextTheme left, TextTheme right) {
            return !Equals(left, right);
        }

        int? _cachedHashCode = null;
        public override int GetHashCode() {
            if (this._cachedHashCode != null) {
                return this._cachedHashCode.Value;
            }
            unchecked {
                var hashCode = this.display4?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.display3?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.display2?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.display1?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.headline?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.title?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.subhead?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.body2?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.body1?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.caption?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.button?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.subtitle?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.overline?.GetHashCode() ?? 0;

                this._cachedHashCode = hashCode;
                return hashCode;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            TextTheme defaultTheme = new Typography().black;
            properties.add(new DiagnosticsProperty<TextStyle>("display4", this.display4,
                defaultValue: defaultTheme.display4));
            properties.add(new DiagnosticsProperty<TextStyle>("display3", this.display3,
                defaultValue: defaultTheme.display3));
            properties.add(new DiagnosticsProperty<TextStyle>("display2", this.display2,
                defaultValue: defaultTheme.display2));
            properties.add(new DiagnosticsProperty<TextStyle>("display1", this.display1,
                defaultValue: defaultTheme.display1));
            properties.add(new DiagnosticsProperty<TextStyle>("headline", this.headline,
                defaultValue: defaultTheme.headline));
            properties.add(new DiagnosticsProperty<TextStyle>("title", this.title, defaultValue: defaultTheme.title));
            properties.add(
                new DiagnosticsProperty<TextStyle>("subhead", this.subhead, defaultValue: defaultTheme.subhead));
            properties.add(new DiagnosticsProperty<TextStyle>("body2", this.body2, defaultValue: defaultTheme.body2));
            properties.add(new DiagnosticsProperty<TextStyle>("body1", this.body1, defaultValue: defaultTheme.body1));
            properties.add(
                new DiagnosticsProperty<TextStyle>("caption", this.caption, defaultValue: defaultTheme.caption));
            properties.add(
                new DiagnosticsProperty<TextStyle>("button", this.button, defaultValue: defaultTheme.button));
            properties.add(new DiagnosticsProperty<TextStyle>("subtitle)", this.subtitle,
                defaultValue: defaultTheme.subtitle));
            properties.add(new DiagnosticsProperty<TextStyle>("overline", this.overline,
                defaultValue: defaultTheme.overline));
        }
    }
}