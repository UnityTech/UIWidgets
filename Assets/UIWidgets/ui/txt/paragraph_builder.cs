using System.Collections.Generic;
using System.Text;

namespace UIWidgets.ui
{
    public class ParagraphBuilder
    {
        private StringBuilder _text = new StringBuilder();
        private ParagraphStyle _paragraphStyle;
        private StyledRuns _runs = new StyledRuns();
        private List<int>  _styleStack = new List<int>();
        private int _paragraph_style_index;
  
        public ParagraphBuilder(ParagraphStyle style)
        {
            setParagraphStyle(style);
        }
        
        public Paragraph build()
        {
            var paragraph = new Paragraph();
            paragraph.setText(_text.ToString(), _runs);
            paragraph.setParagraphStyle(_paragraphStyle);
            return paragraph;
        }

        public void pushStyle(TextStyle style)
        {
            var newStyle = peekStyle().merge(style);
            var styleIndex = _runs.addStyle(newStyle);
            _styleStack.Add(styleIndex);
            _runs.startRun(styleIndex, _text.Length);
        }

        public void pop()
        {
            var lastIndex = _styleStack.Count - 1;
            if (lastIndex < 0)
            {
                return;
            }
            _styleStack.RemoveAt(lastIndex);
            _runs.startRun(peekStyleIndex(), _text.Length);
        }
        
        public void addText(string text)
        {
            this._text.Append(text);
        }

        public TextStyle peekStyle()
        {
            return _runs.getStyle(peekStyleIndex());
        }
        
        
        public int peekStyleIndex() {
            int count = _styleStack.Count;
            if (count > 0)
            {
                return _styleStack[count - 1];
            }
            return _paragraph_style_index;
        }

        private void setParagraphStyle(ParagraphStyle style)
        {
            _paragraphStyle = style;
            _paragraph_style_index = _runs.addStyle(style.getTextStyle());
            _runs.startRun(_paragraph_style_index, _text.Length);
        }
        
        
    }
}