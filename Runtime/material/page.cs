using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class MaterialPageRoute : PageRoute {
        public MaterialPageRoute(
            WidgetBuilder builder = null,
            RouteSettings settings = null,
            bool maintainState = true,
            bool fullscreenDialog = false) : base(settings: settings, fullscreenDialog: fullscreenDialog) {
            D.assert(builder != null);
            this.builder = builder;
            this.maintainState = maintainState;
            D.assert(this.opaque);
        }

        public readonly WidgetBuilder builder;

        public override bool maintainState { get; }

        public override TimeSpan transitionDuration {
            get { return new TimeSpan(0, 0, 0, 0, 300); }
        }

        public override Color barrierColor {
            get { return null; }
        }

        public override bool canTransitionFrom(TransitionRoute previousRoute) {
            return previousRoute is MaterialPageRoute;
        }

        public override bool canTransitionTo(TransitionRoute nextRoute) {
            return nextRoute is MaterialPageRoute && !((MaterialPageRoute) nextRoute).fullscreenDialog;
        }

        public override Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation) {
            Widget result = this.builder(context);
            D.assert(() => {
                if (result == null) {
                    throw new UIWidgetsError(
                        "The builder for route " + this.settings.name + "returned null. \n" +
                        "Route builders must never return null."
                    );
                }

                return true;
            });
            return result;
        }

        public override Widget buildTransitions(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation, Widget child) {
            PageTransitionsTheme theme = Theme.of(context).pageTransitionsTheme;
            return theme.buildTranstions(this, context, animation, secondaryAnimation, child);
        }
    }
}