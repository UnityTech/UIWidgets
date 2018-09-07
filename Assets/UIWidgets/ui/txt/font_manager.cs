using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UIWidgets.ui
{
    public class FontEntry
    {
        public readonly Font font;
        private int _textureBuildVersion = 0;
        
        public FontEntry(Font font)
        {
            this.font = font;
            
        }

        public int textureBuildVersion
        {
            get { return _textureBuildVersion; }
        }

        internal void onFontTextureRebuild()
        {
            _textureBuildVersion++;
        }
    }
    
    public class FontManager
    {
        private List<FontEntry> _fonts = new List<FontEntry>();
        
        public static readonly FontManager instance = new FontManager();

        private FontManager()
        {
            Font.textureRebuilt += this.onFontTextureRebuilt;
        }
        
        public FontEntry getOrCreate(string[] names, int fontSize)
        {
            var founded = _fonts.Find((font) =>
                (
                font.font.fontSize == fontSize &&
                    (names == font.font.fontNames || (names != null && names.SequenceEqual(font.font.fontNames)))));
            if (founded != null)
            {
                return founded;
            }
            
            Debug.Log(string.Format("Create new Font names={0}, size={1}", names, fontSize));
            var newFont = new FontEntry(Font.CreateDynamicFontFromOSFont(names,
                fontSize));
            _fonts.Add(newFont);
            return newFont;
        }

        public FontEntry getOrCreate(string name, int fontSize)
        {
            return getOrCreate(new []{name}, fontSize);
        }

        private void onFontTextureRebuilt(Font font)
        {
            var entry = _fonts.Find((f) => f.font == font);
            if (entry != null)
            {
                entry.onFontTextureRebuild();
            }
        }
    }
}
