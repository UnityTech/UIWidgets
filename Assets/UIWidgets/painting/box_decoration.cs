using System;
using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.ui;
using UnityEditor;

namespace UIWidgets.painting {
    public class BoxDecoration : Decoration, IEquatable<BoxDecoration> {
        public BoxDecoration(
            Color color = null,
            DecorationImage image = null,
            Border border = null,
            BorderRadius borderRadius = null,
            List<BoxShadow> boxShadow = null,
            Gradient gradient = null
        ) {
            this.color = color;
            this.image = image;
            this.border = border;
            this.borderRadius = borderRadius;
            this.boxShadow = boxShadow;
            this.gradient = gradient;
        }

        public readonly Color color;
        public readonly DecorationImage image;
        public readonly Border border;
        public readonly BorderRadius borderRadius;
        public readonly List<BoxShadow> boxShadow;
        public readonly Gradient gradient;


        public override EdgeInsets padding {
            get {
                if (this.border != null) {
                    return this.border.dimensions;
                }

                return base.padding;
            }
        }

        public override BoxPainter createBoxPainter(VoidCallback onChanged = null) {
            return new _BoxDecorationPainter(this, onChanged);
        }

        public bool Equals(BoxDecoration other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return object.Equals(this.color, other.color)
                   && object.Equals(this.image, other.image)
                   && object.Equals(this.border, other.border)
                   && object.Equals(this.borderRadius, other.borderRadius)
                   && object.Equals(this.boxShadow, other.boxShadow)
                   && object.Equals(this.gradient, other.gradient);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((BoxDecoration) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.color != null ? this.color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.image != null ? this.image.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.border != null ? this.border.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.borderRadius != null ? this.borderRadius.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.boxShadow != null ? this.boxShadow.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.gradient != null ? this.gradient.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(BoxDecoration a, BoxDecoration b) {
            return object.Equals(a, b);
        }

        public static bool operator !=(BoxDecoration a, BoxDecoration b) {
            return !(a == b);
        }
    }


    public class _BoxDecorationPainter : BoxPainter {
        public _BoxDecorationPainter(BoxDecoration decoration, VoidCallback onChanged)
            : base(onChanged) {
            this._decoration = decoration;
        }

        public readonly BoxDecoration _decoration;

        public Paint _cachedBackgroundPaint;

        public Rect _rectForCachedBackgroundPaint;

        public Paint _getBackgroundPaint(Rect rect) {
            if (this._cachedBackgroundPaint == null) {
                var paint = new Paint();
                if (this._decoration.color != null) {
                    paint.color = this._decoration.color;
                }

                this._cachedBackgroundPaint = paint;
            }

            return this._cachedBackgroundPaint;
        }

        public void _paintBox(Canvas canvas, Rect rect, Paint paint) {
            canvas.drawRect(rect, null, this._decoration.borderRadius, paint);
        }

        public void _paintShadows(Canvas canvas, Rect rect) {
            if (this._decoration.boxShadow == null) {
                return;
            }

            foreach (BoxShadow boxShadow in this._decoration.boxShadow) {
                Paint paint = boxShadow.toPaint();
                Rect bounds = rect.shift(boxShadow.offset).inflate(boxShadow.spreadRadius);
                canvas.drawRectShadow(bounds, paint);
            }
        }

        public void _paintBackgroundColor(Canvas canvas, Rect rect) {
            if (this._decoration.color != null || this._decoration.gradient != null) {
                var paint = this._getBackgroundPaint(rect);
                canvas.drawRect(rect, null, this._decoration.borderRadius, paint);
            }
        }

        public void _paintBackgroundImage(Canvas canvas, Rect rect, ImageConfiguration configuration) {
            if (this._decoration.image == null) {
                return;
            }
        }

        public override void dispose() {
            base.dispose();
        }

        public override void paint(Canvas canvas, Offset offset, ImageConfiguration configuration) {
            Rect rect = offset & configuration.size;

            this._paintShadows(canvas, rect);
            this._paintBackgroundColor(canvas, rect);
            this._paintBackgroundImage(canvas, rect, configuration);
            if (this._decoration.border != null) {
                this._decoration.border.paint(
                    canvas,
                    rect,
                    borderRadius: this._decoration.borderRadius
                );
            }
        }
    }
}