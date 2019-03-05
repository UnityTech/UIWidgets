using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class _FadeUpwardsPageTransition : StatelessWidget {
        public _FadeUpwardsPageTransition(
            Key key = null,
            Animation<float> routeAnimation = null,
            Widget child = null) : base(key: key) {
            D.assert(routeAnimation != null);
            D.assert(child != null);
            this._positionAnimation = routeAnimation.drive(_bottomUpTween.chain(_fastOutSlowInTween));
            this._opacityAnimation = routeAnimation.drive(_easeInTween);
            this.child = child;
        }

        static readonly Tween<Offset> _bottomUpTween = new OffsetTween(
            begin: new Offset(0.0f, 0.25f),
            end: Offset.zero
        );

        static readonly Animatable<float> _fastOutSlowInTween = new CurveTween(
            curve: Curves.fastOutSlowIn);

        static readonly Animatable<float> _easeInTween = new CurveTween(
            curve: Curves.easeIn);

        public readonly Animation<Offset> _positionAnimation;
        public readonly Animation<float> _opacityAnimation;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new SlideTransition(
                position: this._positionAnimation,
                child: new FadeTransition(
                    opacity: this._opacityAnimation,
                    child: this.child));
        }
    }

    public abstract class PageTransitionsBuilder {
        public PageTransitionsBuilder() {
        }

        public abstract Widget buildTransitions(
            PageRoute route,
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation,
            Widget child);
    }


    public class FadeUpwardsPageTransitionsBuilder : PageTransitionsBuilder {
        public FadeUpwardsPageTransitionsBuilder() {
        }

        public override Widget buildTransitions(
            PageRoute route,
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation,
            Widget child) {
            return new _FadeUpwardsPageTransition(
                routeAnimation: animation,
                child: child);
        }
    }

    public class PageTransitionsTheme : Diagnosticable, IEquatable<PageTransitionsTheme> {
        public PageTransitionsTheme(
            PageTransitionsBuilder builder = null) {
            this._builder = builder;
        }

        static PageTransitionsBuilder _defaultBuilder = new FadeUpwardsPageTransitionsBuilder();

        public PageTransitionsBuilder builder {
            get { return this._builder ?? _defaultBuilder; }
        }

        readonly PageTransitionsBuilder _builder;

        public Widget buildTranstions(
            PageRoute route,
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation,
            Widget child) {
            PageTransitionsBuilder matchingBuilder = this.builder;
            return matchingBuilder.buildTransitions(route, context, animation, secondaryAnimation, child);
        }

        List<PageTransitionsBuilder> _all(PageTransitionsBuilder builder) {
            return new List<PageTransitionsBuilder> {this.builder};
        }

        public bool Equals(PageTransitionsTheme other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this._all(this.builder) == this._all(other.builder);
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

            return this.Equals((PageTransitionsTheme) obj);
        }

        public static bool operator ==(PageTransitionsTheme left, PageTransitionsTheme right) {
            return Equals(left, right);
        }

        public static bool operator !=(PageTransitionsTheme left, PageTransitionsTheme right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this._all(this.builder).GetHashCode();
                return hashCode;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<PageTransitionsBuilder>("builder", this.builder,
                defaultValue: _defaultBuilder));
        }
    }
}