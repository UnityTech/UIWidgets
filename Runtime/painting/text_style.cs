using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class TextStyle : Diagnosticable, IEquatable<TextStyle> {
        public static readonly float _defaultFontSize = 14.0f;
        public readonly bool inherit;
        public readonly Color color;
        public readonly Color backgroundColor;
        public readonly float? fontSize;
        public readonly FontWeight fontWeight;
        public readonly FontStyle? fontStyle;
        public readonly float? letterSpacing;
        public readonly float? wordSpacing;
        public readonly TextBaseline? textBaseline;
        public readonly float? height;
        public readonly TextDecoration decoration;
        public readonly Color decorationColor;
        public readonly TextDecorationStyle? decorationStyle;
        public readonly float? decorationThickness;
        public readonly Paint foreground;
        public readonly Paint background;
        public readonly string fontFamily;
        public readonly List<BoxShadow> shadows;

        public List<string> fontFamilyFallback {
            get { return this._fontFamilyFallback; }
        }

        readonly List<string> _fontFamilyFallback;
        public readonly string debugLabel;

        const string _kDefaultDebugLabel = "unknown";

        const string _kColorForegroundWarning = "Cannot provide both a color and a foreground\n" +
                                                "The color argument is just a shorthand for 'foreground: new Paint()..color = color'.";

        const string _kColorBackgroundWarning = "Cannot provide both a backgroundColor and a background\n" +
                                                "The backgroundColor argument is just a shorthand for 'background: new Paint()..color = color'.";

        public TextStyle(bool inherit = true,
            Color color = null,
            Color backgroundColor = null,
            float? fontSize = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            float? letterSpacing = null,
            float? wordSpacing = null,
            TextBaseline? textBaseline = null,
            float? height = null,
            Paint foreground = null,
            Paint background = null,
            TextDecoration decoration = null,
            Color decorationColor = null,
            TextDecorationStyle? decorationStyle = null,
            float? decorationThickness = null,
            string fontFamily = null,
            List<string> fontFamilyFallback = null,
            List<BoxShadow> shadows = null,
            string debugLabel = null) {
            D.assert(color == null || foreground == null, () => _kColorForegroundWarning);
            D.assert(backgroundColor == null || background == null, () => _kColorBackgroundWarning);
            this.inherit = inherit;
            this.color = color;
            this.backgroundColor = backgroundColor;
            this.fontSize = fontSize;
            this.fontWeight = fontWeight;
            this.fontStyle = fontStyle;
            this.letterSpacing = letterSpacing;
            this.wordSpacing = wordSpacing;
            this.textBaseline = textBaseline;
            this.height = height;
            this.decoration = decoration;
            this.decorationColor = decorationColor;
            this.decorationStyle = decorationStyle;
            this.decorationThickness = decorationThickness;
            this.fontFamily = fontFamily;
            this._fontFamilyFallback = fontFamilyFallback;
            this.debugLabel = debugLabel;
            this.foreground = foreground;
            this.background = background;
            this.shadows = shadows;
        }

        public RenderComparison compareTo(TextStyle other) {
            if (this.inherit != other.inherit || this.fontFamily != other.fontFamily ||
                this.fontSize != other.fontSize || this.fontWeight != other.fontWeight ||
                this.fontStyle != other.fontStyle || this.letterSpacing != other.letterSpacing ||
                this.wordSpacing != other.wordSpacing || this.textBaseline != other.textBaseline ||
                this.height != other.height || this.background != other.background ||
                this.shadows.equalsList(other.shadows)) {
                return RenderComparison.layout;
            }

            if (this.color != other.color || this.decoration != other.decoration ||
                this.decorationColor != other.decorationColor ||
                this.decorationStyle != other.decorationStyle) {
                return RenderComparison.paint;
            }

            return RenderComparison.identical;
        }

        public ParagraphStyle getParagraphStyle(TextAlign textAlign,
            TextDirection textDirection, string ellipsis, int? maxLines, float textScaleFactor = 1.0f) {
            return new ParagraphStyle(
                textAlign, textDirection, this.fontWeight, this.fontStyle,
                maxLines, (this.fontSize ?? _defaultFontSize) * textScaleFactor, this.fontFamily, this.height,
                ellipsis
            );
        }


        public TextStyle apply(
            Color color = null,
            Color backgroundColor = null,
            TextDecoration decoration = null,
            Color decorationColor = null,
            TextDecorationStyle? decorationStyle = null,
            float decorationThicknessFactor = 1.0f,
            float decorationThicknessDelta = 0.0f,
            string fontFamily = null,
            List<string> fontFamilyFallback = null,
            List<BoxShadow> shadows = null,
            float fontSizeFactor = 1.0f,
            float fontSizeDelta = 0.0f,
            int fontWeightDelta = 0,
            float letterSpacingFactor = 1.0f,
            float letterSpacingDelta = 0.0f,
            float wordSpacingFactor = 1.0f,
            float wordSpacingDelta = 0.0f,
            float heightFactor = 1.0f,
            float heightDelta = 0.0f
        ) {
            D.assert(this.fontSize != null || (fontSizeFactor == 1.0f && fontSizeDelta == 0.0f));
            D.assert(this.fontWeight != null || fontWeightDelta == 0.0f);
            D.assert(this.letterSpacing != null || (letterSpacingFactor == 1.0f && letterSpacingDelta == 0.0f));
            D.assert(this.wordSpacing != null || (wordSpacingFactor == 1.0f && wordSpacingDelta == 0.0f));
            D.assert(this.height != null || (heightFactor == 1.0f && heightDelta == 0.0f));
            D.assert(this.decorationThickness != null ||
                     (decorationThicknessFactor == 1.0f && decorationThicknessDelta == 0.0f));

            string modifiedDebugLabel = "";
            D.assert(() => {
                if (this.debugLabel != null) {
                    modifiedDebugLabel = this.debugLabel + ".apply";
                }

                return true;
            });

            return new TextStyle(
                inherit: this.inherit,
                color: this.foreground == null ? color ?? this.color : null,
                backgroundColor: this.background == null ? backgroundColor ?? this.backgroundColor : null,
                fontFamily: fontFamily ?? this.fontFamily,
                fontFamilyFallback: fontFamilyFallback ?? this.fontFamilyFallback,
                fontSize: this.fontSize == null ? null : this.fontSize * fontSizeFactor + fontSizeDelta,
                fontWeight: this.fontWeight == null ? null : this.fontWeight,
                fontStyle: this.fontStyle,
                letterSpacing: this.letterSpacing == null
                    ? null
                    : this.letterSpacing * letterSpacingFactor + letterSpacingDelta,
                wordSpacing: this.wordSpacing == null ? null : this.wordSpacing * wordSpacingFactor + wordSpacingDelta,
                textBaseline: this.textBaseline,
                height: this.height == null ? null : this.height * heightFactor + heightDelta,
                foreground: this.foreground,
                background: this.background,
                decoration: decoration ?? this.decoration,
                decorationColor: decorationColor ?? this.decorationColor,
                decorationStyle: decorationStyle ?? this.decorationStyle,
                decorationThickness: this.decorationThickness == null
                    ? null
                    : this.decorationThickness * decorationThicknessFactor + decorationThicknessDelta,
                shadows: shadows ?? this.shadows,
                debugLabel: modifiedDebugLabel
            );
        }

        public TextStyle merge(TextStyle other) {
            if (other == null) {
                return this;
            }

            if (!other.inherit) {
                return other;
            }

            string mergedDebugLabel = null;
            D.assert(() => {
                if (other.debugLabel != null || this.debugLabel != null) {
                    mergedDebugLabel =
                        $"({this.debugLabel ?? _kDefaultDebugLabel}).merge({other.debugLabel ?? _kDefaultDebugLabel})";
                }

                return true;
            });

            return this.copyWith(
                color: other.color,
                backgroundColor: other.backgroundColor,
                fontFamily: other.fontFamily,
                fontFamilyFallback: other.fontFamilyFallback,
                fontSize: other.fontSize,
                fontWeight: other.fontWeight,
                fontStyle: other.fontStyle,
                letterSpacing: other.letterSpacing,
                wordSpacing: other.wordSpacing,
                textBaseline: other.textBaseline,
                height: other.height,
                foreground: other.foreground,
                background: other.background,
                decoration: other.decoration,
                decorationColor: other.decorationColor,
                decorationStyle: other.decorationStyle,
                decorationThickness: other.decorationThickness,
                shadows: other.shadows,
                debugLabel: mergedDebugLabel
            );
        }

        public TextStyle copyWith(
            bool? inherit = null,
            Color color = null,
            Color backgroundColor = null,
            string fontFamily = null,
            List<string> fontFamilyFallback = null,
            float? fontSize = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            float? letterSpacing = null,
            float? wordSpacing = null,
            TextBaseline? textBaseline = null,
            float? height = null,
            Paint foreground = null,
            Paint background = null,
            TextDecoration decoration = null,
            Color decorationColor = null,
            TextDecorationStyle? decorationStyle = null,
            float? decorationThickness = null,
            List<BoxShadow> shadows = null,
            string debugLabel = null) {
            D.assert(color == null || foreground == null, () => _kColorForegroundWarning);
            D.assert(backgroundColor == null || background == null, () => _kColorBackgroundWarning);
            string newDebugLabel = null;
            D.assert(() => {
                if (this.debugLabel != null) {
                    newDebugLabel = debugLabel ?? $"({this.debugLabel}).copyWith";
                }

                return true;
            });

            return new TextStyle(
                inherit: inherit ?? this.inherit,
                color: this.foreground == null && foreground == null ? color ?? this.color : null,
                backgroundColor: this.background == null && background == null ? color ?? this.color : null,
                fontFamily: fontFamily ?? this.fontFamily,
                fontFamilyFallback: fontFamilyFallback ?? this.fontFamilyFallback,
                fontSize: fontSize ?? this.fontSize,
                fontWeight: fontWeight ?? this.fontWeight,
                fontStyle: fontStyle ?? this.fontStyle,
                letterSpacing: letterSpacing ?? this.letterSpacing,
                wordSpacing: wordSpacing ?? this.wordSpacing,
                textBaseline: textBaseline ?? this.textBaseline,
                height: height ?? this.height,
                decoration: decoration ?? this.decoration,
                decorationColor: decorationColor ?? this.decorationColor,
                decorationStyle: decorationStyle ?? this.decorationStyle,
                decorationThickness: decorationThickness ?? this.decorationThickness,
                foreground: foreground ?? this.foreground,
                background: background ?? this.background,
                shadows: shadows ?? this.shadows,
                debugLabel: newDebugLabel
            );
        }

        public static TextStyle lerp(TextStyle a, TextStyle b, float t) {
            D.assert(a == null || b == null || a.inherit == b.inherit);
            if (a == null && b == null) {
                return null;
            }

            string lerpDebugLabel = "";
            D.assert(() => {
                lerpDebugLabel = "lerp" + (a?.debugLabel ?? _kDefaultDebugLabel) + "-" + t + "-" +
                                 (b?.debugLabel ?? _kDefaultDebugLabel);
                return true;
            });

            if (a == null) {
                return new TextStyle(
                    inherit: b.inherit,
                    color: Color.lerp(null, b.color, t),
                    backgroundColor: Color.lerp(null, b.backgroundColor, t),
                    fontFamily: t < 0.5f ? null : b.fontFamily,
                    fontFamilyFallback: t < 0.5f ? null : b.fontFamilyFallback,
                    fontSize: t < 0.5f ? null : b.fontSize,
                    fontWeight: t < 0.5f ? null : b.fontWeight,
                    fontStyle: t < 0.5f ? null : b.fontStyle,
                    letterSpacing: t < 0.5f ? null : b.letterSpacing,
                    wordSpacing: t < 0.5f ? null : b.wordSpacing,
                    textBaseline: t < 0.5f ? null : b.textBaseline,
                    height: t < 0.5f ? null : b.height,
                    foreground: t < 0.5f ? null : b.foreground,
                    background: t < 0.5f ? null : b.background,
                    decoration: t < 0.5f ? null : b.decoration,
                    decorationColor: Color.lerp(null, b.decorationColor, t),
                    decorationStyle: t < 0.5f ? null : b.decorationStyle,
                    decorationThickness: t < 0.5f ? null : b.decorationThickness,
                    shadows: t < 0.5f ? null : b.shadows,
                    debugLabel: lerpDebugLabel
                );
            }

            if (b == null) {
                return new TextStyle(
                    inherit: a.inherit,
                    color: Color.lerp(a.color, null, t),
                    backgroundColor: Color.lerp(a.backgroundColor, null, t),
                    fontFamily: t < 0.5f ? a.fontFamily : null,
                    fontFamilyFallback: t < 0.5f ? a.fontFamilyFallback : null,
                    fontSize: t < 0.5f ? a.fontSize : null,
                    fontWeight: t < 0.5f ? a.fontWeight : null,
                    fontStyle: t < 0.5f ? a.fontStyle : null,
                    letterSpacing: t < 0.5f ? a.letterSpacing : null,
                    wordSpacing: t < 0.5f ? a.wordSpacing : null,
                    textBaseline: t < 0.5f ? a.textBaseline : null,
                    height: t < 0.5f ? a.height : null,
                    foreground: t < 0.5f ? a.foreground : null,
                    background: t < 0.5f ? a.background : null,
                    decoration: t < 0.5f ? a.decoration : null,
                    decorationColor: Color.lerp(a.decorationColor, null, t),
                    decorationStyle: t < 0.5f ? a.decorationStyle : null,
                    decorationThickness: t < 0.5f ? a.decorationThickness : null,
                    shadows: t < 0.5f ? a.shadows : null,
                    debugLabel: lerpDebugLabel
                );
            }

            return new TextStyle(
                inherit: b.inherit,
                color: a.foreground == null && b.foreground == null ? Color.lerp(a.color, b.color, t) : null,
                backgroundColor: a.background == null && b.background == null
                    ? Color.lerp(a.backgroundColor, b.backgroundColor, t)
                    : null,
                fontFamily: t < 0.5 ? a.fontFamily : b.fontFamily,
                fontFamilyFallback: t < 0.5 ? a.fontFamilyFallback : b.fontFamilyFallback,
                fontSize: MathUtils.lerpNullableFloat(a.fontSize ?? b.fontSize, b.fontSize ?? a.fontSize, t),
                fontWeight: t < 0.5 ? a.fontWeight : b.fontWeight,
                fontStyle: t < 0.5 ? a.fontStyle : b.fontStyle,
                letterSpacing: MathUtils.lerpNullableFloat(a.letterSpacing ?? b.letterSpacing,
                    b.letterSpacing ?? a.letterSpacing, t),
                wordSpacing: MathUtils.lerpNullableFloat(a.wordSpacing ?? b.wordSpacing,
                    b.wordSpacing ?? a.wordSpacing, t),
                textBaseline: t < 0.5 ? a.textBaseline : b.textBaseline,
                height: MathUtils.lerpNullableFloat(a.height ?? b.height, b.height ?? a.height, t),
                foreground: (a.foreground != null || b.foreground != null)
                    ? t < 0.5
                        ? a.foreground ?? new Paint() {color = a.color}
                        : b.foreground ?? new Paint() {color = b.color}
                    : null,
                background: (a.background != null || b.background != null)
                    ? t < 0.5
                        ? a.background ?? new Paint() {color = a.backgroundColor}
                        : b.background ?? new Paint() {color = b.backgroundColor}
                    : null,
                decoration: t < 0.5 ? a.decoration : b.decoration,
                decorationColor: Color.lerp(a.decorationColor, b.decorationColor, t),
                decorationStyle: t < 0.5 ? a.decorationStyle : b.decorationStyle,
                decorationThickness: MathUtils.lerpFloat(
                    a.decorationThickness ?? b.decorationThickness ?? 0.0f,
                    b.decorationThickness ?? a.decorationThickness ?? 0.0f, t),
                shadows: t < 0.5f ? a.shadows : b.shadows,
                debugLabel: lerpDebugLabel
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);

            List<DiagnosticsNode> styles = new List<DiagnosticsNode>();
            styles.Add(new DiagnosticsProperty<Color>("color", this.color,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new DiagnosticsProperty<Color>("backgroundColor", this.backgroundColor,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new StringProperty("family", this.fontFamily, defaultValue: Diagnostics.kNullDefaultValue,
                quoted: false));
            styles.Add(new EnumerableProperty<string>("familyFallback", this.fontFamilyFallback,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new DiagnosticsProperty<float?>("size", this.fontSize,
                defaultValue: Diagnostics.kNullDefaultValue));
            string weightDescription = "";
            if (this.fontWeight != null) {
                weightDescription = this.fontWeight.weightValue.ToString();
            }

            styles.Add(new DiagnosticsProperty<FontWeight>(
                "weight", this.fontWeight,
                description: weightDescription,
                defaultValue: Diagnostics.kNullDefaultValue
            ));
            styles.Add(new EnumProperty<FontStyle?>("style", this.fontStyle,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new DiagnosticsProperty<float?>("letterSpacing", this.letterSpacing,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new DiagnosticsProperty<float?>("wordSpacing", this.wordSpacing,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new EnumProperty<TextBaseline?>("baseline", this.textBaseline,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new DiagnosticsProperty<float?>("height", this.height,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new StringProperty("foreground", this.foreground == null ? null : this.foreground.ToString(),
                defaultValue: Diagnostics.kNullDefaultValue, quoted: false));
            styles.Add(new StringProperty("background", this.background == null ? null : this.background.ToString(),
                defaultValue: Diagnostics.kNullDefaultValue, quoted: false));
            if (this.decoration != null) {
                List<string> decorationDescription = new List<string>();
                if (this.decorationStyle != null) {
                    decorationDescription.Add(this.decorationStyle.ToString());
                }

                styles.Add(new DiagnosticsProperty<Color>("decorationColor", this.decorationColor,
                    defaultValue: Diagnostics.kNullDefaultValue,
                    level: DiagnosticLevel.fine));
                if (this.decorationColor != null) {
                    decorationDescription.Add(this.decorationColor.ToString());
                }

                styles.Add(new DiagnosticsProperty<TextDecoration>("decoration", this.decoration,
                    defaultValue: Diagnostics.kNullDefaultValue,
                    level: DiagnosticLevel.hidden));
                if (this.decoration != null) {
                    decorationDescription.Add("$decoration");
                }

                D.assert(decorationDescription.isNotEmpty);
                styles.Add(new MessageProperty("decoration", string.Join(" ", decorationDescription.ToArray())));
                styles.Add(new FloatProperty("decorationThickness", this.decorationThickness, unit: "x",
                    defaultValue: Diagnostics.kNoDefaultValue));
            }

            bool styleSpecified = styles.Any((DiagnosticsNode n) => !n.isFiltered(DiagnosticLevel.info));
            properties.add(new DiagnosticsProperty<bool>("inherit", this.inherit,
                level: (!styleSpecified && this.inherit) ? DiagnosticLevel.fine : DiagnosticLevel.info));
            foreach (var style in styles) {
                properties.add(style);
            }

            if (!styleSpecified) {
                properties.add(new FlagProperty("inherit", value: this.inherit, ifTrue: "<all styles inherited>",
                    ifFalse: "<no style specified>"));
            }
        }

        public bool Equals(TextStyle other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.inherit == other.inherit &&
                   Equals(this.color, other.color) &&
                   Equals(this.backgroundColor, other.backgroundColor) &&
                   this.fontSize.Equals(other.fontSize) &&
                   this.fontWeight == other.fontWeight &&
                   this.fontStyle == other.fontStyle &&
                   this.letterSpacing.Equals(other.letterSpacing) &&
                   this.wordSpacing.Equals(other.wordSpacing) &&
                   this.textBaseline == other.textBaseline &&
                   this.height.Equals(other.height) &&
                   Equals(this.decoration, other.decoration) &&
                   Equals(this.decorationColor, other.decorationColor) &&
                   this.decorationStyle == other.decorationStyle &&
                   this.decorationThickness == other.decorationThickness &&
                   Equals(this.foreground, other.foreground) &&
                   Equals(this.background, other.background) &&
                   this.fontFamilyFallback.equalsList(other.fontFamilyFallback) &&
                   this.shadows.equalsList(other.shadows) &&
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

            return this.Equals((TextStyle) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.inherit.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.color != null ? this.color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.backgroundColor != null ? this.backgroundColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.fontSize.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.fontWeight != null ? this.fontWeight.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.fontStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ this.letterSpacing.GetHashCode();
                hashCode = (hashCode * 397) ^ this.wordSpacing.GetHashCode();
                hashCode = (hashCode * 397) ^ this.textBaseline.GetHashCode();
                hashCode = (hashCode * 397) ^ this.height.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.decoration != null ? this.decoration.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.decorationColor != null ? this.decorationColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.decorationStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ this.decorationThickness.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.foreground != null ? this.foreground.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.background != null ? this.background.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.fontFamily != null ? this.fontFamily.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^
                           (this.fontFamilyFallback != null ? this.fontFamilyFallback.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.shadows != null ? this.shadows.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(TextStyle left, TextStyle right) {
            return Equals(left, right);
        }

        public static bool operator !=(TextStyle left, TextStyle right) {
            return !Equals(left, right);
        }

        public override string toStringShort() {
            return this.GetType().FullName;
        }
    }
}