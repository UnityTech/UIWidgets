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

    public class CodeUnitRun {
        public int lineNumber;
        public TextDirection direction;
        public IndexRange codeUnits;
        public FontMetrics fontMetrics;

        public CodeUnitRun(IndexRange cu, int line, FontMetrics fontMetrics, TextDirection direction) {
            this.lineNumber = line;
            this.codeUnits = cu;
            this.fontMetrics = fontMetrics;
            this.direction = direction;
        }
    }

    public class FontMetrics {
        public readonly double ascent;
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
            var ascent = font.ascent * (height ?? 1.0) * fontSize / font.fontSize;
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

    public class PositionWithAffinity {
        public readonly int position;
        public readonly TextAffinity affinity;

        public PositionWithAffinity(int p, TextAffinity a) {
            this.position = p;
            this.affinity = a;
        }
    }

    public class Paragraph {
        struct Range<T> : IEquatable<Range<T>> {
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
        Vector2d[] _characterPositions;
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

        public const char CHAR_NBSP = '\u00A0';
        const double kDoubleDecorationSpacing = 3.0;

        public static bool isWordSpace(char ch) {
            return ch == ' ' || ch == CHAR_NBSP;
        }

        // This function determines whether a character is a space that disappears at end of line.
        // It is the Unicode set: [[:General_Category=Space_Separator:]-[:Line_Break=Glue:]],
        // plus '\n'.
        // Note: all such characters are in the BMP, so it's ok to use code units for this.
        public static bool isLineEndSpace(char c) {
            return c == '\n' || c == ' ' || c == 0x1680 || (0x2000 <= c && c <= 0x200A && c != 0x2007) ||
                   c == 0x205F || c == 0x3000;
        }

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
                    color = paintRecord.style.color,
                };
                canvas.drawTextBlob(paintRecord.text, offset, paint);
                this.paintDecorations(canvas, paintRecord, offset);
            }
        }

        public void layout(ParagraphConstraints constraints) {
            if (!this._needsLayout && this._width == constraints.width) {
                return;
            }

            this._needsLayout = false;
            this._width = Math.Floor(constraints.width);

            this.setup();
            this.computeLineBreak();

            var maxLines = this._paragraphStyle.maxLines ?? 0;
            this._didExceedMaxLines = !(maxLines == 0 || this._lineRanges.Count <= maxLines);
            var lineLimits = maxLines == 0 ? this._lineRanges.Count : Math.Min(maxLines, this._lineRanges.Count);
            this.layoutLines(lineLimits);

            double maxWordWidth = 0;
            for (int lineNumber = 0; lineNumber < lineLimits; ++lineNumber) {
                var line = this._lineRanges[lineNumber];
                var words = this.findWords(line.start, line.end);
                words.ForEach((word) => {
                    Debug.Assert(word.start < word.end);
                    double wordWidth = this._characterPositions[word.end - 1].x -
                                       this._characterPositions[word.start].x + this._characterWidths[word.end - 1];
                    if (wordWidth > maxWordWidth) {
                        maxWordWidth = wordWidth;
                    }
                });

                if (this._paragraphStyle.TextAlign == TextAlign.justify && !this._lineRanges[lineNumber].hardBreak
                                                                        && lineNumber != lineLimits - 1) {
                    this.justifyLine(lineNumber, words);
                } else if (line.endExcludingWhitespace > line.start) {
                    Debug.Assert(!isLineEndSpace(this._text[line.endExcludingWhitespace - 1]));
                    var lineTotalAdvance = this._characterPositions[line.endExcludingWhitespace - 1].x +
                                           this._characterWidths[line.endExcludingWhitespace - 1];
                    double xOffset = this.getLineXOffset(lineTotalAdvance);
                    if (xOffset > 0 || xOffset < 0) {
                        offsetCharacters(new Vector2d(xOffset, 0), this._characterPositions, line.start,
                            line.endExcludingWhitespace);
                    }
                }
            }

            this.computeWidthMetrics(maxWordWidth);
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

        void computeWidthMetrics(double maxWordWidth) {
            this._maxIntrinsicWidth = 0;
            double lineBlockWidth = 0;
            for (int i = 0; i < this._lineWidths.Count; ++i) {
                var line = this._lineRanges[i];
                lineBlockWidth += this._lineWidths[i];
                if (line.hardBreak) {
                    this._maxIntrinsicWidth = Math.Max(lineBlockWidth, this._maxIntrinsicWidth);
                    lineBlockWidth = 0;
                }
            }

            if (this._paragraphStyle.maxLines == 1 || (((this._paragraphStyle.maxLines ?? 0) == 0) &&
                                                       !string.IsNullOrEmpty(this._paragraphStyle.ellipsis))) {
                this._minIntrinsicWidth = this._maxIntrinsicWidth;
            } else {
                this._minIntrinsicWidth = Math.Min(maxWordWidth, this._maxIntrinsicWidth);
            }
        }

        void setup() {
            if (this._characterPositions == null || this._characterPositions.Length < this._text.Length) {
                this._characterPositions = new Vector2d[this._text.Length];
            }

            this._lineHeights.Clear();
            this._lineRanges.Clear();
            this._lineWidths.Clear();
            this._lineBaseLines.Clear();
            this._paintRecords.Clear();
            this._codeUnitRuns.Clear();
            this._characterWidths = new double[this._text.Length];
            for (int i = 0; i < this._runs.size; ++i) {
                var run = this._runs.getRun(i);
                if (run.start < run.end) {
                    var font = FontManager.instance.getOrCreate(run.style.fontFamily).font;
                    font.RequestCharactersInTexture(this._text.Substring(run.start, run.end - run.start),
                        run.style.UnityFontSize,
                        run.style.UnityFontStyle);
                }
            }
        }

        void layoutLines(int lineLimits) {
            double yOffset = 0;
            var runIndex = 0;
            double lastDescent = 0.0f;
            var linePaintRecords = new List<PaintRecord>();

            for (int lineNumber = 0; lineNumber < lineLimits; lineNumber++) {
                var line = this._lineRanges[lineNumber];
                double maxAscent = 0.0f;
                double maxDescent = 0.0f;
                linePaintRecords.Clear();
                for (;;) {
                    var run = runIndex < this._runs.size ? this._runs.getRun(runIndex) : null;
                    if (run != null && run.start < run.end && run.start < line.end && run.end > line.start) {
                        var font = FontManager.instance.getOrCreate(run.style.fontFamily).font;
                        var metrics = FontMetrics.fromFont(font, run.style.UnityFontSize, run.style.height);
                        if (metrics.ascent > maxAscent) {
                            maxAscent = metrics.ascent;
                        }
                        if (metrics.descent > maxDescent) {
                            maxDescent = metrics.descent;
                        }

                        int start = Math.Max(run.start, line.start);
                        int end = Math.Min(run.end, line.end);
                        var width = this._characterPositions[end - 1].x + this._characterWidths[end - 1] -
                                    this._characterPositions[start].x;
                        if (end > start) {
                            var bounds = Rect.fromLTWH(0, -metrics.ascent,
                                this._characterPositions[end - 1].x + this._characterWidths[end - 1] -
                                this._characterPositions[start].x,
                                metrics.ascent + metrics.descent);

                            linePaintRecords.Add(new PaintRecord(run.style,
                                new Offset(this._characterPositions[start].x, yOffset)
                                , new TextBlob(this._text, start, end, this._characterPositions, run.style, bounds),
                                metrics,
                                lineNumber, width));
                            this._codeUnitRuns.Add(new CodeUnitRun(
                                new IndexRange(start, end), lineNumber, metrics, TextDirection.ltr));
                        }
                    }

                    if (runIndex + 1 >= this._runs.size) {
                        break;
                    }

                    if (run.end < line.end) {
                        runIndex++;
                    } else {
                        break;
                    }
                }

                if (lineNumber == 0) {
                    this._alphabeticBaseline = maxAscent;
                    this._ideographicBaseline = maxAscent; // todo Properly implement ideographic_baseline
                }

                yOffset = Utils.PixelCorrectRound(yOffset + maxAscent + lastDescent);
                foreach (var record in linePaintRecords) {
                    record.offset = new Offset(record.offset.dx, yOffset);
                }
                this._paintRecords.AddRange(linePaintRecords);

                for (var charIndex = line.start; charIndex < line.end; charIndex++) {
                    this._characterPositions[charIndex].y = yOffset;
                }

                this._lineHeights.Add(
                    (this._lineHeights.Count == 0 ? 0 : this._lineHeights[this._lineHeights.Count - 1]) +
                    Math.Round(maxAscent + maxDescent));
                this._lineBaseLines.Add(yOffset);
                lastDescent = maxDescent;
            }
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
            lineBreaker.setup(this._text, this._runs, this._width, this._characterPositions, this._characterWidths);

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

                lineBreaker.doBreak(blockStart, blockEnd);
                var lines = lineBreaker.getLines();
                for (int i = 0; i < lines.Count; ++i) {
                    var line = lines[i];
                    var end = i + 1 < lines.Count ? lines[i + 1].start : blockEnd;

                    var nonSpaceEnd = end;
                    while (nonSpaceEnd > line.start && isLineEndSpace(this._text[nonSpaceEnd - 1])) {
                        nonSpaceEnd--;
                    }

                    this._lineRanges.Add(new LineRange(line.start, end, nonSpaceEnd,
                        end == blockEnd && end < this._text.Length ? end + 1 : end, end == blockEnd));
                    this._lineWidths.Add(line.width);
                }
            }

            return;
        }

        double getLineXOffset(double lineTotalAdvance) {
            if (double.IsInfinity(this._width)) {
                return 0;
            }

            var align = this._paragraphStyle.TextAlign;
            if (align == TextAlign.right) {
                return this._width - lineTotalAdvance;
            } else if (align == TextAlign.center) {
                return Utils.PixelCorrectRound((this._width - lineTotalAdvance) / 2);
            } else {
                return 0;
            }
        }

        void justifyLine(int lineNumber, List<Range<int>> words) {
            if (words.Count <= 1) {
                return;
            }

            var line = this._lineRanges[lineNumber];
            Debug.Assert(!isLineEndSpace(this._text[line.endExcludingWhitespace - 1]));
            var lineTotalAdvance = this._characterPositions[line.endExcludingWhitespace - 1].x +
                                   this._characterWidths[line.endExcludingWhitespace - 1];
            double gapWidth = (this._width - lineTotalAdvance) / (words.Count - 1);

            double justifyOffset = 0.0;
            foreach (var word in words) {
                offsetCharacters(new Vector2d(justifyOffset), this._characterPositions, word.start, word.end);
                justifyOffset += gapWidth;
                justifyOffset = Utils.PixelCorrectRound(justifyOffset);
            }
        }

        List<Range<int>> findWords(int start, int end) {
            var inWord = false;
            int wordStart = 0;
            List<Range<int>> words = new List<Range<int>>();
            for (int i = start; i < end; ++i) {
                bool isSpace = isWordSpace(this._text[i]);
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

        static bool _isUtf16Surrogate(int value) {
            return (value & 0xF800) == 0xD800;
        }
    }
}
