using UIWidgets.ui;

namespace UIWidgets.painting
{
    // todo should be in text.cs
    public enum TextDirection
    {
        /// The text flows from right to left (e.g. Arabic, Hebrew).
        rtl,

        /// The text flows from left to right (e.g., English, French).
        ltr,
    }

    public abstract class AlignmentGeometry
    {
        public AlignmentGeometry()
        {
        }

        public abstract double x { get; }
        public abstract double start { get; }
        public abstract double y { get; }

        AlignmentGeometry add(AlignmentGeometry other)
        {
            return new _MixedAlignment(
                x + other.x,
                start + other.start,
                y + other.y
            );
        }

        public virtual Alignment resolve(TextDirection direction)
        {
            return null;
        }
    }

    public class Alignment : AlignmentGeometry
    {
        public Alignment(double x, double y)
        {
            this._x = x;
            this._y = y;
        }

        private readonly double _x;

        public override double x
        {
            get { return _x; }
        }

        private readonly double _y;

        public override double y
        {
            get { return _y; }
        }

        public override double start
        {
            get { return 0.0; }
        }

        public static readonly Alignment topLeft = new Alignment(-1.0, -1.0);
        public static readonly Alignment topCenter = new Alignment(0.0, -1.0);
        public static readonly Alignment topRight = new Alignment(1.0, -1.0);
        public static readonly Alignment centerLeft = new Alignment(-1.0, 0.0);
        public static readonly Alignment center = new Alignment(0.0, 0.0);
        public static readonly Alignment centerRight = new Alignment(1.0, 0.0);
        public static readonly Alignment bottomLeft = new Alignment(-1.0, 1.0);
        public static readonly Alignment bottomCenter = new Alignment(0.0, 1.0);
        public static readonly Alignment bottomRight = new Alignment(1.0, 1.0);

        public static Alignment operator -(Alignment a, Alignment b)
        {
            return new Alignment(a._x - b._x, a._y - b._y);
        }

        // todo more operators

        public override Alignment resolve(TextDirection direction)
        {
            return this;
        }

        public Rect inscribe(Size size, Rect rect)
        {
            double halfWidthDelta = (rect.width - size.width) / 2.0;
            double halfHeightDelta = (rect.height - size.height) / 2.0;
            return Rect.fromLTWH(
                rect.left + halfWidthDelta + _x * halfWidthDelta,
                rect.top + halfHeightDelta + _y * halfHeightDelta,
                size.width,
                size.height
            );
        }
    }

    public class _MixedAlignment : AlignmentGeometry
    {
        public _MixedAlignment(double x, double start, double y)
        {
            this._x = x;
            this._start = start;
            this._y = y;
        }

        private readonly double _x;

        public override double x
        {
            get { return _x; }
        }

        private readonly double _start;

        public override double start
        {
            get { return _start; }
        }

        private readonly double _y;

        public override double y
        {
            get { return _y; }
        }


        public static _MixedAlignment operator -(_MixedAlignment a)
        {
            return new _MixedAlignment(
                -a._x,
                -a._start,
                -a._y
            );
        }

        public static _MixedAlignment operator *(_MixedAlignment a, double other)
        {
            return new _MixedAlignment(
                a._x * other,
                a._start * other,
                a._y * other
            );
        }

        public static _MixedAlignment operator /(_MixedAlignment a, double other)
        {
            return new _MixedAlignment(
                a._x / other,
                a._start / other,
                a._y / other
            );
        }

        public static _MixedAlignment operator %(_MixedAlignment a, double other)
        {
            return new _MixedAlignment(
                a._x % other,
                a._start % other,
                a._y % other
            );
        }

        public override Alignment resolve(TextDirection direction)
        {
            switch (direction)
            {
                case TextDirection.rtl:
                    return new Alignment(_x - _start, _y);
                case TextDirection.ltr:
                    return new Alignment(_x + _start, _y);
            }

            return null;
        }
    }
}