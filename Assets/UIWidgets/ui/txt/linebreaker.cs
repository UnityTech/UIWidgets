using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIWidgets.ui
{
    public class LineBreaker
    {
        public class LineInfo
        {
            public int start;
            public double width;
        }
        
        private StyledRuns _runs;
        
        public Vector2[] _characterPositions;
        public float[] _characterWidth;
 //       private List<int> breaks;
        private string _text;
        private float _width;
        private int _lineStart;
        private int _wordStart;
       // private int _preWordEnd;
        private int _runIndex = 0;
        private int _spaceCount = 0;
        private int tabCount = 4;
        private double _lineLength;

        private List<LineInfo> _lines;
        // private double lengths

        public void setup(string text, StyledRuns runs, float width, Vector2[] characterPositions, float[] characterWidth)
        {
            _text = text;
            _runs = runs;
            _characterPositions = characterPositions;
            _characterWidth = characterWidth;
            _width = width;
        }
        
        public List<LineInfo> getLines()
        {
            return _lines;
        }
        
        public void doBreak(int blockStart, int blockEnd)
        {
            _lines = new List<LineInfo>();
            float offsetX = 0.0f;

            for (var charIndex = blockStart; charIndex < blockEnd; charIndex++)
            {
                var run = _runs.getRun(_runIndex);
                while ((run == null || run.end <= charIndex || charIndex < run.start) &&
                       _runIndex + 1 < _runs.size)
                {
                    _runIndex++;
                    run = _runs.getRun(_runIndex);
                }

                if (run.start == charIndex)
                {
                    run.font.RequestCharactersInTexture(_text.Substring(run.start, run.end - run.start), run.style.UnityFontSize, run.style.UnityFontStyle);
                }

                var style = run.style;
                var font = run.font;


                CharacterInfo charInfo;
                var result = font.GetCharacterInfo(_text[charIndex], out charInfo);
                // _characterSize[charIndex] = style.UnityFontSize;

                // 
                if (_text[charIndex] == '\t')
                {
                    _spaceCount++;

                    font.GetCharacterInfo(' ', out charInfo,
                        style.UnityFontSize, style.UnityFontStyle);
                    float tabSize = charInfo.advance * tabCount;
                    var newX = (float)Math.Floor(((offsetX / tabSize) + 1) * tabSize);
                    if (newX > _width && _lineStart != charIndex)
                    {
                        _characterWidth[charIndex] = tabSize;
                        makeLine(charIndex, charIndex);
                    }
                    else
                    {
                        _characterWidth[charIndex] = newX - offsetX;
                        _characterPositions[charIndex].x = offsetX;
                    }
                    offsetX = _characterPositions[charIndex].x + _characterWidth[charIndex];
                }
                else if (_text[charIndex] == ' ')
                {
                    _spaceCount++;
                    _characterPositions[charIndex].x = offsetX;
                    _characterWidth[charIndex] = charInfo.advance;
                    offsetX = _characterPositions[charIndex].x + _characterWidth[charIndex];
                    // todo no wrap in space ?
                }
                else
                {
                    if (_spaceCount > 0 || blockStart == charIndex)
                    {
                        _wordStart = charIndex;
                    }

                    _characterPositions[charIndex].x = offsetX;
                    _characterWidth[charIndex] = charInfo.advance;
                    
                    if (offsetX + charInfo.advance > _width && _lineStart != charIndex)
                    {
                        if (_lineStart == _wordStart)
                        {    
                            makeLine(charIndex, charIndex);
                            _wordStart = charIndex;
                        }
                        else
                        {
                            makeLine(_wordStart, charIndex);
                        }
                    }

                    offsetX = _characterPositions[charIndex].x + _characterWidth[charIndex];
                    _spaceCount = 0;
                }


                /*
                 *  CharacterInfo charInfo;
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
                 */
            }

            makeLine(blockEnd, blockEnd);
        }


        private void makeLine(int end, int last)
        {
            Debug.Assert(_lineStart < end);
            Debug.Assert(end <= last);
            _lines.Add(new LineInfo()
            {
                start =  _lineStart,
                width = _characterPositions[end - 1].x + _characterWidth[end - 1],
            });
            _lineStart = end;

            if (end >= _characterPositions.Length)
            {
                return;
            }
            var offset = new Vector2(-_characterPositions[end].x, 0);
            _characterPositions[end].x = 0;
            if (end < last)
            {
                Paragraph.offsetCharacters(offset,
                    _characterPositions, end + 1, last + 1);
            }
        }
        
    }
}