using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public delegate Tween<Rect> CreateRectTween(Rect begin, Rect end);

    public delegate Widget HeroFlightShuttleBuilder(
        BuildContext flightContext,
        Animation<float> animation,
        HeroFlightDirection flightDirection,
        BuildContext fromHeroContext,
        BuildContext toHeroContext
    );

    delegate void _OnFlightEnded(_HeroFlight flight);

    public enum HeroFlightDirection {
        push,
        pop
    }

    class HeroUtils {
        public static Rect _globalBoundingBoxFor(BuildContext context) {
            RenderBox box = (RenderBox) context.findRenderObject();
            D.assert(box != null && box.hasSize);
            return box.getTransformTo(null).mapRect(Offset.zero & box.size);
        }
    }


    public class Hero : StatefulWidget {
        public Hero(
            Key key = null,
            object tag = null,
            CreateRectTween createRectTween = null,
            HeroFlightShuttleBuilder flightShuttleBuilder = null,
            TransitionBuilder placeholderBuilder = null,
            bool transitionOnUserGestures = false,
            Widget child = null
        ) : base(key: key) {
            D.assert(tag != null);
            D.assert(child != null);
            this.tag = tag;
            this.createRectTween = createRectTween;
            this.child = child;
            this.flightShuttleBuilder = flightShuttleBuilder;
            this.placeholderBuilder = placeholderBuilder;
            this.transitionOnUserGestures = transitionOnUserGestures;
        }


        public readonly object tag;

        public readonly CreateRectTween createRectTween;

        public readonly Widget child;

        public readonly HeroFlightShuttleBuilder flightShuttleBuilder;

        public readonly TransitionBuilder placeholderBuilder;

        public readonly bool transitionOnUserGestures;

        internal static Dictionary<object, _HeroState>
            _allHeroesFor(BuildContext context, bool isUserGestureTransition, NavigatorState navigator) {
            D.assert(context != null);
            D.assert(navigator != null);
            Dictionary<object, _HeroState> result = new Dictionary<object, _HeroState> { };

            void addHero(StatefulElement hero, object tag) {
                D.assert(() => {
                    if (result.ContainsKey(tag)) {
                        throw new UIWidgetsError(
                            "There are multiple heroes that share the same tag within a subtree.\n" +
                            "Within each subtree for which heroes are to be animated (typically a PageRoute subtree), " +
                            "each Hero must have a unique non-null tag.\n" +
                            $"In this case, multiple heroes had the following tag: {tag}\n" +
                            "Here is the subtree for one of the offending heroes:\n" +
                            $"{hero.toStringDeep(prefixLineOne: "# ")}"
                        );
                    }

                    return true;
                });
                _HeroState heroState = (_HeroState) hero.state;
                result[tag] = heroState;
            }

            void visitor(Element element) {
                if (element.widget is Hero) {
                    StatefulElement hero = (StatefulElement) element;
                    Hero heroWidget = (Hero) element.widget;
                    if (!isUserGestureTransition || heroWidget.transitionOnUserGestures) {
                        object tag = heroWidget.tag;
                        D.assert(tag != null);
                        if (Navigator.of(hero) == navigator) {
                            addHero(hero, tag);
                        }
                        else {
                            ModalRoute heroRoute = ModalRoute.of(hero);
                            if (heroRoute != null && heroRoute is PageRoute && heroRoute.isCurrent) {
                                addHero(hero, tag);
                            }
                        }
                    }
                }

                element.visitChildren(visitor);
            }

            context.visitChildElements(visitor);
            return result;
        }

        public override State createState() {
            return new _HeroState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<object>("tag", this.tag));
        }
    }

    class _HeroState : State<Hero> {
        GlobalKey _key = GlobalKey.key();
        Size _placeholderSize;

        public void startFlight() {
            D.assert(this.mounted);
            RenderBox box = (RenderBox) this.context.findRenderObject();
            D.assert(box != null && box.hasSize);
            this.setState(() => { this._placeholderSize = box.size; });
        }

        public void endFlight() {
            if (this.mounted) {
                this.setState(() => { this._placeholderSize = null; });
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(context.ancestorWidgetOfExactType(typeof(Hero)) == null,
                () => "A Hero widget cannot be the descendant of another Hero widget.");
            if (this._placeholderSize != null) {
                if (this.widget.placeholderBuilder == null) {
                    return new SizedBox(
                        width: this._placeholderSize.width,
                        height: this._placeholderSize.height
                    );
                }
                else {
                    return this.widget.placeholderBuilder(context, this.widget.child);
                }
            }

            return new KeyedSubtree(
                key: this._key,
                child: this.widget.child
            );
        }
    }

    class _HeroFlightManifest {
        public _HeroFlightManifest(
            HeroFlightDirection type,
            OverlayState overlay,
            Rect navigatorRect,
            PageRoute fromRoute,
            PageRoute toRoute,
            _HeroState fromHero,
            _HeroState toHero,
            CreateRectTween createRectTween,
            HeroFlightShuttleBuilder shuttleBuilder,
            bool isUserGestureTransition
        ) {
            D.assert(fromHero.widget.tag.Equals(toHero.widget.tag));
            this.type = type;
            this.overlay = overlay;
            this.navigatorRect = navigatorRect;
            this.fromRoute = fromRoute;
            this.toRoute = toRoute;
            this.fromHero = fromHero;
            this.toHero = toHero;
            this.createRectTween = createRectTween;
            this.shuttleBuilder = shuttleBuilder;
            this.isUserGestureTransition = isUserGestureTransition;
        }

        public readonly HeroFlightDirection type;
        public readonly OverlayState overlay;
        public readonly Rect navigatorRect;
        public readonly PageRoute fromRoute;
        public readonly PageRoute toRoute;
        public readonly _HeroState fromHero;
        public readonly _HeroState toHero;
        public readonly CreateRectTween createRectTween;
        public readonly HeroFlightShuttleBuilder shuttleBuilder;
        public readonly bool isUserGestureTransition;

        public object tag {
            get { return this.fromHero.widget.tag; }
        }

        public Animation<float> animation {
            get {
                return new CurvedAnimation(
                    parent: (this.type == HeroFlightDirection.push) ? this.toRoute.animation : this.fromRoute.animation,
                    curve: Curves.fastOutSlowIn
                );
            }
        }

        public override string ToString() {
            return $"_HeroFlightManifest($type tag: $tag from route: {this.fromRoute.settings} " +
                   $"to route: {this.toRoute.settings} with hero: {this.fromHero} to {this.toHero})";
        }
    }

    class _HeroFlight {
        public _HeroFlight(_OnFlightEnded onFlightEnded) {
            this.onFlightEnded = onFlightEnded;
            this._proxyAnimation = new ProxyAnimation();
            this._proxyAnimation.addStatusListener(this._handleAnimationUpdate);
        }

        public readonly _OnFlightEnded onFlightEnded;

        Tween<Rect> heroRectTween;
        Widget shuttle;

        Animation<float> _heroOpacity = Animations.kAlwaysCompleteAnimation;
        ProxyAnimation _proxyAnimation;
        public _HeroFlightManifest manifest;
        public OverlayEntry overlayEntry;
        bool _aborted = false;

        Tween<Rect> _doCreateRectTween(Rect begin, Rect end) {
            CreateRectTween createRectTween =
                this.manifest.toHero.widget.createRectTween ?? this.manifest.createRectTween;
            if (createRectTween != null) {
                return createRectTween(begin, end);
            }

            return new RectTween(begin: begin, end: end);
        }

        static readonly Animatable<float> _reverseTween = new FloatTween(begin: 1.0f, end: 0.0f);

        Widget _buildOverlay(BuildContext context) {
            D.assert(this.manifest != null);
            this.shuttle = this.shuttle ?? this.manifest.shuttleBuilder(
                               context, this.manifest.animation, this.manifest.type, this.manifest.fromHero.context,
                               this.manifest.toHero.context
                           );
            D.assert(this.shuttle != null);

            return new AnimatedBuilder(
                animation: this._proxyAnimation,
                child: this.shuttle,
                builder: (BuildContext _, Widget child) => {
                    RenderBox toHeroBox = (RenderBox) this.manifest.toHero.context?.findRenderObject();
                    if (this._aborted || toHeroBox == null || !toHeroBox.attached) {
                        if (this._heroOpacity.isCompleted) {
                            this._heroOpacity = this._proxyAnimation.drive(
                                _reverseTween.chain(
                                    new CurveTween(curve: new Interval(this._proxyAnimation.value, 1.0f)))
                            );
                        }
                    }
                    else if (toHeroBox.hasSize) {
                        RenderBox finalRouteBox = (RenderBox) this.manifest.toRoute.subtreeContext?.findRenderObject();
                        Offset toHeroOrigin = toHeroBox.localToGlobal(Offset.zero, ancestor: finalRouteBox);
                        if (toHeroOrigin != this.heroRectTween.end.topLeft) {
                            Rect heroRectEnd = toHeroOrigin & this.heroRectTween.end.size;
                            this.heroRectTween = this._doCreateRectTween(this.heroRectTween.begin, heroRectEnd);
                        }
                    }

                    Rect rect = this.heroRectTween.evaluate(this._proxyAnimation);
                    Size size = this.manifest.navigatorRect.size;
                    RelativeRect offsets = RelativeRect.fromSize(rect, size);

                    return new Positioned(
                        top: offsets.top,
                        right: offsets.right,
                        bottom: offsets.bottom,
                        left: offsets.left,
                        child: new IgnorePointer(
                            child: new RepaintBoundary(
                                child: new Opacity(
                                    opacity: this._heroOpacity.value,
                                    child: child
                                )
                            )
                        )
                    );
                }
            );
        }

        void _handleAnimationUpdate(AnimationStatus status) {
            if (status == AnimationStatus.completed || status == AnimationStatus.dismissed) {
                this._proxyAnimation.parent = null;

                D.assert(this.overlayEntry != null);
                this.overlayEntry.remove();
                this.overlayEntry = null;

                this.manifest.fromHero.endFlight();
                this.manifest.toHero.endFlight();
                this.onFlightEnded(this);
            }
        }

        public void start(_HeroFlightManifest initialManifest) {
            D.assert(!this._aborted);
            D.assert(() => {
                Animation<float> initial = initialManifest.animation;
                D.assert(initial != null);
                HeroFlightDirection type = initialManifest.type;
                switch (type) {
                    case HeroFlightDirection.pop:
                        return initial.value == 1.0f && initialManifest.isUserGestureTransition
                            ? initial.status == AnimationStatus.completed
                            : initial.status == AnimationStatus.reverse;
                    case HeroFlightDirection.push:
                        return initial.value == 0.0f && initial.status == AnimationStatus.forward;
                }

                throw new Exception("Unknown type: " + type);
            });

            this.manifest = initialManifest;

            if (this.manifest.type == HeroFlightDirection.pop) {
                this._proxyAnimation.parent = new ReverseAnimation(this.manifest.animation);
            }
            else {
                this._proxyAnimation.parent = this.manifest.animation;
            }

            this.manifest.fromHero.startFlight();
            this.manifest.toHero.startFlight();

            this.heroRectTween = this._doCreateRectTween(
                HeroUtils._globalBoundingBoxFor(this.manifest.fromHero.context),
                HeroUtils._globalBoundingBoxFor(this.manifest.toHero.context)
            );

            this.overlayEntry = new OverlayEntry(builder: this._buildOverlay);
            this.manifest.overlay.insert(this.overlayEntry);
        }

        public void divert(_HeroFlightManifest newManifest) {
            D.assert(this.manifest.tag == newManifest.tag);

            if (this.manifest.type == HeroFlightDirection.push && newManifest.type == HeroFlightDirection.pop) {
                D.assert(newManifest.animation.status == AnimationStatus.reverse);
                D.assert(this.manifest.fromHero == newManifest.toHero);
                D.assert(this.manifest.toHero == newManifest.fromHero);
                D.assert(this.manifest.fromRoute == newManifest.toRoute);
                D.assert(this.manifest.toRoute == newManifest.fromRoute);

                this._proxyAnimation.parent = new ReverseAnimation(newManifest.animation);
                this.heroRectTween = new ReverseTween<Rect>(this.heroRectTween);
            }
            else if (this.manifest.type == HeroFlightDirection.pop && newManifest.type == HeroFlightDirection.push) {
                D.assert(newManifest.animation.status == AnimationStatus.forward);
                D.assert(this.manifest.toHero == newManifest.fromHero);
                D.assert(this.manifest.toRoute == newManifest.fromRoute);

                this._proxyAnimation.parent = newManifest.animation.drive(
                    new FloatTween(
                        begin: this.manifest.animation.value,
                        end: 1.0f
                    )
                );

                if (this.manifest.fromHero != newManifest.toHero) {
                    this.manifest.fromHero.endFlight();
                    newManifest.toHero.startFlight();
                    this.heroRectTween = this._doCreateRectTween(this.heroRectTween.end,
                        HeroUtils._globalBoundingBoxFor(newManifest.toHero.context));
                }
                else {
                    this.heroRectTween = this._doCreateRectTween(this.heroRectTween.end, this.heroRectTween.begin);
                }
            }
            else {
                D.assert(this.manifest.fromHero != newManifest.fromHero);
                D.assert(this.manifest.toHero != newManifest.toHero);

                this.heroRectTween = this._doCreateRectTween(this.heroRectTween.evaluate(this._proxyAnimation),
                    HeroUtils._globalBoundingBoxFor(newManifest.toHero.context));
                this.shuttle = null;

                if (newManifest.type == HeroFlightDirection.pop) {
                    this._proxyAnimation.parent = new ReverseAnimation(newManifest.animation);
                }
                else {
                    this._proxyAnimation.parent = newManifest.animation;
                }

                this.manifest.fromHero.endFlight();
                this.manifest.toHero.endFlight();

                newManifest.fromHero.startFlight();
                newManifest.toHero.startFlight();

                this.overlayEntry.markNeedsBuild();
            }

            this._aborted = false;
            this.manifest = newManifest;
        }

        public void abort() {
            this._aborted = true;
        }

        public override string ToString() {
            RouteSettings from = this.manifest.fromRoute.settings;
            RouteSettings to = this.manifest.toRoute.settings;
            object tag = this.manifest.tag;
            return "HeroFlight(for: $tag, from: $from, to: $to ${_proxyAnimation.parent})";
        }
    }

    public class HeroController : NavigatorObserver {
        public HeroController(CreateRectTween createRectTween = null) {
            this.createRectTween = createRectTween;
        }

        public readonly CreateRectTween createRectTween;

        Dictionary<object, _HeroFlight> _flights = new Dictionary<object, _HeroFlight>();

        public override void didPush(Route route, Route previousRoute) {
            D.assert(this.navigator != null);
            D.assert(route != null);
            this._maybeStartHeroTransition(previousRoute, route, HeroFlightDirection.push, false);
        }

        public override void didPop(Route route, Route previousRoute) {
            D.assert(this.navigator != null);
            D.assert(route != null);
            if (!this.navigator.userGestureInProgress) {
                this._maybeStartHeroTransition(route, previousRoute, HeroFlightDirection.pop, false);
            }
        }

        public override void didReplace(Route newRoute = null, Route oldRoute = null) {
            D.assert(this.navigator != null);
            if (newRoute?.isCurrent == true) {
                this._maybeStartHeroTransition(oldRoute, newRoute, HeroFlightDirection.push, false);
            }
        }

        public override void didStartUserGesture(Route route, Route previousRoute) {
            D.assert(this.navigator != null);
            D.assert(route != null);
            this._maybeStartHeroTransition(route, previousRoute, HeroFlightDirection.pop, true);
        }

        void _maybeStartHeroTransition(
            Route fromRoute,
            Route toRoute,
            HeroFlightDirection flightType,
            bool isUserGestureTransition
        ) {
            if (toRoute != fromRoute && toRoute is PageRoute && fromRoute is PageRoute) {
                PageRoute from = (PageRoute) fromRoute;
                PageRoute to = (PageRoute) toRoute;
                Animation<float> animation = (flightType == HeroFlightDirection.push) ? to.animation : from.animation;

                switch (flightType) {
                    case HeroFlightDirection.pop:
                        if (animation.value == 0.0f) {
                            return;
                        }

                        break;
                    case HeroFlightDirection.push:
                        if (animation.value == 1.0f) {
                            return;
                        }

                        break;
                }

                if (isUserGestureTransition && flightType == HeroFlightDirection.pop && to.maintainState) {
                    this._startHeroTransition(from, to, animation, flightType, isUserGestureTransition);
                }
                else {
                    to.offstage = to.animation.value == 0.0f;

                    WidgetsBinding.instance.addPostFrameCallback((TimeSpan value) => {
                        this._startHeroTransition(from, to, animation, flightType, isUserGestureTransition);
                    });
                }
            }
        }

        void _startHeroTransition(
            PageRoute from,
            PageRoute to,
            Animation<float> animation,
            HeroFlightDirection flightType,
            bool isUserGestureTransition
        ) {
            if (this.navigator == null || from.subtreeContext == null || to.subtreeContext == null) {
                to.offstage = false; // in case we set this in _maybeStartHeroTransition
                return;
            }

            Rect navigatorRect = HeroUtils._globalBoundingBoxFor(this.navigator.context);

            Dictionary<object, _HeroState> fromHeroes =
                Hero._allHeroesFor(from.subtreeContext, isUserGestureTransition, this.navigator);
            Dictionary<object, _HeroState> toHeroes =
                Hero._allHeroesFor(to.subtreeContext, isUserGestureTransition, this.navigator);

            to.offstage = false;

            foreach (object tag in fromHeroes.Keys) {
                if (toHeroes.ContainsKey(tag)) {
                    HeroFlightShuttleBuilder fromShuttleBuilder = fromHeroes[tag].widget.flightShuttleBuilder;
                    HeroFlightShuttleBuilder toShuttleBuilder = toHeroes[tag].widget.flightShuttleBuilder;

                    _HeroFlightManifest manifest = new _HeroFlightManifest(
                        type: flightType,
                        overlay: this.navigator.overlay,
                        navigatorRect: navigatorRect,
                        fromRoute: from,
                        toRoute: to,
                        fromHero: fromHeroes[tag],
                        toHero: toHeroes[tag],
                        createRectTween: this.createRectTween,
                        shuttleBuilder:
                        toShuttleBuilder ?? fromShuttleBuilder ?? _defaultHeroFlightShuttleBuilder,
                        isUserGestureTransition: isUserGestureTransition
                    );

                    if (this._flights.TryGetValue(tag, out var result)) {
                        result.divert(manifest);
                    }
                    else {
                        this._flights[tag] = new _HeroFlight(this._handleFlightEnded);
                        this._flights[tag].start(manifest);
                    }
                }
                else if (this._flights.TryGetValue(tag, out var result)) {
                    result.abort();
                }
            }
        }

        void _handleFlightEnded(_HeroFlight flight) {
            this._flights.Remove(flight.manifest.tag);
        }

        static readonly HeroFlightShuttleBuilder _defaultHeroFlightShuttleBuilder = (
            BuildContext flightContext,
            Animation<float> animation,
            HeroFlightDirection flightDirection,
            BuildContext fromHeroContext,
            BuildContext toHeroContext
        ) => {
            Hero toHero = (Hero) toHeroContext.widget;
            return toHero.child;
        };
    }
}
