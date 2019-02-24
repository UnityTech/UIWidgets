using System;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class RoundedRectangleBorder : ShapeBorder, IEquatable<RoundedRectangleBorder> {
        public RoundedRectangleBorder(
            BorderSide side = null,
            BorderRadius borderRadius = null
        ) {
            this.side = side ?? BorderSide.none;
            this.borderRadius = borderRadius ?? BorderRadius.zero;
        }

        public readonly BorderSide side;

        public readonly BorderRadius borderRadius;


        public override EdgeInsets dimensions {
            get { return EdgeInsets.all(this.side.width); }
        }

        public override ShapeBorder scale(float t) {
            return new RoundedRectangleBorder(
                side: this.side.scale(t),
                borderRadius: this.borderRadius * t
            );
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is RoundedRectangleBorder border) {
                return new RoundedRectangleBorder(
                    side: BorderSide.lerp(border.side, this.side, t),
                    borderRadius: BorderRadius.lerp(border.borderRadius, this.borderRadius, t)
                );
            }

            if (a is CircleBorder circleBorder) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(circleBorder.side, this.side, t),
                    borderRadius: this.borderRadius,
                    circleness: 1.0f - t
                );
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is RoundedRectangleBorder border) {
                return new RoundedRectangleBorder(
                    side: BorderSide.lerp(this.side, border.side, t),
                    borderRadius: BorderRadius.lerp(this.borderRadius, border.borderRadius, t)
                );
            }

            if (b is CircleBorder circleBorder) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(this.side, circleBorder.side, t),
                    borderRadius: this.borderRadius,
                    circleness: t
                );
            }

            return base.lerpTo(b, t);
        }

        public override Path getInnerPath(Rect rect) {
            var path = new Path();
            path.addRRect(this.borderRadius.toRRect(rect).deflate(this.side.width));
            return path;
        }

        public override Path getOuterPath(Rect rect) {
            var path = new Path();
            path.addRRect(this.borderRadius.toRRect(rect));
            return path;
        }

        public override void paint(Canvas canvas, Rect rect) {
            switch (this.side.style) {
                case BorderStyle.none:
                    break;
                case BorderStyle.solid:
                    float width = this.side.width;
                    if (width == 0.0) {
                        canvas.drawRRect(this.borderRadius.toRRect(rect), this.side.toPaint());
                    }
                    else {
                        RRect outer = this.borderRadius.toRRect(rect);
                        RRect inner = outer.deflate(width);
                        Paint paint = new Paint {
                            color = this.side.color,
                        };
                        canvas.drawDRRect(outer, inner, paint);
                    }

                    break;
            }
        }

        public bool Equals(RoundedRectangleBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.side, other.side) && Equals(this.borderRadius, other.borderRadius);
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

            return this.Equals((RoundedRectangleBorder) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((this.side != null ? this.side.GetHashCode() : 0) * 397) ^
                       (this.borderRadius != null ? this.borderRadius.GetHashCode() : 0);
            }
        }

        public static bool operator ==(RoundedRectangleBorder left, RoundedRectangleBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(RoundedRectangleBorder left, RoundedRectangleBorder right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.side}, {this.borderRadius})";
        }
    }

    class _RoundedRectangleToCircleBorder : ShapeBorder, IEquatable<_RoundedRectangleToCircleBorder> {
        public _RoundedRectangleToCircleBorder(
            BorderSide side = null,
            BorderRadius borderRadius = null,
            float circleness = 0.0f
        ) {
            this.side = side ?? BorderSide.none;
            this.borderRadius = borderRadius ?? BorderRadius.zero;
            this.circleness = circleness;
        }

        public readonly BorderSide side;

        public readonly BorderRadius borderRadius;

        public readonly float circleness;

        public override EdgeInsets dimensions {
            get { return EdgeInsets.all(this.side.width); }
        }

        public override ShapeBorder scale(float t) {
            return new _RoundedRectangleToCircleBorder(
                side: this.side.scale(t),
                borderRadius: this.borderRadius * t,
                circleness: t
            );
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is RoundedRectangleBorder rectBorder) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(rectBorder.side, this.side, t),
                    borderRadius: BorderRadius.lerp(rectBorder.borderRadius, this.borderRadius, t),
                    circleness: this.circleness * t
                );
            }

            if (a is CircleBorder circleBorder) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(circleBorder.side, this.side, t),
                    borderRadius: this.borderRadius,
                    circleness: this.circleness + (1.0f - this.circleness) * (1.0f - t)
                );
            }

            if (a is _RoundedRectangleToCircleBorder border) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(border.side, this.side, t),
                    borderRadius: BorderRadius.lerp(border.borderRadius, this.borderRadius, t),
                    circleness: MathUtils.lerpFloat(border.circleness, this.circleness, t)
                );
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is RoundedRectangleBorder rectBorder) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(this.side, rectBorder.side, t),
                    borderRadius: BorderRadius.lerp(this.borderRadius, rectBorder.borderRadius, t),
                    circleness: this.circleness * (1.0f - t)
                );
            }

            if (b is CircleBorder circleBorder) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(this.side, circleBorder.side, t),
                    borderRadius: this.borderRadius,
                    circleness: this.circleness + (1.0f - this.circleness) * t
                );
            }

            if (b is _RoundedRectangleToCircleBorder border) {
                return new _RoundedRectangleToCircleBorder(
                    side: BorderSide.lerp(this.side, border.side, t),
                    borderRadius: BorderRadius.lerp(this.borderRadius, border.borderRadius, t),
                    circleness: MathUtils.lerpFloat(this.circleness, border.circleness, t)
                );
            }

            return base.lerpTo(b, t);
        }

        Rect _adjustRect(Rect rect) {
            if (this.circleness == 0.0 || rect.width == rect.height) {
                return rect;
            }

            if (rect.width < rect.height) {
                float delta = this.circleness * (rect.height - rect.width) / 2.0f;
                return Rect.fromLTRB(
                    rect.left,
                    rect.top + delta,
                    rect.right,
                    rect.bottom - delta
                );
            }
            else {
                float delta = this.circleness * (rect.width - rect.height) / 2.0f;
                return Rect.fromLTRB(
                    rect.left + delta,
                    rect.top,
                    rect.right - delta,
                    rect.bottom
                );
            }
        }

        BorderRadius _adjustBorderRadius(Rect rect) {
            BorderRadius resolvedRadius = this.borderRadius;
            if (this.circleness == 0.0f) {
                return resolvedRadius;
            }

            return BorderRadius.lerp(resolvedRadius, BorderRadius.circular(rect.shortestSide / 2.0f), this.circleness);
        }

        public override Path getInnerPath(Rect rect) {
            var path = new Path();
            path.addRRect(this._adjustBorderRadius(rect).toRRect(this._adjustRect(rect)).deflate(this.side.width));
            return path;
        }

        public override Path getOuterPath(Rect rect) {
            var path = new Path();
            path.addRRect(this._adjustBorderRadius(rect).toRRect(this._adjustRect(rect)));
            return path;
        }

        public override void paint(Canvas canvas, Rect rect) {
            switch (this.side.style) {
                case BorderStyle.none:
                    break;
                case BorderStyle.solid:
                    float width = this.side.width;
                    if (width == 0.0) {
                        canvas.drawRRect(this._adjustBorderRadius(rect).toRRect(this._adjustRect(rect)),
                            this.side.toPaint());
                    }
                    else {
                        RRect outer = this._adjustBorderRadius(rect).toRRect(this._adjustRect(rect));
                        RRect inner = outer.deflate(width);
                        Paint paint = new Paint {
                            color = this.side.color,
                        };
                        canvas.drawDRRect(outer, inner, paint);
                    }

                    break;
            }
        }

        public bool Equals(_RoundedRectangleToCircleBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.side, other.side) && Equals(this.borderRadius, other.borderRadius) &&
                   this.circleness.Equals(other.circleness);
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

            return this.Equals((_RoundedRectangleToCircleBorder) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.side != null ? this.side.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.borderRadius != null ? this.borderRadius.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.circleness.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(_RoundedRectangleToCircleBorder left, _RoundedRectangleToCircleBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(_RoundedRectangleToCircleBorder left, _RoundedRectangleToCircleBorder right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"RoundedRectangleBorder({this.side}, {this.borderRadius}, " +
                   $"{this.circleness * 100:F1}% of the way to being a CircleBorder)";
        }
    }
}