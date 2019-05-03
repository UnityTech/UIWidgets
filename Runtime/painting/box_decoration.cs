using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public class BoxDecoration : Decoration, IEquatable<BoxDecoration> {
        public BoxDecoration(
            Color color = null,
            DecorationImage image = null,
            Border border = null,
            BorderRadius borderRadius = null,
            List<BoxShadow> boxShadow = null,
            Gradient gradient = null,
            BlendMode? backgroundBlendMode = null,
            BoxShape shape = BoxShape.rectangle
        ) {
            D.assert(
                backgroundBlendMode == null || color != null || gradient != null,
                () => "backgroundBlendMode applies to BoxDecoration\'s background color or " +
                "gradient, but no color or gradient was provided."
            );

            this.color = color;
            this.image = image;
            this.border = border;
            this.borderRadius = borderRadius;
            this.boxShadow = boxShadow;
            this.gradient = gradient;
            this.backgroundBlendMode = backgroundBlendMode;
            this.shape = shape;
        }

        public override bool debugAssertIsValid() {
            D.assert(this.shape != BoxShape.circle || this.borderRadius == null);
            return base.debugAssertIsValid();
        }

        public readonly Color color;
        public readonly DecorationImage image;
        public readonly Border border;
        public readonly BorderRadius borderRadius;
        public readonly List<BoxShadow> boxShadow;
        public readonly Gradient gradient;
        public readonly BlendMode? backgroundBlendMode;
        public readonly BoxShape shape;

        public override EdgeInsets padding {
            get { return this.border?.dimensions; }
        }

        public BoxDecoration scale(float factor) {
            return new BoxDecoration(
                color: Color.lerp(null, this.color, factor),
                image: this.image,
                border: Border.lerp(null, this.border, factor),
                borderRadius: BorderRadius.lerp(null, this.borderRadius, factor),
                boxShadow: BoxShadow.lerpList(null, this.boxShadow, factor),
                gradient: this.gradient?.scale(factor),
                backgroundBlendMode: this.backgroundBlendMode,
                shape: this.shape
            );
        }

        public override bool isComplex {
            get { return this.boxShadow != null; }
        }

        public override Decoration lerpFrom(Decoration a, float t) {
            if (a == null) {
                return this.scale(t);
            }

            if (a is BoxDecoration boxDecoration) {
                return lerp(boxDecoration, this, t);
            }

            return base.lerpFrom(a, t);
        }

        public override Decoration lerpTo(Decoration b, float t) {
            if (b == null) {
                return this.scale(1.0f - t);
            }

            if (b is BoxDecoration boxDecoration) {
                return lerp(this, boxDecoration, t);
            }

            return base.lerpTo(b, t);
        }

        public static BoxDecoration lerp(BoxDecoration a, BoxDecoration b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return b.scale(t);
            }

            if (b == null) {
                return a.scale(1.0f - t);
            }

            if (t == 0.0) {
                return a;
            }

            if (t == 1.0) {
                return b;
            }

            return new BoxDecoration(
                color: Color.lerp(a.color, b.color, t),
                image: t < 0.5 ? a.image : b.image,
                border: Border.lerp(a.border, b.border, t),
                borderRadius: BorderRadius.lerp(a.borderRadius, b.borderRadius, t),
                boxShadow: BoxShadow.lerpList(a.boxShadow, b.boxShadow, t),
                gradient: Gradient.lerp(a.gradient, b.gradient, t),
                backgroundBlendMode: t < 0.5 ? a.backgroundBlendMode : b.backgroundBlendMode,
                shape: t < 0.5 ? a.shape : b.shape
            );
        }

        public bool Equals(BoxDecoration other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.color, other.color) && Equals(this.image, other.image) &&
                   Equals(this.border, other.border) && Equals(this.borderRadius, other.borderRadius) &&
                   Equals(this.boxShadow, other.boxShadow) && Equals(this.gradient, other.gradient) &&
                   this.backgroundBlendMode == other.backgroundBlendMode && this.shape == other.shape;
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
                hashCode = (hashCode * 397) ^ this.backgroundBlendMode.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) this.shape;
                return hashCode;
            }
        }

        public static bool operator ==(BoxDecoration left, BoxDecoration right) {
            return Equals(left, right);
        }

        public static bool operator !=(BoxDecoration left, BoxDecoration right) {
            return !Equals(left, right);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.defaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.whitespace;
            properties.emptyBodyDescription = "<no decorations specified>";
            properties.add(new DiagnosticsProperty<Color>("color", this.color,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<DecorationImage>("image", this.image,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Border>("border", this.border,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<BorderRadius>("borderRadius", this.borderRadius,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new EnumerableProperty<BoxShadow>("boxShadow", this.boxShadow,
                defaultValue: Diagnostics.kNullDefaultValue, style: DiagnosticsTreeStyle.whitespace));
            properties.add(new DiagnosticsProperty<Gradient>("gradient", this.gradient,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<BlendMode?>("backgroundBlendMode", this.backgroundBlendMode,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<BoxShape>("shape", this.shape, defaultValue: BoxShape.rectangle));
        }

        public override bool hitTest(Size size, Offset position) {
            D.assert((Offset.zero & size).contains(position));
            switch (this.shape) {
                case BoxShape.rectangle:
                    if (this.borderRadius != null) {
                        RRect bounds = this.borderRadius.toRRect(Offset.zero & size);
                        return bounds.contains(position);
                    }

                    return true;
                case BoxShape.circle:
                    Offset center = size.center(Offset.zero);
                    float distance = (position - center).distance;
                    return distance <= Mathf.Min(size.width, size.height) / 2.0;
            }

            return false;
        }

        public override BoxPainter createBoxPainter(VoidCallback onChanged = null) {
            D.assert(onChanged != null || this.image == null);
            return new _BoxDecorationPainter(this, onChanged);
        }
    }

    class _BoxDecorationPainter : BoxPainter {
        public _BoxDecorationPainter(BoxDecoration decoration, VoidCallback onChanged)
            : base(onChanged) {
            D.assert(decoration != null);
            this._decoration = decoration;
        }

        readonly BoxDecoration _decoration;

        Paint _cachedBackgroundPaint;

        Rect _rectForCachedBackgroundPaint;

        Paint _getBackgroundPaint(Rect rect) {
            D.assert(rect != null);
            D.assert(this._decoration.gradient != null || this._rectForCachedBackgroundPaint == null);

            if (this._cachedBackgroundPaint == null ||
                (this._decoration.gradient != null && this._rectForCachedBackgroundPaint != rect)) {
                var paint = new Paint();
                if (this._decoration.backgroundBlendMode != null) {
                    paint.blendMode = this._decoration.backgroundBlendMode.Value;
                }

                if (this._decoration.color != null) {
                    paint.color = this._decoration.color;
                }

                if (this._decoration.gradient != null) {
                    paint.shader = this._decoration.gradient.createShader(rect);
                    this._rectForCachedBackgroundPaint = rect;
                }

                this._cachedBackgroundPaint = paint;
            }

            return this._cachedBackgroundPaint;
        }

        void _paintBox(Canvas canvas, Rect rect, Paint paint) {
            switch (this._decoration.shape) {
                case BoxShape.circle:
                    D.assert(this._decoration.borderRadius == null);
                    Offset center = rect.center;
                    float radius = rect.shortestSide / 2.0f;
                    canvas.drawCircle(center, radius, paint);
                    break;
                case BoxShape.rectangle:
                    if (this._decoration.borderRadius == null) {
                        canvas.drawRect(rect, paint);
                    }
                    else {
                        canvas.drawRRect(this._decoration.borderRadius.toRRect(rect), paint);
                    }

                    break;
            }
        }

        void _paintShadows(Canvas canvas, Rect rect) {
            if (this._decoration.boxShadow == null) {
                return;
            }

            foreach (BoxShadow boxShadow in this._decoration.boxShadow) {
                Paint paint = boxShadow.toPaint();
                Rect bounds = rect.shift(boxShadow.offset).inflate(boxShadow.spreadRadius);
                this._paintBox(canvas, bounds, paint);
            }
        }

        void _paintBackgroundColor(Canvas canvas, Rect rect) {
            if (this._decoration.color != null || this._decoration.gradient != null) {
                this._paintBox(canvas, rect, this._getBackgroundPaint(rect));
            }
        }

        DecorationImagePainter _imagePainter;

        void _paintBackgroundImage(Canvas canvas, Rect rect, ImageConfiguration configuration) {
            if (this._decoration.image == null) {
                return;
            }

            this._imagePainter = this._imagePainter ?? this._decoration.image.createPainter(this.onChanged);

            Path clipPath = null;
            switch (this._decoration.shape) {
                case BoxShape.circle:
                    clipPath = new Path();
                    clipPath.addOval(rect);
                    break;
                case BoxShape.rectangle:
                    if (this._decoration.borderRadius != null) {
                        clipPath = new Path();
                        clipPath.addRRect(this._decoration.borderRadius.toRRect(rect));
                    }

                    break;
            }

            this._imagePainter.paint(canvas, rect, clipPath, configuration);
        }

        public override void Dispose() {
            this._imagePainter?.Dispose();
            base.Dispose();
        }

        public override void paint(Canvas canvas, Offset offset, ImageConfiguration configuration) {
            D.assert(configuration != null);
            D.assert(configuration.size != null);

            Rect rect = offset & configuration.size;

            this._paintShadows(canvas, rect);
            this._paintBackgroundColor(canvas, rect);
            this._paintBackgroundImage(canvas, rect, configuration);
            this._decoration.border?.paint(
                canvas,
                rect,
                shape: this._decoration.shape,
                borderRadius: this._decoration.borderRadius
            );
        }

        public override string ToString() {
            return $"BoxPainter for {this._decoration}";
        }
    }
}