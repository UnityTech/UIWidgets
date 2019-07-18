namespace Unity.UIWidgets.ui {
    public struct TextBlob {
        internal TextBlob(string text, int textOffset, int textSize, float[] positionXs,
            UnityEngine.Rect bounds, TextStyle style) {
            this.instanceId = ++_nextInstanceId;
            this._positionXs = positionXs;
            this.text = text;
            this.textOffset = textOffset;
            this.textSize = textSize;
            this.style = style;
            this._bounds = bounds;
            this._boundsInText = null;
        }

        public Rect boundsInText {
            get {
                if (this._boundsInText == null) {
                    var pos = this.getPositionX(0);
                    this._boundsInText = Rect.fromLTWH(this._bounds.xMin + pos, this._bounds.yMin,
                        this._bounds.width, this._bounds.height);
                }

                return this._boundsInText;
            }
        }

        public Rect shiftedBoundsInText(float dx, float dy) {
            var pos = this.getPositionX(0);
            return Rect.fromLTWH(this._bounds.xMin + pos + dx, this._bounds.yMin + dy,
                this._bounds.width, this._bounds.height);
        }

        public float getPositionX(int i) {
            return this._positionXs[this.textOffset + i];
        }

        static long _nextInstanceId;
        internal readonly long instanceId;
        internal readonly string text;
        internal readonly int textOffset;
        internal readonly int textSize;
        internal readonly TextStyle style;
        readonly UnityEngine.Rect _bounds; // bounds with positions[start] as origin       
        readonly float[] _positionXs;

        Rect _boundsInText;
    }

    public struct TextBlobBuilder {
        TextStyle _style;
        float[] _positionXs;
        string _text;
        int _textOffset;
        int _size;
        UnityEngine.Rect _bounds;

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
            if (this._positionXs == null || this._positionXs.Length < size) {
                this._positionXs = new float[size];
            }
        }

        public void setPositionX(int i, float positionX) {
            this._positionXs[this._textOffset + i] = positionX;
        }

        public void setPositionXs(float[] positionXs) {
            this._positionXs = positionXs;
        }

        public void setBounds(UnityEngine.Rect bounds) {
            this._bounds = bounds;
        }

        public TextBlob make() {
            var result = new TextBlob(this._text, this._textOffset,
                this._size, this._positionXs, this._bounds, this._style);
            return result;
        }
    }
}