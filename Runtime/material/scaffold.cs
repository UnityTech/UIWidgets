using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.material {
    static class ScaffoldUtils {
        public static FloatingActionButtonLocation _kDefaultFloatingActionButtonLocation =
            FloatingActionButtonLocation.endFloat;

        public static FloatingActionButtonAnimator _kDefaultFloatingActionButtonAnimator =
            FloatingActionButtonAnimator.scaling;
    }

    public enum _ScaffoldSlot {
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

        public new ScaffoldGeometry value {
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

    class _ScaffoldLayout : MultiChildLayoutDelegate {
        public _ScaffoldLayout(
            EdgeInsets minInsets,
            _ScaffoldGeometryNotifier geometryNotifier,
            FloatingActionButtonLocation previousFloatingActionButtonLocation,
            FloatingActionButtonLocation currentFloatingActionButtonLocation,
            float floatingActionButtonMoveAnimationProgress,
            FloatingActionButtonAnimator floatingActionButtonMotionAnimator
        ) {
            D.assert(previousFloatingActionButtonLocation != null);
            D.assert(currentFloatingActionButtonLocation != null);

            this.minInsets = minInsets;
            this.geometryNotifier = geometryNotifier;
            this.previousFloatingActionButtonLocation = previousFloatingActionButtonLocation;
            this.currentFloatingActionButtonLocation = currentFloatingActionButtonLocation;
            this.floatingActionButtonMoveAnimationProgress = floatingActionButtonMoveAnimationProgress;
            this.floatingActionButtonMotionAnimator = floatingActionButtonMotionAnimator;
        }

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
                BoxConstraints bodyConstraints = new BoxConstraints(
                    maxWidth: fullWidthConstraints.maxWidth,
                    maxHeight: Mathf.Max(0.0f, contentBottom - contentTop)
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

        static Animatable<float> _entranceTurnTween = new FloatTween(
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
            bool resizeToAvoidBottomPadding = true,
            bool primary = true) : base(key: key) {
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
            this.primary = primary;
        }

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

        public readonly bool resizeToAvoidBottomPadding;

        public readonly bool primary;

        public static ScaffoldState of(BuildContext context, bool nullOk = false) {
            D.assert(context != null);
            ScaffoldState result = (ScaffoldState)context.ancestorStateOfType(new TypeMatcher<ScaffoldState>());
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

        static ValueListenable<ScaffoldGeometry> geometryOf(BuildContext context) {
            _ScaffoldScope scaffoldScope = (_ScaffoldScope)context.inheritFromWidgetOfExactType(typeof(_ScaffoldScope));
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
                _ScaffoldScope scaffold = (_ScaffoldScope)context.inheritFromWidgetOfExactType(typeof(_ScaffoldScope));
                return scaffold?.hasDrawer ?? false;
            }
            else {
                ScaffoldState scaffold = context.ancestorStateOfType(new TypeMatcher<ScaffoldState>());
                return scaffold?.hasDrawer ?? false;
            }
        }

        public override State createState() {
            return new ScaffoldState();
        }
    }

    public class ScaffoldState : TickerProviderStateMixin<Scaffold> {

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
        
        
    }
    
    class _ScaffoldScope : InheritedWidget {
        public _ScaffoldScope(
            bool? hasDrawer = null,
            _ScaffoldGeometryNotifier geometryNotifier = null,
            Widget child = null
        ) : base(child: child) {
            D.assert(hasDrawer != null);
            D.assert(child != null);
            D.assert(geometryNotifier != null);
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