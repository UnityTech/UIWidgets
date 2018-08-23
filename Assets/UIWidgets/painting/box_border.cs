using System;
using System.Runtime.CompilerServices;
using UIWidgets.foundation;
using UIWidgets.ui;
using UnityEditor;

namespace UIWidgets.painting {
    public class Border : IEquatable<Border> {
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

        public readonly BorderSide top;
        public readonly BorderSide right;
        public readonly BorderSide bottom;
        public readonly BorderSide left;

        public static Border all(
            Color color = null,
            double width = 1.0
        ) {
            BorderSide side = new BorderSide(color: color, width: width);
            return new Border(top: side, right: side, bottom: side, left: side);
        }

        public static Border merge(Border a, Border b) {
            return new Border(
                top: BorderSide.merge(a.top, b.top),
                right: BorderSide.merge(a.right, b.right),
                bottom: BorderSide.merge(a.bottom, b.bottom),
                left: BorderSide.merge(a.left, b.left)
            );
        }

        public EdgeInsets dimensions {
            get {
                return EdgeInsets.fromLTRB(
                    this.left.width,
                    this.top.width,
                    this.right.width,
                    this.bottom.width);
            }
        }

        public bool isSameColor {
            get {
                Color topColor = this.top.color;
                return this.right.color == topColor
                       && this.bottom.color == topColor
                       && this.left.color == topColor;
            }
        }

        public Border add(Border other) {
            if (BorderSide.canMerge(this.top, other.top) &&
                BorderSide.canMerge(this.right, other.right) &&
                BorderSide.canMerge(this.bottom, other.bottom) &&
                BorderSide.canMerge(this.left, other.left)) {
                return Border.merge(this, other);
            }

            return null;
        }

        public void paint(Canvas canvas, Rect rect, BorderRadius borderRadius = null) {
            var paint = new Paint();

            if (this.isSameColor) {
                paint.color = this.top.color;

                canvas.drawRect(rect,
                    BorderWidth.only(this.top.width, this.right.width, this.bottom.width, this.left.width),
                    borderRadius, paint);

                return;
            }

            if (borderRadius != null) {
                canvas.save();
                canvas.clipRRect(RRect.fromRectAndCorners(rect,
                    borderRadius.topLeft, borderRadius.topRight,
                    borderRadius.bottomRight, borderRadius.bottomLeft));
            }

            if (this.top.width > 0) {
                paint.color = this.top.color;
                var points = new Offset[] {
                    new Offset(rect.left, rect.top),
                    new Offset(rect.right, rect.top),
                    new Offset(rect.right - this.right.width, rect.top + this.top.width),
                    new Offset(rect.left + this.right.width, rect.top + this.top.width),
                };
                canvas.drawPloygon4(points, paint);
            }

            if (this.right.width > 0) {
                paint.color = this.right.color;
                var points = new Offset[] {
                    new Offset(rect.right, rect.top),
                    new Offset(rect.right, rect.bottom),
                    new Offset(rect.right - this.right.width, rect.bottom - this.bottom.width),
                    new Offset(rect.right - this.right.width, rect.top + this.top.width),
                };
                canvas.drawPloygon4(points, paint);
            }

            if (this.bottom.width > 0) {
                paint.color = this.bottom.color;
                var points = new Offset[] {
                    new Offset(rect.right, rect.bottom),
                    new Offset(rect.left, rect.bottom),
                    new Offset(rect.left + this.left.width, rect.bottom - this.bottom.width),
                    new Offset(rect.right - this.right.width, rect.bottom - this.bottom.width),
                };
                canvas.drawPloygon4(points, paint);
            }

            if (this.left.width > 0) {
                paint.color = this.left.color;
                var points = new Offset[] {
                    new Offset(rect.left, rect.bottom),
                    new Offset(rect.left, rect.top),
                    new Offset(rect.left + this.left.width, rect.top + this.top.width),
                    new Offset(rect.left + this.left.width, rect.bottom - this.bottom.width),
                };
                canvas.drawPloygon4(points, paint);
            }

            if (borderRadius != null) {
                canvas.restore();
            }
        }

        public bool Equals(Border other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return object.Equals(this.top, other.top)
                   && object.Equals(this.right, other.right)
                   && object.Equals(this.bottom, other.bottom)
                   && object.Equals(this.left, other.left);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
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
    }

    public class ImageConfiguration {
        public ImageConfiguration(Size size = null) {
            this.size = size;
        }

        public static readonly ImageConfiguration empty = new ImageConfiguration();

        public ImageConfiguration copyWith(
            Size size = null) {
            return new ImageConfiguration(
                size: size ?? this.size
            );
        }

        public readonly Size size;
    }
}