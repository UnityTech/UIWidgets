using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public class TableBorder : IEquatable<TableBorder> {
        public TableBorder(
            BorderSide top = null,
            BorderSide right = null,
            BorderSide bottom = null,
            BorderSide left = null,
            BorderSide horizontalInside = null,
            BorderSide verticalInside = null
        ) {
            this.top = top ?? BorderSide.none;
            this.right = right ?? BorderSide.none;
            this.bottom = bottom ?? BorderSide.none;
            this.left = left ?? BorderSide.none;
            this.horizontalInside = horizontalInside ?? BorderSide.none;
            this.verticalInside = verticalInside ?? BorderSide.none;
        }

        public static TableBorder all(
            Color color = null,
            float width = 1.0f,
            BorderStyle style = BorderStyle.solid
        ) {
            color = color ?? new Color(0xFF000000);
            BorderSide side = new BorderSide(color: color, width: width, style: style);
            return new TableBorder(
                top: side, right: side, bottom: side, left: side, horizontalInside: side, verticalInside: side);
        }

        public static TableBorder symmetric(
            BorderSide inside = null,
            BorderSide outside = null
        ) {
            inside = inside ?? BorderSide.none;
            outside = outside ?? BorderSide.none;
            return new TableBorder(
                top: outside,
                right: outside,
                bottom: outside,
                left: outside,
                horizontalInside: inside,
                verticalInside: inside
            );
        }

        public readonly BorderSide top;

        public readonly BorderSide right;

        public readonly BorderSide bottom;

        public readonly BorderSide left;

        public readonly BorderSide horizontalInside;

        public readonly BorderSide verticalInside;

        public EdgeInsets dimensions {
            get {
                return EdgeInsets.fromLTRB(this.left.width,
                    this.top.width,
                    this.right.width,
                    this.bottom.width);
            }
        }

        public bool isUniform {
            get {
                D.assert(this.top != null);
                D.assert(this.right != null);
                D.assert(this.bottom != null);
                D.assert(this.left != null);
                D.assert(this.horizontalInside != null);
                D.assert(this.verticalInside != null);

                Color topColor = this.top.color;
                if (this.right.color != topColor ||
                    this.bottom.color != topColor ||
                    this.left.color != topColor ||
                    this.horizontalInside.color != topColor ||
                    this.verticalInside.color != topColor) {
                    return false;
                }

                float topWidth = this.top.width;
                if (this.right.width != topWidth ||
                    this.bottom.width != topWidth ||
                    this.left.width != topWidth ||
                    this.horizontalInside.width != topWidth ||
                    this.verticalInside.width != topWidth) {
                    return false;
                }

                BorderStyle topStyle = this.top.style;
                if (this.right.style != topStyle ||
                    this.bottom.style != topStyle ||
                    this.left.style != topStyle ||
                    this.horizontalInside.style != topStyle ||
                    this.verticalInside.style != topStyle) {
                    return false;
                }

                return true;
            }
        }

        TableBorder scale(float t) {
            return new TableBorder(
                top: this.top.scale(t),
                right: this.right.scale(t),
                bottom: this.bottom.scale(t),
                left: this.left.scale(t),
                horizontalInside: this.horizontalInside.scale(t),
                verticalInside: this.verticalInside.scale(t)
            );
        }

        public static TableBorder lerp(TableBorder a, TableBorder b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b.scale(t);
            }

            if (b == null) {
                return a.scale(1.0f - t);
            }

            return new TableBorder(
                top: BorderSide.lerp(a.top, b.top, t),
                right: BorderSide.lerp(a.right, b.right, t),
                bottom: BorderSide.lerp(a.bottom, b.bottom, t),
                left: BorderSide.lerp(a.left, b.left, t),
                horizontalInside: BorderSide.lerp(a.horizontalInside, b.horizontalInside, t),
                verticalInside: BorderSide.lerp(a.verticalInside, b.verticalInside, t)
            );
        }

        public void paint(Canvas canvas, Rect rect, List<float> rows, List<float> columns) {
            D.assert(this.top != null);
            D.assert(this.right != null);
            D.assert(this.bottom != null);
            D.assert(this.left != null);
            D.assert(this.horizontalInside != null);
            D.assert(this.verticalInside != null);

            D.assert(canvas != null);
            D.assert(rect != null);
            D.assert(rows != null);
            D.assert(rows.isEmpty() || (rows.First() >= 0.0f && rows.Last() <= rect.height));
            D.assert(columns != null);
            D.assert(columns.isEmpty() || (columns.First() >= 0.0f && columns.Last() <= rect.width));

            if (columns.isNotEmpty() || rows.isNotEmpty()) {
                Paint paint = new Paint();

                if (columns.isNotEmpty()) {
                    switch (this.verticalInside.style) {
                        case BorderStyle.solid: {
                            paint.color = this.verticalInside.color;
                            paint.strokeWidth = this.verticalInside.width;
                            paint.style = PaintingStyle.stroke;
                            Path path = new Path();

                            foreach (float x in columns) {
                                path.moveTo(rect.left + x, rect.top);
                                path.lineTo(rect.left + x, rect.bottom);
                            }

                            canvas.drawPath(path, paint);
                            break;
                        }
                        case BorderStyle.none: {
                            break;
                        }
                    }
                }

                if (rows.isNotEmpty()) {
                    switch (this.horizontalInside.style) {
                        case BorderStyle.solid: {
                            paint.color = this.horizontalInside.color;
                            paint.strokeWidth = this.horizontalInside.width;
                            paint.style = PaintingStyle.stroke;
                            Path path = new Path();

                            foreach (float y in rows) {
                                path.moveTo(rect.left, rect.top + y);
                                path.lineTo(rect.right, rect.top + y);
                            }

                            canvas.drawPath(path, paint);
                            break;
                        }
                        case BorderStyle.none: {
                            break;
                        }
                    }
                }

                BorderUtils.paintBorder(canvas, rect, top: this.top, right: this.right, bottom: this.bottom,
                    left: this.left);
            }
        }

        public bool Equals(TableBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return other.top == this.top &&
                   other.right == this.right &&
                   other.bottom == this.bottom &&
                   other.left == this.left &&
                   other.horizontalInside == this.horizontalInside &&
                   other.verticalInside == this.verticalInside;
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

            return this.Equals((TableBorder) obj);
        }

        public static bool operator ==(TableBorder left, TableBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(TableBorder left, TableBorder right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.top.GetHashCode();
                hashCode = (hashCode * 397) ^ this.right.GetHashCode();
                hashCode = (hashCode * 397) ^ this.bottom.GetHashCode();
                hashCode = (hashCode * 397) ^ this.left.GetHashCode();
                hashCode = (hashCode * 397) ^ this.horizontalInside.GetHashCode();
                hashCode = (hashCode * 397) ^ this.verticalInside.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() {
            return
                $"TableBorder({this.top}, {this.right}, {this.bottom}, {this.left}, {this.horizontalInside}, {this.verticalInside})";
        }
    }
}