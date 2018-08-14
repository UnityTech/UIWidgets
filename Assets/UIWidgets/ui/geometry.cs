using System;

namespace UIWidgets.ui {
    public abstract class OffsetBase : IEquatable<OffsetBase> {
        protected OffsetBase(double _dx, double _dy) {
            this._dx = _dx;
            this._dy = _dy;
        }

        public readonly double _dx;

        public readonly double _dy;

        public bool isInfinite {
            get { return double.IsInfinity(this._dx) || double.IsInfinity(this._dy); }
        }

        public bool isFinite {
            get { return !double.IsInfinity(this._dx) && !double.IsInfinity(this._dy); }
        }

        public static bool operator <(OffsetBase a, OffsetBase b) {
            return a._dx < b._dx && a._dy < b._dy;
        }

        public static bool operator <=(OffsetBase a, OffsetBase b) {
            return a._dx <= b._dx && a._dy <= b._dy;
        }

        public static bool operator >(OffsetBase a, OffsetBase b) {
            return a._dx > b._dx && a._dy > b._dy;
        }

        public static bool operator >=(OffsetBase a, OffsetBase b) {
            return a._dx >= b._dx && a._dy >= b._dy;
        }

        public static bool operator ==(OffsetBase a, OffsetBase b) {
            return a.Equals(b);
        }

        public static bool operator !=(OffsetBase a, OffsetBase b) {
            return !(a == b);
        }

        public bool Equals(OffsetBase other) {
            return this._dx.Equals(other._dx) && this._dy.Equals(other._dy);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            return obj is OffsetBase && this.Equals((OffsetBase) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this._dx.GetHashCode() * 397) ^ this._dy.GetHashCode();
            }
        }

        public override string ToString() {
            return this.GetType() + "(" + this._dx.ToString("0.0") + ", " + this._dy.ToString("0.0") + ")";
        }
    }

    public class Offset : OffsetBase, IEquatable<Offset> {
        public Offset(double dx, double dy) : base(dx, dy) {
        }

        public double dx {
            get { return this._dx; }
        }

        public double dy {
            get { return this._dy; }
        }

        public double distance {
            get { return Math.Sqrt(this._dx * this._dx + this._dy * this._dy); }
        }

        public double distanceSquared {
            get { return this._dx * this._dx + this._dy * this._dy; }
        }

        public static readonly Offset zero = new Offset(0.0, 0.0);
        public static readonly Offset infinite = new Offset(double.PositiveInfinity, double.PositiveInfinity);

        public Offset scale(double scaleX, double scaleY) {
            return new Offset(this.dx * scaleX, this.dy * scaleY);
        }

        public Offset translate(double translateX, double translateY) {
            return new Offset(this.dx + translateX, this.dy + translateY);
        }

        public static Offset operator -(Offset a, Offset b) {
            return new Offset(a.dx - b.dx, a.dy - b.dy);
        }

        public static Offset operator +(Offset a, Offset b) {
            return new Offset(a.dx + b.dx, a.dy + b.dy);
        }

        public static Rect operator &(Offset a, Size other) {
            return Rect.fromLTWH(a.dx, a.dy, other.width, other.height);
        }

        public bool Equals(Offset other) {
            return this._dx.Equals(other._dx) && this._dy.Equals(other._dy);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            return obj is Offset && this.Equals((Offset) obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override string ToString() {
            return "Offset(" + this._dx.ToString("0.0") + ", " + this._dy.ToString("0.0") + ")";
        }
    }

    public class Size : OffsetBase, IEquatable<Size> {
        public Size(double width, double height) : base(width, height) {
        }

        public static Size copy(Size source) {
            return new Size(source.width, source.height);
        }

        public static Size square(double dimension) {
            return new Size(dimension, dimension);
        }

        public static Size fromWidth(double width) {
            return new Size(width, double.PositiveInfinity);
        }

        public static Size fromHeight(double height) {
            return new Size(double.PositiveInfinity, height);
        }

        public static Size fromRadius(double radius) {
            return new Size(radius * 2, radius * 2);
        }

        public double width {
            get { return this._dx; }
        }

        public double height {
            get { return this._dy; }
        }

        public static readonly Size zero = new Size(0.0, 0.0);

        public static readonly Size infinite = new Size(double.PositiveInfinity, double.PositiveInfinity);

        public bool isEmpty {
            get { return this.width <= 0.0 || this.height <= 0.0; }
        }

        public double shortestSide {
            get { return Math.Min(Math.Abs(this.width), Math.Abs(this.height)); }
        }

        public double longestSide {
            get { return Math.Max(Math.Abs(this.width), Math.Abs(this.height)); }
        }

        public bool Equals(Size other) {
            return this._dx.Equals(other._dx) && this._dy.Equals(other._dy);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            return obj is Size && this.Equals((Size) obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override string ToString() {
            return "Size(" + this._dx.ToString("0.0") + ", " + this._dy.ToString("0.0") + ")";
        }
    }

    public class Radius : IEquatable<Radius> {
        private Radius(double x, double y) {
            this.x = x;
            this.y = y;
        }

        public readonly double x;

        public readonly double y;

        public static Radius circular(double radius) {
            return new Radius(radius, radius);
        }

        public static Radius elliptical(double x, double y) {
            return new Radius(x, y);
        }

        public static readonly Radius zero = Radius.circular(0);

        public static Radius operator -(Radius a) {
            return new Radius(-a.x, -a.y);
        }

        public static Radius operator -(Radius a, Radius b) {
            return new Radius(a.x - b.x, a.y - b.y);
        }

        public static Radius operator +(Radius a, Radius b) {
            return new Radius(a.x + b.x, a.y + b.y);
        }

        public static Radius operator *(Radius a, double operand) {
            return new Radius(a.x * operand, a.y * operand);
        }

        public static Radius operator /(Radius a, double operand) {
            return new Radius(a.x / operand, a.y / operand);
        }

        public static Radius operator %(Radius a, double operand) {
            return new Radius(a.x % operand, a.y % operand);
        }

        public bool Equals(Radius other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.x.Equals(other.x) && this.y.Equals(other.y);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Radius) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.x.GetHashCode() * 397) ^ this.y.GetHashCode();
            }
        }

        public override String ToString() {
            if (this.x == this.y) {
                return "Radius.circular(" + this.x.ToString("0.0") + ")";
            }

            return "Radius.elliptical(" + this.x.ToString("0.0") + ", " + this.y.ToString("0.0") + ")";
        }
    }


    public class Rect : IEquatable<Rect> {
        private Rect(double left, double top, double right, double bottom) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public static Rect fromLTRB(double left, double top, double right, double bottom) {
            return new Rect(left, top, right, bottom);
        }

        public static Rect fromLTWH(double left, double top, double width, double height) {
            return new Rect(left, top, left + width, top + height);
        }

        public static Rect fromCircle(Offset offset, double radius) {
            return new Rect(offset.dx - radius, offset.dy - radius, offset.dx + radius, offset.dy + radius);
        }

        public static Rect fromPoints(Offset a, Offset b) {
            return new Rect(
                Math.Min(a.dx, b.dx),
                Math.Min(a.dy, b.dy),
                Math.Max(a.dx, b.dx),
                Math.Max(a.dy, b.dy)
            );
        }


        public readonly double left;
        public readonly double top;
        public readonly double right;
        public readonly double bottom;

        public double width {
            get { return this.right - this.left; }
        }

        public double height {
            get { return this.bottom - this.top; }
        }

        public Size size {
            get { return new Size(this.width, this.height); }
        }

        public static readonly Rect zero = new Rect(0, 0, 0, 0);

        public const double _giantScalar = 1.0E+9;

        public static readonly Rect largest = Rect.fromLTRB(-_giantScalar, -_giantScalar, _giantScalar, _giantScalar);

        public bool isInfinite {
            get {
                return double.IsInfinity(this.left)
                       || double.IsInfinity(this.top)
                       || double.IsInfinity(this.right)
                       || double.IsInfinity(this.bottom);
            }
        }

        public bool isFinite {
            get { return !this.isInfinite; }
        }

        public bool isEmpty {
            get { return this.left >= this.right || this.top >= this.bottom; }
        }

        public Rect shift(Offset offset) {
            return Rect.fromLTRB(this.left + offset.dx, this.top + offset.dy, this.right + offset.dx,
                this.bottom + offset.dy);
        }

        public Rect inflate(double delta) {
            return Rect.fromLTRB(this.left - delta, this.top - delta, this.right + delta, this.bottom + delta);
        }

        public bool contains(Offset offset) {
            return offset.dx >= this.left && offset.dx < this.right && offset.dy >= this.top && offset.dy < this.bottom;
        }        

        public bool Equals(Rect other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.left.Equals(other.left) && this.top.Equals(other.top) && this.right.Equals(other.right) &&
                   this.bottom.Equals(other.bottom);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Rect) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.left.GetHashCode();
                hashCode = (hashCode * 397) ^ this.top.GetHashCode();
                hashCode = (hashCode * 397) ^ this.right.GetHashCode();
                hashCode = (hashCode * 397) ^ this.bottom.GetHashCode();
                return hashCode;
            }
        }

        public override String ToString() {
            return "Rect.fromLTRB(" + this.left.ToString("0.0") + ", " + this.top.ToString("0.0") + ", " +
                   this.right.ToString("0.0") + ", " + this.bottom.ToString("0.0") + ")";
        }
    }
}