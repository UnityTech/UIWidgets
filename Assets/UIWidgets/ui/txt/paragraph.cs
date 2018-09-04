using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.UIElements;

namespace UIWidgets.ui
{
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

        class LayoutContext
        {
            public int width;
            public int index;
            public Vector2 offset;
            public TextStyle style;
            public Font font;
            public int wordStart;
            public int lineStart;
            public int prevWordEnd;
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
        private Vector2[] _characterPositions;
        private double _maxIntrinsicWidth;
        private double _minIntrinsicWidth;
        private double _alphabeticBaseline;
        private double _ideographicBaseline;
        private Font[] _styleRunFonts;
        private float[] _characterWidths; 
        private List<double> _lineHeights = new List<double>();
        private LayoutContext context;
        private bool _didExceedMaxLines;
      
        // private double _characterWidth;

        private double _width;

        public const char CHAR_NBSP = '\u00A0';
        public static bool isWordSpace(char ch)
        {
            return ch == ' ' || ch == CHAR_NBSP;
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
            get { return 0.0; }
        }
        
        
        public double ideographicBaseline
        {
            get { return 0.0; }
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

        public static void offsetCharacters(Vector2 offset, Vector2[] characterPos, int start, int end)
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
            _characterPositions = new Vector2[_text.Length];
            _lineHeights.Clear();
            _lineRanges.Clear();
            _lineWidths.Clear();
            _characterWidths = new float[_text.Length];
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
                yOffset += maxAscent + lastDescent;
                for (var charIndex = line.start; charIndex < line.end; charIndex++)
                {
                    _characterPositions[charIndex].y = (float)yOffset;
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


            int runIndex = 0;
            StyledRuns.Run lastRun = null;
            var lineBreaker = new LineBreaker();
            lineBreaker.setup(_text, _runs, _styleRunFonts, (float)_width, _characterPositions, _characterWidths);
            
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

                    var nonWhiteSpace = end - 1;
                    while (nonWhiteSpace >= line.start && _text[nonWhiteSpace] == ' ' || _text[nonWhiteSpace] == '\t')
                    {
                        nonWhiteSpace--;
                    }
                    
                    _lineRanges.Add(new LineRange(line.start, end, nonWhiteSpace, end + 1, end == blockEnd));
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
            Vector3 offset = new Vector3((float)x, (float)y, 0);
            font.RequestCharactersInTexture(_text.Substring(run.start, run.end - run.start), run.style.UnityFontSize, run.style.UnityFontStyle);
            for (int charIndex = run.start; charIndex < run.end; ++charIndex)
            {
                CharacterInfo charInfo;
                var result = font.GetCharacterInfo(_text[charIndex], out charInfo, run.style.UnityFontSize, run.style.UnityFontStyle);
                var position = _characterPositions[charIndex];
                 
                vertices[4 * charIndex + 0] = offset + new Vector3(position.x + charInfo.minX, position.y - charInfo.maxY, 0);
                vertices[4 * charIndex + 1] = offset + new Vector3(position.x + charInfo.maxX, position.y - charInfo.maxY, 0);
                vertices[4 * charIndex + 2] = offset + new Vector3(position.x + charInfo.maxX, position.y - charInfo.minY, 0);
                vertices[4 * charIndex + 3] = offset + new Vector3(position.x + charInfo.minX, position.y - charInfo.minY, 0);

                if (_text[charIndex] != ' ' && _text[charIndex] != '\t' && _text[charIndex] != '\n')
                {
                    uv[4 * charIndex + 0] = charInfo.uvTopLeft;
                    uv[4 * charIndex + 1] = charInfo.uvTopRight;
                    uv[4 * charIndex + 2] = charInfo.uvBottomRight;
                    uv[4 * charIndex + 3] = charInfo.uvBottomLeft;
                   
                }
                else
                {
                    uv[4 * charIndex + 0] = Vector2.zero;
                    uv[4 * charIndex + 1] = Vector2.zero;
                    uv[4 * charIndex + 2] = Vector2.zero;
                    uv[4 * charIndex + 3] = Vector2.zero;
                    
                }
               
                triangles[6 * charIndex + 0] = 4 * charIndex + 0;
                triangles[6 * charIndex + 1] = 4 * charIndex + 1;
                triangles[6 * charIndex + 2] = 4 * charIndex + 2;

                triangles[6 * charIndex + 3] = 4 * charIndex + 0;
                triangles[6 * charIndex + 4] = 4 * charIndex + 2;
                triangles[6 * charIndex + 5] = 4 * charIndex + 3;
            }

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