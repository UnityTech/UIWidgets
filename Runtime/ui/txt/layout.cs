using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    
    class Layout {
        int _start;
        int _count;
        List<float> _advances = new List<float>();
        List<float> _positions = new List<float>();
        float _advance;
        UnityEngine.Rect _bounds;
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
            this._advances.reset(count);
            this._positions.reset(count);
            this._advance = 0;
            this._bounds = default;
            
            var font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;
            font.RequestCharactersInTextureSafe(buff.text, style.UnityFontSize, style.UnityFontStyle);

            int wordstart = start == buff.size
                ? start
                : LayoutUtils.getPrevWordBreakForCache(buff, start + 1);
            int wordend;
            for (int iter = start; iter < start + count; iter = wordend) {
                wordend = LayoutUtils.getNextWordBreakForCache(buff, iter);
                int wordCount = Mathf.Min(start + count, wordend) - iter;
                this.layoutWord(offset, iter - start,  buff.subBuff(wordstart, wordend - wordstart),
                    iter - wordstart, wordCount, style, font);
                wordstart = wordend;
            }
            this._count = count;
        }

        void layoutWord(float offset, int layoutOffset, 
            TextBuff buff, int start, int wordCount, TextStyle style, Font font) {
            float wordSpacing =
                wordCount == 1 && LayoutUtils.isWordSpace(buff.charAt(start)) ? style.wordSpacing : 0;

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

                if (font.getGlyphInfo(ch, out var glyphInfo, style.UnityFontSize, style.UnityFontStyle)) {
                    var minX = glyphInfo.minX + x;
                    var maxX = glyphInfo.maxX + x;
                    var minY = -glyphInfo.maxY;
                    var maxY = -glyphInfo.minY;

                    if (this._bounds.width <= 0 || this._bounds.height <= 0) {
                        this._bounds = UnityEngine.Rect.MinMaxRect(
                            minX, minY, maxX, maxY);
                    } else {
                        if (minX < this._bounds.x) {
                            this._bounds.x = minX;
                        }
                        if (minY < this._bounds.y) {
                            this._bounds.y = minY;
                        }
                        if (maxX > this._bounds.xMax) {
                            this._bounds.xMax = maxX;
                        }
                        if (maxY > this._bounds.yMax) {
                            this._bounds.yMax = maxY;
                        }
                    }
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
            return Rect.fromLTWH(this._bounds.x, this._bounds.y, this._bounds.width, this._bounds.height);
        }
    }
}