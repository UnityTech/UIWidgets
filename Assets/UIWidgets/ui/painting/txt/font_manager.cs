using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UIWidgets.ui
{
    
    public class FontManager
    {
        private List<Font> _fonts = new List<Font>();
        
        public static readonly FontManager instance = new FontManager();
        
        public Font getOrCreate(string[] names, int fontSize)
        {
            var founded = _fonts.Find((font) =>
                (
                font.fontSize == fontSize &&
                    (names == font.fontNames || (names != null && names.SequenceEqual(font.fontNames)))));
            if (founded != null)
            {
                return founded;
            }
            
            var newFont = Font.CreateDynamicFontFromOSFont(names,
                fontSize);
            _fonts.Add(newFont);
            return newFont;
        }

        public Font getOrCreate(string name, int fontSize)
        {
            return getOrCreate(new []{name}, fontSize);
        }
    }
}
