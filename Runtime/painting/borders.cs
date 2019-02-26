using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public enum BorderStyle {
        none,
        solid,
    }

    public class BorderSide : IEquatable<BorderSide> {
        public BorderSide(
            Color color = null,
            float width = 1.0f,
            BorderStyle style = BorderStyle.solid
        ) {
            this.color = color ?? Color.black;
            this.width = width;
            this.style = style;
        }

        public static BorderSide merge(BorderSide a, BorderSide b) {
            D.assert(a != null);
            D.assert(b != null);
            D.assert(canMerge(a, b));
            bool aIsNone = a.style == BorderStyle.none && a.width == 0.0;
            bool bIsNone = b.style == BorderStyle.none && b.width == 0.0;

            if (aIsNone && bIsNone) {
                return none;
            }

            if (aIsNone) {
                return b;
            }

            if (bIsNone) {
                return a;
            }

            D.assert(a.color == b.color);
            D.assert(a.style == b.style);
            return new BorderSide(
                color: a.color, // == b.color
                width: a.width + b.width,
                style: a.style // == b.style
            );
        }

        public readonly Color color;
        public readonly float width;
        public readonly BorderStyle style;

        public static readonly BorderSide none = new BorderSide(width: 0.0f, style: BorderStyle.none);

        public BorderSide copyWith(
            Color color = null,
            float? width = null,
            BorderStyle? style = null
        ) {
            D.assert(width == null || width >= 0.0);

            return new BorderSide(
                color: color ?? this.color,
                width: width ?? this.width,
                style: style ?? this.style
            );
        }

        public BorderSide scale(float t) {
            return new BorderSide(
                color: this.color,
                width: Mathf.Max(0.0f, this.width * t),
                style: t <= 0.0 ? BorderStyle.none : this.style
            );
        }

        public Paint toPaint() {
            switch (this.style) {
                case BorderStyle.solid:
                    return new Paint {
                        color = this.color,
                        strokeWidth = this.width,
                        style = PaintingStyle.stroke,
                    };
                case BorderStyle.none:
                    return new Paint {
                        color = Color.clear,
                        strokeWidth = 0.0f,
                        style = PaintingStyle.stroke
                    };
            }

            return null;
        }

        public static bool canMerge(BorderSide a, BorderSide b) {
            D.assert(a != null);
            D.assert(b != null);
            if ((a.style == BorderStyle.none && a.width == 0.0) ||
                (b.style == BorderStyle.none && b.width == 0.0)) {
                return true;
            }

            return a.style == b.style && a.color == b.color;
        }

        public static BorderSide lerp(BorderSide a, BorderSide b, float t) {
            D.assert(a != null);
            D.assert(b != null);
            if (t == 0.0f) {
                return a;
            }

            if (t == 1.0f) {
                return b;
            }

            float width = MathUtils.lerpFloat(a.width, b.width, t);
            if (width < 0.0) {
                return none;
            }

            if (a.style == b.style) {
                return new BorderSide(
                    color: Color.lerp(a.color, b.color, t),
                    width: width,
                    style: a.style // == b.style
                );
            }

            Color colorA = null, colorB = null;
            switch (a.style) {
                case BorderStyle.solid:
                    colorA = a.color;
                    break;
                case BorderStyle.none:
                    colorA = a.color.withAlpha(0x00);
                    break;
            }

            switch (b.style) {
                case BorderStyle.solid:
                    colorB = b.color;
                    break;
                case BorderStyle.none:
                    colorB = b.color.withAlpha(0x00);
                    break;
            }

            return new BorderSide(
                color: Color.lerp(colorA, colorB, t),
                width: width,
                style: BorderStyle.solid
            );
        }

        public bool Equals(BorderSide other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.color, other.color) && this.width.Equals(other.width) && this.style == other.style;
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

            return this.Equals((BorderSide) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.color != null ? this.color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.width.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) this.style;
                return hashCode;
            }
        }

        public static bool operator ==(BorderSide left, BorderSide right) {
            return Equals(left, right);
        }

        public static bool operator !=(BorderSide left, BorderSide right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.color}, {this.width:F1}, {this.style})";
        }
    }

    public abstract class ShapeBorder {
        protected ShapeBorder() {
        }

        public abstract EdgeInsets dimensions { get; }

        public virtual ShapeBorder add(ShapeBorder other, bool reversed = false) {
            return null;
        }

        public static ShapeBorder operator +(ShapeBorder it, ShapeBorder other) {
            return it.add(other) ?? other.add(it, reversed: true) ??
                   new _CompoundBorder(new List<ShapeBorder> {other, it});
        }

        public abstract ShapeBorder scale(float t);

        public virtual ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a == null) {
                return this.scale(t);
            }

            return null;
        }

        public virtual ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b == null) {
                return this.scale(1.0f - t);
            }

            return null;
        }

        public static ShapeBorder lerp(ShapeBorder a, ShapeBorder b, float t) {
            ShapeBorder result = null;
            if (b != null) {
                result = b.lerpFrom(a, t);
            }

            if (result == null && a != null) {
                result = a.lerpTo(b, t);
            }

            return result ?? (t < 0.5 ? a : b);
        }

        public abstract Path getOuterPath(Rect rect);

        public abstract Path getInnerPath(Rect rect);

        public abstract void paint(Canvas canvas, Rect rect);

        public override string ToString() {
            return $"{this.GetType()}()";
        }
    }

    class _CompoundBorder : ShapeBorder, IEquatable<_CompoundBorder> {
        public _CompoundBorder(List<ShapeBorder> borders) {
            D.assert(borders != null);
            D.assert(borders.Count >= 2);
            D.assert(!borders.Any(border => border is _CompoundBorder));

            this.borders = borders;
        }

        public readonly List<ShapeBorder> borders;

        public override EdgeInsets dimensions {
            get {
                return this.borders.Aggregate(
                    EdgeInsets.zero,
                    (previousValue, border) => previousValue.add(border.dimensions));
            }
        }

        public override ShapeBorder add(ShapeBorder other, bool reversed = false) {
            if (!(other is _CompoundBorder)) {
                ShapeBorder ours = reversed ? this.borders.Last() : this.borders.First();
                ShapeBorder merged = ours.add(other, reversed: reversed) ?? other.add(ours, reversed: !reversed);
                if (merged != null) {
                    List<ShapeBorder> result = new List<ShapeBorder>(this.borders);
                    result[reversed ? result.Count - 1 : 0] = merged;
                    return new _CompoundBorder(result);
                }
            }

            List<ShapeBorder> mergedBorders = new List<ShapeBorder>();
            if (reversed) {
                mergedBorders.AddRange(this.borders);
            }

            if (other is _CompoundBorder border) {
                mergedBorders.AddRange(border.borders);
            }
            else {
                mergedBorders.Add(other);
            }

            if (!reversed) {
                mergedBorders.AddRange(this.borders);
            }

            return new _CompoundBorder(mergedBorders);
        }

        public override ShapeBorder scale(float t) {
            return new _CompoundBorder(
                this.borders.Select(border => border.scale(t)).ToList()
            );
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            return lerp(a, this, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            return lerp(this, b, t);
        }

        public new static _CompoundBorder lerp(ShapeBorder a, ShapeBorder b, float t) {
            D.assert(a is _CompoundBorder || b is _CompoundBorder);
            List<ShapeBorder> aList = a is _CompoundBorder aBorder ? aBorder.borders : new List<ShapeBorder> {a};
            List<ShapeBorder> bList = b is _CompoundBorder bBorder ? bBorder.borders : new List<ShapeBorder> {b};
            List<ShapeBorder> results = new List<ShapeBorder>();
            int length = Mathf.Max(aList.Count, bList.Count);
            for (int index = 0; index < length; index += 1) {
                ShapeBorder localA = index < aList.Count ? aList[index] : null;
                ShapeBorder localB = index < bList.Count ? bList[index] : null;
                if (localA != null && localB != null) {
                    ShapeBorder localResult = localA.lerpTo(localB, t) ?? localB.lerpFrom(localA, t);
                    if (localResult != null) {
                        results.Add(localResult);
                        continue;
                    }
                }

                if (localB != null) {
                    results.Add(localB.scale(t));
                }

                if (localA != null) {
                    results.Add(localA.scale(1.0f - t));
                }
            }

            return new _CompoundBorder(results);
        }

        public override Path getInnerPath(Rect rect) {
            for (int index = 0; index < this.borders.Count - 1; index += 1) {
                rect = this.borders[index].dimensions.deflateRect(rect);
            }

            return this.borders.Last().getInnerPath(rect);
        }

        public override Path getOuterPath(Rect rect) {
            return this.borders.First().getOuterPath(rect);
        }

        public override void paint(Canvas canvas, Rect rect) {
            foreach (ShapeBorder border in this.borders) {
                border.paint(canvas, rect);
                rect = border.dimensions.deflateRect(rect);
            }
        }

        public bool Equals(_CompoundBorder other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.borders.SequenceEqual(other.borders);
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

            return this.Equals((_CompoundBorder) obj);
        }

        public override int GetHashCode() {
            return (this.borders != null ? this.borders.GetHashCode() : 0);
        }

        public static bool operator ==(_CompoundBorder left, _CompoundBorder right) {
            return Equals(left, right);
        }

        public static bool operator !=(_CompoundBorder left, _CompoundBorder right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return string.Join(" + ",
                ((IList<ShapeBorder>) this.borders).Reverse().Select((border) => border.ToString()));
        }
    }

    public static class BorderUtils {
        public static void paintBorder(Canvas canvas, Rect rect,
            BorderSide top = null,
            BorderSide right = null,
            BorderSide bottom = null,
            BorderSide left = null
        ) {
            D.assert(canvas != null);
            D.assert(rect != null);
            top = top ?? BorderSide.none;
            right = right ?? BorderSide.none;
            bottom = bottom ?? BorderSide.none;
            left = left ?? BorderSide.none;

            switch (top.style) {
                case BorderStyle.solid:
                    Paint paint = new Paint {
                        strokeWidth = 0.0f,
                        color = top.color,
                    };

                    Path path = new Path();
                    path.moveTo(rect.left, rect.top);
                    path.lineTo(rect.right, rect.top);
                    if (top.width == 0.0f) {
                        paint.style = PaintingStyle.stroke;
                    }
                    else {
                        paint.style = PaintingStyle.fill;
                        path.lineTo(rect.right - right.width, rect.top + top.width);
                        path.lineTo(rect.left + left.width, rect.top + top.width);
                    }

                    canvas.drawPath(path, paint);
                    break;
                case BorderStyle.none:
                    break;
            }

            switch (right.style) {
                case BorderStyle.solid:
                    Paint paint = new Paint {
                        strokeWidth = 0.0f,
                        color = right.color,
                    };

                    Path path = new Path();
                    path.moveTo(rect.right, rect.top);
                    path.lineTo(rect.right, rect.bottom);
                    if (right.width == 0.0) {
                        paint.style = PaintingStyle.stroke;
                    }
                    else {
                        paint.style = PaintingStyle.fill;
                        path.lineTo(rect.right - right.width, rect.bottom - bottom.width);
                        path.lineTo(rect.right - right.width, rect.top + top.width);
                    }

                    canvas.drawPath(path, paint);
                    break;
                case BorderStyle.none:
                    break;
            }

            switch (bottom.style) {
                case BorderStyle.solid:
                    Paint paint = new Paint {
                        strokeWidth = 0.0f,
                        color = bottom.color,
                    };

                    Path path = new Path();
                    path.moveTo(rect.right, rect.bottom);
                    path.lineTo(rect.left, rect.bottom);
                    if (bottom.width == 0.0) {
                        paint.style = PaintingStyle.stroke;
                    }
                    else {
                        paint.style = PaintingStyle.fill;
                        path.lineTo(rect.left + left.width, rect.bottom - bottom.width);
                        path.lineTo(rect.right - right.width, rect.bottom - bottom.width);
                    }

                    canvas.drawPath(path, paint);
                    break;
                case BorderStyle.none:
                    break;
            }
            
            switch (left.style) {
                case BorderStyle.solid:
                    Paint paint = new Paint {
                        strokeWidth = 0.0f,
                        color = left.color,
                    };

                    Path path = new Path();
                    path.moveTo(rect.left, rect.bottom);
                    path.lineTo(rect.left, rect.top);
                    if (left.width == 0.0f) {
                        paint.style = PaintingStyle.stroke;
                    }
                    else {
                        paint.style = PaintingStyle.fill;
                        path.lineTo(rect.left + left.width, rect.top + top.width);
                        path.lineTo(rect.left + left.width, rect.bottom - bottom.width);
                    }

                    canvas.drawPath(path, paint);
                    break;
                case BorderStyle.none:
                    break;
            }
        }
    }
}