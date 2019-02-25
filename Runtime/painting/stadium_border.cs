using System;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class StadiumBorder : ShapeBorder, IEquatable<StadiumBorder> {
        public StadiumBorder(BorderSide side = null) {
            this.side = side ?? BorderSide.none;
        }

        public readonly BorderSide side;

        public override EdgeInsets dimensions {
            get { return EdgeInsets.all(this.side.width); }
        }

        public override ShapeBorder scale(float t) {
            return new StadiumBorder(side: this.side.scale(t));
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is StadiumBorder stadiumBorder) {
                return new StadiumBorder(side: BorderSide.lerp(stadiumBorder.side, this.side, t));
            }

            if (a is CircleBorder circleBorder) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(circleBorder.side, this.side, t),
                    circleness: 1.0f - t
                );
            }

            if (a is RoundedRectangleBorder rectBorder) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(rectBorder.side, this.side, t),
                    borderRadius: rectBorder.borderRadius,
                    rectness: 1.0f - t
                );
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is StadiumBorder stadiumBorder) {
                return new StadiumBorder(side: BorderSide.lerp(this.side, stadiumBorder.side, t));
            }

            if (b is CircleBorder circleBorder) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(this.side, circleBorder.side, t),
                    circleness: t
                );
            }

            if (b is RoundedRectangleBorder rectBorder) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(this.side, rectBorder.side, t),
                    borderRadius: rectBorder.borderRadius,
                    rectness: t
                );
            }

            return base.lerpTo(b, t);
        }

        public override Path getInnerPath(Rect rect) {
            Radius radius = Radius.circular(rect.shortestSide / 2.0f);
            var path = new Path();
            path.addRRect(RRect.fromRectAndRadius(rect, radius).deflate(this.side.width));
            return path;
        }

        public override Path getOuterPath(Rect rect) {
            Radius radius = Radius.circular(rect.shortestSide / 2.0f);
            var path = new Path();
            path.addRRect(RRect.fromRectAndRadius(rect, radius));
            return path;
        }

        public override void paint(Canvas canvas, Rect rect) {
            switch (this.side.style) {
                case BorderStyle.none:
                    break;
                case BorderStyle.solid:
                    Radius radius = Radius.circular(rect.shortestSide / 2.0f);
                    canvas.drawRRect(
                        RRect.fromRectAndRadius(rect, radius).deflate(this.side.width / 2.0f),
                        this.side.toPaint()
                    );
                    break;
            }
        }

        public bool Equals(StadiumBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.side, other.side);
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

            return this.Equals((StadiumBorder) obj);
        }

        public override int GetHashCode() {
            return (this.side != null ? this.side.GetHashCode() : 0);
        }

        public static bool operator ==(StadiumBorder left, StadiumBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(StadiumBorder left, StadiumBorder right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.side})";
        }
    }

    class _StadiumToCircleBorder : ShapeBorder, IEquatable<_StadiumToCircleBorder> {
        public _StadiumToCircleBorder(
            BorderSide side = null,
            float circleness = 0.0f
        ) {
            this.side = BorderSide.none;
            this.circleness = circleness;
        }

        public readonly BorderSide side;

        public readonly float circleness;

        public override EdgeInsets dimensions {
            get { return EdgeInsets.all(this.side.width); }
        }

        public override ShapeBorder scale(float t) {
            return new _StadiumToCircleBorder(
                side: this.side.scale(t),
                circleness: t
            );
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is StadiumBorder stadiumBorder) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(stadiumBorder.side, this.side, t),
                    circleness: this.circleness * t
                );
            }

            if (a is CircleBorder circleBorder) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(circleBorder.side, this.side, t),
                    circleness: this.circleness + (1.0f - this.circleness) * (1.0f - t)
                );
            }

            if (a is _StadiumToCircleBorder border) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(border.side, this.side, t),
                    circleness: MathUtils.lerpFloat(border.circleness, this.circleness, t)
                );
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is StadiumBorder stadiumBorder) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(this.side, stadiumBorder.side, t),
                    circleness: this.circleness * (1.0f - t)
                );
            }

            if (b is CircleBorder circleBorder) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(this.side, circleBorder.side, t),
                    circleness: this.circleness + (1.0f - this.circleness) * t
                );
            }

            if (b is _StadiumToCircleBorder border) {
                return new _StadiumToCircleBorder(
                    side: BorderSide.lerp(this.side, border.side, t),
                    circleness: MathUtils.lerpFloat(this.circleness, border.circleness, t)
                );
            }

            return base.lerpTo(b, t);
        }

        Rect _adjustRect(Rect rect) {
            if (this.circleness == 0.0f || rect.width == rect.height) {
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
                    (rect.left + delta),
                    rect.top,
                    (rect.right - delta),
                    rect.bottom
                );
            }
        }

        BorderRadius _adjustBorderRadius(Rect rect) {
            return BorderRadius.circular(rect.shortestSide / 2.0f);
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
                    if (width == 0.0f) {
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

        public bool Equals(_StadiumToCircleBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.side, other.side) && this.circleness.Equals(other.circleness);
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

            return this.Equals((_StadiumToCircleBorder) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((this.side != null ? this.side.GetHashCode() : 0) * 397) ^ this.circleness.GetHashCode();
            }
        }

        public static bool operator ==(_StadiumToCircleBorder left, _StadiumToCircleBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(_StadiumToCircleBorder left, _StadiumToCircleBorder right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"StadiumBorder($side, {this.circleness * 100:F1}% " +
                   "of the way to being a CircleBorder)";
        }
    }

    class _StadiumToRoundedRectangleBorder : ShapeBorder, IEquatable<_StadiumToRoundedRectangleBorder> {
        public _StadiumToRoundedRectangleBorder(
            BorderSide side = null,
            BorderRadius borderRadius = null,
            float rectness = 0.0f
        ) {
            this.side = side ?? BorderSide.none;
            this.borderRadius = borderRadius ?? BorderRadius.zero;
            this.rectness = rectness;
        }

        public readonly BorderSide side;

        public readonly BorderRadius borderRadius;

        public readonly float rectness;

        public override EdgeInsets dimensions {
            get { return EdgeInsets.all(this.side.width); }
        }

        public override ShapeBorder scale(float t) {
            return new _StadiumToRoundedRectangleBorder(
                side: this.side.scale(t),
                borderRadius: this.borderRadius * t,
                rectness: t
            );
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is StadiumBorder stadiumBorder) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(stadiumBorder.side, this.side, t),
                    borderRadius: this.borderRadius,
                    rectness: this.rectness * t
                );
            }

            if (a is RoundedRectangleBorder rectBorder) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(rectBorder.side, this.side, t),
                    borderRadius: this.borderRadius,
                    rectness: this.rectness + (1.0f - this.rectness) * (1.0f - t)
                );
            }

            if (a is _StadiumToRoundedRectangleBorder border) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(border.side, this.side, t),
                    borderRadius: BorderRadius.lerp(border.borderRadius, this.borderRadius, t),
                    rectness: MathUtils.lerpFloat(border.rectness, this.rectness, t)
                );
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is StadiumBorder stadiumBorder) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(this.side, stadiumBorder.side, t),
                    borderRadius: this.borderRadius,
                    rectness: this.rectness * (1.0f - t)
                );
            }

            if (b is RoundedRectangleBorder rectBorder) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(this.side, rectBorder.side, t),
                    borderRadius: this.borderRadius,
                    rectness: this.rectness + (1.0f - this.rectness) * t
                );
            }

            if (b is _StadiumToRoundedRectangleBorder border) {
                return new _StadiumToRoundedRectangleBorder(
                    side: BorderSide.lerp(this.side, border.side, t),
                    borderRadius: BorderRadius.lerp(this.borderRadius, border.borderRadius, t),
                    rectness: MathUtils.lerpFloat(this.rectness, border.rectness, t)
                );
            }

            return base.lerpTo(b, t);
        }

        BorderRadius _adjustBorderRadius(Rect rect) {
            return BorderRadius.lerp(
                this.borderRadius,
                BorderRadius.all(Radius.circular(rect.shortestSide / 2.0f)),
                1.0f - this.rectness
            );
        }

        public override Path getInnerPath(Rect rect) {
            var path = new Path();
            path.addRRect(this._adjustBorderRadius(rect).toRRect(rect).deflate(this.side.width));
            return path;
        }

        public override Path getOuterPath(Rect rect) {
            var path = new Path();
            path.addRRect(this._adjustBorderRadius(rect).toRRect(rect));
            return path;
        }

        public override void paint(Canvas canvas, Rect rect) {
            switch (this.side.style) {
                case BorderStyle.none:
                    break;
                case BorderStyle.solid:
                    float width = this.side.width;
                    if (width == 0.0f) {
                        canvas.drawRRect(this._adjustBorderRadius(rect).toRRect(rect), this.side.toPaint());
                    }
                    else {
                        RRect outer = this._adjustBorderRadius(rect).toRRect(rect);
                        RRect inner = outer.deflate(width);
                        Paint paint = new Paint {
                            color = this.side.color,
                        };
                        canvas.drawDRRect(outer, inner, paint);
                    }

                    break;
            }
        }

        public bool Equals(_StadiumToRoundedRectangleBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.side, other.side) && Equals(this.borderRadius, other.borderRadius) &&
                   this.rectness.Equals(other.rectness);
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

            return this.Equals((_StadiumToRoundedRectangleBorder) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.side != null ? this.side.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.borderRadius != null ? this.borderRadius.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.rectness.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(_StadiumToRoundedRectangleBorder left, _StadiumToRoundedRectangleBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(_StadiumToRoundedRectangleBorder left, _StadiumToRoundedRectangleBorder right) {
            return !Equals(left, right);
        }


        public override string ToString() {
            return $"StadiumBorder({this.side}, {this.borderRadius}, " +
                   $"{this.rectness * 100:F1}% of the way to being a " +
                   "RoundedRectangleBorder)";
        }
    }
}