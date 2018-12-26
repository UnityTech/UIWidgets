using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui
{
    public class FontInfo
    {
        public readonly Font font;
        private int _textureVersion;

        public FontInfo(Font font)
        {
            this.font = font;
            this._textureVersion = 0;
        }

        public int textureVersion
        {
            get { return _textureVersion; }
        }

        public void onTextureRebuilt()
        {
            _textureVersion++;
        }
    }

    public class FontManager
    {
        private List<FontInfo> _fonts = new List<FontInfo>();
        private static readonly int defaultFontSize = 14;

        public static readonly FontManager instance = new FontManager();

        private FontManager()
        {
            Font.textureRebuilt += this.onFontTextureRebuilt;
        }

        public FontInfo getOrCreate(string[] names)
        {
            _fonts = _fonts.FindAll((info) => info.font != null); // filter out destoryed fonts
            var founded = _fonts.Find((info) =>
                ( (names == info.font.fontNames || (names != null &&
                                                      names.SequenceEqual(info.font.fontNames)))));
            if (founded != null)
            {
                return founded;
            }

            if (names.SequenceEqual(new string[] {"Material Icons"}))
            {
                var font = Resources.Load<Font>("MaterialIcons-Regular");
                D.assert(font != null);
                var fontInfo = new FontInfo(font);
                _fonts.Add(fontInfo);
                return fontInfo;
            }

            var newFont = new FontInfo(Font.CreateDynamicFontFromOSFont(names, defaultFontSize));
            _fonts.Add(newFont);
            return newFont;
        }

        public FontInfo getOrCreate(string name)
        {
            return getOrCreate(new[] {name});
        }

        private void onFontTextureRebuilt(Font font)
        {
            var id = font.GetInstanceID();
            var entry = _fonts.Find((f) => f.font != null && f.font.GetInstanceID() == id);
            if (entry != null)
            {
                entry.onTextureRebuilt();
            }
        }
    }
}