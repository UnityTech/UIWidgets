using System.Collections.Generic;
using System.Linq;
using RSG.Promises;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    public delegate void GestureDoubleTapCallback(DoubleTapDetails details);

    public class DoubleTapDetails {
        public DoubleTapDetails(Offset firstGlobalPosition = null) {
            this.firstGlobalPosition = firstGlobalPosition ?? Offset.zero;
        }

        public readonly Offset firstGlobalPosition;
    }

    class _TapTracker {
        internal _TapTracker(
            PointerDownEvent evt = null,
            GestureArenaEntry entry = null) {
            this.pointer = evt.pointer;
            this._initialPosition = evt.position;
            this.entry = entry;
        }

        public readonly int pointer;
        public readonly GestureArenaEntry entry;
        internal readonly Offset _initialPosition;

        bool _isTrackingPointer = false;

        public void startTrackingPointer(PointerRoute route) {
            if (!this._isTrackingPointer) {
                this._isTrackingPointer = true;
                GestureBinding.instance.pointerRouter.addRoute(this.pointer, route);
            }
        }

        public void stopTrackingPointer(PointerRoute route) {
            if (this._isTrackingPointer) {
                this._isTrackingPointer = false;
                GestureBinding.instance.pointerRouter.removeRoute(this.pointer, route);
            }
        }

        public bool isWithinTolerance(PointerEvent evt, float tolerance) {
            Offset offset = evt.position - this._initialPosition;
            return offset.distance <= tolerance;
        }
    }


    public class DoubleTapGestureRecognizer : GestureRecognizer {
        public DoubleTapGestureRecognizer(object debugOwner = null, PointerDeviceKind? kind = null)
            : base(debugOwner: debugOwner, kind: kind) {
        }

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
                entry: GestureBinding.instance.gestureArena.add(evt.pointer, this)
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

        public override void acceptGesture(int pointer) {
        }

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
            this._trackers.Values.ToList().Each(this._reject);
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
}