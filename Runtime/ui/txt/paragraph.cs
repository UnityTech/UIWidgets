using System;
using System.Collections.Generic;
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
    
    
//flutter  build ios --debug
//--local-engine-src-path /Users/fzhang/codebase/flutter/engine/src  --local-engine=ios_debug_sim_unopt
// flutter clean --local-engine-src-path /Users/fzhang/codebase/flutter/engine/src  --local-engine=ios_debug_sim_unopt --verbose 
    // flutter run --local-engine-src-path /Users/fzhang/codebase/flutter/engine/src  --local-engine=ios_debug_sim_unopt 
    public class CodeUnitRun {
        public int lineNumber;
        public TextDirection direction;
        public IndexRange codeUnits;
        public FontMetrics fontMetrics;
        public Range<double> xPos;
        public List<Range<double>> positions;

        public CodeUnitRun(IndexRange cu, int line, Range<double> xPos,  FontMetrics fontMetrics, TextDirection direction) {
            this.lineNumber = line;
            this.codeUnits = cu;
            this.xPos = xPos;
            this.fontMetrics = fontMetrics;
            this.direction = direction;
        }

        public void Shift(double shift) {
            this.xPos.start += shift;
            this.xPos.end += shift;
        }
    }

    
    public class FontMetrics {
        public readonly double ascent;
        public readonly double leading = 0.0;
        public readonly double descent;
        public readonly double? underlineThickness;
        public readonly double? underlinePosition;
        public readonly double? strikeoutPosition;
        public readonly double? fxHeight;

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

        public static FontMetrics fromFont(Font font, int fontSize, double? height) {
            var ascent = -font.ascent * (height ?? 1.0) * fontSize / font.fontSize;
            var descent = (font.lineHeight - font.ascent) * (height ?? 1.0) * fontSize / font.fontSize;
            double? fxHeight = null;
            font.RequestCharactersInTexture("x", fontSize);
            CharacterInfo charInfo;
            if (font.GetCharacterInfo('x', out charInfo, fontSize)) {
                fxHeight = charInfo.glyphHeight;
            }
            return new FontMetrics(ascent, descent, fxHeight: fxHeight);
        }
    }

    public struct IndexRange : IEquatable<IndexRange> {
        public int start, end;

        public IndexRange(int s, int e) {
            this.start = s;
            this.end = e;
        }

        int width() {
            return this.end - this.start;
        }

        void shift(int delta) {
            this.start += delta;
            this.end += delta;
        }

        public bool Equals(IndexRange other) {
            return this.start == other.start && this.end == other.end;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            return obj is IndexRange && this.Equals((IndexRange) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.start * 397) ^ this.end;
            }
        }

        public static bool operator ==(IndexRange left, IndexRange right) {
            return left.Equals(right);
        }

        public static bool operator !=(IndexRange left, IndexRange right) {
            return !left.Equals(right);
        }
    }

    public class LineStyleRun {
        public readonly int start;
        public readonly int end;
        public readonly TextStyle style;

        public LineStyleRun(int start, int end, TextStyle style) {
            this.start = start;
            this.end = end;
            this.style = style;
        }
    }

    public class PositionWithAffinity {
        public readonly int position;
        public readonly TextAffinity affinity;

        public PositionWithAffinity(int p, TextAffinity a) {
            this.position = p;
            this.affinity = a;
        }
    }

    public class Range<T> : IEquatable<Range<T>> {
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

        public T start, end;
    }

    public class GlyphLine {
        public readonly List<Range<double>> positions;
        public readonly int totalCountUnits;

        public GlyphLine(List<Range<double>> positions, int totalCountUnits) {
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
        List<double> _lineWidths = new List<double>();
        List<double> _lineBaseLines = new List<double>();
        List<GlyphLine> _glyphLines = new List<GlyphLine>();
        double _maxIntrinsicWidth;
        double _minIntrinsicWidth;
        double _alphabeticBaseline;
        double _ideographicBaseline;
        double[] _characterWidths;
        List<double> _lineHeights = new List<double>();
        List<PaintRecord> _paintRecords = new List<PaintRecord>();
        List<CodeUnitRun> _codeUnitRuns = new List<CodeUnitRun>();
        bool _didExceedMaxLines;

        // private double _characterWidth;

        double _width;

        const double kDoubleDecorationSpacing = 3.0;


        // This function determines whether a character is a space that disappears at end of line.
        // It is the Unicode set: [[:General_Category=Space_Separator:]-[:Line_Break=Glue:]],
        // plus '\n'.
        // Note: all such characters are in the BMP, so it's ok to use code units for this.
      
        public double height {
            get { return this._lineHeights.Count == 0 ? 0 : this._lineHeights[this._lineHeights.Count - 1]; }
        }

        public double minIntrinsicWidth {
            get { return this._minIntrinsicWidth; }
        }

        public double maxIntrinsicWidth {
            get { return this._maxIntrinsicWidth; }
        }

        public double width {
            get { return this._width; }
        }


        public double alphabeticBaseline {
            get { return this._alphabeticBaseline; }
        }

        public double ideographicBaseline {
            get { return this._ideographicBaseline; }
        }

        public bool didExceedMaxLines {
            get { return this._didExceedMaxLines; }
        }

        public void paint(Canvas canvas, Offset offset) {
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

            this._needsLayout = false;
            this._width = Math.Floor(constraints.width);

            this.computeLineBreak();
            
            this._paintRecords.Clear();
            this._lineHeights.Clear();
            this._lineBaseLines.Clear();
            this._codeUnitRuns.Clear();
            this._glyphLines.Clear();

            int styleMaxLines = this._paragraphStyle.maxLines ?? int.MaxValue;
            var lineLimit = Math.Min(styleMaxLines, this._lineRanges.Count);
            this._didExceedMaxLines = this._lineRanges.Count > styleMaxLines;
            double maxWordWidth = 0;

            Layout layout = new Layout();
            TextBlobBuilder builder = new TextBlobBuilder();
            int styleRunIndex = 0;
            double yOffset = 0;
            double preMaxDescent = 0;
            
            List<CodeUnitRun> lineCodeUnitRuns = new List<CodeUnitRun>();
            List<Range<double>> lineGlyphPositions = new List<Range<double>>();
            List<Range<double>> glyphPositions = new List<Range<double>>();
            
            for (int lineNumber = 0; lineNumber < lineLimit; ++lineNumber) {
                var lineRange = this._lineRanges[lineNumber];
                double wordGapWidth = 0;
                
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
                        lineRuns.Add(new LineStyleRun(Math.Max(styleRun.start, lineRange.start),
                            Math.Min(styleRun.end, lineEndIndex), styleRun.style));
                    }

                    if (styleRun.end >= lineEndIndex) {
                        break;
                    }

                    styleRunIndex++;
                }

                double runXOffset = 0;
                double justifyXOffset = 0;
                lineCodeUnitRuns.Clear();
                lineGlyphPositions.Clear();
                List<PaintRecord> paintRecords = new List<PaintRecord>();
                for (int i = 0; i < lineRuns.Count; ++i) {
                    var run = lineRuns[i];
                    int textStart = Math.Max(run.start, lineRange.start);
                    int textEnd = Math.Min(run.end, lineEndIndex);
                    int textCount = textEnd - textStart;

                    layout.doLayout(this._text, textStart, textCount, run.style);
                    if (layout.nGlyphs() == 0) {
                        continue;
                    }

                    double wordStartPosition = Double.NaN;
                    var layoutAdvances = layout.getAdvances();
                    builder.allocRunPos(run.style, this._text, textStart, textCount);
                    builder.setBounds(layout.getBounds());
                    glyphPositions.Clear();

                    for (int glyphIndex = 0; glyphIndex < textCount; ++glyphIndex) {
                        double glyphXOffset = layout.getX(glyphIndex) + justifyXOffset;
                        builder.positions[glyphIndex] = new Vector2d(
                            glyphXOffset, layout.getY(glyphIndex)
                        );

                        float glyphAdvance = layout.getCharAdvance(glyphIndex);
                        glyphPositions.Add(new Range<double>(runXOffset + glyphXOffset, glyphAdvance));
                        if (wordIndex < words.Count && words[wordIndex].start == run.start + glyphIndex) {
                            wordStartPosition = runXOffset + glyphXOffset;
                        }

                        if (wordIndex < words.Count && words[wordIndex].end == run.start + glyphIndex + 1) {
                            // todo plus 1?
                            if (justifyLine) {
                                justifyXOffset += wordGapWidth;
                            }

                            wordIndex++;
                            if (!double.IsNaN(wordStartPosition)) {
                                double wordWidth = glyphPositions[glyphPositions.Count - 1].end - wordStartPosition;
                                maxWordWidth = Math.Max(wordWidth, maxWordWidth);
                                wordStartPosition = double.NaN;
                            }
                        }

                    }

                    if (glyphPositions.Count == 0) {
                        continue;
                    }

                    var font = FontManager.instance.getOrCreate(run.style.fontFamily).font;
                    var metrics = FontMetrics.fromFont(font, run.style.UnityFontSize, run.style.height);
                    paintRecords.Add(new PaintRecord(run.style, new Offset(runXOffset, 0),
                        builder.make(), metrics, lineNumber, layout.getAdvance()
                    ));
                    lineGlyphPositions.AddRange(glyphPositions);

                    lineCodeUnitRuns.Add(new CodeUnitRun(new IndexRange(run.start, run.end), lineNumber,
                        new Range<double>(glyphPositions[0].start, glyphPositions[glyphPositions.Count - 1].end),
                        metrics, TextDirection.ltr));
                    runXOffset += layout.getAdvance();

                }

                double lineXOffset = this.getLineXOffset(runXOffset);
                if (lineXOffset > 0) {
                    foreach (var codeUnitRun in lineCodeUnitRuns) {
                        codeUnitRun.Shift(lineXOffset);
                    }

                    foreach (var position in lineGlyphPositions) {
                        position.start += lineXOffset;
                        position.end += lineXOffset;
                    }
                }

                int nextLineStart = (lineNumber < this._lineRanges.Count - 1)
                    ? this._lineRanges[lineNumber + 1].start
                    : this._text.Length;
                this._glyphLines.Add(new GlyphLine(lineGlyphPositions, nextLineStart - lineRange.start));
                this._codeUnitRuns.AddRange(lineCodeUnitRuns);

                double maxLineSpacing = 0;
                double maxDescent = 0;

                var updateLineMetrics = new Action<FontMetrics, TextStyle>((FontMetrics metrics, TextStyle style) => {
                    double lineSpacing = (lineNumber == 0)
                        ? -metrics.ascent  * style.height
                        : (-metrics.ascent + metrics.leading) * (style.height);
                    if (lineSpacing > maxLineSpacing) {
                        maxLineSpacing = lineSpacing;
                        if (lineNumber == 0) {
                            this._alphabeticBaseline = lineSpacing;
                            this._ideographicBaseline = (metrics.underlinePosition??0.0 - metrics.ascent) * style.height;
                        }
                    }

                    double descent = metrics.descent * style.height;
                    maxDescent = Math.Max(descent, maxDescent);
                });

                foreach (var paintRecord in paintRecords) {
                    updateLineMetrics(paintRecord.metrics, paintRecord.style);
                }

                if (paintRecords.Count == 0) {
                    var defaultStyle = this._paragraphStyle.getTextStyle();
                    var defaultFont = FontManager.instance.getOrCreate(defaultStyle.fontFamily).font;
                    var metrics = FontMetrics.fromFont(defaultFont, defaultStyle.UnityFontSize, defaultStyle.height);
                    updateLineMetrics(metrics, defaultStyle);

                }

                this._lineHeights.Add(
                    (this._lineHeights.Count == 0 ? 0 : this._lineHeights[this._lineHeights.Count - 1])
                    + (maxLineSpacing + maxDescent)/*Math.Round(maxLineSpacing + maxDescent)*/);
                this._lineBaseLines.Add(this._lineHeights[this._lineHeights.Count - 1] - maxDescent);
                yOffset += maxLineSpacing + preMaxDescent/*Math.Round(maxLineSpacing + preMaxDescent)*/;
                preMaxDescent = maxDescent;

                foreach (var paintRecord in paintRecords) {
                    paintRecord.offset = new Offset(paintRecord.offset.dx + lineXOffset, yOffset);
                    this._paintRecords.Add(paintRecord);
                }
            }

            this._maxIntrinsicWidth = 0;
            double lineBlockWidth = 0;
            for (int i = 0; i < this._lineWidths.Count; ++i) {
                lineBlockWidth += this._lineWidths[i];
                if (this._lineRanges[i].hardBreak) {
                    this._maxIntrinsicWidth = Math.Max(lineBlockWidth, this._maxIntrinsicWidth);
                    lineBlockWidth = 0;
                }
            }
            this._maxIntrinsicWidth = Math.Max(lineBlockWidth, this._maxIntrinsicWidth);

            if (this._paragraphStyle.maxLines == 1 || (this._paragraphStyle.maxLines == null &&
                                                       this._paragraphStyle.ellipsized())) {
                this._minIntrinsicWidth = this.maxIntrinsicWidth;
            }
            else {
                this._minIntrinsicWidth = Math.Min(maxWordWidth, this.maxIntrinsicWidth);
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
            var lineBoxes = new SortedDictionary<int, List<TextBox>>();
            foreach (var run in this._codeUnitRuns) {
                if (run.codeUnits.start >= end) {
                    break;
                }

                if (run.codeUnits.end <= start) {
                    continue;
                }

                var baseLine = this._lineBaseLines[run.lineNumber];
                double top = baseLine - run.fontMetrics.ascent;
                double bottom = baseLine + run.fontMetrics.descent;

                var from = Math.Max(start, run.codeUnits.start);
                var to = Math.Min(end, run.codeUnits.end);
                if (from < to) {
                    List<TextBox> boxs;
                    if (!lineBoxes.TryGetValue(run.lineNumber, out boxs)) {
                        boxs = new List<TextBox>();
                        lineBoxes.Add(run.lineNumber, boxs);
                    }

                    double left = this._characterPositions[from].x;
                    double right = this._characterPositions[to - 1].x + this._characterWidths[to - 1];
                    boxs.Add(TextBox.fromLTBD(left, top, right, bottom, run.direction));
                }
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

        public PositionWithAffinity getGlyphPositionAtCoordinate(double dx, double dy) {
            if (this._lineHeights.Count == 0) {
                return new PositionWithAffinity(0, TextAffinity.downstream);
            }

            int yIndex;
            for (yIndex = 0; yIndex < this._lineHeights.Count - 1; ++yIndex) {
                if (dy < this._lineHeights[yIndex]) {
                    break;
                }
            }

            var line = this._lineRanges[yIndex];
            if (line.start >= line.end) {
                return new PositionWithAffinity(line.start, TextAffinity.downstream);
            }

            int index;
            for (index = line.start; index < line.end; ++index) {
                if (dx < this._characterPositions[index].x + this._characterWidths[index]) {
                    break;
                }
            }

            if (index >= line.end) {
                return new PositionWithAffinity(line.end, TextAffinity.upstream);
            }

            TextDirection direction = TextDirection.ltr;
            var codeUnit = this._codeUnitRuns.Find((u) => u.codeUnits.start >= index && index < u.codeUnits.end);
            if (codeUnit != null) {
                direction = codeUnit.direction;
            }

            double glyphCenter = (this._characterPositions[index].x + this._characterPositions[index].x +
                                  this._characterWidths[index]) / 2;
            if ((direction == TextDirection.ltr && dx < glyphCenter) ||
                (direction == TextDirection.rtl && dx >= glyphCenter)) {
                return new PositionWithAffinity(index, TextAffinity.downstream);
            } else {
                return new PositionWithAffinity(index + 1, TextAffinity.upstream);
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

            return Math.Max(lineCount - 1, 0);
        }

        public LineRange getLineRange(int lineIndex) {
            return this._lineRanges[lineIndex];
        }

        public IndexRange getWordBoundary(int offset) {
            WordSeparate s = new WordSeparate(this._text);
            return s.findWordRange(offset);
        }

        public static void offsetCharacters(Vector2d offset, Vector2d[] characterPos, int start, int end) {
            if (characterPos != null) {
                for (int i = start; i < characterPos.Length && i < end; ++i) {
                    characterPos[i] = characterPos[i] + offset;
                }
            }
        }

        public int getLineCount() {
            return this._lineHeights.Count;
        }

        void computeLineBreak() {
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

                lineBreaker.setLineWidth((float)this._width);
                lineBreaker.resize(blockSize);
                lineBreaker.setTabStops(null, 14);
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

                    int runStart = Math.Max(run.start, blockStart) - blockStart;
                    int runEnd = Math.Min(run.end, blockEnd) - blockStart;
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

            return;
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
                } else if (inWord && isSpace) {
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
            } else {
                paint.color = record.style.decorationColor;
            }


            var width = record.runWidth;
            var metrics = record.metrics;
            double underLineThickness = metrics.underlineThickness ?? (record.style.fontSize / 14.0);
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
                if (decoration != null && decoration.contains(TextDecoration.underline)) {
                    // underline
                    yOffset += metrics.underlinePosition ?? underLineThickness;
                    canvas.drawLine(new Offset(x, y + yOffset), new Offset(x + width, y + yOffset), paint);
                    yOffset = yOffsetOriginal;
                }

                if (decoration != null && decoration.contains(TextDecoration.overline)) {
                    yOffset -= metrics.ascent;
                    canvas.drawLine(new Offset(x, y + yOffset), new Offset(x + width, y + yOffset), paint);
                    yOffset = yOffsetOriginal;
                }

                if (decoration != null && decoration.contains(TextDecoration.lineThrough)) {
                    yOffset += (decorationCount - 1.0) * underLineThickness * kDoubleDecorationSpacing / -2.0;
                    yOffset += metrics.strikeoutPosition ?? (metrics.fxHeight ?? 0) / -2.0;
                    canvas.drawLine(new Offset(x, y + yOffset), new Offset(x + width, y + yOffset), paint);
                    yOffset = yOffsetOriginal;
                }
            }
        }

        double getLineXOffset(double lineTotalAdvance) {
            if (double.IsInfinity(this._width)) {
                return 0;
            }

            if (this._paragraphStyle.textAlign == TextAlign.right) {
                return this._width - lineTotalAdvance;
            } else if (this._paragraphStyle.textAlign == TextAlign.center) {
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
}
