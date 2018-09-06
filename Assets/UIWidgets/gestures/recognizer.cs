using System;
using System.Collections.Generic;
using UIWidgets.async;
using UIWidgets.foundation;
using UIWidgets.ui;
using UnityEngine;

namespace UIWidgets.gestures {
    public delegate T RecognizerCallback<T>();

    public abstract class GestureRecognizer : GestureArenaMember {
        protected GestureRecognizer(GestureBinding binding = null) {
            this._binding = binding;
        }

        protected readonly GestureBinding _binding;

        public abstract void addPointer(PointerDownEvent evt);

        public virtual void dispose() {
        }

        protected T invokeCallback<T>(string name, RecognizerCallback<T> callback) {
            T result = default(T);
            try {
                result = callback();
            }
            catch (Exception ex) {
                Debug.LogError("Error while handling a gesture [" + name + "]: " + ex);
            }

            return result;
        }

        public abstract void acceptGesture(int pointer);
        public abstract void rejectGesture(int pointer);
    }

    public abstract class OneSequenceGestureRecognizer : GestureRecognizer {
        protected OneSequenceGestureRecognizer(GestureBinding binding = null) : base(binding) {
        }

        readonly Dictionary<int, GestureArenaEntry> _entries = new Dictionary<int, GestureArenaEntry>();

        readonly HashSet<int> _trackedPointers = new HashSet<int>();

        protected abstract void handleEvent(PointerEvent evt);

        public override void acceptGesture(int pointer) {
        }

        public override void rejectGesture(int pointer) {
        }

        protected abstract void didStopTrackingLastPointer(int pointer);

        protected virtual void resolve(GestureDisposition disposition) {
            var localEntries = new List<GestureArenaEntry>(this._entries.Values);
            this._entries.Clear();
            foreach (GestureArenaEntry entry in localEntries) {
                entry.resolve(disposition);
            }
        }

        public override void dispose() {
            this.resolve(GestureDisposition.rejected);
            foreach (int pointer in this._trackedPointers) {
                this._binding.pointerRouter.removeRoute(pointer, this.handleEvent);
            }

            this._trackedPointers.Clear();
            D.assert(this._entries.isEmpty());
            base.dispose();
        }

        public GestureArenaTeam team {
            get { return this._team; }
            set {
                D.assert(value != null);
                D.assert(this._entries.isEmpty());
                D.assert(this._trackedPointers.isEmpty());
                D.assert(this._team == null);
                this._team = value;
            }
        }

        GestureArenaTeam _team;

        GestureArenaEntry _addPointerToArena(int pointer) {
            if (this._team != null) {
                return this._team.add(pointer, this);
            }

            return this._binding.gestureArena.add(pointer, this);
        }

        protected void startTrackingPointer(int pointer) {
            this._binding.pointerRouter.addRoute(pointer, this.handleEvent);
            this._trackedPointers.Add(pointer);
            D.assert(!this._entries.ContainsKey(pointer));
            this._entries[pointer] = this._addPointerToArena(pointer);
        }

        protected void stopTrackingPointer(int pointer) {
            if (this._trackedPointers.Contains(pointer)) {
                this._binding.pointerRouter.removeRoute(pointer, this.handleEvent);
                this._trackedPointers.Remove(pointer);
                if (this._trackedPointers.isEmpty()) {
                    this.didStopTrackingLastPointer(pointer);
                }
            }
        }

        protected void stopTrackingIfPointerNoLongerDown(PointerEvent evt) {
            if (evt is PointerUpEvent || evt is PointerCancelEvent) {
                this.stopTrackingPointer(evt.pointer);
            }
        }
    }

    public enum GestureRecognizerState {
        ready,
        possible,
        defunct,
    }

    public abstract class PrimaryPointerGestureRecognizer : OneSequenceGestureRecognizer {
        protected PrimaryPointerGestureRecognizer(
            TimeSpan? deadline = null,
            GestureBinding binding = null
        ) : base(binding: binding) {
            this.deadline = deadline;
        }

        public readonly TimeSpan? deadline;

        public GestureRecognizerState state = GestureRecognizerState.ready;

        public int primaryPointer;

        public Offset initialPosition;

        Timer _timer;

        public override void addPointer(PointerDownEvent evt) {
            this.startTrackingPointer(evt.pointer);
            if (this.state == GestureRecognizerState.ready) {
                this.state = GestureRecognizerState.possible;
                this.primaryPointer = evt.pointer;
                this.initialPosition = evt.position;
                if (this.deadline != null) {
                    this._timer = this._binding.window.run(this.deadline.Value, this.didExceedDeadline);
                }
            }
        }

        protected override void handleEvent(PointerEvent evt) {
            D.assert(this.state != GestureRecognizerState.ready);
            if (this.state == GestureRecognizerState.possible && evt.pointer == this.primaryPointer) {
                if (evt is PointerMoveEvent && this._getDistance(evt) > Constants.kTouchSlop) {
                    this.resolve(GestureDisposition.rejected);
                    this.stopTrackingPointer(this.primaryPointer);
                } else {
                    this.handlePrimaryPointer(evt);
                }
            }

            this.stopTrackingIfPointerNoLongerDown(evt);
        }

        protected abstract void handlePrimaryPointer(PointerEvent evt);

        protected virtual void didExceedDeadline() {
            D.assert(this.deadline == null);
        }

        public override void rejectGesture(int pointer) {
            if (pointer == this.primaryPointer && this.state == GestureRecognizerState.possible) {
                this._stopTimer();
                this.state = GestureRecognizerState.defunct;
            }
        }

        protected override void didStopTrackingLastPointer(int pointer) {
            this._stopTimer();
            this.state = GestureRecognizerState.ready;
        }

        public override void dispose() {
            this._stopTimer();
            base.dispose();
        }

        void _stopTimer() {
            if (this._timer != null) {
                this._timer.cancel();
                this._timer = null;
            }
        }

        double _getDistance(PointerEvent evt) {
            Offset offset = evt.position - this.initialPosition;
            return offset.distance;
        }
    }
}