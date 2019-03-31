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
    static class InkSplashUtils {
        public static readonly TimeSpan _kUnconfirmedSplashDuration = new TimeSpan(0, 0, 0, 1, 0);

        public static readonly TimeSpan _kSplashFadeDuration = new TimeSpan(0, 0, 0, 0, 200);

        public const float _kSplashInitialSize = 0.0f;

        public const float _kSplashConfirmedVelocity = 1.0f;

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
            if (containedInkWell) {
                Size size = rectCallback != null ? rectCallback().size : referenceBox.size;
                return _getSplashRadiusForPositionInSize(size, position);
            }

            return Material.defaultSplashRadius;
        }

        static float _getSplashRadiusForPositionInSize(Size bounds, Offset position) {
            float d1 = (position - bounds.topLeft(Offset.zero)).distance;
            float d2 = (position - bounds.topRight(Offset.zero)).distance;
            float d3 = (position - bounds.bottomLeft(Offset.zero)).distance;
            float d4 = (position - bounds.bottomRight(Offset.zero)).distance;
            return Mathf.Max(Mathf.Max(d1, d2), Mathf.Max(d3, d4)).ceil();
        }
    }

    public class _InkSplashFactory : InteractiveInkFeatureFactory {
        public _InkSplashFactory() {
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
            return new InkSplash(
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

    public class InkSplash : InteractiveInkFeature {
        public InkSplash(
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
            this._position = position;
            this._borderRadius = borderRadius ?? BorderRadius.zero;
            this._customBorder = customBorder;
            this._targetRadius =
                radius ?? InkSplashUtils._getTargetRadius(referenceBox, containedInkWell, rectCallback, position);
            this._clipCallback = InkSplashUtils._getClipCallback(referenceBox, containedInkWell, rectCallback);
            this._repositionToReferenceBox = !containedInkWell;

            D.assert(this._borderRadius != null);
            this._radiusController = new AnimationController(
                duration: InkSplashUtils._kUnconfirmedSplashDuration,
                vsync: controller.vsync);
            this._radiusController.addListener(controller.markNeedsPaint);
            this._radiusController.forward();
            this._radius = this._radiusController.drive(new FloatTween(
                begin: InkSplashUtils._kSplashInitialSize,
                end: this._targetRadius));

            this._alphaController = new AnimationController(
                duration: InkSplashUtils._kSplashFadeDuration,
                vsync: controller.vsync);
            this._alphaController.addListener(controller.markNeedsPaint);
            this._alphaController.addStatusListener(this._handleAlphaStatusChanged);
            this._alpha = this._alphaController.drive(new IntTween(
                begin: color.alpha,
                end: 0));

            controller.addInkFeature(this);
        }

        readonly Offset _position;

        readonly BorderRadius _borderRadius;

        readonly ShapeBorder _customBorder;

        readonly float _targetRadius;

        readonly RectCallback _clipCallback;

        readonly bool _repositionToReferenceBox;

        Animation<float> _radius;
        AnimationController _radiusController;

        Animation<int> _alpha;
        AnimationController _alphaController;

        public static InteractiveInkFeatureFactory splashFactory = new _InkSplashFactory();

        public override void confirm() {
            int duration = (this._targetRadius / InkSplashUtils._kSplashConfirmedVelocity).floor();
            this._radiusController.duration = new TimeSpan(0, 0, 0, 0, duration);
            this._radiusController.forward();
            this._alphaController.forward();
        }

        public override void cancel() {
            this._alphaController?.forward();
        }

        void _handleAlphaStatusChanged(AnimationStatus status) {
            if (status == AnimationStatus.completed) {
                this.dispose();
            }
        }

        public override void dispose() {
            this._radiusController.dispose();
            this._alphaController.dispose();
            this._alphaController = null;
            base.dispose();
        }

        protected override void paintFeature(Canvas canvas, Matrix3 transform) {
            Paint paint = new Paint {color = this.color.withAlpha(this._alpha.value)};
            Offset center = this._position;
            if (this._repositionToReferenceBox) {
                center = Offset.lerp(center, this.referenceBox.size.center(Offset.zero), this._radiusController.value);
            }

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