using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
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

    class _OpenUpwardsPageTransition : StatelessWidget {
        public _OpenUpwardsPageTransition(
            Key key = null,
            Animation<float> animation = null,
            Animation<float> secondaryAnimation = null,
            Widget child = null
        ) : base(key: key) {
            this.animation = animation;
            this.secondaryAnimation = secondaryAnimation;
            this.child = child;
        }

        static readonly OffsetTween _primaryTranslationTween = new OffsetTween(
            begin: new Offset(0.0f, 0.05f),
            end: Offset.zero
        );

        static readonly OffsetTween _secondaryTranslationTween = new OffsetTween(
            begin: Offset.zero,
            end: new Offset(0.0f, -0.025f)
        );

        static readonly FloatTween _scrimOpacityTween = new FloatTween(
            begin: 0.0f,
            end: 0.25f
        );

        static readonly Curve _transitionCurve = new Cubic(0.20f, 0.00f, 0.00f, 1.00f);

        public readonly Animation<float> animation;
        public readonly Animation<float> secondaryAnimation;
        public readonly Widget child;

        public override Widget build(BuildContext context) {
            return new LayoutBuilder(
                builder: (BuildContext _context, BoxConstraints constraints) => {
                    Size size = constraints.biggest;

                    CurvedAnimation primaryAnimation = new CurvedAnimation(
                        parent: this.animation,
                        curve: _transitionCurve,
                        reverseCurve: _transitionCurve.flipped
                    );

                    Animation<float> clipAnimation = new FloatTween(
                        begin: 0.0f,
                        end: size.height
                    ).animate(primaryAnimation);

                    Animation<float> opacityAnimation = _scrimOpacityTween.animate(primaryAnimation);
                    Animation<Offset> primaryTranslationAnimation = _primaryTranslationTween.animate(primaryAnimation);

                    Animation<Offset> secondaryTranslationAnimation = _secondaryTranslationTween.animate(
                        new CurvedAnimation(
                            parent: this.secondaryAnimation,
                            curve: _transitionCurve,
                            reverseCurve: _transitionCurve.flipped
                        )
                    );

                    return new AnimatedBuilder(
                        animation: this.animation,
                        builder: (BuildContext _, Widget child) => {
                            return new Container(
                                color: Colors.black.withOpacity(opacityAnimation.value),
                                alignment: Alignment.bottomLeft,
                                child: new ClipRect(
                                    child: new SizedBox(
                                        height: clipAnimation.value,
                                        child: new OverflowBox(
                                            alignment: Alignment.bottomLeft,
                                            maxHeight: size.height,
                                            child: child
                                        )
                                    )
                                )
                            );
                        },
                        child: new AnimatedBuilder(
                            animation: this.secondaryAnimation,
                            child: new FractionalTranslation(
                                translation: primaryTranslationAnimation.value,
                                child: this.child
                            ),
                            builder: (BuildContext _, Widget child) => {
                                return new FractionalTranslation(
                                    translation: secondaryTranslationAnimation.value,
                                    child: child
                                );
                            }
                        )
                    );
                }
            );
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

    public class OpenUpwardsPageTransitionsBuilder : PageTransitionsBuilder {
        public OpenUpwardsPageTransitionsBuilder() {
        }

        public override Widget buildTransitions(
            PageRoute route,
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation,
            Widget child
        ) {
            return new _OpenUpwardsPageTransition(
                animation: animation,
                secondaryAnimation: secondaryAnimation,
                child: child
            );
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

        PageTransitionsBuilder _all(PageTransitionsBuilder builder) {
            return builder;
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