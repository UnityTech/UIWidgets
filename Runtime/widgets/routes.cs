using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public abstract class OverlayRoute : Route {
        readonly List<OverlayEntry> _overlayEntries = new List<OverlayEntry>();

        public OverlayRoute(
            RouteSettings settings = null
        ) : base(settings) {
        }

        public override List<OverlayEntry> overlayEntries {
            get { return this._overlayEntries; }
        }

        protected virtual bool finishedWhenPopped {
            get { return true; }
        }

        public abstract ICollection<OverlayEntry> createOverlayEntries();

        protected internal override void install(OverlayEntry insertionPoint) {
            D.assert(this._overlayEntries.isEmpty());
            this._overlayEntries.AddRange(this.createOverlayEntries());
            this.navigator.overlay?.insertAll(this._overlayEntries, above: insertionPoint);
            base.install(insertionPoint);
        }

        protected internal override bool didPop(object result) {
            var returnValue = base.didPop(result);
            D.assert(returnValue);
            if (this.finishedWhenPopped) {
                this.navigator.finalizeRoute(this);
            }

            return returnValue;
        }

        protected internal override void dispose() {
            foreach (var entry in this._overlayEntries) {
                entry.remove();
            }

            this._overlayEntries.Clear();
            base.dispose();
        }
    }

    public abstract class TransitionRoute : OverlayRoute {
        public TransitionRoute(
            RouteSettings settings = null
        ) : base(settings) {
        }

        public IPromise<object> completed {
            get { return this._transitionCompleter; }
        }

        internal readonly Promise<object> _transitionCompleter = new Promise<object>();

        public virtual TimeSpan transitionDuration { get; }

        public virtual bool opaque { get; }


        protected override bool finishedWhenPopped {
            get { return this.controller.status == AnimationStatus.dismissed; }
        }

        public virtual Animation<float> animation {
            get { return this._animation; }
        }

        internal Animation<float> _animation;

        public AnimationController controller {
            get { return this._controller; }
        }

        internal AnimationController _controller;

        public virtual AnimationController createAnimationController() {
            D.assert(this._transitionCompleter.CurState == PromiseState.Pending,
                () => $"Cannot reuse a {this.GetType()} after disposing it.");
            TimeSpan duration = this.transitionDuration;
            D.assert(duration >= TimeSpan.Zero);
            return new AnimationController(
                duration: duration,
                debugLabel: this.debugLabel,
                vsync: this.navigator
            );
        }

        public virtual Animation<float> createAnimation() {
            D.assert(this._transitionCompleter.CurState == PromiseState.Pending,
                () => $"Cannot reuse a {this.GetType()} after disposing it.");
            D.assert(this._controller != null);
            return this._controller.view;
        }

        object _result;

        internal void _handleStatusChanged(AnimationStatus status) {
            switch (status) {
                case AnimationStatus.completed:
                    if (this.overlayEntries.isNotEmpty()) {
                        this.overlayEntries.first().opaque = this.opaque;
                    }

                    break;
                case AnimationStatus.forward:
                case AnimationStatus.reverse:
                    if (this.overlayEntries.isNotEmpty()) {
                        this.overlayEntries.first().opaque = false;
                    }

                    break;
                case AnimationStatus.dismissed:
                    // We might still be an active route if a subclass is controlling the
                    // the transition and hits the dismissed status. For example, the iOS
                    // back gesture drives this animation to the dismissed status before
                    // popping the navigator.
                    if (!this.isActive) {
                        this.navigator.finalizeRoute(this);
                        D.assert(this.overlayEntries.isEmpty());
                    }

                    break;
            }

            this.changedInternalState();
        }

        public virtual Animation<float> secondaryAnimation {
            get { return this._secondaryAnimation; }
        }

        readonly ProxyAnimation _secondaryAnimation = new ProxyAnimation(Animations.kAlwaysDismissedAnimation);

        protected internal override void install(OverlayEntry insertionPoint) {
            D.assert(!this._transitionCompleter.isCompleted, () => $"Cannot install a {this.GetType()} after disposing it.");
            this._controller = this.createAnimationController();
            D.assert(this._controller != null, () => $"{this.GetType()}.createAnimationController() returned null.");
            this._animation = this.createAnimation();
            D.assert(this._animation != null, () => $"{this.GetType()}.createAnimation() returned null.");
            base.install(insertionPoint);
        }

        protected internal override TickerFuture didPush() {
            D.assert(this._controller != null,
                () => $"{this.GetType()}.didPush called before calling install() or after calling dispose().");
            D.assert(!this._transitionCompleter.isCompleted, () => $"Cannot reuse a {this.GetType()} after disposing it.");
            this._animation.addStatusListener(this._handleStatusChanged);
            return this._controller.forward();
        }

        protected internal override void didReplace(Route oldRoute) {
            D.assert(this._controller != null,
                () => $"{this.GetType()}.didReplace called before calling install() or after calling dispose().");
            D.assert(!this._transitionCompleter.isCompleted, () => $"Cannot reuse a {this.GetType()} after disposing it.");
            if (oldRoute is TransitionRoute route) {
                this._controller.setValue(route._controller.value);
            }

            this._animation.addStatusListener(this._handleStatusChanged);
            base.didReplace(oldRoute);
        }

        protected internal override bool didPop(object result) {
            D.assert(this._controller != null,
                () => $"{this.GetType()}.didPop called before calling install() or after calling dispose().");
            D.assert(!this._transitionCompleter.isCompleted, () => $"Cannot reuse a {this.GetType()} after disposing it.");
            this._result = result;
            this._controller.reverse();
            return base.didPop(result);
        }

        protected internal override void didPopNext(Route nextRoute) {
            D.assert(this._controller != null,
                () => $"{this.GetType()}.didPopNext called before calling install() or after calling dispose().");
            D.assert(!this._transitionCompleter.isCompleted, () => $"Cannot reuse a {this.GetType()} after disposing it.");
            this._updateSecondaryAnimation(nextRoute);
            base.didPopNext(nextRoute);
        }

        protected internal override void didChangeNext(Route nextRoute) {
            D.assert(this._controller != null,
                () => $"{this.GetType()}.didChangeNext called before calling install() or after calling dispose().");
            D.assert(!this._transitionCompleter.isCompleted, () => $"Cannot reuse a {this.GetType()} after disposing it.");
            this._updateSecondaryAnimation(nextRoute);
            base.didChangeNext(nextRoute);
        }

        void _updateSecondaryAnimation(Route nextRoute) {
            if (nextRoute is TransitionRoute && this.canTransitionTo((TransitionRoute) nextRoute) &&
                ((TransitionRoute) nextRoute).canTransitionFrom(this)) {
                Animation<float> current = this._secondaryAnimation.parent;
                if (current != null) {
                    if (current is TrainHoppingAnimation) {
                        TrainHoppingAnimation newAnimation = null;
                        newAnimation = new TrainHoppingAnimation(
                            ((TrainHoppingAnimation) current).currentTrain,
                            ((TransitionRoute) nextRoute)._animation,
                            onSwitchedTrain: () => {
                                D.assert(this._secondaryAnimation.parent == newAnimation);
                                D.assert(newAnimation.currentTrain == ((TransitionRoute) nextRoute)._animation);
                                this._secondaryAnimation.parent = newAnimation.currentTrain;
                                newAnimation.dispose();
                            }
                        );
                        this._secondaryAnimation.parent = newAnimation;
                        ((TrainHoppingAnimation) current).dispose();
                    }
                    else {
                        this._secondaryAnimation.parent =
                            new TrainHoppingAnimation(current, ((TransitionRoute) nextRoute)._animation);
                    }
                }
                else {
                    this._secondaryAnimation.parent = ((TransitionRoute) nextRoute)._animation;
                }
            }
            else {
                this._secondaryAnimation.parent = Animations.kAlwaysDismissedAnimation;
            }
        }


        public virtual bool canTransitionTo(TransitionRoute nextRoute) {
            return true;
        }

        public virtual bool canTransitionFrom(TransitionRoute previousRoute) {
            return true;
        }

        protected internal override void dispose() {
            D.assert(!this._transitionCompleter.isCompleted, () => $"Cannot dispose a {this.GetType()} twice.");
            this._controller?.dispose();
            this._transitionCompleter.Resolve(this._result);
            base.dispose();
        }

        public string debugLabel {
            get { return $"{this.GetType()}"; }
        }

        public override string ToString() {
            return $"{this.GetType()}(animation: {this._controller}";
        }
    }

    public class LocalHistoryEntry {
        public LocalHistoryEntry(VoidCallback onRemove = null) {
            this.onRemove = onRemove;
        }

        public readonly VoidCallback onRemove;

        internal LocalHistoryRoute _owner;

        public void remove() {
            this._owner.removeLocalHistoryEntry(this);
            D.assert(this._owner == null);
        }

        internal void _notifyRemoved() {
            this.onRemove?.Invoke();
        }
    }


    public interface LocalHistoryRoute {
        void addLocalHistoryEntry(LocalHistoryEntry entry);
        void removeLocalHistoryEntry(LocalHistoryEntry entry);

        Route route { get; }
    }

    // todo make it to mixin
    public abstract class LocalHistoryRouteTransitionRoute : TransitionRoute, LocalHistoryRoute {
        List<LocalHistoryEntry> _localHistory;

        protected LocalHistoryRouteTransitionRoute(RouteSettings settings = null) : base(settings: settings) {
        }

        public void addLocalHistoryEntry(LocalHistoryEntry entry) {
            D.assert(entry._owner == null);
            entry._owner = this;
            this._localHistory = this._localHistory ?? new List<LocalHistoryEntry>();
            var wasEmpty = this._localHistory.isEmpty();
            this._localHistory.Add(entry);
            if (wasEmpty) {
                this.changedInternalState();
            }
        }

        public void removeLocalHistoryEntry(LocalHistoryEntry entry) {
            D.assert(entry != null);
            D.assert(entry._owner == this);
            D.assert(this._localHistory.Contains(entry));
            this._localHistory.Remove(entry);
            entry._owner = null;
            entry._notifyRemoved();
            if (this._localHistory.isEmpty()) {
                this.changedInternalState();
            }
        }

        public override IPromise<RoutePopDisposition> willPop() {
            if (this.willHandlePopInternally) {
                return Promise<RoutePopDisposition>.Resolved(RoutePopDisposition.pop);
            }

            return base.willPop();
        }

        protected internal override bool didPop(object result) {
            if (this._localHistory != null && this._localHistory.isNotEmpty()) {
                var entry = this._localHistory.removeLast();
                D.assert(entry._owner == this);
                entry._owner = null;
                entry._notifyRemoved();
                if (this._localHistory.isEmpty()) {
                    this.changedInternalState();
                }

                return false;
            }

            return base.didPop(result);
        }

        public override bool willHandlePopInternally {
            get { return this._localHistory != null && this._localHistory.isNotEmpty(); }
        }

        public Route route {
            get { return this; }
        }
    }


    public class _ModalScopeStatus : InheritedWidget {
        public _ModalScopeStatus(Key key = null, bool isCurrent = false,
            bool canPop = false, Route route = null, Widget child = null) : base(key: key, child: child) {
            D.assert(route != null);
            D.assert(child != null);

            this.isCurrent = isCurrent;
            this.canPop = canPop;
            this.route = route;
        }

        public readonly bool isCurrent;
        public readonly bool canPop;
        public readonly Route route;

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return this.isCurrent != ((_ModalScopeStatus) oldWidget).isCurrent ||
                   this.canPop != ((_ModalScopeStatus) oldWidget).canPop ||
                   this.route != ((_ModalScopeStatus) oldWidget).route;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder description) {
            base.debugFillProperties(description);
            description.add(new FlagProperty("isCurrent", value: this.isCurrent, ifTrue: "active",
                ifFalse: "inactive"));
            description.add(new FlagProperty("canPop", value: this.canPop, ifTrue: "can pop"));
        }
    }

    public class _ModalScope : StatefulWidget {
        public _ModalScope(Key key = null, ModalRoute route = null) : base(key) {
            this.route = route;
        }

        public readonly ModalRoute route;

        public override State createState() {
            return new _ModalScopeState();
        }
    }

    public class _ModalScopeState : State<_ModalScope> {
        Widget _page;

        Listenable _listenable;

        public override void initState() {
            base.initState();
            var animations = new List<Listenable> { };
            if (this.widget.route.animation != null) {
                animations.Add(this.widget.route.animation);
            }

            if (this.widget.route.secondaryAnimation != null) {
                animations.Add(this.widget.route.secondaryAnimation);
            }

            this._listenable = ListenableUtils.merge(animations);
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            D.assert(this.widget.route == ((_ModalScope) oldWidget).route);
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            this._page = null;
        }

        internal void _forceRebuildPage() {
            this.setState(() => { this._page = null; });
        }

        internal void _routeSetState(VoidCallback fn) {
            this.setState(fn);
        }

        public override Widget build(BuildContext context) {
            this._page = this._page ?? new RepaintBoundary(
                             key: this.widget.route._subtreeKey, // immutable
                             child: new Builder(
                                 builder: (BuildContext _context) => this.widget.route.buildPage(
                                     _context,
                                     this.widget.route.animation,
                                     this.widget.route.secondaryAnimation
                                 ))
                         );

            return new _ModalScopeStatus(
                route: this.widget.route,
                isCurrent: this.widget.route.isCurrent,
                canPop: this.widget.route.canPop,
                child: new Offstage(
                    offstage: this.widget.route.offstage,
                    child: new PageStorage(
                        bucket: this.widget.route._storageBucket,
                        child: new FocusScope(
                            node: this.widget.route.focusScopeNode,
                            child: new RepaintBoundary(
                                child: new AnimatedBuilder(
                                    animation: this._listenable, // immutable
                                    builder: (BuildContext _context, Widget child) =>
                                        this.widget.route.buildTransitions(
                                            _context,
                                            this.widget.route.animation,
                                            this.widget.route.secondaryAnimation,
                                            new IgnorePointer(
                                                ignoring: this.widget.route.animation?.status ==
                                                          AnimationStatus.reverse,
                                                child: child
                                            )
                                        ),
                                    child: this._page
                                )
                            )
                        )
                    )
                )
            );
        }
    }

    public abstract class ModalRoute : LocalHistoryRouteTransitionRoute {
        
        protected ModalRoute() {}
        protected ModalRoute(RouteSettings settings) : base(settings) { }

        public static Color _kTransparent = new Color(0x00000000);

        public static ModalRoute of(BuildContext context) {
            _ModalScopeStatus widget =
                (_ModalScopeStatus) context.inheritFromWidgetOfExactType(typeof(_ModalScopeStatus));
            return (ModalRoute) widget?.route;
        }

        protected virtual void setState(VoidCallback fn) {
            if (this._scopeKey.currentState != null) {
                this._scopeKey.currentState._routeSetState(fn);
            }
            else {
                fn();
            }
        }

        public RoutePredicate withName(string name) {
            return (Route route) => !route.willHandlePopInternally
                                    && route is ModalRoute
                                    && route.settings.name == name;
        }

        public abstract Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation);

        public virtual Widget buildTransitions(
            BuildContext context,
            Animation<float> animation,
            Animation<float> secondaryAnimation,
            Widget child
        ) {
            return child;
        }

        public readonly FocusScopeNode focusScopeNode = new FocusScopeNode();

        protected internal override void install(OverlayEntry insertionPoint) {
            base.install(insertionPoint);
            this._animationProxy = new ProxyAnimation(base.animation);
            this._secondaryAnimationProxy = new ProxyAnimation(base.secondaryAnimation);
        }

        protected internal override TickerFuture didPush() {
            this.navigator.focusScopeNode.setFirstFocus(this.focusScopeNode);
            return base.didPush();
        }

        protected internal override void dispose() {
            this.focusScopeNode.detach();
            base.dispose();
        }

        public virtual bool barrierDismissible { get; }

        public virtual Color barrierColor { get; }

        public virtual bool maintainState { get; }

        public bool offstage {
            get { return this._offstage; }
            set {
                if (this._offstage == value) {
                    return;
                }

                this.setState(() => { this._offstage = value; });
                this._animationProxy.parent = this._offstage ? Animations.kAlwaysCompleteAnimation : base.animation;
                this._secondaryAnimationProxy.parent =
                    this._offstage ? Animations.kAlwaysDismissedAnimation : base.secondaryAnimation;
            }
        }

        bool _offstage = false;

        public BuildContext subtreeContext {
            get { return this._subtreeKey.currentContext; }
        }

        public override Animation<float> animation {
            get { return this._animationProxy; }
        }

        ProxyAnimation _animationProxy;

        public override Animation<float> secondaryAnimation {
            get { return this._secondaryAnimationProxy; }
        }

        ProxyAnimation _secondaryAnimationProxy;

        readonly List<WillPopCallback> _willPopCallbacks = new List<WillPopCallback>();

        public override IPromise<RoutePopDisposition> willPop() {
            _ModalScopeState scope = this._scopeKey.currentState;
            D.assert(scope != null);

            var callbacks = new List<WillPopCallback>(this._willPopCallbacks);
            Promise<RoutePopDisposition> result = new Promise<RoutePopDisposition>();
            Action<int> fn = null;
            fn = (int index) => {
                if (index < callbacks.Count) {
                    callbacks[index]().Then((pop) => {
                        if (!pop) {
                            result.Resolve(RoutePopDisposition.doNotPop);
                        }
                        else {
                            fn(index + 1);
                        }
                    });
                }
                else {
                    base.willPop().Then((pop) => result.Resolve(pop));
                }
            };
            fn(0);
            return result;
        }

        public void addScopedWillPopCallback(WillPopCallback callback) {
            D.assert(this._scopeKey.currentState != null,
                () => "Tried to add a willPop callback to a route that is not currently in the tree.");
            this._willPopCallbacks.Add(callback);
        }

        public void removeScopedWillPopCallback(WillPopCallback callback) {
            D.assert(this._scopeKey.currentState != null,
                () => "Tried to remove a willPop callback from a route that is not currently in the tree.");
            this._willPopCallbacks.Remove(callback);
        }

        public bool hasScopedWillPopCallback {
            get { return this._willPopCallbacks.isNotEmpty(); }
        }

        protected internal override void didChangePrevious(Route previousRoute) {
            base.didChangePrevious(previousRoute);
            this.changedInternalState();
        }

        protected internal override void changedInternalState() {
            base.changedInternalState();
            this.setState(() => { });
            this._modalBarrier.markNeedsBuild();
        }

        protected internal override void changedExternalState() {
            base.changedExternalState();
            this._scopeKey.currentState?._forceRebuildPage();
        }

        public bool canPop {
            get { return !this.isFirst || this.willHandlePopInternally; }
        }


        readonly GlobalKey<_ModalScopeState> _scopeKey = new LabeledGlobalKey<_ModalScopeState>();
        internal readonly GlobalKey _subtreeKey = new LabeledGlobalKey<_ModalScopeState>();
        internal readonly PageStorageBucket _storageBucket = new PageStorageBucket();

        static readonly Animatable<float> _easeCurveTween = new CurveTween(curve: Curves.ease);
        OverlayEntry _modalBarrier;

        Widget _buildModalBarrier(BuildContext context) {
            Widget barrier;
            if (this.barrierColor != null && !this.offstage) {
                // changedInternalState is called if these update
                D.assert(this.barrierColor != _kTransparent);
                Animation<Color> color =
                    new ColorTween(
                        begin: _kTransparent,
                        end: this.barrierColor // changedInternalState is called if this updates
                    ).chain(_easeCurveTween).animate(this.animation);
                barrier = new AnimatedModalBarrier(
                    color: color,
                    dismissible: this.barrierDismissible
                );
            }
            else {
                barrier = new ModalBarrier(
                    dismissible: this.barrierDismissible
                );
            }

            return new IgnorePointer(
                ignoring: this.animation.status == AnimationStatus.reverse ||
                          this.animation.status == AnimationStatus.dismissed,
                child: barrier
            );
        }

        Widget _modalScopeCache;

        Widget _buildModalScope(BuildContext context) {
            return this._modalScopeCache = this._modalScopeCache ?? new _ModalScope(
                                               key: this._scopeKey,
                                               route: this
                                               // _ModalScope calls buildTransitions() and buildChild(), defined above
                                           );
        }

        public override ICollection<OverlayEntry> createOverlayEntries() {
            this._modalBarrier = new OverlayEntry(builder: this._buildModalBarrier);
            var content = new OverlayEntry(
                builder: this._buildModalScope, maintainState: this.maintainState
            );
            return new List<OverlayEntry> {this._modalBarrier, content};
        }

        public override string ToString() {
            return $"{this.GetType()}({this.settings}, animation: {this._animation})";
        }
    }

    public abstract class PopupRoute : ModalRoute {
        protected PopupRoute(
            RouteSettings settings = null
        ) : base(settings: settings) {
        }

        public override bool opaque {
            get { return false; }
        }

        public override bool maintainState {
            get { return true; }
        }
    }

    public class RouteObserve<R> : NavigatorObserver where R : Route {
        readonly Dictionary<R, HashSet<RouteAware>> _listeners = new Dictionary<R, HashSet<RouteAware>>();

        public void subscribe(RouteAware routeAware, R route) {
            D.assert(routeAware != null);
            D.assert(route != null);
            HashSet<RouteAware> subscribers = this._listeners.putIfAbsent(route, () => new HashSet<RouteAware>());
            if (subscribers.Add(routeAware)) {
                routeAware.didPush();
            }
        }

        public void unsubscribe(RouteAware routeAware) {
            D.assert(routeAware != null);
            foreach (R route in this._listeners.Keys) {
                HashSet<RouteAware> subscribers = this._listeners[route];
                subscribers?.Remove(routeAware);
            }
        }

        public override void didPop(Route route, Route previousRoute) {
            if (route is R && previousRoute is R) {
                var previousSubscribers = this._listeners.getOrDefault((R) previousRoute);

                if (previousSubscribers != null) {
                    foreach (RouteAware routeAware in previousSubscribers) {
                        routeAware.didPopNext();
                    }
                }

                var subscribers = this._listeners.getOrDefault((R) route);

                if (subscribers != null) {
                    foreach (RouteAware routeAware in subscribers) {
                        routeAware.didPop();
                    }
                }
            }
        }

        public override void didPush(Route route, Route previousRoute) {
            if (route is R && previousRoute is R) {
                var previousSubscribers = this._listeners.getOrDefault((R) previousRoute);

                if (previousSubscribers != null) {
                    foreach (RouteAware routeAware in previousSubscribers) {
                        routeAware.didPushNext();
                    }
                }
            }
        }
    }

    public interface RouteAware {
        void didPopNext();

        void didPush();

        void didPop();

        void didPushNext();
    }

    class _DialogRoute : PopupRoute {
        internal _DialogRoute(RoutePageBuilder pageBuilder = null, bool barrierDismissible = true,
            Color barrierColor = null,
            TimeSpan? transitionDuration = null,
            RouteTransitionsBuilder transitionBuilder = null,
            RouteSettings setting = null) : base(settings: setting) {
            this._pageBuilder = pageBuilder;
            this.barrierDismissible = barrierDismissible;
            this.barrierColor = barrierColor ?? new Color(0x80000000);
            this.transitionDuration = transitionDuration ?? TimeSpan.FromMilliseconds(200);
            this._transitionBuilder = transitionBuilder;
        }

        readonly RoutePageBuilder _pageBuilder;

        public override bool barrierDismissible { get; }

        public override Color barrierColor { get; }

        public override TimeSpan transitionDuration { get; }

        readonly RouteTransitionsBuilder _transitionBuilder;

        public override Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation) {
            return this._pageBuilder(context, animation, secondaryAnimation);
        }

        public override Widget buildTransitions(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation, Widget child) {
            if (this._transitionBuilder == null) {
                return new FadeTransition(
                    opacity: new CurvedAnimation(
                        parent: animation,
                        curve: Curves.linear
                    ),
                    child: child);
            }

            return this._transitionBuilder(context, animation, secondaryAnimation, child);
        }
    }

    public static class DialogUtils {
        public static IPromise<object> showGeneralDialog(
            BuildContext context = null,
            RoutePageBuilder pageBuilder = null,
            bool barrierDismissible = false,
            Color barrierColor = null,
            TimeSpan? transitionDuration = null,
            RouteTransitionsBuilder transitionBuilder = null
        ) {
            D.assert(pageBuilder != null);
            return Navigator.of(context, rootNavigator: true).push(new _DialogRoute(
                pageBuilder: pageBuilder,
                barrierDismissible: barrierDismissible,
                barrierColor: barrierColor,
                transitionDuration: transitionDuration,
                transitionBuilder: transitionBuilder
            ));
        }
    }

    public delegate Widget RoutePageBuilder(BuildContext context, Animation<float> animation,
        Animation<float> secondaryAnimation);

    public delegate Widget RouteTransitionsBuilder(BuildContext context, Animation<float> animation,
        Animation<float> secondaryAnimation, Widget child);
}