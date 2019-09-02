using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    public delegate void GestureDoubleTapCallback(DoubleTapDetails details);

    public delegate void GestureMultiTapDownCallback(int pointer, TapDownDetails details);

    public delegate void GestureMultiTapUpCallback(int pointer, TapUpDetails details);

    public delegate void GestureMultiTapCallback(int pointer);

    public delegate void GestureMultiTapCancelCallback(int pointer);

    class _CountdownZoned {
        public _CountdownZoned(TimeSpan duration) {
            D.assert(duration != null);
            this._timer = Window.instance.run(duration, this._onTimeout);
        }

        public bool _timeout = false;
        public Timer _timer;

        public bool timeout {
            get { return this._timeout; }
        }

        void _onTimeout() {
            this._timeout = true;
        }
    }

    public class DoubleTapDetails {
        public DoubleTapDetails(Offset firstGlobalPosition = null) {
            this.firstGlobalPosition = firstGlobalPosition ?? Offset.zero;
        }

        public readonly Offset firstGlobalPosition;
    }

    class _TapTracker {
        internal _TapTracker(
            PointerDownEvent evt,
            TimeSpan doubleTapMinTime,
            GestureArenaEntry entry = null
        ) {
            this.pointer = evt.pointer;
            this._initialPosition = evt.position;
            this._doubleTapMinTimeCountdown = new _CountdownZoned(duration: doubleTapMinTime);
            this.entry = entry;
        }

        public readonly int pointer;
        public readonly GestureArenaEntry entry;
        internal readonly Offset _initialPosition;
        internal readonly _CountdownZoned _doubleTapMinTimeCountdown;

        bool _isTrackingPointer = false;

        public void startTrackingPointer(PointerRoute route) {
            if (!this._isTrackingPointer) {
                this._isTrackingPointer = true;
                GestureBinding.instance.pointerRouter.addRoute(this.pointer, route);
            }
        }

        public virtual void stopTrackingPointer(PointerRoute route) {
            if (this._isTrackingPointer) {
                this._isTrackingPointer = false;
                GestureBinding.instance.pointerRouter.removeRoute(this.pointer, route);
            }
        }

        public bool isWithinTolerance(PointerEvent evt, float tolerance) {
            Offset offset = evt.position - this._initialPosition;
            return offset.distance <= tolerance;
        }

        public bool hasElapsedMinTime() {
            return this._doubleTapMinTimeCountdown.timeout;
        }
    }


    public class DoubleTapGestureRecognizer : GestureRecognizer {
        public DoubleTapGestureRecognizer(object debugOwner = null, PointerDeviceKind? kind = null)
            : base(debugOwner: debugOwner, kind: kind) { }

        public GestureDoubleTapCallback onDoubleTap;

        Timer _doubleTapTimer;
        _TapTracker _firstTap;
        readonly Dictionary<int, _TapTracker> _trackers = new Dictionary<int, _TapTracker>();

        public override void addAllowedPointer(PointerDownEvent evt) {
            if (this._firstTap != null &&
                !this._firstTap.isWithinTolerance(evt, Constants.kDoubleTapSlop)) {
                return;
            }

            this._stopDoubleTapTimer();
            _TapTracker tracker = new _TapTracker(
                evt: evt,
                entry: GestureBinding.instance.gestureArena.add(evt.pointer, this),
                doubleTapMinTime: Constants.kDoubleTapMinTime
            );
            this._trackers[evt.pointer] = tracker;
            tracker.startTrackingPointer(this._handleEvent);
        }

        void _handleEvent(PointerEvent evt) {
            _TapTracker tracker = this._trackers[evt.pointer];
            D.assert(tracker != null);
            if (evt is PointerUpEvent) {
                if (this._firstTap == null) {
                    this._registerFirstTap(tracker);
                }
                else {
                    this._registerSecondTap(tracker);
                }
            }
            else if (evt is PointerMoveEvent) {
                if (!tracker.isWithinTolerance(evt, Constants.kDoubleTapTouchSlop)) {
                    this._reject(tracker);
                }
            }
            else if (evt is PointerCancelEvent) {
                this._reject(tracker);
            }
        }

        public override void acceptGesture(int pointer) { }

        public override void rejectGesture(int pointer) {
            _TapTracker tracker;
            this._trackers.TryGetValue(pointer, out tracker);

            if (tracker == null &&
                this._firstTap != null &&
                this._firstTap.pointer == pointer) {
                tracker = this._firstTap;
            }

            if (tracker != null) {
                this._reject(tracker);
            }
        }

        void _reject(_TapTracker tracker) {
            this._trackers.Remove(tracker.pointer);
            tracker.entry.resolve(GestureDisposition.rejected);
            this._freezeTracker(tracker);
            if (this._firstTap != null &&
                (this._trackers.isEmpty() || tracker == this._firstTap)) {
                this._reset();
            }
        }

        public override void dispose() {
            this._reset();
            base.dispose();
        }

        void _reset() {
            this._stopDoubleTapTimer();
            if (this._firstTap != null) {
                _TapTracker tracker = this._firstTap;
                this._firstTap = null;
                this._reject(tracker);
                GestureBinding.instance.gestureArena.release(tracker.pointer);
            }

            this._clearTrackers();
        }

        void _registerFirstTap(_TapTracker tracker) {
            this._startDoubleTapTimer();
            GestureBinding.instance.gestureArena.hold(tracker.pointer);
            this._freezeTracker(tracker);
            this._trackers.Remove(tracker.pointer);
            this._clearTrackers();
            this._firstTap = tracker;
        }

        void _registerSecondTap(_TapTracker tracker) {
            var initialPosition = tracker._initialPosition;
            this._firstTap.entry.resolve(GestureDisposition.accepted);
            tracker.entry.resolve(GestureDisposition.accepted);
            this._freezeTracker(tracker);
            this._trackers.Remove(tracker.pointer);
            if (this.onDoubleTap != null) {
                this.invokeCallback<object>("onDoubleTap", () => {
                    this.onDoubleTap(new DoubleTapDetails(initialPosition));
                    return null;
                });
            }

            this._reset();
        }

        void _clearTrackers() {
            foreach (var tracker in this._trackers.Values) {
                this._reject(tracker);
            }

            D.assert(this._trackers.isEmpty());
        }

        void _freezeTracker(_TapTracker tracker) {
            tracker.stopTrackingPointer(this._handleEvent);
        }

        void _startDoubleTapTimer() {
            this._doubleTapTimer =
                this._doubleTapTimer
                ?? Window.instance.run(Constants.kDoubleTapTimeout, this._reset);
        }

        void _stopDoubleTapTimer() {
            if (this._doubleTapTimer != null) {
                this._doubleTapTimer.cancel();
                this._doubleTapTimer = null;
            }
        }

        public override string debugDescription {
            get { return "double tap"; }
        }
    }

    class _TapGesture : _TapTracker {
        public _TapGesture(
            MultiTapGestureRecognizer gestureRecognizer,
            PointerEvent evt,
            TimeSpan longTapDelay
        ) : base(
            evt: (PointerDownEvent) evt,
            entry: GestureBinding.instance.gestureArena.add(evt.pointer, gestureRecognizer),
            doubleTapMinTime: Constants.kDoubleTapMinTime
        ) {
            this.gestureRecognizer = gestureRecognizer;
            this._lastPosition = evt.position;
            this.startTrackingPointer(this.handleEvent);
            if (longTapDelay > TimeSpan.Zero) {
                this._timer = Window.instance.run(longTapDelay, () => {
                    this._timer = null;
                    this.gestureRecognizer._dispatchLongTap(evt.pointer, this._lastPosition);
                });
            }
        }

        public readonly MultiTapGestureRecognizer gestureRecognizer;

        bool _wonArena = false;
        Timer _timer;

        Offset _lastPosition;
        Offset _finalPosition;

        void handleEvent(PointerEvent evt) {
            D.assert(evt.pointer == this.pointer);
            if (evt is PointerMoveEvent) {
                if (!this.isWithinTolerance(evt, Constants.kTouchSlop)) {
                    this.cancel();
                }
                else {
                    this._lastPosition = evt.position;
                }
            }
            else if (evt is PointerCancelEvent) {
                this.cancel();
            }
            else if (evt is PointerUpEvent) {
                this.stopTrackingPointer(this.handleEvent);
                this._finalPosition = evt.position;
                this._check();
            }
        }

        public override void stopTrackingPointer(PointerRoute route) {
            this._timer?.cancel();
            this._timer = null;
            base.stopTrackingPointer(route);
        }

        public void accept() {
            this._wonArena = true;
            this._check();
        }

        public void reject() {
            this.stopTrackingPointer(this.handleEvent);
            this.gestureRecognizer._dispatchCancel(this.pointer);
        }

        public void cancel() {
            if (this._wonArena) {
                this.reject();
            }
            else {
                this.entry.resolve(GestureDisposition.rejected);
            }
        }

        void _check() {
            if (this._wonArena && this._finalPosition != null) {
                this.gestureRecognizer._dispatchTap(this.pointer, this._finalPosition);
            }
        }
    }

    public class MultiTapGestureRecognizer : GestureRecognizer {
        public MultiTapGestureRecognizer(
            TimeSpan? longTapDelay = null,
            object debugOwner = null,
            PointerDeviceKind? kind = null
        ) : base(debugOwner: debugOwner, kind: kind) {
            this.longTapDelay = longTapDelay ?? TimeSpan.Zero;
        }

        public GestureMultiTapDownCallback onTapDown;

        public GestureMultiTapUpCallback onTapUp;

        public GestureMultiTapCallback onTap;

        public GestureMultiTapCancelCallback onTapCancel;

        public TimeSpan longTapDelay;

        public GestureMultiTapDownCallback onLongTapDown;

        readonly Dictionary<int, _TapGesture> _gestureMap = new Dictionary<int, _TapGesture>();

        public override void addAllowedPointer(PointerDownEvent evt) {
            D.assert(!this._gestureMap.ContainsKey(evt.pointer));
            this._gestureMap[evt.pointer] = new _TapGesture(
                gestureRecognizer: this,
                evt: evt,
                longTapDelay: this.longTapDelay
            );
            if (this.onTapDown != null) {
                this.invokeCallback<object>("onTapDown", () => {
                    this.onTapDown(evt.pointer, new TapDownDetails(globalPosition: evt.position));
                    return null;
                });
            }
        }

        public override void acceptGesture(int pointer) {
            D.assert(this._gestureMap.ContainsKey(pointer));
            this._gestureMap[pointer].accept();
        }

        public override void rejectGesture(int pointer) {
            D.assert(this._gestureMap.ContainsKey(pointer));
            this._gestureMap[pointer].reject();
            D.assert(!this._gestureMap.ContainsKey(pointer));
        }

        public void _dispatchCancel(int pointer) {
            D.assert(this._gestureMap.ContainsKey(pointer));
            this._gestureMap.Remove(pointer);
            if (this.onTapCancel != null) {
                this.invokeCallback<object>("onTapCancel", () => {
                    this.onTapCancel(pointer);
                    return null;
                });
            }
        }

        public void _dispatchTap(int pointer, Offset globalPosition) {
            D.assert(this._gestureMap.ContainsKey(pointer));
            this._gestureMap.Remove(pointer);
            if (this.onTapUp != null) {
                this.invokeCallback<object>("onTapUp",
                    () => {
                        this.onTapUp(pointer, new TapUpDetails(globalPosition: globalPosition));
                        return null;
                    });
            }

            if (this.onTap != null) {
                this.invokeCallback<object>("onTap", () => {
                    this.onTap(pointer);
                    return null;
                });
            }
        }

        public void _dispatchLongTap(int pointer, Offset lastPosition) {
            D.assert(this._gestureMap.ContainsKey(pointer));
            if (this.onLongTapDown != null) {
                this.invokeCallback<object>("onLongTapDown",
                    () => {
                        this.onLongTapDown(pointer, new TapDownDetails(globalPosition: lastPosition));
                        return null;
                    });
            }
        }

        public override void dispose() {
            List<_TapGesture> localGestures = new List<_TapGesture>();
            foreach (var item in this._gestureMap) {
                localGestures.Add(item.Value);
            }

            foreach (_TapGesture gesture in localGestures) {
                gesture.cancel();
            }

            D.assert(this._gestureMap.isEmpty);
            base.dispose();
        }

        public override string debugDescription {
            get { return "multitap"; }
        }
    }
}