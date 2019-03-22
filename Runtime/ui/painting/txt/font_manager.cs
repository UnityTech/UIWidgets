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
            D.assert(font.dynamic, $"adding font which is not dynamic is not allowed {font.name}");
            font.hideFlags = HideFlags.DontSave & ~HideFlags.DontSaveInBuild;

            FontInfo current;
            var name = font.fontNames[0];
            this._fonts.TryGetValue(name, out current);
            D.assert(current == null || current.font == font, $"font with name {name} already exists, object name={font.name}");
            var fontInfo = new FontInfo(font);
            this._fonts[name] = fontInfo;
        }

        internal FontInfo getOrCreate(string name) {
            if (this._fonts.TryGetValue(name, out var fontInfo)) {
                D.assert(fontInfo.font.fontNames[0] == name);
                return fontInfo;
            }

            var osFont = Font.CreateDynamicFontFromOSFont(name, defaultFontSize);
            osFont.hideFlags = HideFlags.DontSave;
            osFont.material.hideFlags = HideFlags.DontSave;
            osFont.material.mainTexture.hideFlags = HideFlags.DontSave;

            var newFont = new FontInfo(osFont);
            this._fonts[osFont.fontNames[0]] = newFont;

            return newFont;
        }

        void onFontTextureRebuilt(Font font) {
            var entry = this._fonts.Values.FirstOrDefault(f => f.font == font);
            if (entry != null) {
                entry.onTextureRebuilt();
            }
        }
        

    }
    
    public static class FontExtension  
    {  
        public static CharacterInfo getCharacterInfo(this Font font, char ch, int fontSize, UnityEngine.FontStyle fontStyle)  
        {  
            CharacterInfo info;
            bool success = font.GetCharacterInfo(ch, out info, fontSize, fontStyle);
            if (!success) {  
                Debug.LogWarning($"character info not found from the given font: character '{ch}' (code{(int)ch}) font: ${font.name}");     
            }
            return info;
        }
    }
}