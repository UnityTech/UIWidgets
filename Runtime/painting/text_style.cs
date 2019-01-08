using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class TextStyle : Diagnosticable, IEquatable<TextStyle>, ParagraphBuilder.ITextStyleProvider {
        public static readonly double _defaultFontSize = 14.0;
        public readonly bool inherit;
        public readonly Color color;
        public readonly double? fontSize;
        public readonly FontWeight? fontWeight;
        public readonly FontStyle? fontStyle;
        public readonly double? letterSpacing;
        public readonly double? wordSpacing;
        public readonly TextBaseline? textBaseline;
        public readonly double? height;
        public readonly TextDecoration decoration;
        public readonly Color decorationColor;
        public readonly TextDecorationStyle? decorationStyle;
        public readonly Paint background;
        public readonly string fontFamily;
        public readonly string debugLabel;

        const string _kDefaultDebugLabel = "unknown";


        public TextStyle(bool inherit = true, Color color = null, double? fontSize = null,
            FontWeight? fontWeight = null,
            FontStyle? fontStyle = null, double? letterSpacing = null, double? wordSpacing = null,
            TextBaseline? textBaseline = null, double? height = null, Paint background = null,
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

        public ui.TextStyle getTextStyle(ui.TextStyle currentStyle = null) {
            if (currentStyle != null) {
                return new ui.TextStyle(
                    color: this.color ?? currentStyle.color,
                    fontSize: this.fontSize ?? currentStyle.fontSize,
                    fontWeight: this.fontWeight ?? currentStyle.fontWeight,
                    fontStyle: this.fontStyle ?? currentStyle.fontStyle,
                    letterSpacing: this.letterSpacing ?? currentStyle.letterSpacing,
                    wordSpacing: this.wordSpacing ?? currentStyle.wordSpacing,
                    textBaseline: this.textBaseline ?? currentStyle.textBaseline,
                    height: this.height ?? currentStyle.height,
                    decoration: this.decoration ?? currentStyle.decoration,
                    decorationColor: this.decorationColor ?? currentStyle.decorationColor,
                    fontFamily: this.fontFamily ?? currentStyle.fontFamily,
                    background: this.background ?? currentStyle.background
                );
            }

            return new ui.TextStyle(
                color: this.color,
                fontSize: this.fontSize,
                fontWeight: this.fontWeight,
                fontStyle: this.fontStyle,
                letterSpacing: this.letterSpacing,
                wordSpacing: this.wordSpacing,
                textBaseline: this.textBaseline,
                height: this.height,
                decoration: this.decoration,
                decorationColor: this.decorationColor,
                fontFamily: this.fontFamily,
                background: this.background
            );
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
            TextDirection textDirection, string ellipsis, int maxLines, double textScaleFactor = 1.0) {
            return new ParagraphStyle(
                textAlign, textDirection, this.fontWeight, this.fontStyle,
                maxLines, (this.fontSize ?? _defaultFontSize) * textScaleFactor, this.fontFamily, this.height,
                ellipsis
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
                debugLabel: mergedDebugLabel
            );
        }

        public TextStyle copyWith(Color color,
            string fontFamily,
            double? fontSize,
            FontWeight? fontWeight,
            FontStyle? fontStyle,
            double? letterSpacing,
            double? wordSpacing,
            TextBaseline? textBaseline = null,
            double? height = null,
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

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);

            List<DiagnosticsNode> styles = new List<DiagnosticsNode>();
            styles.Add(new DiagnosticsProperty<Color>("color", this.color,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new StringProperty("family", this.fontFamily, defaultValue: Diagnostics.kNullDefaultValue,
                quoted: false));
            styles.Add(new DiagnosticsProperty<double?>("size", this.fontSize,
                defaultValue: Diagnostics.kNullDefaultValue));
            string weightDescription = "";
            if (this.fontWeight != null) {
                switch (this.fontWeight) {
                    case FontWeight.w400:
                        weightDescription = "400";
                        break;
                    case FontWeight.w700:
                        weightDescription = "700";
                        break;
                }
            }

            styles.Add(new DiagnosticsProperty<FontWeight?>(
                "weight", this.fontWeight,
                description: weightDescription,
                defaultValue: Diagnostics.kNullDefaultValue
            ));
            styles.Add(new EnumProperty<FontStyle?>("style", this.fontStyle,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new DiagnosticsProperty<double?>("letterSpacing", this.letterSpacing,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new DiagnosticsProperty<double?>("wordSpacing", this.wordSpacing,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new EnumProperty<TextBaseline?>("baseline", this.textBaseline,
                defaultValue: Diagnostics.kNullDefaultValue));
            styles.Add(new DiagnosticsProperty<double?>("height", this.height,
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
                hashCode = (hashCode * 397) ^ this.fontWeight.GetHashCode();
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
