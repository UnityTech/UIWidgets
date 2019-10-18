using UnityEngine;

namespace Unity.UIWidgets.ui {
    static class Layout {
        // Measure the length of the span of the text. Currently, this is only used to compute the length
        // of ellipsis, assuming that the ellipsis does not contain any tab, tab is not considered for simplicity
        public static float measureText(string text, TextStyle style) {
            char startingChar = text[0];
            float totalWidth = 0;
            if (char.IsHighSurrogate(startingChar) || EmojiUtils.isSingleCharEmoji(startingChar)) {
                float advance = style.fontSize * EmojiUtils.advanceFactor + style.letterSpacing;
                for (int i = 0; i < text.Length; i++) {
                    char ch = text[i];
                    if (char.IsHighSurrogate(ch) || EmojiUtils.isSingleCharNonEmptyEmoji(ch)) {
                        totalWidth += advance;
                    }
                }
            }
            else {
                Font font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;
                font.RequestCharactersInTextureSafe(text, style.UnityFontSize, style.UnityFontStyle);
                for (int i = 0; i < text.Length; i++) {
                    char ch = text[i];
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
                float advance = style.fontSize * EmojiUtils.advanceFactor + style.letterSpacing;
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
                        currentAdvance += style.letterSpacing;
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

        public static float computeCharWidths(float offset, string text, int start, int count, TextStyle style,
            float[] advances, int advanceOffset, TabStops tabStops) {
            char startingChar = text[start];
            float totalWidths = 0;
            if (char.IsHighSurrogate(startingChar) || EmojiUtils.isSingleCharEmoji(startingChar)) {
                float advance = style.fontSize * EmojiUtils.advanceFactor + style.letterSpacing;
                for (int i = 0; i < count; i++) {
                    char ch = text[start + i];
                    if (char.IsHighSurrogate(ch) || EmojiUtils.isSingleCharNonEmptyEmoji(ch)) {
                        advances[i + advanceOffset] = advance;
                        totalWidths += advance;
                    }
                    else {
                        advances[i + advanceOffset] = 0;
                    }
                }
            }
            else {
                Font font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;
                // TODO: it is kind of a waste to require the entire string for this style, but SubString causes alloc
                font.RequestCharactersInTextureSafe(text, style.UnityFontSize, style.UnityFontStyle);
                for (int i = 0; i < count; i++) {
                    char ch = text[start + i];
                    if (ch == '\t') {
                        advances[i + advanceOffset] = tabStops.nextTab(offset + totalWidths) - (offset + totalWidths);
                    }
                    else if (font.getGlyphInfo(ch, out var glyphInfo, style.UnityFontSize, style.UnityFontStyle)) {
                        advances[i + advanceOffset] = glyphInfo.advance + style.letterSpacing;
                    }
                    else {
                        advances[i + advanceOffset] = style.letterSpacing;
                    }

                    if (LayoutUtils.isWordSpace(ch)) {
                        advances[i + advanceOffset] += style.wordSpacing;
                    }

                    totalWidths += advances[i + advanceOffset];
                }
            }

            return totalWidths;
        }

        public static float doLayout(float offset, string text, int start, int count, TextStyle style,
            float[] advances, float[] positions, TabStops tabStops, out UnityEngine.Rect bounds) {
            float advance = 0;
            Font font = FontManager.instance.getOrCreate(style.fontFamily, style.fontWeight, style.fontStyle).font;

            char startingChar = text[start];
            bounds = new UnityEngine.Rect();
            if (char.IsHighSurrogate(startingChar) || EmojiUtils.isSingleCharEmoji(startingChar)) {
                advance = _layoutEmoji(text, start, count, style, font, advances, positions, ref bounds);
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
                        wordend - iter, style, font, advances, positions, advance,
                        tabStops, ref bounds);
                }
            }

            // bounds relative to first character
            bounds.x -= positions[0];
            return advance;
        }

        static float _layoutWord(float offset, int layoutOffset,
            string text, int start, int wordCount, TextStyle style, Font font, float[] advances,
            float[] positions, float initAdvance, TabStops tabStops, ref UnityEngine.Rect bounds) {
            float wordSpacing =
                wordCount == 1 && LayoutUtils.isWordSpace(text[start]) ? style.wordSpacing : 0;

            float x = initAdvance;
            float letterSpace = style.letterSpacing;
            float letterSpaceHalfLeft = letterSpace * 0.5f;
            float letterSpaceHalfRight = letterSpace - letterSpaceHalfLeft;

            for (int i = 0; i < wordCount; i++) {
                initAdvance = x;
                var ch = text[start + i];
                if (i == 0) {
                    x += letterSpaceHalfLeft + wordSpacing;
                    advances[i + layoutOffset] = letterSpaceHalfLeft + wordSpacing;
                }
                else {
                    advances[i - 1 + layoutOffset] += letterSpaceHalfRight;
                    advances[i + layoutOffset] = letterSpaceHalfLeft;

                    x += letterSpace;
                }

                if (font.getGlyphInfo(ch, out var glyphInfo, style.UnityFontSize, style.UnityFontStyle)) {
                    _updateBounds(glyphInfo, x, ref bounds);
                }

                positions[i + layoutOffset] = x;

                float advance;
                if (ch == '\t') {
                    advance = tabStops.nextTab(initAdvance + offset) - initAdvance - offset;
                }
                else {
                    advance = glyphInfo.advance;
                }

                x += advance;
                advances[i + layoutOffset] += advance;

                if (i + 1 == wordCount) {
                    advances[i + layoutOffset] += letterSpaceHalfRight;
                    x += letterSpaceHalfRight;
                }
            }

            return x;
        }

        static float _layoutEmoji(string text, int start, int count, TextStyle style, Font font, float[] advances,
            float[] positions, ref UnityEngine.Rect bounds) {
            var metrics = FontMetrics.fromFont(font, style.UnityFontSize);
            float x = 0;
            for (int i = 0; i < count; i++) {
                char c = text[start + i];
                if (EmojiUtils.isSingleCharNonEmptyEmoji(c) || char.IsHighSurrogate(c)) {
                    float letterSpace = style.letterSpacing;
                    float letterSpaceHalfLeft = letterSpace * 0.5f;
                    float letterSpaceHalfRight = letterSpace - letterSpaceHalfLeft;

                    x += letterSpaceHalfLeft;
                    advances[i] = letterSpaceHalfLeft;

                    float advance = style.fontSize * EmojiUtils.advanceFactor;
                    var minX = x;
                    var maxX = advance + x;
                    var minY = -style.fontSize * EmojiUtils.sizeFactor;
                    var maxY = metrics.descent;
                    _updateBounds(minX, maxX, minY, maxY, ref bounds);

                    positions[i] = x;

                    x += advance;

                    advances[i] += advance;
                    advances[i] += letterSpaceHalfRight;

                    x += letterSpaceHalfRight;
                }
                else {
                    advances[i] = 0;
                    positions[i] = x;
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
    }
}