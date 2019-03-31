using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.material {
    public static class FloatingActionButtonLocationUtils {
        public const float kFloatingActionButtonMargin = 16.0f;

        public static readonly TimeSpan kFloatingActionButtonSegue = new TimeSpan(0, 0, 0, 0, 200);

        public const float kFloatingActionButtonTurnInterval = 0.125f;
    }


    public abstract class FloatingActionButtonLocation {
        protected FloatingActionButtonLocation() {
        }

        public static readonly FloatingActionButtonLocation endFloat = new _EndFloatFabLocation();

        public static readonly FloatingActionButtonLocation centerFloat = new _CenterFloatFabLocation();

        public static readonly FloatingActionButtonLocation endDocked = new _EndDockedFloatingActionButtonLocation();

        public static readonly FloatingActionButtonLocation centerDocked =
            new _CenterDockedFloatingActionButtonLocation();

        public abstract Offset getOffset(ScaffoldPrelayoutGeometry scaffoldGeometry);

        public override string ToString() {
            return this.GetType().ToString();
        }
    }

    class _CenterFloatFabLocation : FloatingActionButtonLocation {
        public _CenterFloatFabLocation() {
        }

        public override Offset getOffset(ScaffoldPrelayoutGeometry scaffoldGeometry) {
            float fabX = (scaffoldGeometry.scaffoldSize.width - scaffoldGeometry.floatingActionButtonSize.width) / 2.0f;

            float contentBottom = scaffoldGeometry.contentBottom;
            float bottomSheetHeight = scaffoldGeometry.bottomSheetSize.height;
            float fabHeight = scaffoldGeometry.floatingActionButtonSize.height;
            float snackBarHeight = scaffoldGeometry.snackBarSize.height;
            float fabY = contentBottom - fabHeight - FloatingActionButtonLocationUtils.kFloatingActionButtonMargin;
            if (snackBarHeight > 0.0f) {
                fabY = Mathf.Min(fabY,
                    contentBottom - snackBarHeight - fabHeight -
                    FloatingActionButtonLocationUtils.kFloatingActionButtonMargin);
            }

            if (bottomSheetHeight > 0.0f) {
                fabY = Mathf.Min(fabY, contentBottom - bottomSheetHeight - fabHeight / 2.0f);
            }

            return new Offset(fabX, fabY);
        }
    }

    class _EndFloatFabLocation : FloatingActionButtonLocation {
        public _EndFloatFabLocation() {
        }

        public override Offset getOffset(ScaffoldPrelayoutGeometry scaffoldGeometry) {
            float endPadding = scaffoldGeometry.minInsets.right;
            float fabX = scaffoldGeometry.scaffoldSize.width - scaffoldGeometry.floatingActionButtonSize.width -
                         FloatingActionButtonLocationUtils.kFloatingActionButtonMargin - endPadding;

            float contentBottom = scaffoldGeometry.contentBottom;
            float bottomSheetHeight = scaffoldGeometry.bottomSheetSize.height;
            float fabHeight = scaffoldGeometry.floatingActionButtonSize.height;
            float snackBarHeight = scaffoldGeometry.snackBarSize.height;

            float fabY = contentBottom - fabHeight - FloatingActionButtonLocationUtils.kFloatingActionButtonMargin;
            if (snackBarHeight > 0.0f) {
                fabY = Mathf.Min(fabY,
                    contentBottom - snackBarHeight - fabHeight -
                    FloatingActionButtonLocationUtils.kFloatingActionButtonMargin);
            }

            if (bottomSheetHeight > 0.0f) {
                fabY = Mathf.Min(fabY, contentBottom - bottomSheetHeight - fabHeight / 2.0f);
            }

            return new Offset(fabX, fabY);
        }
    }

    abstract class _DockedFloatingActionButtonLocation : FloatingActionButtonLocation {
        protected _DockedFloatingActionButtonLocation() {
        }

        protected float getDockedY(ScaffoldPrelayoutGeometry scaffoldGeometry) {
            float contentBottom = scaffoldGeometry.contentBottom;
            float bottomSheetHeight = scaffoldGeometry.bottomSheetSize.height;
            float fabHeight = scaffoldGeometry.floatingActionButtonSize.height;
            float snackBarHeight = scaffoldGeometry.snackBarSize.height;

            float fabY = contentBottom - fabHeight / 2.0f;
            if (snackBarHeight > 0.0f) {
                fabY = Mathf.Min(fabY,
                    contentBottom - snackBarHeight - fabHeight -
                    FloatingActionButtonLocationUtils.kFloatingActionButtonMargin);
            }

            if (bottomSheetHeight > 0.0f) {
                fabY = Mathf.Min(fabY, contentBottom - bottomSheetHeight - fabHeight / 2.0f);
            }

            float maxFabY = scaffoldGeometry.scaffoldSize.height - fabHeight;
            return Mathf.Min(maxFabY, fabY);
        }
    }

    class _EndDockedFloatingActionButtonLocation : _DockedFloatingActionButtonLocation {
        public _EndDockedFloatingActionButtonLocation() {
        }

        public override Offset getOffset(ScaffoldPrelayoutGeometry scaffoldGeometry) {
            float endPadding = scaffoldGeometry.minInsets.right;
            float fabX = scaffoldGeometry.scaffoldSize.width - scaffoldGeometry.floatingActionButtonSize.width -
                         FloatingActionButtonLocationUtils.kFloatingActionButtonMargin - endPadding;
            return new Offset(fabX, this.getDockedY(scaffoldGeometry));
        }
    }

    class _CenterDockedFloatingActionButtonLocation : _DockedFloatingActionButtonLocation {
        public _CenterDockedFloatingActionButtonLocation() {
        }

        public override Offset getOffset(ScaffoldPrelayoutGeometry scaffoldGeometry) {
            float fabX = (scaffoldGeometry.scaffoldSize.width - scaffoldGeometry.floatingActionButtonSize.width) / 2.0f;
            return new Offset(fabX, this.getDockedY(scaffoldGeometry));
        }
    }

    public abstract class FloatingActionButtonAnimator {
        protected FloatingActionButtonAnimator() {
        }

        public static readonly FloatingActionButtonAnimator scaling = new _ScalingFabMotionAnimator();

        public abstract Offset getOffset(Offset begin, Offset end, float progress);

        public abstract Animation<float> getScaleAnimation(Animation<float> parent);

        public abstract Animation<float> getRotationAnimation(Animation<float> parent);

        public virtual float getAnimationRestart(float previousValue) {
            return 0.0f;
        }

        public override string ToString() {
            return this.GetType().ToString();
        }
    }

    class _ScalingFabMotionAnimator : FloatingActionButtonAnimator {
        public _ScalingFabMotionAnimator() {
        }

        public override Offset getOffset(Offset begin, Offset end, float progress) {
            if (progress < 0.5f) {
                return begin;
            }
            else {
                return end;
            }
        }

        public override Animation<float> getScaleAnimation(Animation<float> parent) {
            Curve curve = new Interval(0.5f, 1.0f, curve: Curves.ease);
            return new _AnimationSwap<float>(
                new ReverseAnimation(parent.drive(new CurveTween(curve: curve.flipped))),
                parent.drive(new CurveTween(curve: curve)),
                parent,
                0.5f
            );
        }

        static readonly Animatable<float> _rotationTween = new FloatTween(
            begin: 1.0f - FloatingActionButtonLocationUtils.kFloatingActionButtonTurnInterval * 2.0f,
            end: 1.0f
        );

        static readonly Animatable<float> _thresholdCenterTween = new CurveTween(curve: new Threshold(0.5f));

        public override Animation<float> getRotationAnimation(Animation<float> parent) {
            return new _AnimationSwap<float>(
                parent.drive(_rotationTween),
                new ReverseAnimation(parent.drive(_thresholdCenterTween)),
                parent,
                0.5f
            );
        }

        public override float getAnimationRestart(float previousValue) {
            return Mathf.Min(1.0f - previousValue, previousValue);
        }
    }


    class _AnimationSwap<T> : CompoundAnimation<T> {
        public _AnimationSwap(
            Animation<T> first,
            Animation<T> next,
            Animation<float> parent,
            float swapThreshold
        ) : base(first: first, next: next) {
            this.parent = parent;
            this.swapThreshold = swapThreshold;
        }

        public readonly Animation<float> parent;
        public readonly float swapThreshold;

        public override T value {
            get { return this.parent.value < this.swapThreshold ? this.first.value : this.next.value; }
        }
    }
}