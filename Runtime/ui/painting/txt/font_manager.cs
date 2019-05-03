using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    class FontInfo {
        public readonly Font font;
        int _textureVersion;

        public FontInfo(Font font) {
            this.font = font;
            this._textureVersion = 0;
        }

        public int textureVersion {
            get { return this._textureVersion; }
        }

        public void onTextureRebuilt() {
            this._textureVersion++;
        }
    }

    class FontRef : IEquatable<FontRef> {
        public readonly string familyName;
        public readonly FontWeight fontWeight;
        public readonly FontStyle fontStyle;

        public FontRef(string familyName, FontWeight fontWeight, FontStyle fontStyle) {
            this.familyName = familyName;
            this.fontWeight = fontWeight;
            this.fontStyle = fontStyle;
        }

        public bool Equals(FontRef other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return string.Equals(this.familyName, other.familyName) && this.fontWeight == other.fontWeight && this.fontStyle == other.fontStyle;
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

            return this.Equals((FontRef) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.familyName != null ? this.familyName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.fontWeight != null ? this.fontWeight.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) this.fontStyle;
                return hashCode;
            }
        }

        public static bool operator ==(FontRef left, FontRef right) {
            return Equals(left, right);
        }

        public static bool operator !=(FontRef left, FontRef right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{nameof(this.familyName)}: {this.familyName}, {nameof(this.fontWeight)}: {this.fontWeight}, {nameof(this.fontStyle)}: {this.fontStyle}";
        }
    }
    
    public class FontManager {
        readonly Dictionary<FontRef, FontInfo> _fonts = new Dictionary<FontRef, FontInfo>();

        static readonly int defaultFontSize = 14;

        public static readonly FontManager instance = new FontManager();

        FontManager() {
            Font.textureRebuilt += this.onFontTextureRebuilt;
        }

        public void addFont(Font font, string familyName, 
            FontWeight fontWeight = null, FontStyle fontStyle = FontStyle.normal) {
            if (fontWeight == null) {
                fontWeight = FontWeight.normal;
            }

            FontRef fontRef = new FontRef(familyName, fontWeight, fontStyle);
            D.assert(font != null);
            D.assert(font.dynamic, () => $"adding font which is not dynamic is not allowed {font.name}");
            font.hideFlags = HideFlags.DontSave & ~HideFlags.DontSaveInBuild;

            FontInfo current;
            this._fonts.TryGetValue(fontRef, out current);
            D.assert(current == null || current.font == font, () => $"font with key {fontRef} already exists");
            var fontInfo = new FontInfo(font);
            this._fonts[fontRef] = fontInfo;
        }

        internal FontInfo getOrCreate(string familyName, FontWeight fontWeight, FontStyle fontStyle) {
            if (fontWeight == null) {
                fontWeight = FontWeight.normal;
            }
            FontRef fontRef = new FontRef(familyName, fontWeight, fontStyle);
            if (this._fonts.TryGetValue(fontRef, out var fontInfo)) {
                return fontInfo;
            }

            // fallback to normal weight & style
            if (fontWeight != FontWeight.normal || fontStyle != FontStyle.normal) {
                fontInfo = this.getOrCreate(familyName, FontWeight.normal, FontStyle.normal);
                if (fontInfo != null) {
                    return fontInfo;
                }
            }
            
            var osFont = Font.CreateDynamicFontFromOSFont(familyName, defaultFontSize);
            osFont.hideFlags = HideFlags.DontSave;
            osFont.material.hideFlags = HideFlags.DontSave;
            osFont.material.mainTexture.hideFlags = HideFlags.DontSave;

            var newFont = new FontInfo(osFont);
            fontRef = new FontRef(familyName, fontWeight, fontStyle);
            this._fonts[fontRef] = newFont;

            return newFont;
        }

        void onFontTextureRebuilt(Font font) {
            var entry = this._fonts.Values.FirstOrDefault(f => f.font == font);
            if (entry != null) {
                entry.onTextureRebuilt();
            }
        }
        

    }

    struct GlyphInfo {
        public static readonly GlyphInfo empty = new GlyphInfo();

        Rect _rect;
        readonly CharacterInfo _info;
        
        public GlyphInfo(CharacterInfo info) {
            this._rect = null;
            this._info = info;
        }

        public Rect rect {
            get {
                if (this._rect == null) {
                    this._rect = Rect.fromLTRB(this._info.minX, -this._info.maxY, this._info.maxX, -this._info.minY);
                }
                return this._rect;
            }
        }

        public float advance {
            get { return this._info.advance; }
        }

        public float glyphHeight {
            get { return this._info.advance; }
        }

        public Vector2 uvTopLeft {
            get { return this._info.uvTopLeft; }
        }

        public Vector2 uvTopRight {
            get { return this._info.uvTopRight; }
        }

        public Vector2 uvBottomLeft {
            get { return this._info.uvBottomLeft; }
        }

        public Vector2 uvBottomRight {
            get { return this._info.uvBottomRight; }
        }
    }
    
    public static class FontExtension  
    {
        internal static GlyphInfo getGlyphInfo(this Font font, char ch, int fontSize, UnityEngine.FontStyle fontStyle) {
            if (fontSize <= 0) {
                return GlyphInfo.empty;
            }
            
            CharacterInfo info;
            bool success = font.GetCharacterInfo(ch, out info, fontSize, fontStyle);
            if (!success) {
                if (!char.IsControl(ch)) {
                    Debug.LogWarning($"character info not found from the given font: character '{ch}' (code{(int)ch}) font: ${font.name}");     
                }
                return GlyphInfo.empty;
            }
            return new GlyphInfo(info);
        }
        
        internal static void RequestCharactersInTextureSafe(this Font font, string text, int fontSize, 
                UnityEngine.FontStyle fontStyle = UnityEngine.FontStyle.Normal) {
            if (fontSize <= 0) {
                return;
            }
            font.RequestCharactersInTexture(text, fontSize, fontStyle);
        }
    }
}