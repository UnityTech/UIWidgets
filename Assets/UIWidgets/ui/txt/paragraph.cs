using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIWidgets.ui
{
    public class Paragraph
    {
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
        
        private bool _needsLayout = true;

        private string _text;
        
        private StyledRuns _runs;

        private ParagraphStyle _paragraphStyle;
        private List<LineRange> _lineRanges = new List<LineRange>();
        private List<double> _lineWidths = new List<double>();
        private Vector2[] _characterPositions;
        private CharacterInfo[] _characterInfos;
        private Font[] _fonts;
        
        private float[] _characterWidths; 
        private float[] _characterSize; 
        private LayoutContext context;
      
        // private double _characterWidth;

        private double _width;
        // mesh
        // 
        public double height
        {
            get { return 0.0; }
        }
        
        public double minIntrinsicWidth
        {
            get { return 0.0; }
        }
        
        public double maxIntrinsicWidth
        {
            get { return 0.0; }
        }
        
        public double width
        {
            get { return 0.0; }
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
            get { return false; }
        }

        public void paint(Canvas canvas, double x, double y)
        {
            var mesh = generateMesh();
            
        }

        public void layout(ParagraphConstraints constraints)
        {
//            if (!_needsLayout && _width == constraints.width)
//            {
//                return;
//            }
            _needsLayout = false;
            _width = Math.Floor(constraints.width);
            
            this.setup();

            computeLineBreak();
            layoutLines();

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
        
        private Offset insertCharacter(char c, LayoutContext context)
        {
            CharacterInfo charInfo;
            context.font.GetCharacterInfo(c, out charInfo,
                context.style.UnityFontSize, context.style.UnityFontStyle);
            _characterInfos[context.index] = charInfo;
            _characterPositions[context.index].x = context.offset.x;
           //  _characterPositions[context.index].y = context.offset.y;
        
            if (context.offset.x + charInfo.advance > _width)
            {
                wordWrap(context);
            }
            return null;
        }

        private void wordWrap(LayoutContext context)
        {
            if (context.wordStart == context.lineStart)
            {
                context.wordStart = context.index;
            }
        }
        
       
        
        private void setup()
        {
            _characterPositions = new Vector2[_text.Length];
            _characterInfos = new CharacterInfo[_text.Length];
            _characterSize = new float[_text.Length];
            _characterWidths = new float[_text.Length];
            _fonts = new Font[_text.Length];
            
            
//            for (var i = 0; i < _runs.size; i++)
//            {
//                var run = _runs.getRun(i);
//                // run.font.RequestCharactersInTexture(_text.Substring(run.start, run.end - run.start), run.font.fontSize, run.) ;
//            }
        }

//        private void layoutBlock(int start, int end, ref int  styleIndex)
//        {
//                
//        }

        private void layoutLines()
        {
            double yOffset = 0;
            var runIndex = 0;

            double lastDescent = 0.0f;
            for (int i = 0; i < _lineRanges.Count; i++)
            {
                var line = _lineRanges[i];
                double maxAscent = 0.0f;
                double maxDescent = 0.0f;
                for (;runIndex < _runs.size;runIndex++)
                {
                    var run = _runs.getRun(runIndex);
                    var ascent = run.font.ascent * (run.style.height??1.0);
                    var descent = (run.font.lineHeight - run.font.ascent) * (run.style.height??1.0);
                    if (ascent > maxAscent)
                    {
                        maxAscent = ascent;
                    }
                    if (descent > maxDescent)
                    {
                        maxDescent = descent;
                    }
                    if (runIndex + 1 < _runs.size)
                    {
                        var nextRun = _runs.getRun(runIndex + 1);
                        if (nextRun.start >= line.end)
                        {
                            break;
                        }
                    }
                }
                
                lastDescent = maxDescent;
                yOffset += maxAscent + lastDescent;
                for (var charIndex = line.start; charIndex < line.end; charIndex++)
                {
                    _characterPositions[charIndex].y = (float)yOffset;
                }
            }
        }
        
        private void computeLineBreak()
        {
            _lineRanges.Clear();
            _lineWidths.Clear();
            
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
            lineBreaker.setup(_text, _runs, (float)_width, _characterPositions, _characterWidths);
            
            for (var newlineIndex = 0; newlineIndex < newLinePositions.Count; ++newlineIndex)
            {
                var blockStart = newlineIndex > 0 ? newLinePositions[newlineIndex - 1] + 1 : 0;
                var blockEnd = newLinePositions[newlineIndex];
                var blockSize = blockEnd - blockStart;
                if (blockSize == 0)
                {
                    _lineRanges.Add(new LineRange(blockStart, blockEnd, blockEnd, blockEnd + 1, true));
                    _lineWidths.Add(0);
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

        private Mesh generateMesh()
        {
            var vertices = new Vector3[_text.Length * 4];
            var triangles = new int[_text.Length * 6];
            var uv = new Vector2[_text.Length * 4];
            Vector3 pos = Vector3.zero;

            int runIndex = 0;
            for (int charIndex = 0; charIndex < _text.Length; charIndex++)
            {
                var run = _runs.getRun(runIndex);
                while ((run == null || run.end <= charIndex || charIndex < run.start) &&
                       charIndex + 1 < _runs.size)
                {
                    charIndex++;
                    run = _runs.getRun(charIndex);
                }

                if (run.start == charIndex)
                {
                    run.font.RequestCharactersInTexture(_text.Substring(run.start, run.end - run.start), run.style.UnityFontSize, run.style.UnityFontStyle);
                }

                CharacterInfo charInfo;
                var result = run.font.GetCharacterInfo(_text[charIndex], out charInfo);
                var position = _characterPositions[charIndex];
                vertices[4 * charIndex + 0] = pos + new Vector3(charInfo.minX, position.y - charInfo.maxY, 0);
                vertices[4 * charIndex + 1] = pos + new Vector3(charInfo.maxX, position.y - charInfo.maxY, 0);
                vertices[4 * charIndex + 2] = pos + new Vector3(charInfo.maxX, position.y + charInfo.minY, 0);
                vertices[4 * charIndex + 3] = pos + new Vector3(charInfo.minX, position.y + charInfo.minY, 0);
                
                uv[4 * charIndex + 0] = charInfo.uvTopLeft;
                uv[4 * charIndex + 1] = charInfo.uvTopRight;
                uv[4 * charIndex + 2] = charInfo.uvBottomRight;
                uv[4 * charIndex + 3] = charInfo.uvBottomLeft;

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
 
            return mesh;
        }
        
    }
}