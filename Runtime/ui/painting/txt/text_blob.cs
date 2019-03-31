namespace Unity.UIWidgets.ui {
    public class TextBlob {
        
        internal TextBlob(string text, int textOffset, int textSize, Vector2d[] positions, Rect bounds, TextStyle style) {
            this.instanceId = ++_nextInstanceId;
            this.positions = positions;
            this.text = text;
            this.textOffset = textOffset;
            this.textSize = textSize;
            this.style = style;
            this.bounds = bounds;
        }

        public Rect boundsInText {
            get { return this.bounds.shift(new Offset(this.positions[0].x, this.positions[0].y)); }
        }

        static long _nextInstanceId = 0;
        internal readonly long instanceId;
        internal readonly string text;
        internal readonly int textOffset;
        internal readonly int textSize;
        internal readonly Vector2d[] positions;
        internal readonly TextStyle style;
        internal readonly Rect bounds; // bounds with positions[start] as origin       
    }

    public class TextBlobBuilder {
        TextStyle _style;
        public Vector2d[] positions;
        string _text;
        int _textOffset;
        int _size;
        Rect _bounds;

        public void allocRunPos(painting.TextStyle style, string text, int offset, int size, float textScaleFactor = 1.0f) {
            this.allocRunPos(TextStyle.applyStyle(null, style, textScaleFactor), text, offset, size);
        }
        
        internal void allocRunPos(TextStyle style, string text, int offset, int size) {
            this._style = style;
            this._text = text;
            this._textOffset = offset;
            this._size = size;
            if (this.positions == null || this.positions.Length < size) {
                this.positions = new Vector2d[size];
            }
        }

        public void setBounds(Rect bounds) {
            this._bounds = bounds;
        }

        public TextBlob make() {
            var result = new TextBlob(this._text, this._textOffset,
                this._size, this.positions, this._bounds, this._style);
            this.positions = null;
            return result;
        }
    }
}