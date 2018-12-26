using System;
using System.Runtime.CompilerServices;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEditor;

namespace Unity.UIWidgets.painting {
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

        public bool isSameWidth {
            get {
                var topWidth = this.top.width;
                return this.right.width == topWidth
                       && this.bottom.width == topWidth
                       && this.left.width == topWidth;
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
            if (this.isSameColor && this.isSameWidth) {
                if (borderRadius == null) {
                    var width = this.top.width;
                    var paint = new Paint {
                        color = this.top.color,
                        strokeWidth = width,
                        style = PaintingStyle.stroke
                    };

                    canvas.drawRect(rect.deflate(width / 2), paint);
                    return;
                }

                var outer = borderRadius.toRRect(rect);
                if (this.top.width == 0) {
                    var paint = new Paint {
                        color = this.top.color,
                        style = PaintingStyle.stroke
                    };

                    canvas.drawRRect(outer, paint);
                    return;
                }

                {
                    var inner = outer.deflate(this.top.width);
                    var paint = new Paint {
                        color = this.top.color,
                    };

                    canvas.drawDRRect(outer, inner, paint);
                }
                return;
            }

            D.assert(borderRadius == null, "A borderRadius can only be given for uniform borders.");


            {
                var paint = new Paint {
                    color = this.top.color,
                };
                var path = new Path();
                path.moveTo(rect.left, rect.top);
                path.lineTo(rect.right, rect.top);
                if (this.top.width == 0) {
                    paint.style = PaintingStyle.stroke;
                } else {
                    path.lineTo(rect.right - this.right.width, rect.top + this.top.width);
                    path.lineTo(rect.left + this.right.width, rect.top + this.top.width);
                }
                
                canvas.drawPath(path, paint);
            }
            
            {
                var paint = new Paint {
                    color = this.right.color,
                };
                var path = new Path();
                path.moveTo(rect.right, rect.top);
                path.lineTo(rect.right, rect.bottom);
                if (this.right.width == 0) {
                    paint.style = PaintingStyle.stroke;
                } else {
                    path.lineTo(rect.right - this.right.width, rect.bottom - this.bottom.width);
                    path.lineTo(rect.right - this.right.width, rect.top + this.top.width);
                }
                
                canvas.drawPath(path, paint);
            }

            {
                var paint = new Paint {
                    color = this.bottom.color,
                };
                var path = new Path();
                path.moveTo(rect.right, rect.bottom);
                path.lineTo(rect.left, rect.bottom);
                if (this.bottom.width == 0) {
                    paint.style = PaintingStyle.stroke;
                } else {
                    path.lineTo(rect.left + this.left.width, rect.bottom - this.bottom.width);
                    path.lineTo(rect.right - this.right.width, rect.bottom - this.bottom.width);
                }
                
                canvas.drawPath(path, paint);
            }
            
            {
                var paint = new Paint {
                    color = this.left.color,
                };
                var path = new Path();
                path.moveTo(rect.left, rect.bottom);
                path.lineTo(rect.left, rect.top);
                if (this.left.width == 0) {
                    paint.style = PaintingStyle.stroke;
                } else {
                    path.lineTo(rect.left + this.left.width, rect.top + this.top.width);
                    path.lineTo(rect.left + this.left.width, rect.bottom - this.bottom.width);
                }
                
                canvas.drawPath(path, paint);
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
}