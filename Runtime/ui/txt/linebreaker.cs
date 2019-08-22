using System.Collections.Generic;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    class TabStops {
        int _tabWidth = int.MaxValue;

        Font _font;

        int _fontSize;

        int _spaceAdvance;

        const int kTabSpaceCount = 4;

        public void setFont(Font font, int size) {
            if (this._font != font || this._fontSize != size) {
                this._tabWidth = int.MaxValue;
            }

            this._font = font;
            // Recompute the advance of space (' ') if font size changes
            if (this._fontSize != size) {
                this._fontSize = size;
                this._font.RequestCharactersInTextureSafe(" ", this._fontSize);
                this._font.getGlyphInfo(' ', out var glyphInfo, this._fontSize, UnityEngine.FontStyle.Normal);
                this._spaceAdvance = glyphInfo.advance;
            }
        }

        public float nextTab(float widthSoFar) {
            if (this._tabWidth == int.MaxValue) {
                if (this._fontSize > 0) {
                    this._tabWidth = this._spaceAdvance * kTabSpaceCount;
                }
            }

            if (this._tabWidth == 0) {
                return widthSoFar;
            }

            return (Mathf.Floor(widthSoFar / this._tabWidth + 1) * this._tabWidth);
        }
    }

    struct Candidate {
        public int offset;
        public int pre;
        public float preBreak;
        public float penalty;

        public float postBreak;
        public int preSpaceCount;
        public int postSpaceCount;
    }

    class LineBreaker {
        const float ScoreInfty = float.MaxValue;
        const float ScoreDesperate = 1e10f;

        int _lineLimit = 0;

        // Limit number of lines, 0 means no limit
        public int lineLimit {
            get { return this._lineLimit; }
            set { this._lineLimit = value; }
        }

        public static LineBreaker instance {
            get {
                if (_instance == null) {
                    _instance = new LineBreaker();
                }

                return _instance;
            }
        }

        static LineBreaker _instance;

        public static int[] newLinePositions(string text, out int count) {
            count = 0;
            for (var i = 0; i < text.Length; i++) {
                if (text[i] == '\n') {
                    count++;
                }
            }

            count++;

            if (_newLinePositions == null || _newLinePositions.Length < count) {
                _newLinePositions = new int[count];
            }

            count = 0;
            for (var i = 0; i < text.Length; i++) {
                if (text[i] == '\n') {
                    _newLinePositions[count++] = i;
                }
            }

            _newLinePositions[count++] = text.Length;

            return _newLinePositions;
        }

        static int[] _newLinePositions;

        TextBuff _textBuf;
        float[] _charWidths;
        List<int> _breaks = new List<int>();
        int _breaksCount = 0;
        List<float> _widths = new List<float>();
        int _widthsCount = 0;
        WordBreaker _wordBreaker = new WordBreaker();
        float _width = 0.0f;
        float _preBreak;
        float _lineWidth;
        int _lastBreak;
        int _bestBreak;
        float _bestScore;
        int _spaceCount;
        TabStops _tabStops;
        int mFirstTabIndex;
        List<Candidate> _candidates = new List<Candidate>();
        int _candidatesCount = 0;

        public int computeBreaks() {
            int nCand = this._candidatesCount;
            if (nCand > 0 && (nCand == 1 || this._lastBreak != nCand - 1)) {
                var cand = this._candidates[this._candidatesCount - 1];
                this._pushBreak(cand.offset, (cand.postBreak - this._preBreak));
            }

            return this._breaksCount;
        }

        public int getBreaksCount() {
            return this._breaksCount;
        }

        public int getBreak(int i) {
            return this._breaks[i];
        }

        public float getWidth(int i) {
            return this._widths[i];
        }

        public void resize(int size) {
            if (this._charWidths == null || this._charWidths.Length < size) {
                this._charWidths = new float[LayoutUtils.minPowerOfTwo(size)];
            }
        }

        public void setText(string text, int textOffset, int textLength) {
            this._textBuf = new TextBuff(text, textOffset, textLength);
            this._wordBreaker.setText(this._textBuf);
            this._wordBreaker.next();
            this._candidatesCount = 0;
            Candidate can = new Candidate {
                offset = 0, postBreak = 0, preBreak = 0, postSpaceCount = 0, preSpaceCount = 0, pre = 0
            };
            this._addCandidateToList(can);
            this._lastBreak = 0;
            this._bestBreak = 0;
            this._bestScore = ScoreInfty;
            this._preBreak = 0;
            this.mFirstTabIndex = int.MaxValue;
            this._spaceCount = 0;
        }

        public void setLineWidth(float lineWidth) {
            this._lineWidth = lineWidth;
        }

        public float addStyleRun(TextStyle style, int start, int end) {
            float width = 0;
            if (style != null) {
//                Layout.measureText(this._width - this._preBreak, this._textBuf,
//                    start, end - start, style,
//                    this._charWidths, start, this._tabStops);
                width = Layout.computeCharWidths(this._width - this._preBreak, this._textBuf.text,
                    this._textBuf.offset + start, end - start, style,
                    this._charWidths, start, this._tabStops);
            }

            int current = this._wordBreaker.current();
            float postBreak = this._width;
            int postSpaceCount = this._spaceCount;

            for (int i = start; i < end; i++) {
                char c = this._textBuf.charAt(i);
                if (c == '\t') {
                    this._width = this._preBreak + this._tabStops.nextTab(this._width - this._preBreak);
                    if (this.mFirstTabIndex == int.MaxValue) {
                        this.mFirstTabIndex = i;
                    }
                }
                else {
                    if (LayoutUtils.isWordSpace(c)) {
                        this._spaceCount += 1;
                    }

                    this._width += this._charWidths[i];
                    if (!LayoutUtils.isLineEndSpace(c)) {
                        postBreak = this._width;
                        postSpaceCount = this._spaceCount;
                    }
                }

                if (i + 1 == current) {
                    if (style != null || current == end || this._charWidths[current] > 0) {
                        this._addWordBreak(current, this._width, postBreak, this._spaceCount, postSpaceCount, 0);
                    }

                    current = this._wordBreaker.next();
                }
            }

            return width;
        }

        public void finish() {
            this._wordBreaker.finish();
            this._width = 0;
            this._candidatesCount = 0;
            this._breaksCount = 0;
            this._widthsCount = 0;
            this._textBuf = default;
        }

        public int getWidthsCount() {
            return this._widthsCount;
        }

        public void setTabStops(TabStops tabStops) {
            this._tabStops = tabStops;
        }

        void _addWordBreak(int offset, float preBreak, float postBreak, int preSpaceCount, int postSpaceCount,
            float penalty) {
            float width = this._candidates[this._candidatesCount - 1].preBreak;
            if (postBreak - width > this._lineWidth) {
                this._addCandidatesInsideWord(width, offset, postSpaceCount);
            }

            this._addCandidate(new Candidate {
                offset = offset,
                preBreak = preBreak,
                postBreak = postBreak,
                preSpaceCount = preSpaceCount,
                postSpaceCount = postSpaceCount,
                penalty = penalty
            });
        }

        void _addCandidatesInsideWord(float width, int offset, int postSpaceCount) {
            int i = this._candidates[this._candidatesCount - 1].offset;
            width += this._charWidths[i++];
            for (; i < offset; i++) {
                float w = this._charWidths[i];
                if (w > 0) {
                    this._addCandidate(new Candidate {
                        offset = i,
                        preBreak = width,
                        postBreak = width,
                        preSpaceCount = postSpaceCount,
                        postSpaceCount = postSpaceCount,
                        penalty = ScoreDesperate,
                    });
                    width += w;
                }
            }
        }

        void _addCandidateToList(Candidate cand) {
            if (this._candidates.Count == this._candidatesCount) {
                this._candidates.Add(cand);
                this._candidatesCount++;
            }
            else {
                this._candidates[this._candidatesCount++] = cand;
            }
        }

        void _addCandidate(Candidate cand) {
            int candIndex = this._candidatesCount;
            this._addCandidateToList(cand);
            if (cand.postBreak - this._preBreak > this._lineWidth) {
                if (this._bestBreak == this._lastBreak) {
                    this._bestBreak = candIndex;
                }

                this._pushGreedyBreak();
            }

            while (this._lastBreak != candIndex && cand.postBreak - this._preBreak > this._lineWidth) {
                for (int i = this._lastBreak + 1; i < candIndex; i++) {
                    float penalty = this._candidates[i].penalty;
                    if (penalty <= this._bestScore) {
                        this._bestBreak = i;
                        this._bestScore = penalty;
                    }
                }

                if (this._bestBreak == this._lastBreak) {
                    this._bestBreak = candIndex;
                }

                this._pushGreedyBreak();
            }

            if (cand.penalty <= this._bestScore) {
                this._bestBreak = candIndex;
                this._bestScore = cand.penalty;
            }
        }

        void _pushGreedyBreak() {
            var bestCandidate = this._candidates[this._bestBreak];
            this._pushBreak(bestCandidate.offset, bestCandidate.postBreak - this._preBreak);
            this._bestScore = ScoreInfty;
            this._lastBreak = this._bestBreak;
            this._preBreak = bestCandidate.preBreak;
        }

        void _pushBreak(int offset, float width) {
            if (this.lineLimit == 0 || this._breaksCount < this.lineLimit) {
                if (this._breaks.Count == this._breaksCount) {
                    this._breaks.Add(offset);
                    this._breaksCount++;
                }
                else {
                    this._breaks[this._breaksCount++] = offset;
                }

                if (this._widths.Count == this._widthsCount) {
                    this._widths.Add(width);
                    this._widthsCount++;
                }
                else {
                    this._widths[this._widthsCount++] = width;
                }
            }
        }
    }
}