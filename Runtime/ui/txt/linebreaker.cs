using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public class LineBreaker {
        public class LineInfo {
            public int start;
            public double width;
        }

        StyledRuns _runs;

        public Vector2d[] _characterPositions;
        public double[] _characterWidth;
        string _text;
        double _width;
        int _lineStart;
        int _wordStart;
        int _spaceCount = 0;
        int tabCount = 4;
        double _lineLength;

        List<LineInfo> _lines;

        public void setup(string text, StyledRuns runs, double width, Vector2d[] characterPositions,
            double[] characterWidth) {
            this._text = text;
            this._runs = runs;
            this._characterPositions = characterPositions;
            this._characterWidth = characterWidth;
            this._width = width;
        }

        public List<LineInfo> getLines() {
            return this._lines;
        }

        public void doBreak(int blockStart, int blockEnd) {
            this._lines = new List<LineInfo>();
            this._lineStart = blockStart;
            this._wordStart = blockStart;
            this._spaceCount = 0;

            double offsetX = 0.0;
            var runIterator = this._runs.iterator();
            for (var charIndex = blockStart; charIndex < blockEnd; charIndex++) {
                runIterator.nextTo(charIndex);
                var run = runIterator.run;
                var font = FontManager.instance.getOrCreate(run.style.fontFamily).font;

                var style = run.style;
                var charInfo = new CharacterInfo();

                if (this._text[charIndex] == '\t') {
                    this._spaceCount++;

                    font.GetCharacterInfo(' ', out charInfo,
                        style.UnityFontSize, style.UnityFontStyle);
                    double tabSize = charInfo.advance * this.tabCount;
                    var newX = Math.Floor(((offsetX / tabSize) + 1) * tabSize);
                    if (newX > this._width && this._lineStart != charIndex) {
                        this._characterWidth[charIndex] = tabSize;
                        this.makeLine(charIndex, charIndex);
                    }
                    else {
                        this._characterWidth[charIndex] = newX - offsetX;
                        this._characterPositions[charIndex].x = offsetX;
                    }

                    offsetX = this._characterPositions[charIndex].x + this._characterWidth[charIndex];
                }
                else if (this._text[charIndex] == ' ') {
                    font.GetCharacterInfo(this._text[charIndex], out charInfo, style.UnityFontSize,
                        run.style.UnityFontStyle);
                    this._spaceCount++;
                    this._characterPositions[charIndex].x = offsetX;
                    this._characterWidth[charIndex] = charInfo.advance;
                    offsetX = this._characterPositions[charIndex].x + this._characterWidth[charIndex];
                    // todo no wrap in space ?
                }
                else {
                    font.GetCharacterInfo(this._text[charIndex], out charInfo, style.UnityFontSize,
                        run.style.UnityFontStyle);
                    if (this._spaceCount > 0 || blockStart == charIndex) {
                        this._wordStart = charIndex;
                    }

                    this._characterPositions[charIndex].x = offsetX;
                    this._characterWidth[charIndex] = charInfo.advance;

                    if (offsetX + charInfo.advance > this._width && this._lineStart != charIndex) {
                        if (this._lineStart == this._wordStart) {
                            this.makeLine(charIndex, charIndex);
                            this._wordStart = charIndex;
                        }
                        else {
                            this.makeLine(this._wordStart, charIndex);
                        }
                    }

                    offsetX = this._characterPositions[charIndex].x + this._characterWidth[charIndex];
                    this._spaceCount = 0;
                }
            }

            this.makeLine(blockEnd, blockEnd);
        }


        void makeLine(int end, int last) {
            Debug.Assert(this._lineStart < end);
            Debug.Assert(end <= last);
            this._lines.Add(new LineInfo() {
                start = this._lineStart,
                width = this._characterPositions[end - 1].x + this._characterWidth[end - 1],
            });
            this._lineStart = end;

            if (end >= this._characterPositions.Length) {
                return;
            }

            var offset = new Vector2d(-this._characterPositions[end].x, 0);
            this._characterPositions[end].x = 0;
            if (end < last) {
                Paragraph.offsetCharacters(offset, this._characterPositions, end + 1, last + 1);
            }
        }
    }
}