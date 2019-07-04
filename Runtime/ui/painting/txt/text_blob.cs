namespace Unity.UIWidgets.ui {
    public struct TextBlob {
        internal TextBlob(string text, int textOffset, int textSize, float[] positions,
            float minX, float minY, float width, float height, TextStyle style) {
            this.instanceId = ++_nextInstanceId;
            this.text = text;
            this.textOffset = textOffset;
            this.textSize = textSize;
            this.style = style;
            this._minX = minX;
            this._minY = minY;
            this._width = width;
            this._height = height;
            this._positions = positions;
        }

        public Rect boundsInText {
            get {
                var pos = this._positions[this.textOffset];
                return Rect.fromLTWH(this._minX + pos, this._minY, this._width, this._height);
            }
        }

        public Rect shiftedBoundsInText(Offset offset) {
            var pos = this._positions[this.textOffset];
            return Rect.fromLTWH(this._minX + pos + offset.dx, this._minY + offset.dy, this._width, this._height);
        }

        public float getPosition(int i) {
            return this._positions[this.textOffset + i];
        }

        static long _nextInstanceId;
        internal readonly long instanceId;
        internal readonly string text;
        internal readonly int textOffset;
        internal readonly int textSize;
        internal readonly TextStyle style;
        readonly float _minX, _minY, _width, _height; // bounds with positions[start] as origin       
        readonly float[] _positions;
    }

    public struct TextBlobBuilder {
        TextStyle _style;
        float[] _positions;
        string _text;
        int _textOffset;
        int _size;
        float _minX, _minY, _width, _height;

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
            if (this._positions == null || this._positions.Length < size) {
                this._positions = new float[size];
            }
        }

        public void setPosition(int i, float position) {
            this._positions[this._textOffset + i] = position;
        }

        public void setPositions(float[] positions) {
            this._positions = positions;
        }

        public void setBounds(float minX, float minY, float width, float height) {
            this._minX = minX;
            this._minY = minY;
            this._width = width;
            this._height = height;
        }

        public void setBounds(UnityEngine.Rect bounds) {
            this.setBounds(bounds.x, bounds.y, bounds.width, bounds.height);
        }

        public TextBlob make() {
            var result = new TextBlob(this._text, this._textOffset,
                this._size, this._positions, this._minX, this._minY, this._width, this._height, this._style);
            return result;
        }
    }
}