using System;
using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.ui;

namespace UIWidgets.painting {
    public class BoxDecoration : Decoration {
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


        public override EdgeInsetsGeometry padding {
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
    }


    public class _BoxDecorationPainter : BoxPainter {
        public _BoxDecorationPainter(BoxDecoration _decoration, VoidCallback onChanged) : base(onChanged) {
            this._decoration = _decoration;
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
                Rect bounds = rect.shift(boxShadow.offset).inflate(boxShadow.spreadRadius);

                Paint paint = new Paint {
                    color = boxShadow.color,
                    blurSigma = boxShadow.blurRadius
                };
                canvas.drawRectShadow(bounds, paint);
            }
        }

        public void _paintBackgroundColor(Canvas canvas, Rect rect) {
            if (this._decoration.color != null || this._decoration.gradient != null) {
                this._paintBox(canvas, rect, this._getBackgroundPaint(rect));
            }
        }

        public void _paintBackgroundImage(Canvas canvas, Rect rect, ImageConfiguration configuration) {
            if (this._decoration.image == null) {
                return;
            }

//            _imagePainter ??= _decoration.image.createPainter(onChanged);
//            Path clipPath;
//            switch (_decoration.shape) {
//                case BoxShape.circle:
//                    clipPath = new Path()..addOval(rect);
//                    break;
//                case BoxShape.rectangle:
//                    if (_decoration.borderRadius != null)
//                        clipPath = new Path()..addRRect(_decoration.borderRadius.resolve(configuration.textDirection).toRRect(rect));
//                    break;
//            }
//            _imagePainter.paint(canvas, rect, clipPath, configuration);
        }


        public void dispose() {
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
                    borderRadius: (BorderRadius) this._decoration.borderRadius
                );
            }
        }
    }
}