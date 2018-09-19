using System;
using System.Collections.Generic;
using System.Linq;
using UIWidgets.foundation;
using UIWidgets.painting;
using UIWidgets.ui;

namespace UIWidgets.painting
{
    public class TextStyle : Diagnosticable
    {
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
        public readonly string fontFamily;
        public readonly string debugLabel;
       
        const string _kDefaultDebugLabel = "unknown";


        public TextStyle(bool inherit = true, Color color = null, double? fontSize = null,
            FontWeight? fontWeight = null,
            FontStyle? fontStyle = null, double? letterSpacing = null, double? wordSpacing = null,
            TextBaseline? textBaseline = null, double? height = null, TextDecoration decoration = null,
            string fontFamily = null, string debugLabel = null)
        {
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
            this.fontFamily = fontFamily;
            this.debugLabel = debugLabel;
        }

        public ui.TextStyle getTextStyle(double textScaleFactor = 1.0)
        {
            return new ui.TextStyle(
                color: color,
                decoration: decoration,
                fontWeight: fontWeight,
                fontStyle: fontStyle,
                fontSize: fontSize == null ? null : fontSize * textScaleFactor,
                letterSpacing: letterSpacing,
                wordSpacing: wordSpacing,
                textBaseline: textBaseline,
                height: height,
                fontFamily: fontFamily
            );
        }

        public RenderComparison compareTo(TextStyle other)
        {
            if (inherit != other.inherit || fontFamily != other.fontFamily
                                         || fontSize != other.fontSize || fontWeight != other.fontWeight
                                         || fontStyle != other.fontStyle || letterSpacing != other.letterSpacing
                                         || wordSpacing != other.wordSpacing || textBaseline != other.textBaseline
                                         || height != other.height)
            {
                return RenderComparison.layout;
            }

            if (color != other.color || decoration != other.decoration)
            {
                return RenderComparison.paint;
            }

            return RenderComparison.identical;
        }

        public ParagraphStyle getParagraphStyle(TextAlign textAlign,
            TextDirection textDirection, string ellipsis, int maxLines, double textScaleFactor = 1.0)
        {
            return new ParagraphStyle(
                textAlign, textDirection, fontWeight, fontStyle,
                maxLines, (fontSize ?? _defaultFontSize) * textScaleFactor,
                fontFamily, height, ellipsis
            );
        }

        public TextStyle merge(TextStyle other)
        {
            if (other == null)
            {
                return this;
            }

            if (!other.inherit)
            {
                return other;
            }
            
            string mergedDebugLabel = null;
            D.assert(() =>
            {
                if (other.debugLabel != null || debugLabel != null)
                {
                    mergedDebugLabel = string.Format("({0}).merge({1})", debugLabel??_kDefaultDebugLabel, other.debugLabel ?? _kDefaultDebugLabel);
                }
                return true;
            });

            return copyWith(
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
                debugLabel: mergedDebugLabel
            );
        }

        public TextStyle copyWith(Color color,
            String fontFamily,
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
            string debugLabel = null)
        {
            string newDebugLabel = null;
            D.assert(() => {
                if (this.debugLabel != null)
                {
                    newDebugLabel = debugLabel ?? string.Format("({0}).copyWith", this.debugLabel);
                }  
                return true;
            });
            
            return new TextStyle(
                inherit: inherit,
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
                debugLabel: newDebugLabel
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties)
        {
            base.debugFillProperties(properties);

            List<DiagnosticsNode> styles = new List<DiagnosticsNode>();
            styles.Add(new DiagnosticsProperty<Color>("color", color, defaultValue: null));
            styles.Add(new StringProperty("family", fontFamily, defaultValue: null, quoted: false));
            styles.Add(new DiagnosticsProperty<double?>("size", fontSize, defaultValue: null));
            string weightDescription = "";
            if (fontWeight != null)
            {
                switch (fontWeight)
                {
                    case FontWeight.w400:
                        weightDescription = "400";
                        break;
                    case FontWeight.w700:
                        weightDescription = "700";
                        break;
                }
            }

            styles.Add(new DiagnosticsProperty<FontWeight?>(
                "weight",
                fontWeight,
                description: weightDescription,
                defaultValue: null
            ));
            styles.Add(new EnumProperty<FontStyle?>("style", fontStyle, defaultValue: null));
            styles.Add(new DiagnosticsProperty<double?>("letterSpacing", letterSpacing, defaultValue: null));
            styles.Add(new DiagnosticsProperty<double?>("wordSpacing", wordSpacing, defaultValue: null));
            styles.Add(new EnumProperty<TextBaseline?>("baseline", textBaseline, defaultValue: null));
            styles.Add(new DiagnosticsProperty<double?>("height", height, defaultValue: null));
            if (decoration != null)
            {
                List<String> decorationDescription = new List<String>();
                styles.Add(new DiagnosticsProperty<TextDecoration>("decoration", decoration, defaultValue: null,
                    level: DiagnosticLevel.hidden));
                if (decoration != null)
                    decorationDescription.Add("$decoration");
                D.assert(decorationDescription.isNotEmpty);
                styles.Add(new MessageProperty("decoration", string.Join(" ", decorationDescription.ToArray())));
            }

            bool styleSpecified = styles.Any((DiagnosticsNode n) => !n.isFiltered(DiagnosticLevel.info));
            properties.add(new DiagnosticsProperty<bool>("inherit", inherit,
                level: (!styleSpecified && inherit) ? DiagnosticLevel.fine : DiagnosticLevel.info));
            foreach (var style in styles)
            {
                properties.add(style);
            }

            if (!styleSpecified)
                properties.add(new FlagProperty("inherit", value: inherit, ifTrue: "<all styles inherited>",
                    ifFalse: "<no style specified>"));
        }
    }
}