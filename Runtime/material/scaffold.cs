using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.material {
    static class ScaffoldUtils {
        public static readonly FloatingActionButtonLocation _kDefaultFloatingActionButtonLocation =
            FloatingActionButtonLocation.endFloat;

        public static readonly FloatingActionButtonAnimator _kDefaultFloatingActionButtonAnimator =
            FloatingActionButtonAnimator.scaling;
    }

    enum _ScaffoldSlot {
        body,
        appBar,
        bottomSheet,
        snackBar,
        persistentFooter,
        bottomNavigationBar,
        floatingActionButton,
        drawer,
        endDrawer,
        statusBar
    }

    public class ScaffoldPrelayoutGeometry {
        public ScaffoldPrelayoutGeometry(
            Size bottomSheetSize = null,
            float? contentBottom = null,
            float? contentTop = null,
            Size floatingActionButtonSize = null,
            EdgeInsets minInsets = null,
            Size scaffoldSize = null,
            Size snackBarSize = null
        ) {
            D.assert(bottomSheetSize != null);
            D.assert(contentBottom != null);
            D.assert(contentTop != null);
            D.assert(floatingActionButtonSize != null);
            D.assert(minInsets != null);
            D.assert(scaffoldSize != null);
            D.assert(snackBarSize != null);

            this.bottomSheetSize = bottomSheetSize;
            this.contentBottom = contentBottom.Value;
            this.contentTop = contentTop.Value;
            this.floatingActionButtonSize = floatingActionButtonSize;
            this.minInsets = minInsets;
            this.scaffoldSize = scaffoldSize;
            this.snackBarSize = snackBarSize;
        }

        public readonly Size floatingActionButtonSize;

        public readonly Size bottomSheetSize;

        public readonly float contentBottom;

        public readonly float contentTop;

        public readonly EdgeInsets minInsets;

        public readonly Size scaffoldSize;

        public readonly Size snackBarSize;
    }

    class _TransitionSnapshotFabLocation : FloatingActionButtonLocation {
        public _TransitionSnapshotFabLocation(
            FloatingActionButtonLocation begin,
            FloatingActionButtonLocation end,
            FloatingActionButtonAnimator animator,
            float progress) {
            this.begin = begin;
            this.end = end;
            this.animator = animator;
            this.progress = progress;
        }

        public readonly FloatingActionButtonLocation begin;

        public readonly FloatingActionButtonLocation end;

        public readonly FloatingActionButtonAnimator animator;

        public readonly float progress;

        public override Offset getOffset(ScaffoldPrelayoutGeometry scaffoldGeometry) {
            return this.animator.getOffset(
                begin: this.begin.getOffset(scaffoldGeometry),
                end: this.end.getOffset(scaffoldGeometry),
                progress: this.progress
            );
        }

        public override string ToString() {
            return this.GetType() + "(begin: " + this.begin + ", end: " + this.end + ", progress: " + this.progress;
        }
    }

    public class ScaffoldGeometry {
        public ScaffoldGeometry(
            float? bottomNavigationBarTop = null,
            Rect floatingActionButtonArea = null) {
            this.bottomNavigationBarTop = bottomNavigationBarTop;
            this.floatingActionButtonArea = floatingActionButtonArea;
        }

        public readonly float? bottomNavigationBarTop;

        public readonly Rect floatingActionButtonArea;

        public ScaffoldGeometry _scaleFloatingActionButton(float scaleFactor) {
            if (scaleFactor == 1.0f) {
                return this;
            }

            if (scaleFactor == 0.0f) {
                return new ScaffoldGeometry(
                    bottomNavigationBarTop: this.bottomNavigationBarTop);
            }

            Rect scaledButton = Rect.lerp(
                this.floatingActionButtonArea.center & Size.zero,
                this.floatingActionButtonArea,
                scaleFactor);

            return this.copyWith(floatingActionButtonArea: scaledButton);
        }

        public ScaffoldGeometry copyWith(
            float? bottomNavigationBarTop = null,
            Rect floatingActionButtonArea = null
        ) {
            return new ScaffoldGeometry(
                bottomNavigationBarTop: bottomNavigationBarTop ?? this.bottomNavigationBarTop,
                floatingActionButtonArea: floatingActionButtonArea ?? this.floatingActionButtonArea);
        }
    }


    class _ScaffoldGeometryNotifier : ValueNotifier<ScaffoldGeometry> {
        public _ScaffoldGeometryNotifier(
            ScaffoldGeometry geometry, BuildContext context) : base(geometry) {
            D.assert(context != null);
            this.context = context;
            this.geometry = geometry;
        }

        public readonly BuildContext context;

        float floatingActionButtonScale;

        ScaffoldGeometry geometry;

        public override ScaffoldGeometry value {
            get {
                D.assert(() => {
                    RenderObject renderObject = this.context.findRenderObject();

                    if (renderObject == null || !renderObject.owner.debugDoingPaint) {
                        throw new UIWidgetsError(
                            "Scaffold.geometryOf() must only be accessed during the paint phase.\n" +
                            "The ScaffoldGeometry is only available during the paint phase, because\n" +
                            "its value is computed during the animation and layout phases prior to painting."
                        );
                    }

                    return true;
                });
                return this.geometry._scaleFloatingActionButton(this.floatingActionButtonScale);
            }
        }

        public void _updateWith(
            float? bottomNavigationBarTop = null,
            Rect floatingActionButtonArea = null,
            float? floatingActionButtonScale = null
        ) {
            this.floatingActionButtonScale = floatingActionButtonScale ?? this.floatingActionButtonScale;
            this.geometry = this.geometry.copyWith(
                bottomNavigationBarTop: bottomNavigationBarTop,
                floatingActionButtonArea: floatingActionButtonArea);
            this.notifyListeners();
        }
    }

    class _BodyBoxConstraints : BoxConstraints {
        public _BodyBoxConstraints(
            float minWidth = 0.0f,
            float maxWidth = float.PositiveInfinity,
            float minHeight = 0.0f,
            float maxHeight = float.PositiveInfinity,
            float? bottomWidgetsHeight = null
        ) : base(minWidth: minWidth, maxWidth: maxWidth, minHeight: minHeight, maxHeight: maxHeight) {
            D.assert(bottomWidgetsHeight != null);
            D.assert(bottomWidgetsHeight >= 0);
            this.bottomWidgetsHeight = bottomWidgetsHeight.Value;
        }

        public readonly float bottomWidgetsHeight;
        
        public bool Equals(_BodyBoxConstraints other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.bottomWidgetsHeight.Equals(other.bottomWidgetsHeight)
                   && base.Equals(other);
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

            return this.Equals((_BodyBoxConstraints) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ this.bottomWidgetsHeight.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(_BodyBoxConstraints left, _BodyBoxConstraints right) {
            return Equals(left, right);
        }

        public static bool operator !=(_BodyBoxConstraints left, _BodyBoxConstraints right) {
            return !Equals(left, right);
        }
    }

    class _BodyBuilder : StatelessWidget {
        public _BodyBuilder(Key key = null, Widget body = null) : base(key: key) {
            this.body = body;
        }

        public readonly Widget body;

        public override Widget build(BuildContext context) {
            return new LayoutBuilder(
                builder: (ctx, constraints) => {
                    _BodyBoxConstraints bodyConstraints = (_BodyBoxConstraints) constraints;
                    MediaQueryData metrics = MediaQuery.of(context);
                    return new MediaQuery(
                        data: metrics.copyWith(
                            padding: metrics.padding.copyWith(
                                bottom: Mathf.Max(metrics.padding.bottom, bodyConstraints.bottomWidgetsHeight)
                            )
                        ),
                        child: this.body
                    );
                }
            );
        }
    }

    class _ScaffoldLayout : MultiChildLayoutDelegate {
        public _ScaffoldLayout(
            EdgeInsets minInsets,
            _ScaffoldGeometryNotifier geometryNotifier,
            FloatingActionButtonLocation previousFloatingActionButtonLocation,
            FloatingActionButtonLocation currentFloatingActionButtonLocation,
            float floatingActionButtonMoveAnimationProgress,
            FloatingActionButtonAnimator floatingActionButtonMotionAnimator,
            bool extendBody
        ) {
            D.assert(minInsets != null);
            D.assert(geometryNotifier != null);
            D.assert(previousFloatingActionButtonLocation != null);
            D.assert(currentFloatingActionButtonLocation != null);

            this.minInsets = minInsets;
            this.geometryNotifier = geometryNotifier;
            this.previousFloatingActionButtonLocation = previousFloatingActionButtonLocation;
            this.currentFloatingActionButtonLocation = currentFloatingActionButtonLocation;
            this.floatingActionButtonMoveAnimationProgress = floatingActionButtonMoveAnimationProgress;
            this.floatingActionButtonMotionAnimator = floatingActionButtonMotionAnimator;
            this.extendBody = extendBody;
        }

        public readonly bool extendBody;
        
        public readonly EdgeInsets minInsets;

        public readonly _ScaffoldGeometryNotifier geometryNotifier;

        public readonly FloatingActionButtonLocation previousFloatingActionButtonLocation;

        public readonly FloatingActionButtonLocation currentFloatingActionButtonLocation;

        public readonly float floatingActionButtonMoveAnimationProgress;

        public readonly FloatingActionButtonAnimator floatingActionButtonMotionAnimator;


        public override void performLayout(Size size) {
            BoxConstraints looseConstraints = BoxConstraints.loose(size);

            BoxConstraints fullWidthConstraints = looseConstraints.tighten(width: size.width);
            float bottom = size.height;
            float contentTop = 0.0f;
            float bottomWidgetsHeight = 0.0f;

            if (this.hasChild(_ScaffoldSlot.appBar)) {
                contentTop = this.layoutChild(_ScaffoldSlot.appBar, fullWidthConstraints).height;
                this.positionChild(_ScaffoldSlot.appBar, Offset.zero);
            }

            float bottomNavigationBarTop = 0.0f;
            if (this.hasChild(_ScaffoldSlot.bottomNavigationBar)) {
                float bottomNavigationBarHeight =
                    this.layoutChild(_ScaffoldSlot.bottomNavigationBar, fullWidthConstraints).height;
                bottomWidgetsHeight += bottomNavigationBarHeight;
                bottomNavigationBarTop = Mathf.Max(0.0f, bottom - bottomWidgetsHeight);
                this.positionChild(_ScaffoldSlot.bottomNavigationBar, new Offset(0.0f, bottomNavigationBarTop));
            }

            if (this.hasChild(_ScaffoldSlot.persistentFooter)) {
                BoxConstraints footerConstraints = new BoxConstraints(
                    maxWidth: fullWidthConstraints.maxWidth,
                    maxHeight: Mathf.Max(0.0f, bottom - bottomWidgetsHeight - contentTop)
                );
                float persistentFooterHeight =
                    this.layoutChild(_ScaffoldSlot.persistentFooter, footerConstraints).height;
                bottomWidgetsHeight += persistentFooterHeight;
                this.positionChild(_ScaffoldSlot.persistentFooter,
                    new Offset(0.0f, Mathf.Max(0.0f, bottom - bottomWidgetsHeight)));
            }

            float contentBottom = Mathf.Max(0.0f, bottom - Mathf.Max(this.minInsets.bottom, bottomWidgetsHeight));

            if (this.hasChild(_ScaffoldSlot.body)) {
                float bodyMaxHeight = Mathf.Max(0.0f, contentBottom - contentTop);
                if (this.extendBody) {
                    bodyMaxHeight += bottomWidgetsHeight;
                    D.assert(bodyMaxHeight <= Mathf.Max(0.0f, looseConstraints.maxHeight - contentTop));
                }
                BoxConstraints bodyConstraints = new _BodyBoxConstraints(
                    maxWidth: fullWidthConstraints.maxWidth,
                    maxHeight: bodyMaxHeight,
                    bottomWidgetsHeight: this.extendBody ? bottomWidgetsHeight : 0.0f
                );
                this.layoutChild(_ScaffoldSlot.body, bodyConstraints);
                this.positionChild(_ScaffoldSlot.body, new Offset(0.0f, contentTop));
            }

            Size bottomSheetSize = Size.zero;
            Size snackBarSize = Size.zero;

            if (this.hasChild(_ScaffoldSlot.bottomSheet)) {
                BoxConstraints bottomSheetConstraints = new BoxConstraints(
                    maxWidth: fullWidthConstraints.maxWidth,
                    maxHeight: Mathf.Max(0.0f, contentBottom - contentTop)
                );
                bottomSheetSize = this.layoutChild(_ScaffoldSlot.bottomSheet, bottomSheetConstraints);
                this.positionChild(_ScaffoldSlot.bottomSheet,
                    new Offset((size.width - bottomSheetSize.width) / 2.0f, contentBottom - bottomSheetSize.height));
            }

            if (this.hasChild(_ScaffoldSlot.snackBar)) {
                snackBarSize = this.layoutChild(_ScaffoldSlot.snackBar, fullWidthConstraints);
                this.positionChild(_ScaffoldSlot.snackBar, new Offset(0.0f, contentBottom - snackBarSize.height));
            }

            Rect floatingActionButtonRect = null;
            if (this.hasChild(_ScaffoldSlot.floatingActionButton)) {
                Size fabSize = this.layoutChild(_ScaffoldSlot.floatingActionButton, looseConstraints);
                ScaffoldPrelayoutGeometry currentGeometry = new ScaffoldPrelayoutGeometry(
                    bottomSheetSize: bottomSheetSize,
                    contentBottom: contentBottom,
                    contentTop: contentTop,
                    floatingActionButtonSize: fabSize,
                    minInsets: this.minInsets,
                    scaffoldSize: size,
                    snackBarSize: snackBarSize
                );
                Offset currentFabOffset = this.currentFloatingActionButtonLocation.getOffset(currentGeometry);
                Offset previousFabOffset = this.previousFloatingActionButtonLocation.getOffset(currentGeometry);
                Offset fabOffset = this.floatingActionButtonMotionAnimator.getOffset(
                    begin: previousFabOffset,
                    end: currentFabOffset,
                    progress: this.floatingActionButtonMoveAnimationProgress
                );
                this.positionChild(_ScaffoldSlot.floatingActionButton, fabOffset);
                floatingActionButtonRect = fabOffset & fabSize;
            }

            if (this.hasChild(_ScaffoldSlot.statusBar)) {
                this.layoutChild(_ScaffoldSlot.statusBar, fullWidthConstraints.tighten(height: this.minInsets.top));
                this.positionChild(_ScaffoldSlot.statusBar, Offset.zero);
            }

            if (this.hasChild(_ScaffoldSlot.drawer)) {
                this.layoutChild(_ScaffoldSlot.drawer, BoxConstraints.tight(size));
                this.positionChild(_ScaffoldSlot.drawer, Offset.zero);
            }

            if (this.hasChild(_ScaffoldSlot.endDrawer)) {
                this.layoutChild(_ScaffoldSlot.endDrawer, BoxConstraints.tight(size));
                this.positionChild(_ScaffoldSlot.endDrawer, Offset.zero);
            }

            this.geometryNotifier._updateWith(
                bottomNavigationBarTop: bottomNavigationBarTop,
                floatingActionButtonArea: floatingActionButtonRect
            );
        }

        public override bool shouldRelayout(MultiChildLayoutDelegate oldDelegate) {
            _ScaffoldLayout _oldDelegate = (_ScaffoldLayout) oldDelegate;
            return _oldDelegate.minInsets != this.minInsets
                   || _oldDelegate.floatingActionButtonMoveAnimationProgress !=
                   this.floatingActionButtonMoveAnimationProgress
                   || _oldDelegate.previousFloatingActionButtonLocation != this.previousFloatingActionButtonLocation
                   || _oldDelegate.currentFloatingActionButtonLocation != this.currentFloatingActionButtonLocation;
        }
    }


    class _FloatingActionButtonTransition : StatefulWidget {
        public _FloatingActionButtonTransition(
            Key key = null,
            Widget child = null,
            Animation<float> fabMoveAnimation = null,
            FloatingActionButtonAnimator fabMotionAnimator = null,
            _ScaffoldGeometryNotifier geometryNotifier = null
        ) : base(key: key) {
            D.assert(fabMoveAnimation != null);
            D.assert(fabMotionAnimator != null);
            this.child = child;
            this.fabMoveAnimation = fabMoveAnimation;
            this.fabMotionAnimator = fabMotionAnimator;
            this.geometryNotifier = geometryNotifier;
        }

        public readonly Widget child;

        public readonly Animation<float> fabMoveAnimation;

        public readonly FloatingActionButtonAnimator fabMotionAnimator;

        public readonly _ScaffoldGeometryNotifier geometryNotifier;

        public override State createState() {
            return new _FloatingActionButtonTransitionState();
        }
    }

    class _FloatingActionButtonTransitionState : TickerProviderStateMixin<_FloatingActionButtonTransition> {
        AnimationController _previousController;

        Animation<float> _previousScaleAnimation;

        Animation<float> _previousRotationAnimation;

        AnimationController _currentController;

        Animation<float> _currentScaleAnimation;

        Animation<float> _extendedCurrentScaleAnimation;

        Animation<float> _currentRotationAnimation;

        Widget _previousChild;

        public override void initState() {
            base.initState();

            this._previousController = new AnimationController(
                duration: FloatingActionButtonLocationUtils.kFloatingActionButtonSegue,
                vsync: this);
            this._previousController.addStatusListener(this._handlePreviousAnimationStatusChanged);

            this._currentController = new AnimationController(
                duration: FloatingActionButtonLocationUtils.kFloatingActionButtonSegue,
                vsync: this);

            this._updateAnimations();

            if (this.widget.child != null) {
                this._currentController.setValue(1.0f);
            }
            else {
                this._updateGeometryScale(0.0f);
            }
        }

        public override void dispose() {
            this._previousController.dispose();
            this._currentController.dispose();
            base.dispose();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);

            _FloatingActionButtonTransition _oldWidget = (_FloatingActionButtonTransition) oldWidget;
            bool oldChildIsNull = _oldWidget.child == null;
            bool newChildIsNull = this.widget.child == null;

            if (oldChildIsNull == newChildIsNull && _oldWidget.child?.key == this.widget.child?.key) {
                return;
            }

            if (_oldWidget.fabMotionAnimator != this.widget.fabMotionAnimator ||
                _oldWidget.fabMoveAnimation != this.widget.fabMoveAnimation) {
                this._updateAnimations();
            }

            if (this._previousController.status == AnimationStatus.dismissed) {
                float currentValue = this._currentController.value;
                if (currentValue == 0.0f || _oldWidget.child == null) {
                    this._previousChild = null;
                    if (this.widget.child != null) {
                        this._currentController.forward();
                    }
                }
                else {
                    this._previousChild = _oldWidget.child;
                    this._previousController.setValue(currentValue);
                    this._previousController.reverse();
                    this._currentController.setValue(0.0f);
                }
            }
        }

        static readonly Animatable<float> _entranceTurnTween = new FloatTween(
            begin: 1.0f - FloatingActionButtonLocationUtils.kFloatingActionButtonTurnInterval,
            end: 1.0f
        ).chain(new CurveTween(curve: Curves.easeIn));

        void _updateAnimations() {
            CurvedAnimation previousExitScaleAnimation = new CurvedAnimation(
                parent: this._previousController,
                curve: Curves.easeIn
            );
            Animation<float> previousExitRotationAnimation = new FloatTween(begin: 1.0f, end: 1.0f).animate(
                new CurvedAnimation(
                    parent: this._previousController,
                    curve: Curves.easeIn
                )
            );

            CurvedAnimation currentEntranceScaleAnimation = new CurvedAnimation(
                parent: this._currentController,
                curve: Curves.easeIn
            );
            Animation<float> currentEntranceRotationAnimation = this._currentController.drive(_entranceTurnTween);
            Animation<float> moveScaleAnimation =
                this.widget.fabMotionAnimator.getScaleAnimation(parent: this.widget.fabMoveAnimation);
            Animation<float> moveRotationAnimation =
                this.widget.fabMotionAnimator.getRotationAnimation(parent: this.widget.fabMoveAnimation);

            this._previousScaleAnimation = new AnimationMin(moveScaleAnimation, previousExitScaleAnimation);
            this._currentScaleAnimation = new AnimationMin(moveScaleAnimation, currentEntranceScaleAnimation);
            this._extendedCurrentScaleAnimation =
                this._currentScaleAnimation.drive(new CurveTween(curve: new Interval(0.0f, 0.1f)));

            this._previousRotationAnimation =
                new TrainHoppingAnimation(previousExitRotationAnimation, moveRotationAnimation);
            this._currentRotationAnimation =
                new TrainHoppingAnimation(currentEntranceRotationAnimation, moveRotationAnimation);

            this._currentScaleAnimation.addListener(this._onProgressChanged);
            this._previousScaleAnimation.addListener(this._onProgressChanged);
        }

        void _handlePreviousAnimationStatusChanged(AnimationStatus status) {
            this.setState(() => {
                if (status == AnimationStatus.dismissed) {
                    D.assert(this._currentController.status == AnimationStatus.dismissed);
                    if (this.widget.child != null) {
                        this._currentController.forward();
                    }
                }
            });
        }

        bool _isExtendedFloatingActionButton(Widget widget) {
            if (!(widget is FloatingActionButton)) {
                return false;
            }

            FloatingActionButton fab = (FloatingActionButton) widget;
            return fab.isExtended;
        }

        public override Widget build(BuildContext context) {
            List<Widget> children = new List<Widget>();

            if (this._previousController.status != AnimationStatus.dismissed) {
                if (this._isExtendedFloatingActionButton(this._previousChild)) {
                    children.Add(new FadeTransition(
                        opacity: this._previousScaleAnimation,
                        child: this._previousChild));
                }
                else {
                    children.Add(new ScaleTransition(
                        scale: this._previousScaleAnimation,
                        child: new RotationTransition(
                            turns: this._previousRotationAnimation,
                            child: this._previousChild)));
                }
            }

            if (this._isExtendedFloatingActionButton(this.widget.child)) {
                children.Add(new ScaleTransition(
                    scale: this._extendedCurrentScaleAnimation,
                    child: new FadeTransition(
                        opacity: this._currentScaleAnimation,
                        child: this.widget.child
                    )
                ));
            }
            else {
                children.Add(new ScaleTransition(
                    scale: this._currentScaleAnimation,
                    child: new RotationTransition(
                        turns: this._currentRotationAnimation,
                        child: this.widget.child
                    )
                ));
            }

            return new Stack(
                alignment: Alignment.centerRight,
                children: children
            );
        }


        void _onProgressChanged() {
            this._updateGeometryScale(Mathf.Max(this._previousScaleAnimation.value, this._currentScaleAnimation.value));
        }

        void _updateGeometryScale(float scale) {
            this.widget.geometryNotifier._updateWith(
                floatingActionButtonScale: scale
            );
        }
    }

    public class Scaffold : StatefulWidget {
        public Scaffold(
            Key key = null,
            PreferredSizeWidget appBar = null,
            Widget body = null,
            Widget floatingActionButton = null,
            FloatingActionButtonLocation floatingActionButtonLocation = null,
            FloatingActionButtonAnimator floatingActionButtonAnimator = null,
            List<Widget> persistentFooterButtons = null,
            Widget drawer = null,
            Widget endDrawer = null,
            Widget bottomNavigationBar = null,
            Widget bottomSheet = null,
            Color backgroundColor = null,
            bool? resizeToAvoidBottomPadding = null,
            bool? resizeToAvoidBottomInset = null,
            bool primary = true,
            DragStartBehavior drawerDragStartBehavior = DragStartBehavior.start,
            bool extendBody = false
        ) : base(key: key) {
            this.appBar = appBar;
            this.body = body;
            this.floatingActionButton = floatingActionButton;
            this.floatingActionButtonLocation = floatingActionButtonLocation;
            this.floatingActionButtonAnimator = floatingActionButtonAnimator;
            this.persistentFooterButtons = persistentFooterButtons;
            this.drawer = drawer;
            this.endDrawer = endDrawer;
            this.bottomNavigationBar = bottomNavigationBar;
            this.bottomSheet = bottomSheet;
            this.backgroundColor = backgroundColor;
            this.resizeToAvoidBottomPadding = resizeToAvoidBottomPadding;
            this.resizeToAvoidBottomInset = resizeToAvoidBottomInset;
            this.primary = primary;
            this.drawerDragStartBehavior = drawerDragStartBehavior;
            this.extendBody = extendBody;
        }

        public readonly bool extendBody;

        public readonly PreferredSizeWidget appBar;

        public readonly Widget body;

        public readonly Widget floatingActionButton;

        public readonly FloatingActionButtonLocation floatingActionButtonLocation;

        public readonly FloatingActionButtonAnimator floatingActionButtonAnimator;

        public readonly List<Widget> persistentFooterButtons;

        public readonly Widget drawer;

        public readonly Widget endDrawer;

        public readonly Color backgroundColor;

        public readonly Widget bottomNavigationBar;

        public readonly Widget bottomSheet;

        public readonly bool? resizeToAvoidBottomPadding;

        public readonly bool? resizeToAvoidBottomInset;

        public readonly bool primary;

        public readonly DragStartBehavior drawerDragStartBehavior;

        public static ScaffoldState of(BuildContext context, bool nullOk = false) {
            D.assert(context != null);
            ScaffoldState result = (ScaffoldState) context.ancestorStateOfType(new TypeMatcher<ScaffoldState>());
            if (nullOk || result != null) {
                return result;
            }

            throw new UIWidgetsError(
                "Scaffold.of() called with a context that does not contain a Scaffold.\n" +
                "No Scaffold ancestor could be found starting from the context that was passed to Scaffold.of(). " +
                "This usually happens when the context provided is from the same StatefulWidget as that " +
                "whose build function actually creates the Scaffold widget being sought.\n" +
                "There are several ways to avoid this problem. The simplest is to use a Builder to get a " +
                "context that is \"under\" the Scaffold. For an example of this, please see the " +
                "documentation for Scaffold.of():\n" +
                "  https://docs.flutter.io/flutter/material/Scaffold/of.html\n" +
                "A more efficient solution is to split your build function into several widgets. This " +
                "introduces a new context from which you can obtain the Scaffold. In this solution, " +
                "you would have an outer widget that creates the Scaffold populated by instances of " +
                "your new inner widgets, and then in these inner widgets you would use Scaffold.of().\n" +
                "A less elegant but more expedient solution is assign a GlobalKey to the Scaffold, " +
                "then use the key.currentState property to obtain the ScaffoldState rather than " +
                "using the Scaffold.of() function.\n" +
                "The context used was:\n" + context);
        }

        public static ValueListenable<ScaffoldGeometry> geometryOf(BuildContext context) {
            _ScaffoldScope scaffoldScope =
                (_ScaffoldScope) context.inheritFromWidgetOfExactType(typeof(_ScaffoldScope));
            if (scaffoldScope == null) {
                throw new UIWidgetsError(
                    "Scaffold.geometryOf() called with a context that does not contain a Scaffold.\n" +
                    "This usually happens when the context provided is from the same StatefulWidget as that " +
                    "whose build function actually creates the Scaffold widget being sought.\n" +
                    "There are several ways to avoid this problem. The simplest is to use a Builder to get a " +
                    "context that is \"under\" the Scaffold. For an example of this, please see the " +
                    "documentation for Scaffold.of():\n" +
                    "  https://docs.flutter.io/flutter/material/Scaffold/of.html\n" +
                    "A more efficient solution is to split your build function into several widgets. This " +
                    "introduces a new context from which you can obtain the Scaffold. In this solution, " +
                    "you would have an outer widget that creates the Scaffold populated by instances of " +
                    "your new inner widgets, and then in these inner widgets you would use Scaffold.geometryOf().\n" +
                    "The context used was:\n" + context);
            }

            return scaffoldScope.geometryNotifier;
        }

        static bool hasDrawer(BuildContext context, bool registerForUpdates = true) {
            D.assert(context != null);
            if (registerForUpdates) {
                _ScaffoldScope scaffold = (_ScaffoldScope) context.inheritFromWidgetOfExactType(typeof(_ScaffoldScope));
                return scaffold?.hasDrawer ?? false;
            }
            else {
                ScaffoldState scaffold = (ScaffoldState) context.ancestorStateOfType(new TypeMatcher<ScaffoldState>());
                return scaffold?.hasDrawer ?? false;
            }
        }

        public override State createState() {
            return new ScaffoldState();
        }
    }

    public class ScaffoldState : TickerProviderStateMixin<Scaffold> {
        // DRAWER API
        public readonly GlobalKey<DrawerControllerState> _drawerKey = GlobalKey<DrawerControllerState>.key();
        public readonly GlobalKey<DrawerControllerState> _endDrawerKey = GlobalKey<DrawerControllerState>.key();

        public bool hasDrawer {
            get { return this.widget.drawer != null; }
        }

        public bool hasEndDrawer {
            get { return this.widget.endDrawer != null; }
        }

        bool _drawerOpened = false;
        bool _endDrawerOpened = false;

        public bool isDrawerOpen {
            get { return this._drawerOpened; }
        }

        public bool isEndDrawerOpen {
            get { return this._endDrawerOpened; }
        }

        void _drawerOpenedCallback(bool isOpened) {
            this.setState(() => { this._drawerOpened = isOpened; });
        }

        void _endDrawerOpenedCallback(bool isOpened) {
            this.setState(() => { this._endDrawerOpened = isOpened; });
        }


        public void openDrawer() {
            if (this._endDrawerKey.currentState != null && this._endDrawerOpened) {
                this._endDrawerKey.currentState.close();
            }

            this._drawerKey.currentState?.open();
        }

        public void openEndDrawer() {
            if (this._drawerKey.currentState != null && this._drawerOpened) {
                this._drawerKey.currentState.close();
            }

            this._endDrawerKey.currentState?.open();
        }

        // SNACK BAR API
        readonly Queue<ScaffoldFeatureController<SnackBar, SnackBarClosedReason>> _snackBars =
            new Queue<ScaffoldFeatureController<SnackBar, SnackBarClosedReason>>();

        AnimationController _snackBarController;
        Timer _snackBarTimer;
        bool _accessibleNavigation;

        public ScaffoldFeatureController<SnackBar, SnackBarClosedReason> showSnackBar(SnackBar snackbar) {
            if (this._snackBarController == null) {
                this._snackBarController = SnackBar.createAnimationController(vsync: this);
                this._snackBarController.addStatusListener(this._handleSnackBarStatusChange);
            }

            if (this._snackBars.isEmpty()) {
                D.assert(this._snackBarController.isDismissed);
                this._snackBarController.forward();
            }

            ScaffoldFeatureController<SnackBar, SnackBarClosedReason> controller = null;
            controller = new ScaffoldFeatureController<SnackBar, SnackBarClosedReason>(
                snackbar.withAnimation(this._snackBarController, fallbackKey: new UniqueKey()),
                new Promise<SnackBarClosedReason>(),
                () => {
                    D.assert(this._snackBars.First() == controller);
                    this.hideCurrentSnackBar(reason: SnackBarClosedReason.hide);
                },
                null);

            this.setState(() => { this._snackBars.Enqueue(controller); });
            return controller;
        }


        void _handleSnackBarStatusChange(AnimationStatus status) {
            switch (status) {
                case AnimationStatus.dismissed: {
                    D.assert(this._snackBars.isNotEmpty());
                    this.setState(() => { this._snackBars.Dequeue(); });
                    if (this._snackBars.isNotEmpty()) {
                        this._snackBarController.forward();
                    }

                    break;
                }
                case AnimationStatus.completed: {
                    this.setState(() => { D.assert(this._snackBarTimer == null); });
                    break;
                }
                case AnimationStatus.forward:
                case AnimationStatus.reverse: {
                    break;
                }
            }
        }

        public void removeCurrentSnackBar(SnackBarClosedReason reason = SnackBarClosedReason.remove) {
            if (this._snackBars.isEmpty()) {
                return;
            }

            Promise<SnackBarClosedReason> completer = this._snackBars.First()._completer;
            if (!completer.isCompleted) {
                completer.Resolve(reason);
            }

            this._snackBarTimer?.cancel();
            this._snackBarTimer = null;
            this._snackBarController.setValue(0.0f);
        }

        public void hideCurrentSnackBar(SnackBarClosedReason reason = SnackBarClosedReason.hide) {
            if (this._snackBars.isEmpty() || this._snackBarController.status == AnimationStatus.dismissed) {
                return;
            }

            MediaQueryData mediaQuery = MediaQuery.of(this.context);
            Promise<SnackBarClosedReason> completer = this._snackBars.First()._completer;
            if (mediaQuery.accessibleNavigation) {
                this._snackBarController.setValue(0.0f);
                completer.Resolve(reason);
            }
            else {
                this._snackBarController.reverse().Then(() => {
                    D.assert(this.mounted);
                    if (!completer.isCompleted) {
                        completer.Resolve(reason);
                    }
                });
            }

            this._snackBarTimer?.cancel();
            this._snackBarTimer = null;
        }

        // PERSISTENT BOTTOM SHEET API
        readonly List<_PersistentBottomSheet> _dismissedBottomSheets = new List<_PersistentBottomSheet>();
        PersistentBottomSheetController<object> _currentBottomSheet;

        void _maybeBuildCurrentBottomSheet() {
            if (this.widget.bottomSheet != null) {
                AnimationController controller = BottomSheet.createAnimationController(this);
                controller.setValue(1.0f);
                this._currentBottomSheet = this._buildBottomSheet<object>(
                    (BuildContext context) => this.widget.bottomSheet,
                    controller,
                    false,
                    null);
            }
        }

        void _closeCurrentBottomSheet() {
            if (this._currentBottomSheet != null) {
                this._currentBottomSheet.close();
                D.assert(this._currentBottomSheet == null);
            }
        }


        PersistentBottomSheetController<T> _buildBottomSheet<T>(WidgetBuilder builder, AnimationController controller,
            bool isLocalHistoryEntry, T resolveValue) {
            Promise<T> completer = new Promise<T>();
            GlobalKey<_PersistentBottomSheetState> bottomSheetKey = GlobalKey<_PersistentBottomSheetState>.key();
            _PersistentBottomSheet bottomSheet = null;

            void _removeCurrentBottomSheet() {
                D.assert(this._currentBottomSheet._widget == bottomSheet);
                D.assert(bottomSheetKey.currentState != null);
                bottomSheetKey.currentState.close();
                if (controller.status != AnimationStatus.dismissed) {
                    this._dismissedBottomSheets.Add(bottomSheet);
                }

                this.setState(() => { this._currentBottomSheet = null; });
                completer.Resolve(resolveValue);
            }

            LocalHistoryEntry entry = isLocalHistoryEntry
                ? new LocalHistoryEntry(onRemove: _removeCurrentBottomSheet)
                : null;

            bottomSheet = new _PersistentBottomSheet(
                key: bottomSheetKey,
                animationController: controller,
                enableDrag: isLocalHistoryEntry,
                onClosing: () => {
                    D.assert(this._currentBottomSheet._widget == bottomSheet);
                    if (isLocalHistoryEntry) {
                        entry.remove();
                    }
                    else {
                        _removeCurrentBottomSheet();
                    }
                },
                onDismissed: () => {
                    if (this._dismissedBottomSheets.Contains(bottomSheet)) {
                        bottomSheet.animationController.dispose();
                        this.setState(() => { this._dismissedBottomSheets.Remove(bottomSheet); });
                    }
                },
                builder: builder);

            if (isLocalHistoryEntry) {
                ModalRoute.of(this.context).addLocalHistoryEntry(entry);
            }

            return new PersistentBottomSheetController<T>(
                bottomSheet,
                completer,
                isLocalHistoryEntry ? (VoidCallback) entry.remove : _removeCurrentBottomSheet,
                (VoidCallback fn) => { bottomSheetKey.currentState?.setState(fn); },
                isLocalHistoryEntry);
        }

        public PersistentBottomSheetController<object> showBottomSheet(WidgetBuilder builder) {
            this._closeCurrentBottomSheet();
            AnimationController controller = BottomSheet.createAnimationController(this);
            controller.forward();
            this.setState(() => {
                this._currentBottomSheet = this._buildBottomSheet<object>(builder, controller, true, null);
            });
            return this._currentBottomSheet;
        }

        // FLOATING ACTION BUTTON API
        AnimationController _floatingActionButtonMoveController;
        FloatingActionButtonAnimator _floatingActionButtonAnimator;
        FloatingActionButtonLocation _previousFloatingActionButtonLocation;
        FloatingActionButtonLocation _floatingActionButtonLocation;

        void _moveFloatingActionButton(FloatingActionButtonLocation newLocation) {
            FloatingActionButtonLocation previousLocation = this._floatingActionButtonLocation;
            float restartAnimationFrom = 0.0f;
            if (this._floatingActionButtonMoveController.isAnimating) {
                previousLocation = new _TransitionSnapshotFabLocation(this._previousFloatingActionButtonLocation,
                    this._floatingActionButtonLocation,
                    this._floatingActionButtonAnimator,
                    this._floatingActionButtonMoveController.value);
                restartAnimationFrom =
                    this._floatingActionButtonAnimator.getAnimationRestart(this._floatingActionButtonMoveController
                        .value);
            }

            this.setState(() => {
                this._previousFloatingActionButtonLocation = previousLocation;
                this._floatingActionButtonLocation = newLocation;
            });

            this._floatingActionButtonMoveController.forward(from: restartAnimationFrom);
        }

        // IOS FEATURES
        readonly ScrollController _primaryScrollController = new ScrollController();

        void _handleStatusBarTap() {
            if (this._primaryScrollController.hasClients) {
                this._primaryScrollController.animateTo(
                    to: 0.0f,
                    duration: new TimeSpan(0, 0, 0, 0, 300),
                    curve: Curves.linear);
            }
        }

        // INTERNALS
        _ScaffoldGeometryNotifier _geometryNotifier;

        bool _resizeToAvoidBottomInset {
            get { return this.widget.resizeToAvoidBottomInset ?? this.widget.resizeToAvoidBottomPadding ?? true; }
        }

        public override void initState() {
            base.initState();
            this._geometryNotifier = new _ScaffoldGeometryNotifier(new ScaffoldGeometry(), this.context);
            this._floatingActionButtonLocation = this.widget.floatingActionButtonLocation ??
                                                 ScaffoldUtils._kDefaultFloatingActionButtonLocation;
            this._floatingActionButtonAnimator = this.widget.floatingActionButtonAnimator ??
                                                 ScaffoldUtils._kDefaultFloatingActionButtonAnimator;
            this._previousFloatingActionButtonLocation = this._floatingActionButtonLocation;
            this._floatingActionButtonMoveController = new AnimationController(
                vsync: this,
                lowerBound: 0.0f,
                upperBound: 1.0f,
                value: 1.0f,
                duration: FloatingActionButtonLocationUtils.kFloatingActionButtonSegue +
                          FloatingActionButtonLocationUtils.kFloatingActionButtonSegue
            );

            this._maybeBuildCurrentBottomSheet();
        }


        public override void didUpdateWidget(StatefulWidget oldWidget) {
            Scaffold _oldWidget = (Scaffold) oldWidget;
            if (this.widget.floatingActionButtonAnimator != _oldWidget.floatingActionButtonAnimator) {
                this._floatingActionButtonAnimator = this.widget.floatingActionButtonAnimator ??
                                                     ScaffoldUtils._kDefaultFloatingActionButtonAnimator;
            }

            if (this.widget.floatingActionButtonLocation != _oldWidget.floatingActionButtonLocation) {
                this._moveFloatingActionButton(this.widget.floatingActionButtonLocation ??
                                               ScaffoldUtils._kDefaultFloatingActionButtonLocation);
            }

            if (this.widget.bottomSheet != _oldWidget.bottomSheet) {
                D.assert(() => {
                    if (this.widget.bottomSheet != null && this._currentBottomSheet?._isLocalHistoryEntry == true) {
                        throw new UIWidgetsError(
                            "Scaffold.bottomSheet cannot be specified while a bottom sheet displayed " +
                            "with showBottomSheet() is still visible.\n Use the PersistentBottomSheetController " +
                            "returned by showBottomSheet() to close the old bottom sheet before creating " +
                            "a Scaffold with a (non null) bottomSheet.");
                    }

                    return true;
                });
                this._closeCurrentBottomSheet();
                this._maybeBuildCurrentBottomSheet();
            }

            base.didUpdateWidget(oldWidget);
        }


        public override void didChangeDependencies() {
            MediaQueryData mediaQuery = MediaQuery.of(this.context);

            if (this._accessibleNavigation
                && !mediaQuery.accessibleNavigation
                && this._snackBarTimer != null) {
                this.hideCurrentSnackBar(reason: SnackBarClosedReason.timeout);
            }

            this._accessibleNavigation = mediaQuery.accessibleNavigation;
            base.didChangeDependencies();
        }

        public override void dispose() {
            this._snackBarController?.dispose();
            this._snackBarTimer?.cancel();
            this._snackBarTimer = null;
            this._geometryNotifier.dispose();
            foreach (_PersistentBottomSheet bottomSheet in this._dismissedBottomSheets) {
                bottomSheet.animationController.dispose();
            }

            if (this._currentBottomSheet != null) {
                this._currentBottomSheet._widget.animationController.dispose();
            }

            this._floatingActionButtonMoveController.dispose();
            base.dispose();
        }

        void _addIfNonNull(List<LayoutId> children, Widget child, object childId,
            bool removeLeftPadding,
            bool removeTopPadding,
            bool removeRightPadding,
            bool removeBottomPadding,
            bool removeBottomInset = false
        ) {
            MediaQueryData data = MediaQuery.of(this.context).removePadding(
                removeLeft: removeLeftPadding,
                removeTop: removeTopPadding,
                removeRight: removeRightPadding,
                removeBottom: removeBottomPadding
            );
            if (removeBottomInset) {
                data = data.removeViewInsets(removeBottom: true);
            }

            if (child != null) {
                children.Add(
                    new LayoutId(
                        id: childId,
                        child: new MediaQuery(data: data, child: child)
                    )
                );
            }
        }

        void _buildEndDrawer(List<LayoutId> children) {
            if (this.widget.endDrawer != null) {
                D.assert(this.hasEndDrawer);
                this._addIfNonNull(
                    children: children,
                    new DrawerController(
                        key: this._endDrawerKey,
                        alignment: DrawerAlignment.end,
                        child: this.widget.endDrawer,
                        drawerCallback: this._endDrawerOpenedCallback,
                        dragStartBehavior: this.widget.drawerDragStartBehavior
                    ),
                    childId: _ScaffoldSlot.endDrawer,
                    removeLeftPadding: true,
                    removeTopPadding: false,
                    removeRightPadding: false,
                    removeBottomPadding: false
                );
            }
        }

        void _buildDrawer(List<LayoutId> children) {
            if (this.widget.drawer != null) {
                D.assert(this.hasDrawer);
                this._addIfNonNull(
                    children: children,
                    new DrawerController(
                        key: this._drawerKey,
                        alignment: DrawerAlignment.start,
                        child: this.widget.drawer,
                        drawerCallback: this._drawerOpenedCallback,
                        dragStartBehavior: this.widget.drawerDragStartBehavior
                    ),
                    childId: _ScaffoldSlot.drawer,
                    removeLeftPadding: false,
                    removeTopPadding: false,
                    removeRightPadding: true,
                    removeBottomPadding: false
                );
            }
        }

        public override Widget build(BuildContext context) {
            MediaQueryData mediaQuery = MediaQuery.of(context);
            ThemeData themeData = Theme.of(context);

            this._accessibleNavigation = mediaQuery.accessibleNavigation;

            if (this._snackBars.isNotEmpty()) {
                ModalRoute route = ModalRoute.of(context);
                if (route == null || route.isCurrent) {
                    if (this._snackBarController.isCompleted && this._snackBarTimer == null) {
                        SnackBar snackBar = this._snackBars.First()._widget;
                        this._snackBarTimer = Window.instance.run(snackBar.duration, () => {
                            D.assert(this._snackBarController.status == AnimationStatus.forward ||
                                     this._snackBarController.status == AnimationStatus.completed);
                            MediaQueryData subMediaQuery = MediaQuery.of(context);
                            if (subMediaQuery.accessibleNavigation && snackBar.action != null) {
                                return;
                            }

                            this.hideCurrentSnackBar(reason: SnackBarClosedReason.timeout);
                        });
                    }
                }
                else {
                    this._snackBarTimer?.cancel();
                    this._snackBarTimer = null;
                }
            }

            List<LayoutId> children = new List<LayoutId>();

            this._addIfNonNull(
                children: children,
                child: this.widget.body != null && this.widget.extendBody
                    ? new _BodyBuilder(body: this.widget.body) : this.widget.body,
                childId: _ScaffoldSlot.body,
                removeLeftPadding: false,
                removeTopPadding: this.widget.appBar != null,
                removeRightPadding: false,
                removeBottomPadding: this.widget.bottomNavigationBar != null ||
                                     this.widget.persistentFooterButtons != null,
                removeBottomInset: this._resizeToAvoidBottomInset
            );

            if (this.widget.appBar != null) {
                float topPadding = this.widget.primary ? mediaQuery.padding.top : 0.0f;
                float extent = this.widget.appBar.preferredSize.height + topPadding;
                D.assert(extent >= 0.0f && extent.isFinite());
                this._addIfNonNull(
                    children: children,
                    new ConstrainedBox(
                        constraints: new BoxConstraints(maxHeight: extent),
                        child: FlexibleSpaceBar.createSettings(
                            currentExtent: extent,
                            child: this.widget.appBar
                        )
                    ),
                    childId: _ScaffoldSlot.appBar,
                    removeLeftPadding: false,
                    removeTopPadding: false,
                    removeRightPadding: false,
                    removeBottomPadding: true
                );
            }

            if (this._snackBars.isNotEmpty()) {
                this._addIfNonNull(
                    children: children,
                    child: this._snackBars.First()._widget,
                    childId: _ScaffoldSlot.snackBar,
                    removeLeftPadding: false,
                    removeTopPadding: true,
                    removeRightPadding: false,
                    removeBottomPadding: this.widget.bottomNavigationBar != null ||
                                         this.widget.persistentFooterButtons != null
                );
            }

            if (this.widget.persistentFooterButtons != null) {
                this._addIfNonNull(
                    children: children,
                    new Container(
                        decoration: new BoxDecoration(
                            border: new Border(
                                top: Divider.createBorderSide(context, width: 1.0f)
                            )
                        ),
                        child: new SafeArea(
                            child: ButtonTheme.bar(
                                child: new SafeArea(
                                    top: false,
                                    child: new ButtonBar(
                                        children: this.widget.persistentFooterButtons
                                    )
                                )
                            )
                        )
                    ),
                    childId: _ScaffoldSlot.persistentFooter,
                    removeLeftPadding: false,
                    removeTopPadding: true,
                    removeRightPadding: false,
                    removeBottomPadding: false
                );
            }

            if (this.widget.bottomNavigationBar != null) {
                this._addIfNonNull(
                    children: children,
                    child: this.widget.bottomNavigationBar,
                    childId: _ScaffoldSlot.bottomNavigationBar,
                    removeLeftPadding: false,
                    removeTopPadding: true,
                    removeRightPadding: false,
                    removeBottomPadding: false
                );
            }

            if (this._currentBottomSheet != null || this._dismissedBottomSheets.isNotEmpty()) {
                List<Widget> bottomSheets = new List<Widget>();
                if (this._dismissedBottomSheets.isNotEmpty()) {
                    bottomSheets.AddRange(this._dismissedBottomSheets);
                }

                if (this._currentBottomSheet != null) {
                    bottomSheets.Add(this._currentBottomSheet._widget);
                }

                Widget stack = new Stack(
                    children: bottomSheets,
                    alignment: Alignment.bottomCenter
                );
                this._addIfNonNull(
                    children: children,
                    child: stack,
                    childId: _ScaffoldSlot.bottomSheet,
                    removeLeftPadding: false,
                    removeTopPadding: true,
                    removeRightPadding: false,
                    removeBottomPadding: this._resizeToAvoidBottomInset
                );
            }

            this._addIfNonNull(
                children: children,
                new _FloatingActionButtonTransition(
                    child: this.widget.floatingActionButton,
                    fabMoveAnimation: this._floatingActionButtonMoveController,
                    fabMotionAnimator: this._floatingActionButtonAnimator,
                    geometryNotifier: this._geometryNotifier
                ),
                childId: _ScaffoldSlot.floatingActionButton,
                removeLeftPadding: true,
                removeTopPadding: true,
                removeRightPadding: true,
                removeBottomPadding: true
            );

            switch (themeData.platform) {
                case RuntimePlatform.IPhonePlayer:
                    this._addIfNonNull(
                        children: children,
                        new GestureDetector(
                            behavior: HitTestBehavior.opaque,
                            onTap: this._handleStatusBarTap
                        ),
                        childId: _ScaffoldSlot.statusBar,
                        removeLeftPadding: false,
                        removeTopPadding: true,
                        removeRightPadding: false,
                        removeBottomPadding: true
                    );
                    break;
            }

            if (this._endDrawerOpened) {
                this._buildDrawer(children);
                this._buildEndDrawer(children);
            }
            else {
                this._buildEndDrawer(children);
                this._buildDrawer(children);
            }

            EdgeInsets minInsets = mediaQuery.padding.copyWith(
                bottom: this._resizeToAvoidBottomInset ? mediaQuery.viewInsets.bottom : 0.0f
            );

            bool _extendBody = !(minInsets.bottom > 0) && this.widget.extendBody;
            
            return new _ScaffoldScope(
                hasDrawer: this.hasDrawer,
                geometryNotifier: this._geometryNotifier,
                child: new PrimaryScrollController(
                    controller: this._primaryScrollController,
                    child: new Material(
                        color: this.widget.backgroundColor ?? themeData.scaffoldBackgroundColor,
                        child: new AnimatedBuilder(animation: this._floatingActionButtonMoveController,
                            builder: (BuildContext subContext, Widget child) => {
                                return new CustomMultiChildLayout(
                                    children: new List<Widget>(children),
                                    layoutDelegate: new _ScaffoldLayout(
                                        extendBody: _extendBody,
                                        minInsets: minInsets,
                                        currentFloatingActionButtonLocation: this._floatingActionButtonLocation,
                                        floatingActionButtonMoveAnimationProgress: this
                                            ._floatingActionButtonMoveController.value,
                                        floatingActionButtonMotionAnimator: this._floatingActionButtonAnimator,
                                        geometryNotifier: this._geometryNotifier,
                                        previousFloatingActionButtonLocation: this._previousFloatingActionButtonLocation
                                    )
                                );
                            }
                        )
                    )
                )
            );
        }
    }

    public class ScaffoldFeatureController<T, U> where T : Widget {
        public ScaffoldFeatureController(
            T _widget,
            Promise<U> _completer,
            VoidCallback close,
            StateSetter setState) {
            this._widget = _widget;
            this._completer = _completer;
            this.close = close;
            this.setState = setState;
        }

        public readonly T _widget;

        public readonly Promise<U> _completer;

        public IPromise<U> closed {
            get { return this._completer; }
        }

        public readonly VoidCallback close;

        public readonly StateSetter setState;
    }


    public class _PersistentBottomSheet : StatefulWidget {
        public _PersistentBottomSheet(
            Key key = null,
            AnimationController animationController = null,
            bool enableDrag = true,
            VoidCallback onClosing = null,
            VoidCallback onDismissed = null,
            WidgetBuilder builder = null
        ) : base(key: key) {
            this.animationController = animationController;
            this.enableDrag = enableDrag;
            this.onClosing = onClosing;
            this.onDismissed = onDismissed;
            this.builder = builder;
        }

        public readonly AnimationController animationController;

        public readonly bool enableDrag;

        public readonly VoidCallback onClosing;

        public readonly VoidCallback onDismissed;

        public readonly WidgetBuilder builder;

        public override State createState() {
            return new _PersistentBottomSheetState();
        }
    }


    class _PersistentBottomSheetState : State<_PersistentBottomSheet> {
        public override void initState() {
            base.initState();
            D.assert(this.widget.animationController.status == AnimationStatus.forward
                     || this.widget.animationController.status == AnimationStatus.completed);
            this.widget.animationController.addStatusListener(this._handleStatusChange);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            _PersistentBottomSheet _oldWidget = (_PersistentBottomSheet) oldWidget;
            D.assert(this.widget.animationController == _oldWidget.animationController);
        }

        public void close() {
            this.widget.animationController.reverse();
        }

        void _handleStatusChange(AnimationStatus status) {
            if (status == AnimationStatus.dismissed && this.widget.onDismissed != null) {
                this.widget.onDismissed();
            }
        }

        public override Widget build(BuildContext context) {
            return new AnimatedBuilder(
                animation: this.widget.animationController,
                builder: (BuildContext subContext, Widget child) => {
                    return new Align(
                        alignment: Alignment.topLeft,
                        heightFactor: this.widget.animationController.value,
                        child: child);
                },
                child: new BottomSheet(
                    animationController: this.widget.animationController,
                    enableDrag: this.widget.enableDrag,
                    onClosing: this.widget.onClosing,
                    builder: this.widget.builder));
        }
    }


    public class PersistentBottomSheetController<T> : ScaffoldFeatureController<_PersistentBottomSheet, T> {
        public PersistentBottomSheetController(
            _PersistentBottomSheet widget,
            Promise<T> completer,
            VoidCallback close,
            StateSetter setState,
            bool _isLocalHistoryEntry
        ) : base(widget, completer, close, setState) {
            this._isLocalHistoryEntry = _isLocalHistoryEntry;
        }

        public readonly bool _isLocalHistoryEntry;
    }

    class _ScaffoldScope : InheritedWidget {
        public _ScaffoldScope(
            bool? hasDrawer = null,
            _ScaffoldGeometryNotifier geometryNotifier = null,
            Widget child = null
        ) : base(child: child) {
            D.assert(hasDrawer != null);
            this.hasDrawer = hasDrawer.Value;
            this.geometryNotifier = geometryNotifier;
        }

        public readonly bool hasDrawer;
        public readonly _ScaffoldGeometryNotifier geometryNotifier;

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            _ScaffoldScope _oldWidget = (_ScaffoldScope) oldWidget;
            return this.hasDrawer != _oldWidget.hasDrawer;
        }
    }
}