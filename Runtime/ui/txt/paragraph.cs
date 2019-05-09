using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public struct Vector2d {
        public float x;
        public float y;

        public Vector2d(float x = 0.0f, float y = 0.0f) {
            this.x = x;
            this.y = y;
        }

        public static Vector2d operator +(Vector2d a, Vector2d b) {
            return new Vector2d(a.x + b.x, a.y + b.y);
        }

        public static Vector2d operator -(Vector2d a, Vector2d b) {
            return new Vector2d(a.x - b.x, a.y - b.y);
        }
    }

    class CodeUnitRun {
        public readonly int lineNumber;
        public readonly TextDirection direction;
        public readonly Range<int> codeUnits;
        public readonly FontMetrics fontMetrics;
        public Range<float> xPos;
        public readonly List<GlyphPosition> positions;

        public CodeUnitRun(List<GlyphPosition> positions, Range<int> cu, int line, Range<float> xPos,
            FontMetrics fontMetrics, TextDirection direction) {
            this.lineNumber = line;
            this.codeUnits = cu;
            this.xPos = xPos;
            this.fontMetrics = fontMetrics;
            this.positions = positions;
            this.direction = direction;
        }

        public void Shift(float shift) {
            this.xPos = RangeUtils.shift(this.xPos, shift);
            for (int i = 0; i < this.positions.Count; ++i) {
                this.positions[i] = this.positions[i].shift(shift);
            }
        }
    }


    class FontMetrics {
        public readonly float ascent;
        public readonly float leading = 0.0f;
        public readonly float descent;
        public readonly float? underlineThickness;
        public readonly float? underlinePosition;
        public readonly float? strikeoutPosition;
        public readonly float? fxHeight;

        public FontMetrics(float ascent, float descent,
            float? underlineThickness = null, float? underlinePosition = null, float? strikeoutPosition = null,
            float? fxHeight = null) {
            this.ascent = ascent;
            this.descent = descent;
            this.underlineThickness = underlineThickness;
            this.underlinePosition = underlinePosition;
            this.strikeoutPosition = strikeoutPosition;
            this.fxHeight = fxHeight;
        }

        public static FontMetrics fromFont(Font font, int fontSize) {
            var ascent = -font.ascent * fontSize / font.fontSize;
            var descent = (font.lineHeight - font.ascent) * fontSize / font.fontSize;
            font.RequestCharactersInTextureSafe("x", fontSize, UnityEngine.FontStyle.Normal);
            font.getGlyphInfo('x', out var glyphInfo, fontSize, UnityEngine.FontStyle.Normal);
            float fxHeight = glyphInfo.glyphHeight;

            return new FontMetrics(ascent, descent, fxHeight: fxHeight);
        }
    }

    class LineStyleRun {
        public readonly int start;
        public readonly int end;
        public readonly TextStyle style;

        public LineStyleRun(int start, int end, TextStyle style) {
            this.start = start;
            this.end = end;
            this.style = style;
        }
    }

    class PositionWithAffinity {
        public readonly int position;
        public readonly TextAffinity affinity;

        public PositionWithAffinity(int p, TextAffinity a) {
            this.position = p;
            this.affinity = a;
        }
    }

    class GlyphPosition {
        public readonly Range<float> xPos;
        public readonly Range<int> codeUnits;

        public GlyphPosition(float start, float advance, Range<int> codeUnits) {
            this.xPos = new Range<float>(start, start + advance);
            this.codeUnits = codeUnits;
        }

        public GlyphPosition shift(float shift) {
            return new GlyphPosition(this.xPos.start + shift, this.xPos.end - this.xPos.start, this.codeUnits);
        }
    }

    class Range<T> : IEquatable<Range<T>> {
        public Range(T start, T end) {
            this.start = start;
            this.end = end;
        }

        public bool Equals(Range<T> other) {
            return EqualityComparer<T>.Default.Equals(this.start, other.start) &&
                   EqualityComparer<T>.Default.Equals(this.end, other.end);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            return obj is Range<T> && this.Equals((Range<T>) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (EqualityComparer<T>.Default.GetHashCode(this.start) * 397) ^
                       EqualityComparer<T>.Default.GetHashCode(this.end);
            }
        }

        public static bool operator ==(Range<T> left, Range<T> right) {
            return left.Equals(right);
        }

        public static bool operator !=(Range<T> left, Range<T> right) {
            return !left.Equals(right);
        }

        public readonly T start, end;
    }

    static class RangeUtils {
        public static Range<float> shift(Range<float> value, float shift) {
            return new Range<float>(value.start + shift, value.end + shift);
        }
    }


    class GlyphLine {
        public readonly List<GlyphPosition> positions;
        public readonly int totalCountUnits;

        public GlyphLine(List<GlyphPosition> positions, int totalCountUnits) {
            this.positions = positions;
            this.totalCountUnits = totalCountUnits;
        }
    }


    public class Paragraph {
        public class LineRange {
            public LineRange(int start, int end, int endExcludingWhitespace, int endIncludingNewLine, bool hardBreak) {
                this.start = start;
                this.end = end;
                this.endExcludingWhitespace = endExcludingWhitespace;
                this.endIncludingNewLine = endIncludingNewLine;
                this.hardBreak = hardBreak;
            }

            public readonly int start;
            public readonly int end;
            public readonly int endExcludingWhitespace;
            public readonly int endIncludingNewLine;
            public readonly bool hardBreak;
        }

        bool _needsLayout = true;

        string _text;

        StyledRuns _runs;

        ParagraphStyle _paragraphStyle;
        List<LineRange> _lineRanges = new List<LineRange>();
        List<float> _lineWidths = new List<float>();
        List<float> _lineBaseLines = new List<float>();
        List<GlyphLine> _glyphLines = new List<GlyphLine>();
        float _maxIntrinsicWidth;
        float _minIntrinsicWidth;
        float _alphabeticBaseline;
        float _ideographicBaseline;
        float[] _characterWidths;
        List<float> _lineHeights = new List<float>();
        List<PaintRecord> _paintRecords = new List<PaintRecord>();
        List<CodeUnitRun> _codeUnitRuns = new List<CodeUnitRun>();
        bool _didExceedMaxLines;
        TabStops _tabStops = new TabStops();

        // private float _characterWidth;

        float _width;

        const float kFloatDecorationSpacing = 3.0f;

        public float height {
            get { return this._lineHeights.Count == 0 ? 0 : this._lineHeights[this._lineHeights.Count - 1]; }
        }

        public float minIntrinsicWidth {
            get { return this._minIntrinsicWidth; }
        }

        public float maxIntrinsicWidth {
            get { return this._maxIntrinsicWidth; }
        }

        public float width {
            get { return this._width; }
        }


        public float alphabeticBaseline {
            get { return this._alphabeticBaseline; }
        }

        public float ideographicBaseline {
            get { return this._ideographicBaseline; }
        }

        public bool didExceedMaxLines {
            get { return this._didExceedMaxLines; }
        }

        public void paint(Canvas canvas, Offset offset) {

            foreach (var paintRecord in this._paintRecords) {
                this.paintBackground(canvas, paintRecord, offset);
            }

            foreach (var paintRecord in this._paintRecords) {
                var paint = new Paint {
                    filterMode = FilterMode.Bilinear,
                    color = paintRecord.style.color
                };
                canvas.drawTextBlob(paintRecord.text, paintRecord.offset + offset, paint);
                this.paintDecorations(canvas, paintRecord, offset);
            }
        }

        public void layout(ParagraphConstraints constraints) {
            if (!this._needsLayout && this._width == constraints.width) {
                return;
            }

            var textStyle = this._paragraphStyle.getTextStyle();
            this._tabStops.setFont(FontManager.instance.getOrCreate(textStyle.fontFamily, 
                    textStyle.fontWeight, textStyle.fontStyle).font,
                textStyle.UnityFontSize);

            this._needsLayout = false;
            this._width = Mathf.Floor(constraints.width);

            this.computeLineBreak();

            this._paintRecords.Clear();
            this._lineHeights.Clear();
            this._lineBaseLines.Clear();
            this._codeUnitRuns.Clear();
            this._glyphLines.Clear();

            int styleMaxLines = this._paragraphStyle.maxLines ?? int.MaxValue;
            var lineLimit = Mathf.Min(styleMaxLines, this._lineRanges.Count);
            this._didExceedMaxLines = this._lineRanges.Count > styleMaxLines;
            float maxWordWidth = 0;

            Layout layout = new Layout();
            layout.setTabStops(this._tabStops);
            TextBlobBuilder builder = new TextBlobBuilder();
            int styleRunIndex = 0;
            float yOffset = 0;
            float preMaxDescent = 0;

            List<CodeUnitRun> lineCodeUnitRuns = new List<CodeUnitRun>();
            List<GlyphPosition> glyphPositions = new List<GlyphPosition>();

            for (int lineNumber = 0; lineNumber < lineLimit; ++lineNumber) {
                var lineRange = this._lineRanges[lineNumber];
                float wordGapWidth = 0;

                // Break the line into words if justification should be applied.
                int wordIndex = 0;
                bool justifyLine = this._paragraphStyle.textAlign == TextAlign.justify &&
                                   lineNumber != lineLimit - 1 &&
                                   !lineRange.hardBreak;
                var words = this.findWords(lineRange.start, lineRange.end);
                if (justifyLine) {
                    if (words.Count > 1) {
                        wordGapWidth = (this._width - this._lineWidths[lineNumber]) / (words.Count - 1);
                    }
                }

                // Exclude trailing whitespace from right-justified lines so the last
                // visible character in the line will be flush with the right margin.
                int lineEndIndex = (this._paragraphStyle.textAlign == TextAlign.right ||
                                    this._paragraphStyle.textAlign == TextAlign.center)
                    ? lineRange.endExcludingWhitespace
                    : lineRange.end;


                List<LineStyleRun> lineRuns = new List<LineStyleRun>();
                while (styleRunIndex < this._runs.size) {
                    var styleRun = this._runs.getRun(styleRunIndex);
                    if (styleRun.start < lineEndIndex && styleRun.end > lineRange.start) {
                        lineRuns.Add(new LineStyleRun(Mathf.Max(styleRun.start, lineRange.start),
                            Mathf.Min(styleRun.end, lineEndIndex), styleRun.style));
                    }

                    if (styleRun.end >= lineEndIndex) {
                        break;
                    }

                    styleRunIndex++;
                }

                float runXOffset = 0;
                float justifyXOffset = 0;
                lineCodeUnitRuns.Clear();

                List<GlyphPosition> lineGlyphPositions = new List<GlyphPosition>();
                List<PaintRecord> paintRecords = new List<PaintRecord>();
                for (int i = 0; i < lineRuns.Count; ++i) {
                    var run = lineRuns[i];
                    string text = this._text;
                    int textStart = Mathf.Max(run.start, lineRange.start);
                    int textEnd = Mathf.Min(run.end, lineEndIndex);
                    int textCount = textEnd - textStart;

                    string ellipsis = this._paragraphStyle.ellipsis;
                    if (!string.IsNullOrEmpty(ellipsis) && !this._width.isInfinite() && !lineRange.hardBreak
                        && i == lineRuns.Count - 1 && (lineNumber == lineLimit - 1 || this._paragraphStyle.maxLines == null)) {

                        float ellipsisWidth = Layout.measureText(runXOffset, new TextBuff(ellipsis), 0,
                            ellipsis.Length, run.style, null, 0, this._tabStops);
                        List<float> textAdvances = new List<float>(textCount);
                        for (int index = 0; index < textCount;++index) {
                            textAdvances.Add(0);
                        }
                        float textWidth = Layout.measureText(runXOffset, new TextBuff(text), textStart, textCount,
                            run.style, textAdvances, 0, this._tabStops);
                        
                        int truncateCount = 0;
                        while (truncateCount < textCount &&
                               runXOffset + textWidth + ellipsisWidth > this._width) {
                            textWidth -= textAdvances[textCount - truncateCount - 1];
                            truncateCount++;
                        }

                        var ellipsizedText = this._text.Substring(textStart, textCount - truncateCount) + ellipsis;
                        textStart = 0;
                        textCount = ellipsizedText.Length;
                        text = ellipsizedText;

                        if (this._paragraphStyle.maxLines == null) {
                            lineLimit = lineNumber + 1;
                            this._didExceedMaxLines = true;
                        }
                    }

                    layout.doLayout(runXOffset, new TextBuff(text), textStart, textCount, run.style);
                    if (layout.nGlyphs() == 0) {
                        continue;
                    }

                    float wordStartPosition = float.NaN;
                    builder.allocRunPos(run.style, text, textStart, textCount);
                    builder.setBounds(layout.getBounds().translate(-layout.getX(0), 0)); // bounds relative to first character
                    
                    glyphPositions.Clear();

                    for (int glyphIndex = 0; glyphIndex < textCount; ++glyphIndex) {
                        float glyphXOffset = layout.getX(glyphIndex) + justifyXOffset;
                        builder.positions[glyphIndex] = new Vector2d(
                            glyphXOffset, layout.getY(glyphIndex)
                        );

                        float glyphAdvance = layout.getCharAdvance(glyphIndex);
                        glyphPositions.Add(new GlyphPosition(runXOffset + glyphXOffset, glyphAdvance,
                            new Range<int>(textStart + glyphIndex, textStart + glyphIndex + 1)));
                        if (wordIndex < words.Count && words[wordIndex].start == run.start + glyphIndex) {
                            wordStartPosition = runXOffset + glyphXOffset;
                        }

                        if (wordIndex < words.Count && words[wordIndex].end == run.start + glyphIndex + 1) {
                            if (justifyLine) {
                                justifyXOffset += wordGapWidth;
                            }

                            wordIndex++;
                            if (!float.IsNaN(wordStartPosition)) {
                                float wordWidth =
                                    glyphPositions[glyphPositions.Count - 1].xPos.end - wordStartPosition;
                                maxWordWidth = Mathf.Max(wordWidth, maxWordWidth);
                                wordStartPosition = float.NaN;
                            }
                        }
                    }

                    if (glyphPositions.Count == 0) {
                        continue;
                    }

                    var font = FontManager.instance.getOrCreate(run.style.fontFamily, 
                        run.style.fontWeight, run.style.fontStyle).font;
                    var metrics = FontMetrics.fromFont(font, run.style.UnityFontSize);
                    paintRecords.Add(new PaintRecord(run.style, new Offset(runXOffset, 0),
                        builder.make(), metrics, lineNumber, layout.getAdvance()
                    ));
                    lineGlyphPositions.AddRange(glyphPositions);
                    var codeUnitPositions = new List<GlyphPosition>(glyphPositions);
                    lineCodeUnitRuns.Add(new CodeUnitRun(codeUnitPositions, new Range<int>(run.start, run.end),
                        lineNumber,
                        new Range<float>(glyphPositions[0].xPos.start,
                            glyphPositions[glyphPositions.Count - 1].xPos.end),
                        metrics, TextDirection.ltr));
                    runXOffset += layout.getAdvance();
                }

                float lineXOffset = this.getLineXOffset(runXOffset);
                if (lineXOffset != 0) {
                    foreach (var codeUnitRun in lineCodeUnitRuns) {
                        codeUnitRun.Shift(lineXOffset);
                    }

                    for (int i = 0; i < lineGlyphPositions.Count; ++i) {
                        lineGlyphPositions[i] = lineGlyphPositions[i].shift(lineXOffset);
                    }
                }

                int nextLineStart = (lineNumber < this._lineRanges.Count - 1)
                    ? this._lineRanges[lineNumber + 1].start
                    : this._text.Length;
                this._glyphLines.Add(new GlyphLine(lineGlyphPositions, nextLineStart - lineRange.start));
                this._codeUnitRuns.AddRange(lineCodeUnitRuns);

                float maxLineSpacing = 0;
                float maxDescent = 0;

                var updateLineMetrics = new Action<FontMetrics, TextStyle>((metrics, style) => {
                    float lineSpacing = ((lineNumber == 0)
                        ? -metrics.ascent * style.height
                        : (-metrics.ascent + metrics.leading) * (style.height));
                    if (lineSpacing > maxLineSpacing) {
                        maxLineSpacing = lineSpacing;
                        if (lineNumber == 0) {
                            this._alphabeticBaseline = lineSpacing;
                            this._ideographicBaseline =
                                (metrics.underlinePosition ?? 0.0f - metrics.ascent) * style.height;
                        }
                    }

                    float descent = metrics.descent * style.height;
                    maxDescent = Mathf.Max(descent, maxDescent);
                });

                foreach (var paintRecord in paintRecords) {
                    updateLineMetrics(paintRecord.metrics, paintRecord.style);
                }

                if (paintRecords.Count == 0) {
                    var defaultStyle = this._paragraphStyle.getTextStyle();
                    var defaultFont = FontManager.instance.getOrCreate(defaultStyle.fontFamily, 
                        defaultStyle.fontWeight, defaultStyle.fontStyle).font;
                    var metrics = FontMetrics.fromFont(defaultFont, defaultStyle.UnityFontSize);
                    updateLineMetrics(metrics, defaultStyle);
                }

                this._lineHeights.Add(
                    (this._lineHeights.Count == 0 ? 0 : this._lineHeights[this._lineHeights.Count - 1])
                    + Mathf.Round(maxLineSpacing + maxDescent));
                this._lineBaseLines.Add(this._lineHeights[this._lineHeights.Count - 1] - maxDescent);
                yOffset += Mathf.Round(maxLineSpacing + preMaxDescent);
                preMaxDescent = maxDescent;

                foreach (var paintRecord in paintRecords) {
                    paintRecord.offset = new Offset(paintRecord.offset.dx + lineXOffset, yOffset);
                    this._paintRecords.Add(paintRecord);
                }
            }

            this._maxIntrinsicWidth = 0;
            float lineBlockWidth = 0;
            for (int i = 0; i < this._lineWidths.Count; ++i) {
                lineBlockWidth += this._lineWidths[i];
                if (this._lineRanges[i].hardBreak) {
                    this._maxIntrinsicWidth = Mathf.Max(lineBlockWidth, this._maxIntrinsicWidth);
                    lineBlockWidth = 0;
                }
            }

            this._maxIntrinsicWidth = Mathf.Max(lineBlockWidth, this._maxIntrinsicWidth);

            if (this._paragraphStyle.maxLines == 1 || (this._paragraphStyle.maxLines == null &&
                                                       this._paragraphStyle.ellipsized())) {
                this._minIntrinsicWidth = this.maxIntrinsicWidth;
            }
            else {
                this._minIntrinsicWidth = Mathf.Min(maxWordWidth, this.maxIntrinsicWidth);
            }
        }


        internal void setText(string text, StyledRuns runs) {
            this._text = text;
            this._runs = runs;
            this._needsLayout = true;
        }

        public void setParagraphStyle(ParagraphStyle style) {
            this._needsLayout = true;
            this._paragraphStyle = style;
        }

        public List<TextBox> getRectsForRange(int start, int end) {
            var lineBoxes = new SplayTree<int, List<TextBox>>();
            foreach (var run in this._codeUnitRuns) {
                if (run.codeUnits.start >= end) {
                    break;
                }

                if (run.codeUnits.end <= start) {
                    continue;
                }

                float top = (run.lineNumber == 0) ? 0 : this._lineHeights[run.lineNumber - 1];
                float bottom = this._lineHeights[run.lineNumber];
                float left, right;
                if (run.codeUnits.start >= start && run.codeUnits.end <= end) {
                    left = run.xPos.start;
                    right = run.xPos.end;
                }
                else {
                    left = float.MaxValue;
                    right = float.MinValue;
                    foreach (var gp in run.positions) {
                        if (gp.codeUnits.start >= start && gp.codeUnits.end <= end) {
                            left = Mathf.Min(left, gp.xPos.start);
                            right = Mathf.Max(right, gp.xPos.end);
                        }
                    }

                    if (left == float.MaxValue || right == float.MinValue) {
                        continue;
                    }
                }

                List<TextBox> boxs;
                if (!lineBoxes.TryGetValue(run.lineNumber, out boxs)) {
                    boxs = new List<TextBox>();
                    lineBoxes.Add(run.lineNumber, boxs);
                }

                boxs.Add(TextBox.fromLTBD(left, top, right, bottom, run.direction));
            }

            for (int lineNumber = 0; lineNumber < this._lineRanges.Count; ++lineNumber) {
                var line = this._lineRanges[lineNumber];
                if (line.start >= end) {
                    break;
                }

                if (line.endIncludingNewLine <= start) {
                    continue;
                }

                if (!lineBoxes.ContainsKey(lineNumber)) {
                    if (line.end != line.endIncludingNewLine && line.end >= start && line.endIncludingNewLine <= end) {
                        var x = this._lineWidths[lineNumber];
                        var top = (lineNumber > 0) ? this._lineHeights[lineNumber - 1] : 0;
                        var bottom = this._lineHeights[lineNumber];
                        lineBoxes.Add(lineNumber, new List<TextBox>() {
                            TextBox.fromLTBD(
                                x, top, x, bottom, TextDirection.ltr)
                        });
                    }
                }
            }

            var result = new List<TextBox>();
            foreach (var keyValuePair in lineBoxes) {
                result.AddRange(keyValuePair.Value);
            }

            return result;
        }

        public TextBox getNextLineStartRect() {
            if (this._text.Length == 0 || this._text[this._text.Length - 1] != '\n') {
                return null;
            }

            var lineNumber = this.getLineCount() - 1;
            var top = (lineNumber > 0) ? this._lineHeights[lineNumber - 1] : 0;
            var bottom = this._lineHeights[lineNumber];
            return TextBox.fromLTBD(0, top, 0, bottom, TextDirection.ltr);
        }

        internal PositionWithAffinity getGlyphPositionAtCoordinate(float dx, float dy) {
            if (this._lineHeights.Count == 0) {
                return new PositionWithAffinity(0, TextAffinity.downstream);
            }

            int yIndex;
            for (yIndex = 0; yIndex < this._lineHeights.Count - 1; ++yIndex) {
                if (dy < this._lineHeights[yIndex]) {
                    break;
                }
            }

            var lineGlyphPosition = this._glyphLines[yIndex].positions;
            if (lineGlyphPosition.Count == 0) {
                int lineStartIndex = this._glyphLines.Where((g, i) => i < yIndex).Sum((gl) => gl.totalCountUnits);
                return new PositionWithAffinity(lineStartIndex, TextAffinity.downstream);
            }


            GlyphPosition gp = null;
            for (int xIndex = 0; xIndex < lineGlyphPosition.Count; ++xIndex) {
                float glyphEnd = xIndex < lineGlyphPosition.Count - 1
                    ? lineGlyphPosition[xIndex + 1].xPos.start
                    : lineGlyphPosition[xIndex].xPos.end;
                if (dx < glyphEnd) {
                    gp = lineGlyphPosition[xIndex];
                    break;
                }
            }

            if (gp == null) {
                GlyphPosition lastGlyph = lineGlyphPosition[lineGlyphPosition.Count - 1];
                return new PositionWithAffinity(lastGlyph.codeUnits.end, TextAffinity.upstream);
            }

            TextDirection direction = TextDirection.ltr;
            foreach (var run in this._codeUnitRuns) {
                if (gp.codeUnits.start >= run.codeUnits.start && gp.codeUnits.end <= run.codeUnits.end) {
                    direction = run.direction;
                    break;
                }
            }

            float glyphCenter = (gp.xPos.start + gp.xPos.end) / 2;
            if ((direction == TextDirection.ltr && dx < glyphCenter) ||
                (direction == TextDirection.rtl && dx >= glyphCenter)) {
                return new PositionWithAffinity(gp.codeUnits.start, TextAffinity.downstream);
            }
            else {
                return new PositionWithAffinity(gp.codeUnits.end, TextAffinity.upstream);
            }
        }

        public int getLine(TextPosition position) {
            D.assert(!this._needsLayout);
            if (position.offset < 0) {
                return 0;
            }

            var offset = position.offset;
            if (position.affinity == TextAffinity.upstream && offset > 0) {
                offset = _isUtf16Surrogate(this._text[offset - 1]) ? offset - 2 : offset - 1;
            }

            var lineCount = this.getLineCount();
            for (int lineIndex = 0; lineIndex < this.getLineCount(); ++lineIndex) {
                var line = this._lineRanges[lineIndex];
                if ((offset >= line.start && offset < line.endIncludingNewLine)) {
                    return lineIndex;
                }
            }

            return Mathf.Max(lineCount - 1, 0);
        }

        internal LineRange getLineRange(int lineIndex) {
            return this._lineRanges[lineIndex];
        }

        internal Range<int> getWordBoundary(int offset) {
            WordSeparate s = new WordSeparate(this._text);
            return s.findWordRange(offset);
        }

        public int getLineCount() {
            return this._lineHeights.Count;
        }

        void computeLineBreak() {
            this._lineRanges.Clear();
            this._lineWidths.Clear();
            this._maxIntrinsicWidth = 0;

            var newLinePositions = new List<int>();
            for (var i = 0; i < this._text.Length; i++) {
                if (this._text[i] == '\n') {
                    newLinePositions.Add(i);
                }
            }

            newLinePositions.Add(this._text.Length);

            var lineBreaker = new LineBreaker();
            int runIndex = 0;
            for (var newlineIndex = 0; newlineIndex < newLinePositions.Count; ++newlineIndex) {
                var blockStart = newlineIndex > 0 ? newLinePositions[newlineIndex - 1] + 1 : 0;
                var blockEnd = newLinePositions[newlineIndex];
                var blockSize = blockEnd - blockStart;
                if (blockSize == 0) {
                    this._lineRanges.Add(new LineRange(blockStart, blockEnd, blockEnd,
                        blockEnd < this._text.Length ? blockEnd + 1 : blockEnd, true));
                    this._lineWidths.Add(0);
                    continue;
                }

                lineBreaker.setLineWidth(this._width);
                lineBreaker.resize(blockSize);
                lineBreaker.setTabStops(this._tabStops);
                lineBreaker.setText(this._text, blockStart, blockSize);

                while (runIndex < this._runs.size) {
                    var run = this._runs.getRun(runIndex);
                    if (run.start >= blockEnd) {
                        break;
                    }

                    if (run.end < blockStart) {
                        runIndex++;
                        continue;
                    }

                    int runStart = Mathf.Max(run.start, blockStart) - blockStart;
                    int runEnd = Mathf.Min(run.end, blockEnd) - blockStart;
                    lineBreaker.addStyleRun(run.style, runStart, runEnd);

                    if (run.end > blockEnd) {
                        break;
                    }

                    runIndex++;
                }

                int breaksCount = lineBreaker.computeBreaks();
                List<int> breaks = lineBreaker.getBreaks();
                List<float> widths = lineBreaker.getWidths();
                for (int i = 0; i < breaksCount; ++i) {
                    var breakStart = (i > 0) ? breaks[i - 1] : 0;
                    var lineStart = breakStart + blockStart;
                    var lineEnd = breaks[i] + blockStart;
                    bool hardBreak = (i == breaksCount - 1);
                    var lineEndIncludingNewline =
                        (hardBreak && lineEnd < this._text.Length) ? lineEnd + 1 : lineEnd;
                    var lineEndExcludingWhitespace = lineEnd;
                    while (lineEndExcludingWhitespace > lineStart &&
                           LayoutUtils.isLineEndSpace(this._text[lineEndExcludingWhitespace - 1])) {
                        lineEndExcludingWhitespace--;
                    }

                    this._lineRanges.Add(new LineRange(lineStart, lineEnd,
                        lineEndExcludingWhitespace, lineEndIncludingNewline, hardBreak));
                    this._lineWidths.Add(widths[i]);
                }

                lineBreaker.finish();
            }
        }

        List<Range<int>> findWords(int start, int end) {
            var inWord = false;
            int wordStart = 0;
            List<Range<int>> words = new List<Range<int>>();
            for (int i = start; i < end; ++i) {
                bool isSpace = LayoutUtils.isWordSpace(this._text[i]);
                if (!inWord && !isSpace) {
                    wordStart = i;
                    inWord = true;
                }
                else if (inWord && isSpace) {
                    words.Add(new Range<int>(wordStart, i));
                    inWord = false;
                }
            }

            if (inWord) {
                words.Add(new Range<int>(wordStart, end));
            }

            return words;
        }

        void paintDecorations(Canvas canvas, PaintRecord record, Offset baseOffset) {
            if (record.style.decoration == null || record.style.decoration == TextDecoration.none) {
                return;
            }

            var paint = new Paint();
            if (record.style.decorationColor == null) {
                paint.color = record.style.color;
            }
            else {
                paint.color = record.style.decorationColor;
            }


            var width = record.runWidth;
            var metrics = record.metrics;
            float underLineThickness = metrics.underlineThickness ?? (record.style.fontSize / 14.0f);
            paint.style = PaintingStyle.stroke;
            paint.strokeWidth = underLineThickness;
            var recordOffset = baseOffset + record.offset;
            var x = recordOffset.dx;
            var y = recordOffset.dy;

            int decorationCount = 1;
            switch (record.style.decorationStyle) {
                case TextDecorationStyle.doubleLine:
                    decorationCount = 2;
                    break;
            }


            var decoration = record.style.decoration;
            for (int i = 0; i < decorationCount; i++) {
                float yOffset = i * underLineThickness * kFloatDecorationSpacing;
                float yOffsetOriginal = yOffset;
                if (decoration != null && decoration.contains(TextDecoration.underline)) {
                    // underline
                    yOffset += metrics.underlinePosition ?? underLineThickness;
                    canvas.drawLine(new Offset(x, y + yOffset), new Offset(x + width, y + yOffset), paint);
                    yOffset = yOffsetOriginal;
                }

                if (decoration != null && decoration.contains(TextDecoration.overline)) {
                    yOffset += metrics.ascent;
                    canvas.drawLine(new Offset(x, y + yOffset), new Offset(x + width, y + yOffset), paint);
                    yOffset = yOffsetOriginal;
                }

                if (decoration != null && decoration.contains(TextDecoration.lineThrough)) {
                    yOffset += (decorationCount - 1.0f) * underLineThickness * kFloatDecorationSpacing / -2.0f;
                    yOffset += metrics.strikeoutPosition ?? (metrics.fxHeight ?? 0) / -2.0f;
                    canvas.drawLine(new Offset(x, y + yOffset), new Offset(x + width, y + yOffset), paint);
                    yOffset = yOffsetOriginal;
                }
            }
        }

        void paintBackground(Canvas canvas, PaintRecord record, Offset baseOffset) {
            if (record.style.background == null) {
                return;
            }

            var metrics = record.metrics;
            Rect rect = Rect.fromLTRB(0, metrics.ascent, record.runWidth, metrics.descent);
            rect = rect.shift(baseOffset + record.offset);
            canvas.drawRect(rect, record.style.background);
        }

        float getLineXOffset(float lineTotalAdvance) {
            if (this._width.isInfinite()) {
                return 0;
            }

            if (this._paragraphStyle.textAlign == TextAlign.right) {
                return this._width - lineTotalAdvance;
            }
            else if (this._paragraphStyle.textAlign == TextAlign.center) {
                return (this._width - lineTotalAdvance) / 2;
            }
            else {
                return 0;
            }
        }

        static bool _isUtf16Surrogate(int value) {
            return (value & 0xF800) == 0xD800;
        }
    }

    class SplayTree<TKey, TValue> : IDictionary<TKey, TValue> where TKey : IComparable<TKey> {
        SplayTreeNode root;
        int count;
        int version = 0;

        public void Add(TKey key, TValue value) {
            this.Set(key, value, throwOnExisting: true);
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            this.Set(item.Key, item.Value, throwOnExisting: true);
        }

        void Set(TKey key, TValue value, bool throwOnExisting) {
            if (this.count == 0) {
                this.version++;
                this.root = new SplayTreeNode(key, value);
                this.count = 1;
                return;
            }

            this.Splay(key);

            var c = key.CompareTo(this.root.Key);
            if (c == 0) {
                if (throwOnExisting) {
                    throw new ArgumentException("An item with the same key already exists in the tree.");
                }

                this.version++;
                this.root.Value = value;
                return;
            }

            var n = new SplayTreeNode(key, value);
            if (c < 0) {
                n.LeftChild = this.root.LeftChild;
                n.RightChild = this.root;
                this.root.LeftChild = null;
            }
            else {
                n.RightChild = this.root.RightChild;
                n.LeftChild = this.root;
                this.root.RightChild = null;
            }

            this.root = n;
            this.count++;
            this.Splay(key);
            this.version++;
        }

        public void Clear() {
            this.root = null;
            this.count = 0;
            this.version++;
        }

        public bool ContainsKey(TKey key) {
            if (this.count == 0) {
                return false;
            }

            this.Splay(key);

            return key.CompareTo(this.root.Key) == 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            if (this.count == 0) {
                return false;
            }

            this.Splay(item.Key);

            return item.Key.CompareTo(this.root.Key) == 0 &&
                   (ReferenceEquals(this.root.Value, item.Value) ||
                    (!ReferenceEquals(item.Value, null) && item.Value.Equals(this.root.Value)));
        }

        public KeyValuePair<TKey, TValue> First() {
            SplayTreeNode t = this.root;
            if (t == null) {
                throw new NullReferenceException("The root of this tree is null!");
            }

            while (t.LeftChild != null) {
                t = t.LeftChild;
            }

            return new KeyValuePair<TKey, TValue>(t.Key, t.Value);
        }

        public KeyValuePair<TKey, TValue> FirstOrDefault() {
            SplayTreeNode t = this.root;
            if (t == null) {
                return new KeyValuePair<TKey, TValue>(default(TKey), default(TValue));
            }

            while (t.LeftChild != null) {
                t = t.LeftChild;
            }

            return new KeyValuePair<TKey, TValue>(t.Key, t.Value);
        }

        public KeyValuePair<TKey, TValue> Last() {
            SplayTreeNode t = this.root;
            if (t == null) {
                throw new NullReferenceException("The root of this tree is null!");
            }

            while (t.RightChild != null) {
                t = t.RightChild;
            }

            return new KeyValuePair<TKey, TValue>(t.Key, t.Value);
        }

        public KeyValuePair<TKey, TValue> LastOrDefault() {
            SplayTreeNode t = this.root;
            if (t == null) {
                return new KeyValuePair<TKey, TValue>(default(TKey), default(TValue));
            }

            while (t.RightChild != null) {
                t = t.RightChild;
            }

            return new KeyValuePair<TKey, TValue>(t.Key, t.Value);
        }

        void Splay(TKey key) {
            SplayTreeNode l, r, t, y, header;
            l = r = header = new SplayTreeNode(default(TKey), default(TValue));
            t = this.root;
            while (true) {
                var c = key.CompareTo(t.Key);
                if (c < 0) {
                    if (t.LeftChild == null) {
                        break;
                    }

                    if (key.CompareTo(t.LeftChild.Key) < 0) {
                        y = t.LeftChild;
                        t.LeftChild = y.RightChild;
                        y.RightChild = t;
                        t = y;
                        if (t.LeftChild == null) {
                            break;
                        }
                    }

                    r.LeftChild = t;
                    r = t;
                    t = t.LeftChild;
                }
                else if (c > 0) {
                    if (t.RightChild == null) {
                        break;
                    }

                    if (key.CompareTo(t.RightChild.Key) > 0) {
                        y = t.RightChild;
                        t.RightChild = y.LeftChild;
                        y.LeftChild = t;
                        t = y;
                        if (t.RightChild == null) {
                            break;
                        }
                    }

                    l.RightChild = t;
                    l = t;
                    t = t.RightChild;
                }
                else {
                    break;
                }
            }

            l.RightChild = t.LeftChild;
            r.LeftChild = t.RightChild;
            t.LeftChild = header.RightChild;
            t.RightChild = header.LeftChild;
            this.root = t;
        }

        public bool Remove(TKey key) {
            if (this.count == 0) {
                return false;
            }

            this.Splay(key);

            if (key.CompareTo(this.root.Key) != 0) {
                return false;
            }

            if (this.root.LeftChild == null) {
                this.root = this.root.RightChild;
            }
            else {
                var swap = this.root.RightChild;
                this.root = this.root.LeftChild;
                this.Splay(key);
                this.root.RightChild = swap;
            }

            this.version++;
            this.count--;
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            if (this.count == 0) {
                value = default(TValue);
                return false;
            }

            this.Splay(key);
            if (key.CompareTo(this.root.Key) != 0) {
                value = default(TValue);
                return false;
            }

            value = this.root.Value;
            return true;
        }

        public TValue this[TKey key] {
            get {
                if (this.count == 0) {
                    throw new KeyNotFoundException("The key was not found in the tree.");
                }

                this.Splay(key);
                if (key.CompareTo(this.root.Key) != 0) {
                    throw new KeyNotFoundException("The key was not found in the tree.");
                }

                return this.root.Value;
            }

            set { this.Set(key, value, throwOnExisting: false); }
        }

        public int Count {
            get { return this.count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            if (this.count == 0) {
                return false;
            }

            this.Splay(item.Key);

            if (item.Key.CompareTo(this.root.Key) == 0 && (ReferenceEquals(this.root.Value, item.Value) ||
                                                           (!ReferenceEquals(item.Value, null) &&
                                                            item.Value.Equals(this.root.Value)))) {
                return false;
            }

            if (this.root.LeftChild == null) {
                this.root = this.root.RightChild;
            }
            else {
                var swap = this.root.RightChild;
                this.root = this.root.LeftChild;
                this.Splay(item.Key);
                this.root.RightChild = swap;
            }

            this.version++;
            this.count--;
            return true;
        }

        public void Trim(int depth) {
            if (depth < 0) {
                throw new ArgumentOutOfRangeException("depth", "The trim depth must not be negative.");
            }

            if (this.count == 0) {
                return;
            }

            if (depth == 0) {
                this.Clear();
            }
            else {
                var prevCount = this.count;
                this.count = this.Trim(this.root, depth - 1);
                if (prevCount != this.count) {
                    this.version++;
                }
            }
        }

        int Trim(SplayTreeNode node, int depth) {
            if (depth == 0) {
                node.LeftChild = null;
                node.RightChild = null;
                return 1;
            }
            else {
                int count = 1;

                if (node.LeftChild != null) {
                    count += this.Trim(node.LeftChild, depth - 1);
                }

                if (node.RightChild != null) {
                    count += this.Trim(node.RightChild, depth - 1);
                }

                return count;
            }
        }

        public ICollection<TKey> Keys {
            get { return new TiedList<TKey>(this, this.version, this.AsList(node => node.Key)); }
        }

        public ICollection<TValue> Values {
            get { return new TiedList<TValue>(this, this.version, this.AsList(node => node.Value)); }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            this.AsList(node => new KeyValuePair<TKey, TValue>(node.Key, node.Value)).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return new TiedList<KeyValuePair<TKey, TValue>>(this, this.version,
                this.AsList(node => new KeyValuePair<TKey, TValue>(node.Key, node.Value))).GetEnumerator();
        }

        IList<TEnumerator> AsList<TEnumerator>(Func<SplayTreeNode, TEnumerator> selector) {
            if (this.root == null) {
                return new TEnumerator[0];
            }

            var result = new List<TEnumerator>(this.count);
            this.PopulateList(this.root, result, selector);
            return result;
        }

        void PopulateList<TEnumerator>(SplayTreeNode node, List<TEnumerator> list,
            Func<SplayTreeNode, TEnumerator> selector) {
            if (node.LeftChild != null) {
                this.PopulateList(node.LeftChild, list, selector);
            }

            list.Add(selector(node));
            if (node.RightChild != null) {
                this.PopulateList(node.RightChild, list, selector);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        sealed class SplayTreeNode {
            public readonly TKey Key;

            public TValue Value;
            public SplayTreeNode LeftChild;
            public SplayTreeNode RightChild;

            public SplayTreeNode(TKey key, TValue value) {
                this.Key = key;
                this.Value = value;
            }
        }

        sealed class TiedList<T> : IList<T> {
            readonly SplayTree<TKey, TValue> tree;
            readonly int version;
            readonly IList<T> backingList;

            public TiedList(SplayTree<TKey, TValue> tree, int version, IList<T> backingList) {
                if (tree == null) {
                    throw new ArgumentNullException("tree");
                }

                if (backingList == null) {
                    throw new ArgumentNullException("backingList");
                }

                this.tree = tree;
                this.version = version;
                this.backingList = backingList;
            }

            public int IndexOf(T item) {
                if (this.tree.version != this.version) {
                    throw new InvalidOperationException("The collection has been modified.");
                }

                return this.backingList.IndexOf(item);
            }

            public void Insert(int index, T item) {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index) {
                throw new NotSupportedException();
            }

            public T this[int index] {
                get {
                    if (this.tree.version != this.version) {
                        throw new InvalidOperationException("The collection has been modified.");
                    }

                    return this.backingList[index];
                }
                set { throw new NotSupportedException(); }
            }

            public void Add(T item) {
                throw new NotSupportedException();
            }

            public void Clear() {
                throw new NotSupportedException();
            }

            public bool Contains(T item) {
                if (this.tree.version != this.version) {
                    throw new InvalidOperationException("The collection has been modified.");
                }

                return this.backingList.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex) {
                if (this.tree.version != this.version) {
                    throw new InvalidOperationException("The collection has been modified.");
                }

                this.backingList.CopyTo(array, arrayIndex);
            }

            public int Count {
                get { return this.tree.count; }
            }

            public bool IsReadOnly {
                get { return true; }
            }

            public bool Remove(T item) {
                throw new NotSupportedException();
            }

            public IEnumerator<T> GetEnumerator() {
                if (this.tree.version != this.version) {
                    throw new InvalidOperationException("The collection has been modified.");
                }

                foreach (var item in this.backingList) {
                    yield return item;
                    if (this.tree.version != this.version) {
                        throw new InvalidOperationException("The collection has been modified.");
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }
        }
    }
}