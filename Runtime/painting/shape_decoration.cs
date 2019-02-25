using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class ShapeDecoration : Decoration, IEquatable<ShapeDecoration> {
        public ShapeDecoration(
            Color color = null,
            DecorationImage image = null,
            Gradient gradient = null,
            List<BoxShadow> shadows = null,
            ShapeBorder shape = null
        ) {
            D.assert(!(color != null && gradient != null));
            D.assert(shape != null);

            this.color = color;
            this.image = image;
            this.gradient = gradient;
            this.shadows = shadows;
            this.shape = shape;
        }

        public readonly Color color;
        public readonly DecorationImage image;
        public readonly Gradient gradient;
        public readonly List<BoxShadow> shadows;
        public readonly ShapeBorder shape;

        public static ShapeDecoration fromBoxDecoration(BoxDecoration source) {
            ShapeBorder shape = null;

            switch (source.shape) {
                case BoxShape.circle:
                    if (source.border != null) {
                        D.assert(source.border.isUniform);
                        shape = new CircleBorder(side: source.border.top);
                    }
                    else {
                        shape = new CircleBorder();
                    }

                    break;
                case BoxShape.rectangle:
                    if (source.borderRadius != null) {
                        D.assert(source.border == null || source.border.isUniform);
                        shape = new RoundedRectangleBorder(
                            side: source.border?.top ?? BorderSide.none,
                            borderRadius: source.borderRadius
                        );
                    }
                    else {
                        shape = source.border ?? new Border();
                    }

                    break;
            }

            return new ShapeDecoration(
                color: source.color,
                image: source.image,
                gradient: source.gradient,
                shadows: source.boxShadow,
                shape: shape
            );
        }

        public override EdgeInsets padding {
            get { return this.shape.dimensions; }
        }

        public override bool isComplex {
            get { return this.shadows != null; }
        }

        public override Decoration lerpFrom(Decoration a, float t) {
            if (a is BoxDecoration decoration) {
                return ShapeDecoration.lerp(ShapeDecoration.fromBoxDecoration(decoration), this, t);
            }
            else if (a == null || a is ShapeDecoration) {
                return ShapeDecoration.lerp(a, this, t);
            }

            return base.lerpFrom(a, t);
        }

        public override Decoration lerpTo(Decoration b, float t) {
            if (b is BoxDecoration decoration) {
                return ShapeDecoration.lerp(this, fromBoxDecoration(decoration), t);
            }
            else if (b == null || b is ShapeDecoration) {
                return ShapeDecoration.lerp(this, b, t);
            }

            return base.lerpTo(b, t);
        }

        public static ShapeDecoration lerp(ShapeDecoration a, ShapeDecoration b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a != null && b != null) {
                if (t == 0.0) {
                    return a;
                }

                if (t == 1.0) {
                    return b;
                }
            }

            return new ShapeDecoration(
                color: Color.lerp(a?.color, b?.color, t),
                gradient: Gradient.lerp(a?.gradient, b?.gradient, t),
                image: t < 0.5 ? a.image : b.image,
                shadows: BoxShadow.lerpList(a?.shadows, b?.shadows, t),
                shape: ShapeBorder.lerp(a?.shape, b?.shape, t)
            );
        }

        public bool Equals(ShapeDecoration other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.color, other.color) && Equals(this.image, other.image) &&
                   Equals(this.gradient, other.gradient) && Equals(this.shadows, other.shadows) &&
                   Equals(this.shape, other.shape);
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

            return this.Equals((ShapeDecoration) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.color != null ? this.color.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.image != null ? this.image.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.gradient != null ? this.gradient.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.shadows != null ? this.shadows.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.shape != null ? this.shape.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ShapeDecoration left, ShapeDecoration right) {
            return Equals(left, right);
        }

        public static bool operator !=(ShapeDecoration left, ShapeDecoration right) {
            return !Equals(left, right);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.defaultDiagnosticsTreeStyle = DiagnosticsTreeStyle.whitespace;
            properties.add(new DiagnosticsProperty<Color>("color", this.color,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<Gradient>("gradient", this.gradient,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<DecorationImage>("image", this.image,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new EnumerableProperty<BoxShadow>("shadows", this.shadows,
                defaultValue: Diagnostics.kNullDefaultValue, style: DiagnosticsTreeStyle.whitespace));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", this.shape));
        }

        public override bool hitTest(Size size, Offset position) {
            return this.shape.getOuterPath(Offset.zero & size).contains(position);
        }

        public override BoxPainter createBoxPainter(VoidCallback onChanged = null) {
            D.assert(onChanged != null || this.image == null);
            return new _ShapeDecorationPainter(this, onChanged);
        }
    }

    class _ShapeDecorationPainter : BoxPainter {
        public _ShapeDecorationPainter(ShapeDecoration decoration, VoidCallback onChanged)
            : base(onChanged) {
            D.assert(decoration != null);
            this._decoration = decoration;
        }

        readonly ShapeDecoration _decoration;

        Rect _lastRect;
        Path _outerPath;
        Path _innerPath;
        Paint _interiorPaint;
        int? _shadowCount;
        Path[] _shadowPaths;
        Paint[] _shadowPaints;

        void _precache(Rect rect) {
            D.assert(rect != null);
            if (rect == this._lastRect) {
                return;
            }

            if (this._interiorPaint == null && (this._decoration.color != null || this._decoration.gradient != null)) {
                this._interiorPaint = new Paint();
                if (this._decoration.color != null) {
                    this._interiorPaint.color = this._decoration.color;
                }
            }

            if (this._decoration.gradient != null) {
                // this._interiorPaint.shader = this._decoration.gradient.createShader(rect);
            }

            if (this._decoration.shadows != null) {
                if (this._shadowCount == null) {
                    this._shadowCount = this._decoration.shadows.Count;
                    this._shadowPaths = new Path[this._shadowCount.Value];
                    this._shadowPaints = new Paint[this._shadowCount.Value];
                    for (int index = 0; index < this._shadowCount.Value; index += 1) {
                        this._shadowPaints[index] = this._decoration.shadows[index].toPaint();
                    }
                }

                for (int index = 0; index < this._shadowCount; index += 1) {
                    BoxShadow shadow = this._decoration.shadows[index];
                    this._shadowPaths[index] = this._decoration.shape.getOuterPath(
                        rect.shift(shadow.offset).inflate(shadow.spreadRadius));
                }
            }

            if (this._interiorPaint != null || this._shadowCount != null) {
                this._outerPath = this._decoration.shape.getOuterPath(rect);
            }

            if (this._decoration.image != null) {
                this._innerPath = this._decoration.shape.getInnerPath(rect);
            }

            this._lastRect = rect;
        }

        void _paintShadows(Canvas canvas) {
            if (this._shadowCount != null) {
                for (int index = 0; index < this._shadowCount.Value; index += 1) {
                    canvas.drawPath(this._shadowPaths[index], this._shadowPaints[index]);
                }
            }
        }

        void _paintInterior(Canvas canvas) {
            if (this._interiorPaint != null) {
                canvas.drawPath(this._outerPath, this._interiorPaint);
            }
        }

        DecorationImagePainter _imagePainter;

        void _paintImage(Canvas canvas, ImageConfiguration configuration) {
            if (this._decoration.image == null) {
                return;
            }

            this._imagePainter = this._imagePainter ?? this._decoration.image.createPainter(this.onChanged);
            this._imagePainter.paint(canvas, this._lastRect, this._innerPath, configuration);
        }

        public override void Dispose() {
            this._imagePainter?.Dispose();
            base.Dispose();
        }

        public override void paint(Canvas canvas, Offset offset, ImageConfiguration configuration) {
            D.assert(configuration != null);
            D.assert(configuration.size != null);
            Rect rect = offset & configuration.size;
            this._precache(rect);
            this._paintShadows(canvas);
            this._paintInterior(canvas);
            this._paintImage(canvas, configuration);
            this._decoration.shape.paint(canvas, rect);
        }
    }
}