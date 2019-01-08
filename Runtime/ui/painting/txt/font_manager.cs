using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    internal class FontInfo {
        public readonly Font font;
        private int _textureVersion;

        public FontInfo(Font font) {
            this.font = font;
            this._textureVersion = 0;
        }

        public int textureVersion {
            get { return _textureVersion; }
        }

        public void onTextureRebuilt() {
            _textureVersion++;
        }
    }

    public class FontManager {
        private List<FontInfo> _fonts = new List<FontInfo>();
        private static readonly int defaultFontSize = 14;

        public static readonly FontManager instance = new FontManager();

        private FontManager() {
            Font.textureRebuilt += this.onFontTextureRebuilt;
        }

        public bool addFont(Font font) {
            var entry = _fonts.Find(f => f.font == font);
            if (entry != null) {
                return false;
            }

            D.assert(font != null);
            font.hideFlags = HideFlags.DontSave & ~HideFlags.DontSaveInBuild;

            var fontInfo = new FontInfo(font);
            _fonts.Add(fontInfo);
            return true;
        }

        internal FontInfo getOrCreate(string[] names) {
            _fonts = _fonts.FindAll(info => info.font != null); // filter out destroyed fonts
            var founded = _fonts.Find(info =>
                names == info.font.fontNames ||
                names != null && names.SequenceEqual(info.font.fontNames));
            if (founded != null) {
                return founded;
            }

            var osFont = Font.CreateDynamicFontFromOSFont(names, defaultFontSize);
            osFont.hideFlags = HideFlags.DontSave;
            osFont.material.hideFlags = HideFlags.DontSave;
            osFont.material.mainTexture.hideFlags = HideFlags.DontSave;

            var newFont = new FontInfo(osFont);
            _fonts.Add(newFont);
            return newFont;
        }

        internal FontInfo getOrCreate(string name) {
            return getOrCreate(new[] {name});
        }

        private void onFontTextureRebuilt(Font font) {
            var entry = _fonts.Find(f => f.font == font);
            if (entry != null) {
                entry.onTextureRebuilt();
            }
        }
    }
}