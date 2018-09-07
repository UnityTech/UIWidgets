using UIWidgets.ui.txt;

namespace UIWidgets.ui
{
    public class PaintRecord
    {
        public PaintRecord(TextStyle style,  TextBlob _text,
            int line, double runWidth)
        {
            this._style = style;
            this._text = _text;
            this._line = line;
            this._runWidth = runWidth;
        }

        public TextBlob text
        {
            get { return _text; }
        }

        public TextStyle style
        {
            get { return _style; }
        }

        public int line
        {
            get { return _line; }
        }

        public double runWidth
        {
            get { return _runWidth; }
        }

        private  TextStyle _style;
        private TextBlob _text;
        private int _line;
        private double _runWidth;

    }
}