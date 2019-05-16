using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    class RefreshIndicatorUtils {
        public const float _kDragContainerExtentPercentage = 0.25f;

        public const float _kDragSizeFactorLimit = 1.5f;

        public static readonly TimeSpan _kIndicatorSnapDuration = new TimeSpan(0, 0, 0, 0, 150);

        public static readonly TimeSpan _kIndicatorScaleDuration = new TimeSpan(0, 0, 0, 0, 200);
    }

    public delegate Promise RefreshCallback();

    enum _RefreshIndicatorMode {
        drag, // Pointer is down.
        armed, // Dragged far enough that an up event will run the onRefresh callback.
        snap, // Animating to the indicator"s final "displacement".
        refresh, // Running the refresh callback.
        done, // Animating the indicator"s fade-out after refreshing.
        canceled, // Animating the indicator"s fade-out after not arming.
    }

    public class RefreshIndicator : StatefulWidget {
        public RefreshIndicator(
            Key key = null,
            Widget child = null,
            float displacement = 40.0f,
            RefreshCallback onRefresh = null,
            Color color = null,
            Color backgroundColor = null,
            ScrollNotificationPredicate notificationPredicate = null
        ) : base(key: key) {
            D.assert(child != null);
            D.assert(onRefresh != null);
            this.child = child;
            this.displacement = displacement;
            this.onRefresh = onRefresh;
            this.color = color;
            this.backgroundColor = backgroundColor;
            this.notificationPredicate = notificationPredicate ?? ScrollNotification.defaultScrollNotificationPredicate;
        }

        public readonly Widget child;

        public readonly float displacement;

        public readonly RefreshCallback onRefresh;

        public readonly Color color;

        public readonly Color backgroundColor;

        public readonly ScrollNotificationPredicate notificationPredicate;

        public override State createState() {
            return new RefreshIndicatorState();
        }
    }

    public class RefreshIndicatorState : TickerProviderStateMixin<RefreshIndicator> {
        AnimationController _positionController;
        AnimationController _scaleController;
        Animation<float> _positionFactor;
        Animation<float> _scaleFactor;
        Animation<float> _value;
        Animation<Color> _valueColor;

        _RefreshIndicatorMode? _mode;
        Promise _pendingRefreshFuture;
        bool? _isIndicatorAtTop;
        float? _dragOffset;

        static readonly Animatable<float> _threeQuarterTween = new FloatTween(begin: 0.0f, end: 0.75f);

        static readonly Animatable<float> _kDragSizeFactorLimitTween =
            new FloatTween(begin: 0.0f, end: RefreshIndicatorUtils._kDragSizeFactorLimit);

        static readonly Animatable<float> _oneToZeroTween = new FloatTween(begin: 1.0f, end: 0.0f);

        public RefreshIndicatorState() {
        }

        public override void initState() {
            base.initState();
            this._positionController = new AnimationController(vsync: this);
            this._positionFactor = this._positionController.drive(_kDragSizeFactorLimitTween);
            this._value =
                this._positionController
                    .drive(_threeQuarterTween); // The "value" of the circular progress indicator during a drag.

            this._scaleController = new AnimationController(vsync: this);
            this._scaleFactor = this._scaleController.drive(_oneToZeroTween);
        }

        public override void didChangeDependencies() {
            ThemeData theme = Theme.of(this.context);
            this._valueColor = this._positionController.drive(
                new ColorTween(
                    begin: (this.widget.color ?? theme.accentColor).withOpacity(0.0f),
                    end: (this.widget.color ?? theme.accentColor).withOpacity(1.0f)
                ).chain(new CurveTween(
                    curve: new Interval(0.0f, 1.0f / RefreshIndicatorUtils._kDragSizeFactorLimit)
                ))
            );
            base.didChangeDependencies();
        }

        public override void dispose() {
            this._positionController.dispose();
            this._scaleController.dispose();
            base.dispose();
        }

        bool _handleScrollNotification(ScrollNotification notification) {
            if (!this.widget.notificationPredicate(notification)) {
                return false;
            }

            if (notification is ScrollStartNotification && notification.metrics.extentBefore() == 0.0f &&
                this._mode == null && this._start(notification.metrics.axisDirection)) {
                this.setState(() => { this._mode = _RefreshIndicatorMode.drag; });
                return false;
            }

            bool? indicatorAtTopNow = null;
            switch (notification.metrics.axisDirection) {
                case AxisDirection.down:
                    indicatorAtTopNow = true;
                    break;
                case AxisDirection.up:
                    indicatorAtTopNow = false;
                    break;
                case AxisDirection.left:
                case AxisDirection.right:
                    indicatorAtTopNow = null;
                    break;
            }

            if (indicatorAtTopNow != this._isIndicatorAtTop) {
                if (this._mode == _RefreshIndicatorMode.drag || this._mode == _RefreshIndicatorMode.armed) {
                    this._dismiss(_RefreshIndicatorMode.canceled);
                }
            }
            else if (notification is ScrollUpdateNotification) {
                if (this._mode == _RefreshIndicatorMode.drag || this._mode == _RefreshIndicatorMode.armed) {
                    if (notification.metrics.extentBefore() > 0.0f) {
                        this._dismiss(_RefreshIndicatorMode.canceled);
                    }
                    else {
                        this._dragOffset -= (notification as ScrollUpdateNotification).scrollDelta;
                        this._checkDragOffset(notification.metrics.viewportDimension);
                    }
                }

                if (this._mode == _RefreshIndicatorMode.armed &&
                    (notification as ScrollUpdateNotification).dragDetails == null) {
                    this._show();
                }
            }
            else if (notification is OverscrollNotification) {
                if (this._mode == _RefreshIndicatorMode.drag || this._mode == _RefreshIndicatorMode.armed) {
                    this._dragOffset -= (notification as OverscrollNotification).overscroll / 2.0f;
                    this._checkDragOffset(notification.metrics.viewportDimension);
                }
            }
            else if (notification is ScrollEndNotification) {
                switch (this._mode) {
                    case _RefreshIndicatorMode.armed:
                        this._show();
                        break;
                    case _RefreshIndicatorMode.drag:
                        this._dismiss(_RefreshIndicatorMode.canceled);
                        break;
                    default:
                        break;
                }
            }

            return false;
        }

        bool _handleGlowNotification(OverscrollIndicatorNotification notification) {
            if (notification.depth != 0 || !notification.leading) {
                return false;
            }

            if (this._mode == _RefreshIndicatorMode.drag) {
                notification.disallowGlow();
                return true;
            }

            return false;
        }

        bool _start(AxisDirection direction) {
            D.assert(this._mode == null);
            D.assert(this._isIndicatorAtTop == null);
            D.assert(this._dragOffset == null);
            switch (direction) {
                case AxisDirection.down:
                    this._isIndicatorAtTop = true;
                    break;
                case AxisDirection.up:
                    this._isIndicatorAtTop = false;
                    break;
                case AxisDirection.left:
                case AxisDirection.right:
                    this._isIndicatorAtTop = null;
                    return false;
            }

            this._dragOffset = 0.0f;
            this._scaleController.setValue(0.0f);
            this._positionController.setValue(0.0f);
            return true;
        }

        void _checkDragOffset(float containerExtent) {
            D.assert(this._mode == _RefreshIndicatorMode.drag || this._mode == _RefreshIndicatorMode.armed);
            float? newValue = this._dragOffset /
                              (containerExtent * RefreshIndicatorUtils._kDragContainerExtentPercentage);
            if (this._mode == _RefreshIndicatorMode.armed) {
                newValue = Mathf.Max(newValue ?? 0.0f, 1.0f / RefreshIndicatorUtils._kDragSizeFactorLimit);
            }

            this._positionController.setValue(newValue?.clamp(0.0f, 1.0f) ?? 0.0f); // this triggers various rebuilds
            if (this._mode == _RefreshIndicatorMode.drag && this._valueColor.value.alpha == 0xFF) {
                this._mode = _RefreshIndicatorMode.armed;
            }
        }

        IPromise _dismiss(_RefreshIndicatorMode newMode) {
            D.assert(newMode == _RefreshIndicatorMode.canceled || newMode == _RefreshIndicatorMode.done);
            this.setState(() => { this._mode = newMode; });
            switch (this._mode) {
                case _RefreshIndicatorMode.done:
                    return this._scaleController
                        .animateTo(1.0f, duration: RefreshIndicatorUtils._kIndicatorScaleDuration).Then(() => {
                            if (this.mounted && this._mode == newMode) {
                                this._dragOffset = null;
                                this._isIndicatorAtTop = null;
                                this.setState(() => { this._mode = null; });
                            }
                        });
                case _RefreshIndicatorMode.canceled:
                    return this._positionController
                        .animateTo(0.0f, duration: RefreshIndicatorUtils._kIndicatorScaleDuration).Then(() => {
                            if (this.mounted && this._mode == newMode) {
                                this._dragOffset = null;
                                this._isIndicatorAtTop = null;
                                this.setState(() => { this._mode = null; });
                            }
                        });
                default:
                    throw new Exception("Unknown refresh indicator mode: " + this._mode);
            }
        }

        void _show() {
            D.assert(this._mode != _RefreshIndicatorMode.refresh);
            D.assert(this._mode != _RefreshIndicatorMode.snap);
            Promise completer = new Promise();
            this._pendingRefreshFuture = completer;
            this._mode = _RefreshIndicatorMode.snap;
            this._positionController
                .animateTo(1.0f / RefreshIndicatorUtils._kDragSizeFactorLimit,
                    duration: RefreshIndicatorUtils._kIndicatorSnapDuration)
                .Then(() => {
                    if (this.mounted && this._mode == _RefreshIndicatorMode.snap) {
                        D.assert(this.widget.onRefresh != null);
                        this.setState(() => { this._mode = _RefreshIndicatorMode.refresh; });

                        Promise refreshResult = this.widget.onRefresh();
                        D.assert(() => {
                            if (refreshResult == null) {
                                UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                                    exception: new UIWidgetsError(
                                        "The onRefresh callback returned null.\n" +
                                        "The RefreshIndicator onRefresh callback must return a Promise."
                                    ),
                                    context: "when calling onRefresh",
                                    library: "material library"
                                ));
                            }

                            return true;
                        });
                        if (refreshResult == null) {
                            return;
                        }

                        refreshResult.Finally(() => {
                            if (this.mounted && this._mode == _RefreshIndicatorMode.refresh) {
                                completer.Resolve();
                                this._dismiss(_RefreshIndicatorMode.done);
                            }
                        });
                    }
                });
        }

        Promise show(bool atTop = true) {
            if (this._mode != _RefreshIndicatorMode.refresh && this._mode != _RefreshIndicatorMode.snap) {
                if (this._mode == null) {
                    this._start(atTop ? AxisDirection.down : AxisDirection.up);
                }

                this._show();
            }

            return this._pendingRefreshFuture;
        }

        GlobalKey _key = GlobalKey.key();

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterialLocalizations(context));
            Widget child = new NotificationListener<ScrollNotification>(
                key: this._key,
                onNotification: this._handleScrollNotification,
                child: new NotificationListener<OverscrollIndicatorNotification>(
                    onNotification: this._handleGlowNotification,
                    child: this.widget.child
                )
            );
            if (this._mode == null) {
                D.assert(this._dragOffset == null);
                D.assert(this._isIndicatorAtTop == null);
                return child;
            }

            D.assert(this._dragOffset != null);
            D.assert(this._isIndicatorAtTop != null);

            bool showIndeterminateIndicator =
                this._mode == _RefreshIndicatorMode.refresh || this._mode == _RefreshIndicatorMode.done;

            return new Stack(
                children: new List<Widget> {
                    child,
                    new Positioned(
                        top: this._isIndicatorAtTop == true ? 0.0f : (float?) null,
                        bottom: this._isIndicatorAtTop != true ? 0.0f : (float?) null,
                        left: 0.0f,
                        right: 0.0f,
                        child: new SizeTransition(
                            axisAlignment: this._isIndicatorAtTop == true ? 1.0f : -1.0f,
                            sizeFactor: this._positionFactor, // this is what brings it down
                            child: new Container(
                                padding: this._isIndicatorAtTop == true
                                    ? EdgeInsets.only(top: this.widget.displacement)
                                    : EdgeInsets.only(bottom: this.widget.displacement),
                                alignment: this._isIndicatorAtTop == true
                                    ? Alignment.topCenter
                                    : Alignment.bottomCenter,
                                child: new ScaleTransition(
                                    scale: this._scaleFactor,
                                    child: new AnimatedBuilder(
                                        animation: this._positionController,
                                        builder: (BuildContext _context, Widget _child) => {
                                            return new RefreshProgressIndicator(
                                                value: showIndeterminateIndicator ? (float?) null : this._value.value,
                                                valueColor: this._valueColor,
                                                backgroundColor: this.widget.backgroundColor
                                            );
                                        }
                                    )
                                )
                            )
                        )
                    )
                }
            );
        }
    }
}
