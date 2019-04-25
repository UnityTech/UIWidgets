using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.ui {
    
    class Layout {
        int _start;
        int _count;
        List<float> _advances = new List<float>();
        List<float> _positions = new List<float>();
        float _advance;
        Rect _bounds;
        TabStops _tabStops;


        public static float measureText(float offset, TextBuff buff, int start, int count, TextStyle style,
            List<float> advances, int advanceOffset, TabStops tabStops) {
            Layout layout = new Layout();
            layout.setTabStops(tabStops);
            layout.doLayout(offset, buff, start, count, style);
            if (advances != null) {
                var layoutAdv = layout.getAdvances();
                for (int i = 0; i < count; i++) {
                    advances[i + advanceOffset] = layoutAdv[i];
                }
            }

            return layout.getAdvance();
        }

        public void doLayout(float offset, TextBuff buff, int start, int count, TextStyle style) {
            this._start = start;
            this._count = count;
            this._advances.resize(count, 0);
            this._positions.resize(count, 0);
            this._advance = 0;
            this._bounds = null;
            
            int wordstart = start == buff.size
                ? start
                : LayoutUtils.getPrevWordBreakForCache(buff, start + 1);
            int wordend;
            for (int iter = start; iter < start + count; iter = wordend) {
                wordend = LayoutUtils.getNextWordBreakForCache(buff, iter);
                int wordCount = Math.Min(start + count, wordend) - iter;
                this.layoutWord(offset, iter - start,  buff.subBuff(wordstart, wordend - wordstart),
                    iter - wordstart, wordCount, style);
                wordstart = wordend;
            }
            this._count = count;
            
        }

        void layoutWord(float offset, int layoutOffset, 
            TextBuff buff, int start, int wordCount, TextStyle style) {
            float wordSpacing =
                wordCount == 1 && LayoutUtils.isWordSpace(buff.charAt(start)) ? style.wordSpacing : 0;

            var font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;
            font.RequestCharactersInTextureSafe(buff.subBuff(start, wordCount).getString(),
                style.UnityFontSize,
                style.UnityFontStyle);
            float x = this._advance;
            float letterSpace = style.letterSpacing;
            float letterSpaceHalfLeft = letterSpace * 0.5f;
            float letterSpaceHalfRight = letterSpace - letterSpaceHalfLeft;
            
            for (int i = 0; i < wordCount; i++) {
                var ch = buff.charAt(start + i);
                if (i == 0) {
                    x += letterSpaceHalfLeft + wordSpacing;
                    this._advances[i + layoutOffset] += letterSpaceHalfLeft + wordSpacing;
                }
                else {
                    this._advances[i - 1 + layoutOffset] += letterSpaceHalfRight;
                    this._advances[i + layoutOffset] += letterSpaceHalfLeft;
                    x += letterSpace;
                }

                var glyphInfo = font.getGlyphInfo(ch, style.UnityFontSize, style.UnityFontStyle);
                var rect = glyphInfo.rect;
                rect = rect.translate(x, 0);
                if (this._bounds == null || this._bounds.isEmpty) {
                    this._bounds = rect;
                }
                else {
                    this._bounds = this._bounds.expandToInclude(rect);
                }
                
                this._positions[i + layoutOffset] = x;
                float advance = glyphInfo.advance;
                if (ch == '\t') {
                    advance = this._tabStops.nextTab((this._advance + offset)) - this._advance;
                }
                x += advance;
                this._advances[i + layoutOffset] += advance;
                if (i + 1 == wordCount) {
                    this._advances[i + layoutOffset] += letterSpaceHalfRight;
                    x += letterSpaceHalfRight;
                }
            }
            
            this._advance = x;
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