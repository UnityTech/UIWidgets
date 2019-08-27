using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public delegate void DismissDirectionCallback(DismissDirection? direction);

    public delegate Promise<bool> ConfirmDismissCallback(DismissDirection? direction);

    public enum DismissDirection {
        vertical,

        horizontal,

        endToStart,

        startToEnd,

        up,

        down
    }

    public class Dismissible : StatefulWidget {
        public Dismissible(
            Key key = null,
            Widget child = null,
            Widget background = null,
            Widget secondaryBackground = null,
            ConfirmDismissCallback confirmDismiss = null,
            VoidCallback onResize = null,
            DismissDirectionCallback onDismissed = null,
            DismissDirection direction = DismissDirection.horizontal,
            TimeSpan? resizeDuration = null,
            Dictionary<DismissDirection?, float?> dismissThresholds = null,
            TimeSpan? movementDuration = null,
            float crossAxisEndOffset = 0.0f,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            D.assert(key != null);
            D.assert(secondaryBackground != null ? background != null : true);
            this.resizeDuration = resizeDuration ?? new TimeSpan(0, 0, 0, 0, 300);
            this.dismissThresholds = dismissThresholds ?? new Dictionary<DismissDirection?, float?>();
            this.movementDuration = movementDuration ?? new TimeSpan(0, 0, 0, 0, 200);
            this.child = child;
            this.background = background;
            this.secondaryBackground = secondaryBackground;
            this.confirmDismiss = confirmDismiss;
            this.onResize = onResize;
            this.onDismissed = onDismissed;
            this.direction = direction;
            this.crossAxisEndOffset = crossAxisEndOffset;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly Widget child;

        public readonly Widget background;

        public readonly Widget secondaryBackground;

        public readonly VoidCallback onResize;

        public readonly DismissDirectionCallback onDismissed;

        public readonly ConfirmDismissCallback confirmDismiss;

        public readonly DismissDirection direction;

        public readonly TimeSpan? resizeDuration;

        public readonly Dictionary<DismissDirection?, float?> dismissThresholds;

        public readonly TimeSpan? movementDuration;

        public readonly float crossAxisEndOffset;

        public readonly DragStartBehavior dragStartBehavior;

        public override State createState() {
            return new _DismissibleState();
        }
    }

    class _AutomaticWidgetTicker<T> : Ticker where T : StatefulWidget {
        internal _AutomaticWidgetTicker(
            TickerCallback onTick,
            AutomaticKeepAliveClientWithTickerProviderStateMixin<T> creator,
            Func<string> debugLabel = null) :
            base(onTick: onTick, debugLabel: debugLabel) {
            this._creator = creator;
        }

        readonly AutomaticKeepAliveClientWithTickerProviderStateMixin<T> _creator;

        public override void dispose() {
            this._creator._removeTicker(this);
            base.dispose();
        }
    }

    class _DismissibleClipper : CustomClipper<Rect> {
        public _DismissibleClipper(
            Axis axis,
            Animation<Offset> moveAnimation
        ) : base(reclip: moveAnimation) {
            D.assert(moveAnimation != null);
            this.axis = axis;
            this.moveAnimation = moveAnimation;
        }

        public readonly Axis axis;
        public readonly Animation<Offset> moveAnimation;

        public override Rect getClip(Size size) {
            switch (this.axis) {
                case Axis.horizontal:
                    float offset1 = this.moveAnimation.value.dx * size.width;
                    if (offset1 < 0) {
                        return Rect.fromLTRB(size.width + offset1, 0.0f, size.width, size.height);
                    }

                    return Rect.fromLTRB(0.0f, 0.0f, offset1, size.height);
                case Axis.vertical:
                    float offset = this.moveAnimation.value.dy * size.height;
                    if (offset < 0) {
                        return Rect.fromLTRB(0.0f, size.height + offset, size.width, size.height);
                    }

                    return Rect.fromLTRB(0.0f, 0.0f, size.width, offset);
            }

            return null;
        }

        public override Rect getApproximateClipRect(Size size) {
            return this.getClip(size);
        }

        public override bool shouldReclip(CustomClipper<Rect> oldClipper) {
            D.assert(oldClipper is _DismissibleClipper);
            _DismissibleClipper clipper = oldClipper as _DismissibleClipper;
            return clipper.axis != this.axis
                   || clipper.moveAnimation.value != this.moveAnimation.value;
        }
    }

    enum _FlingGestureKind {
        none,
        forward,
        reverse
    }

    public class _DismissibleState : AutomaticKeepAliveClientWithTickerProviderStateMixin<Dismissible> {
        static readonly Curve _kResizeTimeCurve = new Interval(0.4f, 1.0f, curve: Curves.ease);
        const float _kMinFlingVelocity = 700.0f;
        const float _kMinFlingVelocityDelta = 400.0f;
        const float _kFlingVelocityScale = 1.0f / 300.0f;
        const float _kDismissThreshold = 0.4f;

        public override void initState() {
            base.initState();
            this._moveController = new AnimationController(duration: this.widget.movementDuration, vsync: this);
            this._moveController.addStatusListener(this._handleDismissStatusChanged);
            this._updateMoveAnimation();
        }

        AnimationController _moveController;
        Animation<Offset> _moveAnimation;

        AnimationController _resizeController;
        Animation<float> _resizeAnimation;

        float _dragExtent = 0.0f;
        bool _dragUnderway = false;
        Size _sizePriorToCollapse;

        protected override bool wantKeepAlive {
            get { return this._moveController?.isAnimating == true || this._resizeController?.isAnimating == true; }
        }

        public override void dispose() {
            this._moveController.dispose();
            this._resizeController?.dispose();
            base.dispose();
        }

        bool _directionIsXAxis {
            get {
                return this.widget.direction == DismissDirection.horizontal
                       || this.widget.direction == DismissDirection.endToStart
                       || this.widget.direction == DismissDirection.startToEnd;
            }
        }

        DismissDirection? _extentToDirection(float? extent) {
            if (extent == 0.0) {
                return null;
            }

            if (this._directionIsXAxis) {
                switch (Directionality.of(this.context)) {
                    case TextDirection.rtl:
                        return extent < 0 ? DismissDirection.startToEnd : DismissDirection.endToStart;
                    case TextDirection.ltr:
                        return extent > 0 ? DismissDirection.startToEnd : DismissDirection.endToStart;
                }

                D.assert(false);
                return null;
            }

            return extent > 0 ? DismissDirection.down : DismissDirection.up;
        }

        DismissDirection? _dismissDirection {
            get { return this._extentToDirection(this._dragExtent); }
        }

        bool _isActive {
            get { return this._dragUnderway || this._moveController.isAnimating; }
        }

        float _overallDragAxisExtent {
            get {
                Size size = this.context.size;
                return this._directionIsXAxis ? size.width : size.height;
            }
        }

        void _handleDragStart(DragStartDetails details) {
            this._dragUnderway = true;
            if (this._moveController.isAnimating) {
                this._dragExtent = this._moveController.value * this._overallDragAxisExtent * this._dragExtent.sign();
                this._moveController.stop();
            }
            else {
                this._dragExtent = 0.0f;
                this._moveController.setValue(0.0f);
            }

            this.setState(() => { this._updateMoveAnimation(); });
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            if (!this._isActive || this._moveController.isAnimating) {
                return;
            }

            float delta = details.primaryDelta ?? 0.0f;
            float oldDragExtent = this._dragExtent;
            switch (this.widget.direction) {
                case DismissDirection.horizontal:
                case DismissDirection.vertical:
                    this._dragExtent += delta;
                    break;

                case DismissDirection.up:
                    if (this._dragExtent + delta < 0) {
                        this._dragExtent += delta;
                    }

                    break;

                case DismissDirection.down:
                    if (this._dragExtent + delta > 0) {
                        this._dragExtent += delta;
                    }

                    break;

                case DismissDirection.endToStart:
                    switch (Directionality.of(this.context)) {
                        case TextDirection.rtl:
                            if (this._dragExtent + delta > 0) {
                                this._dragExtent += delta;
                            }

                            break;
                        case TextDirection.ltr:
                            if (this._dragExtent + delta < 0) {
                                this._dragExtent += delta;
                            }

                            break;
                    }

                    break;

                case DismissDirection.startToEnd:
                    switch (Directionality.of(this.context)) {
                        case TextDirection.rtl:
                            if (this._dragExtent + delta < 0) {
                                this._dragExtent += delta;
                            }

                            break;
                        case TextDirection.ltr:
                            if (this._dragExtent + delta > 0) {
                                this._dragExtent += delta;
                            }

                            break;
                    }

                    break;
            }

            if (oldDragExtent.sign() != this._dragExtent.sign()) {
                this.setState(() => { this._updateMoveAnimation(); });
            }

            if (!this._moveController.isAnimating) {
                this._moveController.setValue(this._dragExtent.abs() / this._overallDragAxisExtent);
            }
        }

        void _updateMoveAnimation() {
            float end = this._dragExtent.sign();
            this._moveAnimation = this._moveController.drive(
                new OffsetTween(
                    begin: Offset.zero,
                    end: this._directionIsXAxis
                        ? new Offset(end, this.widget.crossAxisEndOffset)
                        : new Offset(this.widget.crossAxisEndOffset, end)
                )
            );
        }

        _FlingGestureKind _describeFlingGesture(Velocity velocity) {
            if (this._dragExtent == 0.0f) {
                return _FlingGestureKind.none;
            }

            float vx = velocity.pixelsPerSecond.dx;
            float vy = velocity.pixelsPerSecond.dy;
            DismissDirection? flingDirection;
            if (this._directionIsXAxis) {
                if (vx.abs() - vy.abs() < _kMinFlingVelocityDelta || vx.abs() < _kMinFlingVelocity) {
                    return _FlingGestureKind.none;
                }

                D.assert(vx != 0.0f);
                flingDirection = this._extentToDirection(vx);
            }
            else {
                if (vy.abs() - vx.abs() < _kMinFlingVelocityDelta || vy.abs() < _kMinFlingVelocity) {
                    return _FlingGestureKind.none;
                }

                D.assert(vy != 0.0);
                flingDirection = this._extentToDirection(vy);
            }

            D.assert(this._dismissDirection != null);
            if (flingDirection == this._dismissDirection) {
                return _FlingGestureKind.forward;
            }

            return _FlingGestureKind.reverse;
        }

        void _handleDragEnd(DragEndDetails details) {
            if (!this._isActive || this._moveController.isAnimating) {
                return;
            }

            this._dragUnderway = false;
            this._confirmStartResizeAnimation().Then((value) => {
                if (this._moveController.isCompleted && value) {
                    this._startResizeAnimation();
                }
                else {
                    float flingVelocity = this._directionIsXAxis
                        ? details.velocity.pixelsPerSecond.dx
                        : details.velocity.pixelsPerSecond.dy;
                    switch (this._describeFlingGesture(details.velocity)) {
                        case _FlingGestureKind.forward:
                            D.assert(this._dragExtent != 0.0f);
                            D.assert(!this._moveController.isDismissed);
                            if ((this.widget.dismissThresholds.getOrDefault(this._dismissDirection) ??
                                 _kDismissThreshold) >= 1.0) {
                                this._moveController.reverse();
                                break;
                            }

                            this._dragExtent = flingVelocity.sign();
                            this._moveController.fling(velocity: flingVelocity.abs() * _kFlingVelocityScale);
                            break;
                        case _FlingGestureKind.reverse:
                            D.assert(this._dragExtent != 0.0f);
                            D.assert(!this._moveController.isDismissed);
                            this._dragExtent = flingVelocity.sign();
                            this._moveController.fling(velocity: -flingVelocity.abs() * _kFlingVelocityScale);
                            break;
                        case _FlingGestureKind.none:
                            if (!this._moveController.isDismissed) {
                                // we already know it's not completed, we check that above
                                if (this._moveController.value >
                                    (this.widget.dismissThresholds.getOrDefault(this._dismissDirection) ??
                                     _kDismissThreshold)) {
                                    this._moveController.forward();
                                }
                                else {
                                    this._moveController.reverse();
                                }
                            }

                            break;
                    }
                }
            });
        }

        void _handleDismissStatusChanged(AnimationStatus status) {
            if (status == AnimationStatus.completed && !this._dragUnderway) {
                this._confirmStartResizeAnimation().Then((value) => {
                    if (value) {
                        this._startResizeAnimation();
                    }
                    else {
                        this._moveController.reverse();
                    }

                    this.updateKeepAlive();
                });
            }
        }

        IPromise<bool> _confirmStartResizeAnimation() {
            if (this.widget.confirmDismiss != null) {
                DismissDirection? direction = this._dismissDirection;
                D.assert(direction != null);
                return this.widget.confirmDismiss(direction);
            }

            return Promise<bool>.Resolved(true);
        }

        void _startResizeAnimation() {
            D.assert(this._moveController != null);
            D.assert(this._moveController.isCompleted);
            D.assert(this._resizeController == null);
            D.assert(this._sizePriorToCollapse == null);
            if (this.widget.resizeDuration == null) {
                if (this.widget.onDismissed != null) {
                    DismissDirection? direction = this._dismissDirection;
                    D.assert(direction != null);
                    this.widget.onDismissed(direction);
                }
            }
            else {
                this._resizeController = new AnimationController(duration: this.widget.resizeDuration, vsync: this);
                this._resizeController.addListener(this._handleResizeProgressChanged);
                this._resizeController.addStatusListener((AnimationStatus status) => this.updateKeepAlive());
                this._resizeController.forward();
                this.setState(() => {
                    this._sizePriorToCollapse = this.context.size;
                    this._resizeAnimation = this._resizeController.drive(
                        new CurveTween(
                            curve: _kResizeTimeCurve
                        )
                    ).drive(
                        new FloatTween(
                            begin: 1.0f,
                            end: 0.0f
                        )
                    );
                });
            }
        }

        void _handleResizeProgressChanged() {
            if (this._resizeController.isCompleted) {
                if (this.widget.onDismissed != null) {
                    DismissDirection? direction = this._dismissDirection;
                    D.assert(direction != null);
                    this.widget.onDismissed(direction);
                }
            }
            else {
                if (this.widget.onResize != null) {
                    this.widget.onResize();
                }
            }
        }

        public override Widget build(BuildContext context) {
            base.build(context); // See AutomaticKeepAliveClientMixin.

            D.assert(!this._directionIsXAxis || WidgetsD.debugCheckHasDirectionality(context));

            Widget background = this.widget.background;
            if (this.widget.secondaryBackground != null) {
                DismissDirection? direction = this._dismissDirection;
                if (direction == DismissDirection.endToStart || direction == DismissDirection.up) {
                    background = this.widget.secondaryBackground;
                }
            }

            if (this._resizeAnimation != null) {
                // we've been dragged aside, and are now resizing.
                D.assert(() => {
                    if (this._resizeAnimation.status != AnimationStatus.forward) {
                        D.assert(this._resizeAnimation.status == AnimationStatus.completed);
                        throw new UIWidgetsError(
                            "A dismissed Dismissible widget is still part of the tree.\n" +
                            "Make sure to implement the onDismissed handler and to immediately remove the Dismissible\n" +
                            "widget from the application once that handler has fired."
                        );
                    }

                    return true;
                });

                return new SizeTransition(
                    sizeFactor: this._resizeAnimation,
                    axis: this._directionIsXAxis ? Axis.vertical : Axis.horizontal,
                    child: new SizedBox(
                        width: this._sizePriorToCollapse.width,
                        height: this._sizePriorToCollapse.height,
                        child: background
                    )
                );
            }

            Widget content = new SlideTransition(
                position: this._moveAnimation,
                child: this.widget.child
            );

            if (background != null) {
                List<Widget> children = new List<Widget> { };

                if (!this._moveAnimation.isDismissed) {
                    children.Add(Positioned.fill(
                        child: new ClipRect(
                            clipper: new _DismissibleClipper(
                                axis: this._directionIsXAxis ? Axis.horizontal : Axis.vertical,
                                moveAnimation: this._moveAnimation
                            ),
                            child: background
                        )
                    ));
                }

                children.Add(content);
                content = new Stack(children: children);
            }

            return new GestureDetector(
                onHorizontalDragStart: this._directionIsXAxis ? (GestureDragStartCallback) this._handleDragStart : null,
                onHorizontalDragUpdate: this._directionIsXAxis
                    ? (GestureDragUpdateCallback) this._handleDragUpdate
                    : null,
                onHorizontalDragEnd: this._directionIsXAxis ? (GestureDragEndCallback) this._handleDragEnd : null,
                onVerticalDragStart: this._directionIsXAxis ? null : (GestureDragStartCallback) this._handleDragStart,
                onVerticalDragUpdate: this._directionIsXAxis
                    ? null
                    : (GestureDragUpdateCallback) this._handleDragUpdate,
                onVerticalDragEnd: this._directionIsXAxis ? null : (GestureDragEndCallback) this._handleDragEnd,
                behavior: HitTestBehavior.opaque,
                child: content,
                dragStartBehavior: this.widget.dragStartBehavior
            );
        }
    }
}