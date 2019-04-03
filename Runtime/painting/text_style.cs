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
        public readonly Paint background;
        public readonly string fontFamily;
        public readonly string debugLabel;

        const string _kDefaultDebugLabel = "unknown";


        public TextStyle(bool inherit = true, Color color = null, float? fontSize = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null, float? letterSpacing = null, float? wordSpacing = null,
            TextBaseline? textBaseline = null, float? height = null, Paint background = null,
            TextDecoration decoration = null,
            Color decorationColor = null, TextDecorationStyle? decorationStyle = null,
            string fontFamily = null, string debugLabel = null) {
            this.inherit = inherit;
            this.color = color;
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
            this.fontFamily = fontFamily;
            this.debugLabel = debugLabel;
            this.background = background;
        }

        public RenderComparison compareTo(TextStyle other) {
            if (this.inherit != other.inherit || this.fontFamily != other.fontFamily
                                              || this.fontSize != other.fontSize || this.fontWeight != other.fontWeight
                                              || this.fontStyle != other.fontStyle ||
                                              this.letterSpacing != other.letterSpacing
                                              || this.wordSpacing != other.wordSpacing ||
                                              this.textBaseline != other.textBaseline
                                              || this.height != other.height || this.background != other.background) {
                return RenderComparison.layout;
            }

            if (this.color != other.color || this.decoration != other.decoration ||
                this.decorationColor != other.decorationColor
                || this.decorationStyle != other.decorationStyle) {
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
            TextDecoration decoration = null,
            Color decorationColor = null,
            TextDecorationStyle? decorationStyle = null,
            string fontFamily = null,
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
            D.assert(this.fontSize != null || (fontSizeFactor == 1.0 && fontSizeDelta == 0.0));
            D.assert(this.fontWeight != null || fontWeightDelta == 0.0);
            D.assert(this.letterSpacing != null || (letterSpacingFactor == 1.0 && letterSpacingDelta == 0.0));
            D.assert(this.wordSpacing != null || (wordSpacingFactor == 1.0 && wordSpacingDelta == 0.0));
            D.assert(this.height != null || (heightFactor == 1.0 && heightDelta == 0.0));

            string modifiedDebugLabel = "";
            D.assert(() => {
                if (this.debugLabel != null) {
                    modifiedDebugLabel = this.debugLabel + ".apply";
                }

                return true;
            });

            return new TextStyle(
                inherit: this.inherit,
                color: color ?? this.color,
                fontFamily: fontFamily ?? this.fontFamily,
                fontSize: this.fontSize == null ? null : this.fontSize * fontSizeFactor + fontSizeDelta,
                fontWeight: this.fontWeight == null ? null : this.fontWeight,
                fontStyle: this.fontStyle,
                letterSpacing: this.letterSpacing == null
                    ? null
                    : this.letterSpacing * letterSpacingFactor + letterSpacingDelta,
                wordSpacing: this.wordSpacing == null ? null : this.wordSpacing * wordSpacingFactor + wordSpacingDelta,
                textBaseline: this.textBaseline,
                height: this.height == null ? null : this.height * heightFactor + heightDelta,
                background: this.background,
                decoration: decoration ?? this.decoration,
                decorationColor: decorationColor ?? this.decorationColor,
                decorationStyle: decorationStyle ?? this.decorationStyle,
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
                fontFamily: other.fontFamily,
                fontSize: other.fontSize,
                fontWeight: other.fontWeight,
                fontStyle: other.fontStyle,
                letterSpacing: other.letterSpacing,
                wordSpacing: other.wordSpacing,
                textBaseline: other.textBaseline,
                height: other.height,
                decoration: other.decoration,
                decorationColor: other.decorationColor,
                decorationStyle: other.decorationStyle,
                background: other.background,
                debugLabel: mergedDebugLabel
            );
        }

        public TextStyle copyWith(Color color = null,
            string fontFamily = null,
            float? fontSize = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            float? letterSpacing = null,
            float? wordSpacing = null,
            TextBaseline? textBaseline = null,
            float? height = null,
            Paint background = null,
            TextDecoration decoration = null,
            Color decorationColor = null,
            TextDecorationStyle? decorationStyle = null,
            string debugLabel = null) {
            string newDebugLabel = null;
            D.assert(() => {
                if (this.debugLabel != null) {
                    newDebugLabel = debugLabel ?? $"({this.debugLabel}).copyWith";
                }

                return true;
            });

            return new TextStyle(
                inherit: this.inherit,
                color: color ?? this.color,
                fontFamily: fontFamily ?? this.fontFamily,
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
                background: background ?? this.background,
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
                    fontFamily: t < 0.5 ? null : b.fontFamily,
                    fontSize: t < 0.5 ? null : b.fontSize,
                    fontWeight: t < 0.5 ? null : b.fontWeight,
                    fontStyle: t < 0.5 ? null : b.fontStyle,
                    letterSpacing: t < 0.5 ? null : b.letterSpacing,
                    wordSpacing: t < 0.5 ? null : b.wordSpacing,
                    textBaseline: t < 0.5 ? null : b.textBaseline,
                    height: t < 0.5 ? null : b.height,
                    background: t < 0.5 ? null : b.background,
                    decoration: t < 0.5 ? null : b.decoration,
                    decorationColor: Color.lerp(null, b.decorationColor, t),
                    decorationStyle: t < 0.5 ? null : b.decorationStyle,
                    debugLabel: lerpDebugLabel
                );
            }

            if (b == null) {
                return new TextStyle(
                    inherit: a.inherit,
                    color: Color.lerp(a.color, null, t),
                    fontFamily: t < 0.5 ? a.fontFamily : null,
                    fontSize: t < 0.5 ? a.fontSize : null,
                    fontWeight: t < 0.5 ? a.fontWeight : null,
                    fontStyle: t < 0.5 ? a.fontStyle : null,
                    letterSpacing: t < 0.5 ? a.letterSpacing : null,
                    wordSpacing: t < 0.5 ? a.wordSpacing : null,
                    textBaseline: t < 0.5 ? a.textBaseline : null,
                    height: t < 0.5 ? a.height : null,
                    background: t < 0.5 ? a.background : null,
                    decoration: t < 0.5 ? a.decoration : null,
                    decorationColor: Color.lerp(a.decorationColor, null, t),
                    decorationStyle: t < 0.5 ? a.decorationStyle : null,
                    debugLabel: lerpDebugLabel
                );
            }

            return new TextStyle(
                inherit: b.inherit,
                color: Color.lerp(a.color, b.color, t),
                fontFamily: t < 0.5 ? a.fontFamily : b.fontFamily,
                fontSize: MathUtils.lerpNullableFloat(a.fontSize ?? b.fontSize, b.fontSize ?? a.fontSize, t),
                fontWeight: t < 0.5 ? a.fontWeight : b.fontWeight,
                fontStyle: t < 0.5 ? a.fontStyle : b.fontStyle,
                letterSpacing: MathUtils.lerpNullableFloat(a.letterSpacing ?? b.letterSpacing,
                    b.letterSpacing ?? a.letterSpacing, t),
                wordSpacing: MathUtils.lerpNullableFloat(a.wordSpacing ?? b.wordSpacing,
                    b.wordSpacing ?? a.wordSpacing, t),
                textBaseline: t < 0.5 ? a.textBaseline : b.textBaseline,
                height: MathUtils.lerpNullableFloat(a.height ?? b.height, b.height ?? a.height, t),
                background: t < 0.5 ? a.background : b.background,
                decoration: t < 0.5 ? a.decoration : b.decoration,
                decorationColor: Color.lerp(a.decorationColor, b.decorationColor, t),
                decorationStyle: t < 0.5 ? a.decorationStyle : b.decorationStyle,
                debugLabel: lerpDebugLabel
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);

            List<DiagnosticsNode> styles = new List<DiagnosticsNode>();
            styles.Add(new DiagnosticsProperty<Color>("color", this.color,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new StringProperty("family", this.fontFamily, defaultValue: Diagnostics.kNullDefaultValue,
                quoted: false));
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

            return this.inherit == other.inherit && Equals(this.color, other.color) &&
                   this.fontSize.Equals(other.fontSize) && this.fontWeight == other.fontWeight &&
                   this.fontStyle == other.fontStyle && this.letterSpacing.Equals(other.letterSpacing) &&
                   this.wordSpacing.Equals(other.wordSpacing) && this.textBaseline == other.textBaseline &&
                   this.height.Equals(other.height) &&
                   Equals(this.decoration, other.decoration) &&
                   Equals(this.decorationColor, other.decorationColor) &&
                   this.decorationStyle == other.decorationStyle && Equals(this.background, other.background) &&
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
                hashCode = (hashCode * 397) ^ (this.background != null ? this.background.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.fontFamily != null ? this.fontFamily.GetHashCode() : 0);
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