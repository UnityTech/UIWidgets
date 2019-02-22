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

    public class FontManager {
        readonly Dictionary<string, FontInfo> _fonts = new Dictionary<string, FontInfo>();

        static readonly int defaultFontSize = 14;

        public static readonly FontManager instance = new FontManager();

        FontManager() {
            Font.textureRebuilt += this.onFontTextureRebuilt;
        }

        public void addFont(Font font) {
            D.assert(font != null);
            font.hideFlags = HideFlags.DontSave & ~HideFlags.DontSaveInBuild;

            var fontInfo = new FontInfo(font);
            foreach (var fontName in font.fontNames) {
                this._fonts[fontName] = fontInfo;
            }
        }

        internal FontInfo getOrCreate(string name) {
            if (this._fonts.TryGetValue(name, out var fontInfo)) {
                return fontInfo;
            }

            var osFont = Font.CreateDynamicFontFromOSFont(name, defaultFontSize);
            osFont.hideFlags = HideFlags.DontSave;
            osFont.material.hideFlags = HideFlags.DontSave;
            osFont.material.mainTexture.hideFlags = HideFlags.DontSave;

            var newFont = new FontInfo(osFont);
            foreach (var fontName in osFont.fontNames) {
                this._fonts[fontName] = newFont;
            }

            return newFont;
        }

        void onFontTextureRebuilt(Font font) {
            var entry = this._fonts.Values.FirstOrDefault(f => f.font == font);
            if (entry != null) {
                entry.onTextureRebuilt();
            }
        }
    }
}