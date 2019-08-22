using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using DialogUtils = Unity.UIWidgets.widgets.DialogUtils;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsSample {
    public class NavigationSample : UIWidgetsSamplePanel {

        protected override Widget createWidget() {
            return new WidgetsApp(
                initialRoute: "/",
                textStyle: new TextStyle(fontSize: 24),
                pageRouteBuilder: this.pageRouteBuilder,
                routes: new Dictionary<string, WidgetBuilder> {
                    {"/", (context) => new HomeScreen()},
                    {"/detail", (context) => new DetailScreen()}
                });
        }
        
        protected override PageRouteFactory pageRouteBuilder {
            get {
                return (RouteSettings settings, WidgetBuilder builder) =>
                    new PageRouteBuilder(
                        settings: settings,
                        pageBuilder: (BuildContext context, Animation<float> animation,
                            Animation<float> secondaryAnimation) => builder(context),
                        transitionsBuilder: (BuildContext context, Animation<float>
                                animation, Animation<float> secondaryAnimation, Widget child) =>
                            new _FadeUpwardsPageTransition(
                                routeAnimation: animation,
                                child: child
                            )
                    );
            }
        }
    }


    class HomeScreen : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new NavigationPage(
                body: new Container(
                    color: new Color(0xFF888888),
                    child: new Center(
                        child: new CustomButton(onPressed: () => { Navigator.pushNamed(context, "/detail"); },
                            child: new Text("Go to Detail"))
                    )),
                title: "Home"
            );
        }
    }

    class DetailScreen : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new NavigationPage(
                body: new Container(
                    color: new Color(0xFF1389FD),
                    child: new Center(
                        child: new Column(
                            children: new List<Widget>() {
                                new CustomButton(onPressed: () => { Navigator.pop(context); }, child: new Text("Back")),
                                new CustomButton(
                                    onPressed: () => {
                                        _Dialog.showDialog(context, builder: (BuildContext c) => new Dialog());
                                    }, child: new Text("Show Dialog"))
                            }
                        )
                    )),
                title: "Detail");
        }
    }

    class Dialog : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new Center(child: new Container(
                color: new Color(0xFFFF0000),
                width: 100,
                height: 80,
                child: new Center(
                    child: new Text("Hello Dialog")
                )));
        }
    }

    class _FadeUpwardsPageTransition : StatelessWidget {
        internal _FadeUpwardsPageTransition(
            Key key = null,
            Animation<float> routeAnimation = null, // The route's linear 0.0 - 1.0 animation.
            Widget child = null
        ) : base(key: key) {
            this._positionAnimation = _bottomUpTween.chain(_fastOutSlowInTween).animate(routeAnimation);
            this._opacityAnimation = _easeInTween.animate(routeAnimation);
            this.child = child;
        }

        static Tween<Offset> _bottomUpTween = new OffsetTween(
            begin: new Offset(0.0f, 0.25f),
            end: Offset.zero
        );

        static Animatable<float> _fastOutSlowInTween = new CurveTween(curve: Curves.fastOutSlowIn);
        static Animatable<float> _easeInTween = new CurveTween(curve: Curves.easeIn);

        readonly Animation<Offset> _positionAnimation;
        readonly Animation<float> _opacityAnimation;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new SlideTransition(
                position: this._positionAnimation,
                child: new FadeTransition(
                    opacity: this._opacityAnimation,
                    child: this.child
                )
            );
        }
    }

    class NavigationPage : StatelessWidget {
        public readonly Widget body;
        public readonly string title;

        public NavigationPage(Widget body = null, string title = null) {
            this.title = title;
            this.body = body;
        }

        public override Widget build(BuildContext context) {
            Widget back = null;
            if (Navigator.of(context).canPop()) {
                back = new CustomButton(onPressed: () => { Navigator.pop(context); },
                    child: new Text("Go Back"));
                back = new Column(mainAxisAlignment: MainAxisAlignment.center, children: new List<Widget>() {back});
            }


            return new Container(
                child: new Column(
                    children: new List<Widget>() {
                        new ConstrainedBox(constraints: new BoxConstraints(maxHeight: 80),
                            child: new DecoratedBox(
                                decoration: new BoxDecoration(color: new Color(0XFFE1ECF4)),
                                child: new NavigationToolbar(leading: back,
                                    middle: new Text(this.title, textAlign: TextAlign.center)))),
                        new Flexible(child: this.body)
                    }
                )
            );
        }
    }

    static class _Dialog {
        public static void showDialog(BuildContext context,
            bool barrierDismissible = true, WidgetBuilder builder = null) {
            DialogUtils.showGeneralDialog(
                context: context,
                pageBuilder: (BuildContext buildContext, Animation<float> animation,
                    Animation<float> secondaryAnimation) => {
                    return builder(buildContext);
                },
                barrierDismissible: barrierDismissible,
                barrierColor: new Color(0x8A000000),
                transitionDuration: TimeSpan.FromMilliseconds(150),
                transitionBuilder: _buildMaterialDialogTransitions
            );
        }

        static Widget _buildMaterialDialogTransitions(BuildContext context,
            Animation<float> animation, Animation<float> secondaryAnimation, Widget child) {
            return new FadeTransition(
                opacity: new CurvedAnimation(
                    parent: animation,
                    curve: Curves.easeOut
                ),
                child: child
            );
        }
    }
}