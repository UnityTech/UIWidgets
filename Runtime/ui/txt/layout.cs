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

        static UnityEngine.Rect _innerBounds; // Used to pass bounds from static to non-static doLayout

        public static float measureText(float offset, TextBuff buff, int start, int count, TextStyle style,
            List<float> advances, int advanceOffset, TabStops tabStops) {
            return _doLayout(offset, buff, start, count, style, advances, null, advanceOffset, tabStops);
        }

        public void doLayout(float offset, TextBuff buff, int start, int count, TextStyle style) {
            this._start = start;
            this._count = count;
            this._advances.reset(count);
            this._positions.reset(count);

            _innerBounds = default;
            this._advance = _doLayout(offset, buff, start, count, style, this._advances, this._positions, 0,
                this._tabStops);
            this._bounds = _innerBounds;

            this._count = count;
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
                font.RequestCharactersInTextureSafe(buff.text, style.UnityFontSize, style.UnityFontStyle);
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
            List<float> advances, List<float> positions, int advanceOffset, TabStops tabStops) {
            float advance = 0;
            Font font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;

            char startingChar = buff.charAt(start);
            if (char.IsHighSurrogate(startingChar) || EmojiUtils.isSingleCharEmoji(startingChar)) {
                advance = _layoutEmoji(buff.text.Substring(buff.offset + start, count), style, font, count,
                    advances, positions, advanceOffset, advance);
            }
            else {
                // According to the logic of Paragraph.layout, it is assured that all the characters are requested
                // in the texture before (in computing line breaks), so skip it here for optimization
                // The only exception is the ellipsis, which is dealt with somewhere else.
                // font.RequestCharactersInTextureSafe(buff.text, style.UnityFontSize, style.UnityFontStyle);

                int wordstart = start == buff.size
                    ? start
                    : LayoutUtils.getPrevWordBreakForCache(buff, start + 1);
                int wordend;
                for (int iter = start; iter < start + count; iter = wordend) {
                    wordend = LayoutUtils.getNextWordBreakForCache(buff, iter);
                    int wordCount = Mathf.Min(start + count, wordend) - iter;
                    advance = _layoutWord(offset, iter - start, buff.subBuff(wordstart, wordend - wordstart),
                        iter - wordstart, wordCount, style, font, advances, positions,
                        advanceOffset, advance, tabStops);
                    wordstart = wordend;
                }
            }

            return advance;
        }

        static float _layoutWord(float offset, int layoutOffset,
            TextBuff buff, int start, int wordCount, TextStyle style, Font font, List<float> advances,
            List<float> positions, int advanceOffset, float initAdvance, TabStops tabStops) {
            float wordSpacing =
                wordCount == 1 && LayoutUtils.isWordSpace(buff.charAt(start)) ? style.wordSpacing : 0;

            float x = initAdvance;
            float letterSpace = style.letterSpacing;
            float letterSpaceHalfLeft = letterSpace * 0.5f;
            float letterSpaceHalfRight = letterSpace - letterSpaceHalfLeft;

            for (int i = 0; i < wordCount; i++) {
                var ch = buff.charAt(start + i);
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
                    var minX = glyphInfo.minX + x;
                    var maxX = glyphInfo.maxX + x;
                    var minY = -glyphInfo.maxY;
                    var maxY = -glyphInfo.minY;

                    if (_innerBounds.width <= 0 || _innerBounds.height <= 0) {
                        _innerBounds.x = minX;
                        _innerBounds.y = minY;
                        _innerBounds.xMax = maxX;
                        _innerBounds.yMax = maxY;
                    }
                    else {
                        if (minX < _innerBounds.x) {
                            _innerBounds.x = minX;
                        }

                        if (minY < _innerBounds.y) {
                            _innerBounds.y = minY;
                        }

                        if (maxX > _innerBounds.xMax) {
                            _innerBounds.xMax = maxX;
                        }

                        if (maxY > _innerBounds.yMax) {
                            _innerBounds.yMax = maxY;
                        }
                    }
                }

                if (positions != null) {
                    positions[i + layoutOffset] = x;
                }

                float advance = glyphInfo.advance;
                if (ch == '\t') {
                    advance = tabStops.nextTab((initAdvance + offset)) - initAdvance;
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

        static float _layoutEmoji(string text, TextStyle style, Font font, int count, List<float> advances,
            List<float> positions, int advanceOffset, float initAdvance) {
            var metrics = FontMetrics.fromFont(font, style.UnityFontSize);
            float x = initAdvance;
            for (int i = 0; i < count; i++) {
                char c = text[i];
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

                    if (_innerBounds.width <= 0 || _innerBounds.height <= 0) {
                        _innerBounds.x = minX;
                        _innerBounds.y = minY;
                        _innerBounds.xMax = maxX;
                        _innerBounds.yMax = maxY;
                    }
                    else {
                        if (minX < _innerBounds.x) {
                            _innerBounds.x = minX;
                        }

                        if (minY < _innerBounds.y) {
                            _innerBounds.y = minY;
                        }

                        if (maxX > _innerBounds.xMax) {
                            _innerBounds.xMax = maxX;
                        }

                        if (maxY > _innerBounds.yMax) {
                            _innerBounds.yMax = maxY;
                        }
                    }

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

        public static void requireEllipsisInTexture(string text, TextStyle style) {
            Font font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;
            font.RequestCharactersInTextureSafe(text, style.UnityFontSize, style.UnityFontStyle);
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