using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public struct Vector2d {
        public double x;
        public double y;

        public Vector2d(double x = 0.0, double y = 0.0) {
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

    public class CodeUnitRun {
        public readonly Range<int> codeUnits;
        public readonly TextDirection direction;
        public readonly FontMetrics fontMetrics;
        public readonly int lineNumber;
        public readonly List<GlyphPosition> positions;
        public Range<double> xPos;

        public CodeUnitRun(List<GlyphPosition> positions, Range<int> cu, int line, Range<double> xPos,
            FontMetrics fontMetrics, TextDirection direction) {
            this.lineNumber = line;
            this.codeUnits = cu;
            this.xPos = xPos;
            this.fontMetrics = fontMetrics;
            this.positions = positions;
            this.direction = direction;
        }

        public void Shift(double shift) {
            this.xPos = RangeUtils.shift(value: this.xPos, shift: shift);
            for (int i = 0; i < this.positions.Count; ++i) {
                this.positions[index: i] = this.positions[index: i].shift(shift: shift);
            }
        }
    }


    public class FontMetrics {
        public readonly double ascent;
        public readonly double descent;
        public readonly double? fxHeight;
        public readonly double leading = 0.0;
        public readonly double? strikeoutPosition;
        public readonly double? underlinePosition;
        public readonly double? underlineThickness;

        public FontMetrics(double ascent, double descent,
            double? underlineThickness = null, double? underlinePosition = null, double? strikeoutPosition = null,
            double? fxHeight = null) {
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
            double? fxHeight = null;
            font.RequestCharactersInTexture("x", size: fontSize);
            CharacterInfo charInfo;
            if (font.GetCharacterInfo('x', info: out charInfo, size: fontSize)) {
                fxHeight = charInfo.glyphHeight;
            }

            return new FontMetrics(ascent: ascent, descent: descent, fxHeight: fxHeight);
        }
    }

    public class LineStyleRun {
        public readonly int end;
        public readonly int start;
        public readonly TextStyle style;

        public LineStyleRun(int start, int end, TextStyle style) {
            this.start = start;
            this.end = end;
            this.style = style;
        }
    }

    public class PositionWithAffinity {
        public readonly TextAffinity affinity;
        public readonly int position;

        public PositionWithAffinity(int p, TextAffinity a) {
            this.position = p;
            this.affinity = a;
        }
    }

    public class GlyphPosition {
        public readonly Range<int> codeUnits;
        public readonly Range<double> xPos;

        public GlyphPosition(double start, double advance, Range<int> codeUnits) {
            this.xPos = new Range<double>(start: start, start + advance);
            this.codeUnits = codeUnits;
        }

        public GlyphPosition shift(double shift) {
            return new GlyphPosition(this.xPos.start + shift, this.xPos.end - this.xPos.start,
                codeUnits: this.codeUnits);
        }
    }

    public class Range<T> : IEquatable<Range<T>> {
        public readonly T start, end;

        public Range(T start, T end) {
            this.start = start;
            this.end = end;
        }

        public bool Equals(Range<T> other) {
            return EqualityComparer<T>.Default.Equals(x: this.start, y: other.start) &&
                   EqualityComparer<T>.Default.Equals(x: this.end, y: other.end);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, objB: obj)) {
                return false;
            }

            return obj is Range<T> && this.Equals((Range<T>) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (EqualityComparer<T>.Default.GetHashCode(obj: this.start) * 397) ^
                       EqualityComparer<T>.Default.GetHashCode(obj: this.end);
            }
        }

        public static bool operator ==(Range<T> left, Range<T> right) {
            return left.Equals(other: right);
        }

        public static bool operator !=(Range<T> left, Range<T> right) {
            return !left.Equals(other: right);
        }
    }

    public static class RangeUtils {
        public static Range<double> shift(Range<double> value, double shift) {
            return new Range<double>(value.start + shift, value.end + shift);
        }
    }


    public class GlyphLine {
        public readonly List<GlyphPosition> positions;
        public readonly int totalCountUnits;

        public GlyphLine(List<GlyphPosition> positions, int totalCountUnits) {
            this.positions = positions;
            this.totalCountUnits = totalCountUnits;
        }
    }


    public class Paragraph {
        const int TabSpaceCount = 4;

        const double kDoubleDecorationSpacing = 3.0;
        double[] _characterWidths;
        readonly List<CodeUnitRun> _codeUnitRuns = new List<CodeUnitRun>();
        readonly List<GlyphLine> _glyphLines = new List<GlyphLine>();
        readonly List<double> _lineBaseLines = new List<double>();
        readonly List<double> _lineHeights = new List<double>();
        readonly List<LineRange> _lineRanges = new List<LineRange>();
        readonly List<double> _lineWidths = new List<double>();


        bool _needsLayout = true;
        readonly List<PaintRecord> _paintRecords = new List<PaintRecord>();

        ParagraphStyle _paragraphStyle;

        StyledRuns _runs;
        readonly TabStops _tabStops = new TabStops();

        string _text;

        // private double _characterWidth;

        public double height {
            get { return this._lineHeights.Count == 0 ? 0 : this._lineHeights[this._lineHeights.Count - 1]; }
        }

        public double minIntrinsicWidth { get; private set; }

        public double maxIntrinsicWidth { get; private set; }

        public double width { get; private set; }


        public double alphabeticBaseline { get; private set; }

        public double ideographicBaseline { get; private set; }

        public bool didExceedMaxLines { get; private set; }

        public void paint(Canvas canvas, Offset offset) {
            foreach (var paintRecord in this._paintRecords) {
                var paint = new Paint {
                    filterMode = FilterMode.Bilinear,
                    color = paintRecord.style.color
                };
                canvas.drawTextBlob(textBlob: paintRecord.text, paintRecord.offset + offset, paint: paint);
                this.paintDecorations(canvas: canvas, record: paintRecord, baseOffset: offset);
            }
        }

        public void layout(ParagraphConstraints constraints) {
            if (!this._needsLayout && this.width == constraints.width) {
                return;
            }

            var textStyle = this._paragraphStyle.getTextStyle();
            this._tabStops.setFont(font: FontManager.instance.getOrCreate(name: textStyle.fontFamily).font,
                size: textStyle.UnityFontSize);

            this._needsLayout = false;
            this.width = Math.Floor(d: constraints.width);

            this.computeLineBreak();

            this._paintRecords.Clear();
            this._lineHeights.Clear();
            this._lineBaseLines.Clear();
            this._codeUnitRuns.Clear();
            this._glyphLines.Clear();

            int styleMaxLines = this._paragraphStyle.maxLines ?? int.MaxValue;
            var lineLimit = Math.Min(val1: styleMaxLines, val2: this._lineRanges.Count);
            this.didExceedMaxLines = this._lineRanges.Count > styleMaxLines;
            double maxWordWidth = 0;

            Layout layout = new Layout();
            layout.setTabStops(tabStops: this._tabStops);
            TextBlobBuilder builder = new TextBlobBuilder();
            int styleRunIndex = 0;
            double yOffset = 0;
            double preMaxDescent = 0;

            List<CodeUnitRun> lineCodeUnitRuns = new List<CodeUnitRun>();
            List<GlyphPosition> glyphPositions = new List<GlyphPosition>();

            for (int lineNumber = 0; lineNumber < lineLimit; ++lineNumber) {
                var lineRange = this._lineRanges[index: lineNumber];
                double wordGapWidth = 0;

                // Break the line into words if justification should be applied.
                int wordIndex = 0;
                bool justifyLine = this._paragraphStyle.textAlign == TextAlign.justify &&
                                   lineNumber != lineLimit - 1 &&
                                   !lineRange.hardBreak;
                var words = this.findWords(start: lineRange.start, end: lineRange.end);
                if (justifyLine) {
                    if (words.Count > 1) {
                        wordGapWidth = (this.width - this._lineWidths[index: lineNumber]) / (words.Count - 1);
                    }
                }

                // Exclude trailing whitespace from right-justified lines so the last
                // visible character in the line will be flush with the right margin.
                int lineEndIndex = this._paragraphStyle.textAlign == TextAlign.right ||
                                   this._paragraphStyle.textAlign == TextAlign.center
                    ? lineRange.endExcludingWhitespace
                    : lineRange.end;


                List<LineStyleRun> lineRuns = new List<LineStyleRun>();
                while (styleRunIndex < this._runs.size) {
                    var styleRun = this._runs.getRun(index: styleRunIndex);
                    if (styleRun.start < lineEndIndex && styleRun.end > lineRange.start) {
                        lineRuns.Add(new LineStyleRun(Math.Max(val1: styleRun.start, val2: lineRange.start),
                            Math.Min(val1: styleRun.end, val2: lineEndIndex), style: styleRun.style));
                    }

                    if (styleRun.end >= lineEndIndex) {
                        break;
                    }

                    styleRunIndex++;
                }

                double runXOffset = 0;
                double justifyXOffset = 0;
                lineCodeUnitRuns.Clear();

                List<GlyphPosition> lineGlyphPositions = new List<GlyphPosition>();
                List<PaintRecord> paintRecords = new List<PaintRecord>();
                for (int i = 0; i < lineRuns.Count; ++i) {
                    var run = lineRuns[index: i];
                    int textStart = Math.Max(val1: run.start, val2: lineRange.start);
                    int textEnd = Math.Min(val1: run.end, val2: lineEndIndex);
                    int textCount = textEnd - textStart;

                    layout.doLayout(offset: runXOffset, text: this._text, start: textStart, count: textCount,
                        style: run.style);
                    if (layout.nGlyphs() == 0) {
                        continue;
                    }

                    double wordStartPosition = double.NaN;
                    // var layoutAdvances = layout.getAdvances();
                    builder.allocRunPos(style: run.style, text: this._text, offset: textStart, size: textCount);
                    builder.setBounds(layout.getBounds());
                    glyphPositions.Clear();

                    for (int glyphIndex = 0; glyphIndex < textCount; ++glyphIndex) {
                        double glyphXOffset = layout.getX(index: glyphIndex) + justifyXOffset;
                        builder.positions[glyphIndex] = new Vector2d(
                            x: glyphXOffset, layout.getY(index: glyphIndex)
                        );

                        float glyphAdvance = layout.getCharAdvance(index: glyphIndex);
                        glyphPositions.Add(new GlyphPosition(runXOffset + glyphXOffset, advance: glyphAdvance,
                            new Range<int>(textStart + glyphIndex, textStart + glyphIndex + 1)));
                        if (wordIndex < words.Count && words[index: wordIndex].start == run.start + glyphIndex) {
                            wordStartPosition = runXOffset + glyphXOffset;
                        }

                        if (wordIndex < words.Count && words[index: wordIndex].end == run.start + glyphIndex + 1) {
                            if (justifyLine) {
                                justifyXOffset += wordGapWidth;
                            }

                            wordIndex++;
                            if (!double.IsNaN(d: wordStartPosition)) {
                                double wordWidth =
                                    glyphPositions[glyphPositions.Count - 1].xPos.end - wordStartPosition;
                                maxWordWidth = Math.Max(val1: wordWidth, val2: maxWordWidth);
                                wordStartPosition = double.NaN;
                            }
                        }
                    }

                    if (glyphPositions.Count == 0) {
                        continue;
                    }

                    var font = FontManager.instance.getOrCreate(name: run.style.fontFamily).font;
                    var metrics = FontMetrics.fromFont(font: font, fontSize: run.style.UnityFontSize);
                    paintRecords.Add(new PaintRecord(style: run.style, new Offset(dx: runXOffset, 0),
                        builder.make(), metrics: metrics, line: lineNumber, layout.getAdvance()
                    ));
                    lineGlyphPositions.AddRange(collection: glyphPositions);
                    var codeUnitPositions = new List<GlyphPosition>(collection: glyphPositions);
                    lineCodeUnitRuns.Add(new CodeUnitRun(positions: codeUnitPositions,
                        new Range<int>(start: run.start, end: run.end),
                        line: lineNumber,
                        new Range<double>(start: glyphPositions[0].xPos.start,
                            end: glyphPositions[glyphPositions.Count - 1].xPos.end),
                        fontMetrics: metrics, direction: TextDirection.ltr));
                    runXOffset += layout.getAdvance();
                }

                double lineXOffset = this.getLineXOffset(lineTotalAdvance: runXOffset);
                if (lineXOffset != 0) {
                    foreach (var codeUnitRun in lineCodeUnitRuns) {
                        codeUnitRun.Shift(shift: lineXOffset);
                    }

                    for (int i = 0; i < lineGlyphPositions.Count; ++i) {
                        lineGlyphPositions[index: i] = lineGlyphPositions[index: i].shift(shift: lineXOffset);
                    }
                }

                int nextLineStart = lineNumber < this._lineRanges.Count - 1
                    ? this._lineRanges[lineNumber + 1].start
                    : this._text.Length;
                this._glyphLines.Add(new GlyphLine(positions: lineGlyphPositions, nextLineStart - lineRange.start));
                this._codeUnitRuns.AddRange(collection: lineCodeUnitRuns);

                double maxLineSpacing = 0;
                double maxDescent = 0;

                var updateLineMetrics = new Action<FontMetrics, TextStyle>((metrics, style) => {
                    double lineSpacing = lineNumber == 0
                        ? -metrics.ascent * style.height
                        : (-metrics.ascent + metrics.leading) * style.height;
                    if (lineSpacing > maxLineSpacing) {
                        maxLineSpacing = lineSpacing;
                        if (lineNumber == 0) {
                            this.alphabeticBaseline = lineSpacing;
                            this.ideographicBaseline =
                                (metrics.underlinePosition ?? 0.0 - metrics.ascent) * style.height;
                        }
                    }

                    double descent = metrics.descent * style.height;
                    maxDescent = Math.Max(val1: descent, val2: maxDescent);
                });

                foreach (var paintRecord in paintRecords) {
                    updateLineMetrics(arg1: paintRecord.metrics, arg2: paintRecord.style);
                }

                if (paintRecords.Count == 0) {
                    var defaultStyle = this._paragraphStyle.getTextStyle();
                    var defaultFont = FontManager.instance.getOrCreate(name: defaultStyle.fontFamily).font;
                    var metrics = FontMetrics.fromFont(font: defaultFont, fontSize: defaultStyle.UnityFontSize);
                    updateLineMetrics(arg1: metrics, arg2: defaultStyle);
                }

                this._lineHeights.Add(
                    (this._lineHeights.Count == 0 ? 0 : this._lineHeights[this._lineHeights.Count - 1])
                    + Math.Round(maxLineSpacing + maxDescent));
                this._lineBaseLines.Add(this._lineHeights[this._lineHeights.Count - 1] - maxDescent);
                yOffset += Math.Round(maxLineSpacing + preMaxDescent);
                preMaxDescent = maxDescent;

                foreach (var paintRecord in paintRecords) {
                    paintRecord.offset = new Offset(paintRecord.offset.dx + lineXOffset, dy: yOffset);
                    this._paintRecords.Add(item: paintRecord);
                }
            }

            this.maxIntrinsicWidth = 0;
            double lineBlockWidth = 0;
            for (int i = 0; i < this._lineWidths.Count; ++i) {
                lineBlockWidth += this._lineWidths[index: i];
                if (this._lineRanges[index: i].hardBreak) {
                    this.maxIntrinsicWidth = Math.Max(val1: lineBlockWidth, val2: this.maxIntrinsicWidth);
                    lineBlockWidth = 0;
                }
            }

            this.maxIntrinsicWidth = Math.Max(val1: lineBlockWidth, val2: this.maxIntrinsicWidth);

            if (this._paragraphStyle.maxLines == 1 || this._paragraphStyle.maxLines == null &&
                this._paragraphStyle.ellipsized()) {
                this.minIntrinsicWidth = this.maxIntrinsicWidth;
            }
            else {
                this.minIntrinsicWidth = Math.Min(val1: maxWordWidth, val2: this.maxIntrinsicWidth);
            }
        }


        public void setText(string text, StyledRuns runs) {
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

                double top = run.lineNumber == 0 ? 0 : this._lineHeights[run.lineNumber - 1];
                double bottom = this._lineHeights[index: run.lineNumber];
                double left, right;
                if (run.codeUnits.start >= start && run.codeUnits.end <= end) {
                    left = run.xPos.start;
                    right = run.xPos.end;
                }
                else {
                    left = double.MaxValue;
                    right = double.MinValue;
                    foreach (var gp in run.positions) {
                        if (gp.codeUnits.start >= start && gp.codeUnits.end <= end) {
                            left = Math.Min(val1: left, val2: gp.xPos.start);
                            right = Math.Max(val1: right, val2: gp.xPos.end);
                        }
                    }

                    if (left == double.MaxValue || right == double.MinValue) {
                        continue;
                    }
                }

                List<TextBox> boxs;
                if (!lineBoxes.TryGetValue(key: run.lineNumber, value: out boxs)) {
                    boxs = new List<TextBox>();
                    lineBoxes.Add(key: run.lineNumber, value: boxs);
                }

                boxs.Add(TextBox.fromLTBD(left: left, top: top, right: right, bottom: bottom,
                    direction: run.direction));
            }

            for (int lineNumber = 0; lineNumber < this._lineRanges.Count; ++lineNumber) {
                var line = this._lineRanges[index: lineNumber];
                if (line.start >= end) {
                    break;
                }

                if (line.endIncludingNewLine <= start) {
                    continue;
                }

                if (!lineBoxes.ContainsKey(key: lineNumber)) {
                    if (line.end != line.endIncludingNewLine && line.end >= start && line.endIncludingNewLine <= end) {
                        var x = this._lineWidths[index: lineNumber];
                        var top = lineNumber > 0 ? this._lineHeights[lineNumber - 1] : 0;
                        var bottom = this._lineHeights[index: lineNumber];
                        lineBoxes.Add(key: lineNumber, new List<TextBox> {
                            TextBox.fromLTBD(
                                left: x, top: top, right: x, bottom: bottom, direction: TextDirection.ltr)
                        });
                    }
                }
            }

            var result = new List<TextBox>();
            foreach (var keyValuePair in lineBoxes) {
                result.AddRange(collection: keyValuePair.Value);
            }

            return result;
        }

        public TextBox getNextLineStartRect() {
            if (this._text.Length == 0 || this._text[this._text.Length - 1] != '\n') {
                return null;
            }

            var lineNumber = this.getLineCount() - 1;
            var top = lineNumber > 0 ? this._lineHeights[lineNumber - 1] : 0;
            var bottom = this._lineHeights[index: lineNumber];
            return TextBox.fromLTBD(0, top: top, 0, bottom: bottom, direction: TextDirection.ltr);
        }

        public PositionWithAffinity getGlyphPositionAtCoordinate(double dx, double dy) {
            if (this._lineHeights.Count == 0) {
                return new PositionWithAffinity(0, a: TextAffinity.downstream);
            }

            int yIndex;
            for (yIndex = 0; yIndex < this._lineHeights.Count - 1; ++yIndex) {
                if (dy < this._lineHeights[index: yIndex]) {
                    break;
                }
            }

            var lineGlyphPosition = this._glyphLines[index: yIndex].positions;
            if (lineGlyphPosition.Count == 0) {
                int lineStartIndex = this._glyphLines.Where((g, i) => i < yIndex).Sum(gl => gl.totalCountUnits);
                return new PositionWithAffinity(p: lineStartIndex, a: TextAffinity.downstream);
            }


            GlyphPosition gp = null;
            for (int xIndex = 0; xIndex < lineGlyphPosition.Count; ++xIndex) {
                double glyphEnd = xIndex < lineGlyphPosition.Count - 1
                    ? lineGlyphPosition[xIndex + 1].xPos.start
                    : lineGlyphPosition[index: xIndex].xPos.end;
                if (dx < glyphEnd) {
                    gp = lineGlyphPosition[index: xIndex];
                    break;
                }
            }

            if (gp == null) {
                GlyphPosition lastGlyph = lineGlyphPosition[lineGlyphPosition.Count - 1];
                return new PositionWithAffinity(p: lastGlyph.codeUnits.end, a: TextAffinity.upstream);
            }

            TextDirection direction = TextDirection.ltr;
            foreach (var run in this._codeUnitRuns) {
                if (gp.codeUnits.start >= run.codeUnits.start && gp.codeUnits.end <= run.codeUnits.end) {
                    direction = run.direction;
                    break;
                }
            }

            double glyphCenter = (gp.xPos.start + gp.xPos.end) / 2;
            if (direction == TextDirection.ltr && dx < glyphCenter ||
                direction == TextDirection.rtl && dx >= glyphCenter) {
                return new PositionWithAffinity(p: gp.codeUnits.start, a: TextAffinity.downstream);
            }

            return new PositionWithAffinity(p: gp.codeUnits.end, a: TextAffinity.upstream);
        }

        public int getLine(TextPosition position) {
            D.assert(result: !this._needsLayout);
            if (position.offset < 0) {
                return 0;
            }

            var offset = position.offset;
            if (position.affinity == TextAffinity.upstream && offset > 0) {
                offset = _isUtf16Surrogate(this._text[offset - 1]) ? offset - 2 : offset - 1;
            }

            var lineCount = this.getLineCount();
            for (int lineIndex = 0; lineIndex < this.getLineCount(); ++lineIndex) {
                var line = this._lineRanges[index: lineIndex];
                if (offset >= line.start && offset < line.endIncludingNewLine) {
                    return lineIndex;
                }
            }

            return Math.Max(lineCount - 1, 0);
        }

        public LineRange getLineRange(int lineIndex) {
            return this._lineRanges[index: lineIndex];
        }

        public Range<int> getWordBoundary(int offset) {
            WordSeparate s = new WordSeparate(text: this._text);
            return s.findWordRange(index: offset);
        }

        public int getLineCount() {
            return this._lineHeights.Count;
        }

        void computeLineBreak() {
            this._lineRanges.Clear();
            this._lineWidths.Clear();
            this.maxIntrinsicWidth = 0;

            var newLinePositions = new List<int>();
            for (var i = 0; i < this._text.Length; i++) {
                if (this._text[index: i] == '\n') {
                    newLinePositions.Add(item: i);
                }
            }

            newLinePositions.Add(item: this._text.Length);

            var lineBreaker = new LineBreaker();
            int runIndex = 0;
            for (var newlineIndex = 0; newlineIndex < newLinePositions.Count; ++newlineIndex) {
                var blockStart = newlineIndex > 0 ? newLinePositions[newlineIndex - 1] + 1 : 0;
                var blockEnd = newLinePositions[index: newlineIndex];
                var blockSize = blockEnd - blockStart;
                if (blockSize == 0) {
                    this._lineRanges.Add(new LineRange(start: blockStart, end: blockEnd,
                        endExcludingWhitespace: blockEnd,
                        blockEnd < this._text.Length ? blockEnd + 1 : blockEnd, true));
                    this._lineWidths.Add(0);
                    continue;
                }

                lineBreaker.setLineWidth((float) this.width);
                lineBreaker.resize(size: blockSize);
                lineBreaker.setTabStops(tabStops: this._tabStops);
                lineBreaker.setText(text: this._text, textOffset: blockStart, textLength: blockSize);

                while (runIndex < this._runs.size) {
                    var run = this._runs.getRun(index: runIndex);
                    if (run.start >= blockEnd) {
                        break;
                    }

                    if (run.end < blockStart) {
                        runIndex++;
                        continue;
                    }

                    int runStart = Math.Max(val1: run.start, val2: blockStart) - blockStart;
                    int runEnd = Math.Min(val1: run.end, val2: blockEnd) - blockStart;
                    lineBreaker.addStyleRun(style: run.style, start: runStart, end: runEnd);

                    if (run.end > blockEnd) {
                        break;
                    }

                    runIndex++;
                }

                int breaksCount = lineBreaker.computeBreaks();
                List<int> breaks = lineBreaker.getBreaks();
                List<float> widths = lineBreaker.getWidths();
                for (int i = 0; i < breaksCount; ++i) {
                    var breakStart = i > 0 ? breaks[i - 1] : 0;
                    var lineStart = breakStart + blockStart;
                    var lineEnd = breaks[index: i] + blockStart;
                    bool hardBreak = i == breaksCount - 1;
                    var lineEndIncludingNewline =
                        hardBreak && lineEnd < this._text.Length ? lineEnd + 1 : lineEnd;
                    var lineEndExcludingWhitespace = lineEnd;
                    while (lineEndExcludingWhitespace > lineStart &&
                           LayoutUtils.isLineEndSpace(this._text[lineEndExcludingWhitespace - 1])) {
                        lineEndExcludingWhitespace--;
                    }

                    this._lineRanges.Add(new LineRange(start: lineStart, end: lineEnd,
                        endExcludingWhitespace: lineEndExcludingWhitespace,
                        endIncludingNewLine: lineEndIncludingNewline, hardBreak: hardBreak));
                    this._lineWidths.Add(widths[index: i]);
                }

                lineBreaker.finish();
            }
        }

        List<Range<int>> findWords(int start, int end) {
            var inWord = false;
            int wordStart = 0;
            List<Range<int>> words = new List<Range<int>>();
            for (int i = start; i < end; ++i) {
                bool isSpace = LayoutUtils.isWordSpace(this._text[index: i]);
                if (!inWord && !isSpace) {
                    wordStart = i;
                    inWord = true;
                }
                else if (inWord && isSpace) {
                    words.Add(new Range<int>(start: wordStart, end: i));
                    inWord = false;
                }
            }

            if (inWord) {
                words.Add(new Range<int>(start: wordStart, end: end));
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
            double underLineThickness = metrics.underlineThickness ?? record.style.fontSize / 14.0;
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
                double yOffset = i * underLineThickness * kDoubleDecorationSpacing;
                double yOffsetOriginal = yOffset;
                if (decoration != null && decoration.contains(other: TextDecoration.underline)) {
                    // underline
                    yOffset += metrics.underlinePosition ?? underLineThickness;
                    canvas.drawLine(new Offset(dx: x, y + yOffset), new Offset(x + width, y + yOffset), paint: paint);
                    yOffset = yOffsetOriginal;
                }

                if (decoration != null && decoration.contains(other: TextDecoration.overline)) {
                    yOffset += metrics.ascent;
                    canvas.drawLine(new Offset(dx: x, y + yOffset), new Offset(x + width, y + yOffset), paint: paint);
                    yOffset = yOffsetOriginal;
                }

                if (decoration != null && decoration.contains(other: TextDecoration.lineThrough)) {
                    yOffset += (decorationCount - 1.0) * underLineThickness * kDoubleDecorationSpacing / -2.0;
                    yOffset += metrics.strikeoutPosition ?? (metrics.fxHeight ?? 0) / -2.0;
                    canvas.drawLine(new Offset(dx: x, y + yOffset), new Offset(x + width, y + yOffset), paint: paint);
                    yOffset = yOffsetOriginal;
                }
            }
        }

        double getLineXOffset(double lineTotalAdvance) {
            if (double.IsInfinity(d: this.width)) {
                return 0;
            }

            if (this._paragraphStyle.textAlign == TextAlign.right) {
                return this.width - lineTotalAdvance;
            }

            if (this._paragraphStyle.textAlign == TextAlign.center) {
                return (this.width - lineTotalAdvance) / 2;
            }

            return 0;
        }

        static bool _isUtf16Surrogate(int value) {
            return (value & 0xF800) == 0xD800;
        }

        public class LineRange {
            public readonly int end;
            public readonly int endExcludingWhitespace;
            public readonly int endIncludingNewLine;
            public readonly bool hardBreak;

            public readonly int start;

            public LineRange(int start, int end, int endExcludingWhitespace, int endIncludingNewLine, bool hardBreak) {
                this.start = start;
                this.end = end;
                this.endExcludingWhitespace = endExcludingWhitespace;
                this.endIncludingNewLine = endIncludingNewLine;
                this.hardBreak = hardBreak;
            }
        }
    }

    public class SplayTree<TKey, TValue> : IDictionary<TKey, TValue> where TKey : IComparable<TKey> {
        SplayTreeNode root;
        int version;

        public void Add(TKey key, TValue value) {
            this.Set(key: key, value: value, true);
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            this.Set(key: item.Key, value: item.Value, true);
        }

        public void Clear() {
            this.root = null;
            this.Count = 0;
            this.version++;
        }

        public bool ContainsKey(TKey key) {
            if (this.Count == 0) {
                return false;
            }

            this.Splay(key: key);

            return key.CompareTo(other: this.root.Key) == 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            if (this.Count == 0) {
                return false;
            }

            this.Splay(key: item.Key);

            return item.Key.CompareTo(other: this.root.Key) == 0 &&
                   (ReferenceEquals(objA: this.root.Value, objB: item.Value) ||
                    !ReferenceEquals(objA: item.Value, null) && item.Value.Equals(obj: this.root.Value));
        }

        public bool Remove(TKey key) {
            if (this.Count == 0) {
                return false;
            }

            this.Splay(key: key);

            if (key.CompareTo(other: this.root.Key) != 0) {
                return false;
            }

            if (this.root.LeftChild == null) {
                this.root = this.root.RightChild;
            }
            else {
                var swap = this.root.RightChild;
                this.root = this.root.LeftChild;
                this.Splay(key: key);
                this.root.RightChild = swap;
            }

            this.version++;
            this.Count--;
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            if (this.Count == 0) {
                value = default;
                return false;
            }

            this.Splay(key: key);
            if (key.CompareTo(other: this.root.Key) != 0) {
                value = default;
                return false;
            }

            value = this.root.Value;
            return true;
        }

        public TValue this[TKey key] {
            get {
                if (this.Count == 0) {
                    throw new KeyNotFoundException("The key was not found in the tree.");
                }

                this.Splay(key: key);
                if (key.CompareTo(other: this.root.Key) != 0) {
                    throw new KeyNotFoundException("The key was not found in the tree.");
                }

                return this.root.Value;
            }

            set { this.Set(key: key, value: value, false); }
        }

        public int Count { get; private set; }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            if (this.Count == 0) {
                return false;
            }

            this.Splay(key: item.Key);

            if (item.Key.CompareTo(other: this.root.Key) == 0 &&
                (ReferenceEquals(objA: this.root.Value, objB: item.Value) || !ReferenceEquals(objA: item.Value, null) &&
                 item.Value.Equals(obj: this.root.Value))) {
                return false;
            }

            if (this.root.LeftChild == null) {
                this.root = this.root.RightChild;
            }
            else {
                var swap = this.root.RightChild;
                this.root = this.root.LeftChild;
                this.Splay(key: item.Key);
                this.root.RightChild = swap;
            }

            this.version++;
            this.Count--;
            return true;
        }

        public ICollection<TKey> Keys {
            get { return new TiedList<TKey>(this, version: this.version, this.AsList(node => node.Key)); }
        }

        public ICollection<TValue> Values {
            get { return new TiedList<TValue>(this, version: this.version, this.AsList(node => node.Value)); }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            this.AsList(node => new KeyValuePair<TKey, TValue>(key: node.Key, value: node.Value))
                .CopyTo(array: array, arrayIndex: arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return new TiedList<KeyValuePair<TKey, TValue>>(this, version: this.version,
                this.AsList(node => new KeyValuePair<TKey, TValue>(key: node.Key, value: node.Value))).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        void Set(TKey key, TValue value, bool throwOnExisting) {
            if (this.Count == 0) {
                this.version++;
                this.root = new SplayTreeNode(key: key, value: value);
                this.Count = 1;
                return;
            }

            this.Splay(key: key);

            var c = key.CompareTo(other: this.root.Key);
            if (c == 0) {
                if (throwOnExisting) {
                    throw new ArgumentException("An item with the same key already exists in the tree.");
                }

                this.version++;
                this.root.Value = value;
                return;
            }

            var n = new SplayTreeNode(key: key, value: value);
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
            this.Count++;
            this.Splay(key: key);
            this.version++;
        }

        public KeyValuePair<TKey, TValue> First() {
            SplayTreeNode t = this.root;
            if (t == null) {
                throw new NullReferenceException("The root of this tree is null!");
            }

            while (t.LeftChild != null) {
                t = t.LeftChild;
            }

            return new KeyValuePair<TKey, TValue>(key: t.Key, value: t.Value);
        }

        public KeyValuePair<TKey, TValue> FirstOrDefault() {
            SplayTreeNode t = this.root;
            if (t == null) {
                return new KeyValuePair<TKey, TValue>(default, default);
            }

            while (t.LeftChild != null) {
                t = t.LeftChild;
            }

            return new KeyValuePair<TKey, TValue>(key: t.Key, value: t.Value);
        }

        public KeyValuePair<TKey, TValue> Last() {
            SplayTreeNode t = this.root;
            if (t == null) {
                throw new NullReferenceException("The root of this tree is null!");
            }

            while (t.RightChild != null) {
                t = t.RightChild;
            }

            return new KeyValuePair<TKey, TValue>(key: t.Key, value: t.Value);
        }

        public KeyValuePair<TKey, TValue> LastOrDefault() {
            SplayTreeNode t = this.root;
            if (t == null) {
                return new KeyValuePair<TKey, TValue>(default, default);
            }

            while (t.RightChild != null) {
                t = t.RightChild;
            }

            return new KeyValuePair<TKey, TValue>(key: t.Key, value: t.Value);
        }

        void Splay(TKey key) {
            SplayTreeNode l, r, t, y, header;
            l = r = header = new SplayTreeNode(default, default);
            t = this.root;
            while (true) {
                var c = key.CompareTo(other: t.Key);
                if (c < 0) {
                    if (t.LeftChild == null) {
                        break;
                    }

                    if (key.CompareTo(other: t.LeftChild.Key) < 0) {
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

                    if (key.CompareTo(other: t.RightChild.Key) > 0) {
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

        public void Trim(int depth) {
            if (depth < 0) {
                throw new ArgumentOutOfRangeException("depth", "The trim depth must not be negative.");
            }

            if (this.Count == 0) {
                return;
            }

            if (depth == 0) {
                this.Clear();
            }
            else {
                var prevCount = this.Count;
                this.Count = this.Trim(node: this.root, depth - 1);
                if (prevCount != this.Count) {
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

            int count = 1;

            if (node.LeftChild != null) {
                count += this.Trim(node: node.LeftChild, depth - 1);
            }

            if (node.RightChild != null) {
                count += this.Trim(node: node.RightChild, depth - 1);
            }

            return count;
        }

        IList<TEnumerator> AsList<TEnumerator>(Func<SplayTreeNode, TEnumerator> selector) {
            if (this.root == null) {
                return new TEnumerator[0];
            }

            var result = new List<TEnumerator>(capacity: this.Count);
            this.PopulateList(node: this.root, list: result, selector: selector);
            return result;
        }

        void PopulateList<TEnumerator>(SplayTreeNode node, List<TEnumerator> list,
            Func<SplayTreeNode, TEnumerator> selector) {
            if (node.LeftChild != null) {
                this.PopulateList(node: node.LeftChild, list: list, selector: selector);
            }

            list.Add(selector(arg: node));
            if (node.RightChild != null) {
                this.PopulateList(node: node.RightChild, list: list, selector: selector);
            }
        }

        sealed class SplayTreeNode {
            public readonly TKey Key;
            public SplayTreeNode LeftChild;
            public SplayTreeNode RightChild;

            public TValue Value;

            public SplayTreeNode(TKey key, TValue value) {
                this.Key = key;
                this.Value = value;
            }
        }

        sealed class TiedList<T> : IList<T> {
            readonly IList<T> backingList;
            readonly SplayTree<TKey, TValue> tree;
            readonly int version;

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

                return this.backingList.IndexOf(item: item);
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

                    return this.backingList[index: index];
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

                return this.backingList.Contains(item: item);
            }

            public void CopyTo(T[] array, int arrayIndex) {
                if (this.tree.version != this.version) {
                    throw new InvalidOperationException("The collection has been modified.");
                }

                this.backingList.CopyTo(array: array, arrayIndex: arrayIndex);
            }

            public int Count {
                get { return this.tree.Count; }
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