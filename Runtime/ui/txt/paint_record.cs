namespace Unity.UIWidgets.ui {
    struct PaintRecord {
        public PaintRecord(TextStyle style, float dx, float dy, TextBlob text, FontMetrics metrics, float runWidth) {
            this._style = style;
            this._text = text;
            this._runWidth = runWidth;
            this._metrics = metrics;
            this._dx = dx;
            this._dy = dy;
        }

        public TextBlob text {
            get { return this._text; }
        }

        public TextStyle style {
            get { return this._style; }
        }

        public float runWidth {
            get { return this._runWidth; }
        }

        public FontMetrics metrics {
            get { return this._metrics; }
        }

        public void shift(float x, float y) {
            this._dx += x;
            this._dy += y;
        }

        public Offset shiftedOffset(Offset other) {
            return new Offset(this._dx + other.dx, this._dy + other.dy);
        }

        TextStyle _style;
        TextBlob _text;
        float _runWidth;
        float _dx;
        float _dy;
        FontMetrics _metrics;
    }
}