using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    public delegate Drag GestureMultiDragStartCallback(Offset position);

    public abstract class MultiDragPointerState {
        public MultiDragPointerState(
            Offset initialPosition = null) {
            D.assert(initialPosition != null);
            this.initialPosition = initialPosition;
        }

        public readonly Offset initialPosition;

        readonly VelocityTracker _velocityTracker = new VelocityTracker();

        Drag _client;

        public Offset pendingDelta {
            get { return this._pendingDelta; }
        }

        public Offset _pendingDelta = Offset.zero;

        TimeSpan? _lastPendingEventTimestamp;

        GestureArenaEntry _arenaEntry;

        public void _setArenaEntry(GestureArenaEntry entry) {
            D.assert(this._arenaEntry == null);
            D.assert(this.pendingDelta != null);
            D.assert(this._client == null);
            this._arenaEntry = entry;
        }

        protected void resolve(GestureDisposition disposition) {
            this._arenaEntry.resolve(disposition);
        }

        public void _move(PointerMoveEvent pEvent) {
            D.assert(this._arenaEntry != null);
            if (!pEvent.synthesized) {
                this._velocityTracker.addPosition(pEvent.timeStamp, pEvent.position);
            }

            if (this._client != null) {
                D.assert(this.pendingDelta == null);
                this._client.update(new DragUpdateDetails(
                    sourceTimeStamp: pEvent.timeStamp,
                    delta: pEvent.delta,
                    globalPosition: pEvent.position
                ));
            }
            else {
                D.assert(this.pendingDelta != null);
                this._pendingDelta += pEvent.delta;
                this._lastPendingEventTimestamp = pEvent.timeStamp;
                this.checkForResolutionAfterMove();
            }
        }

        public virtual void checkForResolutionAfterMove() {
        }

        public abstract void accepted(GestureMultiDragStartCallback starter);

        public void rejected() {
            D.assert(this._arenaEntry != null);
            D.assert(this._client == null);
            D.assert(this.pendingDelta != null);
            this._pendingDelta = null;
            this._lastPendingEventTimestamp = null;
            this._arenaEntry = null;
        }

        public void _startDrag(Drag client) {
            D.assert(this._arenaEntry != null);
            D.assert(this._client == null);
            D.assert(client != null);
            D.assert(this.pendingDelta != null);

            this._client = client;
            DragUpdateDetails details = new DragUpdateDetails(
                sourceTimeStamp: this._lastPendingEventTimestamp ?? TimeSpan.Zero,
                this.pendingDelta,
                globalPosition: this.initialPosition
            );

            this._pendingDelta = null;
            this._lastPendingEventTimestamp = null;
            this._client.update(details);
        }

        public void _up() {
            D.assert(this._arenaEntry != null);
            if (this._client != null) {
                D.assert(this.pendingDelta == null);
                DragEndDetails details = new DragEndDetails(velocity: this._velocityTracker.getVelocity());
                Drag client = this._client;
                this._client = null;
                client.end(details);
            }
            else {
                D.assert(this.pendingDelta != null);
                this._pendingDelta = null;
                this._lastPendingEventTimestamp = null;
            }
        }

        public void _cancel() {
            D.assert(this._arenaEntry != null);
            if (this._client != null) {
                D.assert(this.pendingDelta == null);
                Drag client = this._client;
                this._client = null;
                client.cancel();
            }
            else {
                D.assert(this.pendingDelta != null);
                this._pendingDelta = null;
                this._lastPendingEventTimestamp = null;
            }
        }

        public virtual void dispose() {
            this._arenaEntry?.resolve(GestureDisposition.rejected);
            this._arenaEntry = null;
            D.assert(() => {
                this._pendingDelta = null;
                return true;
            });
        }
    }


    public abstract class MultiDragGestureRecognizer<T> : GestureRecognizer where T : MultiDragPointerState {
        protected MultiDragGestureRecognizer(
            object debugOwner, PointerDeviceKind? kind = null) : base(debugOwner: debugOwner, kind: kind) {
        }

        public GestureMultiDragStartCallback onStart;

        Dictionary<int, T> _pointers = new Dictionary<int, T>();

        public override void addAllowedPointer(PointerDownEvent pEvent) {
            D.assert(this._pointers != null);
            D.assert(pEvent.position != null);
            D.assert(!this._pointers.ContainsKey(pEvent.pointer));

            T state = this.createNewPointerState(pEvent);
            this._pointers[pEvent.pointer] = state;
            GestureBinding.instance.pointerRouter.addRoute(pEvent.pointer, this._handleEvent);
            state._setArenaEntry(GestureBinding.instance.gestureArena.add(pEvent.pointer, this));
        }

        public abstract T createNewPointerState(PointerDownEvent pEvent);

        void _handleEvent(PointerEvent pEvent) {
            D.assert(this._pointers != null);
            D.assert(pEvent.timeStamp != null);
            D.assert(pEvent.position != null);
            D.assert(this._pointers.ContainsKey(pEvent.pointer));

            T state = this._pointers[pEvent.pointer];
            if (pEvent is PointerMoveEvent) {
                state._move((PointerMoveEvent) pEvent);
            }
            else if (pEvent is PointerUpEvent) {
                D.assert(pEvent.delta == Offset.zero);
                state._up();
                this._removeState(pEvent.pointer);
            }
            else if (pEvent is PointerCancelEvent) {
                D.assert(pEvent.delta == Offset.zero);
                state._cancel();
                this._removeState(pEvent.pointer);
            }
            else if (!(pEvent is PointerDownEvent)) {
                D.assert(false);
            }
        }

        public override void acceptGesture(int pointer) {
            D.assert(this._pointers != null);
            T state = this._pointers[pointer];
            if (state == null) {
                return;
            }

            state.accepted((Offset initialPosition) => this._startDrag(initialPosition, pointer));
        }

        Drag _startDrag(Offset initialPosition, int pointer) {
            D.assert(this._pointers != null);
            T state = this._pointers[pointer];
            D.assert(state != null);
            D.assert(state._pendingDelta != null);
            Drag drag = null;
            if (this.onStart != null) {
                drag = this.invokeCallback("onStart", () => this.onStart(initialPosition));
            }

            if (drag != null) {
                state._startDrag(drag);
            }
            else {
                this._removeState(pointer);
            }

            return drag;
        }

        public override void rejectGesture(int pointer) {
            D.assert(this._pointers != null);
            if (this._pointers.ContainsKey(pointer)) {
                T state = this._pointers[pointer];
                D.assert(state != null);
                state.rejected();
                this._removeState(pointer);
            }
        }

        void _removeState(int pointer) {
            if (this._pointers == null) {
                return;
            }

            D.assert(this._pointers.ContainsKey(pointer));
            GestureBinding.instance.pointerRouter.removeRoute(pointer, this._handleEvent);
            this._pointers[pointer].dispose();
            this._pointers.Remove(pointer);
        }


        public override void dispose() {
            foreach (var key in this._pointers.Keys) {
                this._removeState(key);
            }
            D.assert(this._pointers.isEmpty);
            this._pointers = null;
            base.dispose();
        }
    }


    public class _ImmediatePointerState : MultiDragPointerState {
        public _ImmediatePointerState(Offset initialPosition) : base(initialPosition) {
        }

        public override void checkForResolutionAfterMove() {
            D.assert(this.pendingDelta != null);
            if (this.pendingDelta.distance > Constants.kTouchSlop) {
                this.resolve(GestureDisposition.accepted);
            }
        }

        public override void accepted(GestureMultiDragStartCallback starter) {
            starter(this.initialPosition);
        }
    }


    public class ImmediateMultiDragGestureRecognizer : MultiDragGestureRecognizer<_ImmediatePointerState> {
        public ImmediateMultiDragGestureRecognizer(object debugOwner, PointerDeviceKind? kind = null) : base(
            debugOwner: debugOwner, kind: kind) {
        }

        public override _ImmediatePointerState createNewPointerState(PointerDownEvent pEvent) {
            return new _ImmediatePointerState(pEvent.position);
        }

        public override string debugDescription {
            get { return "multidrag"; }
        }
    }

    public class _HorizontalPointerState : MultiDragPointerState {
        public _HorizontalPointerState(Offset initialPosition) : base(initialPosition) {
        }

        public override void checkForResolutionAfterMove() {
            D.assert(this.pendingDelta != null);
            if (this.pendingDelta.dx.abs() > Constants.kTouchSlop) {
                this.resolve(GestureDisposition.accepted);
            }
        }

        public override void accepted(GestureMultiDragStartCallback starter) {
            starter(this.initialPosition);
        }
    }

    public class HorizontalMultiDragGestureRecognizer : MultiDragGestureRecognizer<_HorizontalPointerState> {
        public HorizontalMultiDragGestureRecognizer(object debugOwner, PointerDeviceKind? kind = null) : base(
            debugOwner: debugOwner, kind: kind) {
        }

        public override _HorizontalPointerState createNewPointerState(PointerDownEvent pEvent) {
            return new _HorizontalPointerState(pEvent.position);
        }

        public override string debugDescription {
            get { return "horizontal multidrag"; }
        }
    }


    public class _VerticalPointerState : MultiDragPointerState {
        public _VerticalPointerState(Offset initialPosition) : base(initialPosition) {
        }

        public override void checkForResolutionAfterMove() {
            D.assert(this.pendingDelta != null);
            if (this.pendingDelta.dy.abs() > Constants.kTouchSlop) {
                this.resolve(GestureDisposition.accepted);
            }
        }

        public override void accepted(GestureMultiDragStartCallback starter) {
            starter(this.initialPosition);
        }
    }


    public class VerticalMultiDragGestureRecognizer : MultiDragGestureRecognizer<_VerticalPointerState> {
        public VerticalMultiDragGestureRecognizer(object debugOwner, PointerDeviceKind? kind = null) : base(
            debugOwner: debugOwner, kind: kind) {
        }

        public override _VerticalPointerState createNewPointerState(PointerDownEvent pEvent) {
            return new _VerticalPointerState(pEvent.position);
        }

        public override string debugDescription {
            get { return "vertical multidrag"; }
        }
    }

    public class _DelayedPointerState : MultiDragPointerState {
        public _DelayedPointerState(
            Offset initialPosition = null,
            TimeSpan? delay = null)
            : base(initialPosition) {
            D.assert(delay != null);
            this._timer = Window.instance.run(delay ?? Constants.kLongPressTimeout, this._delayPassed, false);
        }

        Timer _timer;
        GestureMultiDragStartCallback _starter;

        void _delayPassed() {
            D.assert(this._timer != null);
            D.assert(this.pendingDelta != null);
            D.assert(this.pendingDelta.distance <= Constants.kTouchSlop);
            this._timer = null;
            if (this._starter != null) {
                this._starter(this.initialPosition);
                this._starter = null;
            }
            else {
                this.resolve(GestureDisposition.accepted);
            }

            D.assert(this._starter == null);
        }

        void _ensureTimerStopped() {
            this._timer?.cancel();
            this._timer = null;
        }

        public override void accepted(GestureMultiDragStartCallback starter) {
            D.assert(this._starter == null);
            if (this._timer == null) {
                starter(this.initialPosition);
            }
            else {
                this._starter = starter;
            }
        }

        public override void checkForResolutionAfterMove() {
            if (this._timer == null) {
                D.assert(this._starter != null);
                return;
            }

            D.assert(this.pendingDelta != null);
            if (this.pendingDelta.distance > Constants.kTouchSlop) {
                this.resolve(GestureDisposition.rejected);
                this._ensureTimerStopped();
            }
        }

        public override void dispose() {
            this._ensureTimerStopped();
            base.dispose();
        }
    }


    public class DelayedMultiDragGestureRecognizer : MultiDragGestureRecognizer<_DelayedPointerState> {
        public DelayedMultiDragGestureRecognizer(
            TimeSpan? delay = null,
            object debugOwner = null,
            PointerDeviceKind? kind = null) : base(debugOwner: debugOwner, kind: kind) {
            if (delay == null) {
                delay = Constants.kLongPressTimeout;
            }

            this.delay = delay;
        }

        readonly TimeSpan? delay;

        public override _DelayedPointerState createNewPointerState(PointerDownEvent pEvent) {
            return new _DelayedPointerState(pEvent.position, this.delay);
        }

        public override string debugDescription {
            get { return "long multidrag"; }
        }
    }
}