using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    static class AppUtils {
        public static readonly TextStyle _errorTextStyle = new TextStyle(
            color: new Color(0xD0FF0000),
            fontFamily: "monospace",
            fontSize: 48.0f,
            fontWeight: FontWeight.w700,
            decoration: TextDecoration.underline,
            decorationColor: new Color(0xFFFFFF00),
            decorationStyle: TextDecorationStyle.doubleLine
        );
    }


    public class MaterialApp : StatefulWidget {
        public MaterialApp(
            Key key = null,
            GlobalKey<NavigatorState> navigatorKey = null,
            Widget home = null,
            Dictionary<string, WidgetBuilder> routes = null,
            string initialRoute = null,
            RouteFactory onGenerateRoute = null,
            RouteFactory onUnknownRoute = null,
            List<NavigatorObserver> navigatorObservers = null,
            TransitionBuilder builder = null,
            string title = "",
            Color color = null,
            ThemeData theme = null,
            ThemeData darkTheme = null,
            Locale locale = null,
            List<LocalizationsDelegate<MaterialLocalizations>> localizationsDelegates = null,
            LocaleListResolutionCallback localeListResolutionCallback = null,
            LocaleResolutionCallback localeResolutionCallback = null,
            List<Locale> supportedLocales = null,
            bool showPerformanceOverlay = false
        ) : base(key: key) {
            supportedLocales = supportedLocales ?? new List<Locale> {new Locale("en", "US")};
            this.navigatorKey = navigatorKey;
            this.home = home;
            this.routes = routes ?? new Dictionary<string, WidgetBuilder>();
            this.initialRoute = initialRoute;
            this.onGenerateRoute = onGenerateRoute;
            this.onUnknownRoute = onUnknownRoute;
            this.navigatorObservers = navigatorObservers ?? new List<NavigatorObserver>();
            this.builder = builder;
            this.title = title;
            this.color = color;
            this.theme = theme;
            this.darkTheme = darkTheme;
            this.locale = locale;
            this.localizationsDelegates = localizationsDelegates;
            this.localeListResolutionCallback = localeListResolutionCallback;
            this.localeResolutionCallback = localeResolutionCallback;
            this.supportedLocales = supportedLocales;
            this.showPerformanceOverlay = showPerformanceOverlay;
        }

        public readonly GlobalKey<NavigatorState> navigatorKey;

        public readonly Widget home;

        public readonly Dictionary<string, WidgetBuilder> routes;

        public readonly string initialRoute;

        public readonly RouteFactory onGenerateRoute;

        public readonly RouteFactory onUnknownRoute;

        public readonly List<NavigatorObserver> navigatorObservers;

        public readonly TransitionBuilder builder;

        public readonly string title;

        public readonly ThemeData theme;

        public readonly ThemeData darkTheme;

        public readonly Color color;

        public readonly Locale locale;

        public readonly List<LocalizationsDelegate<MaterialLocalizations>> localizationsDelegates;

        public readonly LocaleListResolutionCallback localeListResolutionCallback;

        public readonly LocaleResolutionCallback localeResolutionCallback;

        public readonly List<Locale> supportedLocales;

        public readonly bool showPerformanceOverlay;

        public override State createState() {
            return new _MaterialAppState();
        }
    }


    class _MaterialAppState : State<MaterialApp> {
        HeroController _heroController;
        
        public override void initState() {
            base.initState();
            this._heroController = new HeroController(createRectTween: this._createRectTween);
            this._updateNavigator();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (this.widget.navigatorKey != (oldWidget as MaterialApp).navigatorKey) {
                this._heroController = new HeroController(createRectTween: this._createRectTween);
            }
            this._updateNavigator();
        }

        List<NavigatorObserver> _navigatorObservers;

        void _updateNavigator() {
            if (this.widget.home != null ||
                this.widget.routes.isNotEmpty() ||
                this.widget.onGenerateRoute != null ||
                this.widget.onUnknownRoute != null) {
                this._navigatorObservers = new List<NavigatorObserver>(this.widget.navigatorObservers);
                this._navigatorObservers.Add(this._heroController);
            }
            else {
                this._navigatorObservers = new List<NavigatorObserver>();
            }
        }

        RectTween _createRectTween(Rect begin, Rect end) {
            return new MaterialRectArcTween(begin: begin, end: end);
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
            Widget result = new WidgetsApp(
                key: new GlobalObjectKey<State>(this),
                navigatorKey: this.widget.navigatorKey,
                navigatorObservers: this._navigatorObservers,
                pageRouteBuilder: (RouteSettings settings, WidgetBuilder builder) =>
                    new MaterialPageRoute(settings: settings, builder: builder),
                home: this.widget.home,
                routes: this.widget.routes,
                initialRoute: this.widget.initialRoute,
                onGenerateRoute: this.widget.onGenerateRoute,
                onUnknownRoute: this.widget.onUnknownRoute,
                builder: (BuildContext _context, Widget child) => {
                    ThemeData theme;
                    Brightness platformBrightness = MediaQuery.platformBrightnessOf(_context);
                    if (platformBrightness == Brightness.dark && this.widget.darkTheme != null) {
                        theme = this.widget.darkTheme;
                    }
                    else if (this.widget.theme != null) {
                        theme = this.widget.theme;
                    }
                    else {
                        theme = ThemeData.fallback();
                    }

                    return new AnimatedTheme(
                        data: theme,
                        isMaterialAppTheme: true,
                        child: this.widget.builder != null
                            ? new Builder(
                                builder: (__context) => { return this.widget.builder(__context, child); }
                            )
                            : child
                    );
                },
                textStyle: AppUtils._errorTextStyle,
                locale: this.widget.locale,
                localizationsDelegates: this._localizationsDelegates,
                localeResolutionCallback: this.widget.localeResolutionCallback,
                localeListResolutionCallback: this.widget.localeListResolutionCallback,
                supportedLocales: this.widget.supportedLocales,
                showPerformanceOverlay: this.widget.showPerformanceOverlay
            );

            return result;
        }
    }
}