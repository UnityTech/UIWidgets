using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    class Layout {
        int _count;
        float[] _advances;
        float[] _positions;
        float _advance;
        UnityEngine.Rect _bounds;

        static float _x, _y, _maxX, _maxY; // Used to pass bounds from static to non-static doLayout

        public static float measureText(float offset, TextBuff buff, int start, int count, TextStyle style,
            float[] advances, int advanceOffset, TabStops tabStops) {
            return _doLayout(offset, buff, start, count, style, advances, null, advanceOffset, tabStops);
        }

        public void doLayout(float offset, TextBuff buff, int start, int count, TextStyle style, TabStops tabStops) {
            this._count = count;
            this.allocAdvancesAndPositions(count);

            _x = _y = _maxX = _maxY = 0;
            this._advance = _doLayout(offset, buff, start, count, style, this._advances, this._positions, 0,
                tabStops);
            this._bounds.Set(_x, _y, _maxX - _x, _maxY - _y);
        }

        public static void computeCharWidths(TextBuff buff, int start, int count, TextStyle style, List<float> advances, int advanceOffset) {
            char startingChar = buff.charAt(start);
            if (char.IsHighSurrogate(startingChar) || EmojiUtils.isSingleCharEmoji(startingChar)) {
                float advance = style.fontSize + style.letterSpacing;
                for (int i = 0; i < count; i++) {
                    char ch = buff.charAt(start + i);
                    if (char.IsHighSurrogate(ch) || EmojiUtils.isSingleCharNonEmptyEmoji(ch)) {
                        advances[i + advanceOffset] = advance;
                    }
                    else {
                        advances[i + advanceOffset] = 0;
                    }
                }
            }
            else {
                Font font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;
                font.RequestCharactersInTextureSafe(buff.subString(start, count), style.UnityFontSize, style.UnityFontStyle);
                for (int i = 0; i < count; i++) {
                    char ch = buff.charAt(start + i);
                    if (font.getGlyphInfo(ch, out var glyphInfo, style.UnityFontSize, style.UnityFontStyle)) {
                        advances[i + advanceOffset] = glyphInfo.advance + style.letterSpacing;
                    }
                    else {
                        advances[i + advanceOffset] = style.letterSpacing;
                    }

                    if (LayoutUtils.isWordSpace(ch)) advances[i + advanceOffset] += style.wordSpacing;
                }
            }
        }

        static float _doLayout(float offset, TextBuff buff, int start, int count, TextStyle style,
            float[] advances, float[] positions, int advanceOffset, TabStops tabStops) {
            float advance = 0;
            Font font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;

            char startingChar = buff.charAt(start);
            if (char.IsHighSurrogate(startingChar) || EmojiUtils.isSingleCharEmoji(startingChar)) {
                advance = _layoutEmoji(buff.text, buff.offset + start, style, font, count,
                    advances, positions, advanceOffset, advance);
            }
            else {
                // According to the logic of Paragraph.layout, it is assured that all the characters are requested
                // in the texture before (in computing line breaks), so skip it here for optimization.
                // The only exception is the ellipsis, which did not appear in line breaking. It is taken care of
                // only when needed.
                
                // font.RequestCharactersInTextureSafe(buff.text, style.UnityFontSize, style.UnityFontStyle);

//                int wordstart = start == buff.size
//                    ? start
//                    : LayoutUtils.getPrevWordBreakForCache(buff, start + 1);
                int wordend;
                for (int iter = start; iter < start + count; iter = wordend) {
                    wordend = LayoutUtils.getNextWordBreak(buff.text, buff.offset + iter, start + count);
                    advance = _layoutWord(offset, iter - start, buff.text, iter + buff.offset, 
                        wordend - iter, style, font, advances, positions, advanceOffset, advance,
                        tabStops);
                }
            }

            return advance;
        }

        static float _layoutWord(float offset, int layoutOffset,
            string text, int start, int wordCount, TextStyle style, Font font, float[] advances,
            float[] positions, int advanceOffset, float initAdvance, TabStops tabStops) {
            float wordSpacing =
                wordCount == 1 && LayoutUtils.isWordSpace(text[start]) ? style.wordSpacing : 0;

            float x = initAdvance;
            float letterSpace = style.letterSpacing;
            float letterSpaceHalfLeft = letterSpace * 0.5f;
            float letterSpaceHalfRight = letterSpace - letterSpaceHalfLeft;

            for (int i = 0; i < wordCount; i++) {
                var ch = text[start + i];
                if (i == 0) {
                    x += letterSpaceHalfLeft + wordSpacing;
                    if (advances != null) {
                        advances[i + layoutOffset + advanceOffset] = letterSpaceHalfLeft + wordSpacing;
                    }
                }
                else {
                    if (advances != null) {
                        advances[i - 1 + layoutOffset + advanceOffset] += letterSpaceHalfRight;
                        advances[i + layoutOffset + advanceOffset] = letterSpaceHalfLeft;
                    }

                    x += letterSpace;
                }

                if (font.getGlyphInfo(ch, out var glyphInfo, style.UnityFontSize, style.UnityFontStyle)) {
                    _updateInnerBounds(glyphInfo, x);
                }

                if (positions != null) {
                    positions[i + layoutOffset] = x;
                }

                float advance = glyphInfo.advance;
                if (ch == '\t') {
                    advance = tabStops.nextTab(initAdvance + offset) - initAdvance;
                }

                x += advance;
                if (advances != null) {
                    advances[i + layoutOffset + advanceOffset] += advance;
                }

                if (i + 1 == wordCount) {
                    if (advances != null) {
                        advances[i + layoutOffset + advanceOffset] += letterSpaceHalfRight;
                    }

                    x += letterSpaceHalfRight;
                }
            }

            return x;
        }

        static float _layoutEmoji(string text, int start, TextStyle style, Font font, int count, float[] advances,
            float[] positions, int advanceOffset, float initAdvance) {
            var metrics = FontMetrics.fromFont(font, style.UnityFontSize);
            float x = initAdvance;
            for (int i = 0; i < count; i++) {
                char c = text[start + i];
                if (EmojiUtils.isSingleCharNonEmptyEmoji(c) || char.IsHighSurrogate(c)) {
                    float letterSpace = style.letterSpacing;
                    float letterSpaceHalfLeft = letterSpace * 0.5f;
                    float letterSpaceHalfRight = letterSpace - letterSpaceHalfLeft;

                    x += letterSpaceHalfLeft;
                    if (advances != null) {
                        advances[i + advanceOffset] = letterSpaceHalfLeft;
                    }


                    var minX = x;
                    var maxX = metrics.descent - metrics.ascent + x;
                    var minY = metrics.ascent;
                    var maxY = metrics.descent;
                    _updateInnerBounds(minX, maxX, minY, maxY);

                    if (positions != null) {
                        positions[i] = x;
                    }

                    float advance = style.fontSize;
                    x += advance;

                    if (advances != null) {
                        advances[i + advanceOffset] += advance;
                        advances[i + advanceOffset] += letterSpaceHalfRight;
                    }

                    x += letterSpaceHalfRight;
                }
                else {
                    if (advances != null) {
                        advances[i + advanceOffset] = 0;
                    }

                    if (positions != null) {
                        positions[i] = x;
                    }
                }
            }

            return x;
        }

        static void _updateInnerBounds(CharacterInfo glyphInfo, float x) {
            var minX = glyphInfo.minX + x;
            var maxX = glyphInfo.maxX + x;
            var minY = -glyphInfo.maxY;
            var maxY = -glyphInfo.minY;
            _updateInnerBounds(minX, maxX, minY, maxY);
        }

        static void _updateInnerBounds(float minX, float maxX, float minY, float maxY) {
            if (_maxX - _x <= 0 || _maxY - _y <= 0) {
                _x = minX;
                _y = minY;
                _maxX = maxX;
                _maxY = maxY;
            }
            else {
                if (minX < _x) {
                    _x = minX;
                }

                if (minY < _y) {
                    _y = minY;
                }

                if (maxX > _maxX) {
                    _maxX = maxX;
                }

                if (maxY > _maxY) {
                    _maxY = maxY;
                }
            }
        }

        public static void requireEllipsisInTexture(string text, TextStyle style) {
            Font font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;
            font.RequestCharactersInTextureSafe(text, style.UnityFontSize, style.UnityFontStyle);
        }

        public void allocAdvancesAndPositions(int count) {
            if (this._advances == null || this._advances.Length < count) {
                this._advances = new float[count];
            }

            if (this._positions == null || this._positions.Length < count) {
                this._positions = new float[count];
            }
        }

        public int nGlyphs() {
            return this._count;
        }

        public float[] getAdvances() {
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

        public UnityEngine.Rect translatedBounds() {
            return new UnityEngine.Rect(this._bounds.x - this._positions[0], 
                this._bounds.y, this._bounds.width, this._bounds.height);
        }
    }
}