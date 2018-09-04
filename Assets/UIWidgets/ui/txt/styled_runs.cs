using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIWidgets.ui
{
    public class StyledRuns
    {
        private readonly  List<TextStyle> styles = new List<TextStyle>();
        private readonly List<IndexedRun> runs = new List<IndexedRun>();

        public class RunIterator
        {
            private int _charIndex;
            private int _runIndex;
            private StyledRuns _runs;

            public void nextTo(int index)
            {
                if (_charIndex > index)
                {
                    throw new ArgumentException("can to move back");
                }
                _charIndex = index;
                while (_runIndex < _runs.size)
                {
                    var run = _runs.getRun(_runIndex);
                    if (run.start <= _charIndex && _charIndex < run.end)
                    {
                        break;
                    }
                    _runIndex++;
                }
            }

            public Run run
            {
                get { return _runs.getRun(_runIndex); }
            }
            
            public int charIndex
            {
                get { return _charIndex; }
            }

            public int runIndex
            {
                get { return _runIndex; }
            }

            public bool end
            {
                get
                {
                    return runIndex >= _runs.size;
                }
            }

            internal RunIterator(StyledRuns runs)
            {
                _charIndex = 0;
                _runIndex = 0;
                _runs = runs;
            }
            
            
        }
        public class Run
        {
            public readonly TextStyle style;
            public readonly int start;
            public readonly int end;

            public Run(TextStyle style, int start, int end)
            {
                this.style = style;
                this.start = start;
                this.end = end;
            }
        }
        
        public class IndexedRun
        {
            public readonly int styleIndex = 0;
            public readonly int start;
            public int end;

            public IndexedRun(int styleIndex, int start, int end)
            {
                this.styleIndex = styleIndex;
                this.start = start;
                this.end = end;
            }
        }
        
        public StyledRuns()
        {
        }
        
        public StyledRuns(StyledRuns other)
        {
            styles = new List<TextStyle>(other.styles);
            runs = new List<IndexedRun>(other.runs);
        }

        public int addStyle(TextStyle style)
        {
            var styleIndex = styles.Count;
            styles.Add( style);
            return styleIndex;

        }

        public TextStyle getStyle(int index)
        {
            return styles[index];
        }

        public void startRun(int styleIndex, int start)
        {
            endRunIfNeeded(start);
            runs.Add(new IndexedRun(styleIndex, start, start));
            
        }

        public void endRunIfNeeded(int end)
        {
            var lastIndex = runs.Count - 1;
            if (lastIndex < 0)
            {
                return;
            }

            var run = runs[lastIndex];
            if (run.start == end)
            {
                runs.RemoveAt(lastIndex);
            }
            else
            {
                run.end = end;
            }
        }

        public Run getRun(int index)
        {
            var run = runs[index];
            return new Run(styles[run.styleIndex], run.start, run.end);
        }

        public RunIterator iterator()
        {
            return new RunIterator(this);
        }
        
        public int size
        {
            get { return runs.Count; }
        }
    }
}