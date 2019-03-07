using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
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
            bool showPerformanceOverlay = false,
            Window window = null) : base(key: key) {
            D.assert(window != null);
            this.window = window;
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

        public readonly Color color;

        public readonly bool showPerformanceOverlay;

        public readonly Window window;


        public override State createState() {
            return new _MaterialAppState();
        }
    }


    class _MaterialAppState : State<MaterialApp> {
        public override void initState() {
            base.initState();
            this._updateNavigator();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            this._updateNavigator();
        }

        List<NavigatorObserver> _navigatorObservers;

        void _updateNavigator() {
            if (this.widget.home != null ||
                this.widget.routes.isNotEmpty() ||
                this.widget.onGenerateRoute != null ||
                this.widget.onUnknownRoute != null) {
                this._navigatorObservers = new List<NavigatorObserver>(this.widget.navigatorObservers);
            }
            else {
                this._navigatorObservers = null;
            }
        }

        RectTween _createRectTween(Rect begin, Rect end) {
            return new MaterialRectArcTween(begin: begin, end: end);
        }

        public override Widget build(BuildContext context) {
            ThemeData theme = this.widget.theme ?? ThemeData.fallback();
            Widget result = new AnimatedTheme(
                data: theme,
                isMaterialAppTheme: true,
                child: new WidgetsApp(
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
                    builder: this.widget.builder,
                    textStyle: AppUtils._errorTextStyle
                )
            );

            return result;
        }
    }
}