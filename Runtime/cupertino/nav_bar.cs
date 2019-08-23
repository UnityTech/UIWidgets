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
using Transform = Unity.UIWidgets.widgets.Transform;

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
            Border border,
            Color backgroundColor,
            Widget child,
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
    }


    class _HeroTag {
        public _HeroTag(NavigatorState navigator) {
            this.navigator = navigator;
        }

        public readonly NavigatorState navigator;

        public override string ToString() {
            return "Default Hero tag for Cupertino navigation bars with navigator $navigator";
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
            D.assert(
                heroTag != null,
                () => "heroTag cannot be null. Use transitionBetweenRoutes = false to " +
                "disable Hero transition on this navigation bar."
            );
            D.assert(
                !transitionBetweenRoutes || ReferenceEquals(heroTag, NavBarUtils._defaultHeroTag),
                () => "Cannot specify a heroTag override if this navigation bar does not " +
                "transition due to transitionBetweenRoutes = false."
            );
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

        public override bool fullObstruction {
            get { return this.backgroundColor == null ? null : this.backgroundColor.alpha == 0xFF; }
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

            if (!this.widget.transitionBetweenRoutes || !_isTransitionable(context)) {
                return _wrapActiveColor(this.widget.actionsForegroundColor, context,
                    navBar); // ignore: deprecated_member_use_from_same_package
            }

            return _wrapActiveColor(
                this.widget.actionsForegroundColor, // ignore: deprecated_member_use_from_same_package
                context,
                Builder(
                    builder: (BuildContext context) => {
                        return new Hero(
                            tag: this.widget.heroTag == NavBarUtils._defaultHeroTag
                                ? _HeroTag(Navigator.of(context))
                                : this.widget.heroTag,
                            createRectTween: _linearTranslateWithLargestRectSizeTween,
                            placeholderBuilder: _navBarHeroLaunchPadBuilder,
                            flightShuttleBuilder: _navBarHeroFlightShuttleBuilder,
                            transitionOnUserGestures: true,
                            child: new _TransitionableNavigationBar(
                                componentsKeys: this.keys,
                                backgroundColor: backgroundColor,
                                backButtonTextStyle: CupertinoTheme.of(context).textTheme.navActionTextStyle,
                                titleTextStyle: CupertinoTheme.of(context).textTheme.navTitleTextStyle,
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
            Key key,
            Widget largeTitle,
            Widget leading,
            bool automaticallyImplyLeading = true,
            bool automaticallyImplyTitle = true,
            string previousPageTitle,
            Widget middle,
            Widget trailing,
            Border border = NavBarUtils._kDefaultNavBarBorder,
            Color backgroundColor,
            EdgeInsets padding,
            Color actionsForegroundColor,
            bool transitionBetweenRoutes = true,
            object heroTag = NavBarUtils._defaultHeroTag
        ) : D.assert(automaticallyImplyLeading != null),
        D.assert(automaticallyImplyTitle != null);
        D.assert(
        automaticallyImplyTitle == true || largeTitle != null,
        "No largeTitle has been provided but automaticallyImplyTitle is also " +
        "false. Either provide a largeTitle or set automaticallyImplyTitle to " +
        "true."
        ),
        base(key: key);

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

        @Deprecated("Use CupertinoTheme and primaryColor to propagate color")
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

            return _wrapActiveColor(
                this.widget.actionsForegroundColor, // ignore: deprecated_member_use_from_same_package
                context,
                SliverPersistentHeader(
                    pinned: true, // iOS navigation bars are always pinned.
                    delegate: new _LargeTitleNavigationBarSliverDelegate(
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
        : SliverPersistentHeaderDelegate with DiagnosticableTreeMixin {
        _LargeTitleNavigationBarSliverDelegate( {
            required this.keys,
            required this.components,
            required this.userMiddle,
            required Color backgroundColor,
                required this.border,
            required this.padding,
            required this.actionsForegroundColor,
            this.transitionBetweenRoutes,
            required this.heroTag,
            this.persistentHeight,
            this.alwaysShowMiddle
                ) {
                D.assert(this.persistentHeight != null);
                D.assert(this.alwaysShowMiddle != null);
                D.assert(this.transitionBetweenRoutes != null);
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

        public override float minExtent {
            get { return this.persistentHeight; }
        }

        public override float maxExtent {
            get { return this.persistentHeight + NavBarUtils._kNavBarLargeTitleHeightExtension; }
        }

        public override Widget build(BuildContext context, float shrinkOffset, bool overlapsContent) {
            bool showLargeTitle = shrinkOffset < this.maxExtent - this.minExtent - NavBarUtils._kNavBarShowLargeTitleThreshold;

            _PersistentNavigationBar persistentNavigationBar =
                _PersistentNavigationBar(
                    components: this.components,
                    padding: this.padding,
                    middleVisible: this.alwaysShowMiddle ? null : !showLargeTitle
                );

            Widget navBar = NavBarUtils._wrapWithBackground(
                border: this.border,
                backgroundColor: this.backgroundColor,
                child: new DefaultTextStyle(
                    style: CupertinoTheme.of(context).textTheme.textStyle,
                    child: new Stack(
                        fit: StackFit.expand,
                        children: new List<Widget> {
                            Positioned(
                                top: this.persistentHeight,
                                left: 0.0f,
                                right: 0.0f,
                                bottom: 0.0f,
                                child: new ClipRect(
                                    child: new OverflowBox(
                                        minHeight: 0.0f,
                                        maxHeight: float.PositiveInfinity,
                                        alignment: Alignment.bottomStart,
                                        child: new Padding(
                                            padding: EdgeInsets.only(
                                                start: NavBarUtils._kNavBarEdgePadding,
                                                bottom: 8.0f, // Bottom has a different padding.
                                            ),
                                            child: new SafeArea(
                                                top: false,
                                                bottom: false,
                                                child: new AnimatedOpacity(
                                                    opacity: showLargeTitle ? 1.0f : 0.0f,
                                                    duration: NavBarUtils._kNavBarTitleFadeDuration,
                                                    child: new Semantics(
                                                        header: true,
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
                                )
                            ),
                            Positioned(
                                left: 0.0f,
                                right: 0.0f,
                                top: 0.0f,
                                child: persistentNavigationBar
                            )
                        }
                    )
                )
            );

            if (!this.transitionBetweenRoutes || !_isTransitionable(context)) {
                return navBar;
            }

            return new Hero(
                tag: this.heroTag == NavBarUtils._defaultHeroTag
                    ? _HeroTag(Navigator.of(context))
                    : this.heroTag,
                createRectTween: _linearTranslateWithLargestRectSizeTween,
                flightShuttleBuilder: _navBarHeroFlightShuttleBuilder,
                placeholderBuilder: _navBarHeroLaunchPadBuilder,
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

        public override bool shouldRebuild(_LargeTitleNavigationBarSliverDelegate oldDelegate) {
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
            Key key,
            _NavigationBarStaticComponents components,
            EdgeInsets padding,
            bool middleVisible
        ) : base(key: key) {
        }

        public readonly _NavigationBarStaticComponents components;

        public readonly EdgeInsets padding;
        public readonly bool middleVisible;

        public override Widget build(BuildContext context) {
            Widget middle = this.components.middle;

            if (middle != null) {
                middle = new DefaultTextStyle(
                    style: CupertinoTheme.of(context).textTheme.navTitleTextStyle,
                    child: new Semantics(header: true, child: middle)
                );
                middle = this.middleVisible == null
                    ? middle
                    : new AnimatedOpacity(
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
        public _NavigationBarStaticComponentsKeys()
            : navBarBoxKey = new GlobalKey(debugLabel: "Navigation bar render box"),
        leadingKey = new GlobalKey(debugLabel: "Leading"),
        backChevronKey = new GlobalKey(debugLabel: "Back chevron"),
        backLabelKey = new GlobalKey(debugLabel: "Back label"),
        middleKey = new GlobalKey(debugLabel: "Middle"),
        trailingKey = new GlobalKey(debugLabel: "Trailing"),
        largeTitleKey = new GlobalKey(debugLabel: "Large title");

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
                ModalRoute<dynamic> route,
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

            static Widget _derivedTitle(
                bool automaticallyImplyTitle,
                ModalRoute<dynamic> currentRoute
            ) {
                if (automaticallyImplyTitle &&
                    currentRoute is CupertinoPageRoute &&
                    currentRoute.title != null) {
                    return new Text(currentRoute.title);
                }

                return null;
            }

        public readonly KeyedSubtree leading;
        static KeyedSubtree createLeading( {
            required GlobalKey leadingKey,
                required Widget userLeading,
                required ModalRoute<dynamic> route,
                required bool automaticallyImplyLeading,
                required EdgeInsets padding
        }) {
            Widget leadingContent;

            if (userLeading != null) {
                leadingContent = userLeading;
            }
            else if (
                automaticallyImplyLeading &&
                route is PageRoute &&
                route.canPop &&
                route.fullscreenDialog
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
                        start: padding?.start ?? NavBarUtils._kNavBarEdgePadding
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
        static KeyedSubtree createBackChevron( {
            required GlobalKey backChevronKey,
                required Widget userLeading,
                required ModalRoute<dynamic> route,
                required bool automaticallyImplyLeading
        }) {
            if (
                userLeading != null ||
                !automaticallyImplyLeading ||
                route == null ||
                !route.canPop ||
                (route is PageRoute && route.fullscreenDialog)
            ) {
                return null;
            }

            return new KeyedSubtree(key: backChevronKey, child: new _BackChevron());
        }

        public readonly KeyedSubtree backLabel;
        static KeyedSubtree createBackLabel( {
            required GlobalKey backLabelKey,
                required Widget userLeading,
                required ModalRoute<dynamic> route,
                required bool automaticallyImplyLeading,
                required string previousPageTitle
        }) {
            if (
                userLeading != null ||
                !automaticallyImplyLeading ||
                route == null ||
                !route.canPop ||
                (route is PageRoute && route.fullscreenDialog)
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
        static KeyedSubtree createMiddle( {
            required GlobalKey middleKey,
                required Widget userMiddle,
                required Widget userLargeTitle,
                required bool large,
                required bool automaticallyImplyTitle,
                required ModalRoute<dynamic> route
        }) {
            Widget middleContent = userMiddle;

            if (large) {
                middleContent ??= userLargeTitle;
            }

            middleContent ??= _derivedTitle(
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
        static KeyedSubtree createTrailing( {
            required GlobalKey trailingKey,
                required Widget userTrailing,
                required EdgeInsets padding
        }) {
            if (userTrailing == null) {
                return null;
            }

            return new KeyedSubtree(
                key: trailingKey,
                child: new Padding(
                    padding: EdgeInsets.only(
                        end: padding?.end ?? NavBarUtils._kNavBarEdgePadding
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
        static KeyedSubtree createLargeTitle( {
            required GlobalKey largeTitleKey,
                required Widget userLargeTitle,
                required bool large,
                required bool automaticImplyTitle,
                required ModalRoute<dynamic> route
        }) {
            if (!large) {
                return null;
            }

            Widget largeTitleContent = userLargeTitle ?? _derivedTitle(
                                           automaticallyImplyTitle: automaticImplyTitle,
                                           currentRoute: route
                                       );

            D.assert(
                largeTitleContent != null,
                "largeTitle was not provided and there was no title from the route."
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
        }

        CupertinoNavigationBarBackButton._assemble(
        this._backChevron,
        this._backLabel
        ) {
            this.previousPageTitle = null;
            this.color = null;
        }

        public readonly Color color;

        public readonly string previousPageTitle;

        public readonly Widget _backChevron;

        public readonly Widget _backLabel;

        public override Widget build(BuildContext context) {
            ModalRoute<dynamic> currentRoute = ModalRoute.of(context);
            D.assert(
                currentRoute?.canPop == true,
                "CupertinoNavigationBarBackButton should only be used in routes that can be popped"
            );

            TextStyle actionTextStyle = CupertinoTheme.of(context).textTheme.navActionTextStyle;
            if (this.color != null) {
                actionTextStyle = actionTextStyle.copyWith(color: this.color);
            }

            return new CupertinoButton(
                child: new Semantics(
                    container: true,
                    excludeSemantics: true,
                    label: "Back",
                    button: true,
                    child: new DefaultTextStyle(
                        style: actionTextStyle,
                        child: new ConstrainedBox(
                            constraints: new BoxConstraints(minWidth: NavBarUtils._kNavBarBackButtonTapWidth),
                            child: new Row(
                                mainAxisSize: MainAxisSize.min,
                                mainAxisAlignment: MainAxisAlignment.start,
                                children: new List<Widget> {


                                    const Padding(padding: EdgeInsets.only(start: 8.0f)),
                                    this._backChevron ?? const _BackChevron(), 
                                    const Padding(padding: EdgeInsets.only(start: 6.0f)),
                                    Flexible(
                                        child: this._backLabel ?? _BackLabel(
                                                   specifiedPreviousTitle: this.previousPageTitle,
                                                   route: currentRoute
                                               )
                                    )
                                }
                            )
                        )
                    )
                ),
                padding: EdgeInsets.zero,
                onPressed: () => { Navigator.maybePop(context); }
            );
        }
    }


    class _BackChevron : StatelessWidget {
        public _BackChevron(
            Key key) : base(key: key
        );

        public override Widget build(BuildContext context) {
            TextDirection textDirection = Directionality.of(context);
            TextStyle textStyle = DefaultTextStyle.of(context).style;

            Widget iconWidget = Text.rich(
                TextSpan(
                    text: string.fromCharCode(CupertinoIcons.back.codePoint),
                    style: new TextStyle(
                        inherit: false,
                        color: textStyle.color,
                        fontSize: 34.0f,
                        fontFamily: CupertinoIcons.back.fontFamily,
                        package: CupertinoIcons.back.fontPackage
                    )
                )
            );
            switch (textDirection) {
                case TextDirection.rtl:
                    iconWidget = new Transform(
                        transform: Matrix3.identity()..scale(-1.0f, 1.0f, 1.0f),
                        alignment: Alignment.center,
                        transformHitTests: false,
                        child: iconWidget
                    );
                    break;
                case TextDirection.ltr:
                    break;
            }

            return iconWidget;
        }
    }

    class _BackLabel : StatelessWidget {
        public _BackLabel(
            Key key,
            required this.specifiedPreviousTitle,
        ModalRoute<dynamic> route
        ) : base(key: key) {
            D.assert(route != null);
        }

        public readonly string specifiedPreviousTitle;
        public readonly ModalRoute<dynamic> route;

        Widget _buildPreviousTitleWidget(BuildContext context, string previousTitle, Widget child) {
            if (previousTitle == null) {
                return const SizedBox  (height: 0.0f, width: 0.0f);
            }

            Text textWidget = new Text(
                previousTitle,
                maxLines: 1,
                overflow: TextOverflow.ellipsis
            );

            if (previousTitle.length > 12) {
                textWidget =  const Text  ("Back");
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
            else if (route is CupertinoPageRoute<dynamic>) {
                CupertinoPageRoute<dynamic> cupertinoRoute = route;
                return new ValueListenableBuilder<string>(
                    valueListenable: cupertinoRoute.previousTitle,
                    builder: _buildPreviousTitleWidget
                );
            }
            else {
                return const SizedBox  (height: 0.0f, width: 0.0f);
            }
        }
    }

    class _TransitionableNavigationBar : StatelessWidget {
        public _TransitionableNavigationBar( {
            _NavigationBarStaticComponentsKeys componentsKeys,
                required Color backgroundColor,
                required this.backButtonTextStyle,
            required this.titleTextStyle,
            required this.largeTitleTextStyle,
            required this.border,
            required this.hasUserMiddle,
            bool largeExpanded,
                required this.child
                ) : base(key: componentsKeys.navBarBoxKey) {
                D.assert(componentsKeys != null);
                D.assert(largeExpanded != null);
                D.assert(!largeExpanded || this.largeTitleTextStyle != null);
            }

        public readonly _NavigationBarStaticComponentsKeys componentsKeys;
        public readonly Color backgroundColor;
        public readonly TextStyle backButtonTextStyle;
        public readonly TextStyle titleTextStyle;
        public readonly TextStyle largeTitleTextStyle;
        public readonly Border border;
        public readonly bool hasUserMiddle;
        public readonly bool largeExpanded;
        public readonly Widget child;

        RenderBox renderBox {
            get {
                RenderBox box = this.componentsKeys.navBarBoxKey.currentContext.findRenderObject();
                D.assert(
                    box.attached,
                    "_TransitionableNavigationBar.renderBox should be called when building " +
                    "hero flight shuttles when the from and the to nav bar boxes are already " +
                    "laid out and painted."
                );
                return box;
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(() {
                bool inHero;
                context.visitAncestorElements((Element ancestor) => {
                    if (ancestor is ComponentElement) {
                        D.assert(
                            ancestor.widget.GetType() != _NavigationBarTransition,
                            "_TransitionableNavigationBar should never re-appear inside " +
                            "_NavigationBarTransition. Keyed _TransitionableNavigationBar should " +
                            "only serve as anchor points in routes rather than appearing inside " +
                            "Hero flights themselves."
                        );
                        if (ancestor.widget.GetType() == Hero) {
                            inHero = true;
                        }
                    }

                    inHero ??= false;
                    return true;
                });
                D.assert(
                    inHero == true,
                    "_TransitionableNavigationBar should only be added as the immediate " +
                    "child of Hero widgets."
                );
                return true;
            }
            ());
            return this.child;
        }
    }

    class _NavigationBarTransition : StatelessWidget {
        public _NavigationBarTransition( {
            required this.animation,
            required this.topNavBar,
            required this.bottomNavBar
                ) :
            this.heightTween = new FloatTween(
                begin: this.bottomNavBar.renderBox.size.height,
                end: this.topNavBar.renderBox.size.height
            ),
            this.backgroundTween = new ColorTween(
                begin: this.bottomNavBar.backgroundColor,
                end: this.topNavBar.backgroundColor
            ),
            this.borderTween = new BorderTween(
                begin: this.bottomNavBar.border,
                end: this.topNavBar.border
            );

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
                AnimatedBuilder(
                    animation: this.animation,
                    builder: (BuildContext context, Widget child) => {
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

            children.removeWhere((Widget child) => child == null);

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
        public _NavigationBarComponentsTransition( {
            required this.animation,
            required _TransitionableNavigationBar bottomNavBar,
                required _TransitionableNavigationBar topNavBar,
                required TextDirection directionality
                ) :
            this.bottomComponents = bottomNavBar.componentsKeys,
            this.topComponents = topNavBar.componentsKeys,
            this.bottomNavBarBox = bottomNavBar.renderBox,
            this.topNavBarBox = topNavBar.renderBox,
            this.bottomBackButtonTextStyle = bottomNavBar.backButtonTextStyle,
            this.topBackButtonTextStyle = topNavBar.backButtonTextStyle,
            this.bottomTitleTextStyle = bottomNavBar.titleTextStyle,
            this.topTitleTextStyle = topNavBar.titleTextStyle,
            this.bottomLargeTitleTextStyle = bottomNavBar.largeTitleTextStyle,
            this.topLargeTitleTextStyle = topNavBar.largeTitleTextStyle,
            this.bottomHasUserMiddle = bottomNavBar.hasUserMiddle,
            this.topHasUserMiddle = topNavBar.hasUserMiddle,
            this.bottomLargeExpanded = bottomNavBar.largeExpanded,
            this.topLargeExpanded = topNavBar.largeExpanded,
            this.transitionBox =
                bottomNavBar.renderBox.paintBounds.expandToInclude(topNavBar.renderBox.paintBounds),
            this.forwardDirection = directionality == TextDirection.ltr ? 1.0f : -1.0f;

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

        public readonly bool bottomHasUserMiddle;
        public readonly bool topHasUserMiddle;
        public readonly bool bottomLargeExpanded;
        public readonly bool topLargeExpanded;

        public readonly Rect transitionBox;

        public readonly float forwardDirection;

        RelativeRect positionInTransitionBox(
            GlobalKey key,  {
            required RenderBox from
        }) {
            RenderBox componentBox = key.currentContext.findRenderObject();
            D.assert(componentBox.attached);

            return RelativeRect.fromRect(
                componentBox.localToGlobal(Offset.zero, ancestor: from) & componentBox.size, this.transitionBox
            );
        }

        RelativeRectTween slideFromLeadingEdge( {
            required GlobalKey fromKey,
                required RenderBox fromNavBarBox,
                required GlobalKey toKey,
                required RenderBox toNavBarBox
        }) {
            RelativeRect fromRect = positionInTransitionBox(fromKey, from: fromNavBarBox);

            RenderBox fromBox = fromKey.currentContext.findRenderObject();
            RenderBox toBox = toKey.currentContext.findRenderObject();

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

        Animation<float> fadeInFrom(float t,  {
            Curve curve = Curves.easeIn
        }) {
            return this.animation.drive(fadeIn.chain(
                CurveTween(curve: new Interval(t, 1.0f, curve: curve))
            ));
        }

        Animation<float> fadeOutBy(float t,  {
            Curve curve = Curves.easeOut
        }) {
            return this.animation.drive(fadeOut.chain(
                CurveTween(curve: new Interval(0.0f, t, curve: curve))
            ));
        }

        Widget bottomLeading {
            get {
                KeyedSubtree bottomLeading = this.bottomComponents.leadingKey.currentWidget;

                if (bottomLeading == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: positionInTransitionBox(this.bottomComponents.leadingKey, from: this.bottomNavBarBox),
                    child: new FadeTransition(
                        opacity: fadeOutBy(0.4f),
                        child: bottomLeading.child
                    )
                );
            }
        }

        Widget bottomBackChevron {
            get {
                KeyedSubtree bottomBackChevron = this.bottomComponents.backChevronKey.currentWidget;

                if (bottomBackChevron == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: positionInTransitionBox(this.bottomComponents.backChevronKey, from: this.bottomNavBarBox),
                    child: new FadeTransition(
                        opacity: fadeOutBy(0.6f),
                        child: new DefaultTextStyle(
                            style: this.bottomBackButtonTextStyle,
                            child: bottomBackChevron.child
                        )
                    )
                );
            }
        }

        Widget bottomBackLabel {
            get {
                KeyedSubtree bottomBackLabel = this.bottomComponents.backLabelKey.currentWidget;

                if (bottomBackLabel == null) {
                    return null;
                }

                RelativeRect from =
                    positionInTransitionBox(this.bottomComponents.backLabelKey, from: this.bottomNavBarBox);

                RelativeRectTween positionTween = new RelativeRectTween(
                    begin: from,
                    end: from.shift(
                        Offset(this.forwardDirection * (-this.bottomNavBarBox.size.width / 2.0f),
                            0.0f
                        )
                    )
                );

                return new PositionedTransition(
                    rect: this.animation.drive(positionTween),
                    child: new FadeTransition(
                        opacity: fadeOutBy(0.2f),
                        child: new DefaultTextStyle(
                            style: this.bottomBackButtonTextStyle,
                            child: bottomBackLabel.child
                        )
                    )
                );
            }
        }

        Widget bottomMiddle {
            get {
                KeyedSubtree bottomMiddle = this.bottomComponents.middleKey.currentWidget;
                KeyedSubtree topBackLabel = this.topComponents.backLabelKey.currentWidget;
                KeyedSubtree topLeading = this.topComponents.leadingKey.currentWidget;

                if (!this.bottomHasUserMiddle && this.bottomLargeExpanded) {
                    return null;
                }

                if (bottomMiddle != null && topBackLabel != null) {
                    return new PositionedTransition(
                        rect: this.animation.drive(slideFromLeadingEdge(
                            fromKey: this.bottomComponents.middleKey,
                            fromNavBarBox: this.bottomNavBarBox,
                            toKey: this.topComponents.backLabelKey,
                            toNavBarBox: this.topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: fadeOutBy(this.bottomHasUserMiddle ? 0.4f : 0.7f),
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
                        rect: positionInTransitionBox(this.bottomComponents.middleKey, from: this.bottomNavBarBox),
                        child: new FadeTransition(
                            opacity: fadeOutBy(this.bottomHasUserMiddle ? 0.4f : 0.7f),
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

        Widget bottomLargeTitle {
            get {
                KeyedSubtree bottomLargeTitle = this.bottomComponents.largeTitleKey.currentWidget;
                KeyedSubtree topBackLabel = this.topComponents.backLabelKey.currentWidget;
                KeyedSubtree topLeading = this.topComponents.leadingKey.currentWidget;

                if (bottomLargeTitle == null || !this.bottomLargeExpanded) {
                    return null;
                }

                if (bottomLargeTitle != null && topBackLabel != null) {
                    return new PositionedTransition(
                        rect: this.animation.drive(slideFromLeadingEdge(
                            fromKey: this.bottomComponents.largeTitleKey,
                            fromNavBarBox: this.bottomNavBarBox,
                            toKey: this.topComponents.backLabelKey,
                            toNavBarBox: this.topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: fadeOutBy(0.6f),
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
                    RelativeRect from = positionInTransitionBox(this.bottomComponents.largeTitleKey,
                        from: this.bottomNavBarBox);

                    RelativeRectTween positionTween = new RelativeRectTween(
                        begin: from,
                        end: from.shift(
                            Offset(this.forwardDirection * this.bottomNavBarBox.size.width / 4.0f,
                                0.0f
                            )
                        )
                    );

                    return new PositionedTransition(
                        rect: this.animation.drive(positionTween),
                        child: new FadeTransition(
                            opacity: fadeOutBy(0.4f),
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

        Widget bottomTrailing {
            get {
                KeyedSubtree bottomTrailing = this.bottomComponents.trailingKey.currentWidget;

                if (bottomTrailing == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: positionInTransitionBox(this.bottomComponents.trailingKey, from: this.bottomNavBarBox),
                    child: new FadeTransition(
                        opacity: fadeOutBy(0.6f),
                        child: bottomTrailing.child
                    )
                );
            }
        }

        Widget topLeading {
            get {
                KeyedSubtree topLeading = this.topComponents.leadingKey.currentWidget;

                if (topLeading == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: positionInTransitionBox(this.topComponents.leadingKey, from: this.topNavBarBox),
                    child: new FadeTransition(
                        opacity: fadeInFrom(0.6f),
                        child: topLeading.child
                    )
                );
            }
        }

        Widget topBackChevron {
            get {
                KeyedSubtree topBackChevron = this.topComponents.backChevronKey.currentWidget;
                KeyedSubtree bottomBackChevron = this.bottomComponents.backChevronKey.currentWidget;

                if (topBackChevron == null) {
                    return null;
                }

                RelativeRect to = positionInTransitionBox(this.topComponents.backChevronKey, from: this.topNavBarBox);
                RelativeRect from = to;

                if (bottomBackChevron == null) {
                    RenderBox topBackChevronBox = this.topComponents.backChevronKey.currentContext.findRenderObject();
                    from = to.shift(
                        Offset(this.forwardDirection * topBackChevronBox.size.width * 2.0f,
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
                        opacity: fadeInFrom(bottomBackChevron == null ? 0.7f : 0.4f),
                        child: new DefaultTextStyle(
                            style: this.topBackButtonTextStyle,
                            child: topBackChevron.child
                        )
                    )
                );
            }
        }

        Widget topBackLabel {
            get {
                KeyedSubtree bottomMiddle = this.bottomComponents.middleKey.currentWidget;
                KeyedSubtree bottomLargeTitle = this.bottomComponents.largeTitleKey.currentWidget;
                KeyedSubtree topBackLabel = this.topComponents.backLabelKey.currentWidget;

                if (topBackLabel == null) {
                    return null;
                }

                RenderAnimatedOpacity topBackLabelOpacity =
                    this.topComponents.backLabelKey.currentContext?.ancestorRenderObjectOfType(
                const TypeMatcher<RenderAnimatedOpacity>  ()
                    );

                Animation<float> midClickOpacity;
                if (topBackLabelOpacity != null && topBackLabelOpacity.opacity.value < 1.0f) {
                    midClickOpacity = this.animation.drive(new FloatTween(
                        begin: 0.0f,
                        end: topBackLabelOpacity.opacity.value
                    ));
                }

                if (bottomLargeTitle != null &&
                    topBackLabel != null && this.bottomLargeExpanded) {
                    return new PositionedTransition(
                        rect: this.animation.drive(slideFromLeadingEdge(
                            fromKey: this.bottomComponents.largeTitleKey,
                            fromNavBarBox: this.bottomNavBarBox,
                            toKey: this.topComponents.backLabelKey,
                            toNavBarBox: this.topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: midClickOpacity ?? fadeInFrom(0.4f),
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
                        rect: this.animation.drive(slideFromLeadingEdge(
                            fromKey: this.bottomComponents.middleKey,
                            fromNavBarBox: this.bottomNavBarBox,
                            toKey: this.topComponents.backLabelKey,
                            toNavBarBox: this.topNavBarBox
                        )),
                        child: new FadeTransition(
                            opacity: midClickOpacity ?? fadeInFrom(0.3f),
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

        Widget topMiddle {
            get {
                KeyedSubtree topMiddle = this.topComponents.middleKey.currentWidget;

                if (topMiddle == null) {
                    return null;
                }

                if (!this.topHasUserMiddle && this.topLargeExpanded) {
                    return null;
                }

                RelativeRect to = positionInTransitionBox(this.topComponents.middleKey, from: this.topNavBarBox);

                RelativeRectTween positionTween = new RelativeRectTween(
                    begin: to.shift(
                        Offset(this.forwardDirection * this.topNavBarBox.size.width / 2.0f,
                            0.0f
                        )
                    ),
                    end: to
                );

                return new PositionedTransition(
                    rect: this.animation.drive(positionTween),
                    child: new FadeTransition(
                        opacity: fadeInFrom(0.25f),
                        child: new DefaultTextStyle(
                            style: this.topTitleTextStyle,
                            child: topMiddle.child
                        )
                    )
                );
            }
        }

        Widget topTrailing {
            get {
                KeyedSubtree topTrailing = this.topComponents.trailingKey.currentWidget;

                if (topTrailing == null) {
                    return null;
                }

                return Positioned.fromRelativeRect(
                    rect: positionInTransitionBox(this.topComponents.trailingKey, from: this.topNavBarBox),
                    child: new FadeTransition(
                        opacity: fadeInFrom(0.4f),
                        child: topTrailing.child
                    )
                );
            }
        }

        Widget topLargeTitle {
            get {
                KeyedSubtree topLargeTitle = this.topComponents.largeTitleKey.currentWidget;

                if (topLargeTitle == null || !this.topLargeExpanded) {
                    return null;
                }

                RelativeRect to = positionInTransitionBox(this.topComponents.largeTitleKey, from: this.topNavBarBox);

                RelativeRectTween positionTween = new RelativeRectTween(
                    begin: to.shift(
                        Offset(this.forwardDirection * this.topNavBarBox.size.width,
                            0.0f
                        )
                    ),
                    end: to
                );

                return new PositionedTransition(
                    rect: this.animation.drive(positionTween),
                    child: new FadeTransition(
                        opacity: fadeInFrom(0.3f),
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

    CreateRectTween _linearTranslateWithLargestRectSizeTween = (Rect begin, Rect end) => {
    Size largestSize = new Size(
            Mathf.Max(begin.size.width, end.size.width),
            Mathf.Max(begin.size.height, end.size.height)
        );
        return new RectTween(
        begin: begin.topLeft & largestSize,
    end: end.topLeft & largestSize
    );
    };
    TransitionBuilder _navBarHeroLaunchPadBuilder = (
    BuildContext context,
    Widget child
    ) {
    D.assert(child is _TransitionableNavigationBar);
    return new Visibility(
        maintainSize: true,
        maintainAnimation: true,
        maintainState: true,
        visible: false,
        child: child
    );
    };
    HeroFlightShuttleBuilder _navBarHeroFlightShuttleBuilder = (
    BuildContext flightContext,
    Animation<float> animation,
    HeroFlightDirection flightDirection,
    BuildContext fromHeroContext,
    BuildContext toHeroContext
    ) {
    D.assert(animation != null);
    D.assert(flightDirection != null);
    D.assert(fromHeroContext != null);
    D.assert(toHeroContext != null);
    D.assert(fromHeroContext.widget is Hero);
    D.assert(toHeroContext.widget is Hero);
    Hero fromHeroWidget = fromHeroContext.widget;
    Hero toHeroWidget = toHeroContext.widget;
    D.assert(fromHeroWidget.child is _TransitionableNavigationBar);
    D.assert(toHeroWidget.child is _TransitionableNavigationBar);
    _TransitionableNavigationBar fromNavBar = fromHeroWidget.child;
    _TransitionableNavigationBar toNavBar = toHeroWidget.child;
    D.assert(fromNavBar.componentsKeys != null);
    D.assert(toNavBar.componentsKeys != null);
    D.assert(
        fromNavBar.componentsKeys.navBarBoxKey.currentContext.owner != null,
        "The from nav bar to Hero must have been mounted in the previous frame"
        );

    D.assert(
        toNavBar.componentsKeys.navBarBoxKey.currentContext.owner != null,
        "The to nav bar to Hero must have been mounted in the previous frame"
        );

    switch (flightDirection) {
        case HeroFlightDirection.push:
        return new _NavigationBarTransition(
            animation: animation,
            bottomNavBar: fromNavBar,
            topNavBar: toNavBar
        );
        break;
        case HeroFlightDirection.pop:
        return new _NavigationBarTransition(
            animation: animation,
            bottomNavBar: toNavBar,
            topNavBar: fromNavBar
        );
    }
    };
}
