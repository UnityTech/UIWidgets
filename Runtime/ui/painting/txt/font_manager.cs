using System.Collections.Generic;
using Unity.UIWidgets.editor;
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

    public class FontManager {
        readonly Dictionary<string, FontInfo>[] _fonts =
            new Dictionary<string, FontInfo>[9 * 2]; // max weight size x max style size 

        static readonly int defaultFontSize = 14;

        public static readonly FontManager instance = new FontManager();

        FontManager() {
            Font.textureRebuilt += this.onFontTextureRebuilt;
        }

        public void addFont(Font font, string familyName,
            FontWeight fontWeight = null, FontStyle fontStyle = FontStyle.normal) {
            if (font == null) {
                D.assert(() => {
                    Debug.LogWarning($"Font missing (when adding font for {familyName})!");
                    return true;
                });
#if UNITY_EDITOR
                if (Resources.Load("fonts/MaterialIcons-Regular") == null) {
                    D.assert(() => {
                        Debug.Log("It appears that you have not imported UIWidgetsResources.");
                        return true;
                    });
                    UIWidgetsResourcesImporterWindow.ShowResourcesImporterWindow();
                }
#endif
                return;
            }

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
        }

        Dictionary<string, FontInfo> _getFonts(int fontWeight, FontStyle fontStyle) {
            var index = fontWeight * 2 + (int) fontStyle;
            var fonts = this._fonts[index];
            if (fonts == null) {
                fonts = this._fonts[index] = new Dictionary<string, FontInfo>();
            }

            return fonts;
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
    }

    public static class FontExtension {
        internal static bool getGlyphInfo(this Font font, char ch, out CharacterInfo info, int fontSize,
            UnityEngine.FontStyle fontStyle) {
            if (fontSize <= 0) {
                info = default;
                return false;
            }

            bool success = font.GetCharacterInfo(ch, out info, fontSize, fontStyle);
            if (!success) {
                if (!char.IsControl(ch)) {
                    D.assert(() => {
                        Debug.LogWarning(
                            $"character info not found from the given font: character '{ch}' (code{(int) ch}) font: ${font.name}");
                        return true;
                    });
                }

                info = default;
                return false;
            }

            return true;
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