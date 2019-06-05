using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.UIWidgets.foundation;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Unity.UIWidgets.ui {
    class FontInfo {
        public readonly Font font;
        public readonly Dictionary<int, TMP_FontAsset> fontAsset;
        int _textureVersion;

        public FontInfo(Font font) {
            this.font = font;
            this.fontAsset = new Dictionary<int, TMP_FontAsset>();
            this._textureVersion = 0;
        }

        public int textureVersion {
            get { return this._textureVersion; }
        }

        public void onTextureRebuilt() {
            this._textureVersion++;
        }
    }

    public class FontManager {
        readonly Dictionary<string, FontInfo>[] _fonts =
            new Dictionary<string, FontInfo>[9 * 2]; // max weight size x max style size 

        static Dictionary<Font, FontInfo> _fontInfos = new Dictionary<Font, FontInfo>();

        static readonly int defaultFontSize = 14;
        public static readonly float sampleScale = 8;
        public static readonly float atlasWidth = 2048;
        public static readonly float atlasHeight = 2048;

        public static readonly FontManager instance = new FontManager();

        FontManager() {
            Font.textureRebuilt += this.onFontTextureRebuilt;
        }

        public void addFont(Font font, string familyName,
            FontWeight fontWeight = null, FontStyle fontStyle = FontStyle.normal) {
            fontWeight = fontWeight ?? FontWeight.normal;

            D.assert(font != null);
            D.assert(font.dynamic, () => $"adding font which is not dynamic is not allowed {font.name}");
            font.hideFlags = HideFlags.DontSave & ~HideFlags.DontSaveInBuild;

            var fonts = this._getFonts(fontWeight.index, fontStyle);
            fonts.TryGetValue(familyName, out var current);
            D.assert(current == null || current.font == font,
                () => $"font with key {familyName} {fontWeight} {fontStyle} already exists");
            var fontInfo = new FontInfo(font);
            fonts[familyName] = fontInfo;
            _fontInfos[font] = fontInfo;
        }

        Dictionary<string, FontInfo> _getFonts(int fontWeight, FontStyle fontStyle) {
            var index = fontWeight * 2 + (int) fontStyle;
            var fonts = this._fonts[index];
            if (fonts == null) {
                fonts = this._fonts[index] = new Dictionary<string, FontInfo>();
            }

            return fonts;
        }

        internal static FontInfo _getFontInfo(Font font) {
            return _fontInfos.getOrDefault(font);
        }

        internal FontInfo getOrCreate(string familyName, FontWeight fontWeight, FontStyle fontStyle) {
            fontWeight = fontWeight ?? FontWeight.normal;

            var fonts = this._getFonts(fontWeight.index, fontStyle);
            if (fonts.TryGetValue(familyName, out var fontInfo)) {
                return fontInfo;
            }

            // fallback to normal weight & style
            if (fontWeight.index != FontWeight.normal.index || fontStyle != FontStyle.normal) {
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
            fonts[familyName] = newFont;
            _fontInfos[osFont] = newFont;

            return newFont;
        }

        void onFontTextureRebuilt(Font font) {
            foreach (var fontInfos in this._fonts) {
                if (fontInfos != null) {
                    foreach (var f in fontInfos.Values) {
                        if (f.font == font) {
                            f.onTextureRebuilt();
                        }
                    }
                }
            }
        }

        public static void encodeUTF16Pair(uint character, out char a, out char b) {
            uint code;
            D.assert(0x10000 <= character && character <= 0x10FFFF);
            code = (character - 0x10000);
            a = (char) (0xD800 | (code >> 10));
            b = (char) (0xDC00 | (code & 0x3FF));
        }

        public static uint decodeUTF16Pair(char a, char b) {
            uint code;
            D.assert(0xD800 <= a && a <= 0xDBFF);
            D.assert(0xDC00 <= b && b <= 0xDFFF);
            code = 0x10000;
            code += (uint) ((a & 0x03FF) << 10);
            code += (uint) (b & 0x03FF);
            return code;
        }

        public static uint[] stringToUnicode(string text) {
            if (text == null) {
                return null;
            }

            List<uint> uints = new List<uint>(capacity: text.Length);
            for (int i = 0; i < text.Length; i++) {
                char a = text[i];
                if (0xD800 <= a && a <= 0xDBFF && i < text.Length - 1) {
                    uints.Add(decodeUTF16Pair(a, text[i + 1]));
                    i++;
                }
                else {
                    uints.Add(a);
                }
            }

            return uints.ToArray();
        }
        
        public static uint[] stringToUnicodeUnique(string text) {
            if (text == null) {
                return null;
            }

            HashSet<uint> uints = new HashSet<uint>();
            for (int i = 0; i < text.Length; i++) {
                char a = text[i];
                if (0xD800 <= a && a <= 0xDBFF && i < text.Length - 1) {
                    uints.Add(decodeUTF16Pair(a, text[i + 1]));
                    i++;
                }
                else {
                    uints.Add(a);
                }
            }

            return uints.ToArray();
        }

        public static bool isSurrogatePairStart(uint c) {
            return 0xD800 <= c && c <= 0xDBFF;
        }

        public static bool isSurrogatePairEnd(uint c) {
            return 0xDC00 <= c && c <= 0xDFFF;
        }

        public static string unicodeToString(uint[] uints, int start, int length) {
            if (uints == null) {
                return null;
            }

            D.assert(start >= 0 && start + length <= uints.Length);
            StringBuilder sb = new StringBuilder();
            for (int i = start; i < start + length; i++) {
                if (0x10000 <= uints[i] && uints[i] <= 0x10FFFF) {
                    encodeUTF16Pair(uints[i], out char a, out char b);
                    sb.Append(a);
                    sb.Append(b);
                }
                else {
                    sb.Append((char) uints[i]);
                }
            }

            return sb.ToString();
        }

        public static string unicodeToString(uint[] uints) {
            return unicodeToString(uints, 0, uints.Length);
        }
    }

    public static class FontExtension {
        internal static bool getGlyphInfo(this Font font, uint ch, out CharacterInfo info, int fontSize,
            UnityEngine.FontStyle fontStyle) {
            if (fontSize <= 0) {
                info = default;
                return false;
            }
            
            // If ch is utf-16
//            if (ch <= 0xffff) {
//                bool success = font.GetCharacterInfo((char) ch, out info, fontSize, fontStyle);
//                if (!success) {
//                    if (!char.IsControl((char) ch)) {
//                        Debug.LogWarning(
//                            $"character info not found from the given font: character '{(char) ch}' (code{ch}) font: ${font.name}");
//                    }
//
//                    info = default;
//                    return false;
//                }
//            }
//            else {
            var fontInfo = FontManager._getFontInfo(font);
            if (fontInfo == null) {
                Debug.LogWarning($"font asset not found for font: {font.name}");
                info = default;
                return false;
            }

            if (!fontInfo.fontAsset.ContainsKey(fontSize)) {
                Debug.LogWarning($"font asset not created for font {font.name} and font size {fontSize}");
                info = default;
                return false;
            }

            FontStyles tmproStyle = FontStyles.Normal;
            TMPro.FontWeight weight = TMPro.FontWeight.Regular;
            if (fontStyle == UnityEngine.FontStyle.Bold) {
                tmproStyle = FontStyles.Bold;
                weight = TMPro.FontWeight.Bold;
            }
            else if (fontStyle == UnityEngine.FontStyle.Italic) {
                tmproStyle = FontStyles.Italic;
                weight = TMPro.FontWeight.Regular;
            }
            else if (fontStyle == UnityEngine.FontStyle.BoldAndItalic) {
                tmproStyle = FontStyles.Bold | FontStyles.Italic;
                weight = TMPro.FontWeight.Bold;
            }

            var character = TMP_FontAssetUtilities.GetCharacterFromFontAsset(ch, fontInfo.fontAsset[fontSize], true,
                tmproStyle, weight, out var isAlternativeTypeface, out var fontAsset);
            if (character == null) {
                Debug.LogWarning($"character info not found for character code {ch} font: {font.name}");
                info = default;
                return false;
            }

            info = new CharacterInfo() {
                index = (int) character.unicode,
                advance = Mathf.RoundToInt(character.glyph.metrics.horizontalAdvance / FontManager.sampleScale),
                size = fontSize, // (int) character.scale,
                style = fontStyle,
                glyphWidth = (int) (character.glyph.metrics.width / FontManager.sampleScale),
                glyphHeight = (int) (character.glyph.metrics.height / FontManager.sampleScale),
                bearing = (int) (character.glyph.metrics.horizontalBearingX / FontManager.sampleScale),
                minX = (int) (character.glyph.metrics.horizontalBearingX / FontManager.sampleScale),
                minY = (int) ((character.glyph.metrics.horizontalBearingY - character.glyph.metrics.height) / FontManager.sampleScale),
                maxX = (int) ((character.glyph.metrics.width + character.glyph.metrics.horizontalBearingX) / FontManager.sampleScale),
                maxY = (int) (character.glyph.metrics.horizontalBearingY / FontManager.sampleScale),
                uvBottomLeft = new Vector2(
                    character.glyph.glyphRect.x / FontManager.atlasWidth, 
                    character.glyph.glyphRect.y / FontManager.atlasHeight),
                uvBottomRight = new Vector2(
                    (character.glyph.glyphRect.x + character.glyph.glyphRect.width) / FontManager.atlasWidth,
                    character.glyph.glyphRect.y / FontManager.atlasHeight),
                uvTopRight = new Vector2(
                    (character.glyph.glyphRect.x + character.glyph.glyphRect.width) / FontManager.atlasWidth,
                    (character.glyph.glyphRect.y + character.glyph.glyphRect.height) / FontManager.atlasHeight),
                uvTopLeft = new Vector2(
                    character.glyph.glyphRect.x / FontManager.atlasWidth,
                    (character.glyph.glyphRect.y + character.glyph.glyphRect.height) / FontManager.atlasHeight),
            };
//            }

            return true;
        }

        internal static void RequestCharactersInTextureSafe(this Font font, string text, int fontSize,
            UnityEngine.FontStyle fontStyle = UnityEngine.FontStyle.Normal) {
            if (fontSize <= 0) {
                return;
            }
            // font.RequestCharactersInTexture(text, fontSize, fontStyle);

            var fontInfo = FontManager._getFontInfo(font);
            if (fontInfo == null) {
                Debug.LogWarning($"font asset not found for font: {font.name}");
                return;
            }

            if (!fontInfo.fontAsset.ContainsKey(fontSize)) {
                var fontAsset = TMP_FontAsset.CreateFontAsset(font, 
                    (int) (fontSize * FontManager.sampleScale), 9, GlyphRenderMode.SDFAA,
                    (int) FontManager.atlasWidth, (int) FontManager.atlasHeight);
                fontInfo.fontAsset.Add(fontSize, fontAsset);
            }
            fontInfo.fontAsset[fontSize].TryAddCharacters(FontManager.stringToUnicodeUnique(text));
        }
    }
}