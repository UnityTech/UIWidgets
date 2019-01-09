using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.ui {
    public class TextBlob {
        public TextBlob(string text, int start, int end, Vector2d[] positions, TextStyle style, Rect bounds) {
            D.assert(start < end);
            this.text = text;
            this.start = start;
            this.end = end;
            this.positions = positions;
            this.style = style;
            this.bounds = bounds;
        }

        public Rect boundsInText {
            get { return this.bounds.shift(new Offset(this.positions[this.start].x, this.positions[this.start].y)); }
        }

        public readonly string text;
        public readonly int start;
        public readonly int end;
        public readonly Vector2d[] positions;
        public readonly TextStyle style;
        public readonly Rect bounds; // bounds with positions[start] as origin       
    }
}
