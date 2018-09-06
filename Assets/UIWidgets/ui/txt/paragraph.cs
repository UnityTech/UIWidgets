using System;
using System.Collections.Generic;
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
        private Vector2d[] _characterPositions;
        private double _maxIntrinsicWidth;
        private double _minIntrinsicWidth;
        private double _alphabeticBaseline;
        private double _ideographicBaseline;
        private Font[] _styleRunFonts;
        private double[] _characterWidths; 
        private List<double> _lineHeights = new List<double>();
        private bool _didExceedMaxLines;
      
        // private double _characterWidth;

        private double _width;

        public const char CHAR_NBSP = '\u00A0';
        public static bool isWordSpace(char ch)
        {
            return ch == ' ' || ch == CHAR_NBSP;
        }
        
        // This function determines whether a character is a space that disappears at end of line.
        // It is the Unicode set: [[:General_Category=Space_Separator:]-[:Line_Break=Glue:]],
        // plus '\n'.
        // Note: all such characters are in the BMP, so it's ok to use code units for this.
        static bool isLineEndSpace(char c) {
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
            for (int runIndex = 0; runIndex < _runs.size; ++runIndex)
            {
                var run = _runs.getRun(runIndex);
                if (run.start < run.end)
                {
                    var font = _styleRunFonts[runIndex];
                    var mesh = generateMesh(x, y, font, run);
                    canvas.drawMesh(mesh, font.material);
                }
     
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
            _didExceedMaxLines = maxLines == 0 || _lineRanges.Count <= maxLines;
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
            _styleRunFonts = null;
        }

        public void setParagraphStyle(ParagraphStyle style)
        {
            _needsLayout = true;
            _paragraphStyle = style;
            _styleRunFonts = null;
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
            _characterWidths = new double[_text.Length];
            if (_styleRunFonts == null)
            {
                _styleRunFonts = new Font[_runs.size];
                for (int i = 0; i < _styleRunFonts.Length; ++i)
                {
                    var run = _runs.getRun(i);
                    if (run.start < run.end)
                    {
                        _styleRunFonts[i] =  Font.CreateDynamicFontFromOSFont(run.style.safeFontFamily,
                            run.style.UnityFontSize);
                        _styleRunFonts[i].material.shader = textShader;
                        _styleRunFonts[i].RequestCharactersInTexture(_text.Substring(run.start, run.end - run.start), 0, 
                            run.style.UnityFontStyle);
                    }
                }
            }
        }

        private void layoutLines(int lineLimits)
        {
            double yOffset = 0;
            var runIndex = 0;
            double lastDescent = 0.0f;
            for (int lineNumber = 0; lineNumber < lineLimits; lineNumber++)
            {
                var line = _lineRanges[lineNumber];
                double maxAscent = 0.0f;
                double maxDescent = 0.0f;

                for (;;)
                {
                    var run = _runs.getRun(runIndex);
                    if (run.start < run.end && run.start < line.end && run.end > line.start)
                    {
                        var font = _styleRunFonts[runIndex];
                        var ascent = font.ascent * (run.style.height??1.0);
                        var descent = (font.lineHeight - font.ascent) * (run.style.height??1.0);
                        if (ascent > maxAscent)
                        {
                            maxAscent = ascent;
                        }
                        if (descent > maxDescent)
                        {
                            maxDescent = descent;
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
                for (var charIndex = line.start; charIndex < line.end; charIndex++)
                {
                    _characterPositions[charIndex].y = yOffset;
                }
               
                _lineHeights.Add((_lineHeights.Count == 0 ? 0 : _lineHeights[_lineHeights.Count - 1]) + 
                                 Math.Round(maxAscent + maxDescent));
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
            lineBreaker.setup(_text, _runs, _styleRunFonts, _width, _characterPositions, _characterWidths);
            
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

        private Mesh generateMesh(double x, double y, Font font, StyledRuns.Run run)
        {
            var vertices = new Vector3[_text.Length * 4];
            var triangles = new int[_text.Length * 6];
            var uv = new Vector2[_text.Length * 4];
            Vector3 offset = new Vector3((float)Utils.PixelCorrectRound(x), (float)Utils.PixelCorrectRound(y), 0);
            font.RequestCharactersInTexture(_text.Substring(run.start, run.end - run.start), run.style.UnityFontSize, run.style.UnityFontStyle);
            for (int charIndex = run.start; charIndex < run.end; ++charIndex)
            {
                CharacterInfo charInfo = new CharacterInfo();
                if (_text[charIndex] != '\n' && _text[charIndex] != '\t')
                {
                    font.GetCharacterInfo(_text[charIndex], out charInfo, run.style.UnityFontSize, run.style.UnityFontStyle);
                    var position = _characterPositions[charIndex];
                    vertices[4 * charIndex + 0] = offset + new Vector3((float)(position.x + charInfo.minX), 
                                                      (float)(position.y - charInfo.maxY), 0);
                    vertices[4 * charIndex + 1] = offset + new Vector3((float)(position.x + charInfo.maxX), 
                                                      (float)(position.y - charInfo.maxY), 0);
                    vertices[4 * charIndex + 2] = offset + new Vector3(
                                                      (float)(position.x + charInfo.maxX), (float)(position.y - charInfo.minY), 0);
                    vertices[4 * charIndex + 3] = offset + new Vector3(
                                                      (float)(position.x + charInfo.minX), (float)(position.y - charInfo.minY), 0);
                }
                else
                {
                    vertices[4 * charIndex + 0] = vertices[4 * charIndex + 1] =
                        vertices[4 * charIndex + 2] = vertices[4 * charIndex + 3] = offset;
                } 

                if (isWordSpace(_text[charIndex]) || isLineEndSpace(_text[charIndex]) || _text[charIndex] == '\t')
                {                    
                    uv[4 * charIndex + 0] = Vector2.zero;
                    uv[4 * charIndex + 1] = Vector2.zero;
                    uv[4 * charIndex + 2] = Vector2.zero;
                    uv[4 * charIndex + 3] = Vector2.zero;
                } else
                {
                    uv[4 * charIndex + 0] = charInfo.uvTopLeft;
                    uv[4 * charIndex + 1] = charInfo.uvTopRight;
                    uv[4 * charIndex + 2] = charInfo.uvBottomRight;
                    uv[4 * charIndex + 3] = charInfo.uvBottomLeft;
                }
               
                triangles[6 * charIndex + 0] = 4 * charIndex + 0;
                triangles[6 * charIndex + 1] = 4 * charIndex + 1;
                triangles[6 * charIndex + 2] = 4 * charIndex + 2;

                triangles[6 * charIndex + 3] = 4 * charIndex + 0;
                triangles[6 * charIndex + 4] = 4 * charIndex + 2;
                triangles[6 * charIndex + 5] = 4 * charIndex + 3;
            }

//            for (var i = 0; i < vertices.Length; i++)
//            {
//                vertices[i].x = (float)Math.Round(vertices[i].x);
//                vertices[i].y = (float)Math.Round(vertices[i].y);
//            }

            var mesh = new Mesh()
            {
                vertices = vertices,
                triangles = triangles,
                uv = uv
            };
            var colors = new UnityEngine.Color[vertices.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = run.style.UnityColor;
            }

            mesh.colors = colors;
 
            return mesh;
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
        
    }
}