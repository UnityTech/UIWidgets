namespace Unity.UIWidgets.ui {
    public class TextBlob {
        internal TextBlob(string text, int textOffset, int textSize, Vector2d[] positions, Rect bounds,
            TextStyle style) {
            this.instanceId = ++_nextInstanceId;
            this.text = text;
            this.textOffset = textOffset;
            this.textSize = textSize;
            this.style = style;
            this.bounds = bounds;
            this._positions = positions;
        }

        public Rect boundsInText {
            get { return this.bounds.translate(this._positions[this.textOffset].x, this._positions[this.textOffset].y); }
        }

        public Rect shiftedBoundsInText(Offset offset) {
            return this.bounds.translate(this._positions[this.textOffset].x + offset.dx, 
                this._positions[this.textOffset].y + offset.dy);
        }

        public Vector2d getPosition(int i) {
            return this._positions[this.textOffset + i];
        }

        static long _nextInstanceId;
        internal readonly long instanceId;
        internal readonly string text;
        internal readonly int textOffset;
        internal readonly int textSize;
        internal readonly TextStyle style;
        internal readonly Rect bounds; // bounds with positions[start] as origin       
        readonly Vector2d[] _positions;
    }

    public class TextBlobBuilder {
        TextStyle _style;
        public Vector2d[] positions;
        string _text;
        int _textOffset;
        int _size;
        Rect _bounds;

        public void allocRunPos(painting.TextStyle style, string text, int offset, int size,
            float textScaleFactor = 1.0f) {
            this.allocRunPos(TextStyle.applyStyle(null, style, textScaleFactor), text, offset, size);
        }

        internal void allocRunPos(TextStyle style, string text, int offset, int size) {
            this._style = style;
            this._text = text;
            this._textOffset = offset;
            this._size = size;
            // Allocate a single buffer for all text blobs that share this text, to save memory and GC.
            // It is assumed that all of `text` is being used. This may cause great waste if a long text is passed
            // but only a small part of it is to be rendered, which is not the case for now.
            this.allocPos(text.Length);
        }

        internal void allocPos(int size) {
            if (this.positions == null || this.positions.Length < size) {
                this.positions = new Vector2d[size];
            }
        }

        public void setPosition(int i, Vector2d position) {
            this.positions[this._textOffset + i] = position;
        }

        public void setBounds(Rect bounds) {
            this._bounds = bounds;
        }

        public TextBlob make() {
            var result = new TextBlob(this._text, this._textOffset,
                this._size, this.positions, this._bounds, this._style);
            return result;
        }
    }
}