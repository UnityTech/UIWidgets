namespace Unity.UIWidgets.ui {
    class PaintRecord {
        public PaintRecord(TextStyle style, Offset offset, TextBlob text,
            FontMetrics metrics,
            int line, float runWidth) {
            this._style = style;
            this._text = text;
            this._line = line;
            this._runWidth = runWidth;
            this._metrics = metrics;
            this._offset = offset;
        }

        public TextBlob text {
            get { return this._text; }
        }

        public TextStyle style {
            get { return this._style; }
        }

        public int line {
            get { return this._line; }
        }

        public float runWidth {
            get { return this._runWidth; }
        }

        public Offset offset {
            get { return this._offset; }
            set { this._offset = value; }
        }

        public FontMetrics metrics {
            get { return this._metrics; }
        }

        TextStyle _style;
        TextBlob _text;
        int _line;
        float _runWidth;
        Offset _offset;
        FontMetrics _metrics;
    }
}