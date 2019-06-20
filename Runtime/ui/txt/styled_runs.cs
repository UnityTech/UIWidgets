using System;
using System.Collections.Generic;

namespace Unity.UIWidgets.ui {
    class StyledRuns {
        readonly List<TextStyle> styles = new List<TextStyle>();
        readonly List<IndexedRun> runs = new List<IndexedRun>();

        public struct RunIterator {
            int _charIndex;
            int _runIndex;
            StyledRuns _runs;

            public void nextTo(int index) {
                if (this._charIndex > index) {
                    throw new ArgumentException("can to move back");
                }

                this._charIndex = index;
                while (this._runIndex < this._runs.size) {
                    var run = this._runs.getRun(this._runIndex);
                    if (run.start <= this._charIndex && this._charIndex < run.end) {
                        break;
                    }

                    this._runIndex++;
                }
            }

            public Run run {
                get { return this._runs.getRun(this._runIndex); }
            }

            public int charIndex {
                get { return this._charIndex; }
            }

            public int runIndex {
                get { return this._runIndex; }
            }

            public bool end {
                get { return this.runIndex >= this._runs.size; }
            }

            internal RunIterator(StyledRuns runs) {
                this._charIndex = 0;
                this._runIndex = 0;
                this._runs = runs;
            }
        }

        internal struct Run {
            public readonly TextStyle style;
            public readonly int start;
            public readonly int end;

            public Run(TextStyle style, int start, int end) {
                this.style = style;
                this.start = start;
                this.end = end;
            }
        }

        internal class IndexedRun {
            public readonly int styleIndex;
            public readonly int start;
            public int end;

            public IndexedRun(int styleIndex, int start, int end) {
                this.styleIndex = styleIndex;
                this.start = start;
                this.end = end;
            }
        }

        public StyledRuns() {
        }

        public StyledRuns(StyledRuns other) {
            this.styles = new List<TextStyle>(other.styles);
            this.runs = new List<IndexedRun>(other.runs);
        }

        public int addStyle(TextStyle style) {
            var styleIndex = this.styles.Count;
            this.styles.Add(style);
            return styleIndex;
        }

        public TextStyle getStyle(int index) {
            return this.styles[index];
        }

        public void startRun(int styleIndex, int start) {
            this.endRunIfNeeded(start);
            this.runs.Add(new IndexedRun(styleIndex, start, start));
        }

        public void endRunIfNeeded(int end) {
            var lastIndex = this.runs.Count - 1;
            if (lastIndex < 0) {
                return;
            }

            var run = this.runs[lastIndex];
            if (run.start == end) {
                this.runs.RemoveAt(lastIndex);
            }
            else {
                run.end = end;
            }
        }

        public Run getRun(int index) {
            var run = this.runs[index];
            return new Run(this.styles[run.styleIndex], run.start, run.end);
        }

        public RunIterator iterator() {
            return new RunIterator(this);
        }

        public int size {
            get { return this.runs.Count; }
        }
    }
}