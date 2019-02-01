using System;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public static class MathUtils {
        public static double clamp(this double value, double min, double max) {
            if (value < min) {
                value = min;
            }
            else if (value > max) {
                value = max;
            }

            return value;
        }

        public static float clamp(this float value, float min, float max) {
            if (value < min) {
                value = min;
            }
            else if (value > max) {
                value = max;
            }

            return value;
        }

        public static int clamp(this int value, int min, int max) {
            if (value < min) {
                value = min;
            }
            else if (value > max) {
                value = max;
            }

            return value;
        }

        public static double abs(this double value) {
            return Math.Abs(value);
        }

        public static float abs(this float value) {
            return Mathf.Abs(value);
        }

        public static int sign(this double value) {
            return Math.Sign(value);
        }

        public static int sign(this float value) {
            return (int) Mathf.Sign(value);
        }

        public static bool isInfinite(this double it) {
            return double.IsInfinity(it);
        }

        public static bool isFinite(this double it) {
            return !double.IsInfinity(it);
        }

        public static bool isNaN(this double it) {
            return double.IsNaN(it);
        }

        public static double lerpDouble(double a, double b, double t) {
            return a + (b - a) * t;
        }

        public static double? lerpNullableDouble(double? a, double? b, double t) {
            if (a == null && b == null) {
                return null;
            }

            a = a ?? b;
            b = b ?? a;
            return (double) a + ((double) b - (double) a) * t;
        }

        public static float lerpFloat(float a, float b, float t) {
            return a + (b - a) * t;
        }

        public static int round(this double value) {
            return (int) Math.Round(value);
        }

        public static int floor(this double value) {
            return (int) Math.Floor(value);
        }

        public static int ceil(this double value) {
            return (int) Math.Ceiling(value);
        }

        public static int round(this float value) {
            return Mathf.RoundToInt(value);
        }

        public static int floor(this float value) {
            return Mathf.FloorToInt(value);
        }

        public static int ceil(this float value) {
            return Mathf.CeilToInt(value);
        }
    }

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

        public bool Equals(OffsetBase other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this._dx.Equals(other._dx) && this._dy.Equals(other._dy);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((OffsetBase) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this._dx.GetHashCode() * 397) ^ this._dy.GetHashCode();
            }
        }

        public static bool operator ==(OffsetBase left, OffsetBase right) {
            return Equals(left, right);
        }

        public static bool operator !=(OffsetBase left, OffsetBase right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}({this._dx:F1}, {this._dy:F1})";
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

        public Offset scale(double scaleX, double? scaleY = null) {
            scaleY = scaleY ?? scaleX;
            return new Offset(this.dx * scaleX, this.dy * scaleY.Value);
        }

        public Offset translate(double translateX, double translateY) {
            return new Offset(this.dx + translateX, this.dy + translateY);
        }

        public static Offset operator -(Offset a) {
            return new Offset(-a.dx, -a.dy);
        }

        public static Offset operator -(Offset a, Offset b) {
            return new Offset(a.dx - b.dx, a.dy - b.dy);
        }

        public static Offset operator +(Offset a, Offset b) {
            return new Offset(a.dx + b.dx, a.dy + b.dy);
        }

        public static Offset operator *(Offset a, double operand) {
            return new Offset(a.dx * operand, a.dy * operand);
        }

        public static Offset operator /(Offset a, double operand) {
            return new Offset(a.dx / operand, a.dy / operand);
        }

        public static Rect operator &(Offset a, Size other) {
            return Rect.fromLTWH(a.dx, a.dy, other.width, other.height);
        }

        public static Offset lerp(Offset a, Offset b, double t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b * t;
            }

            if (b == null) {
                return a * (1.0 - t);
            }

            return new Offset(MathUtils.lerpDouble(a.dx, b.dx, t), MathUtils.lerpDouble(a.dy, b.dy, t));
        }

        public bool Equals(Offset other) {
            return base.Equals(other);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((Offset) obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public static bool operator ==(Offset left, Offset right) {
            return Equals(left, right);
        }

        public static bool operator !=(Offset left, Offset right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"Offset({this._dx:F1}, {this._dy:F1})";
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

        public static Size operator -(Size a, Offset b) {
            return new Size(a.width - b.dx, a.height - b.dy);
        }

        public static Size operator +(Size a, Offset b) {
            return new Size(a.width + b.dx, a.height + b.dy);
        }

        public static Offset operator -(Size a, Size b) {
            return new Offset(a.width - b.width, a.height - b.height);
        }

        public static Size operator *(Size a, double operand) {
            return new Size(a.width * operand, a.height * operand);
        }

        public static Size operator /(Size a, double operand) {
            return new Size(a.width / operand, a.height / operand);
        }

        public double shortestSide {
            get { return Math.Min(this.width.abs(), this.height.abs()); }
        }

        public double longestSide {
            get { return Math.Max(this.width.abs(), this.height.abs()); }
        }

        public Offset topLeft(Offset origin) {
            return origin;
        }

        public Offset topCenter(Offset origin) {
            return new Offset(origin.dx + this.width / 2.0, origin.dy);
        }

        public Offset topRight(Offset origin) {
            return new Offset(origin.dx + this.width, origin.dy);
        }

        public Offset centerLeft(Offset origin) {
            return new Offset(origin.dx, origin.dy + this.height / 2.0);
        }

        public Offset center(Offset origin) {
            return new Offset(origin.dx + this.width / 2.0, origin.dy + this.height / 2.0);
        }

        public Offset centerRight(Offset origin) {
            return new Offset(origin.dx + this.width, origin.dy + this.height / 2.0);
        }

        public Offset bottomLeft(Offset origin) {
            return new Offset(origin.dx, origin.dy + this.height);
        }

        public Offset bottomCenter(Offset origin) {
            return new Offset(origin.dx + this.width / 2.0, origin.dy + this.height);
        }

        public Offset bottomRight(Offset origin) {
            return new Offset(origin.dx + this.width, origin.dy + this.height);
        }

        public bool contains(Offset offset) {
            return offset.dx >= 0.0 && offset.dx < this.width && offset.dy >= 0.0 && offset.dy < this.height;
        }

        public Size flipped {
            get { return new Size(this.height, this.width); }
        }

        public static Size lerp(Size a, Size b, double t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b * t;
            }

            if (b == null) {
                return a * (1.0 - t);
            }

            return new Size(MathUtils.lerpDouble(a.width, b.width, t), MathUtils.lerpDouble(a.height, b.height, t));
        }

        public bool Equals(Size other) {
            return base.Equals(other);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((Size) obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public static bool operator ==(Size left, Size right) {
            return Equals(left, right);
        }

        public static bool operator !=(Size left, Size right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"Size({this._dx:F1}, {this._dy:F1})";
        }
    }

    public class Rect : IEquatable<Rect> {
        Rect(double left, double top, double right, double bottom) {
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

        public static Rect fromCircle(Offset center, double radius) {
            return new Rect(center.dx - radius, center.dy - radius, center.dx + radius, center.dy + radius);
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

        public static readonly Rect one = new Rect(0, 0, 1, 1);

        public static readonly Rect infinity = new Rect(double.NegativeInfinity, double.NegativeInfinity,
            double.PositiveInfinity, double.PositiveInfinity);

        public const double _giantScalar = 1.0E+9;

        public static readonly Rect largest =
            fromLTRB(-_giantScalar, -_giantScalar, _giantScalar, _giantScalar);

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
            return fromLTRB(this.left + offset.dx, this.top + offset.dy, this.right + offset.dx,
                this.bottom + offset.dy);
        }

        public Rect translate(double translateX, double translateY) {
            return fromLTRB(this.left + translateX, this.top + translateY, this.right + translateX,
                this.bottom + translateY);
        }

        public Rect scale(double scaleX, double? scaleY = null) {
            scaleY = scaleY ?? scaleX;
            return fromLTRB(
                this.left * scaleX, this.top * scaleY.Value,
                this.right * scaleX, this.bottom * scaleY.Value);
        }

        public Rect inflate(double delta) {
            return fromLTRB(this.left - delta, this.top - delta, this.right + delta, this.bottom + delta);
        }

        public Rect deflate(double delta) {
            return this.inflate(-delta);
        }

        public Rect intersect(Rect other) {
            return fromLTRB(
                Math.Max(this.left, other.left),
                Math.Max(this.top, other.top),
                Math.Min(this.right, other.right),
                Math.Min(this.bottom, other.bottom)
            );
        }

        public Rect expandToInclude(Rect other) {
            return fromLTRB(
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

        public bool containsInclusive(Offset offset) {
            return offset.dx >= this.left && offset.dx <= this.right && offset.dy >= this.top &&
                   offset.dy <= this.bottom;
        }

        public bool contains(Rect rect) {
            return this.contains(rect.topLeft) && this.contains(rect.bottomRight);
        }

        public Rect round() {
            return fromLTRB(
                Math.Round(this.left), Math.Round(this.top),
                Math.Round(this.right), Math.Round(this.bottom));
        }

        public Rect roundOut() {
            return fromLTRB(
                Math.Floor(this.left), Math.Floor(this.top),
                Math.Ceiling(this.right), Math.Ceiling(this.bottom));
        }
        
        public Rect roundIn() {
            return fromLTRB(
                Math.Ceiling(this.left), Math.Ceiling(this.top),
                Math.Floor(this.right), Math.Floor(this.bottom));
        }

        public static Rect lerp(Rect a, Rect b, double t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return fromLTRB(b.left * t, b.top * t, b.right * t, b.bottom * t);
            }

            if (b == null) {
                double k = 1.0 - t;
                return fromLTRB(a.left * k, a.top * k, a.right * k, a.bottom * k);
            }

            return fromLTRB(
                MathUtils.lerpDouble(a.left, b.left, t),
                MathUtils.lerpDouble(a.top, b.top, t),
                MathUtils.lerpDouble(a.right, b.right, t),
                MathUtils.lerpDouble(a.bottom, b.bottom, t)
            );
        }

        public bool Equals(Rect other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.left.Equals(other.left) && this.top.Equals(other.top) && this.right.Equals(other.right) &&
                   this.bottom.Equals(other.bottom);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

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

        public static bool operator ==(Rect left, Rect right) {
            return Equals(left, right);
        }

        public static bool operator !=(Rect left, Rect right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return "Rect.fromLTRB(" + this.left.ToString("0.0") + ", " + this.top.ToString("0.0") + ", " +
                   this.right.ToString("0.0") + ", " + this.bottom.ToString("0.0") + ")";
        }
    }

    public class Radius : IEquatable<Radius> {
        Radius(double x, double y) {
            this.x = x;
            this.y = y;
        }

        public static Radius circular(double radius) {
            return elliptical(radius, radius);
        }

        public static Radius elliptical(double x, double y) {
            return new Radius(x, y);
        }

        public readonly double x;
        public readonly double y;

        public static readonly Radius zero = circular(0.0);

        public static Radius operator -(Radius a) {
            return elliptical(-a.x, -a.y);
        }

        public static Radius operator -(Radius a, Radius b) {
            return elliptical(a.x - b.x, a.y - b.y);
        }

        public static Radius operator -(Radius a, double b) {
            return elliptical(a.x - b, a.y - b);
        }

        public static Radius operator +(Radius a, Radius b) {
            return elliptical(a.x + b.x, a.y + b.y);
        }

        public static Radius operator +(Radius a, double b) {
            return elliptical(a.x + b, a.y + b);
        }

        public static Radius operator *(Radius a, Radius b) {
            return elliptical(a.x * b.x, a.y * b.y);
        }

        public static Radius operator *(Radius a, double b) {
            return elliptical(a.x * b, a.y * b);
        }

        public static Radius operator /(Radius a, Radius b) {
            return elliptical(a.x / b.x, a.y / b.y);
        }

        public static Radius operator /(Radius a, double b) {
            return elliptical(a.x / b, a.y / b);
        }

        public static Radius operator %(Radius a, Radius b) {
            return elliptical(a.x % b.x, a.y % b.y);
        }

        public static Radius operator %(Radius a, double b) {
            return elliptical(a.x % b, a.y % b);
        }

        public static Radius lerp(Radius a, Radius b, double t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return elliptical(b.x * t, b.y * t);
            }

            if (b == null) {
                double k = 1.0 - t;
                return elliptical(a.x * k, a.y * k);
            }

            return elliptical(
                MathUtils.lerpDouble(a.x, b.x, t),
                MathUtils.lerpDouble(a.y, b.y, t)
            );
        }

        public bool Equals(Radius other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.x.Equals(other.x) && this.y.Equals(other.y);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((Radius) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.x.GetHashCode() * 397) ^ this.y.GetHashCode();
            }
        }

        public static bool operator ==(Radius left, Radius right) {
            return Equals(left, right);
        }

        public static bool operator !=(Radius left, Radius right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return this.x == this.y
                ? $"Radius.circular({this.x:F1})"
                : $"Radius.elliptical({this.x:F1}, ${this.y:F1})";
        }
    }

    public class RRect : IEquatable<RRect> {
        RRect(double left, double top, double right, double bottom,
            Radius tlRadius = null, Radius trRadius = null, Radius brRadius = null, Radius blRadius = null) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.tlRadius = tlRadius ?? Radius.zero;
            this.trRadius = trRadius ?? Radius.zero;
            this.brRadius = brRadius ?? Radius.zero;
            this.blRadius = blRadius ?? Radius.zero;
        }

        RRect(double left, double top, double right, double bottom,
            double? tlRadius = null, double? trRadius = null, double? brRadius = null, double? blRadius = null) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.tlRadius = tlRadius != null ? Radius.circular(tlRadius.Value) : Radius.zero;
            this.trRadius = trRadius != null ? Radius.circular(trRadius.Value) : Radius.zero;
            this.brRadius = brRadius != null ? Radius.circular(brRadius.Value) : Radius.zero;
            this.blRadius = blRadius != null ? Radius.circular(blRadius.Value) : Radius.zero;
        }


        public static RRect fromLTRBXY(
            double left, double top, double right, double bottom,
            double radiusX, double radiusY) {
            var radius = Radius.elliptical(radiusX, radiusY);
            return new RRect(left, top, right, bottom,
                radius, radius, radius, radius);
        }


        public static RRect fromLTRBR(
            double left, double top, double right, double bottom, Radius radius) {
            return new RRect(left, top, right, bottom,
                radius, radius, radius, radius);
        }

        public static RRect fromLTRBR(
            double left, double top, double right, double bottom, double radius) {
            var r = Radius.circular(radius);
            return new RRect(left, top, right, bottom,
                r, r, r, r);
        }

        public static RRect fromRectXY(Rect rect, double radiusX, double radiusY) {
            var radius = Radius.elliptical(radiusX, radiusY);
            return new RRect(rect.left, rect.top, rect.right, rect.bottom,
                radius, radius, radius, radius);
        }

        public static RRect fromRect(Rect rect) {
            return new RRect(rect.left, rect.top, rect.right, rect.bottom, (Radius) null);
        }

        public static RRect fromRectAndRadius(Rect rect, Radius radius) {
            return new RRect(rect.left, rect.top, rect.right, rect.bottom,
                radius, radius, radius, radius);
        }

        public static RRect fromRectAndRadius(Rect rect, double radius) {
            var r = Radius.circular(radius);
            return new RRect(rect.left, rect.top, rect.right, rect.bottom,
                r, r, r, r);
        }

        public static RRect fromLTRBAndCorners(
            double left, double top, double right, double bottom,
            Radius topLeft = null, Radius topRight = null, Radius bottomRight = null, Radius bottomLeft = null) {
            return new RRect(left, top, right, bottom,
                topLeft, topRight, bottomRight, bottomLeft);
        }

        public static RRect fromLTRBAndCorners(
            double left, double top, double right, double bottom,
            double? topLeft = null, double? topRight = null, double? bottomRight = null, double? bottomLeft = null) {
            return new RRect(left, top, right, bottom,
                topLeft, topRight, bottomRight, bottomLeft);
        }

        public static RRect fromRectAndCorners(
            Rect rect,
            Radius topLeft = null, Radius topRight = null, Radius bottomRight = null, Radius bottomLeft = null) {
            return new RRect(rect.left, rect.top, rect.right, rect.bottom,
                topLeft, topRight, bottomRight, bottomLeft);
        }

        public static RRect fromRectAndCorners(
            Rect rect,
            double? topLeft = null, double? topRight = null, double? bottomRight = null, double? bottomLeft = null) {
            return new RRect(rect.left, rect.top, rect.right, rect.bottom,
                topLeft, topRight, bottomRight, bottomLeft);
        }

        public readonly double left;
        public readonly double top;
        public readonly double right;
        public readonly double bottom;

        public readonly Radius tlRadius;
        public readonly Radius trRadius;
        public readonly Radius brRadius;
        public readonly Radius blRadius;

        public double tlRadiusX {
            get { return this.tlRadius.x; }
        }

        public double tlRadiusY {
            get { return this.tlRadius.y; }
        }

        public double trRadiusX {
            get { return this.trRadius.x; }
        }

        public double trRadiusY {
            get { return this.trRadius.y; }
        }

        public double blRadiusX {
            get { return this.blRadius.x; }
        }

        public double blRadiusY {
            get { return this.blRadius.y; }
        }

        public double brRadiusX {
            get { return this.brRadius.x; }
        }

        public double brRadiusY {
            get { return this.brRadius.y; }
        }

        public static readonly RRect zero = new RRect(0, 0, 0, 0, (Radius) null);

        public RRect shift(Offset offset) {
            return fromLTRBAndCorners(
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
            return fromLTRBAndCorners(
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

                double leftRadius = Math.Max(this.blRadiusX, this.tlRadiusX);
                double topRadius = Math.Max(this.tlRadiusY, this.trRadiusY);
                double rightRadius = Math.Max(this.trRadiusX, this.brRadiusX);
                double bottomRadius = Math.Max(this.brRadiusY, this.blRadiusY);

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
                double leftRadius = Math.Max(this.blRadiusX, this.tlRadiusX);
                double topRadius = Math.Max(this.tlRadiusY, this.trRadiusY);
                double rightRadius = Math.Max(this.trRadiusX, this.brRadiusX);
                double bottomRadius = Math.Max(this.brRadiusY, this.blRadiusY);

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
                double topRadius = Math.Max(this.tlRadiusY, this.trRadiusY);
                double bottomRadius = Math.Max(this.brRadiusY, this.blRadiusY);

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
                double leftRadius = Math.Max(this.blRadiusX, this.tlRadiusX);
                double rightRadius = Math.Max(this.trRadiusX, this.brRadiusX);

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
                return this.left.isFinite()
                       && this.top.isFinite()
                       && this.right.isFinite()
                       && this.bottom.isFinite();
            }
        }

        public bool isInfinite {
            get { return !this.isFinite; }
        }

        public bool isRect {
            get {
                return this.tlRadius == Radius.zero &&
                       this.trRadius == Radius.zero &&
                       this.blRadius == Radius.zero &&
                       this.brRadius == Radius.zero;
            }
        }

        public bool isStadium {
            get {
                return this.tlRadius == this.trRadius
                       && this.trRadius == this.brRadius
                       && this.brRadius == this.blRadius
                       && (this.width <= 2.0 * this.tlRadiusX || this.height <= 2.0 * this.tlRadiusY);
            }
        }

        public bool isEllipse {
            get {
                return this.tlRadius == this.trRadius
                       && this.trRadius == this.brRadius
                       && this.brRadius == this.blRadius
                       && (this.width <= 2.0 * this.tlRadiusX && this.height <= 2.0 * this.tlRadiusY);
            }
        }

        public bool isCircle {
            get { return this.width == this.height && this.isEllipse; }
        }

        public double shortestSide {
            get { return Math.Min(this.width.abs(), this.height.abs()); }
        }

        public double longestSide {
            get { return Math.Max(this.width.abs(), this.height.abs()); }
        }

        public Offset center {
            get { return new Offset(this.left + this.width / 2.0, this.top + this.height / 2.0); }
        }

        double _getMin(double min, double radius1, double radius2, double limit) {
            double sum = radius1 + radius2;
            if (sum > limit && sum != 0.0) {
                return Math.Min(min, limit / sum);
            }

            return min;
        }

        RRect _scaled;

        void _scaleRadii() {
            if (this._scaled == null) {
                double scale = 1.0;

                scale = this._getMin(scale, this.blRadiusY, this.tlRadiusY, this.height);
                scale = this._getMin(scale, this.tlRadiusX, this.trRadiusX, this.width);
                scale = this._getMin(scale, this.trRadiusY, this.brRadiusY, this.height);
                scale = this._getMin(scale, this.brRadiusX, this.blRadiusX, this.width);

                if (scale < 1.0) {
                    this._scaled = fromLTRBAndCorners(
                        left: this.left, top: this.top, right: this.right, bottom: this.bottom,
                        topLeft: this.tlRadius * scale, topRight: this.trRadius * scale,
                        bottomRight: this.brRadius * scale, bottomLeft: this.blRadius * scale);
                }
                else {
                    this._scaled = this;
                }
            }
        }

        public bool contains(Offset point) {
            if (point.dx < this.left || point.dx >= this.right || point.dy < this.top || point.dy >= this.bottom) {
                return false;
            }

            this._scaleRadii();

            double x;
            double y;
            double radiusX;
            double radiusY;

            if (point.dx < this.left + this._scaled.tlRadiusX &&
                point.dy < this.top + this._scaled.tlRadiusY) {
                x = point.dx - this.left - this._scaled.tlRadiusX;
                y = point.dy - this.top - this._scaled.tlRadiusY;
                radiusX = this._scaled.tlRadiusX;
                radiusY = this._scaled.tlRadiusY;
            }
            else if (point.dx > this.right - this._scaled.trRadiusX &&
                     point.dy < this.top + this._scaled.trRadiusY) {
                x = point.dx - this.right + this._scaled.trRadiusX;
                y = point.dy - this.top - this._scaled.trRadiusY;
                radiusX = this._scaled.trRadiusX;
                radiusY = this._scaled.trRadiusY;
            }
            else if (point.dx > this.right - this._scaled.brRadiusX &&
                     point.dy > this.bottom - this._scaled.brRadiusY) {
                x = point.dx - this.right + this._scaled.brRadiusX;
                y = point.dy - this.bottom + this._scaled.brRadiusY;
                radiusX = this._scaled.brRadiusX;
                radiusY = this._scaled.brRadiusY;
            }
            else if (point.dx < this.left + this._scaled.blRadiusX &&
                     point.dy > this.bottom - this._scaled.blRadiusY) {
                x = point.dx - this.left - this._scaled.blRadiusX;
                y = point.dy - this.bottom + this._scaled.blRadiusY;
                radiusX = this._scaled.blRadiusX;
                radiusY = this._scaled.blRadiusY;
            }
            else {
                return true;
            }

            x = x / radiusX;
            y = y / radiusY;
            if (x * x + y * y > 1.0) {
                return false;
            }

            return true;
        }

        public static RRect lerp(RRect a, RRect b, double t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return fromLTRBAndCorners(
                    b.left * t,
                    b.top * t,
                    b.right * t,
                    b.bottom * t,
                    b.tlRadius * t,
                    b.trRadius * t,
                    b.brRadius * t,
                    b.blRadius * t
                );
            }

            if (b == null) {
                double k = 1.0 - t;
                return fromLTRBAndCorners(
                    a.left * k,
                    a.top * k,
                    a.right * k,
                    a.bottom * k,
                    a.tlRadius * k,
                    a.trRadius * k,
                    a.brRadius * k,
                    a.blRadius * k);
            }

            return fromLTRBAndCorners(
                MathUtils.lerpDouble(a.left, b.left, t),
                MathUtils.lerpDouble(a.top, b.top, t),
                MathUtils.lerpDouble(a.right, b.right, t),
                MathUtils.lerpDouble(a.bottom, b.bottom, t),
                Radius.lerp(a.tlRadius, b.tlRadius, t),
                Radius.lerp(a.trRadius, b.trRadius, t),
                Radius.lerp(a.brRadius, b.brRadius, t),
                Radius.lerp(a.blRadius, b.blRadius, t));
        }

        public bool contains(Rect rect) {
            if (!this.outerRect.contains(rect)) {
                return false;
            }

            if (this.isRect) {
                return true;
            }

            return this.contains(rect.topLeft) &&
                   this.contains(rect.topRight) &&
                   this.contains(rect.bottomRight) &&
                   this.contains(rect.bottomLeft);
        }

        public bool Equals(RRect other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

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
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

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
            return Equals(a, b);
        }

        public static bool operator !=(RRect a, RRect b) {
            return !(a == b);
        }

        public override string ToString() {
            string rect = $"{this.left:F1)}, " +
                          $"{this.top:F1}, " +
                          $"{this.right:F1}, " +
                          $"{this.bottom:F1}";

            if (this.tlRadius == this.trRadius &&
                this.trRadius == this.brRadius &&
                this.brRadius == this.blRadius) {
                if (this.tlRadius.x == this.tlRadius.y) {
                    return $"RRect.fromLTRBR({rect}, {this.tlRadius.x:F1})";
                }

                return $"RRect.fromLTRBXY($rect, {this.tlRadius.x:F1}, {this.tlRadius.y:F1})";
            }

            return "RRect.fromLTRBAndCorners(" +
                   $"{rect}, " +
                   $"topLeft: {this.tlRadius}, " +
                   $"topRight: {this.trRadius}, " +
                   $"bottomRight: {this.brRadius}, " +
                   $"bottomLeft: {this.blRadius}" +
                   ")";
        }
    }
}