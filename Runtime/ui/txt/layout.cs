using System.Collections.Generic;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class Layout {
        int _start;
        int _count;
        List<float> _advances = new List<float>();
        List<float> _positions = new List<float>();
        float _advance;
        Rect _bounds;
        string _text;
        TabStops _tabStops;


        public static float measureText(float offset, string buf, int start, int count, TextStyle style,
            List<float> advances, int advanceOffset, TabStops tabStops) {
            Layout layout = new Layout();
            layout.setTabStops(tabStops);
            layout.doLayout(offset, buf, start, count, style);
            if (advances != null) {
                var layoutAdv = layout.getAdvances();
                for (int i = 0; i < count; i++) {
                    advances[i + advanceOffset] = layoutAdv[i];
                }
            }

            return layout.getAdvance();
        }

        public void doLayout(float offset, string text, int start, int count, TextStyle style) {
            this._text = text;
            this._advances.Clear();
            this._positions.Clear();
            this._count = count;
            var font = FontManager.instance.getOrCreate(style.fontFamily).font;
            font.RequestCharactersInTexture(this._text.Substring(start, count),
                style.UnityFontSize,
                style.UnityFontStyle);

            this._advance = 0;
            this._bounds = null;
            for (int i = 0; i < count; i++) {
                int charIndex = start + i;
                var ch = text[charIndex];
                CharacterInfo characterInfo;
                font.GetCharacterInfo(ch, out characterInfo, style.UnityFontSize, style.UnityFontStyle);

                var rect = Rect.fromLTRB(characterInfo.minX, -characterInfo.maxY, characterInfo.maxX,
                    -characterInfo.minY);
                rect = rect.translate(this._advance, 0);
                if (this._bounds == null || this._bounds.isEmpty) {
                    this._bounds = rect;
                }
                else {
                    this._bounds = this._bounds.expandToInclude(rect);
                }

                this._positions.Add(this._advance);
                float advance = characterInfo.advance;
                if (ch == '\t') {
                    advance = this._tabStops.nextTab((this._advance + offset)) - this._advance;
                }

                this._advances.Add(advance);
                this._advance += advance;
            }
        }

        public void setTabStops(TabStops tabStops) {
            this._tabStops = tabStops;
        }

        public int nGlyphs() {
            return this._count;
        }

        public List<float> getAdvances() {
            return this._advances;
        }

        public float getAdvance() {
            return this._advance;
        }

        public float getX(int index) {
            return this._positions[index];
        }

        public float getY(int index) {
            return 0;
        }

        public float getCharAdvance(int index) {
            return this._advances[index];
        }

        public Rect getBounds() {
            return this._bounds;
        }
    }
}