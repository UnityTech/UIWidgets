using System;
using UIWidgets.painting;
using UIWidgets.ui;

namespace UIWidgets.painting
{
    public class TextStyle
    {
        public static readonly double _defaultFontSize = 14.0;
        public readonly bool? inherit;
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

        public TextStyle(bool? inherit, Color color, double? fontSize, FontWeight? fontWeight, 
            FontStyle? fontStyle, double? letterSpacing, double? wordSpacing, 
            TextBaseline? textBaseline, double? height, TextDecoration decoration)
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
        }

        public ui.TextStyle getTextStyle(double textScaleFactor = 1.0)
        {
            return new ui.TextStyle(
                color: color,
                decoration: decoration,
                fontWeight: fontWeight,
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
        
        
    }
}