using System.Collections.Generic;
using System.Linq;
using UIWidgets.foundation;
using UnityEngine;

namespace UIWidgets.ui
{
    
    public class FontManager
    {
        private List<Font> _fonts = new List<Font>();
        
        public static readonly FontManager instance = new FontManager();
        
        public Font getOrCreate(string[] names, int fontSize)
        {
            _fonts = _fonts.FindAll((font) => font != null); // filter out destoryed fonts
            var founded = _fonts.Find((font) =>
                
                (
                font.fontSize == fontSize &&
                    (names == font.fontNames || (names != null && names.SequenceEqual(font.fontNames)))));
            if (founded != null)
            {
                return founded;
            }

            if (names.SequenceEqual(new string[] {"MaterialIcons"})) {
                var font = Resources.Load<Font>("MaterialIcons-Regular");
                D.assert(font != null);
                _fonts.Add(font);
                return font;
            }

            var newFont = Font.CreateDynamicFontFromOSFont(names, fontSize);
            _fonts.Add(newFont);
            return newFont;
        }

        public Font getOrCreate(string name, int fontSize)
        {
            return getOrCreate(new []{name}, fontSize);
        }
    }
}
