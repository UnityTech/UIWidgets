using Unity.UIWidgets.ui.txt;

namespace Unity.UIWidgets.ui
{
    public class PaintRecord
    {
        public PaintRecord(TextStyle style,  Offset offset, TextBlob _text,
            FontMetrics metrics,
            int line, double runWidth)
        {
            this._style = style;
            this._text = _text;
            this._line = line;
            this._runWidth = runWidth;
            this._metrics = metrics;
            this._offset = offset;
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

        public Offset offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        public FontMetrics metrics
        {
            get { return _metrics; }
        }

        private  TextStyle _style;
        private TextBlob _text;
        private int _line;
        private double _runWidth;
        private Offset _offset;
        private FontMetrics _metrics;

    }
}