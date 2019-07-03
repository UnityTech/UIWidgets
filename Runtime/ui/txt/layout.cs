using UnityEngine;

namespace Unity.UIWidgets.ui {
    static class Layout {
        // Measure the length of the span of the text. Currently, this is only used to compute the length
        // of ellipsis, assuming that the ellipsis does not contain any tab, tab is not considered for simplicity
        public static float measureText(string text, int start, int count, TextStyle style) {
            char startingChar = text[start];
            float totalWidth = 0;
            if (char.IsHighSurrogate(startingChar) || EmojiUtils.isSingleCharEmoji(startingChar)) {
                float advance = style.fontSize + style.letterSpacing;
                for (int i = 0; i < count; i++) {
                    char ch = text[start + i];
                    if (char.IsHighSurrogate(ch) || EmojiUtils.isSingleCharNonEmptyEmoji(ch)) {
                        totalWidth += advance;
                    }
                }
            }
            else {
                Font font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;
                font.RequestCharactersInTextureSafe(text.Substring(start, count), style.UnityFontSize, style.UnityFontStyle);
                for (int i = 0; i < count; i++) {
                    char ch = text[start + i];
                    if (font.getGlyphInfo(ch, out var glyphInfo, style.UnityFontSize, style.UnityFontStyle)) {
                        totalWidth += glyphInfo.advance + style.letterSpacing;
                    }
                    else {
                        totalWidth += style.letterSpacing;
                    }

                    if (LayoutUtils.isWordSpace(ch)) {
                        totalWidth += style.wordSpacing;
                    }
                }
            }

            return totalWidth;
        }
        
        public static int computeTruncateCount(float offset, string text, int start, int count, TextStyle style,
            float advanceLimit, TabStops tabStops) {
            char startingChar = text[start];
            float currentAdvance = offset;
            if (char.IsHighSurrogate(startingChar) || EmojiUtils.isSingleCharEmoji(startingChar)) {
                float advance = style.fontSize + style.letterSpacing;
                for (int i = 0; i < count; i++) {
                    char ch = text[start + i];
                    if (char.IsHighSurrogate(ch) || EmojiUtils.isSingleCharNonEmptyEmoji(ch)) {
                        currentAdvance += advance;
                        if (currentAdvance > advanceLimit) {
                            return count - i;
                        }
                    } 
                }
            }
            else {
                Font font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;
                for (int i = 0; i < count; i++) {
                    char ch = text[start + i];
                    if (ch == '\t') {
                        currentAdvance = tabStops.nextTab(currentAdvance);
                    }
                    else if (font.getGlyphInfo(ch, out var glyphInfo, style.UnityFontSize, style.UnityFontStyle)) {
                        currentAdvance += glyphInfo.advance + style.letterSpacing;
                    }
                    else {
                        currentAdvance = style.letterSpacing;
                    }

                    if (LayoutUtils.isWordSpace(ch)) {
                        currentAdvance += style.wordSpacing;
                    }
                    
                    if (currentAdvance > advanceLimit) {
                        return count - i;
                    }
                }
            }

            return 0;
        }

        public static void computeCharWidths(string text, int start, int count, TextStyle style, float[] advances, int advanceOffset) {
            char startingChar = text[start];
            if (char.IsHighSurrogate(startingChar) || EmojiUtils.isSingleCharEmoji(startingChar)) {
                float advance = style.fontSize + style.letterSpacing;
                for (int i = 0; i < count; i++) {
                    char ch = text[start + i];
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
                font.RequestCharactersInTextureSafe(text.Substring(start, count), style.UnityFontSize, style.UnityFontStyle);
                for (int i = 0; i < count; i++) {
                    char ch = text[start + i];
                    if (font.getGlyphInfo(ch, out var glyphInfo, style.UnityFontSize, style.UnityFontStyle)) {
                        advances[i + advanceOffset] = glyphInfo.advance + style.letterSpacing;
                    }
                    else {
                        advances[i + advanceOffset] = style.letterSpacing;
                    }

                    if (LayoutUtils.isWordSpace(ch)) {
                        advances[i + advanceOffset] += style.wordSpacing;
                    }
                }
            }
        }

        public static float doLayout(float offset, string text, int start, int count, TextStyle style,
            float[] advances, float[] positions, int advanceOffset, TabStops tabStops, out UnityEngine.Rect bounds) {
            float advance = 0;
            Font font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;

            char startingChar = text[start];
            bounds = new UnityEngine.Rect();
            if (char.IsHighSurrogate(startingChar) || EmojiUtils.isSingleCharEmoji(startingChar)) {
                advance = _layoutEmoji(text, start, style, font, count, advances, positions, advanceOffset, advance, ref bounds);
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
                    wordend = LayoutUtils.getNextWordBreak(text, iter, start + count);
                    advance = _layoutWord(offset, iter - start, text, iter, 
                        wordend - iter, style, font, advances, positions, advanceOffset, advance,
                        tabStops, ref bounds);
                }
            }

            return advance;
        }

        static float _layoutWord(float offset, int layoutOffset,
            string text, int start, int wordCount, TextStyle style, Font font, float[] advances,
            float[] positions, int advanceOffset, float initAdvance, TabStops tabStops, ref UnityEngine.Rect bounds) {
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
                    _updateBounds(glyphInfo, x, ref bounds);
                }

                if (positions != null) {
                    positions[i + layoutOffset] = x;
                }

                float advance = glyphInfo.advance;
                if (ch == '\t') {
                    advance = tabStops.nextTab(initAdvance + offset) - initAdvance - offset;
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
            float[] positions, int advanceOffset, float initAdvance, ref UnityEngine.Rect bounds) {
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
                    _updateBounds(minX, maxX, minY, maxY, ref bounds);

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

        static void _updateBounds(CharacterInfo glyphInfo, float x, ref UnityEngine.Rect bounds) {
            var minX = glyphInfo.minX + x;
            var maxX = glyphInfo.maxX + x;
            var minY = -glyphInfo.maxY;
            var maxY = -glyphInfo.minY;
            _updateBounds(minX, maxX, minY, maxY, ref bounds);
        }

        static void _updateBounds(float minX, float maxX, float minY, float maxY, ref UnityEngine.Rect bounds) {
            if (bounds.width <= 0 || bounds.height <= 0) {
                bounds.Set(minX, minY, maxX - minX, maxY - minY);
            }
            else {
                if (minX < bounds.x) {
                    bounds.x = minX;
                }

                if (minY < bounds.y) {
                    bounds.y = minY;
                }

                if (maxX > bounds.xMax) {
                    bounds.xMax = maxX;
                }

                if (maxY > bounds.yMax) {
                    bounds.yMax = maxY;
                }
            }
        }

        public static void requireEllipsisInTexture(string text, TextStyle style) {
            Font font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;
            font.RequestCharactersInTextureSafe(text, style.UnityFontSize, style.UnityFontStyle);
        }
    }
}