using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace UIWidgets.ui {
    
    public enum FontStyle {
        /// Use the upright glyphs
        normal,

        /// Use glyphs designed for slanting
        italic,
    }

    public enum TextBaseline {
        alphabetic,
        ideographic,
    }

    public enum TextAlign {
        /// Align the text on the left edge of the container.
        left,

        /// Align the text on the right edge of the container.
        right,

        /// Align the text in the center of the container.
        center,

        /// Stretch lines of text that end with a soft line break to fill the width of
        /// the container.
        ///
        /// Lines that end with hard line breaks are aligned towards the [start] edge.
        justify,
    }

    public class ParagraphConstraints: IEquatable<ParagraphConstraints>
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

    public class TextStyle:IEquatable<TextStyle>
    {
        public static readonly string defaultFontFamily = "Helvetica";
        public static readonly double defaultFontSize = 14.0;
        public static readonly FontWeight defaultFontWeight = FontWeight.w400;
        public static readonly FontStyle defaultFontStyle = FontStyle.normal;
        public static readonly Color defaultColor = Color.fromARGB(255, 0, 0, 0);
        public Color color;
        public double? fontSize;
        public FontWeight? fontWeight;
        public FontStyle? fontStyle;
        public double? letterSpacing;
        public double? wordSpacing;
        public TextBaseline? textBaseline;
        public double? height;
        public TextDecoration decoration;
        public string fontFamily;

        public FontStyle safeFontStyle
        {
            get { return fontStyle ?? defaultFontStyle; }
        }

        public string safeFontFamily
        {
            get { return fontFamily ?? defaultFontFamily; }
        }

        public double safeFontSize
        {
            get { return fontSize ?? defaultFontSize; }
        }

        public FontWeight safeFontWeight
        {
            get { return fontWeight ?? defaultFontWeight; }
        }

        public UnityEngine.Color UnityColor
        {
            get { return (color ?? defaultColor).toColor(); }
        }
        
        public UnityEngine.FontStyle UnityFontStyle
        {
            get
            {
                if (safeFontStyle == FontStyle.italic)
                {
                    if (safeFontWeight == FontWeight.w700)
                    {
                        return UnityEngine.FontStyle.BoldAndItalic;
                    }
                    else
                    {
                        return UnityEngine.FontStyle.Italic;
                    }
                } else if (safeFontWeight == FontWeight.w700)
                {
                    return UnityEngine.FontStyle.Bold;
                }

                return UnityEngine.FontStyle.Normal;
            }
        }

        public int UnityFontSize
        {
            get { return (int) safeFontSize; }
        }
        
        public TextStyle merge(TextStyle style)
        {
            var ret = new TextStyle();
            ret.color = style.color??color;
            ret.fontSize = style.fontSize??fontSize;
            ret.fontWeight = style.fontWeight??fontWeight;
            ret.fontStyle = style.fontStyle??fontStyle;
            ret.letterSpacing = style.letterSpacing??letterSpacing;
            ret.textBaseline = style.textBaseline??textBaseline;
            ret.height = style.height??height;
            ret.decoration = style.decoration??decoration;
            ret.fontFamily = style.fontFamily??fontFamily;
            return ret;
        }
        
        public bool Equals(TextStyle other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return color == other.color && fontSize == other.fontSize && fontWeight == other.fontWeight && 
                   fontStyle == other.fontStyle && letterSpacing == other.letterSpacing && 
                   wordSpacing == other.wordSpacing && textBaseline == other.textBaseline && 
                   height == other.height &&  decoration == other.decoration && fontFamily == other.fontFamily;
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

        public TextStyle(Color color = null, double? fontSize = default(double?), FontWeight? fontWeight = default(FontWeight?), FontStyle? fontStyle = default(FontStyle?), double? letterSpacing = default(double?), double? wordSpacing = default(double?), TextBaseline? textBaseline = default(TextBaseline?), double? height = default(double?), TextDecoration decoration = null, string fontFamily = null)
        {
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
        }
    }
    
    public class ParagraphStyle: IEquatable<ParagraphStyle>
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
            return textAlign == other.textAlign && textDirection == other.textDirection && fontWeight == other.fontWeight && fontStyle == other.fontStyle && maxLines == other.maxLines && fontSize.Equals(other.fontSize) && string.Equals(fontFamily, other.fontFamily) && lineHeight.Equals(other.lineHeight) && string.Equals(ellipsis, other.ellipsis);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ParagraphStyle) obj);
        }
        
        public static bool operator ==(ParagraphStyle a, ParagraphStyle b) {
            return Equals(a, b);
        }

        public static bool operator !=(ParagraphStyle a, ParagraphStyle b) {
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
    
    public class TextDecoration: IEquatable<TextDecoration>
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
        
        
        public static bool operator ==(TextDecoration a, TextDecoration b) {
            return Equals(a, b);
        }

        public static bool operator !=(TextDecoration a, TextDecoration b) {
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
        
        bool contains(TextDecoration other) {
            return (mask | other.mask) == mask;
        }
    }
    
    public enum TextDirection {
        rtl,
        ltr,
    }
    
    public enum TextAffinity {
        upstream,
        downstream,
    }

    public enum FontWeight
    {
        w400, // normal
        w700, // bold
    }

    public class TextPosition
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

        protected bool Equals(TextPosition other)
        {
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
    }
}