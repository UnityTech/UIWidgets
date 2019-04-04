using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.widgets {
    public class GlowingOverscrollIndicator : StatefulWidget {
        public GlowingOverscrollIndicator(
            Key key = null,
            bool showLeading = true,
            bool showTrailing = true,
            AxisDirection axisDirection = AxisDirection.up,
            Color color = null,
            ScrollNotificationPredicate notificationPredicate = null,
            Widget child = null
        ) : base(key: key) {
            D.assert(color != null);
            this.showLeading = showLeading;
            this.showTrailing = showTrailing;
            this.axisDirection = axisDirection;
            this.child = child;
            this.color = color;
            this.notificationPredicate = notificationPredicate ?? ScrollNotification.defaultScrollNotificationPredicate;
        }

        public readonly bool showLeading;

        public readonly bool showTrailing;

        public readonly AxisDirection axisDirection;

        public Axis axis {
            get { return AxisUtils.axisDirectionToAxis(this.axisDirection); }
        }

        public readonly Color color;

        public readonly ScrollNotificationPredicate notificationPredicate;

        public readonly Widget child;

        public override State createState() {
            return new _GlowingOverscrollIndicatorState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<AxisDirection>("axisDirection", this.axisDirection));
            string showDescription;
            if (this.showLeading && this.showTrailing) {
                showDescription = "both sides";
            }
            else if (this.showLeading) {
                showDescription = "leading side only";
            }
            else if (this.showTrailing) {
                showDescription = "trailing side only";
            }
            else {
                showDescription = "neither side (!)";
            }

            properties.add(new MessageProperty("show", showDescription));
            properties.add(new DiagnosticsProperty<Color>("color", this.color, showName: false));
        }
    }

    class _GlowingOverscrollIndicatorState : TickerProviderStateMixin<GlowingOverscrollIndicator> {
        _GlowController _leadingController;
        _GlowController _trailingController;
        Listenable _leadingAndTrailingListener;

        public override void initState() {
            base.initState();
            this._leadingController =
                new _GlowController(vsync: this, color: this.widget.color, axis: this.widget.axis);
            this._trailingController =
                new _GlowController(vsync: this, color: this.widget.color, axis: this.widget.axis);
            this._leadingAndTrailingListener = ListenableUtils.merge(new List<Listenable>
                {this._leadingController, this._trailingController});
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            base.didUpdateWidget(_oldWidget);
            GlowingOverscrollIndicator oldWidget = _oldWidget as GlowingOverscrollIndicator;
            if (oldWidget.color != this.widget.color || oldWidget.axis != this.widget.axis) {
                this._leadingController.color = this.widget.color;
                this._leadingController.axis = this.widget.axis;
                this._trailingController.color = this.widget.color;
                this._trailingController.axis = this.widget.axis;
            }
        }

        Type _lastNotificationType;
        Dictionary<bool, bool> _accepted = new Dictionary<bool, bool> {{false, true}, {true, true}};

        bool _handleScrollNotification(ScrollNotification notification) {
            if (!this.widget.notificationPredicate(notification)) {
                return false;
            }

            if (notification is OverscrollNotification) {
                _GlowController controller;
                OverscrollNotification _notification = notification as OverscrollNotification;
                if (_notification.overscroll < 0.0f) {
                    controller = this._leadingController;
                }
                else if (_notification.overscroll > 0.0f) {
                    controller = this._trailingController;
                }
                else {
                    throw new Exception("overscroll is 0.0f!");
                }

                bool isLeading = controller == this._leadingController;
                if (this._lastNotificationType != typeof(OverscrollNotification)) {
                    OverscrollIndicatorNotification confirmationNotification =
                        new OverscrollIndicatorNotification(leading: isLeading);
                    confirmationNotification.dispatch(this.context);
                    this._accepted[isLeading] = confirmationNotification._accepted;
                }

                D.assert(controller != null);
                D.assert(_notification.metrics.axis() == this.widget.axis);
                if (this._accepted[isLeading]) {
                    if (_notification.velocity != 0.0f) {
                        D.assert(_notification.dragDetails == null);
                        controller.absorbImpact(_notification.velocity.abs());
                    }
                    else {
                        D.assert(_notification.overscroll != 0.0f);
                        if (_notification.dragDetails != null) {
                            D.assert(_notification.dragDetails.globalPosition != null);
                            RenderBox renderer = (RenderBox) _notification.context.findRenderObject();
                            D.assert(renderer != null);
                            D.assert(renderer.hasSize);
                            Size size = renderer.size;
                            Offset position = renderer.globalToLocal(_notification.dragDetails.globalPosition);
                            switch (_notification.metrics.axis()) {
                                case Axis.horizontal:
                                    controller.pull(_notification.overscroll.abs(), size.width,
                                        position.dy.clamp(0.0f, size.height), size.height);
                                    break;
                                case Axis.vertical:
                                    controller.pull(_notification.overscroll.abs(), size.height,
                                        position.dx.clamp(0.0f, size.width), size.width);
                                    break;
                            }
                        }
                    }
                }
            }
            else if (notification is ScrollEndNotification || notification is ScrollUpdateNotification) {
                if ((notification as ScrollEndNotification).dragDetails != null) {
                    this._leadingController.scrollEnd();
                    this._trailingController.scrollEnd();
                }
            }

            this._lastNotificationType = notification.GetType();
            return false;
        }

        public override void dispose() {
            this._leadingController.dispose();
            this._trailingController.dispose();
            base.dispose();
        }

        public override Widget build(BuildContext context) {
            return new NotificationListener<ScrollNotification>(
                onNotification: this._handleScrollNotification,
                child: new RepaintBoundary(
                    child: new CustomPaint(
                        foregroundPainter: new _GlowingOverscrollIndicatorPainter(
                            leadingController: this.widget.showLeading ? this._leadingController : null,
                            trailingController: this.widget.showTrailing ? this._trailingController : null,
                            axisDirection: this.widget.axisDirection,
                            repaint: this._leadingAndTrailingListener
                        ),
                        child: new RepaintBoundary(
                            child: this.widget.child
                        )
                    )
                )
            );
        }
    }


    enum _GlowState {
        idle,
        absorb,
        pull,
        recede
    }

    class _GlowController : ChangeNotifier {
        public _GlowController(
            TickerProvider vsync,
            Color color,
            Axis axis
        ) {
            D.assert(vsync != null);
            D.assert(color != null);
            this._color = color;
            this._axis = axis;
            this._glowController = new AnimationController(vsync: vsync);
            this._glowController.addStatusListener(this._changePhase);
            Animation<float> decelerator = new CurvedAnimation(
                parent: this._glowController,
                curve: Curves.decelerate
            );
            decelerator.addListener(this.notifyListeners);
            this._glowOpacity = decelerator.drive(this._glowOpacityTween);
            this._glowSize = decelerator.drive(this._glowSizeTween);
            this._displacementTicker = vsync.createTicker(this._tickDisplacement);
        }

        _GlowState _state = _GlowState.idle;
        AnimationController _glowController;
        Timer _pullRecedeTimer;

        FloatTween _glowOpacityTween = new FloatTween(begin: 0.0f, end: 0.0f);
        Animation<float> _glowOpacity;
        FloatTween _glowSizeTween = new FloatTween(begin: 0.0f, end: 0.0f);
        Animation<float> _glowSize;

        Ticker _displacementTicker;
        TimeSpan? _displacementTickerLastElapsed;
        float _displacementTarget = 0.5f;
        float _displacement = 0.5f;

        float _pullDistance = 0.0f;

        public Color color {
            get { return this._color; }
            set {
                D.assert(this.color != null);
                if (this.color == value) {
                    return;
                }

                this._color = value;
                this.notifyListeners();
            }
        }

        Color _color;

        public Axis axis {
            get { return this._axis; }
            set {
                if (this.axis == value) {
                    return;
                }

                this._axis = value;
                this.notifyListeners();
            }
        }

        Axis _axis;

        readonly TimeSpan _recedeTime = new TimeSpan(0, 0, 0, 0, 600);
        readonly TimeSpan _pullTime = new TimeSpan(0, 0, 0, 0, 167);
        readonly TimeSpan _pullHoldTime = new TimeSpan(0, 0, 0, 0, 167);
        readonly TimeSpan _pullDecayTime = new TimeSpan(0, 0, 0, 0, 2000);
        static readonly TimeSpan _crossAxisHalfTime = new TimeSpan(0, 0, 0, 0, (1000.0f / 60.0f).round());

        const float _maxOpacity = 0.5f;
        const float _pullOpacityGlowFactor = 0.8f;
        const float _velocityGlowFactor = 0.00006f;
        const float _sqrt3 = 1.73205080757f; // Mathf.Sqrt(3)
        const float _widthToHeightFactor = (3.0f / 4.0f) * (2.0f - _sqrt3);

        const float _minVelocity = 100.0f; // logical pixels per second
        const float _maxVelocity = 10000.0f; // logical pixels per second

        public override void dispose() {
            this._glowController.dispose();
            this._displacementTicker.dispose();
            this._pullRecedeTimer?.cancel();
            base.dispose();
        }

        public void absorbImpact(float velocity) {
            D.assert(velocity >= 0.0f);
            this._pullRecedeTimer?.cancel();
            this._pullRecedeTimer = null;
            velocity = velocity.clamp(_minVelocity, _maxVelocity);
            this._glowOpacityTween.begin = this._state == _GlowState.idle ? 0.3f : this._glowOpacity.value;
            this._glowOpacityTween.end =
                (velocity * _velocityGlowFactor).clamp(this._glowOpacityTween.begin, _maxOpacity);
            this._glowSizeTween.begin = this._glowSize.value;
            this._glowSizeTween.end = Mathf.Min(0.025f + 7.5e-7f * velocity * velocity, 1.0f);
            this._glowController.duration = new TimeSpan(0, 0, 0, 0, (0.15f + velocity * 0.02f).round());
            this._glowController.forward(from: 0.0f);
            this._displacement = 0.5f;
            this._state = _GlowState.absorb;
        }

        public void pull(float overscroll, float extent, float crossAxisOffset, float crossExtent) {
            this._pullRecedeTimer?.cancel();
            this._pullDistance +=
                overscroll / 200.0f; // This factor is magic. Not clear why we need it to match Android.
            this._glowOpacityTween.begin = this._glowOpacity.value;
            this._glowOpacityTween.end =
                Mathf.Min(this._glowOpacity.value + overscroll / extent * _pullOpacityGlowFactor, _maxOpacity);
            float height = Mathf.Min(extent, crossExtent * _widthToHeightFactor);
            this._glowSizeTween.begin = this._glowSize.value;
            this._glowSizeTween.end = Mathf.Max(1.0f - 1.0f / (0.7f * Mathf.Sqrt(this._pullDistance * height)),
                this._glowSize.value);
            this._displacementTarget = crossAxisOffset / crossExtent;
            if (this._displacementTarget != this._displacement) {
                if (!this._displacementTicker.isTicking) {
                    D.assert(this._displacementTickerLastElapsed == null);
                    this._displacementTicker.start();
                }
            }
            else {
                this._displacementTicker.stop();
                this._displacementTickerLastElapsed = null;
            }

            this._glowController.duration = this._pullTime;
            if (this._state != _GlowState.pull) {
                this._glowController.forward(from: 0.0f);
                this._state = _GlowState.pull;
            }
            else {
                if (!this._glowController.isAnimating) {
                    D.assert(this._glowController.value == 1.0f);
                    this.notifyListeners();
                }
            }

            this._pullRecedeTimer =
                Window.instance.run(this._pullHoldTime, () => this._recede(this._pullDecayTime));
        }

        public void scrollEnd() {
            if (this._state == _GlowState.pull) {
                this._recede(this._recedeTime);
            }
        }

        void _changePhase(AnimationStatus status) {
            if (status != AnimationStatus.completed) {
                return;
            }

            switch (this._state) {
                case _GlowState.absorb:
                    this._recede(this._recedeTime);
                    break;
                case _GlowState.recede:
                    this._state = _GlowState.idle;
                    this._pullDistance = 0.0f;
                    break;
                case _GlowState.pull:
                case _GlowState.idle:
                    break;
            }
        }

        void _recede(TimeSpan duration) {
            if (this._state == _GlowState.recede || this._state == _GlowState.idle) {
                return;
            }

            this._pullRecedeTimer?.cancel();
            this._pullRecedeTimer = null;
            this._glowOpacityTween.begin = this._glowOpacity.value;
            this._glowOpacityTween.end = 0.0f;
            this._glowSizeTween.begin = this._glowSize.value;
            this._glowSizeTween.end = 0.0f;
            this._glowController.duration = duration;
            this._glowController.forward(from: 0.0f);
            this._state = _GlowState.recede;
        }

        void _tickDisplacement(TimeSpan elapsed) {
            if (this._displacementTickerLastElapsed != null) {
                float? t = elapsed.Milliseconds - this._displacementTickerLastElapsed?.Milliseconds;
                this._displacement = this._displacementTarget - (this._displacementTarget - this._displacement) *
                                     Mathf.Pow(2.0f, (-t ?? 0.0f) / _crossAxisHalfTime.Milliseconds);
                this.notifyListeners();
            }

            if (PhysicsUtils.nearEqual(this._displacementTarget, this._displacement,
                Tolerance.defaultTolerance.distance)) {
                this._displacementTicker.stop();
                this._displacementTickerLastElapsed = null;
            }
            else {
                this._displacementTickerLastElapsed = elapsed;
            }
        }

        public void paint(Canvas canvas, Size size) {
            if (this._glowOpacity.value == 0.0f) {
                return;
            }

            float baseGlowScale = size.width > size.height ? size.height / size.width : 1.0f;
            float radius = size.width * 3.0f / 2.0f;
            float height = Mathf.Min(size.height, size.width * _widthToHeightFactor);
            float scaleY = this._glowSize.value * baseGlowScale;
            Rect rect = Rect.fromLTWH(0.0f, 0.0f, size.width, height);
            Offset center = new Offset((size.width / 2.0f) * (0.5f + this._displacement), height - radius);
            Paint paint = new Paint();
            paint.color = this.color.withOpacity(this._glowOpacity.value);
            canvas.save();
            canvas.scale(1.0f, scaleY);
            canvas.clipRect(rect);
            canvas.drawCircle(center, radius, paint);
            canvas.restore();
        }
    }

    class _GlowingOverscrollIndicatorPainter : AbstractCustomPainter {
        public _GlowingOverscrollIndicatorPainter(
            _GlowController leadingController,
            _GlowController trailingController,
            AxisDirection axisDirection,
            Listenable repaint
        ) : base(
            repaint: repaint
        ) {
            this.leadingController = leadingController;
            this.trailingController = trailingController;
            this.axisDirection = axisDirection;
        }

        public readonly _GlowController leadingController;

        public readonly _GlowController trailingController;

        public readonly AxisDirection axisDirection;

        const float piOver2 = Mathf.PI / 2.0f;

        void _paintSide(Canvas canvas, Size size, _GlowController controller, AxisDirection axisDirection,
            GrowthDirection growthDirection) {
            if (controller == null) {
                return;
            }

            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(axisDirection, growthDirection)) {
                case AxisDirection.up:
                    controller.paint(canvas, size);
                    break;
                case AxisDirection.down:
                    canvas.save();
                    canvas.translate(0.0f, size.height);
                    canvas.scale(1.0f, -1.0f);
                    controller.paint(canvas, size);
                    canvas.restore();
                    break;
                case AxisDirection.left:
                    canvas.save();
                    canvas.rotate(piOver2);
                    canvas.scale(1.0f, -1.0f);
                    controller.paint(canvas, new Size(size.height, size.width));
                    canvas.restore();
                    break;
                case AxisDirection.right:
                    canvas.save();
                    canvas.translate(size.width, 0.0f);
                    canvas.rotate(piOver2);
                    controller.paint(canvas, new Size(size.height, size.width));
                    canvas.restore();
                    break;
            }
        }

        public override void paint(Canvas canvas, Size size) {
            this._paintSide(canvas, size, this.leadingController, this.axisDirection, GrowthDirection.reverse);
            this._paintSide(canvas, size, this.trailingController, this.axisDirection, GrowthDirection.forward);
        }

        public override bool shouldRepaint(CustomPainter _oldDelegate) {
            _GlowingOverscrollIndicatorPainter oldDelegate = _oldDelegate as _GlowingOverscrollIndicatorPainter;
            return oldDelegate.leadingController != this.leadingController
                   || oldDelegate.trailingController != this.trailingController;
        }
    }

    public class OverscrollIndicatorNotification : ViewportNotificationMixinNotification {
        public OverscrollIndicatorNotification(
            bool leading
        ) {
            this.leading = leading;
        }

        public readonly bool leading;

        internal bool _accepted = true;

        public void disallowGlow() {
            this._accepted = false;
        }

        protected override void debugFillDescription(List<string> description) {
            base.debugFillDescription(description);
            description.Add($"side: {(this.leading ? "leading edge" : "trailing edge")}");
        }
    }
}