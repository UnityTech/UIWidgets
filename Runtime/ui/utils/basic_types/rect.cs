using System;
using UnityEngine;

namespace Unity.UIWidgets.ui {

    public struct uiRect {

        public uiRect(float left, float top, float right, float bottom) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }
        
        public readonly float left;
        public readonly float top;
        public readonly float right;
        public readonly float bottom;
        
        public bool isEmpty {
            get { return this.left >= this.right || this.top >= this.bottom; }
        }
        
        public float width {
            get { return this.right - this.left; }
        }

        public float height {
            get { return this.bottom - this.top; }
        }
        
        public Offset topLeft {
            get { return new Offset(this.left, this.top); }
        }

        public Offset topCenter {
            get { return new Offset(this.left + this.width / 2.0f, this.top); }
        }

        public Offset topRight {
            get { return new Offset(this.right, this.top); }
        }

        public Offset centerLeft {
            get { return new Offset(this.left, this.top + this.height / 2.0f); }
        }

        public Offset center {
            get { return new Offset(this.left + this.width / 2.0f, this.top + this.height / 2.0f); }
        }

        public Offset centerRight {
            get { return new Offset(this.right, this.bottom); }
        }

        public Offset bottomLeft {
            get { return new Offset(this.left, this.bottom); }
        }

        public Offset bottomCenter {
            get { return new Offset(this.left + this.width / 2.0f, this.bottom); }
        }

        public Offset bottomRight {
            get { return new Offset(this.right, this.bottom); }
        }
    }

    public static class uiRectHelper {
        public static uiRect fromRect(Rect rect) {
            return new uiRect(rect.left, rect.top, rect.right, rect.bottom);
        }
        
        public static uiRect fromLTRB(float left, float top, float right, float bottom) {
            return new uiRect(left, top, right, bottom);
        }
        
        public static uiRect fromLTWH(float left, float top, float width, float height) {
            return new uiRect(left, top, left + width, top + height);
        }
        
        public static readonly uiRect zero = new uiRect(0, 0, 0, 0);

        public static readonly uiRect one = new uiRect(0, 0, 1, 1);

        public static bool equals(uiRect? a, uiRect? b) {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            var aval = a.Value;
            var bval = b.Value;
            
            return aval.left == bval.left && aval.right == bval.right && aval.top == bval.top && aval.bottom == bval.bottom;
        }
        
        public static uiRect scale(uiRect a, float scaleX, float? scaleY = null) {
            scaleY = scaleY ?? scaleX;
            return fromLTRB(
                a.left * scaleX, a.top * scaleY.Value,
                a.right * scaleX, a.bottom * scaleY.Value);
        }
        
        public static uiRect inflate(uiRect a, float delta) {
            return fromLTRB(a.left - delta, a.top - delta, a.right + delta, a.bottom + delta);
        }

        public static uiRect deflate(uiRect a, float delta) {
            return inflate(a, -delta);
        }
        
        public static uiRect intersect(uiRect a, uiRect other) {
            return fromLTRB(
                Mathf.Max(a.left, other.left),
                Mathf.Max(a.top, other.top),
                Mathf.Min(a.right, other.right),
                Mathf.Min(a.bottom, other.bottom)
            );
        }
        
        public static uiRect round(uiRect a) {
            return fromLTRB(
                Mathf.Round(a.left), Mathf.Round(a.top),
                Mathf.Round(a.right), Mathf.Round(a.bottom));
        }

        public static uiRect roundOut(uiRect a) {
            return fromLTRB(
                Mathf.Floor(a.left), Mathf.Floor(a.top),
                Mathf.Ceil(a.right), Mathf.Ceil(a.bottom));
        }

        public static uiRect roundOut(uiRect a, float devicePixelRatio) {
            return fromLTRB(
                Mathf.Floor(a.left * devicePixelRatio) / devicePixelRatio, 
                Mathf.Floor(a.top * devicePixelRatio) / devicePixelRatio,
                Mathf.Ceil(a.right * devicePixelRatio) / devicePixelRatio, 
                Mathf.Ceil(a.bottom * devicePixelRatio) / devicePixelRatio);
        }

        public static uiRect roundIn(uiRect a) {
            return fromLTRB(
                Mathf.Ceil(a.left), Mathf.Ceil(a.top),
                Mathf.Floor(a.right), Mathf.Floor(a.bottom));
        }
        
        public static uiRect normalize(uiRect a) {
            if (a.left <= a.right && a.top <= a.bottom) {
                return a;
            }

            return fromLTRB(
                Mathf.Min(a.left, a.right),
                Mathf.Min(a.top, a.bottom),
                Mathf.Max(a.left, a.right),
                Mathf.Max(a.top, a.bottom)
            );
        }
        
        
        public static Offset[] toQuad(uiRect a) {
            Offset[] dst = new Offset[4];
            dst[0] = new Offset(a.left, a.top);
            dst[1] = new Offset(a.right, a.top);
            dst[2] = new Offset(a.right, a.bottom);
            dst[3] = new Offset(a.left, a.bottom);
            return dst;
        }
        
        public static bool contains(uiRect a, Offset offset) {
            return offset.dx >= a.left && offset.dx < a.right && offset.dy >= a.top && offset.dy < a.bottom;
        }
        
        public static bool contains(uiRect a, uiRect rect) {
            return contains(a, rect.topLeft) && contains(a, rect.bottomRight);
        }
        
        public static bool overlaps(uiRect a, uiRect other) {
            if (a.right <= other.left || other.right <= a.left) {
                return false;
            }

            if (a.bottom <= other.top || other.bottom <= a.top) {
                return false;
            }

            return true;
        }
        
        public static UnityEngine.Rect toRect(uiRect rect) {
            return new UnityEngine.Rect(rect.left, rect.top, rect.width, rect.height);
        }
    }
    
    /*public struct uiRect : IEquatable<uiRect> {
        uiRect(float left, float top, float right, float bottom) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public static uiRect fromLTRB(float left, float top, float right, float bottom) {
            return new uiRect(left, top, right, bottom);
        }

        public static uiRect fromLTWH(float left, float top, float width, float height) {
            return new uiRect(left, top, left + width, top + height);
        }

        public static uiRect fromCircle(Offset center, float radius) {
            return new uiRect(center.dx - radius, center.dy - radius, center.dx + radius, center.dy + radius);
        }

        public static uiRect fromPoints(Offset a, Offset b) {
            return new uiRect(
                Mathf.Min(a.dx, b.dx),
                Mathf.Min(a.dy, b.dy),
                Mathf.Max(a.dx, b.dx),
                Mathf.Max(a.dy, b.dy)
            );
        }

        public readonly float left;
        public readonly float top;
        public readonly float right;
        public readonly float bottom;

        public float width {
            get { return this.right - this.left; }
        }

        public float height {
            get { return this.bottom - this.top; }
        }

        public Size size {
            get { return new Size(this.width, this.height); }
        }

        public static readonly uiRect zero = new uiRect(0, 0, 0, 0);

        public static readonly uiRect one = new uiRect(0, 0, 1, 1);

        public static readonly uiRect infinity = new uiRect(float.NegativeInfinity, float.NegativeInfinity,
            float.PositiveInfinity, float.PositiveInfinity);

        public const float _giantScalar = 1.0E+9f;

        public static readonly uiRect largest =
            fromLTRB(-_giantScalar, -_giantScalar, _giantScalar, _giantScalar);

        public bool isInfinite {
            get {
                return float.IsInfinity(this.left)
                       || float.IsInfinity(this.top)
                       || float.IsInfinity(this.right)
                       || float.IsInfinity(this.bottom);
            }
        }

        public bool isFinite {
            get { return !this.isInfinite; }
        }

        public bool isEmpty {
            get { return this.left >= this.right || this.top >= this.bottom; }
        }

        public uiRect shift(Offset offset) {
            return fromLTRB(this.left + offset.dx, this.top + offset.dy, this.right + offset.dx,
                this.bottom + offset.dy);
        }

        public uiRect translate(float translateX, float translateY) {
            return fromLTRB(this.left + translateX, this.top + translateY, this.right + translateX,
                this.bottom + translateY);
        }

        public uiRect scale(float scaleX, float? scaleY = null) {
            scaleY = scaleY ?? scaleX;
            return fromLTRB(
                this.left * scaleX, this.top * scaleY.Value,
                this.right * scaleX, this.bottom * scaleY.Value);
        }
        
        public uiRect outset(float dx, float dy) {
            return new uiRect(this.left - dx, this.top - dy, this.right + dx, this.bottom + dy);
        }


        public uiRect inflate(float delta) {
            return fromLTRB(this.left - delta, this.top - delta, this.right + delta, this.bottom + delta);
        }

        public uiRect deflate(float delta) {
            return this.inflate(-delta);
        }

        public uiRect intersect(Rect other) {
            return fromLTRB(
                Mathf.Max(this.left, other.left),
                Mathf.Max(this.top, other.top),
                Mathf.Min(this.right, other.right),
                Mathf.Min(this.bottom, other.bottom)
            );
        }

        public uiRect expandToInclude(uiRect other) {
            if (this.isEmpty) {
                return other;
            }

            if (other == null || other.isEmpty) {
                return this;
            }

            return fromLTRB(
                Mathf.Min(this.left, other.left),
                Mathf.Min(this.top, other.top),
                Mathf.Max(this.right, other.right),
                Mathf.Max(this.bottom, other.bottom)
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

        public float shortestSide {
            get { return Mathf.Min(Mathf.Abs(this.width), Mathf.Abs(this.height)); }
        }

        public float longestSide {
            get { return Mathf.Max(Mathf.Abs(this.width), Mathf.Abs(this.height)); }
        }

        public Offset topLeft {
            get { return new Offset(this.left, this.top); }
        }

        public Offset topCenter {
            get { return new Offset(this.left + this.width / 2.0f, this.top); }
        }

        public Offset topRight {
            get { return new Offset(this.right, this.top); }
        }

        public Offset centerLeft {
            get { return new Offset(this.left, this.top + this.height / 2.0f); }
        }

        public Offset center {
            get { return new Offset(this.left + this.width / 2.0f, this.top + this.height / 2.0f); }
        }

        public Offset centerRight {
            get { return new Offset(this.right, this.bottom); }
        }

        public Offset bottomLeft {
            get { return new Offset(this.left, this.bottom); }
        }

        public Offset bottomCenter {
            get { return new Offset(this.left + this.width / 2.0f, this.bottom); }
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

        public uiRect round() {
            return fromLTRB(
                Mathf.Round(this.left), Mathf.Round(this.top),
                Mathf.Round(this.right), Mathf.Round(this.bottom));
        }

        public uiRect roundOut() {
            return fromLTRB(
                Mathf.Floor(this.left), Mathf.Floor(this.top),
                Mathf.Ceil(this.right), Mathf.Ceil(this.bottom));
        }

        public uiRect roundOut(float devicePixelRatio) {
            return fromLTRB(
                Mathf.Floor(this.left * devicePixelRatio) / devicePixelRatio, 
                Mathf.Floor(this.top * devicePixelRatio) / devicePixelRatio,
                Mathf.Ceil(this.right * devicePixelRatio) / devicePixelRatio, 
                Mathf.Ceil(this.bottom * devicePixelRatio) / devicePixelRatio);
        }

        public uiRect roundIn() {
            return fromLTRB(
                Mathf.Ceil(this.left), Mathf.Ceil(this.top),
                Mathf.Floor(this.right), Mathf.Floor(this.bottom));
        }

        public uiRect normalize() {
            if (this.left <= this.right && this.top <= this.bottom) {
                return this;
            }

            return fromLTRB(
                Mathf.Min(this.left, this.right),
                Mathf.Min(this.top, this.bottom),
                Mathf.Max(this.left, this.right),
                Mathf.Max(this.top, this.bottom)
            );
        }

        public static uiRect lerp(uiRect a, uiRect b, float t) {
            if (a == null && b == null) {
                Debug.Assert(false, "uiRect cannot be lerpped between two nulls!");
                return zero;
            }

            if (a == null) {
                return fromLTRB(b.left * t, b.top * t, b.right * t, b.bottom * t);
            }

            if (b == null) {
                float k = 1.0f - t;
                return fromLTRB(a.left * k, a.top * k, a.right * k, a.bottom * k);
            }

            return fromLTRB(
                MathUtils.lerpFloat(a.left, b.left, t),
                MathUtils.lerpFloat(a.top, b.top, t),
                MathUtils.lerpFloat(a.right, b.right, t),
                MathUtils.lerpFloat(a.bottom, b.bottom, t)
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

        public static bool operator ==(uiRect left, uiRect right) {
            return Equals(left, right);
        }

        public static bool operator !=(uiRect left, uiRect right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return "Rect.fromLTRB(" + this.left.ToString("0.0") + ", " + this.top.ToString("0.0") + ", " +
                   this.right.ToString("0.0") + ", " + this.bottom.ToString("0.0") + ")";
        }
    }*/
}