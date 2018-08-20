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

        public static Size operator *(Size a, double operand) {
            return new Size(a.width * operand, a.height * operand);
        }

        public static Size operator /(Size a, double operand) {
            return new Size(a.width / operand, a.height / operand);
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

        public static readonly Rect infinity = new Rect(double.NegativeInfinity, double.NegativeInfinity,
            double.PositiveInfinity, double.PositiveInfinity);

        public const double _giantScalar = 1.0E+9;

        public static readonly Rect largest =
            Rect.fromLTRB(-Rect._giantScalar, -Rect._giantScalar, Rect._giantScalar, Rect._giantScalar);

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

        public Rect intersect(Rect other) {
            return Rect.fromLTRB(
                Math.Max(this.left, other.left),
                Math.Max(this.top, other.top),
                Math.Min(this.right, other.right),
                Math.Min(this.bottom, other.bottom)
            );
        }

        public Rect expandToInclude(Rect other) {
            return Rect.fromLTRB(
                Math.Min(this.left, other.left),
                Math.Min(this.top, other.top),
                Math.Max(this.right, other.right),
                Math.Max(this.bottom, other.bottom)
            );
        }

        public bool overlaps(Rect other) {
            if (this.right <= other.left || other.right <= this.left) {
                return false;
            }

            if (this.bottom <= other.top || other.bottom <= this.top) {
                return false;
            }

            return true;
        }


        public double shortestSide {
            get { return Math.Min(Math.Abs(this.width), Math.Abs(this.height)); }
        }

        public double longestSide {
            get { return Math.Max(Math.Abs(this.width), Math.Abs(this.height)); }
        }

        public Offset topLeft {
            get { return new Offset(this.left, this.top); }
        }

        public Offset topCenter {
            get { return new Offset(this.left + this.width / 2.0, this.top); }
        }

        public Offset topRight {
            get { return new Offset(this.right, this.top); }
        }

        public Offset centerLeft {
            get { return new Offset(this.left, this.top + this.height / 2.0); }
        }

        public Offset center {
            get { return new Offset(this.left + this.width / 2.0, this.top + this.height / 2.0); }
        }

        public Offset centerRight {
            get { return new Offset(this.right, this.bottom); }
        }

        public Offset bottomLeft {
            get { return new Offset(this.left, this.bottom); }
        }

        public Offset bottomCenter {
            get { return new Offset(this.left + this.width / 2.0, this.bottom); }
        }

        public Offset bottomRight {
            get { return new Offset(this.right, this.bottom); }
        }

        public bool contains(Offset offset) {
            return offset.dx >= this.left && offset.dx < this.right && offset.dy >= this.top && offset.dy < this.bottom;
        }
        
        public bool contains(Rect rect) {
            return this.contains(rect.topLeft) && this.contains(rect.bottomRight);
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

    public class RRect : IEquatable<RRect> {
        private RRect(double left, double top, double right, double bottom,
            double tlRadius, double trRadius, double brRadius, double blRadius) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.tlRadius = tlRadius;
            this.trRadius = trRadius;
            this.brRadius = brRadius;
            this.blRadius = blRadius;
        }

        public static RRect fromLTRBAndRadius(
            double left, double top, double right, double bottom,
            double radius) {
            return new RRect(left, top, right, bottom,
                radius, radius, radius, radius);
        }

        public static RRect fromRectAndRadius(Rect rect, double radius) {
            return new RRect(rect.left, rect.top, rect.right, rect.bottom,
                radius, radius, radius, radius);
        }

        public static RRect fromLTRBAndCorners(
            double left, double top, double right, double bottom,
            double topLeft = 0.0, double topRight = 0.0, double bottomRight = 0.0, double bottomLeft = 0.0) {
            return new RRect(left, top, right, bottom,
                topLeft, topRight, bottomRight, bottomLeft);
        }

        public static RRect fromRectAndCorners(
            Rect rect,
            double topLeft = 0.0, double topRight = 0.0, double bottomRight = 0.0, double bottomLeft = 0.0) {
            return new RRect(rect.left, rect.top, rect.right, rect.bottom,
                topLeft, topRight, bottomRight, bottomLeft);
        }

        public static readonly RRect zero = new RRect(0, 0, 0, 0, 0, 0, 0, 0);

        public readonly double left;
        public readonly double top;
        public readonly double right;
        public readonly double bottom;

        public readonly double tlRadius;
        public readonly double trRadius;
        public readonly double brRadius;
        public readonly double blRadius;

        public RRect shift(Offset offset) {
            return RRect.fromLTRBAndCorners(
                this.left + offset.dx,
                this.top + offset.dy,
                this.right + offset.dx,
                this.bottom + offset.dy,
                this.tlRadius,
                this.trRadius,
                this.brRadius,
                this.blRadius
            );
        }

        public RRect inflate(double delta) {
            return RRect.fromLTRBAndCorners(
                this.left - delta,
                this.top - delta,
                this.right + delta,
                this.bottom + delta,
                this.tlRadius + delta,
                this.trRadius + delta,
                this.brRadius + delta,
                this.blRadius + delta
            );
        }

        public RRect deflate(double delta) {
            return this.inflate(-delta);
        }

        public double width {
            get { return this.right - this.left; }
        }

        public double height {
            get { return this.bottom - this.top; }
        }

        public Rect outerRect {
            get { return Rect.fromLTRB(this.left, this.top, this.right, this.bottom); }
        }

        public Rect safeInnerRect {
            get {
                const double kInsetFactor = 0.29289321881; // 1-cos(pi/4)

                double leftRadius = Math.Max(this.blRadius, this.tlRadius);
                double topRadius = Math.Max(this.tlRadius, this.trRadius);
                double rightRadius = Math.Max(this.trRadius, this.brRadius);
                double bottomRadius = Math.Max(this.brRadius, this.blRadius);

                return Rect.fromLTRB(
                    this.left + leftRadius * kInsetFactor,
                    this.top + topRadius * kInsetFactor,
                    this.right - rightRadius * kInsetFactor,
                    this.bottom - bottomRadius * kInsetFactor
                );
            }
        }

        public Rect middleRect {
            get {
                double leftRadius = Math.Max(this.blRadius, this.tlRadius);
                double topRadius = Math.Max(this.tlRadius, this.trRadius);
                double rightRadius = Math.Max(this.trRadius, this.brRadius);
                double bottomRadius = Math.Max(this.brRadius, this.blRadius);

                return Rect.fromLTRB(
                    this.left + leftRadius,
                    this.top + topRadius,
                    this.right - rightRadius,
                    this.bottom - bottomRadius
                );
            }
        }

        public Rect wideMiddleRect {
            get {
                double topRadius = Math.Max(this.tlRadius, this.trRadius);
                double bottomRadius = Math.Max(this.brRadius, this.blRadius);

                return Rect.fromLTRB(
                    this.left,
                    this.top + topRadius,
                    this.right,
                    this.bottom - bottomRadius
                );
            }
        }

        public Rect tallMiddleRect {
            get {
                double leftRadius = Math.Max(this.blRadius, this.tlRadius);
                double rightRadius = Math.Max(this.trRadius, this.brRadius);

                return Rect.fromLTRB(
                    this.left + leftRadius,
                    this.top,
                    this.right - rightRadius,
                    this.bottom
                );
            }
        }

        public bool isEmpty {
            get { return this.left >= this.right || this.top >= this.bottom; }
        }

        public bool isFinite {
            get {
                return !Double.IsInfinity(this.left)
                       && !Double.IsInfinity(this.top)
                       && !Double.IsInfinity(this.right)
                       && !Double.IsInfinity(this.bottom);
            }
        }

        public bool isInfinite {
            get { return !this.isFinite; }
        }

        public bool isRect {
            get {
                return this.tlRadius == 0.0 &&
                       this.trRadius == 0.0 &&
                       this.blRadius == 0.0 &&
                       this.brRadius == 0.0;
            }
        }

        public bool isStadium {
            get {
                return this.tlRadius == this.trRadius
                       && this.trRadius == this.brRadius
                       && this.brRadius == this.blRadius
                       && (this.width <= 2.0 * this.tlRadius || this.height <= 2.0 * this.tlRadius);
            }
        }

        public bool isEllipse {
            get {
                return this.tlRadius == this.trRadius
                       && this.trRadius == this.brRadius
                       && this.brRadius == this.blRadius
                       && (this.width <= 2.0 * this.tlRadius && this.height <= 2.0 * this.tlRadius);
            }
        }

        public bool isCircle {
            get {
                return this.width == this.height
                       && this.isEllipse;
            }
        }

        public double shortestSide {
            get { return Math.Min(Math.Abs(this.width), Math.Abs(this.height)); }
        }

        public double longestSide {
            get { return Math.Max(Math.Abs(this.width), Math.Abs(this.height)); }
        }

        public Offset center {
            get { return new Offset(this.left + this.width / 2.0, this.top + this.height / 2.0); }
        }

        public bool Equals(RRect other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.left.Equals(other.left)
                   && this.top.Equals(other.top)
                   && this.right.Equals(other.right)
                   && this.bottom.Equals(other.bottom)
                   && this.tlRadius.Equals(other.tlRadius)
                   && this.trRadius.Equals(other.trRadius)
                   && this.brRadius.Equals(other.brRadius)
                   && this.blRadius.Equals(other.blRadius);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((RRect) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.left.GetHashCode();
                hashCode = (hashCode * 397) ^ this.top.GetHashCode();
                hashCode = (hashCode * 397) ^ this.right.GetHashCode();
                hashCode = (hashCode * 397) ^ this.bottom.GetHashCode();
                hashCode = (hashCode * 397) ^ this.tlRadius.GetHashCode();
                hashCode = (hashCode * 397) ^ this.trRadius.GetHashCode();
                hashCode = (hashCode * 397) ^ this.brRadius.GetHashCode();
                hashCode = (hashCode * 397) ^ this.blRadius.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(RRect a, RRect b) {
            return object.Equals(a, b);
        }

        public static bool operator !=(RRect a, RRect b) {
            return !(a == b);
        }
    }
}