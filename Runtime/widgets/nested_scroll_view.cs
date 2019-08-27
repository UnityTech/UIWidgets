using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.widgets {
    public delegate List<Widget> NestedScrollViewHeaderSliversBuilder(BuildContext context, bool innerBoxIsScrolled);

    public class NestedScrollView : StatefulWidget {
        public NestedScrollView(
            Key key = null,
            ScrollController controller = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            ScrollPhysics physics = null,
            NestedScrollViewHeaderSliversBuilder headerSliverBuilder = null,
            Widget body = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            D.assert(headerSliverBuilder != null);
            D.assert(body != null);
            this.controller = controller;
            this.scrollDirection = scrollDirection;
            this.reverse = reverse;
            this.physics = physics;
            this.headerSliverBuilder = headerSliverBuilder;
            this.body = body;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly ScrollController controller;

        public readonly Axis scrollDirection;

        public readonly bool reverse;

        public readonly ScrollPhysics physics;

        public readonly NestedScrollViewHeaderSliversBuilder headerSliverBuilder;

        public readonly Widget body;

        public readonly DragStartBehavior dragStartBehavior;

        public static SliverOverlapAbsorberHandle sliverOverlapAbsorberHandleFor(BuildContext context) {
            _InheritedNestedScrollView target =
                (_InheritedNestedScrollView) context.inheritFromWidgetOfExactType(typeof(_InheritedNestedScrollView));
            D.assert(target != null,
                () => {
                    return
                        "NestedScrollView.sliverOverlapAbsorberHandleFor must be called with a context that contains a NestedScrollView.";
                });
            return target.state._absorberHandle;
        }

        internal List<Widget> _buildSlivers(BuildContext context, ScrollController innerController,
            bool bodyIsScrolled) {
            List<Widget> slivers = new List<Widget> { };
            slivers.AddRange(this.headerSliverBuilder(context, bodyIsScrolled));
            slivers.Add(new SliverFillRemaining(
                child: new PrimaryScrollController(
                    controller: innerController,
                    child: this.body
                )
            ));
            return slivers;
        }

        public override State createState() {
            return new _NestedScrollViewState();
        }
    }

    class _NestedScrollViewState : State<NestedScrollView> {
        internal SliverOverlapAbsorberHandle _absorberHandle = new SliverOverlapAbsorberHandle();

        _NestedScrollCoordinator _coordinator;

        public override void initState() {
            base.initState();
            this._coordinator =
                new _NestedScrollCoordinator(this, this.widget.controller, this._handleHasScrolledBodyChanged);
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            this._coordinator.setParent(this.widget.controller);
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            NestedScrollView oldWidget = _oldWidget as NestedScrollView;
            base.didUpdateWidget(oldWidget);
            if (oldWidget.controller != this.widget.controller) {
                this._coordinator.setParent(this.widget.controller);
            }
        }

        public override void dispose() {
            this._coordinator.dispose();
            this._coordinator = null;
            base.dispose();
        }

        bool _lastHasScrolledBody;

        void _handleHasScrolledBodyChanged() {
            if (!this.mounted) {
                return;
            }

            bool newHasScrolledBody = this._coordinator.hasScrolledBody;
            if (this._lastHasScrolledBody != newHasScrolledBody) {
                this.setState(() => { });
            }
        }

        public override Widget build(BuildContext context) {
            return new _InheritedNestedScrollView(
                state: this,
                child: new Builder(
                    builder: (BuildContext _context) => {
                        this._lastHasScrolledBody = this._coordinator.hasScrolledBody;
                        return new _NestedScrollViewCustomScrollView(
                            dragStartBehavior: this.widget.dragStartBehavior,
                            scrollDirection: this.widget.scrollDirection,
                            reverse: this.widget.reverse,
                            physics: this.widget.physics != null
                                ? this.widget.physics.applyTo(new ClampingScrollPhysics())
                                : new ClampingScrollPhysics(),
                            controller: this._coordinator._outerController,
                            slivers: this.widget._buildSlivers(
                                _context, this._coordinator._innerController, this._lastHasScrolledBody
                            ),
                            handle: this._absorberHandle
                        );
                    }
                )
            );
        }
    }

    class _NestedScrollViewCustomScrollView : CustomScrollView {
        public _NestedScrollViewCustomScrollView(
            Axis scrollDirection,
            bool reverse,
            ScrollPhysics physics,
            ScrollController controller,
            List<Widget> slivers,
            SliverOverlapAbsorberHandle handle,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(
            scrollDirection: scrollDirection,
            reverse: reverse,
            physics: physics,
            controller: controller,
            slivers: slivers,
            dragStartBehavior: dragStartBehavior
        ) {
            this.handle = handle;
        }

        public readonly SliverOverlapAbsorberHandle handle;

        protected override Widget buildViewport(
            BuildContext context,
            ViewportOffset offset,
            AxisDirection axisDirection,
            List<Widget> slivers
        ) {
            D.assert(!this.shrinkWrap);
            return new NestedScrollViewViewport(
                axisDirection: axisDirection,
                offset: offset,
                slivers: slivers,
                handle: this.handle
            );
        }
    }

    class _InheritedNestedScrollView : InheritedWidget {
        public _InheritedNestedScrollView(
            Key key = null,
            _NestedScrollViewState state = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(state != null);
            D.assert(child != null);
            this.state = state;
        }

        public readonly _NestedScrollViewState state;

        public override bool updateShouldNotify(InheritedWidget _old) {
            _InheritedNestedScrollView old = _old as _InheritedNestedScrollView;
            return this.state != old.state;
        }
    }

    class _NestedScrollMetrics : FixedScrollMetrics {
        public _NestedScrollMetrics(
            float minScrollExtent,
            float maxScrollExtent,
            float pixels,
            float viewportDimension,
            AxisDirection axisDirection,
            float minRange,
            float maxRange,
            float correctionOffset
        ) : base(
            minScrollExtent: minScrollExtent,
            maxScrollExtent: maxScrollExtent,
            pixels: pixels,
            viewportDimension: viewportDimension,
            axisDirection: axisDirection
        ) {
            this.minRange = minRange;
            this.maxRange = maxRange;
            this.correctionOffset = correctionOffset;
        }

        public _NestedScrollMetrics copyWith(
            float? minScrollExtent = null,
            float? maxScrollExtent = null,
            float? pixels = null,
            float? viewportDimension = null,
            AxisDirection? axisDirection = null,
            float? minRange = null,
            float? maxRange = null,
            float? correctionOffset = null
        ) {
            return new _NestedScrollMetrics(
                minScrollExtent: minScrollExtent ?? this.minScrollExtent,
                maxScrollExtent: maxScrollExtent ?? this.maxScrollExtent,
                pixels: pixels ?? this.pixels,
                viewportDimension: viewportDimension ?? this.viewportDimension,
                axisDirection: axisDirection ?? this.axisDirection,
                minRange: minRange ?? this.minRange,
                maxRange: maxRange ?? this.maxRange,
                correctionOffset: correctionOffset ?? this.correctionOffset
            );
        }

        public readonly float minRange;

        public readonly float maxRange;

        public readonly float correctionOffset;
    }

    delegate ScrollActivity _NestedScrollActivityGetter(_NestedScrollPosition position);

    class _NestedScrollCoordinator : ScrollActivityDelegate, ScrollHoldController {
        public _NestedScrollCoordinator(
            _NestedScrollViewState _state,
            ScrollController _parent,
            VoidCallback _onHasScrolledBodyChanged) {
            float initialScrollOffset = _parent?.initialScrollOffset ?? 0.0f;
            this._outerController =
                new _NestedScrollController(this, initialScrollOffset: initialScrollOffset, debugLabel: "outer");
            this._innerController = new _NestedScrollController(this, initialScrollOffset: 0.0f, debugLabel: "inner");
            this._state = _state;
            this._parent = _parent;
            this._onHasScrolledBodyChanged = _onHasScrolledBodyChanged;
        }

        public readonly _NestedScrollViewState _state;
        ScrollController _parent;
        public readonly VoidCallback _onHasScrolledBodyChanged;

        internal _NestedScrollController _outerController;
        internal _NestedScrollController _innerController;

        _NestedScrollPosition _outerPosition {
            get {
                if (!this._outerController.hasClients) {
                    return null;
                }

                return this._outerController.nestedPositions.Single();
            }
        }

        IEnumerable<_NestedScrollPosition> _innerPositions {
            get { return this._innerController.nestedPositions; }
        }

        public bool canScrollBody {
            get {
                _NestedScrollPosition outer = this._outerPosition;
                if (outer == null) {
                    return true;
                }

                return outer.haveDimensions && outer.extentAfter() == 0.0f;
            }
        }

        public bool hasScrolledBody {
            get {
                foreach (_NestedScrollPosition position in this._innerPositions) {
                    if (position.pixels > position.minScrollExtent) {
                        return true;
                    }
                }

                return false;
            }
        }

        public void updateShadow() {
            if (this._onHasScrolledBodyChanged != null) {
                this._onHasScrolledBodyChanged();
            }
        }

        public ScrollDirection userScrollDirection {
            get { return this._userScrollDirection; }
        }

        ScrollDirection _userScrollDirection = ScrollDirection.idle;

        public void updateUserScrollDirection(ScrollDirection value) {
            if (this.userScrollDirection == value) {
                return;
            }

            this._userScrollDirection = value;
            this._outerPosition.didUpdateScrollDirection(value);
            foreach (_NestedScrollPosition position in this._innerPositions) {
                position.didUpdateScrollDirection(value);
            }
        }

        ScrollDragController _currentDrag;

        public void beginActivity(ScrollActivity newOuterActivity, _NestedScrollActivityGetter innerActivityGetter) {
            this._outerPosition.beginActivity(newOuterActivity);
            bool scrolling = newOuterActivity.isScrolling;
            foreach (_NestedScrollPosition position in this._innerPositions) {
                ScrollActivity newInnerActivity = innerActivityGetter(position);
                position.beginActivity(newInnerActivity);
                scrolling = scrolling && newInnerActivity.isScrolling;
            }

            this._currentDrag?.dispose();
            this._currentDrag = null;
            if (!scrolling) {
                this.updateUserScrollDirection(ScrollDirection.idle);
            }
        }

        public AxisDirection axisDirection {
            get { return this._outerPosition.axisDirection; }
        }

        static IdleScrollActivity _createIdleScrollActivity(_NestedScrollPosition position) {
            return new IdleScrollActivity(position);
        }

        public void goIdle() {
            this.beginActivity(_createIdleScrollActivity(this._outerPosition), _createIdleScrollActivity);
        }

        public void goBallistic(float velocity) {
            this.beginActivity(this.createOuterBallisticScrollActivity(velocity),
                (_NestedScrollPosition position) => this.createInnerBallisticScrollActivity(position, velocity)
            );
        }

        public ScrollActivity createOuterBallisticScrollActivity(float velocity) {
            _NestedScrollPosition innerPosition = null;
            if (velocity != 0.0f) {
                foreach (_NestedScrollPosition position in this._innerPositions) {
                    if (innerPosition != null) {
                        if (velocity > 0.0f) {
                            if (innerPosition.pixels < position.pixels) {
                                continue;
                            }
                        }
                        else {
                            D.assert(velocity < 0.0f);
                            if (innerPosition.pixels > position.pixels) {
                                continue;
                            }
                        }
                    }

                    innerPosition = position;
                }
            }

            if (innerPosition == null) {
                return this._outerPosition.createBallisticScrollActivity(
                    this._outerPosition.physics.createBallisticSimulation(this._outerPosition, velocity),
                    mode: _NestedBallisticScrollActivityMode.independent
                );
            }

            _NestedScrollMetrics metrics = this._getMetrics(innerPosition, velocity);

            return this._outerPosition.createBallisticScrollActivity(
                this._outerPosition.physics.createBallisticSimulation(metrics, velocity),
                mode: _NestedBallisticScrollActivityMode.outer,
                metrics: metrics
            );
        }

        protected internal ScrollActivity createInnerBallisticScrollActivity(_NestedScrollPosition position,
            float velocity) {
            return position.createBallisticScrollActivity(
                position.physics.createBallisticSimulation(
                    velocity == 0 ? (ScrollMetrics) position : this._getMetrics(position, velocity),
                    velocity
                ),
                mode: _NestedBallisticScrollActivityMode.inner
            );
        }

        _NestedScrollMetrics _getMetrics(_NestedScrollPosition innerPosition, float velocity) {
            D.assert(innerPosition != null);
            float pixels, minRange, maxRange, correctionOffset, extra;
            if (innerPosition.pixels == innerPosition.minScrollExtent) {
                pixels = this._outerPosition.pixels.clamp(this._outerPosition.minScrollExtent,
                    this._outerPosition.maxScrollExtent); // TODO(ianh): gracefully handle out-of-range outer positions
                minRange = this._outerPosition.minScrollExtent;
                maxRange = this._outerPosition.maxScrollExtent;
                D.assert(minRange <= maxRange);
                correctionOffset = 0.0f;
                extra = 0.0f;
            }
            else {
                D.assert(innerPosition.pixels != innerPosition.minScrollExtent);
                if (innerPosition.pixels < innerPosition.minScrollExtent) {
                    pixels = innerPosition.pixels - innerPosition.minScrollExtent + this._outerPosition.minScrollExtent;
                }
                else {
                    D.assert(innerPosition.pixels > innerPosition.minScrollExtent);
                    pixels = innerPosition.pixels - innerPosition.minScrollExtent + this._outerPosition.maxScrollExtent;
                }

                if ((velocity > 0.0f) && (innerPosition.pixels > innerPosition.minScrollExtent)) {
                    extra = this._outerPosition.maxScrollExtent - this._outerPosition.pixels;
                    D.assert(extra >= 0.0f);
                    minRange = pixels;
                    maxRange = pixels + extra;
                    D.assert(minRange <= maxRange);
                    correctionOffset = this._outerPosition.pixels - pixels;
                }
                else if ((velocity < 0.0f) && (innerPosition.pixels < innerPosition.minScrollExtent)) {
                    extra = this._outerPosition.pixels - this._outerPosition.minScrollExtent;
                    D.assert(extra >= 0.0f);
                    minRange = pixels - extra;
                    maxRange = pixels;
                    D.assert(minRange <= maxRange);
                    correctionOffset = this._outerPosition.pixels - pixels;
                }
                else {
                    if (velocity > 0.0f) {
                        extra = this._outerPosition.minScrollExtent - this._outerPosition.pixels;
                    }
                    else {
                        D.assert(velocity < 0.0f);
                        extra = this._outerPosition.pixels -
                                (this._outerPosition.maxScrollExtent - this._outerPosition.minScrollExtent);
                    }

                    D.assert(extra <= 0.0f);
                    minRange = this._outerPosition.minScrollExtent;
                    maxRange = this._outerPosition.maxScrollExtent + extra;
                    D.assert(minRange <= maxRange);
                    correctionOffset = 0.0f;
                }
            }

            return new _NestedScrollMetrics(
                minScrollExtent: this._outerPosition.minScrollExtent,
                maxScrollExtent: this._outerPosition.maxScrollExtent + innerPosition.maxScrollExtent -
                                 innerPosition.minScrollExtent + extra,
                pixels: pixels,
                viewportDimension: this._outerPosition.viewportDimension,
                axisDirection: this._outerPosition.axisDirection,
                minRange: minRange,
                maxRange: maxRange,
                correctionOffset: correctionOffset
            );
        }

        public float unnestOffset(float value, _NestedScrollPosition source) {
            if (source == this._outerPosition) {
                return value.clamp(this._outerPosition.minScrollExtent, this._outerPosition.maxScrollExtent);
            }

            if (value < source.minScrollExtent) {
                return value - source.minScrollExtent + this._outerPosition.minScrollExtent;
            }

            return value - source.minScrollExtent + this._outerPosition.maxScrollExtent;
        }

        public float nestOffset(float value, _NestedScrollPosition target) {
            if (target == this._outerPosition) {
                return value.clamp(this._outerPosition.minScrollExtent, this._outerPosition.maxScrollExtent);
            }

            if (value < this._outerPosition.minScrollExtent) {
                return value - this._outerPosition.minScrollExtent + target.minScrollExtent;
            }

            if (value > this._outerPosition.maxScrollExtent) {
                return value - this._outerPosition.maxScrollExtent + target.minScrollExtent;
            }

            return target.minScrollExtent;
        }

        public void updateCanDrag() {
            if (!this._outerPosition.haveDimensions) {
                return;
            }

            float maxInnerExtent = 0.0f;
            foreach (_NestedScrollPosition position in this._innerPositions) {
                if (!position.haveDimensions) {
                    return;
                }

                maxInnerExtent = Mathf.Max(maxInnerExtent, position.maxScrollExtent - position.minScrollExtent);
            }

            this._outerPosition.updateCanDrag(maxInnerExtent);
        }

        public IPromise animateTo(float to,
            TimeSpan duration,
            Curve curve
        ) {
            DrivenScrollActivity outerActivity = this._outerPosition.createDrivenScrollActivity(
                this.nestOffset(to, this._outerPosition),
                duration,
                curve
            );
            List<IPromise> resultFutures = new List<IPromise> {outerActivity.done};
            this.beginActivity(
                outerActivity,
                (_NestedScrollPosition position) => {
                    DrivenScrollActivity innerActivity = position.createDrivenScrollActivity(
                        this.nestOffset(to, position),
                        duration,
                        curve
                    );
                    resultFutures.Add(innerActivity.done);
                    return innerActivity;
                }
            );
            return Promise.All(resultFutures);
        }

        public void jumpTo(float to) {
            this.goIdle();
            this._outerPosition.localJumpTo(this.nestOffset(to, this._outerPosition));
            foreach (_NestedScrollPosition position in this._innerPositions) {
                position.localJumpTo(this.nestOffset(to, position));
            }

            this.goBallistic(0.0f);
        }

        public float setPixels(float newPixels) {
            D.assert(false);
            return 0.0f;
        }

        public ScrollHoldController hold(VoidCallback holdCancelCallback) {
            this.beginActivity(
                new HoldScrollActivity(del: this._outerPosition, onHoldCanceled: holdCancelCallback),
                (_NestedScrollPosition position) => new HoldScrollActivity(del: position)
            );
            return this;
        }

        public void cancel() {
            this.goBallistic(0.0f);
        }

        public Drag drag(DragStartDetails details, VoidCallback dragCancelCallback) {
            ScrollDragController drag = new ScrollDragController(
                del: this,
                details: details,
                onDragCanceled: dragCancelCallback
            );
            this.beginActivity(
                new DragScrollActivity(this._outerPosition, drag),
                (_NestedScrollPosition position) => new DragScrollActivity(position, drag)
            );
            D.assert(this._currentDrag == null);
            this._currentDrag = drag;
            return drag;
        }

        public void applyUserOffset(float delta) {
            this.updateUserScrollDirection(delta > 0.0f ? ScrollDirection.forward : ScrollDirection.reverse);
            D.assert(delta != 0.0f);
            if (!this._innerPositions.Any()) {
                this._outerPosition.applyFullDragUpdate(delta);
            }
            else if (delta < 0.0f) {
                float innerDelta = this._outerPosition.applyClampedDragUpdate(delta);
                if (innerDelta != 0.0f) {
                    foreach (_NestedScrollPosition position in this._innerPositions) {
                        position.applyFullDragUpdate(innerDelta);
                    }
                }
            }
            else {
                float outerDelta = 0.0f; // it will go positive if it changes
                List<float> overscrolls = new List<float> { };
                List<_NestedScrollPosition> innerPositions = this._innerPositions.ToList();
                foreach (_NestedScrollPosition position in innerPositions) {
                    float overscroll = position.applyClampedDragUpdate(delta);
                    outerDelta = Mathf.Max(outerDelta, overscroll);
                    overscrolls.Add(overscroll);
                }

                if (outerDelta != 0.0f) {
                    outerDelta -= this._outerPosition.applyClampedDragUpdate(outerDelta);
                }

                for (int i = 0; i < innerPositions.Count; ++i) {
                    float remainingDelta = overscrolls[i] - outerDelta;
                    if (remainingDelta > 0.0f) {
                        innerPositions[i].applyFullDragUpdate(remainingDelta);
                    }
                }
            }
        }

        public void applyUserScrollOffset(float delta) {
            // TODO: replace with real implementation
            this.applyUserOffset(delta);
        }

        public void setParent(ScrollController value) {
            this._parent = value;
            this.updateParent();
        }

        public void updateParent() {
            this._outerPosition?.setParent(this._parent ?? PrimaryScrollController.of(this._state.context));
        }

        public void dispose() {
            this._currentDrag?.dispose();
            this._currentDrag = null;
            this._outerController.dispose();
            this._innerController.dispose();
        }

        public override string ToString() {
            return "$GetType()(outer=$_outerController; inner=$_innerController)";
        }
    }

    class _NestedScrollController : ScrollController {
        public _NestedScrollController(
            _NestedScrollCoordinator coordinator,
            float initialScrollOffset = 0.0f,
            string debugLabel = null
        ) : base(initialScrollOffset: initialScrollOffset, debugLabel: debugLabel) {
            this.coordinator = coordinator;
        }

        public readonly _NestedScrollCoordinator coordinator;

        public override ScrollPosition createScrollPosition(
            ScrollPhysics physics,
            ScrollContext context,
            ScrollPosition oldPosition
        ) {
            return new _NestedScrollPosition(
                coordinator: this.coordinator,
                physics: physics,
                context: context,
                initialPixels: this.initialScrollOffset,
                oldPosition: oldPosition,
                debugLabel: this.debugLabel
            );
        }

        public override void attach(ScrollPosition position) {
            D.assert(position is _NestedScrollPosition);
            base.attach(position);
            this.coordinator.updateParent();
            this.coordinator.updateCanDrag();
            position.addListener(this._scheduleUpdateShadow);
            this._scheduleUpdateShadow();
        }

        public override void detach(ScrollPosition position) {
            D.assert(position is _NestedScrollPosition);
            position.removeListener(this._scheduleUpdateShadow);
            base.detach(position);
            this._scheduleUpdateShadow();
        }

        void _scheduleUpdateShadow() {
            SchedulerBinding.instance.addPostFrameCallback(
                (TimeSpan timeStamp) => { this.coordinator.updateShadow(); }
            );
        }

        public IEnumerable<_NestedScrollPosition> nestedPositions {
            get {
                foreach (var scrollPosition in this.positions) {
                    yield return (_NestedScrollPosition) scrollPosition;
                }
            }
        }
    }

    class _NestedScrollPosition : ScrollPosition, ScrollActivityDelegate {
        public _NestedScrollPosition(
            ScrollPhysics physics,
            ScrollContext context,
            float initialPixels = 0.0f,
            ScrollPosition oldPosition = null,
            string debugLabel = null,
            _NestedScrollCoordinator coordinator = null
        ) : base(
            physics: physics,
            context: context,
            oldPosition: oldPosition,
            debugLabel: debugLabel,
            coordinator: coordinator
        ) {
            D.assert(coordinator != null);
            if (!this.havePixels) {
                this.correctPixels(initialPixels);
            }

            if (this.activity == null) {
                this.goIdle();
            }

            D.assert(this.activity != null);
            this.saveScrollOffset(); // in case we didn't restore but could, so that we don't restore it later
        }

        public _NestedScrollCoordinator coordinator {
            get { return (_NestedScrollCoordinator) this._coordinator; }
        }

        public TickerProvider vsync {
            get { return this.context.vsync; }
        }

        ScrollController _parent;

        public void setParent(ScrollController value) {
            this._parent?.detach(this);
            this._parent = value;
            this._parent?.attach(this);
        }

        public override AxisDirection axisDirection {
            get { return this.context.axisDirection; }
        }

        protected override void absorb(ScrollPosition other) {
            base.absorb(other);
            this.activity.updateDelegate(this);
        }

        protected override void restoreScrollOffset() {
            if (this.coordinator.canScrollBody) {
                base.restoreScrollOffset();
            }
        }

        public float applyClampedDragUpdate(float delta) {
            D.assert(delta != 0.0f);
            float min = delta < 0.0f ? -float.PositiveInfinity : Mathf.Min(this.minScrollExtent, this.pixels);
            float max = delta > 0.0f ? float.PositiveInfinity : Mathf.Max(this.maxScrollExtent, this.pixels);
            float oldPixels = this.pixels;
            float newPixels = (this.pixels - delta).clamp(min, max);
            float clampedDelta = newPixels - this.pixels;
            if (clampedDelta == 0.0f) {
                return delta;
            }

            float overscroll = this.physics.applyBoundaryConditions(this, newPixels);
            float actualNewPixels = newPixels - overscroll;
            float offset = actualNewPixels - oldPixels;
            if (offset != 0.0f) {
                this.forcePixels(actualNewPixels);
                this.didUpdateScrollPositionBy(offset);
            }

            return delta + offset;
        }

        public float applyFullDragUpdate(float delta) {
            D.assert(delta != 0.0f);
            float oldPixels = this.pixels;
            float newPixels = this.pixels - this.physics.applyPhysicsToUserOffset(this, delta);
            if (oldPixels == newPixels) {
                return 0.0f; // delta must have been so small we dropped it during floating point addition
            }

            float overscroll = this.physics.applyBoundaryConditions(this, newPixels);
            float actualNewPixels = newPixels - overscroll;
            if (actualNewPixels != oldPixels) {
                this.forcePixels(actualNewPixels);
                this.didUpdateScrollPositionBy(actualNewPixels - oldPixels);
            }

            if (overscroll != 0.0f) {
                this.didOverscrollBy(overscroll);
                return overscroll;
            }

            return 0.0f;
        }

        public override ScrollDirection userScrollDirection {
            get { return this.coordinator.userScrollDirection; }
        }

        public DrivenScrollActivity createDrivenScrollActivity(float to, TimeSpan duration, Curve curve) {
            return new DrivenScrollActivity(
                this,
                from: this.pixels,
                to: to,
                duration: duration,
                curve: curve,
                vsync: this.vsync
            );
        }

        public void applyUserOffset(float delta) {
            D.assert(false);
        }

        public void applyUserScrollOffset(float delta) {
            // TODO: replace with real implementation
            this.applyUserOffset(delta);
        }

        public void goIdle() {
            this.beginActivity(new IdleScrollActivity(this));
        }

        public void goBallistic(float velocity) {
            Simulation simulation = null;
            if (velocity != 0.0f || this.outOfRange()) {
                simulation = this.physics.createBallisticSimulation(this, velocity);
            }

            this.beginActivity(this.createBallisticScrollActivity(
                simulation,
                mode: _NestedBallisticScrollActivityMode.independent
            ));
        }

        public ScrollActivity createBallisticScrollActivity(
            Simulation simulation = null,
            _NestedBallisticScrollActivityMode? mode = null,
            _NestedScrollMetrics metrics = null
        ) {
            if (simulation == null) {
                return new IdleScrollActivity(this);
            }

            D.assert(mode != null);
            switch (mode) {
                case _NestedBallisticScrollActivityMode.outer:
                    D.assert(metrics != null);
                    if (metrics.minRange == metrics.maxRange) {
                        return new IdleScrollActivity(this);
                    }

                    return new _NestedOuterBallisticScrollActivity(this.coordinator, this, metrics, simulation,
                        this.context.vsync);
                case _NestedBallisticScrollActivityMode.inner:
                    return new _NestedInnerBallisticScrollActivity(this.coordinator, this, simulation,
                        this.context.vsync);
                case _NestedBallisticScrollActivityMode.independent:
                    return new BallisticScrollActivity(this, simulation, this.context.vsync);
            }

            return null;
        }

        public override IPromise animateTo(float to,
            TimeSpan duration,
            Curve curve
        ) {
            return this.coordinator.animateTo(this.coordinator.unnestOffset(to, this), duration: duration,
                curve: curve);
        }

        public override void jumpTo(float value) {
            this.coordinator.jumpTo(this.coordinator.unnestOffset(value, this));
        }

        public void localJumpTo(float value) {
            if (this.pixels != value) {
                float oldPixels = this.pixels;
                this.forcePixels(value);
                this.didStartScroll();
                this.didUpdateScrollPositionBy(this.pixels - oldPixels);
                this.didEndScroll();
            }
        }

        protected override void applyNewDimensions() {
            base.applyNewDimensions();
            this.coordinator.updateCanDrag();
        }

        public void updateCanDrag(float totalExtent) {
            this.context.setCanDrag(totalExtent > (this.viewportDimension - this.maxScrollExtent) ||
                                    this.minScrollExtent != this.maxScrollExtent);
        }

        public override ScrollHoldController hold(VoidCallback holdCancelCallback) {
            return this.coordinator.hold(holdCancelCallback);
        }

        public override Drag drag(DragStartDetails details, VoidCallback dragCancelCallback) {
            return this.coordinator.drag(details, dragCancelCallback);
        }

        public override void dispose() {
            this._parent?.detach(this);
            base.dispose();
        }
    }

    enum _NestedBallisticScrollActivityMode {
        outer,
        inner,
        independent
    }

    class _NestedInnerBallisticScrollActivity : BallisticScrollActivity {
        public _NestedInnerBallisticScrollActivity(
            _NestedScrollCoordinator coordinator,
            _NestedScrollPosition position,
            Simulation simulation,
            TickerProvider vsync
        ) : base(position, simulation, vsync) {
            this.coordinator = coordinator;
        }

        public readonly _NestedScrollCoordinator coordinator;

        public new _NestedScrollPosition del {
            get { return (_NestedScrollPosition) base.del; }
        }

        public override void resetActivity() {
            this.del.beginActivity(this.coordinator.createInnerBallisticScrollActivity(this.del, this.velocity));
        }

        public override void applyNewDimensions() {
            this.del.beginActivity(this.coordinator.createInnerBallisticScrollActivity(this.del, this.velocity));
        }

        protected override bool applyMoveTo(float value) {
            return base.applyMoveTo(this.coordinator.nestOffset(value, this.del));
        }
    }

    class _NestedOuterBallisticScrollActivity : BallisticScrollActivity {
        public _NestedOuterBallisticScrollActivity(
            _NestedScrollCoordinator coordinator,
            _NestedScrollPosition position,
            _NestedScrollMetrics metrics,
            Simulation simulation,
            TickerProvider vsync
        ) : base(position, simulation, vsync) {
            D.assert(metrics.minRange != metrics.maxRange);
            D.assert(metrics.maxRange > metrics.minRange);
            this.coordinator = coordinator;
            this.metrics = metrics;
        }

        public readonly _NestedScrollCoordinator coordinator;
        public readonly _NestedScrollMetrics metrics;

        public new _NestedScrollPosition del {
            get { return (_NestedScrollPosition) base.del; }
        }

        public override void resetActivity() {
            this.del.beginActivity(this.coordinator.createOuterBallisticScrollActivity(this.velocity));
        }

        public override void applyNewDimensions() {
            this.del.beginActivity(this.coordinator.createOuterBallisticScrollActivity(this.velocity));
        }

        protected override bool applyMoveTo(float value) {
            bool done = false;
            if (this.velocity > 0.0f) {
                if (value < this.metrics.minRange) {
                    return true;
                }

                if (value > this.metrics.maxRange) {
                    value = this.metrics.maxRange;
                    done = true;
                }
            }
            else if (this.velocity < 0.0f) {
                if (value > this.metrics.maxRange) {
                    return true;
                }

                if (value < this.metrics.minRange) {
                    value = this.metrics.minRange;
                    done = true;
                }
            }
            else {
                value = value.clamp(this.metrics.minRange, this.metrics.maxRange);
                done = true;
            }

            bool result = base.applyMoveTo(value + this.metrics.correctionOffset);
            D.assert(result); // since we tried to pass an in-range value, it shouldn"t ever overflow
            return !done;
        }

        public override string ToString() {
            return
                $"{this.GetType()}({this.metrics.minRange} .. {this.metrics.maxRange}; correcting by {this.metrics.correctionOffset})";
        }
    }

    public class SliverOverlapAbsorberHandle : ChangeNotifier {
        internal int _writers = 0;

        public float layoutExtent {
            get { return this._layoutExtent; }
        }

        float _layoutExtent;

        public float scrollExtent {
            get { return this._scrollExtent; }
        }

        float _scrollExtent;

        internal void _setExtents(float layoutValue, float scrollValue) {
            D.assert(this._writers == 1,
                () => "Multiple RenderSliverOverlapAbsorbers have been provided the same SliverOverlapAbsorberHandle.");
            this._layoutExtent = layoutValue;
            this._scrollExtent = scrollValue;
        }

        internal void _markNeedsLayout() {
            this.notifyListeners();
        }

        public override string ToString() {
            string extra = "";
            switch (this._writers) {
                case 0:
                    extra = ", orphan";
                    break;
                case 1:
                    break;
                default:
                    extra = ", $_writers WRITERS ASSIGNED";
                    break;
            }

            return $"{this.GetType()}({this.layoutExtent}{extra})";
        }
    }

    public class SliverOverlapAbsorber : SingleChildRenderObjectWidget {
        public SliverOverlapAbsorber(
            Key key = null,
            SliverOverlapAbsorberHandle handle = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(handle != null);
            this.handle = handle;
        }

        public readonly SliverOverlapAbsorberHandle handle;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderSliverOverlapAbsorber(
                handle: this.handle
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            RenderSliverOverlapAbsorber renderObject = _renderObject as RenderSliverOverlapAbsorber;
            renderObject.handle = this.handle;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverOverlapAbsorberHandle>("handle", this.handle));
        }
    }

    public class RenderSliverOverlapAbsorber : RenderObjectWithChildMixinRenderSliver<RenderSliver> {
        public RenderSliverOverlapAbsorber(
            SliverOverlapAbsorberHandle handle,
            RenderSliver child = null
        ) {
            D.assert(handle != null);
            this._handle = handle;
            this.child = child;
        }

        public SliverOverlapAbsorberHandle handle {
            get { return this._handle; }
            set {
                D.assert(value != null);
                if (this.handle == value) {
                    return;
                }

                if (this.attached) {
                    this.handle._writers -= 1;
                    value._writers += 1;
                    value._setExtents(this.handle.layoutExtent, this.handle.scrollExtent);
                }

                this._handle = value;
            }
        }

        SliverOverlapAbsorberHandle _handle;

        public override void attach(object owner) {
            base.attach(owner);
            this.handle._writers += 1;
        }

        public override void detach() {
            this.handle._writers -= 1;
            base.detach();
        }

        protected override void performLayout() {
            D.assert(this.handle._writers == 1,
                () =>
                    "A SliverOverlapAbsorberHandle cannot be passed to multiple RenderSliverOverlapAbsorber objects at the same time.");
            if (this.child == null) {
                this.geometry = new SliverGeometry();
                return;
            }

            this.child.layout(this.constraints, parentUsesSize: true);
            SliverGeometry childLayoutGeometry = this.child.geometry;
            this.geometry = new SliverGeometry(
                scrollExtent: childLayoutGeometry.scrollExtent - childLayoutGeometry.maxScrollObstructionExtent,
                paintExtent: childLayoutGeometry.paintExtent,
                paintOrigin: childLayoutGeometry.paintOrigin,
                layoutExtent: childLayoutGeometry.paintExtent - childLayoutGeometry.maxScrollObstructionExtent,
                maxPaintExtent: childLayoutGeometry.maxPaintExtent,
                maxScrollObstructionExtent: childLayoutGeometry.maxScrollObstructionExtent,
                hitTestExtent: childLayoutGeometry.hitTestExtent,
                visible: childLayoutGeometry.visible,
                hasVisualOverflow: childLayoutGeometry.hasVisualOverflow,
                scrollOffsetCorrection: childLayoutGeometry.scrollOffsetCorrection
            );
            this.handle._setExtents(childLayoutGeometry.maxScrollObstructionExtent,
                childLayoutGeometry.maxScrollObstructionExtent);
        }

        public override void applyPaintTransform(RenderObject child, Matrix3 transform) {
        }

        protected override bool hitTestChildren(
            HitTestResult result,
            float mainAxisPosition = 0,
            float crossAxisPosition = 0
        ) {
            if (this.child != null) {
                return this.child.hitTest(result, mainAxisPosition: mainAxisPosition,
                    crossAxisPosition: crossAxisPosition);
            }

            return false;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                context.paintChild(this.child, offset);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverOverlapAbsorberHandle>("handle", this.handle));
        }
    }

    public class SliverOverlapInjector : SingleChildRenderObjectWidget {
        public SliverOverlapInjector(
            Key key = null,
            SliverOverlapAbsorberHandle handle = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(handle != null);
            this.handle = handle;
        }

        public readonly SliverOverlapAbsorberHandle handle;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderSliverOverlapInjector(
                handle: this.handle
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            RenderSliverOverlapInjector renderObject = _renderObject as RenderSliverOverlapInjector;
            renderObject.handle = this.handle;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverOverlapAbsorberHandle>("handle", this.handle));
        }
    }

    public class RenderSliverOverlapInjector : RenderSliver {
        public RenderSliverOverlapInjector(
            SliverOverlapAbsorberHandle handle
        ) {
            D.assert(handle != null);
            this._handle = handle;
        }

        float _currentLayoutExtent;
        float _currentMaxExtent;

        public SliverOverlapAbsorberHandle handle {
            get { return this._handle; }
            set {
                D.assert(value != null);
                if (this.handle == value) {
                    return;
                }

                if (this.attached) {
                    this.handle.removeListener(this.markNeedsLayout);
                }

                this._handle = value;
                if (this.attached) {
                    this.handle.addListener(this.markNeedsLayout);
                    if (this.handle.layoutExtent != this._currentLayoutExtent ||
                        this.handle.scrollExtent != this._currentMaxExtent) {
                        this.markNeedsLayout();
                    }
                }
            }
        }

        SliverOverlapAbsorberHandle _handle;

        public override void attach(object owner) {
            base.attach(owner);
            this.handle.addListener(this.markNeedsLayout);
            if (this.handle.layoutExtent != this._currentLayoutExtent ||
                this.handle.scrollExtent != this._currentMaxExtent) {
                this.markNeedsLayout();
            }
        }

        public override void detach() {
            this.handle.removeListener(this.markNeedsLayout);
            base.detach();
        }

        protected override void performLayout() {
            this._currentLayoutExtent = this.handle.layoutExtent;
            this._currentMaxExtent = this.handle.layoutExtent;
            float clampedLayoutExtent = Mathf.Min(this._currentLayoutExtent - this.constraints.scrollOffset,
                this.constraints.remainingPaintExtent);
            this.geometry = new SliverGeometry(
                scrollExtent: this._currentLayoutExtent,
                paintExtent: Mathf.Max(0.0f, clampedLayoutExtent),
                maxPaintExtent: this._currentMaxExtent
            );
        }

        public override void debugPaint(PaintingContext context, Offset offset) {
            D.assert(() => {
                if (D.debugPaintSizeEnabled) {
                    Paint paint = new Paint();
                    paint.color = new Color(0xFFCC9933);
                    paint.strokeWidth = 3.0f;
                    paint.style = PaintingStyle.stroke;
                    Offset start, end, delta;
                    switch (this.constraints.axis) {
                        case Axis.vertical:
                            float x = offset.dx + this.constraints.crossAxisExtent / 2.0f;
                            start = new Offset(x, offset.dy);
                            end = new Offset(x, offset.dy + this.geometry.paintExtent);
                            delta = new Offset(this.constraints.crossAxisExtent / 5.0f, 0.0f);
                            break;
                        case Axis.horizontal:
                            float y = offset.dy + this.constraints.crossAxisExtent / 2.0f;
                            start = new Offset(offset.dx, y);
                            end = new Offset(offset.dy + this.geometry.paintExtent, y);
                            delta = new Offset(0.0f, this.constraints.crossAxisExtent / 5.0f);
                            break;
                        default:
                            throw new Exception("");
                    }

                    for (int index = -2; index <= 2; index += 1) {
                        PaintingUtilities.paintZigZag(context.canvas, paint, start - delta * (float) index,
                            end - delta * (float) index, 10, 10.0f);
                    }
                }

                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverOverlapAbsorberHandle>("handle", this.handle));
        }
    }

    public class NestedScrollViewViewport : Viewport {
        public NestedScrollViewViewport(
            Key key = null,
            AxisDirection axisDirection = AxisDirection.down,
            AxisDirection crossAxisDirection = AxisDirection.right,
            float anchor = 0.0f,
            ViewportOffset offset = null,
            Key center = null,
            List<Widget> slivers = null,
            SliverOverlapAbsorberHandle handle = null
        ) : base(
            key: key,
            axisDirection: axisDirection,
            crossAxisDirection: crossAxisDirection,
            anchor: anchor,
            offset: offset,
            center: center,
            slivers: slivers ?? new List<Widget>()
        ) {
            D.assert(handle != null);
            D.assert(this.offset != null);
            this.handle = handle;
        }

        public readonly SliverOverlapAbsorberHandle handle;

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderNestedScrollViewViewport(
                axisDirection: this.axisDirection,
                crossAxisDirection: this.crossAxisDirection ??
                                    getDefaultCrossAxisDirection(context, this.axisDirection),
                anchor: this.anchor,
                offset: this.offset,
                handle: this.handle
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            RenderNestedScrollViewViewport renderObject = _renderObject as RenderNestedScrollViewViewport;
            renderObject.axisDirection = this.axisDirection;
            renderObject.crossAxisDirection = this.crossAxisDirection ??
                                              getDefaultCrossAxisDirection(context, this.axisDirection);
            if (this.crossAxisDirection == null) {
                renderObject.anchor = this.anchor;
            }

            renderObject.offset = this.offset;
            renderObject.handle = this.handle;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverOverlapAbsorberHandle>("handle", this.handle));
        }
    }

    public class RenderNestedScrollViewViewport : RenderViewport {
        public RenderNestedScrollViewViewport(
            AxisDirection axisDirection = AxisDirection.down,
            AxisDirection crossAxisDirection = AxisDirection.right,
            ViewportOffset offset = null,
            float anchor = 0.0f,
            List<RenderSliver> children = null,
            RenderSliver center = null,
            SliverOverlapAbsorberHandle handle = null
        ) : base(
            axisDirection: axisDirection,
            crossAxisDirection: crossAxisDirection,
            offset: offset,
            anchor: anchor,
            children: children,
            center: center
        ) {
            D.assert(handle != null);
            D.assert(offset != null);
            this._handle = handle;
        }

        public SliverOverlapAbsorberHandle handle {
            get { return this._handle; }
            set {
                D.assert(value != null);
                if (this.handle == value) {
                    return;
                }

                this._handle = value;
                this.handle._markNeedsLayout();
            }
        }

        SliverOverlapAbsorberHandle _handle;

        public override void markNeedsLayout() {
            this.handle._markNeedsLayout();
            base.markNeedsLayout();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<SliverOverlapAbsorberHandle>("handle", this.handle));
        }
    }
}