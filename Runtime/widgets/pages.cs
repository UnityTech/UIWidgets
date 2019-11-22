using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public abstract class PageRoute : ModalRoute {
        public readonly bool fullscreenDialog;

        public PageRoute() {}

        public PageRoute(RouteSettings settings, bool fullscreenDialog = false) : base(settings) {
            this.fullscreenDialog = fullscreenDialog;
        }

        public override bool opaque {
            get { return true; }
        }

        public override bool barrierDismissible {
            get { return false; }
        }

        public override bool canTransitionTo(TransitionRoute nextRoute) {
            return nextRoute is PageRoute;
        }

        public override bool canTransitionFrom(TransitionRoute previousRoute) {
            return previousRoute is PageRoute;
        }

        public override AnimationController createAnimationController() {
            var controller = base.createAnimationController();
            if (this.settings.isInitialRoute) {
                controller.setValue(1.0f);
            }

            return controller;
        }
    }

    public class PageRouteBuilder : PageRoute {
        public readonly RoutePageBuilder pageBuilder;

        public readonly RouteTransitionsBuilder transitionsBuilder;

        public PageRouteBuilder(
            RouteSettings settings = null,
            RoutePageBuilder pageBuilder = null,
            RouteTransitionsBuilder transitionsBuilder = null,
            TimeSpan? transitionDuration = null,
            bool opaque = true,
            bool barrierDismissible = false,
            Color barrierColor = null,
            bool maintainState = true
        ) : base(settings) {
            D.assert(pageBuilder != null);
            this.opaque = opaque;
            this.pageBuilder = pageBuilder;
            this.transitionsBuilder = transitionsBuilder ?? this._defaultTransitionsBuilder;
            this.transitionDuration = transitionDuration ?? TimeSpan.FromMilliseconds(300);
            this.barrierColor = barrierColor;
            this.maintainState = maintainState;
            this.barrierDismissible = barrierDismissible;
        }

        public override TimeSpan transitionDuration { get; }

        public override bool opaque { get; }

        public override bool barrierDismissible { get; }

        public override Color barrierColor { get; }

        public override bool maintainState { get; }

        Widget _defaultTransitionsBuilder(BuildContext context, Animation<float>
            animation, Animation<float> secondaryAnimation, Widget child) {
            return child;
        }

        public override Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation) {
            return this.pageBuilder(context, animation, secondaryAnimation);
        }

        public override Widget buildTransitions(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation, Widget child) {
            return this.transitionsBuilder(context, animation, secondaryAnimation, child);
        }
    }
}