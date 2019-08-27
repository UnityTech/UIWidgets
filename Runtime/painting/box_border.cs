using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public enum BoxShape {
        rectangle,
        circle,
    }

    public class Border : ShapeBorder, IEquatable<Border> {
        public Border(
            BorderSide top = null,
            BorderSide right = null,
            BorderSide bottom = null,
            BorderSide left = null
        ) {
            this.top = top ?? BorderSide.none;
            this.right = right ?? BorderSide.none;
            this.bottom = bottom ?? BorderSide.none;
            this.left = left ?? BorderSide.none;
        }

        public static Border fromBorderSide(BorderSide side) {
            D.assert(side != null);
            return new Border(top: side, right: side, bottom: side, left: side);
        }

        public static Border all(
            Color color = null,
            float width = 1.0f,
            BorderStyle style = BorderStyle.solid
        ) {
            BorderSide side = new BorderSide(color: color, width: width, style: style);
            return Border.fromBorderSide(side);
        }

        public static Border merge(Border a, Border b) {
            D.assert(a != null);
            D.assert(b != null);
            D.assert(BorderSide.canMerge(a.top, b.top));
            D.assert(BorderSide.canMerge(a.right, b.right));
            D.assert(BorderSide.canMerge(a.bottom, b.bottom));
            D.assert(BorderSide.canMerge(a.left, b.left));

            return new Border(
                top: BorderSide.merge(a.top, b.top),
                right: BorderSide.merge(a.right, b.right),
                bottom: BorderSide.merge(a.bottom, b.bottom),
                left: BorderSide.merge(a.left, b.left)
            );
        }

        public readonly BorderSide top;
        public readonly BorderSide right;
        public readonly BorderSide bottom;
        public readonly BorderSide left;

        public override EdgeInsets dimensions {
            get {
                return EdgeInsets.fromLTRB(
                    this.left.width,
                    this.top.width,
                    this.right.width,
                    this.bottom.width);
            }
        }

        public bool isUniform {
            get { return this.isSameColor && this.isSameWidth && this.isSameStyle; }
        }

        public bool isSameColor {
            get {
                Color topColor = this.top.color;
                return this.right.color == topColor
                       && this.bottom.color == topColor
                       && this.left.color == topColor;
            }
        }

        public bool isSameWidth {
            get {
                var topWidth = this.top.width;
                return this.right.width == topWidth
                       && this.bottom.width == topWidth
                       && this.left.width == topWidth;
            }
        }

        public bool isSameStyle {
            get {
                var topStyle = this.top.style;
                return this.right.style == topStyle
                       && this.bottom.style == topStyle
                       && this.left.style == topStyle;
            }
        }

        public override ShapeBorder add(ShapeBorder other, bool reversed = false) {
            if (!(other is Border border)) {
                return null;
            }

            if (BorderSide.canMerge(this.top, border.top) &&
                BorderSide.canMerge(this.right, border.right) &&
                BorderSide.canMerge(this.bottom, border.bottom) &&
                BorderSide.canMerge(this.left, border.left)) {
                return merge(this, border);
            }

            return null;
        }

        public override ShapeBorder scale(float t) {
            return new Border(
                top: this.top.scale(t),
                right: this.right.scale(t),
                bottom: this.bottom.scale(t),
                left: this.left.scale(t)
            );
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is Border border) {
                return lerp(border, this, t);
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is Border border) {
                return lerp(this, border, t);
            }

            return base.lerpTo(b, t);
        }

        public static Border lerp(Border a, Border b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return (Border) b.scale(t);
            }

            if (b == null) {
                return (Border) a.scale(1.0f - t);
            }

            return new Border(
                top: BorderSide.lerp(a.top, b.top, t),
                right: BorderSide.lerp(a.right, b.right, t),
                bottom: BorderSide.lerp(a.bottom, b.bottom, t),
                left: BorderSide.lerp(a.left, b.left, t)
            );
        }

        public override void paint(Canvas canvas, Rect rect) {
            this.paint(canvas, rect, BoxShape.rectangle, null);
        }

        public void paint(Canvas canvas, Rect rect,
            BoxShape shape = BoxShape.rectangle,
            BorderRadius borderRadius = null) {
            if (this.isUniform) {
                switch (this.top.style) {
                    case BorderStyle.none:
                        return;
                    case BorderStyle.solid:
                        switch (shape) {
                            case BoxShape.circle:
                                D.assert(borderRadius == null,
                                    () => "A borderRadius can only be given for rectangular boxes.");
                                _paintUniformBorderWithCircle(canvas, rect, this.top);
                                break;
                            case BoxShape.rectangle:
                                if (borderRadius != null) {
                                    _paintUniformBorderWithRadius(canvas, rect, this.top, borderRadius);
                                }
                                else {
                                    _paintUniformBorderWithRectangle(canvas, rect, this.top);
                                }

                                break;
                        }

                        return;
                }
            }

            D.assert(borderRadius == null, () => "A borderRadius can only be given for uniform borders.");
            D.assert(shape == BoxShape.rectangle, () => "A border can only be drawn as a circle if it is uniform.");

            BorderUtils.paintBorder(canvas, rect,
                top: this.top, right: this.right, bottom: this.bottom, left: this.left);
        }

        public override Path getInnerPath(Rect rect) {
            var path = new Path();
            path.addRect(this.dimensions.deflateRect(rect));
            return path;
        }

        public override Path getOuterPath(Rect rect) {
            var path = new Path();
            path.addRect(rect);
            return path;
        }

        static void _paintUniformBorderWithRadius(Canvas canvas, Rect rect, BorderSide side,
            BorderRadius borderRadius) {
            D.assert(side.style != BorderStyle.none);
            Paint paint = new Paint {
                color = side.color,
            };

            RRect outer = borderRadius.toRRect(rect);
            float width = side.width;
            if (width == 0.0) {
                paint.style = PaintingStyle.stroke;
                paint.strokeWidth = 0.0f;
                canvas.drawRRect(outer, paint);
            }
            else {
                RRect inner = outer.deflate(width);
                canvas.drawDRRect(outer, inner, paint);
            }
        }

        static void _paintUniformBorderWithCircle(Canvas canvas, Rect rect, BorderSide side) {
            D.assert(side.style != BorderStyle.none);
            float width = side.width;
            Paint paint = side.toPaint();
            float radius = (rect.shortestSide - width) / 2.0f;
            canvas.drawCircle(rect.center, radius, paint);
        }

        static void _paintUniformBorderWithRectangle(Canvas canvas, Rect rect, BorderSide side) {
            D.assert(side.style != BorderStyle.none);
            float width = side.width;
            Paint paint = side.toPaint();
            canvas.drawRect(rect.deflate(width / 2.0f), paint);
        }

        public bool Equals(Border other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.top, other.top)
                   && Equals(this.right, other.right)
                   && Equals(this.bottom, other.bottom)
                   && Equals(this.left, other.left);
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

            return this.Equals((Border) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.top != null ? this.top.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.right != null ? this.right.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.bottom != null ? this.bottom.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.left != null ? this.left.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString() {
            if (this.isUniform) {
                return $"{this.GetType()}.all({this.top})";
            }

            List<string> arguments = new List<string>();
            if (this.top != BorderSide.none) {
                arguments.Add($"top: {this.top}");
            }

            if (this.right != BorderSide.none) {
                arguments.Add($"right: {this.right}");
            }

            if (this.bottom != BorderSide.none) {
                arguments.Add($"bottom: {this.bottom}");
            }

            if (this.left != BorderSide.none) {
                arguments.Add($"left: {this.left}");
            }

            return $"{this.GetType()}({string.Join(", ", arguments)})";
        }
    }
}