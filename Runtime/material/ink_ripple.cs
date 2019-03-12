using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.material {
    static class InkRippleUtils {
        public static readonly TimeSpan _kUnconfirmedRippleDuration = new TimeSpan(0, 0, 1);
        public static readonly TimeSpan _kFadeInDuration = new TimeSpan(0, 0, 0, 0, 75);
        public static readonly TimeSpan _kRadiusDuration = new TimeSpan(0, 0, 0, 0, 225);
        public static readonly TimeSpan _kFadeOutDuration = new TimeSpan(0, 0, 0, 0, 375);
        public static readonly TimeSpan _kCancelDuration = new TimeSpan(0, 0, 0, 0, 75);

        public const float _kFadeOutIntervalStart = 225.0f / 375.0f;

        public static RectCallback _getClipCallback(RenderBox referenceBox, bool containedInkWell,
            RectCallback rectCallback) {
            if (rectCallback != null) {
                D.assert(containedInkWell);
                return rectCallback;
            }

            if (containedInkWell) {
                return () => Offset.zero & referenceBox.size;
            }

            return null;
        }

        public static float _getTargetRadius(RenderBox referenceBox, bool containedInkWell, RectCallback rectCallback,
            Offset position) {
            Size size = rectCallback != null ? rectCallback().size : referenceBox.size;
            float d1 = size.bottomRight(Offset.zero).distance;
            float d2 = (size.topRight(Offset.zero) - size.bottomLeft(Offset.zero)).distance;
            return (Mathf.Max(d1, d2) / 2.0f);
        }
    }

    public class _InkRippleFactory : InteractiveInkFeatureFactory {
        public _InkRippleFactory() {
        }

        public override InteractiveInkFeature create(
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            Offset position = null,
            Color color = null,
            bool containedInkWell = false,
            RectCallback rectCallback = null,
            BorderRadius borderRadius = null,
            ShapeBorder customBorder = null,
            float? radius = null,
            VoidCallback onRemoved = null
        ) {
            D.assert(controller != null);
            D.assert(referenceBox != null);
            D.assert(position != null);
            D.assert(color != null);
            return new InkRipple(
                controller: controller,
                referenceBox: referenceBox,
                position: position,
                color: color,
                containedInkWell: containedInkWell,
                rectCallback: rectCallback,
                borderRadius: borderRadius,
                customBorder: customBorder,
                radius: radius,
                onRemoved: onRemoved);
        }
    }

    public class InkRipple : InteractiveInkFeature {
        public InkRipple(
            MaterialInkController controller = null,
            RenderBox referenceBox = null,
            Offset position = null,
            Color color = null,
            bool containedInkWell = false,
            RectCallback rectCallback = null,
            BorderRadius borderRadius = null,
            ShapeBorder customBorder = null,
            float? radius = null,
            VoidCallback onRemoved = null
        ) : base(
            controller: controller,
            referenceBox: referenceBox,
            color: color,
            onRemoved: onRemoved) {
            D.assert(controller != null);
            D.assert(referenceBox != null);
            D.assert(color != null);
            D.assert(position != null);

            this._position = position;
            this._borderRadius = borderRadius ?? BorderRadius.zero;
            this._customBorder = customBorder;
            this._targetRadius =
                radius ?? InkRippleUtils._getTargetRadius(referenceBox, containedInkWell, rectCallback, position);
            this._clipCallback = InkRippleUtils._getClipCallback(referenceBox, containedInkWell, rectCallback);

            D.assert(this._borderRadius != null);

            this._fadeInController =
                new AnimationController(duration: InkRippleUtils._kFadeInDuration, vsync: controller.vsync);
            this._fadeInController.addListener(controller.markNeedsPaint);
            this._fadeInController.forward();
            this._fadeIn = this._fadeInController.drive(new IntTween(
                begin: 0,
                end: color.alpha
            ));

            this._radiusController = new AnimationController(
                duration: InkRippleUtils._kUnconfirmedRippleDuration,
                vsync: controller.vsync);
            this._radiusController.addListener(controller.markNeedsPaint);
            this._radiusController.forward();
            this._radius = this._radiusController.drive(new FloatTween(
                    begin: this._targetRadius * 0.30f,
                    end: this._targetRadius + 5.0f
                ).chain(_easeCurveTween)
            );

            this._fadeOutController = new AnimationController(
                duration: InkRippleUtils._kFadeOutDuration,
                vsync: controller.vsync);
            this._fadeOutController.addListener(controller.markNeedsPaint);
            this._fadeOutController.addStatusListener(this._handleAlphaStatusChanged);
            this._fadeOut = this._fadeOutController.drive(new IntTween(
                    begin: color.alpha,
                    end: 0
                ).chain(_fadeOutIntervalTween)
            );

            controller.addInkFeature(this);
        }

        readonly Offset _position;

        readonly BorderRadius _borderRadius;

        readonly ShapeBorder _customBorder;

        readonly float _targetRadius;

        readonly RectCallback _clipCallback;

        Animation<float> _radius;
        AnimationController _radiusController;

        Animation<int> _fadeIn;
        AnimationController _fadeInController;

        Animation<int> _fadeOut;
        AnimationController _fadeOutController;

        public static InteractiveInkFeatureFactory splashFactory = new _InkRippleFactory();

        static readonly Animatable<float> _easeCurveTween = new CurveTween(curve: Curves.ease);

        static readonly Animatable<float> _fadeOutIntervalTween =
            new CurveTween(curve: new Interval(InkRippleUtils._kFadeOutIntervalStart, 1.0f));

        public override void confirm() {
            this._radiusController.duration = InkRippleUtils._kRadiusDuration;
            this._radiusController.forward();
            this._fadeInController.forward();
            this._fadeOutController.animateTo(1.0f, duration: InkRippleUtils._kFadeOutDuration);
        }

        public override void cancel() {
            this._fadeInController.stop();
            float fadeOutValue = 1.0f - this._fadeInController.value;
            this._fadeOutController.setValue(fadeOutValue);
            if (fadeOutValue < 1.0) {
                this._fadeOutController.animateTo(1.0f, duration: InkRippleUtils._kCancelDuration);
            }
        }

        void _handleAlphaStatusChanged(AnimationStatus status) {
            if (status == AnimationStatus.completed) {
                this.dispose();
            }
        }

        public override void dispose() {
            this._radiusController.dispose();
            this._fadeInController.dispose();
            this._fadeOutController.dispose();
            base.dispose();
        }

        protected override void paintFeature(Canvas canvas, Matrix3 transform) {
            int alpha = this._fadeInController.isAnimating ? this._fadeIn.value : this._fadeOut.value;
            Paint paint = new Paint {color = this.color.withAlpha(alpha)};
            Offset center = Offset.lerp(
                this._position,
                this.referenceBox.size.center(Offset.zero),
                Curves.ease.transform(this._radiusController.value)
            );
            Offset originOffset = transform.getAsTranslation();
            canvas.save();
            if (originOffset == null) {
                canvas.concat(transform);
            }
            else {
                canvas.translate(originOffset.dx, originOffset.dy);
            }

            if (this._clipCallback != null) {
                Rect rect = this._clipCallback();
                if (this._customBorder != null) {
                    canvas.clipPath(this._customBorder.getOuterPath(rect));
                }
                else if (this._borderRadius != BorderRadius.zero) {
                    canvas.clipRRect(RRect.fromRectAndCorners(
                        rect,
                        topLeft: this._borderRadius.topLeft,
                        topRight: this._borderRadius.topRight,
                        bottomLeft: this._borderRadius.bottomLeft,
                        bottomRight: this._borderRadius.bottomRight));
                }
                else {
                    canvas.clipRect(rect);
                }
            }

            canvas.drawCircle(center, this._radius.value, paint);
            canvas.restore();
        }
    }
}