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
        readonly List<FontInfo> _fonts = new List<FontInfo>();
        static readonly int defaultFontSize = 14;

        public static readonly FontManager instance = new FontManager();

        FontManager() {
            Font.textureRebuilt += this.onFontTextureRebuilt;
        }

        public bool addFont(Font font) {
            var entry = this._fonts.Find(f => f.font == font);
            if (entry != null) {
                return false;
            }

            D.assert(font != null);
            font.hideFlags = HideFlags.DontSave & ~HideFlags.DontSaveInBuild;

            var fontInfo = new FontInfo(font);
            this._fonts.Add(fontInfo);
            return true;
        }

        internal FontInfo getOrCreate(string name) {
            var founded = this._fonts.Find(info =>
                info.font && info.font.fontNames.Contains(name));
            if (founded != null) {
                return founded;
            }

            var osFont = Font.CreateDynamicFontFromOSFont(name, defaultFontSize);
            osFont.hideFlags = HideFlags.DontSave;
            osFont.material.hideFlags = HideFlags.DontSave;
            osFont.material.mainTexture.hideFlags = HideFlags.DontSave;

            var newFont = new FontInfo(osFont);
            this._fonts.Add(newFont);
            return newFont;
        }

        void onFontTextureRebuilt(Font font) {
            var entry = this._fonts.Find(f => f.font == font);
            if (entry != null) {
                entry.onTextureRebuilt();
            }
        }
    }
}