using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.widgets {
    public class ScrollbarPainter : ChangeNotifier, CustomPainter {
        public ScrollbarPainter(
            Color color,
            TextDirection textDirection,
            float thickness,
            Animation<float> fadeoutOpacityAnimation,
            float mainAxisMargin = 0.0f,
            float crossAxisMargin = 0.0f,
            Radius radius = null,
            float minLength = _kMinThumbExtent,
            float minOverscrollLength = _kMinThumbExtent
        ) {
            this.color = color;
            this.textDirection = textDirection;
            this.thickness = thickness;
            this.fadeoutOpacityAnimation = fadeoutOpacityAnimation;
            this.mainAxisMargin = mainAxisMargin;
            this.crossAxisMargin = crossAxisMargin;
            this.radius = radius;
            this.minLength = minLength;
            this.minOverscrollLength = minOverscrollLength;
            fadeoutOpacityAnimation.addListener(this.notifyListeners);
        }

        const float _kMinThumbExtent = 18.0f;

        public Color color;
        public TextDirection? textDirection;
        public float thickness;
        public Animation<float> fadeoutOpacityAnimation;
        public float mainAxisMargin;
        public float crossAxisMargin;
        public Radius radius;
        public float minLength;
        public float minOverscrollLength;

        ScrollMetrics _lastMetrics;
        AxisDirection? _lastAxisDirection;

        public void update(ScrollMetrics metrics, AxisDirection axisDirection) {
            this._lastMetrics = metrics;
            this._lastAxisDirection = axisDirection;
            this.notifyListeners();
        }

        Paint _paint {
            get {
                var paint = new Paint();
                paint.color = this.color.withOpacity(this.color.opacity * this.fadeoutOpacityAnimation.value);
                return paint;
            }
        }

        float _getThumbX(Size size) {
            D.assert(this.textDirection != null);
            switch (this.textDirection) {
                case TextDirection.rtl:
                    return this.crossAxisMargin;
                case TextDirection.ltr:
                    return size.width - this.thickness - this.crossAxisMargin;
            }

            return 0;
        }

        void _paintVerticalThumb(Canvas canvas, Size size, float thumbOffset, float thumbExtent) {
            Offset thumbOrigin = new Offset(this._getThumbX(size), thumbOffset);
            Size thumbSize = new Size(this.thickness, thumbExtent);
            Rect thumbRect = thumbOrigin & thumbSize;
            if (this.radius == null) {
                canvas.drawRect(thumbRect, this._paint);
            }
            else {
                canvas.drawRRect(RRect.fromRectAndRadius(thumbRect, this.radius), this._paint);
            }
        }

        void _paintHorizontalThumb(Canvas canvas, Size size, float thumbOffset, float thumbExtent) {
            Offset thumbOrigin = new Offset(thumbOffset, size.height - this.thickness);
            Size thumbSize = new Size(thumbExtent, this.thickness);
            Rect thumbRect = thumbOrigin & thumbSize;
            if (this.radius == null) {
                canvas.drawRect(thumbRect, this._paint);
            }
            else {
                canvas.drawRRect(RRect.fromRectAndRadius(thumbRect, this.radius), this._paint);
            }
        }

        public delegate void painterDelegate(Canvas canvas, Size size, float thumbOffset, float thumbExtent);

        void _paintThumb(
            float before,
            float inside,
            float after,
            float viewport,
            Canvas canvas,
            Size size,
            painterDelegate painter
        ) {
            float thumbExtent = Mathf.Min(viewport, this.minOverscrollLength);

            if (before + inside + after > 0.0) {
                float fractionVisible = inside / (before + inside + after);
                thumbExtent = Mathf.Max(
                    thumbExtent,
                    viewport * fractionVisible - 2 * this.mainAxisMargin
                );

                if (before != 0.0 && after != 0.0) {
                    thumbExtent = Mathf.Max(
                        this.minLength,
                        thumbExtent
                    );
                }
                else {
                    thumbExtent = Mathf.Max(
                        thumbExtent,
                        this.minLength * (((inside / viewport) - 0.8f) / 0.2f)
                    );
                }

                float fractionPast = before / (before + after);
                float thumbOffset = (before + after > 0.0)
                    ? fractionPast * (viewport - thumbExtent - 2 * this.mainAxisMargin) + this.mainAxisMargin
                    : this.mainAxisMargin;

                painter(canvas, size, thumbOffset, thumbExtent);
            }
        }

        public override void dispose() {
            this.fadeoutOpacityAnimation.removeListener(this.notifyListeners);
            base.dispose();
        }


        public void paint(Canvas canvas, Size size) {
            if (this._lastAxisDirection == null
                || this._lastMetrics == null
                || this.fadeoutOpacityAnimation.value == 0.0) {
                return;
            }

            switch (this._lastAxisDirection) {
                case AxisDirection.down:
                    this._paintThumb(this._lastMetrics.extentBefore(), this._lastMetrics.extentInside(),
                        this._lastMetrics.extentAfter(), size.height, canvas, size, this._paintVerticalThumb);
                    break;
                case AxisDirection.up:
                    this._paintThumb(this._lastMetrics.extentAfter(), this._lastMetrics.extentInside(),
                        this._lastMetrics.extentBefore(), size.height, canvas, size, this._paintVerticalThumb);
                    break;
                case AxisDirection.right:
                    this._paintThumb(this._lastMetrics.extentBefore(), this._lastMetrics.extentInside(),
                        this._lastMetrics.extentAfter(), size.width, canvas, size, this._paintHorizontalThumb);
                    break;
                case AxisDirection.left:
                    this._paintThumb(this._lastMetrics.extentAfter(), this._lastMetrics.extentInside(),
                        this._lastMetrics.extentBefore(), size.width, canvas, size, this._paintHorizontalThumb);
                    break;
            }
        }

        public bool? hitTest(Offset position) {
            return false;
        }

        public bool shouldRepaint(CustomPainter oldRaw) {
            if (oldRaw is ScrollbarPainter old) {
                return this.color != old.color
                       || this.textDirection != old.textDirection
                       || this.thickness != old.thickness
                       || this.fadeoutOpacityAnimation != old.fadeoutOpacityAnimation
                       || this.mainAxisMargin != old.mainAxisMargin
                       || this.crossAxisMargin != old.crossAxisMargin
                       || this.radius != old.radius
                       || this.minLength != old.minLength;
            }

            return false;
        }
    }
}