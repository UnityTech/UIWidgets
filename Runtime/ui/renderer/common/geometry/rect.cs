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

        public uiOffset topLeft {
            get { return new uiOffset(this.left, this.top); }
        }

        public uiOffset topCenter {
            get { return new uiOffset(this.left + this.width / 2.0f, this.top); }
        }

        public uiOffset topRight {
            get { return new uiOffset(this.right, this.top); }
        }

        public uiOffset centerLeft {
            get { return new uiOffset(this.left, this.top + this.height / 2.0f); }
        }

        public uiOffset center {
            get { return new uiOffset(this.left + this.width / 2.0f, this.top + this.height / 2.0f); }
        }

        public uiOffset centerRight {
            get { return new uiOffset(this.right, this.bottom); }
        }

        public uiOffset bottomLeft {
            get { return new uiOffset(this.left, this.bottom); }
        }

        public uiOffset bottomCenter {
            get { return new uiOffset(this.left + this.width / 2.0f, this.bottom); }
        }

        public uiOffset bottomRight {
            get { return new uiOffset(this.right, this.bottom); }
        }

        public uiRect shift(uiOffset offset) {
            return uiRectHelper.fromLTRB(this.left + offset.dx, this.top + offset.dy, this.right + offset.dx,
                this.bottom + offset.dy);
        }

        public uiRect intersect(uiRect other) {
            return uiRectHelper.fromLTRB(
                Mathf.Max(this.left, other.left),
                Mathf.Max(this.top, other.top),
                Mathf.Min(this.right, other.right),
                Mathf.Min(this.bottom, other.bottom)
            );
        }

        public uiRect expandToInclude(uiRect? other) {
            if (this.isEmpty) {
                return other.Value;
            }

            if (other == null || other.Value.isEmpty) {
                return this;
            }

            return uiRectHelper.fromLTRB(
                Mathf.Min(this.left, other.Value.left),
                Mathf.Min(this.top, other.Value.top),
                Mathf.Max(this.right, other.Value.right),
                Mathf.Max(this.bottom, other.Value.bottom)
            );
        }
    }

    public static class uiRectHelper {
        public static uiRect? fromRect(Rect rect) {
            if (rect == null) {
                return null;
            }
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
            if (a == null && b == null) {
                return true;
            }

            if (a == null || b == null) {
                return false;
            }

            var aval = a.Value;
            var bval = b.Value;

            return aval.left == bval.left && aval.right == bval.right && aval.top == bval.top &&
                   aval.bottom == bval.bottom;
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


        public static uiOffset[] toQuad(uiRect a) {
            uiOffset[] dst = new uiOffset[4];
            dst[0] = new uiOffset(a.left, a.top);
            dst[1] = new uiOffset(a.right, a.top);
            dst[2] = new uiOffset(a.right, a.bottom);
            dst[3] = new uiOffset(a.left, a.bottom);
            return dst;
        }

        public static bool contains(uiRect a, uiOffset offset) {
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
}