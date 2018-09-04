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
        private string _text;
        private float _width;
        private int _lineStart;
        private int _wordStart;
        private int _spaceCount = 0;
        private int tabCount = 4;
        private double _lineLength;
        private Font[] _styleRunFonts;

        private List<LineInfo> _lines;

        public void setup(string text, StyledRuns runs, Font[] styleRunFonts, float width, Vector2[] characterPositions, float[] characterWidth)
        {
            _text = text;
            _runs = runs;
            _characterPositions = characterPositions;
            _characterWidth = characterWidth;
            _width = width;
            _styleRunFonts = styleRunFonts;
        }
        
        public List<LineInfo> getLines()
        {
            return _lines;
        }
   
        public void doBreak(int blockStart, int blockEnd)
        {
            _lines = new List<LineInfo>();
            _lineStart = blockStart;
            _wordStart = blockStart;
            _spaceCount = 0;
            
            float offsetX = 0.0f;
            var runIterator = _runs.iterator();
            for (var charIndex = blockStart; charIndex < blockEnd; charIndex++)
            {
                runIterator.nextTo(charIndex);
                var run = runIterator.run;
                var font = _styleRunFonts[runIterator.runIndex];

                var style = run.style;
                CharacterInfo charInfo;
                
                var result = font.GetCharacterInfo(_text[charIndex], out charInfo, 0, run.style.UnityFontStyle);
 
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