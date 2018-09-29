using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UIWidgets.foundation;
using UnityEngine;

namespace UIWidgets.ui
{
    public enum FontStyle
    {
        normal,
        italic,
    }

    public enum TextBaseline
    {
        alphabetic,
        ideographic,
    }

    public enum TextAlign
    {
        left,
        right,
        center,
        justify,
    }

    public class ParagraphConstraints : IEquatable<ParagraphConstraints>
    {
        public readonly double width;

        public ParagraphConstraints(double width)
        {
            this.width = width;
        }

        public bool Equals(ParagraphConstraints other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return width.Equals(other.width);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ParagraphConstraints) obj);
        }

        public override int GetHashCode()
        {
            return width.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Width: {0}", width);
        }
    }

    public class TextStyle : IEquatable<TextStyle>
    {
        public readonly Color color = Color.fromARGB(255, 0, 0, 0);
        public readonly double fontSize = 14.0;
        public readonly FontWeight fontWeight = FontWeight.w400;
        public readonly FontStyle fontStyle = FontStyle.normal;
        public readonly double letterSpacing = 0.0;
        public readonly double wordSpacing = 0.0;
        public readonly TextBaseline textBaseline = TextBaseline.alphabetic;
        public double height = 1.0;
        public readonly TextDecoration decoration = TextDecoration.none;
        public readonly Color decorationColor;
        public readonly TextDecorationStyle decorationStyle = TextDecorationStyle.solid;
        public readonly string fontFamily = "Helvetica";
        public readonly Paint background;
        
        public UnityEngine.Color UnityColor
        {
            get { return color.toColor(); }
        }

        public UnityEngine.FontStyle UnityFontStyle
        {
            get
            {
                if (fontStyle == FontStyle.italic)
                {
                    if (fontWeight == FontWeight.w700)
                    {
                        return UnityEngine.FontStyle.BoldAndItalic;
                    }
                    else
                    {
                        return UnityEngine.FontStyle.Italic;
                    }
                }
                else if (fontWeight == FontWeight.w700)
                {
                    return UnityEngine.FontStyle.Bold;
                }

                return UnityEngine.FontStyle.Normal;
            }
        }

        public int UnityFontSize
        {
            get { return (int) fontSize; }
        }

        public bool Equals(TextStyle other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(color, other.color) && fontSize.Equals(other.fontSize) && fontWeight == other.fontWeight &&
                   fontStyle == other.fontStyle && letterSpacing.Equals(other.letterSpacing) &&
                   wordSpacing.Equals(other.wordSpacing) && textBaseline == other.textBaseline &&
                   height.Equals(other.height) && Equals(decoration, other.decoration) &&
                   Equals(decorationColor, other.decorationColor) && decorationStyle == other.decorationStyle &&
                   string.Equals(fontFamily, other.fontFamily);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextStyle) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (color != null ? color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ fontSize.GetHashCode();
                hashCode = (hashCode * 397) ^ fontWeight.GetHashCode();
                hashCode = (hashCode * 397) ^ fontStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ letterSpacing.GetHashCode();
                hashCode = (hashCode * 397) ^ wordSpacing.GetHashCode();
                hashCode = (hashCode * 397) ^ textBaseline.GetHashCode();
                hashCode = (hashCode * 397) ^ height.GetHashCode();
                hashCode = (hashCode * 397) ^ (decoration != null ? decoration.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (decorationColor != null ? decorationColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ decorationStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ (fontFamily != null ? fontFamily.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(TextStyle left, TextStyle right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TextStyle left, TextStyle right)
        {
            return !Equals(left, right);
        }

        
        public TextStyle(Color color = null, double? fontSize = null,
            FontWeight? fontWeight = null, FontStyle? fontStyle = null, double? letterSpacing = null,
            double? wordSpacing = null, TextBaseline? textBaseline = null, double? height= null, 
            TextDecoration decoration = null, TextDecorationStyle? decorationStyle = null, Color decorationColor = null, string fontFamily = null,
            Paint background = null
        )
        {
            this.color = color ?? this.color;
            this.fontSize = fontSize ?? this.fontSize;
            this.fontWeight = fontWeight ?? this.fontWeight;
            this.fontStyle = fontStyle ?? this.fontStyle;
            this.letterSpacing = letterSpacing ?? this.letterSpacing;
            this.wordSpacing = wordSpacing ?? this.wordSpacing;
            this.fontSize = fontSize ?? this.fontSize;
            this.textBaseline = textBaseline ?? this.textBaseline;
            this.height = height ?? this.height;
            this.decoration = decoration ?? this.decoration;
            this.decorationStyle = decorationStyle ?? this.decorationStyle;
            this.decorationColor = decorationColor ?? this.decorationColor;
            this.fontFamily = fontFamily ?? this.fontFamily;
            this.background = background ?? this.background; 
        }
    }

    public class ParagraphStyle : IEquatable<ParagraphStyle>
    {
        public ParagraphStyle(TextAlign? textAlign = null,
            TextDirection? textDirection = null,
            FontWeight? fontWeight = null,
            FontStyle? fontStyle = null,
            int? maxLines = null,
            double? fontSize = null,
            string fontFamily = null,
            double? lineHeight = null, // todo  
            string ellipsis = null)
        {
            this.textAlign = textAlign;
            this.textDirection = textDirection;
            this.fontWeight = fontWeight;
            this.fontStyle = fontStyle;
            this.maxLines = maxLines;
            this.fontSize = fontSize;
            this.fontFamily = fontFamily;
            this.lineHeight = lineHeight;
            this.ellipsis = ellipsis;
        }

        public bool Equals(ParagraphStyle other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return textAlign == other.textAlign && textDirection == other.textDirection &&
                   fontWeight == other.fontWeight && fontStyle == other.fontStyle && maxLines == other.maxLines &&
                   fontSize.Equals(other.fontSize) && string.Equals(fontFamily, other.fontFamily) &&
                   lineHeight.Equals(other.lineHeight) && string.Equals(ellipsis, other.ellipsis);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ParagraphStyle) obj);
        }

        public static bool operator ==(ParagraphStyle a, ParagraphStyle b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(ParagraphStyle a, ParagraphStyle b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = textAlign.GetHashCode();
                hashCode = (hashCode * 397) ^ textDirection.GetHashCode();
                hashCode = (hashCode * 397) ^ fontWeight.GetHashCode();
                hashCode = (hashCode * 397) ^ fontStyle.GetHashCode();
                hashCode = (hashCode * 397) ^ maxLines.GetHashCode();
                hashCode = (hashCode * 397) ^ fontSize.GetHashCode();
                hashCode = (hashCode * 397) ^ (fontFamily != null ? fontFamily.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ lineHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ (ellipsis != null ? ellipsis.GetHashCode() : 0);
                return hashCode;
            }
        }

        public TextStyle getTextStyle()
        {
            return new TextStyle(
                fontWeight: fontWeight,
                fontStyle: fontStyle,
                fontFamily: fontFamily,
                fontSize: fontSize,
                height: lineHeight
            );
        }

        public TextAlign TextAlign
        {
            get { return textAlign ?? TextAlign.left; }
        }

        public readonly TextAlign? textAlign;
        public readonly TextDirection? textDirection;
        public readonly FontWeight? fontWeight;
        public readonly FontStyle? fontStyle;
        public readonly int? maxLines;
        public readonly double? fontSize;
        public readonly string fontFamily;
        public readonly double? lineHeight;
        public readonly string ellipsis;
    }

    public enum TextDecorationStyle
    {
        solid,
        doubleLine,
    }

    public class TextDecoration : IEquatable<TextDecoration>
    {
        public bool Equals(TextDecoration other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return mask == other.mask;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextDecoration) obj);
        }

        public override int GetHashCode()
        {
            return mask;
        }


        public static bool operator ==(TextDecoration a, TextDecoration b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(TextDecoration a, TextDecoration b)
        {
            return !(a == b);
        }

        public static readonly TextDecoration none = new TextDecoration(0);

        public static readonly TextDecoration underline = new TextDecoration(1);

        public static readonly TextDecoration overline = new TextDecoration(2);

        public static readonly TextDecoration lineThrough = new TextDecoration(4);

        public readonly int mask;

        public TextDecoration(int mask)
        {
            this.mask = mask;
        }

        public bool contains(TextDecoration other)
        {
            return (mask | other.mask) == mask;
        }
    }

    public enum TextDirection
    {
        rtl,
        ltr,
    }

    public enum TextAffinity
    {
        upstream,
        downstream,
    }

    public enum FontWeight
    {
        w400, // normal
        w700, // bold
    }

    public class TextPosition: IEquatable<TextPosition>
    {
        public readonly int offset;
        public readonly TextAffinity affinity;

        public TextPosition(int offset, TextAffinity affinity = TextAffinity.downstream)
        {
            this.offset = offset;
            this.affinity = affinity;
        }

        public override string ToString()
        {
            return string.Format("Offset: {0}, Affinity: {1}", offset, affinity);
        }

        public bool Equals(TextPosition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return offset == other.offset && affinity == other.affinity;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextPosition) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (offset * 397) ^ (int) affinity;
            }
        }

        public static bool operator ==(TextPosition left, TextPosition right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TextPosition left, TextPosition right)
        {
            return !Equals(left, right);
        }
    }

    public class TextBox : IEquatable<TextBox>
    {
        public readonly double left;

        public readonly double top;

        public readonly double right;

        public readonly double bottom;

        public readonly TextDirection direction;

        private TextBox(double left, double top, double right, double bottom, TextDirection direction)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.direction = direction;
        }

        public static TextBox fromLTBD(double left, double top, double right, double bottom, TextDirection direction)
        {
            return new TextBox(left, top, right, bottom, direction);
        }

        public Rect toRect()
        {
            return Rect.fromLTRB(left, top, right, bottom);
        }

        public double start
        {
            get { return direction == TextDirection.ltr ? left : right; }
        }

        public double end
        {
            get { return direction == TextDirection.ltr ? right : left; }
        }

        public bool Equals(TextBox other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return left.Equals(other.left) && top.Equals(other.top) && right.Equals(other.right) &&
                   bottom.Equals(other.bottom) && direction == other.direction;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextBox) obj);
        }

        public override string ToString()
        {
            return string.Format("Left: {0}, Top: {1}, Right: {2}, Bottom: {3}, Direction: {4}", left, top, right,
                bottom, direction);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = left.GetHashCode();
                hashCode = (hashCode * 397) ^ top.GetHashCode();
                hashCode = (hashCode * 397) ^ right.GetHashCode();
                hashCode = (hashCode * 397) ^ bottom.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) direction;
                return hashCode;
            }
        }

        public static bool operator ==(TextBox left, TextBox right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TextBox left, TextBox right)
        {
            return !Equals(left, right);
        }
    }
}