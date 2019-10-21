using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.cupertino {
    public class CupertinoApp : StatefulWidget {
        public CupertinoApp(
            Key key = null,
            GlobalKey<NavigatorState> navigatorKey = null,
            Widget home = null,
            CupertinoThemeData theme = null,
            Dictionary<string, WidgetBuilder> routes = null,
            string initialRoute = null,
            RouteFactory onGenerateRoute = null,
            RouteFactory onUnknownRoute = null,
            List<NavigatorObserver> navigatorObservers = null,
            TransitionBuilder builder = null,
            string title = "",
            GenerateAppTitle onGenerateTitle = null,
            Color color = null,
            Locale locale = null,
            List<LocalizationsDelegate<CupertinoLocalizations>> localizationsDelegates = null,
            LocaleListResolutionCallback localeListResolutionCallback = null,
            LocaleResolutionCallback localeResolutionCallback = null,
            List<Locale> supportedLocales = null,
            bool showPerformanceOverlay = false
        ) : base(key: key) {
            D.assert(title != null);

            supportedLocales = supportedLocales ?? new List<Locale> {new Locale("en", "US")};
            this.navigatorKey = navigatorKey;
            this.home = home;
            this.theme = theme;
            this.routes = routes ?? new Dictionary<string, WidgetBuilder>();
            this.initialRoute = initialRoute;
            this.onGenerateRoute = onGenerateRoute;
            this.onUnknownRoute = onUnknownRoute;
            this.navigatorObservers = navigatorObservers ?? new List<NavigatorObserver>();
            this.builder = builder;
            this.title = title;
            this.onGenerateTitle = onGenerateTitle;
            this.color = color;
            this.locale = locale;
            this.localizationsDelegates = localizationsDelegates;
            this.localeListResolutionCallback = localeListResolutionCallback;
            this.localeResolutionCallback = localeResolutionCallback;
            this.supportedLocales = supportedLocales;
            this.showPerformanceOverlay = showPerformanceOverlay;
        }

        public readonly GlobalKey<NavigatorState> navigatorKey;
        public readonly Widget home;
        public readonly CupertinoThemeData theme;
        public readonly Dictionary<string, WidgetBuilder> routes;
        public readonly string initialRoute;
        public readonly RouteFactory onGenerateRoute;
        public readonly RouteFactory onUnknownRoute;
        public readonly List<NavigatorObserver> navigatorObservers;
        public readonly TransitionBuilder builder;
        public readonly string title;
        public readonly GenerateAppTitle onGenerateTitle;
        public readonly Color color;
        public readonly Locale locale;
        public readonly List<LocalizationsDelegate<CupertinoLocalizations>> localizationsDelegates;
        public readonly LocaleListResolutionCallback localeListResolutionCallback;
        public readonly LocaleResolutionCallback localeResolutionCallback;
        public readonly List<Locale> supportedLocales;
        public readonly bool showPerformanceOverlay;

        public override State createState() {
            return new _CupertinoAppState();
        }

        public static HeroController createCupertinoHeroController() {
            return new HeroController();
        }
    }


    public class _AlwaysCupertinoScrollBehavior : ScrollBehavior {
        public override Widget buildViewportChrome(BuildContext context, Widget child, AxisDirection axisDirection) {
            return child;
        }

        public override ScrollPhysics getScrollPhysics(BuildContext context) {
            return new BouncingScrollPhysics();
        }
    }

    class _CupertinoAppState : State<CupertinoApp> {
        HeroController _heroController;

        public override void initState() {
            base.initState();
            this._heroController = CupertinoApp.createCupertinoHeroController();
            this._updateNavigator();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (this.widget.navigatorKey != ((CupertinoApp) oldWidget).navigatorKey) {
                this._heroController = CupertinoApp.createCupertinoHeroController();
            }

            this._updateNavigator();
        }

        List<NavigatorObserver> _navigatorObservers;

        void _updateNavigator() {
            if (this.widget.home != null || this.widget.routes.isNotEmpty() || this.widget.onGenerateRoute != null ||
                this.widget.onUnknownRoute != null) {
                this._navigatorObservers = new List<NavigatorObserver>();
                foreach (var item in this.widget.navigatorObservers) {
                    this._navigatorObservers.Add(item);
                }
            }
            else {
                this._navigatorObservers = new List<NavigatorObserver>();
            }
        }
        
        List<LocalizationsDelegate> _localizationsDelegates {
            get {
                var _delegates = new List<LocalizationsDelegate>();
                if (this.widget.localizationsDelegates != null) {
                    _delegates.AddRange(this.widget.localizationsDelegates);
                }

                _delegates.Add(DefaultCupertinoLocalizations.del);
                _delegates.Add(DefaultMaterialLocalizations.del);
                return new List<LocalizationsDelegate>(_delegates);
            }
        }

        public override Widget build(BuildContext context) {
            CupertinoThemeData effectiveThemeData = this.widget.theme ?? new CupertinoThemeData();

            return new ScrollConfiguration(
                behavior: new _AlwaysCupertinoScrollBehavior(),
                child: new CupertinoTheme(
                    data: effectiveThemeData,
                    child: new WidgetsApp(
                        pageRouteBuilder: (RouteSettings settings, WidgetBuilder builder) =>
                            new CupertinoPageRoute(settings: settings, builder: builder),
                        home: this.widget.home,
                        routes: this.widget.routes,
                        initialRoute: this.widget.initialRoute,
                        onGenerateRoute: this.widget.onGenerateRoute,
                        onUnknownRoute: this.widget.onUnknownRoute,
                        builder: this.widget.builder,
                        title: this.widget.title,
                        onGenerateTitle: this.widget.onGenerateTitle,
                        textStyle: effectiveThemeData.textTheme.textStyle,
                        color: this.widget.color ?? CupertinoColors.activeBlue,
                        locale: this.widget.locale,
                        localizationsDelegates: this._localizationsDelegates,
                        localeResolutionCallback: this.widget.localeResolutionCallback,
                        localeListResolutionCallback: this.widget.localeListResolutionCallback,
                        supportedLocales: this.widget.supportedLocales,
                        showPerformanceOverlay: this.widget.showPerformanceOverlay,
                        inspectorSelectButtonBuilder: (BuildContext _context, VoidCallback onPressed) => {
                            return CupertinoButton.filled(
                                child: new Icon(
                                    CupertinoIcons.search,
                                    size: 28.0f,
                                    color: CupertinoColors.white
                                ),
                                padding: EdgeInsets.zero,
                                onPressed: onPressed
                            );
                        }
                    )
                )
            );
        }
    }
}