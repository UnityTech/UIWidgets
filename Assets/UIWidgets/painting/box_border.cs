using System;
using System.Runtime.CompilerServices;
using UIWidgets.foundation;
using UIWidgets.ui;
using UnityEditor;

namespace UIWidgets.painting {
    public class BorderSide : IEquatable<BorderSide> {
        public BorderSide(
            Color color = null,
            double width = 1.0
        ) {
            this.color = color ?? Color.fromARGB(255, 0, 0, 0);
            this.width = width;
        }

        public readonly Color color;
        public readonly double width;

        public static readonly BorderSide none = new BorderSide(width: 0.0);

        public bool Equals(BorderSide other) {
            return this.color.Equals(other.color) && this.width.Equals(other.width);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BorderSide && Equals((BorderSide) obj);
        }


        public override int GetHashCode() {
            unchecked {
                var hashCode = this.color.GetHashCode();
                hashCode = (hashCode * 397) ^ this.width.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(BorderSide lhs, BorderSide rhs) {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(BorderSide lhs, BorderSide rhs) {
            return !(lhs == rhs);
        }
    }

    public class Border : ShapeBorder {
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


        public override EdgeInsetsGeometry dimensions {
            get { return null; }
        }

        public bool isSameColor {
            get {
                Color topColor = this.top.color;
                return this.right.color == topColor && this.bottom.color == topColor && this.left.color == topColor;
            }
        }

        public void paint(Canvas canvas, Rect rect, BorderRadius borderRadius = null) {
            var paint = new Paint();

            if (this.isSameColor) {
                paint.color = this.top.color;

                canvas.drawRect(paint, rect,
                    BorderWidth.only(this.top.width, this.right.width, this.bottom.width, this.left.width),
                    borderRadius ?? BorderRadius.zero);

                return;
            }

            if (this.top.width > 0) {
                paint.color = this.top.color;
                var points = new Offset[] {
                    new Offset(rect.left, rect.top),
                    new Offset(rect.right, rect.top),
                    new Offset(rect.right - this.right.width, rect.top + this.top.width),
                    new Offset(rect.left + this.right.width, rect.top + this.top.width),
                };
                canvas.drawPloygon4(paint, points);
            }

            if (this.right.width > 0) {
                paint.color = this.right.color;
                var points = new Offset[] {
                    new Offset(rect.right, rect.top),
                    new Offset(rect.right, rect.bottom),
                    new Offset(rect.right - this.right.width, rect.bottom - this.bottom.width),
                    new Offset(rect.right - this.right.width, rect.top + this.top.width),
                };
                canvas.drawPloygon4(paint, points);
            }

            if (this.bottom.width > 0) {
                paint.color = this.bottom.color;
                var points = new Offset[] {
                    new Offset(rect.right, rect.bottom),
                    new Offset(rect.left, rect.bottom),
                    new Offset(rect.left + this.left.width, rect.bottom - this.bottom.width),
                    new Offset(rect.right - this.right.width, rect.bottom - this.bottom.width),
                };
                canvas.drawPloygon4(paint, points);
            }

            if (this.left.width > 0) {
                paint.color = this.left.color;
                var points = new Offset[] {
                    new Offset(rect.left, rect.bottom),
                    new Offset(rect.left, rect.top),
                    new Offset(rect.left + this.left.width, rect.top + this.top.width),
                    new Offset(rect.left + this.left.width, rect.bottom - this.bottom.width),
                };
                canvas.drawPloygon4(paint, points);
            }
        }
    }


    public class ImageConfiguration {
        public ImageConfiguration(Size size = null) {
            this.size = size;
        }

        public readonly Size size;
    }
}