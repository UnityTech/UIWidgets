using System.Collections.Generic;
using UnityEngine;

namespace UIWidgets.ui
{
    public class StyledRuns
    {
        private readonly  List<TextStyle> styles = new List<TextStyle>();
        private readonly List<IndexedRun> runs = new List<IndexedRun>();

        public class Run
        {
            public readonly TextStyle style;
            public readonly int start;
            public readonly int end;
            public Font _font;

            public Run(TextStyle style, int start, int end)
            {
                this.style = style;
                this.start = start;
                this.end = end;
            }

            public Font font
            {
                get
                {
                    if (_font == null)
                    {
                        _font = Font.CreateDynamicFontFromOSFont(style.safeFontFamily,
                            (style.UnityFontSize));
                    }
                    return _font;
                } 
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

        public int size
        {
            get { return runs.Count; }
        }
    }
}