using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.material {
    static class InkHighlightUtils {
        public static readonly TimeSpan _kHighlightFadeDuration = new TimeSpan(0, 0, 0, 0, 200);
    }

    public class InkHighlight : InteractiveInkFeature {
        public InkHighlight(
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            Color color = null,
            BoxShape shape = BoxShape.rectangle,
            BorderRadius borderRadius = null,
            ShapeBorder customBorder = null,
            RectCallback rectCallback = null,
            VoidCallback onRemoved = null) : base(
            controller: controller,
            referenceBox: referenceBox,
            color: color,
            onRemoved: onRemoved) {
            D.assert(color != null);
            D.assert(controller != null);
            D.assert(referenceBox != null);
            this._shape = shape;
            this._borderRadius = borderRadius ?? BorderRadius.zero;
            this._customBorder = customBorder;
            this._rectCallback = rectCallback;

            this._alphaController = new AnimationController(
                duration: InkHighlightUtils._kHighlightFadeDuration,
                vsync: controller.vsync);
            this._alphaController.addListener(controller.markNeedsPaint);
            this._alphaController.addStatusListener(this._handleAlphaStatusChanged);
            this._alphaController.forward();

            this._alpha = this._alphaController.drive(new IntTween(
                begin: 0, end: color.alpha));

            this.controller.addInkFeature(this);
        }

        readonly BoxShape _shape;

        readonly BorderRadius _borderRadius;

        readonly ShapeBorder _customBorder;

        readonly RectCallback _rectCallback;

        Animation<int> _alpha;
        AnimationController _alphaController;

        public bool active {
            get { return this._active; }
        }

        bool _active = true;

        public void activate() {
            this._active = true;
            this._alphaController.forward();
        }

        public void deactivate() {
            this._active = false;
            this._alphaController.reverse();
        }

        void _handleAlphaStatusChanged(AnimationStatus status) {
            if (status == AnimationStatus.dismissed && !this._active) {
                this.dispose();
            }
        }

        public override void dispose() {
            this._alphaController.dispose();
            base.dispose();
        }

        void _paintHighlight(Canvas canvas, Rect rect, Paint paint) {
            canvas.save();
            if (this._customBorder != null) {
                canvas.clipPath(this._customBorder.getOuterPath(rect));
            }

            switch (this._shape) {
                case BoxShape.circle: {
                    canvas.drawCircle(rect.center, Material.defaultSplashRadius, paint);
                    break;
                }
                case BoxShape.rectangle: {
                    if (this._borderRadius != BorderRadius.zero) {
                        RRect clipRRect = RRect.fromRectAndCorners(
                            rect,
                            topLeft: this._borderRadius.topLeft,
                            topRight: this._borderRadius.topRight,
                            bottomLeft: this._borderRadius.bottomLeft,
                            bottomRight: this._borderRadius.bottomRight);
                        canvas.drawRRect(clipRRect, paint);
                    }
                    else {
                        canvas.drawRect(rect, paint);
                    }

                    break;
                }
            }

            canvas.restore();
        }

        protected override void paintFeature(Canvas canvas, Matrix3 transform) {
            Paint paint = new Paint {color = this.color.withAlpha(this._alpha.value)};
            Offset originOffset = transform.getAsTranslation();
            Rect rect = this._rectCallback != null ? this._rectCallback() : Offset.zero & this.referenceBox.size;
            if (originOffset == null) {
                canvas.save();
                canvas.concat(transform);
                this._paintHighlight(canvas, rect, paint);
                canvas.restore();
            }
            else {
                this._paintHighlight(canvas, rect.shift(originOffset), paint);
            }
        }
    }
}