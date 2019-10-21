using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.cupertino {
    public class CupertinoRouteUtils {
        public const float _kBackGestureWidth = 20.0f;
        public const float _kMinFlingVelocity = 1.0f;

        public const int _kMaxDroppedSwipePageForwardAnimationTime = 800; // Milliseconds.

        public const int _kMaxPageBackAnimationTime = 300; // Milliseconds.

        public static readonly Color _kModalBarrierColor = new Color(0x6604040F);

        public static readonly TimeSpan _kModalPopupTransitionDuration = new TimeSpan(0, 0, 0, 0, 335);

        public static readonly Animatable<Offset> _kRightMiddleTween = new OffsetTween(
            begin: new Offset(1.0f, 0.0f),
            end: Offset.zero
        );

        public static readonly Animatable<Offset> _kMiddleLeftTween = new OffsetTween(
            begin: Offset.zero,
            end: new Offset(-1.0f / 3.0f, 0.0f)
        );

        public static readonly Animatable<Offset> _kBottomUpTween = new OffsetTween(
            begin: new Offset(0.0f, 1.0f),
            end: Offset.zero
        );

        public static readonly DecorationTween _kGradientShadowTween = new DecorationTween(
            begin: _CupertinoEdgeShadowDecoration.none,
            end: new _CupertinoEdgeShadowDecoration(
                edgeGradient: new LinearGradient(
                    begin: new Alignment(0.9f, 0.0f),
                    end: Alignment.centerRight,
                    colors: new List<Color> {
                        new Color(0x00000000),
                        new Color(0x04000000),
                        new Color(0x12000000),
                        new Color(0x38000000),
                    },
                    stops: new List<float> {0.0f, 0.3f, 0.6f, 1.0f}
                )
            )
        );


        public static IPromise<object> showCupertinoModalPopup(
            BuildContext context,
            WidgetBuilder builder
        ) {
            return Navigator.of(context, rootNavigator: true).push(
                new _CupertinoModalPopupRoute(
                    builder: builder,
                    barrierLabel: "Dismiss"
                )
            );
        }


        public static readonly Animatable<float> _dialogScaleTween = new FloatTween(begin: 1.3f, end: 1.0f)
            .chain(new CurveTween(curve: Curves.linearToEaseOut));

        public static Widget _buildCupertinoDialogTransitions(BuildContext context, Animation<float> animation,
            Animation<float>
                secondaryAnimation, Widget child) {
            CurvedAnimation fadeAnimation = new CurvedAnimation(
                parent: animation,
                curve: Curves.easeInOut
            );
            if (animation.status == AnimationStatus.reverse) {
                return new FadeTransition(
                    opacity: fadeAnimation,
                    child: child
                );
            }

            return new FadeTransition(
                opacity: fadeAnimation,
                child: new ScaleTransition(
                    child: child,
                    scale: animation.drive(_dialogScaleTween)
                )
            );
        }

        public static IPromise<object> showCupertinoDialog(
            BuildContext context,
            WidgetBuilder builder
        ) {
            D.assert(builder != null);
            return DialogUtils.showGeneralDialog(
                context: context,
                barrierDismissible: false,
                barrierColor: _kModalBarrierColor,
                transitionDuration: new TimeSpan(0, 0, 0, 0, 250),
                pageBuilder:
                (BuildContext _context, Animation<float> animation, Animation<float> secondaryAnimation) => {
                    return builder(_context);
                },
                transitionBuilder: _buildCupertinoDialogTransitions
            );
        }
    }

    class _CupertinoEdgeShadowDecoration : Decoration, IEquatable<_CupertinoEdgeShadowDecoration> {
        public _CupertinoEdgeShadowDecoration(
            LinearGradient edgeGradient = null
        ) {
            this.edgeGradient = edgeGradient;
        }

        public static _CupertinoEdgeShadowDecoration none =
            new _CupertinoEdgeShadowDecoration();

        public readonly LinearGradient edgeGradient;

        static _CupertinoEdgeShadowDecoration lerpCupertino(
            _CupertinoEdgeShadowDecoration a,
            _CupertinoEdgeShadowDecoration b,
            float t
        ) {
            if (a == null && b == null) {
                return null;
            }

            return new _CupertinoEdgeShadowDecoration(
                edgeGradient: LinearGradient.lerp(a?.edgeGradient, b?.edgeGradient, t)
            );
        }

        public override Decoration lerpFrom(Decoration a, float t) {
            if (!(a is _CupertinoEdgeShadowDecoration)) {
                return lerpCupertino(null, this, t);
            }

            return lerpCupertino((_CupertinoEdgeShadowDecoration) a, this, t);
        }

        public override Decoration lerpTo(Decoration b, float t) {
            if (!(b is _CupertinoEdgeShadowDecoration)) {
                return lerpCupertino(this, null, t);
            }

            return lerpCupertino(this, (_CupertinoEdgeShadowDecoration) b, t);
        }

        public override BoxPainter createBoxPainter(VoidCallback onChanged = null) {
            return new _CupertinoEdgeShadowPainter(this, onChanged);
        }

        public override int GetHashCode() {
            return this.edgeGradient.GetHashCode();
        }

        public bool Equals(_CupertinoEdgeShadowDecoration other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.edgeGradient, other.edgeGradient);
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

            return this.Equals((_CupertinoEdgeShadowDecoration) obj);
        }

        public static bool operator ==(_CupertinoEdgeShadowDecoration left, _CupertinoEdgeShadowDecoration right) {
            return Equals(left, right);
        }

        public static bool operator !=(_CupertinoEdgeShadowDecoration left, _CupertinoEdgeShadowDecoration right) {
            return !Equals(left, right);
        }

        public int hashCode {
            get { return this.edgeGradient.GetHashCode(); }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<LinearGradient>("edgeGradient", this.edgeGradient));
        }
    }

    class _CupertinoEdgeShadowPainter : BoxPainter {
        public _CupertinoEdgeShadowPainter(
            _CupertinoEdgeShadowDecoration decoration = null,
            VoidCallback onChange = null
        ) : base(onChange) {
            D.assert(decoration != null);
            this._decoration = decoration;
        }

        readonly _CupertinoEdgeShadowDecoration _decoration;

        public override void paint(Canvas canvas, Offset offset, ImageConfiguration configuration) {
            LinearGradient gradient = this._decoration.edgeGradient;
            if (gradient == null) {
                return;
            }

            float deltaX = -configuration.size.width;
            Rect rect = (offset & configuration.size).translate(deltaX, 0.0f);
            Paint paint = new Paint();
            paint.shader = gradient.createShader(rect);
            canvas.drawRect(rect, paint);
        }
    }

    public class CupertinoPageRoute : PageRoute {
        public CupertinoPageRoute(
            WidgetBuilder builder,
            RouteSettings settings = null,
            string title = "",
            bool maintainState = true,
            bool fullscreenDialog = false
        ) :
            base(settings: settings, fullscreenDialog: fullscreenDialog) {
            D.assert(builder != null);
            D.assert(this.opaque);
            this.builder = builder;
            this.title = title;
            this.maintainState = maintainState;
        }

        public readonly WidgetBuilder builder;
        public readonly string title;
        ValueNotifier<string> _previousTitle;

        public ValueListenable<string> previousTitle {
            get {
                D.assert(
                    this._previousTitle != null,
                    () => "Cannot read the previousTitle for a route that has not yet been installed"
                );
                return this._previousTitle;
            }
        }

        protected internal override void didChangePrevious(Route previousRoute) {
            string previousTitleString = previousRoute is CupertinoPageRoute
                ? ((CupertinoPageRoute) previousRoute).title
                : null;
            if (this._previousTitle == null) {
                this._previousTitle = new ValueNotifier<string>(previousTitleString);
            }

            else {
                this._previousTitle.value = previousTitleString;
            }

            base.didChangePrevious(previousRoute);
        }

        public override bool maintainState { get; }

        public override TimeSpan transitionDuration {
            get { return new TimeSpan(0, 0, 0, 0, 400); }
        }

        public override Color barrierColor {
            get { return null; }
        }


        public string barrierLabel {
            get { return null; }
        }


        public override bool canTransitionFrom(TransitionRoute previousRoute) {
            return previousRoute is CupertinoPageRoute;
        }


        public override bool canTransitionTo(TransitionRoute nextRoute) {
            return nextRoute is CupertinoPageRoute && !((CupertinoPageRoute) nextRoute).fullscreenDialog;
        }

        static bool isPopGestureInProgress(PageRoute route) {
            return route.navigator.userGestureInProgress;
        }


        public bool popGestureInProgress {
            get { return isPopGestureInProgress(this); }
        }

        public bool popGestureEnabled {
            get { return _isPopGestureEnabled(this); }
        }

        static bool _isPopGestureEnabled(PageRoute route) {
            if (route.isFirst) {
                return false;
            }

            if (route.willHandlePopInternally) {
                return false;
            }

            if (route.hasScopedWillPopCallback) {
                return false;
            }

            if (route.fullscreenDialog) {
                return false;
            }

            if (route.animation.status != AnimationStatus.completed) {
                return false;
            }

            if (route.secondaryAnimation.status != AnimationStatus.dismissed) {
                return false;
            }

            if (isPopGestureInProgress(route)) {
                return false;
            }

            return true;
        }


        public override Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation) {
            Widget result = this.builder(context);

            D.assert(() => {
                if (result == null) {
                    throw new UIWidgetsError(
                        $"The builder for route {this.settings.name} returned null.\nRoute builders must never return null.");
                }

                return true;
            });
            return result;
        }


        static _CupertinoBackGestureController _startPopGesture(PageRoute route) {
            D.assert(_isPopGestureEnabled(route));
            return new _CupertinoBackGestureController(
                navigator: route.navigator,
                controller: route.controller
            );
        }

        public static Widget buildPageTransitions(
            PageRoute route,
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation,
            Widget child
        ) {
            if (route.fullscreenDialog) {
                return new CupertinoFullscreenDialogTransition(
                    animation: animation,
                    child: child
                );
            }

            else {
                return new CupertinoPageTransition(
                    primaryRouteAnimation: animation,
                    secondaryRouteAnimation: secondaryAnimation,
                    linearTransition: isPopGestureInProgress(route),
                    child: new _CupertinoBackGestureDetector(
                        enabledCallback: () => _isPopGestureEnabled(route),
                        onStartPopGesture: () => _startPopGesture(route),
                        child: child
                    )
                );
            }
        }

        public override Widget buildTransitions(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation, Widget child) {
            return buildPageTransitions(this, context, animation, secondaryAnimation, child);
        }

        public new string debugLabel {
            get { return $"{base.debugLabel}(${this.settings.name})"; }
        }
    }

    class CupertinoPageTransition : StatelessWidget {
        public CupertinoPageTransition(
            Animation<float> primaryRouteAnimation,
            Animation<float> secondaryRouteAnimation,
            Widget child,
            bool linearTransition,
            Key key = null
        ) : base(key: key) {
            this._primaryPositionAnimation =
                (linearTransition
                    ? primaryRouteAnimation
                    : new CurvedAnimation(
                        parent: primaryRouteAnimation,
                        curve: Curves.linearToEaseOut,
                        reverseCurve: Curves.easeInToLinear
                    )
                ).drive(CupertinoRouteUtils._kRightMiddleTween);

            this._secondaryPositionAnimation =
                (linearTransition
                    ? secondaryRouteAnimation
                    : new CurvedAnimation(
                        parent: secondaryRouteAnimation,
                        curve: Curves.linearToEaseOut,
                        reverseCurve: Curves.easeInToLinear
                    )
                ).drive(CupertinoRouteUtils._kMiddleLeftTween);
            this._primaryShadowAnimation =
                (linearTransition
                    ? primaryRouteAnimation
                    : new CurvedAnimation(
                        parent: primaryRouteAnimation,
                        curve: Curves.linearToEaseOut
                    )
                ).drive(CupertinoRouteUtils._kGradientShadowTween);
            this.child = child;
        }

        public readonly Animation<Offset> _primaryPositionAnimation;

        public readonly Animation<Offset> _secondaryPositionAnimation;
        public readonly Animation<Decoration> _primaryShadowAnimation;

        public readonly Widget child;

        public override Widget build(BuildContext context) {
            TextDirection textDirection = Directionality.of(context);
            return new SlideTransition(
                position: this._secondaryPositionAnimation,
                textDirection: textDirection,
                transformHitTests: false,
                child: new SlideTransition(
                    position: this._primaryPositionAnimation,
                    textDirection: textDirection,
                    child: new DecoratedBoxTransition(
                        decoration: this._primaryShadowAnimation,
                        child: this.child
                    )
                )
            );
        }
    }

    class CupertinoFullscreenDialogTransition : StatelessWidget {
        public CupertinoFullscreenDialogTransition(
            Animation<float> animation,
            Widget child,
            Key key = null
        ) : base(key: key) {
            this._positionAnimation = new CurvedAnimation(
                parent: animation,
                curve: Curves.linearToEaseOut,
                reverseCurve: Curves.linearToEaseOut.flipped
            ).drive(CupertinoRouteUtils._kBottomUpTween);
            this.child = child;
        }

        readonly Animation<Offset> _positionAnimation;

        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new SlideTransition(
                position: this._positionAnimation,
                child: this.child
            );
        }
    }

    class _CupertinoBackGestureDetector : StatefulWidget {
        public _CupertinoBackGestureDetector(
            Widget child,
            ValueGetter<bool> enabledCallback,
            ValueGetter<_CupertinoBackGestureController> onStartPopGesture,
            Key key = null
        ) : base(key: key) {
            D.assert(enabledCallback != null);
            D.assert(onStartPopGesture != null);
            D.assert(child != null);
            this.child = child;
            this.enabledCallback = enabledCallback;
            this.onStartPopGesture = onStartPopGesture;
        }

        public readonly Widget child;

        public readonly ValueGetter<bool> enabledCallback;

        public readonly ValueGetter<_CupertinoBackGestureController> onStartPopGesture;

        public override State createState() {
            return new _CupertinoBackGestureDetectorState();
        }
    }

    class _CupertinoBackGestureDetectorState : State<_CupertinoBackGestureDetector> {
        _CupertinoBackGestureController _backGestureController;
        HorizontalDragGestureRecognizer _recognizer;


        public override void initState() {
            base.initState();
            this._recognizer = new HorizontalDragGestureRecognizer(debugOwner: this);
            this._recognizer.onStart = this._handleDragStart;
            this._recognizer.onUpdate = this._handleDragUpdate;
            this._recognizer.onEnd = this._handleDragEnd;
            this._recognizer.onCancel = this._handleDragCancel;
        }

        public override void dispose() {
            this._recognizer.dispose();
            base.dispose();
        }

        void _handleDragStart(DragStartDetails details) {
            D.assert(this.mounted);
            D.assert(this._backGestureController == null);
            this._backGestureController = this.widget.onStartPopGesture();
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            D.assert(this.mounted);
            D.assert(this._backGestureController != null);
            this._backGestureController.dragUpdate(
                this._convertToLogical(details.primaryDelta / this.context.size.width));
        }

        void _handleDragEnd(DragEndDetails details) {
            D.assert(this.mounted);
            D.assert(this._backGestureController != null);
            this._backGestureController.dragEnd(
                this._convertToLogical(details.velocity.pixelsPerSecond.dx / this.context.size.width) ?? 0);
            this._backGestureController = null;
        }

        void _handleDragCancel() {
            D.assert(this.mounted);
            this._backGestureController?.dragEnd(0.0f);
            this._backGestureController = null;
        }

        void _handlePointerDown(PointerDownEvent evt) {
            if (this.widget.enabledCallback()) {
                this._recognizer.addPointer(evt);
            }
        }

        float? _convertToLogical(float? value) {
            switch (Directionality.of(this.context)) {
                case TextDirection.rtl:
                    return -value;
                case TextDirection.ltr:
                    return value;
            }

            return value;
        }


        public override Widget build(BuildContext context) {
            float dragAreaWidth = Directionality.of(context) == TextDirection.ltr
                ? MediaQuery.of(context).padding.left
                : MediaQuery.of(context).padding.right;
            dragAreaWidth = Mathf.Max(dragAreaWidth, CupertinoRouteUtils._kBackGestureWidth);
            return new Stack(
                fit: StackFit.passthrough,
                children: new List<Widget> {
                    this.widget.child,
                    new Positioned(
                        left: 0.0f,
                        width: dragAreaWidth,
                        top: 0.0f,
                        bottom: 0.0f,
                        child: new Listener(
                            onPointerDown: this._handlePointerDown,
                            behavior: HitTestBehavior.translucent
                        )
                    )
                }
            );
        }
    }

    class _CupertinoBackGestureController {
        public _CupertinoBackGestureController(
            NavigatorState navigator,
            AnimationController controller
        ) {
            D.assert(navigator != null);
            D.assert(controller != null);

            this.navigator = navigator;
            this.controller = controller;
            this.navigator.didStartUserGesture();
        }

        public readonly AnimationController controller;
        public readonly NavigatorState navigator;

        public void dragUpdate(float? delta) {
            if (delta != null) {
                this.controller.setValue(this.controller.value - (float) delta);
            }
        }

        public void dragEnd(float velocity) {
            Curve animationCurve = Curves.fastLinearToSlowEaseIn;
            bool animateForward;

            if (velocity.abs() >= CupertinoRouteUtils._kMinFlingVelocity) {
                animateForward = velocity > 0 ? false : true;
            }
            else {
                animateForward = this.controller.value > 0.5 ? true : false;
            }

            if (animateForward) {
                int droppedPageForwardAnimationTime = Mathf.Min(
                    MathUtils.lerpFloat(CupertinoRouteUtils._kMaxDroppedSwipePageForwardAnimationTime, 0f,
                        this.controller.value).floor(),
                    CupertinoRouteUtils._kMaxPageBackAnimationTime
                );
                this.controller.animateTo(1.0f, duration: new TimeSpan(0, 0, 0, 0, droppedPageForwardAnimationTime),
                    curve: animationCurve);
            }
            else {
                this.navigator.pop();

                if (this.controller.isAnimating) {
                    int droppedPageBackAnimationTime =
                        MathUtils.lerpFloat(0f, CupertinoRouteUtils._kMaxDroppedSwipePageForwardAnimationTime,
                            this.controller.value).floor();
                    this.controller.animateBack(0.0f, duration: new TimeSpan(0, 0, 0, 0, droppedPageBackAnimationTime),
                        curve: animationCurve);
                }
            }

            if (this.controller.isAnimating) {
                AnimationStatusListener animationStatusCallback = null;
                animationStatusCallback = (AnimationStatus status) => {
                    this.navigator.didStopUserGesture();
                    this.controller.removeStatusListener(animationStatusCallback);
                };
                this.controller.addStatusListener(animationStatusCallback);
            }
            else {
                this.navigator.didStopUserGesture();
            }
        }
    }

    class _CupertinoModalPopupRoute : PopupRoute {
        public _CupertinoModalPopupRoute(
            WidgetBuilder builder = null,
            string barrierLabel = null,
            RouteSettings settings = null
        ) : base(settings: settings) {
            this.builder = builder;
            this.barrierLabel = barrierLabel;
        }

        public readonly WidgetBuilder builder;

        public readonly string barrierLabel;


        public override Color barrierColor {
            get { return CupertinoRouteUtils._kModalBarrierColor; }
        }

        public override bool barrierDismissible {
            get { return true; }
        }

        public bool semanticsDismissible {
            get { return false; }
        }

        public override TimeSpan transitionDuration {
            get { return CupertinoRouteUtils._kModalPopupTransitionDuration; }
        }

        new Animation<float> _animation;

        Tween<Offset> _offsetTween;

        public override Animation<float> createAnimation() {
            D.assert(this._animation == null);
            this._animation = new CurvedAnimation(
                parent: base.createAnimation(),
                curve: Curves.linearToEaseOut,
                reverseCurve: Curves.linearToEaseOut.flipped
            );
            this._offsetTween = new OffsetTween(
                begin: new Offset(0.0f, 1.0f),
                end: new Offset(0.0f, 0.0f)
            );
            return this._animation;
        }


        public override Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation) {
            return this.builder(context);
        }


        public override Widget buildTransitions(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation, Widget child) {
            return new Align(
                alignment: Alignment.bottomCenter,
                child: new FractionalTranslation(
                    translation: this._offsetTween.evaluate(this._animation),
                    child: child
                )
            );
        }
    }
}