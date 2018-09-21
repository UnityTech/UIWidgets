using System;
using System.Collections.Generic;
using UIWidgets.ui.txt;
using UnityEngine;

namespace UIWidgets.ui
{
    
    public struct Vector2d
    {
        public double x;
        public double y;

        public Vector2d(double x = 0.0, double y = 0.0)
        {
            this.x = x;
            this.y = y;
        }
            
        public static Vector2d operator +(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.x + b.x, a.y + b.y);
        }

        public static Vector2d operator -(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.x - b.x, a.y - b.y);
        }
    }

    public class CodeUnitRun
    {
        public int lineNumber;
        public TextDirection direction;
        public IndexRange codeUnits;
        public FontMetrics fontMetrics;

        public CodeUnitRun(IndexRange cu, int line, FontMetrics fontMetrics, TextDirection direction)
        {
            this.lineNumber = line;
            this.codeUnits = cu;
            this.fontMetrics = fontMetrics;
            this.direction = direction;
        }
    }

    public class FontMetrics
    {
        public readonly double ascent;
        public readonly double descent;
        public readonly double? underlineThickness;
        public readonly double? underlinePosition;
        public readonly double? strikeoutPosition;
        public readonly double? fxHeight;

        public FontMetrics(double ascent, double descent,
            double? underlineThickness = null, double? underlinePosition = null, double? strikeoutPosition = null, 
            double? fxHeight = null)
        {
            this.ascent = ascent;
            this.descent = descent;
            this.underlineThickness = underlineThickness;
            this.underlinePosition = underlinePosition;
            this.strikeoutPosition = strikeoutPosition;
            this.fxHeight = fxHeight;
        }

        public static FontMetrics fromFont(Font font, double? height)
        {
            var ascent = font.ascent * (height??1.0);
            var descent = (font.lineHeight - font.ascent) * (height??1.0);
            double? fxHeight = null;
            font.RequestCharactersInTexture("x");
            CharacterInfo charInfo;
            if (font.GetCharacterInfo('x', out charInfo))
            {
                fxHeight = charInfo.glyphHeight;
            }
            return new FontMetrics(ascent, descent, fxHeight: fxHeight); 
        }
    }

    public struct IndexRange :IEquatable<IndexRange>
    {
        public int start, end;

        public IndexRange(int s, int e)
        {
            start = s;
            end = e;
        }

        int width()
        {
            return end - start;
        }

        void shift(int delta)
        {
            start += delta;
            end += delta;
        }

        public bool Equals(IndexRange other)
        {
            return start == other.start && end == other.end;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is IndexRange && Equals((IndexRange) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (start * 397) ^ end;
            }
        }

        public static bool operator ==(IndexRange left, IndexRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IndexRange left, IndexRange right)
        {
            return !left.Equals(right);
        }
    }

    public class PositionWithAffinity
    {
        public readonly int position;
        public readonly TextAffinity affinity;

        public PositionWithAffinity(int p, TextAffinity a)
        {
            position = p;
            affinity = a;
        }
    }
    
    public class Paragraph
    {
        struct Range<T>: IEquatable<Range<T>>
        {
            public Range(T start, T end)
            {
                this.start = start;
                this.end = end;
            }

            public bool Equals(Range<T> other)
            {
                return EqualityComparer<T>.Default.Equals(start, other.start) && EqualityComparer<T>.Default.Equals(end, other.end);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is Range<T> && Equals((Range<T>) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (EqualityComparer<T>.Default.GetHashCode(start) * 397) ^ EqualityComparer<T>.Default.GetHashCode(end);
                }
            }

            public static bool operator ==(Range<T> left, Range<T> right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Range<T> left, Range<T> right)
            {
                return !left.Equals(right);
            }

            public T start, end;
            
        }
        
        class LineRange
        {
            public LineRange(int start, int end, int endExcludingWhitespace, int endIncludingNewLine, bool hardBreak)
            {
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
        
        private static readonly Shader textShader;
        
        static Paragraph() {
    
            textShader = Shader.Find("UIWidgets/Text Shader");
            if (textShader == null) {
                throw new Exception("UIWidgets/Text Shader Lines not found");
            }
        }

        private bool _needsLayout = true;

        private string _text;
        
        private StyledRuns _runs;

        private ParagraphStyle _paragraphStyle;
        private List<LineRange> _lineRanges = new List<LineRange>();
        private List<double> _lineWidths = new List<double>();
        private List<double> _lineBaseLines = new List<double>();
        private Vector2d[] _characterPositions;
        private double _maxIntrinsicWidth;
        private double _minIntrinsicWidth;
        private double _alphabeticBaseline;
        private double _ideographicBaseline;
        private double[] _characterWidths; 
        private List<double> _lineHeights = new List<double>();
        private List<PaintRecord> _paintRecords = new List<PaintRecord>();
        private List<CodeUnitRun> _codeUnitRuns = new List<CodeUnitRun>();
        private bool _didExceedMaxLines;
      
        // private double _characterWidth;

        private double _width;

        public const char CHAR_NBSP = '\u00A0';
        private const double kDoubleDecorationSpacing = 3.0;

        public static bool isWordSpace(char ch)
        {
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
        
        public double height
        {
            get { return _lineHeights.Count == 0 ? 0 : _lineHeights[_lineHeights.Count - 1]; }
        }
        
        public double minIntrinsicWidth
        {
            get { return _minIntrinsicWidth; }
        }
        
        public double maxIntrinsicWidth
        {
            get { return _maxIntrinsicWidth; }
        }
        
        public double width
        {
            get { return _width; }
        }
        
        
        public double alphabeticBaseline
        {
            get { return _alphabeticBaseline; }
        }
        
        public double ideographicBaseline
        {
            get { return _ideographicBaseline; }
        }

        public bool didExceedMaxLines
        {
            get { return _didExceedMaxLines; }
        }

        public void paint(Canvas canvas, double x, double y)
        {
            var baseOffset = new Offset(x, y);
            foreach (var paintRecord in _paintRecords)
            {
                canvas.drawTextBlob(paintRecord.text, x, y);
                paintDecorations(canvas, paintRecord, baseOffset);
            }
        }
        
        public void layout(ParagraphConstraints constraints)
        {
            if (!_needsLayout && _width == constraints.width)
            {
                return;
            }
            
            _needsLayout = false;
            _width = Math.Floor(constraints.width);
            
            this.setup();
            computeLineBreak();

            var maxLines = _paragraphStyle.maxLines ?? 0;
            _didExceedMaxLines = !(maxLines == 0 || _lineRanges.Count <= maxLines);
            var lineLimits = maxLines == 0 ? _lineRanges.Count : Math.Min(maxLines, _lineRanges.Count);
            layoutLines(lineLimits);

            double maxWordWidth = 0;
            for (int lineNumber = 0; lineNumber < lineLimits; ++lineNumber)
            {
                var line = _lineRanges[lineNumber];
                var words = findWords(line.start, line.end);
                words.ForEach((word) =>
                {
                    Debug.Assert(word.start < word.end);
                    double wordWidth = _characterPositions[word.end - 1].x - _characterPositions[word.start].x +
                                       _characterWidths[word.end - 1];
                    if (wordWidth > maxWordWidth)
                    {
                        maxWordWidth = wordWidth;
                    }
                });

                if (_paragraphStyle.TextAlign == TextAlign.justify && !_lineRanges[lineNumber].hardBreak
                    && lineNumber != lineLimits - 1)
                {
                    justifyLine(lineNumber, words);
                } else if (line.endExcludingWhitespace > line.start)
                {
                    Debug.Assert(!isLineEndSpace(_text[line.endExcludingWhitespace - 1]));
                    var lineTotalAdvance = _characterPositions[line.endExcludingWhitespace - 1].x +
                                           _characterWidths[line.endExcludingWhitespace - 1];
                    double xOffset = getLineXOffset(lineTotalAdvance);
                    if (xOffset > 0 || xOffset < 0)
                    {
                        offsetCharacters(new Vector2d(xOffset, 0), 
                            _characterPositions, line.start, line.endExcludingWhitespace);
                    }
                }
                
            }
            
            computeWidthMetrics(maxWordWidth);
        }
        

        public void setText(string text, StyledRuns runs)
        {
            _text = text;
            _runs = runs;
            _needsLayout = true;
        }

        public void setParagraphStyle(ParagraphStyle style)
        {
            _needsLayout = true;
            _paragraphStyle = style;
        }

        public List<TextBox> getRectsForRange(int start, int end)
        {
            var lineBoxes = new SortedDictionary<int, List<TextBox>>();
            foreach (var run in _codeUnitRuns)
            {
                if (run.codeUnits.start >= end)
                {
                    break;
                }

                if (run.codeUnits.end <= start)
                {
                    continue;
                }

                var baseLine = _lineBaseLines[run.lineNumber];
                double top = baseLine - run.fontMetrics.ascent;
                double bottom = baseLine + run.fontMetrics.descent;

                // double left, right;
                var from = Math.Max(start, run.codeUnits.start);
                var to = Math.Min(end, run.codeUnits.end);
                if (from < to)
                {
                    List<TextBox> boxs;
                    if (!lineBoxes.TryGetValue(run.lineNumber, out boxs))
                    {
                        boxs = new List<TextBox>();
                        lineBoxes.Add(run.lineNumber, boxs);
                    }

                    double left = _characterPositions[from].x;
                    double right = _characterPositions[to - 1].x + _characterWidths[to - 1];
                    boxs.Add(TextBox.fromLTBD(left, top, right, bottom, run.direction));
                }
            }

            for (int lineNumber = 0; lineNumber < _lineRanges.Count; ++lineNumber)
            {
                var line = _lineRanges[lineNumber];
                if (line.start >= end)
                {
                    break;
                }

                if (line.endIncludingNewLine <= start)
                {
                    continue;
                }

                if (!lineBoxes.ContainsKey(lineNumber))
                {
                    if (line.end != line.endIncludingNewLine && line.end >= start && line.endIncludingNewLine <= end)
                    {
                        var x = _lineWidths[lineNumber];
                        var top = (lineNumber > 0) ? _lineHeights[lineNumber - 1] : 0;
                        var bottom = _lineHeights[lineNumber];
                        lineBoxes.Add(lineNumber, new List<TextBox>(){TextBox.fromLTBD(
                            x, top, x, bottom, TextDirection.ltr)});
                    }
                }
                
            }

            var result = new List<TextBox>();
            foreach (var keyValuePair in lineBoxes)
            {
                result.AddRange(keyValuePair.Value);
            }

            return result;
        }

        public PositionWithAffinity getGlyphPositionAtCoordinate(double dx, double dy)
        {
            if (_lineHeights.Count == 0)
            {
                return new PositionWithAffinity(0, TextAffinity.downstream);
            }

            int yIndex;
            for (yIndex = 0; yIndex < _lineHeights.Count - 1; ++yIndex)
            {
                if (dy < _lineHeights[yIndex])
                {
                    break;
                }
            }

            var line = _lineRanges[yIndex];
            if (line.start >= line.end)
            {
                return new PositionWithAffinity(line.start, TextAffinity.downstream);
            }

            int index;
            for (index = line.start; index < line.end; ++index)
            {
                if (dx < _characterPositions[index].x + _characterWidths[index])
                {
                    break;
                }
            }

            if (index >= line.end)
            {
                return new PositionWithAffinity(line.end, TextAffinity.upstream);
            }
            
            TextDirection direction = TextDirection.ltr;
            var codeUnit = _codeUnitRuns.Find((u) => u.codeUnits.start >= index && index < u.codeUnits.end);
            if (codeUnit != null)
            {
                direction = codeUnit.direction;
            }

            double glyphCenter = (_characterPositions[index].x + _characterPositions[index].x + _characterWidths[index]) / 2;
            if ((direction == TextDirection.ltr && dx < glyphCenter) || (direction == TextDirection.rtl && dx >= glyphCenter))
            {
                return new PositionWithAffinity(index, TextAffinity.downstream);
            } else
            {
                return new PositionWithAffinity(index, TextAffinity.upstream);
            }
        }

        public IndexRange getWordBoundary(int offset)
        {
            WordSeparate s = new WordSeparate(_text);
            return s.findWordRange(offset);
        }

        public static void offsetCharacters(Vector2d offset, Vector2d[] characterPos, int start, int end)
        {
            if (characterPos != null)
            {
                for (int i = start; i < characterPos.Length && i < end; ++i)
                {
                    characterPos[i] = characterPos[i] + offset;
                }
            }
        }
        

        private void computeWidthMetrics(double maxWordWidth)
        {
            _maxIntrinsicWidth = 0;
            double lineBlockWidth = 0;
            for (int i = 0; i < _lineWidths.Count; ++i)
            {
                var line = _lineRanges[i];
                lineBlockWidth += _lineWidths[i];
                if (line.hardBreak)
                {
                    _maxIntrinsicWidth = Math.Max(lineBlockWidth, _maxIntrinsicWidth);
                    lineBlockWidth = 0;
                }
            }

            if (_paragraphStyle.maxLines == 1 || (((_paragraphStyle.maxLines??0) == 0) &&
                    !string.IsNullOrEmpty(_paragraphStyle.ellipsis)))
            {
                _minIntrinsicWidth = _maxIntrinsicWidth;
            }
            else
            {
                _minIntrinsicWidth = Math.Min(maxWordWidth, _maxIntrinsicWidth);
            }
        }
        
        private void setup()
        {
            if (_characterPositions == null || _characterPositions.Length < _text.Length)
            {
                _characterPositions = new Vector2d[_text.Length];   
            }

            _lineHeights.Clear();
            _lineRanges.Clear();
            _lineWidths.Clear();
            _lineBaseLines.Clear();
            _paintRecords.Clear();
            _codeUnitRuns.Clear();
            _characterWidths = new double[_text.Length];
            for (int i = 0; i < _runs.size; ++i)
            {
                var run = _runs.getRun(i);
                if (run.start < run.end)
                {
                    var font = FontManager.instance.getOrCreate(run.style.fontFamily, run.style.UnityFontSize);
                    font.RequestCharactersInTexture(_text.Substring(run.start, run.end - run.start), 0, 
                        run.style.UnityFontStyle);
                } 
            }
        }

        private void layoutLines(int lineLimits)
        {
            double yOffset = 0;
            var runIndex = 0;
            double lastDescent = 0.0f;
            var linePaintRecords = new List<PaintRecord>();
            
            for (int lineNumber = 0; lineNumber < lineLimits; lineNumber++)
            {
                var line = _lineRanges[lineNumber];
                double maxAscent = 0.0f;
                double maxDescent = 0.0f;
                linePaintRecords.Clear();
                for (;;)
                {
                    var run = runIndex < _runs.size ? _runs.getRun(runIndex) : null;
                    if (run != null && run.start < run.end && run.start < line.end && run.end > line.start)
                    {
                        var font = FontManager.instance.getOrCreate(run.style.fontFamily, run.style.UnityFontSize);
                        var metrics = FontMetrics.fromFont(font, run.style.height);
                        var ascent = font.ascent * (run.style.height);
                        var descent = (font.lineHeight - font.ascent) * (run.style.height);
                        if (metrics.ascent > maxAscent)
                        {
                            maxAscent = metrics.ascent;
                        }
                        if (metrics.descent > maxDescent)
                        {
                            maxDescent = metrics.descent;
                        }

                        int start = Math.Max(run.start, line.start);
                        int end = Math.Min(run.end, line.end);
                        var width = _characterPositions[end - 1].x + _characterWidths[end - 1] -
                                    _characterPositions[start].x;
                        if (end > start)
                        {
                            var bounds = Rect.fromLTWH(0, -ascent,
                                _characterPositions[end - 1].x + _characterWidths[end - 1] -
                                _characterPositions[start].x,
                                descent);
                            
                            linePaintRecords.Add(new PaintRecord(run.style, new Offset(_characterPositions[start].x, yOffset)
                                , new TextBlob(
                                    _text, start, end, _characterPositions, run.style, bounds), metrics, 
                                lineNumber, width));
                            _codeUnitRuns.Add(new CodeUnitRun(
                                new IndexRange(start, end), lineNumber, metrics, TextDirection.ltr));
                        }
                    }

                    if (runIndex + 1 >= _runs.size)
                    {
                        break;
                    }

                    if (run.end < line.end)
                    {
                        runIndex++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (lineNumber == 0)
                {
                    _alphabeticBaseline = maxAscent;
                    _ideographicBaseline = maxAscent; // todo Properly implement ideographic_baseline
                }
                lastDescent = maxDescent;
                yOffset = Utils.PixelCorrectRound(yOffset + maxAscent + lastDescent);
                foreach (var record in linePaintRecords)
                {
                    record.offset = new Offset(record.offset.dx, yOffset);
                }
                _paintRecords.AddRange(linePaintRecords);
                
                for (var charIndex = line.start; charIndex < line.end; charIndex++)
                {
                    _characterPositions[charIndex].y = yOffset;
                }
               
                _lineHeights.Add((_lineHeights.Count == 0 ? 0 : _lineHeights[_lineHeights.Count - 1]) + 
                                 Math.Round(maxAscent + maxDescent));
                _lineBaseLines.Add(yOffset);
            }
        }
        
        private void computeLineBreak()
        {
            var newLinePositions = new List<int>();
            for (var i = 0; i < _text.Length; i++)
            {
                if (_text[i] == '\n')
                {
                    newLinePositions.Add(i);
                }
            }
            newLinePositions.Add(_text.Length);

            var lineBreaker = new LineBreaker();
            lineBreaker.setup(_text, _runs, _width, _characterPositions, _characterWidths);
            
            for (var newlineIndex = 0; newlineIndex < newLinePositions.Count; ++newlineIndex)
            {
                var blockStart = newlineIndex > 0 ? newLinePositions[newlineIndex - 1] + 1 : 0;
                var blockEnd = newLinePositions[newlineIndex];
                var blockSize = blockEnd - blockStart;
                if (blockSize == 0)
                {
                    _lineRanges.Add(new LineRange(blockStart, blockEnd, blockEnd, blockEnd + 1, true));
                    _lineWidths.Add(0);
                    continue;
                }

                lineBreaker.doBreak(blockStart, blockEnd);
                var lines = lineBreaker.getLines();
                for (int i = 0; i < lines.Count; ++i)
                {
                    var line = lines[i];
                    var end = i + 1 < lines.Count ? lines[i + 1].start : blockEnd;

                    var nonSpaceEnd = end;
                    while (nonSpaceEnd > line.start && isLineEndSpace(_text[nonSpaceEnd - 1]))
                    {
                        nonSpaceEnd--;
                    }
                    
                    _lineRanges.Add(new LineRange(line.start, end, nonSpaceEnd, end + 1, end == blockEnd));
                    _lineWidths.Add(line.width);
                }
            }

            return;

        }

        private double getLineXOffset(double lineTotalAdvance) {
            if (double.IsInfinity(_width))
            {
                return 0;
            }
                
            var align = _paragraphStyle.TextAlign;
            if (align == TextAlign.right) {
                return _width - lineTotalAdvance;
            } else if (align == TextAlign.center) {
                return Utils.PixelCorrectRound((_width - lineTotalAdvance) / 2);
            } else {
                return 0;
            }
        }

        private void justifyLine(int lineNumber, List<Range<int>> words)
        {
            if (words.Count <= 1)
            {
                return;
            }

            var line = _lineRanges[lineNumber];
            Debug.Assert(!isLineEndSpace(_text[line.endExcludingWhitespace - 1]));
            var lineTotalAdvance = _characterPositions[line.endExcludingWhitespace - 1].x +
                                   _characterWidths[line.endExcludingWhitespace - 1];
            double gapWidth = (_width - lineTotalAdvance) / (words.Count - 1);
    
            double justifyOffset = 0.0;
            foreach (var word in words)
            {
                offsetCharacters(new Vector2d(justifyOffset), 
                    _characterPositions, word.start, word.end);
                justifyOffset += gapWidth;
                justifyOffset = Utils.PixelCorrectRound(justifyOffset);
            }
        }
        
        private List<Range<int>> findWords(int start, int end)
        {
            var inWord = false;
            int wordStart = 0;
            List<Range<int>> words = new List<Range<int>>();
            for (int i = start; i < end; ++i) {
                bool isSpace = isWordSpace(_text[i]);
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

        private void paintDecorations(Canvas canvas, PaintRecord record, Offset baseOffset)
        {
            if (record.style.decoration == null || record.style.decoration == TextDecoration.none)
            {
                return;
            }

            var paint = new Paint();
            if (record.style.decorationColor == null)
            {
                paint.color = record.style.color;
            }
            else
            {
                paint.color = record.style.decorationColor;
            }
            
            
            var width = record.runWidth;
            var metrics = record.metrics;
            double underLineThickness = metrics.underlineThickness ?? (record.style.fontSize / 14.0);
            paint.strokeWidth = underLineThickness;
            var recordOffset = baseOffset + record.offset;
            var x = recordOffset.dx;
            var y = recordOffset.dy;
            
            int decorationCount = 1;
            switch (record.style.decorationStyle)
            {
                   case TextDecorationStyle.doubleLine:
                       decorationCount = 2;
                       break;
            }


            var decoration = record.style.decoration;
            for (int i = 0; i < decorationCount; i++)
            {
                double yOffset = i * underLineThickness * kDoubleDecorationSpacing;
                double yOffsetOriginal = yOffset;
                if (decoration != null && decoration.contains(TextDecoration.underline))
                {
                    // underline
                    yOffset += metrics.underlinePosition ?? underLineThickness;
                    canvas.drawLine(new Offset(x, y + yOffset), new Offset(x + width, y + yOffset), paint);
                    yOffset = yOffsetOriginal;
                }

                if (decoration != null && decoration.contains(TextDecoration.overline))
                {
                    yOffset -= metrics.ascent;
                    canvas.drawLine(new Offset(x, y + yOffset), new Offset(x + width, y + yOffset), paint);
                    yOffset = yOffsetOriginal;
                }

                if (decoration != null && decoration.contains(TextDecoration.lineThrough))
                {
                    yOffset += (decorationCount - 1.0) * underLineThickness * kDoubleDecorationSpacing / -2.0;
                    yOffset += metrics.strikeoutPosition ?? (metrics.fxHeight??0) / -2.0;
                    canvas.drawLine(new Offset(x, y + yOffset), new Offset(x + width, y + yOffset), paint);
                    yOffset = yOffsetOriginal;
                }
            }
        }
        
    }
}