using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    class NavBarUtils {
        public const float _kNavBarPersistentHeight = 44.0f;

        public const float _kNavBarLargeTitleHeightExtension = 52.0f;

        public const float _kNavBarShowLargeTitleThreshold = 10.0f;

        public const float _kNavBarEdgePadding = 16.0f;

        public const float _kNavBarBackButtonTapWidth = 50.0f;

        public static readonly TimeSpan _kNavBarTitleFadeDuration = new TimeSpan(0, 0, 0, 0, 150);

        public static readonly Color _kDefaultNavBarBorderColor = new Color(0x4C000000);

        public static readonly Border _kDefaultNavBarBorder = new Border(
            bottom: new BorderSide(
                color: _kDefaultNavBarBorderColor,
                width: 0.0f, // One physical pixel.
                style: BorderStyle.solid
            )
        );

        public static readonly _HeroTag _defaultHeroTag = new _HeroTag(null);

        public static Widget _wrapWithBackground(
            Border border = null,
            Color backgroundColor = null,
            Widget child = null,
            bool updateSystemUiOverlay = true
        ) {
            Widget result = child;
            if (updateSystemUiOverlay) {
                bool darkBackground = backgroundColor.computeLuminance() < 0.179f;
                SystemUiOverlayStyle overlayStyle = darkBackground
                    ? SystemUiOverlayStyle.light
                    : SystemUiOverlayStyle.dark;
                result = new AnnotatedRegion<SystemUiOverlayStyle>(
                    value: overlayStyle,
                    sized: true,
                    child: result
                );
            }

            DecoratedBox childWithBackground = new DecoratedBox(
                decoration: new BoxDecoration(
                    border: border,
                    color: backgroundColor
                ),
                child: result
            );
            if (backgroundColor.alpha == 0xFF) {
                return childWithBackground;
            }

            return new ClipRect(
                child: new BackdropFilter(
                    filter: ImageFilter.blur(sigmaX: 10.0f, sigmaY: 10.0f),
                    child: childWithBackground
                )
            );
        }

        public static Widget _wrapActiveColor(Color color, BuildContext context, Widget child) {
            if (color == null) {
                return child;
            }

            return new CupertinoTheme(
                data: CupertinoTheme.of(context).copyWith(primaryColor: color),
                child: child
            );
        }

        public static bool _isTransitionable(BuildContext context) {
            ModalRoute route = ModalRoute.of(context);
            return route is PageRoute && !(route as PageRoute).fullscreenDialog;
        }

        public static HeroFlightShuttleBuilder _navBarHeroFlightShuttleBuilder = (
            BuildContext flightContext,
            Animation<float> animation,
            HeroFlightDirection flightDirection,
            BuildContext fromHeroContext,
            BuildContext toHeroContext
        ) => {
            D.assert(animation != null);

            D.assert(fromHeroContext != null);
            D.assert(toHeroContext != null);
            D.assert(fromHeroContext.widget is Hero);
            D.assert(toHeroContext.widget is Hero);
            Hero fromHeroWidget = (Hero) fromHeroContext.widget;
            Hero toHeroWidget = (Hero) toHeroContext.widget;
            D.assert(fromHeroWidget.child is _TransitionableNavigationBar);
            D.assert(toHeroWidget.child is _TransitionableNavigationBar);
            _TransitionableNavigationBar fromNavBar = (_TransitionableNavigationBar) fromHeroWidget.child;
            _TransitionableNavigationBar toNavBar = (_TransitionableNavigationBar) toHeroWidget.child;
            D.assert(fromNavBar.componentsKeys != null);
            D.assert(toNavBar.componentsKeys != null);
            D.assert(
                fromNavBar.componentsKeys.navBarBoxKey.currentContext.owner != null,
                () => "The from nav bar to Hero must have been mounted in the previous frame"
            );

            D.assert(
                toNavBar.componentsKeys.navBarBoxKey.currentContext.owner != null,
                () => "The to nav bar to Hero must have been mounted in the previous frame"
            );

            switch (flightDirection) {
                case HeroFlightDirection.push:
                    return new _NavigationBarTransition(
                        animation: animation,
                        bottomNavBar: fromNavBar,
                        topNavBar: toNavBar
                    );
                case HeroFlightDirection.pop:
                    return new _NavigationBarTransition(
                        animation: animation,
                        bottomNavBar: toNavBar,
                        topNavBar: fromNavBar
                    );
            }

            throw new UIWidgetsError($"Unknown flight direction: {flightDirection}");
        };

        public static CreateRectTween _linearTranslateWithLargestRectSizeTween = (Rect begin, Rect end) => {
            Size largestSize = new Size(
                Mathf.Max(begin.size.width, end.size.width),
                Mathf.Max(begin.size.height, end.size.height)
            );
            return new RectTween(
                begin: begin.topLeft & largestSize,
                end: end.topLeft & largestSize
            );
        };

        public static TransitionBuilder _navBarHeroLaunchPadBuilder = (
            BuildContext context,
            Widget child
        ) => {
            D.assert(child is _TransitionableNavigationBar);
            return new Visibility(
                maintainSize: true,
                maintainAnimation: true,
                maintainState: true,
                visible: false,
                child: child
            );
        };
    }


    class _HeroTag {
        public _HeroTag(
            NavigatorState navigator
        ) {
            this.navigator = navigator;
        }

        public readonly NavigatorState navigator;

        public override string ToString() {
            return $"Default Hero tag for Cupertino navigation bars with navigator {this.navigator}";
        }

        public bool Equals(_HeroTag other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.navigator == other.navigator;
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

            return this.Equals((_HeroTag) obj);
        }

        public override int GetHashCode() {
            return this.navigator.GetHashCode();
        }

        public static bool operator ==(_HeroTag left, _HeroTag right) {
            return Equals(left, right);
        }

        public static bool operator !=(_HeroTag left, _HeroTag right) {
            return !Equals(left, right);
        }
    }

    public class CupertinoNavigationBar : ObstructingPreferredSizeWidget {
        public CupertinoNavigationBar(
            Key key = null,
            Widget leading = null,
            bool automaticallyImplyLeading = true,
            bool automaticallyImplyMiddle = true,
            string previousPageTitle = null,
            Widget middle = null,
            Widget trailing = null,
            Border border = null,
            Color backgroundColor = null,
            EdgeInsets padding = null,
            Color actionsForegroundColor = null,
            bool transitionBetweenRoutes = true,
            object heroTag = null
        ) : base(key: key) {
            this.leading = leading;
            this.automaticallyImplyLeading = automaticallyImplyLeading;
            this.automaticallyImplyMiddle = automaticallyImplyMiddle;
            this.previousPageTitle = previousPageTitle;
            this.middle = middle;
            this.trailing = trailing;
            this.border = border ?? NavBarUtils._kDefaultNavBarBorder;
            this.backgroundColor = backgroundColor;
            this.padding = padding;
            this.actionsForegroundColor = actionsForegroundColor;
            this.transitionBetweenRoutes = transitionBetweenRoutes;
            this.heroTag = heroTag ?? NavBarUtils._defaultHeroTag;

            D.assert(
                this.heroTag != null,
                () => "heroTag cannot be null. Use transitionBetweenRoutes = false to " +
                      "disable Hero transition on this navigation bar."
            );

            D.assert(
                !transitionBetweenRoutes || ReferenceEquals(this.heroTag, NavBarUtils._defaultHeroTag),
                () => "Cannot specify a heroTag override if this navigation bar does not " +
                      "transition due to transitionBetweenRoutes = false."
            );
        }

        public readonly Widget leading;

        public readonly bool automaticallyImplyLeading;

        public readonly bool automaticallyImplyMiddle;

        public readonly string previousPageTitle;

        public readonly Widget middle;

        public readonly Widget trailing;


        public readonly Color backgroundColor;

        public readonly EdgeInsets padding;

        public readonly Border border;

        public readonly Color actionsForegroundColor;

        public readonly bool transitionBetweenRoutes;

        public readonly object heroTag;

        public override bool? fullObstruction {
            get { return this.backgroundColor == null ? null : (bool?) (this.backgroundColor.alpha == 0xFF); }
        }

        public override Size preferredSize {
            get { return Size.fromHeight(NavBarUtils._kNavBarPersistentHeight); }
        }

        public override State createState() {
            return new _CupertinoNavigationBarState();
        }
    }

    class _CupertinoNavigationBarState : State<CupertinoNavigationBar> {
        _NavigationBarStaticComponentsKeys keys;

        public override void initState() {
            base.initState();
            this.keys = new _NavigationBarStaticComponentsKeys();
        }

        public override Widget build(BuildContext context) {
            Color backgroundColor = this.widget.backgroundColor ?? CupertinoTheme.of(context).barBackgroundColor;

            _NavigationBarStaticComponents components = new _NavigationBarStaticComponents(
                keys: this.keys,
                route: ModalRoute.of(context),
                userLeading: this.widget.leading,
                automaticallyImplyLeading: this.widget.automaticallyImplyLeading,
                automaticallyImplyTitle: this.widget.automaticallyImplyMiddle,
                previousPageTitle: this.widget.previousPageTitle,
                userMiddle: this.widget.middle,
                userTrailing: this.widget.trailing,
                padding: this.widget.padding,
                userLargeTitle: null,
                large: false
            );

            Widget navBar = NavBarUtils._wrapWithBackground(
                border: this.widget.border,
                backgroundColor: backgroundColor,
                child: new DefaultTextStyle(
                    style: CupertinoTheme.of(context).textTheme.textStyle,
                    child: new _PersistentNavigationBar(
                        components: components,
                        padding: this.widget.padding
                    )
                )
            );

            if (!this.widget.transitionBetweenRoutes || !NavBarUtils._isTransitionable(context)) {
                return NavBarUtils._wrapActiveColor(this.widget.actionsForegroundColor, context,
                    navBar); // ignore: deprecated_member_use_from_same_package
            }

            return NavBarUtils._wrapActiveColor(
                this.widget.actionsForegroundColor, // ignore: deprecated_member_use_from_same_package
                context,
                new Builder(
                    builder: (BuildContext _context) => {
                        return new Hero(
                            tag: this.widget.heroTag as _HeroTag == NavBarUtils._defaultHeroTag
                                ? new _HeroTag(Navigator.of(_context))
                                : this.widget.heroTag,
                            createRectTween: NavBarUtils._linearTranslateWithLargestRectSizeTween,
                            placeholderBuilder: NavBarUtils._navBarHeroLaunchPadBuilder,
                            flightShuttleBuilder: NavBarUtils._navBarHeroFlightShuttleBuilder,
                            transitionOnUserGestures: true,
                            child: new _TransitionableNavigationBar(
                                componentsKeys: this.keys,
                                backgroundColor: backgroundColor,
                                backButtonTextStyle: CupertinoTheme.of(_context).textTheme.navActionTextStyle,
                                titleTextStyle: CupertinoTheme.of(_context).textTheme.navTitleTextStyle,
                                largeTitleTextStyle: null,
                                border: this.widget.border,
                                hasUserMiddle: this.widget.middle != null,
                                largeExpanded: false,
                                child: navBar
                            )
                        );
                    }
                )
            );
        }
    }

    public class CupertinoSliverNavigationBar : StatefulWidget {
        public CupertinoSliverNavigationBar(
            Key key = null,
            Widget largeTitle = null,
            Widget leading = null,
            bool automaticallyImplyLeading = true,
            bool automaticallyImplyTitle = true,
            string previousPageTitle = null,
            Widget middle = null,
            Widget trailing = null,
            Border border = null,
            Color backgroundColor = null,
            EdgeInsets padding = null,
            Color actionsForegroundColor = null,
            bool transitionBetweenRoutes = true,
            object heroTag = null
        ) : base(key: key) {
            D.assert(
                automaticallyImplyTitle == true || largeTitle != null,
                () => "No largeTitle has been provided but automaticallyImplyTitle is also " +
                      "false. Either provide a largeTitle or set automaticallyImplyTitle to " +
                      "true."
            );
            this.largeTitle = largeTitle;
            this.leading = leading;
            this.automaticallyImplyLeading = automaticallyImplyLeading;
            this.automaticallyImplyTitle = automaticallyImplyTitle;
            this.previousPageTitle = previousPageTitle;
            this.middle = middle;
            this.trailing = trailing;
            this.border = border ?? NavBarUtils._kDefaultNavBarBorder;
            this.backgroundColor = backgroundColor;
            this.padding = padding;
            this.actionsForegroundColor = actionsForegroundColor;
            this.transitionBetweenRoutes = transitionBetweenRoutes;
            this.heroTag = heroTag ?? NavBarUtils._defaultHeroTag;
        }

        public readonly Widget largeTitle;

        public readonly Widget leading;

        public readonly bool automaticallyImplyLeading;

        public readonly bool automaticallyImplyTitle;

        public readonly string previousPageTitle;

        public readonly Widget middle;

        public readonly Widget trailing;

        public readonly Color backgroundColor;

        public readonly EdgeInsets padding;

        public readonly Border border;

        public readonly Color actionsForegroundColor;

        public readonly bool transitionBetweenRoutes;

        public readonly object heroTag;

        public bool opaque {
            get { return this.backgroundColor.alpha == 0xFF; }
        }

        public override State createState() {
            return new _CupertinoSliverNavigationBarState();
        }
    }

    class _CupertinoSliverNavigationBarState : State<CupertinoSliverNavigationBar> {
        _NavigationBarStaticComponentsKeys keys;

        public override void initState() {
            base.initState();
            this.keys = new _NavigationBarStaticComponentsKeys();
        }

        public override Widget build(BuildContext context) {
            Color actionsForegroundColor =
                this.widget.actionsForegroundColor ??
                CupertinoTheme.of(context).primaryColor; // ignore: deprecated_member_use_from_same_package

            _NavigationBarStaticComponents components = new _NavigationBarStaticComponents(
                keys: this.keys,
                route: ModalRoute.of(context),
                userLeading: this.widget.leading,
                automaticallyImplyLeading: this.widget.automaticallyImplyLeading,
                automaticallyImplyTitle: this.widget.automaticallyImplyTitle,
                previousPageTitle: this.widget.previousPageTitle,
                userMiddle: this.widget.middle,
                userTrailing: this.widget.trailing,
                userLargeTitle: this.widget.largeTitle,
                padding: this.widget.padding,
                large: true
            );

            return NavBarUtils._wrapActiveColor(
                this.widget.actionsForegroundColor, // ignore: deprecated_member_use_from_same_package
                context,
                new SliverPersistentHeader(
                    pinned: true, // iOS navigation bars are always pinned.
                    del: new _LargeTitleNavigationBarSliverDelegate(
                        keys: this.keys,
                        components: components,
                        userMiddle: this.widget.middle,
                        backgroundColor: this.widget.backgroundColor ?? CupertinoTheme.of(context).barBackgroundColor,
                        border: this.widget.border,
                        padding: this.widget.padding,
                        actionsForegroundColor: actionsForegroundColor,
                        transitionBetweenRoutes: this.widget.transitionBetweenRoutes,
                        heroTag: this.widget.heroTag,
                        persistentHeight: NavBarUtils._kNavBarPersistentHeight + MediaQuery.of(context).padding.top,
                        alwaysShowMiddle: this.widget.middle != null
                    )
                )
            );
        }
    }

    class _LargeTitleNavigationBarSliverDelegate
        : SliverPersistentHeaderDelegate {
        public _LargeTitleNavigationBarSliverDelegate(
            _NavigationBarStaticComponentsKeys keys,
            _NavigationBarStaticComponents components,
            Widget userMiddle,
            Color backgroundColor,
            Border border,
            EdgeInsets padding,
            Color actionsForegroundColor,
            bool transitionBetweenRoutes,
            object heroTag,
            float persistentHeight,
            bool alwaysShowMiddle
        ) {
            this.keys = keys;
            this.components = components;
            this.userMiddle = userMiddle;
            this.backgroundColor = backgroundColor;
            this.border = border;
            this.padding = padding;
            this.actionsForegroundColor = actionsForegroundColor;
            this.transitionBetweenRoutes = transitionBetweenRoutes;
            this.heroTag = heroTag;
            this.persistentHeight = persistentHeight;
            this.alwaysShowMiddle = alwaysShowMiddle;
        }

        public readonly _NavigationBarStaticComponentsKeys keys;
        public readonly _NavigationBarStaticComponents components;
        public readonly Widget userMiddle;
        public readonly Color backgroundColor;
        public readonly Border border;
        public readonly EdgeInsets padding;
        public readonly Color actionsForegroundColor;
        public readonly bool transitionBetweenRoutes;
        public readonly object heroTag;
        public readonly float persistentHeight;
        public readonly bool alwaysShowMiddle;

        public override float? minExtent {
            get { return this.persistentHeight; }
        }

        public override float? maxExtent {
            get { return this.persistentHeight + NavBarUtils._kNavBarLargeTitleHeightExtension; }
        }

        public override Widget build(BuildContext context, float shrinkOffset, bool overlapsContent) {
            bool showLargeTitle =
                shrinkOffset < this.maxExtent - this.minExtent - NavBarUtils._kNavBarShowLargeTitleThreshold;

            _PersistentNavigationBar persistentNavigationBar =
                new _PersistentNavigationBar(
                    components: this.components,
                    padding: this.padding,
                    middleVisible: this.alwaysShowMiddle ? null : (bool?) !showLargeTitle
                );

            Widget navBar = NavBarUtils._wrapWithBackground(
                border: this.border,
                backgroundColor: this.backgroundColor,
                child: new DefaultTextStyle(
                    style: CupertinoTheme.of(context).textTheme.textStyle,
                    child: new Stack(
                        fit: StackFit.expand,
                        children: new List<Widget> {
                            new Positioned(
                                top: this.persistentHeight,
                                left: 0.0f,
                                right: 0.0f,
                                bottom: 0.0f,
                                child: new ClipRect(
                                    child: new OverflowBox(
                                        minHeight: 0.0f,
                                        maxHeight: float.PositiveInfinity,
                                        alignment: Alignment.bottomLeft,
                                        child: new Padding(
                                            padding: EdgeInsets.only(
                                                left: NavBarUtils._kNavBarEdgePadding,
                                                bottom: 8.0f
                                            ),
                                            child: new SafeArea(
                                                top: false,
                                                bottom: false,
                                                child: new AnimatedOpacity(
                                                    opacity: showLargeTitle ? 1.0f : 0.0f,
                                                    duration: NavBarUtils._kNavBarTitleFadeDuration,
                                                    child: new DefaultTextStyle(
                                                        style: CupertinoTheme.of(context).textTheme
                                                            .navLargeTitleTextStyle,
                                                        maxLines: 1,
                                                        overflow: TextOverflow.ellipsis,
                                                        child: this.components.largeTitle
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            ),
                            new Positioned(
                                left: 0.0f,
                                right: 0.0f,
                                top: 0.0f,
                                child: persistentNavigationBar
                            )
                        }
                    )
                )
            );

            if (!this.transitionBetweenRoutes || !NavBarUtils._isTransitionable(context)) {
                return navBar;
            }

            return new Hero(
                tag: this.heroTag as _HeroTag == NavBarUtils._defaultHeroTag
                    ? new _HeroTag(Navigator.of(context))
                    : this.heroTag,
                createRectTween: NavBarUtils._linearTranslateWithLargestRectSizeTween,
                flightShuttleBuilder: NavBarUtils._navBarHeroFlightShuttleBuilder,
                placeholderBuilder: NavBarUtils._navBarHeroLaunchPadBuilder,
                transitionOnUserGestures: true,
                child: new _TransitionableNavigationBar(
                    componentsKeys: this.keys,
                    backgroundColor: this.backgroundColor,
                    backButtonTextStyle: CupertinoTheme.of(context).textTheme.navActionTextStyle,
                    titleTextStyle: CupertinoTheme.of(context).textTheme.navTitleTextStyle,
                    largeTitleTextStyle: CupertinoTheme.of(context).textTheme.navLargeTitleTextStyle,
                    border: this.border,
                    hasUserMiddle: this.userMiddle != null,
                    largeExpanded: showLargeTitle,
                    child: navBar
                )
            );
        }

        public override bool shouldRebuild(SliverPersistentHeaderDelegate _oldDelegate) {
            _LargeTitleNavigationBarSliverDelegate oldDelegate = _oldDelegate as _LargeTitleNavigationBarSliverDelegate;
            return this.components != oldDelegate.components
                   || this.userMiddle != oldDelegate.userMiddle
                   || this.backgroundColor != oldDelegate.backgroundColor
                   || this.border != oldDelegate.border
                   || this.padding != oldDelegate.padding
                   || this.actionsForegroundColor != oldDelegate.actionsForegroundColor
                   || this.transitionBetweenRoutes != oldDelegate.transitionBetweenRoutes
                   || this.persistentHeight != oldDelegate.persistentHeight
                   || this.alwaysShowMiddle != oldDelegate.alwaysShowMiddle
                   || this.heroTag != oldDelegate.heroTag;
        }
    }

    class _PersistentNavigationBar : StatelessWidget {
        public _PersistentNavigationBar(
            Key key = null,
            _NavigationBarStaticComponents components = null,
            EdgeInsets padding = null,
            bool? middleVisible = null
        ) : base(key: key) {
            this.components = components;
            this.padding = padding;
            this.middleVisible = middleVisible ?? true;
        }

        public readonly _NavigationBarStaticComponents components;

        public readonly EdgeInsets padding;
        public readonly bool middleVisible;

        public override Widget build(BuildContext context) {
            Widget middle = this.components.middle;

            if (middle != null) {
                middle = new DefaultTextStyle(
                    style: CupertinoTheme.of(context).textTheme.navTitleTextStyle,
                    child: middle
                );
                middle = new AnimatedOpacity(
                    opacity: this.middleVisible ? 1.0f : 0.0f,
                    duration: NavBarUtils._kNavBarTitleFadeDuration,
                    child: middle
                );
            }

            Widget leading = this.components.leading;
            Widget backChevron = this.components.backChevron;
            Widget backLabel = this.components.backLabel;

            if (leading == null && backChevron != null && backLabel != null) {
                leading = CupertinoNavigationBarBackButton._assemble(
                    backChevron,
                    backLabel
                );
            }

            Widget paddedToolbar = new NavigationToolbar(
                leading: leading,
                middle: middle,
                trailing: this.components.trailing,
                centerMiddle: true,
                middleSpacing: 6.0f
            );

            if (this.padding != null) {
                paddedToolbar = new Padding(
                    padding: EdgeInsets.only(
                        top: this.padding.top,
                        bottom: this.padding.bottom
                    ),
                    child: paddedToolbar
                );
            }

            return new SizedBox(
                height: NavBarUtils._kNavBarPersistentHeight + MediaQuery.of(context).padding.top,
                child: new SafeArea(
                    bottom: false,
                    child: paddedToolbar
                )
            );
        }
    }

    class _NavigationBarStaticComponentsKeys {
        public _NavigationBarStaticComponentsKeys() {
            this.navBarBoxKey = GlobalKey.key(debugLabel: "Navigation bar render box");
            this.leadingKey = GlobalKey.key(debugLabel: "Leading");
            this.backChevronKey = GlobalKey.key(debugLabel: "Back chevron");
            this.backLabelKey = GlobalKey.key(debugLabel: "Back label");
            this.middleKey = GlobalKey.key(debugLabel: "Middle");
            this.trailingKey = GlobalKey.key(debugLabel: "Trailing");
            this.largeTitleKey = GlobalKey.key(debugLabel: "Large title");
        }

        public readonly GlobalKey navBarBoxKey;
        public readonly GlobalKey leadingKey;
        public readonly GlobalKey backChevronKey;
        public readonly GlobalKey backLabelKey;
        public readonly GlobalKey middleKey;
        public readonly GlobalKey trailingKey;
        public readonly GlobalKey largeTitleKey;
    }

    class _NavigationBarStaticComponents {
        public _NavigationBarStaticComponents(
            _NavigationBarStaticComponentsKeys keys,
            ModalRoute route,
            Widget userLeading,
            bool automaticallyImplyLeading,
            bool automaticallyImplyTitle,
            string previousPageTitle,
            Widget userMiddle,
            Widget userTrailing,
            Widget userLargeTitle,
            EdgeInsets padding,
            bool large
        ) {
            this.leading = createLeading(
                leadingKey: keys.leadingKey,
                userLeading: userLeading,
                route: route,
                automaticallyImplyLeading: automaticallyImplyLeading,
                padding: padding
            );
            this.backChevron = createBackChevron(
                backChevronKey: keys.backChevronKey,
                userLeading: userLeading,
                route: route,
                automaticallyImplyLeading: automaticallyImplyLeading
            );
            this.backLabel = createBackLabel(
                backLabelKey: keys.backLabelKey,
                userLeading: userLeading,
                route: route,
                previousPageTitle: previousPageTitle,
                automaticallyImplyLeading: automaticallyImplyLeading
            );
            this.middle = createMiddle(
                middleKey: keys.middleKey,
                userMiddle: userMiddle,
                userLargeTitle: userLargeTitle,
                route: route,
                automaticallyImplyTitle: automaticallyImplyTitle,
                large: large
            );
            this.trailing = createTrailing(
                trailingKey: keys.trailingKey,
                userTrailing: userTrailing,
                padding: padding
            );
            this.largeTitle = createLargeTitle(
                largeTitleKey: keys.largeTitleKey,
                userLargeTitle: userLargeTitle,
                route: route,
                automaticImplyTitle: automaticallyImplyTitle,
                large: large
            );
        }

        static Widget _derivedTitle(
            bool automaticallyImplyTitle,
            ModalRoute currentRoute
        ) {
            if (automaticallyImplyTitle &&
                currentRoute is CupertinoPageRoute route &&
                route.title != null) {
                return new Text(route.title);
            }

            return null;
        }

        public readonly KeyedSubtree leading;

        static KeyedSubtree createLeading(
            GlobalKey leadingKey,
            Widget userLeading,
            ModalRoute route,
            bool automaticallyImplyLeading,
            EdgeInsets padding
        ) {
            Widget leadingContent = null;

            if (userLeading != null) {
                leadingContent = userLeading;
            }
            else if (
                automaticallyImplyLeading &&
                route is PageRoute pageRoute &&
                route.canPop &&
                pageRoute.fullscreenDialog
            ) {
                leadingContent = new CupertinoButton(
                    child: new Text("Close"),
                    padding: EdgeInsets.zero,
                    onPressed: () => { route.navigator.maybePop(); }
                );
            }

            if (leadingContent == null) {
                return null;
            }

            return new KeyedSubtree(
                key: leadingKey,
                child: new Padding(
                    padding: EdgeInsets.only(
                        left: padding?.left ?? NavBarUtils._kNavBarEdgePadding
                    ),
                    child: IconTheme.merge(
                        data: new IconThemeData(
                            size: 32.0f
                        ),
                        child: leadingContent
                    )
                )
            );
        }

        public readonly KeyedSubtree backChevron;

        static KeyedSubtree createBackChevron(
            GlobalKey backChevronKey,
            Widget userLeading,
            ModalRoute route,
            bool automaticallyImplyLeading
        ) {
            if (
                userLeading != null ||
                !automaticallyImplyLeading ||
                route == null ||
                !route.canPop ||
                (route is PageRoute pageRoute && pageRoute.fullscreenDialog)
            ) {
                return null;
            }

            return new KeyedSubtree(key: backChevronKey, child: new _BackChevron());
        }

        public readonly KeyedSubtree backLabel;

        static KeyedSubtree createBackLabel(
            GlobalKey backLabelKey,
            Widget userLeading,
            ModalRoute route,
            bool automaticallyImplyLeading,
            string previousPageTitle
        ) {
            if (
                userLeading != null ||
                !automaticallyImplyLeading ||
                route == null ||
                !route.canPop ||
                (route is PageRoute pageRoute && pageRoute.fullscreenDialog)
            ) {
                return null;
            }

            return new KeyedSubtree(
                key: backLabelKey,
                child: new _BackLabel(
                    specifiedPreviousTitle: previousPageTitle,
                    route: route
                )
            );
        }

        public readonly KeyedSubtree middle;

        static KeyedSubtree createMiddle(
            GlobalKey middleKey,
            Widget userMiddle,
            Widget userLargeTitle,
            bool large,
            bool automaticallyImplyTitle,
            ModalRoute route
        ) {
            Widget middleContent = userMiddle;

            if (large) {
                middleContent = middleContent ?? userLargeTitle;
            }

            middleContent = middleContent ?? _derivedTitle(
                                automaticallyImplyTitle: automaticallyImplyTitle,
                                currentRoute: route
                            );

            if (middleContent == null) {
                return null;
            }

            return new KeyedSubtree(
                key: middleKey,
                child: middleContent
            );
        }

        public readonly KeyedSubtree trailing;

        static KeyedSubtree createTrailing(
            GlobalKey trailingKey,
            Widget userTrailing,
            EdgeInsets padding
        ) {
            if (userTrailing == null) {
                return null;
            }

            return new KeyedSubtree(
                key: trailingKey,
                child: new Padding(
                    padding: EdgeInsets.only(
                        right: padding?.right ?? NavBarUtils._kNavBarEdgePadding
                    ),
                    child: IconTheme.merge(
                        data: new IconThemeData(
                            size: 32.0f
                        ),
                        child: userTrailing
                    )
                )
            );
        }

        public readonly KeyedSubtree largeTitle;

        static KeyedSubtree createLargeTitle(
            GlobalKey largeTitleKey,
            Widget userLargeTitle,
            bool large,
            bool automaticImplyTitle,
            ModalRoute route
        ) {
            if (!large) {
                return null;
            }

            Widget largeTitleContent = userLargeTitle ?? _derivedTitle(
                                           automaticallyImplyTitle: automaticImplyTitle,
                                           currentRoute: route
                                       );

            D.assert(
                largeTitleContent != null,
                () => "largeTitle was not provided and there was no title from the route."
            );

            return new KeyedSubtree(
                key: largeTitleKey,
                child: largeTitleContent
            );
        }
    }

    public class CupertinoNavigationBarBackButton : StatelessWidget {
        public CupertinoNavigationBarBackButton(
            Color color,
            string previousPageTitle
        ) {
            this._backChevron = null;
            this._backLabel = null;
            this.color = color;
            this.previousPageTitle = previousPageTitle;
        }

        internal CupertinoNavigationBarBackButton(
            Color color,
            string previousPageTitle,
            Widget backChevron,
            Widget backLabel
        ) {
            this._backChevron = backChevron;
            this._backLabel = backLabel;
            this.color = color;
            this.previousPageTitle = previousPageTitle;
        }

        public static CupertinoNavigationBarBackButton _assemble(
            Widget _backChevron,
            Widget _backLabel
        ) {
            return new CupertinoNavigationBarBackButton(
                backChevron: _backChevron,
                backLabel: _backLabel,
                color: null,
                previousPageTitle: null
            );
        }

        public readonly Color color;

        public readonly string previousPageTitle;

        public readonly Widget _backChevron;

        public readonly Widget _backLabel;

        public override Widget build(BuildContext context) {
            ModalRoute currentRoute = ModalRoute.of(context);
            D.assert(
                currentRoute?.canPop == true,
                () => "CupertinoNavigationBarBackButton should only be used in routes that can be popped"
            );

            TextStyle actionTextStyle = CupertinoTheme.of(context).textTheme.navActionTextStyle;
            if (this.color != null) {
                actionTextStyle = actionTextStyle.copyWith(color: this.color);
            }

            return new CupertinoButton(
                child: new DefaultTextStyle(
                    style: actionTextStyle,
                    child: new ConstrainedBox(
                        constraints: new BoxConstraints(minWidth: NavBarUtils._kNavBarBackButtonTapWidth),
                        child: new Row(
                            mainAxisSize: MainAxisSize.min,
                            mainAxisAlignment: MainAxisAlignment.start,
                            children: new List<Widget> {
                                new Padding(padding: EdgeInsets.only(left: 8.0f)),
                                this._backChevron ?? new _BackChevron(),
                                new Padding(padding: EdgeInsets.only(left: 6.0f)),
                                new Flexible(
                                    child: this._backLabel ?? new _BackLabel(
                                               specifiedPreviousTitle: this.previousPageTitle,
                                               route: currentRoute
                                           )
                                )
                            }
                        )
                    )
                ),
                padding: EdgeInsets.zero,
                onPressed: () => { Navigator.maybePop(context); }
            );
        }
    }


    class _BackChevron : StatelessWidget {
        public _BackChevron(Key key = null) : base(key: key) { }

        public override Widget build(BuildContext context) {
            TextStyle textStyle = DefaultTextStyle.of(context).style;

            Widget iconWidget = Text.rich(
                new TextSpan(
                    text: char.ConvertFromUtf32(CupertinoIcons.back.codePoint),
                    style: new TextStyle(
                        inherit: false,
                        color: textStyle.color,
                        fontSize: 34.0f,
                        fontFamily: CupertinoIcons.back.fontFamily
                    )
                )
            );

            return iconWidget;
        }
    }

    class _BackLabel : StatelessWidget {
        public _BackLabel(
            Key key = null,
            string specifiedPreviousTitle = null,
            ModalRoute route = null
        ) : base(key: key) {
            D.assert(route != null);
            this.specifiedPreviousTitle = specifiedPreviousTitle;
            this.route = route;
        }

        public readonly string specifiedPreviousTitle;
        public readonly ModalRoute route;

        Widget _buildPreviousTitleWidget(BuildContext context, string previousTitle, Widget child) {
            if (previousTitle == null) {
                return new SizedBox(height: 0.0f, width: 0.0f);
            }

            Text textWidget = new Text(
                previousTitle,
                maxLines: 1,
                overflow: TextOverflow.ellipsis
            );

            if (previousTitle.Length > 12) {
                textWidget = new Text("Back");
            }

            return new Align(
                alignment: Alignment.centerLeft,
                widthFactor: 1.0f,
                child: textWidget
            );
        }

        public override Widget build(BuildContext context) {
            if (this.specifiedPreviousTitle != null) {
                return this._buildPreviousTitleWidget(context, this.specifiedPreviousTitle, null);
            }
            else if (this.route is CupertinoPageRoute cupertinoRoute) {
                return new ValueListenableBuilder<string>(
                    valueListenable: cupertinoRoute.previousTitle,
                    builder: this._buildPreviousTitleWidget
                );
            }
            else {
                return new SizedBox(height: 0.0f, width: 0.0f);
            }
        }
    }

    class _TransitionableNavigationBar : StatelessWidget {
        public _TransitionableNavigationBar(
            _NavigationBarStaticComponentsKeys componentsKeys = null,
            Color backgroundColor = null,
            TextStyle backButtonTextStyle = null,
            TextStyle titleTextStyle = null,
            TextStyle largeTitleTextStyle = null,
            Border border = null,
            bool? hasUserMiddle = null,
            bool? largeExpanded = null,
            Widget child = null
        ) : base(key: componentsKeys.navBarBoxKey) {
            D.assert(largeExpanded != null);
            D.assert(!largeExpanded.Value || largeTitleTextStyle != null);

            this.componentsKeys = componentsKeys;
            this.backgroundColor = backgroundColor;
            this.backButtonTextStyle = backButtonTextStyle;
            this.titleTextStyle = titleTextStyle;
            this.largeTitleTextStyle = largeTitleTextStyle;
            this.border = border;
            this.hasUserMiddle = hasUserMiddle;
            this.largeExpanded = largeExpanded;
            this.child = child;
        }

        public readonly _NavigationBarStaticComponentsKeys componentsKeys;
        public readonly Color backgroundColor;
        public readonly TextStyle backButtonTextStyle;
        public readonly TextStyle titleTextStyle;
        public readonly TextStyle largeTitleTextStyle;
        public readonly Border border;
        public readonly bool? hasUserMiddle;
        public readonly bool? largeExpanded;
        public readonly Widget child;

        public RenderBox renderBox {
            get {
                RenderBox box = (RenderBox) this.componentsKeys.navBarBoxKey.currentContext.findRenderObject();
                D.assert(
                    box.attached,
                    () => "_TransitionableNavigationBar.renderBox should be called when building " +
                          "hero flight shuttles when the from and the to nav bar boxes are already " +
                          "laid out and painted."
                );
                return box;
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(() => {
                bool? inHero = null;
                context.visitAncestorElements((Element ancestor) => {
                    if (ancestor is ComponentElement) {
                        D.assert(
                            ancestor.widget.GetType() != typeof(_NavigationBarTransition),
                            () => "_TransitionableNavigationBar should never re-appear inside " +
                                  "_NavigationBarTransition. Keyed _TransitionableNavigationBar should " +
                                  "only serve as anchor points in routes rather than appearing inside " +
                                  "Hero flights themselves."
                        );
                        if (ancestor.widget.GetType() == typeof(Hero)) {
                            inHero = true;
                        }
                    }

                    inHero = inHero ?? false;
                    return true;
                });
                D.assert(
                    inHero == true,
                    () => "_TransitionableNavigationBar should only be added as the immediate " +
                          "child of Hero widgets."
                );
                return true;
            });
            return this.child;
        }
    }

    class _NavigationBarTransition : StatelessWidget {
        public _NavigationBarTransition(
            Animation<float> animation,
            _TransitionableNavigationBar topNavBar,
            _TransitionableNavigationBar bottomNavBar
        ) {
            this.animation = animation;
            this.topNavBar = topNavBar;
            this.bottomNavBar = bottomNavBar;
            this.heightTween = new FloatTween(
                begin: this.bottomNavBar.renderBox.size.height,
                end: this.topNavBar.renderBox.size.height
            );
            this.backgroundTween = new ColorTween(
                begin: this.bottomNavBar.backgroundColor,
                end: this.topNavBar.backgroundColor
            );
            this.borderTween = new BorderTween(
                begin: this.bottomNavBar.border,
                end: this.topNavBar.border
            );
        }

        public readonly Animation<float> animation;
        public readonly _TransitionableNavigationBar topNavBar;
        public readonly _TransitionableNavigationBar bottomNavBar;

        public readonly FloatTween heightTween;
        public readonly ColorTween backgroundTween;
        public readonly BorderTween borderTween;

        public override Widget build(BuildContext context) {
            _NavigationBarComponentsTransition componentsTransition = new _NavigationBarComponentsTransition(
                animation: this.animation,
                bottomNavBar: this.bottomNavBar,
                topNavBar: this.topNavBar,
                directionality: Directionality.of(context)
            );

            List<Widget> children = new List<Widget> {
                new AnimatedBuilder(
                    animation: this.animation,
                    builder: (BuildContext _context, Widget child) => {
                        return NavBarUtils._wrapWithBackground(
                            updateSystemUiOverlay: false,
                            backgroundColor: this.backgroundTween.evaluate(this.animation),
                            border: this.borderTween.evaluate(this.animation),
                            child: new SizedBox(
                                height: this.heightTween.evaluate(this.animation),
                                width: float.PositiveInfinity
                            )
                        );
                    }
                ),
                componentsTransition.bottomBackChevron,
                componentsTransition.bottomBackLabel,
                componentsTransition.bottomLeading,
                componentsTransition.bottomMiddle,
                componentsTransition.bottomLargeTitle,
                componentsTransition.bottomTrailing,
                componentsTransition.topLeading,
                componentsTransition.topBackChevron,
                componentsTransition.topBackLabel,
                componentsTransition.topMiddle,
                componentsTransition.topLargeTitle,
                componentsTransition.topTrailing
            };

            children.RemoveAll((Widget child) => child == null);

            return new SizedBox(
                height: Mathf.Max(this.heightTween.begin, this.heightTween.end) + MediaQuery.of(context).padding.top,
                width: float.PositiveInfinity,
                child: new Stack(
                    children: children
                )
            );
        }
    }

    class _NavigationBarComponentsTransition {
        public _NavigationBarComponentsTransition(
            Animation<float> animation,
            _TransitionableNavigationBar bottomNavBar,
            _TransitionableNavigationBar topNavBar,
            TextDirection directionality
        ) {
            this.animation = animation;
            this.bottomComponents = bottomNavBar.componentsKeys;
            this.topComponents = topNavBar.componentsKeys;
            this.bottomNavBarBox = bottomNavBar.renderBox;
            this.topNavBarBox = topNavBar.renderBox;
            this.bottomBackButtonTextStyle = bottomNavBar.backButtonTextStyle;
            this.topBackButtonTextStyle = topNavBar.backButtonTextStyle;
            this.bottomTitleTextStyle = bottomNavBar.titleTextStyle;
            this.topTitleTextStyle = topNavBar.titleTextStyle;
            this.bottomLargeTitleTextStyle = bottomNavBar.largeTitleTextStyle;
            this.topLargeTitleTextStyle = topNavBar.largeTitleTextStyle;
            this.bottomHasUserMiddle = bottomNavBar.hasUserMiddle;
            this.topHasUserMiddle = topNavBar.hasUserMiddle;
            this.bottomLargeExpanded = bottomNavBar.largeExpanded;
            this.topLargeExpanded = topNavBar.largeExpanded;
            this.transitionBox =
                bottomNavBar.renderBox.paintBounds.expandToInclude(topNavBar.renderBox.paintBounds);
            this.forwardDirection = directionality == TextDirection.ltr ? 1.0f : -1.0f;
        }

        public static Animatable<float> fadeOut = new FloatTween(
            begin: 1.0f,
            end: 0.0f
        );

        public static Animatable<float> fadeIn = new FloatTween(
            begin: 0.0f,
            end: 1.0f
        );

        public readonly Animation<float> animation;
        public readonly _NavigationBarStaticComponentsKeys bottomComponents;
        public readonly _NavigationBarStaticComponentsKeys topComponents;

        public readonly RenderBox bottomNavBarBox;
        public readonly RenderBox topNavBarBox;

        public readonly TextStyle bottomBackButtonTextStyle;
        public readonly TextStyle topBackButtonTextStyle;
        public readonly TextStyle bottomTitleTextStyle;
        public readonly TextStyle topTitleTextStyle;
        public readonly TextStyle bottomLargeTitleTextStyle;
        public readonly TextStyle topLargeTitleTextStyle;

        public readonly bool? bottomHasUserMiddle;
        public readonly bool? topHasUserMiddle;
        public readonly bool? bottomLargeExpanded;
        public readonly bool? topLargeExpanded;

        public readonly Rect transitionBox;

        public readonly float forwardDirection;

        public RelativeRect positionInTransitionBox(
            GlobalKey key,
            RenderBox from
        ) {
            RenderBox componentBox = (RenderBox) key.currentContext.findRenderObject();
            D.assert(componentBox.attached);

            return RelativeRect.fromRect(
                componentBox.localToGlobal(Offset.zero, ancestor: from) & componentBox.size, this.transitionBox
            );
        }

        public RelativeRectTween slideFromLeadingEdge(
            GlobalKey fromKey,
            RenderBox fromNavBarBox,
            GlobalKey toKey,
            RenderBox toNavBarBox
        ) {
            RelativeRect fromRect = this.positionInTransitionBox(fromKey, from: fromNavBarBox);

            RenderBox fromBox = (RenderBox) fromKey.currentContext.findRenderObject();
            RenderBox toBox = (RenderBox) toKey.currentContext.findRenderObject();

            Rect toRect =
                toBox.localToGlobal(
                    Offset.zero,
                    ancestor: toNavBarBox
                ).translate(
                    0.0f,
                    -fromBox.size.height / 2 + toBox.size.height / 2
                ) & fromBox.size; // Keep the from render object"s size.

            if (this.forwardDirection < 0) {
                toRect = toRect.translate(-fromBox.size.width + toBox.size.width, 0.0f);
            }

            return new RelativeRectTween(
                begin: fromRect,
                end: RelativeRect.fromRect(toRect, this.transitionBox)
            );
        }

        public Animation<float> fadeInFrom(float t, Curve curve = null) {
            return this.animation.drive(fadeIn.chain(
                new CurveTween(curve: new Interval(t, 1.0f, curve: curve ?? Curves.easeIn))
            ));
        }

        public Animation<float> fadeOutBy(float t, Curve curve = null) {
            return this.animation.drive(fadeOut.chain(
                new CurveTween(curve: new Interval(0.0f, t, curve: curve ?? Curves.easeOut))
            ));
        }

        public Widget bottomLeading {
            get {
                KeyedSubtree bottomLeading = (KeyedSubtree) this.bottomComponents.leadingKey.currentWidget;

                if (bottomLeading == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: this.positionInTransitionBox(this.bottomComponents.leadingKey, from: this.bottomNavBarBox),
                    child: new FadeTransition(
                        opacity: this.fadeOutBy(0.4f),
                        child: bottomLeading.child
                    )
                );
            }
        }

        public Widget bottomBackChevron {
            get {
                KeyedSubtree bottomBackChevron = (KeyedSubtree) this.bottomComponents.backChevronKey.currentWidget;

                if (bottomBackChevron == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: this.positionInTransitionBox(this.bottomComponents.backChevronKey,
                        from: this.bottomNavBarBox),
                    child: new FadeTransition(
                        opacity: this.fadeOutBy(0.6f),
                        child: new DefaultTextStyle(
                            style: this.bottomBackButtonTextStyle,
                            child: bottomBackChevron.child
                        )
                    )
                );
            }
        }

        public Widget bottomBackLabel {
            get {
                KeyedSubtree bottomBackLabel = (KeyedSubtree) this.bottomComponents.backLabelKey.currentWidget;

                if (bottomBackLabel == null) {
                    return null;
                }

                RelativeRect from =
                    this.positionInTransitionBox(this.bottomComponents.backLabelKey, from: this.bottomNavBarBox);

                RelativeRectTween positionTween = new RelativeRectTween(
                    begin: from,
                    end: from.shift(
                        new Offset(this.forwardDirection * (-this.bottomNavBarBox.size.width / 2.0f),
                            0.0f
                        )
                    )
                );

                return new PositionedTransition(
                    rect: this.animation.drive(positionTween),
                    child: new FadeTransition(
                        opacity: this.fadeOutBy(0.2f),
                        child: new DefaultTextStyle(
                            style: this.bottomBackButtonTextStyle,
                            child: bottomBackLabel.child
                        )
                    )
                );
            }
        }

        public Widget bottomMiddle {
            get {
                KeyedSubtree bottomMiddle = (KeyedSubtree) this.bottomComponents.middleKey.currentWidget;
                KeyedSubtree topBackLabel = (KeyedSubtree) this.topComponents.backLabelKey.currentWidget;
                KeyedSubtree topLeading = (KeyedSubtree) this.topComponents.leadingKey.currentWidget;

                if (this.bottomHasUserMiddle != true && this.bottomLargeExpanded == true) {
                    return null;
                }

                if (bottomMiddle != null && topBackLabel != null) {
                    return new PositionedTransition(
                        rect: this.animation.drive(this.slideFromLeadingEdge(
                            fromKey: this.bottomComponents.middleKey,
                            fromNavBarBox: this.bottomNavBarBox,
                            toKey: this.topComponents.backLabelKey,
                            toNavBarBox: this.topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: this.fadeOutBy(this.bottomHasUserMiddle == true ? 0.4f : 0.7f),
                            child: new Align(
                                alignment: Alignment.centerLeft,
                                child: new DefaultTextStyleTransition(
                                    style: this.animation.drive(new TextStyleTween(
                                        begin: this.bottomTitleTextStyle,
                                        end: this.topBackButtonTextStyle
                                    )),
                                    child: bottomMiddle.child
                                )
                            )
                        )
                    );
                }

                if (bottomMiddle != null && topLeading != null) {
                    return Positioned.fromRelativeRect(
                        rect: this.positionInTransitionBox(this.bottomComponents.middleKey, from: this.bottomNavBarBox),
                        child: new FadeTransition(
                            opacity: this.fadeOutBy(this.bottomHasUserMiddle == true ? 0.4f : 0.7f),
                            child: new DefaultTextStyle(
                                style: this.bottomTitleTextStyle,
                                child: bottomMiddle.child
                            )
                        )
                    );
                }

                return null;
            }
        }

        public Widget bottomLargeTitle {
            get {
                KeyedSubtree bottomLargeTitle = (KeyedSubtree) this.bottomComponents.largeTitleKey.currentWidget;
                KeyedSubtree topBackLabel = (KeyedSubtree) this.topComponents.backLabelKey.currentWidget;
                KeyedSubtree topLeading = (KeyedSubtree) this.topComponents.leadingKey.currentWidget;

                if (bottomLargeTitle == null || this.bottomLargeExpanded != true) {
                    return null;
                }

                if (bottomLargeTitle != null && topBackLabel != null) {
                    return new PositionedTransition(
                        rect: this.animation.drive(this.slideFromLeadingEdge(
                            fromKey: this.bottomComponents.largeTitleKey,
                            fromNavBarBox: this.bottomNavBarBox,
                            toKey: this.topComponents.backLabelKey,
                            toNavBarBox: this.topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: this.fadeOutBy(0.6f),
                            child: new Align(
                                alignment: Alignment.centerLeft,
                                child: new DefaultTextStyleTransition(
                                    style: this.animation.drive(new TextStyleTween(
                                        begin: this.bottomLargeTitleTextStyle,
                                        end: this.topBackButtonTextStyle
                                    )),
                                    maxLines: 1,
                                    overflow: TextOverflow.ellipsis,
                                    child: bottomLargeTitle.child
                                )
                            )
                        )
                    );
                }

                if (bottomLargeTitle != null && topLeading != null) {
                    RelativeRect from = this.positionInTransitionBox(this.bottomComponents.largeTitleKey,
                        from: this.bottomNavBarBox);

                    RelativeRectTween positionTween = new RelativeRectTween(
                        begin: from,
                        end: from.shift(
                            new Offset(this.forwardDirection * this.bottomNavBarBox.size.width / 4.0f,
                                0.0f
                            )
                        )
                    );

                    return new PositionedTransition(
                        rect: this.animation.drive(positionTween),
                        child: new FadeTransition(
                            opacity: this.fadeOutBy(0.4f),
                            child: new DefaultTextStyle(
                                style: this.bottomLargeTitleTextStyle,
                                child: bottomLargeTitle.child
                            )
                        )
                    );
                }

                return null;
            }
        }

        public Widget bottomTrailing {
            get {
                KeyedSubtree bottomTrailing = (KeyedSubtree) this.bottomComponents.trailingKey.currentWidget;

                if (bottomTrailing == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: this.positionInTransitionBox(this.bottomComponents.trailingKey, from: this.bottomNavBarBox),
                    child: new FadeTransition(
                        opacity: this.fadeOutBy(0.6f),
                        child: bottomTrailing.child
                    )
                );
            }
        }

        public Widget topLeading {
            get {
                KeyedSubtree topLeading = (KeyedSubtree) this.topComponents.leadingKey.currentWidget;

                if (topLeading == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: this.positionInTransitionBox(this.topComponents.leadingKey, from: this.topNavBarBox),
                    child: new FadeTransition(
                        opacity: this.fadeInFrom(0.6f),
                        child: topLeading.child
                    )
                );
            }
        }

        public Widget topBackChevron {
            get {
                KeyedSubtree topBackChevron = (KeyedSubtree) this.topComponents.backChevronKey.currentWidget;
                KeyedSubtree bottomBackChevron = (KeyedSubtree) this.bottomComponents.backChevronKey.currentWidget;

                if (topBackChevron == null) {
                    return null;
                }

                RelativeRect to =
                    this.positionInTransitionBox(this.topComponents.backChevronKey, from: this.topNavBarBox);
                RelativeRect from = to;

                if (bottomBackChevron == null) {
                    RenderBox topBackChevronBox =
                        (RenderBox) this.topComponents.backChevronKey.currentContext.findRenderObject();
                    from = to.shift(
                        new Offset(this.forwardDirection * topBackChevronBox.size.width * 2.0f,
                            0.0f
                        )
                    );
                }

                RelativeRectTween positionTween = new RelativeRectTween(
                    begin: from,
                    end: to
                );

                return new PositionedTransition(
                    rect: this.animation.drive(positionTween),
                    child: new FadeTransition(
                        opacity: this.fadeInFrom(bottomBackChevron == null ? 0.7f : 0.4f),
                        child: new DefaultTextStyle(
                            style: this.topBackButtonTextStyle,
                            child: topBackChevron.child
                        )
                    )
                );
            }
        }

        public Widget topBackLabel {
            get {
                KeyedSubtree bottomMiddle = (KeyedSubtree) this.bottomComponents.middleKey.currentWidget;
                KeyedSubtree bottomLargeTitle = (KeyedSubtree) this.bottomComponents.largeTitleKey.currentWidget;
                KeyedSubtree topBackLabel = (KeyedSubtree) this.topComponents.backLabelKey.currentWidget;

                if (topBackLabel == null) {
                    return null;
                }

                RenderAnimatedOpacity topBackLabelOpacity =
                    (RenderAnimatedOpacity) this.topComponents.backLabelKey.currentContext?.ancestorRenderObjectOfType(
                        new TypeMatcher<RenderAnimatedOpacity>()
                    );

                Animation<float> midClickOpacity = null;
                if (topBackLabelOpacity != null && topBackLabelOpacity.opacity.value < 1.0f) {
                    midClickOpacity = this.animation.drive(new FloatTween(
                        begin: 0.0f,
                        end: topBackLabelOpacity.opacity.value
                    ));
                }

                if (bottomLargeTitle != null &&
                    topBackLabel != null && this.bottomLargeExpanded.Value) {
                    return new PositionedTransition(
                        rect: this.animation.drive(this.slideFromLeadingEdge(
                            fromKey: this.bottomComponents.largeTitleKey,
                            fromNavBarBox: this.bottomNavBarBox,
                            toKey: this.topComponents.backLabelKey,
                            toNavBarBox: this.topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: midClickOpacity ?? this.fadeInFrom(0.4f),
                            child: new DefaultTextStyleTransition(
                                style: this.animation.drive(new TextStyleTween(
                                    begin: this.bottomLargeTitleTextStyle,
                                    end: this.topBackButtonTextStyle
                                )),
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                                child: topBackLabel.child
                            )
                        )
                    );
                }

                if (bottomMiddle != null && topBackLabel != null) {
                    return new PositionedTransition(
                        rect: this.animation.drive(this.slideFromLeadingEdge(
                            fromKey: this.bottomComponents.middleKey,
                            fromNavBarBox: this.bottomNavBarBox,
                            toKey: this.topComponents.backLabelKey,
                            toNavBarBox: this.topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: midClickOpacity ?? this.fadeInFrom(0.3f),
                            child: new DefaultTextStyleTransition(
                                style: this.animation.drive(new TextStyleTween(
                                    begin: this.bottomTitleTextStyle,
                                    end: this.topBackButtonTextStyle
                                )),
                                child: topBackLabel.child
                            )
                        )
                    );
                }

                return null;
            }
        }

        public Widget topMiddle {
            get {
                KeyedSubtree topMiddle = (KeyedSubtree) this.topComponents.middleKey.currentWidget;

                if (topMiddle == null) {
                    return null;
                }

                if (this.topHasUserMiddle != true && this.topLargeExpanded == true) {
                    return null;
                }

                RelativeRect to = this.positionInTransitionBox(this.topComponents.middleKey, from: this.topNavBarBox);

                RelativeRectTween positionTween = new RelativeRectTween(
                    begin: to.shift(
                        new Offset(this.forwardDirection * this.topNavBarBox.size.width / 2.0f,
                            0.0f
                        )
                    ),
                    end: to
                );

                return new PositionedTransition(
                    rect: this.animation.drive(positionTween),
                    child: new FadeTransition(
                        opacity: this.fadeInFrom(0.25f),
                        child: new DefaultTextStyle(
                            style: this.topTitleTextStyle,
                            child: topMiddle.child
                        )
                    )
                );
            }
        }

        public Widget topTrailing {
            get {
                KeyedSubtree topTrailing = (KeyedSubtree) this.topComponents.trailingKey.currentWidget;

                if (topTrailing == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: this.positionInTransitionBox(this.topComponents.trailingKey, from: this.topNavBarBox),
                    child: new FadeTransition(
                        opacity: this.fadeInFrom(0.4f),
                        child: topTrailing.child
                    )
                );
            }
        }

        public Widget topLargeTitle {
            get {
                KeyedSubtree topLargeTitle = (KeyedSubtree) this.topComponents.largeTitleKey.currentWidget;

                if (topLargeTitle == null || this.topLargeExpanded != true) {
                    return null;
                }

                RelativeRect to =
                    this.positionInTransitionBox(this.topComponents.largeTitleKey, from: this.topNavBarBox);

                RelativeRectTween positionTween = new RelativeRectTween(
                    begin: to.shift(
                        new Offset(this.forwardDirection * this.topNavBarBox.size.width,
                            0.0f
                        )
                    ),
                    end: to
                );

                return new PositionedTransition(
                    rect: this.animation.drive(positionTween),
                    child: new FadeTransition(
                        opacity: this.fadeInFrom(0.3f),
                        child: new DefaultTextStyle(
                            style: this.topLargeTitleTextStyle,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            child: topLargeTitle.child
                        )
                    )
                );
            }
        }
    }
}