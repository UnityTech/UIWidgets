using System;
using System.Collections.Generic;
using System.Linq;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public delegate Widget ViewportBuilder(BuildContext context, ViewportOffset position);

    public class Scrollable : StatefulWidget {
        public Scrollable(
            Key key = null,
            AxisDirection axisDirection = AxisDirection.down,
            ScrollController controller = null,
            ScrollPhysics physics = null,
            ViewportBuilder viewportBuilder = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            D.assert(viewportBuilder != null);

            this.axisDirection = axisDirection;
            this.controller = controller;
            this.physics = physics;
            this.viewportBuilder = viewportBuilder;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly AxisDirection axisDirection;

        public readonly ScrollController controller;

        public readonly ScrollPhysics physics;

        public readonly ViewportBuilder viewportBuilder;

        public readonly DragStartBehavior dragStartBehavior;

        public Axis axis {
            get { return AxisUtils.axisDirectionToAxis(this.axisDirection); }
        }

        public override State createState() {
            return new ScrollableState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<AxisDirection>("axisDirection", this.axisDirection));
            properties.add(new DiagnosticsProperty<ScrollPhysics>("physics", this.physics));
        }

        public static ScrollableState of(BuildContext context) {
            _ScrollableScope widget = (_ScrollableScope) context.inheritFromWidgetOfExactType(typeof(_ScrollableScope));
            return widget == null ? null : widget.scrollable;
        }

        public static IPromise ensureVisible(BuildContext context,
            float alignment = 0.0f,
            TimeSpan? duration = null,
            Curve curve = null
        ) {
            duration = duration ?? TimeSpan.Zero;
            curve = curve ?? Curves.ease;
            List<IPromise> futures = new List<IPromise>();

            ScrollableState scrollable = of(context);
            while (scrollable != null) {
                futures.Add(scrollable.position.ensureVisible(
                    context.findRenderObject(),
                    alignment: alignment,
                    duration: duration,
                    curve: curve
                ));
                context = scrollable.context;
                scrollable = of(context);
            }

            if (futures.isEmpty() || duration == TimeSpan.Zero) {
                return Promise.Resolved();
            }

            if (futures.Count == 1) {
                return futures.Single();
            }

            return Promise.All(futures);
        }
    }

    class _ScrollableScope : InheritedWidget {
        internal _ScrollableScope(
            Key key = null,
            ScrollableState scrollable = null,
            ScrollPosition position = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(scrollable != null);
            D.assert(child != null);
            this.scrollable = scrollable;
            this.position = position;
        }

        public readonly ScrollableState scrollable;

        public readonly ScrollPosition position;

        public override bool updateShouldNotify(InheritedWidget old) {
            return this.position != ((_ScrollableScope) old).position;
        }
    }

    public class ScrollableState : TickerProviderStateMixin<Scrollable>, ScrollContext {
        public ScrollPosition position {
            get { return this._position; }
        }

        ScrollPosition _position;

        public AxisDirection axisDirection {
            get { return this.widget.axisDirection; }
        }

        ScrollBehavior _configuration;

        ScrollPhysics _physics;

        void _updatePosition() {
            this._configuration = ScrollConfiguration.of(this.context);
            this._physics = this._configuration.getScrollPhysics(this.context);
            if (this.widget.physics != null) {
                this._physics = this.widget.physics.applyTo(this._physics);
            }

            ScrollController controller = this.widget.controller;
            ScrollPosition oldPosition = this.position;
            if (oldPosition != null) {
                if (controller != null) {
                    controller.detach(oldPosition);
                }

                Window.instance.scheduleMicrotask(oldPosition.dispose);
            }

            this._position = controller == null
                ? null
                : controller.createScrollPosition(this._physics, this, oldPosition);
            this._position = this._position
                             ?? new ScrollPositionWithSingleContext(physics: this._physics, context: this,
                                 oldPosition: oldPosition);
            D.assert(this.position != null);
            if (controller != null) {
                controller.attach(this.position);
            }
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            this._updatePosition();
        }

        bool _shouldUpdatePosition(Scrollable oldWidget) {
            ScrollPhysics newPhysics = this.widget.physics;
            ScrollPhysics oldPhysics = oldWidget.physics;
            do {
                Type newPhysicsType = newPhysics != null ? newPhysics.GetType() : null;
                Type oldPhysicsType = oldPhysics != null ? oldPhysics.GetType() : null;

                if (newPhysicsType != oldPhysicsType) {
                    return true;
                }

                if (newPhysics != null) {
                    newPhysics = newPhysics.parent;
                }

                if (oldPhysics != null) {
                    oldPhysics = oldPhysics.parent;
                }
            } while (newPhysics != null || oldPhysics != null);

            Type controllerType = this.widget.controller == null ? null : this.widget.controller.GetType();
            Type oldControllerType = oldWidget.controller == null ? null : oldWidget.controller.GetType();
            return controllerType != oldControllerType;
        }

        public override void didUpdateWidget(StatefulWidget oldWidgetRaw) {
            Scrollable oldWidget = (Scrollable) oldWidgetRaw;
            base.didUpdateWidget(oldWidget);

            if (this.widget.controller != oldWidget.controller) {
                if (oldWidget.controller != null) {
                    oldWidget.controller.detach(this.position);
                }

                if (this.widget.controller != null) {
                    this.widget.controller.attach(this.position);
                }
            }

            if (this._shouldUpdatePosition(oldWidget)) {
                this._updatePosition();
            }
        }

        public override void dispose() {
            if (this.widget.controller != null) {
                this.widget.controller.detach(this.position);
            }

            this.position.dispose();
            base.dispose();
        }

        readonly GlobalKey<RawGestureDetectorState> _gestureDetectorKey = GlobalKey<RawGestureDetectorState>.key();
        readonly GlobalKey _ignorePointerKey = GlobalKey.key();

        Dictionary<Type, GestureRecognizerFactory> _gestureRecognizers =
            new Dictionary<Type, GestureRecognizerFactory>();

        bool _shouldIgnorePointer = false;

        bool _lastCanDrag;
        Axis _lastAxisDirection;

        public void setCanDrag(bool canDrag) {
            if (canDrag == this._lastCanDrag && (!canDrag || this.widget.axis == this._lastAxisDirection)) {
                return;
            }

            if (!canDrag) {
                this._gestureRecognizers = new Dictionary<Type, GestureRecognizerFactory>();
            }
            else {
                switch (this.widget.axis) {
                    case Axis.vertical:
                        this._gestureRecognizers = new Dictionary<Type, GestureRecognizerFactory>();
                        this._gestureRecognizers.Add(typeof(VerticalDragGestureRecognizer),
                            new GestureRecognizerFactoryWithHandlers<VerticalDragGestureRecognizer>(
                                () => new VerticalDragGestureRecognizer(),
                                instance => {
                                    instance.onDown = this._handleDragDown;
                                    instance.onStart = this._handleDragStart;
                                    instance.onUpdate = this._handleDragUpdate;
                                    instance.onEnd = this._handleDragEnd;
                                    instance.onCancel = this._handleDragCancel;
                                    instance.minFlingDistance =
                                        this._physics == null ? (float?) null : this._physics.minFlingDistance;
                                    instance.minFlingVelocity =
                                        this._physics == null ? (float?) null : this._physics.minFlingVelocity;
                                    instance.maxFlingVelocity =
                                        this._physics == null ? (float?) null : this._physics.maxFlingVelocity;
                                    instance.dragStartBehavior = this.widget.dragStartBehavior;
                                }
                            ));
                        break;
                    case Axis.horizontal:
                        this._gestureRecognizers = new Dictionary<Type, GestureRecognizerFactory>();
                        this._gestureRecognizers.Add(typeof(HorizontalDragGestureRecognizer),
                            new GestureRecognizerFactoryWithHandlers<HorizontalDragGestureRecognizer>(
                                () => new HorizontalDragGestureRecognizer(),
                                instance => {
                                    instance.onDown = this._handleDragDown;
                                    instance.onStart = this._handleDragStart;
                                    instance.onUpdate = this._handleDragUpdate;
                                    instance.onEnd = this._handleDragEnd;
                                    instance.onCancel = this._handleDragCancel;
                                    instance.minFlingDistance =
                                        this._physics == null ? (float?) null : this._physics.minFlingDistance;
                                    instance.minFlingVelocity =
                                        this._physics == null ? (float?) null : this._physics.minFlingVelocity;
                                    instance.maxFlingVelocity =
                                        this._physics == null ? (float?) null : this._physics.maxFlingVelocity;
                                    instance.dragStartBehavior = this.widget.dragStartBehavior;
                                }
                            ));
                        break;
                }
            }

            this._lastCanDrag = canDrag;
            this._lastAxisDirection = this.widget.axis;
            if (this._gestureDetectorKey.currentState != null) {
                this._gestureDetectorKey.currentState.replaceGestureRecognizers(this._gestureRecognizers);
            }
        }

        public TickerProvider vsync {
            get { return this; }
        }

        public void setIgnorePointer(bool value) {
            if (this._shouldIgnorePointer == value) {
                return;
            }

            this._shouldIgnorePointer = value;
            if (this._ignorePointerKey.currentContext != null) {
                var renderBox = (RenderIgnorePointer) this._ignorePointerKey.currentContext.findRenderObject();
                renderBox.ignoring = this._shouldIgnorePointer;
            }
        }

        public BuildContext notificationContext {
            get { return this._gestureDetectorKey.currentContext; }
        }

        public BuildContext storageContext {
            get { return this.context; }
        }

        Drag _drag;

        ScrollHoldController _hold;

        void _handleDragDown(DragDownDetails details) {
            D.assert(this._drag == null);
            D.assert(this._hold == null);
            this._hold = this.position.hold(this._disposeHold);
        }

        void _handleDragStart(DragStartDetails details) {
            D.assert(this._drag == null);
            this._drag = this.position.drag(details, this._disposeDrag);
            D.assert(this._drag != null);
            D.assert(this._hold == null);
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            D.assert(this._hold == null || this._drag == null);
            if (this._drag != null) {
                this._drag.update(details);
            }
        }

        void _handleDragEnd(DragEndDetails details) {
            D.assert(this._hold == null || this._drag == null);
            if (this._drag != null) {
                this._drag.end(details);
            }

            D.assert(this._drag == null);
        }

        void _handleDragCancel() {
            D.assert(this._hold == null || this._drag == null);
            if (this._hold != null) {
                this._hold.cancel();
            }

            if (this._drag != null) {
                this._drag.cancel();
            }

            D.assert(this._hold == null);
            D.assert(this._drag == null);
        }
        
        float _targetScrollOffsetForPointerScroll(PointerScrollEvent e) {
            float delta = this.widget.axis == Axis.horizontal ? e.delta.dx : e.delta.dy;
            return Mathf.Min(Mathf.Max(this.position.pixels + delta, this.position.minScrollExtent),
                this.position.maxScrollExtent);
        }
        
        void _receivedPointerSignal(PointerSignalEvent e) {
            if (e is PointerScrollEvent && this.position != null) {
                float targetScrollOffset = this._targetScrollOffsetForPointerScroll(e as PointerScrollEvent);
                if (targetScrollOffset != this.position.pixels) {
                    GestureBinding.instance.pointerSignalResolver.register(e, this._handlePointerScroll);
                }
            }
        }

        void _handlePointerScroll(PointerEvent e) {
            D.assert(e is PointerScrollEvent);
            float targetScrollOffset = this._targetScrollOffsetForPointerScroll(e as PointerScrollEvent);
            if (targetScrollOffset != this.position.pixels) {
                this.position.jumpTo(targetScrollOffset);
            }
        }

        void _disposeHold() {
            this._hold = null;
        }

        void _disposeDrag() {
            this._drag = null;
        }

        public override Widget build(BuildContext context) {
            D.assert(this.position != null);

            Widget result = new _ScrollableScope(
                scrollable: this,
                position: this.position,
                child: new Listener(
                    onPointerSignal: this._receivedPointerSignal,
                    child: new RawGestureDetector(
                        key: this._gestureDetectorKey,
                        gestures: this._gestureRecognizers,
                        behavior: HitTestBehavior.opaque,
                        child: new IgnorePointer(
                            key: this._ignorePointerKey,
                            ignoring: this._shouldIgnorePointer,
                            child: this.widget.viewportBuilder(context, this.position)
                        )
                    )
                )
            );

            return this._configuration.buildViewportChrome(context, result, this.widget.axisDirection);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<ScrollPosition>("position", this.position));
        }
    }
}