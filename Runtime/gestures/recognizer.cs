using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    public delegate T RecognizerCallback<T>();

    public abstract class GestureRecognizer : DiagnosticableTree, GestureArenaMember {
        protected GestureRecognizer(object debugOwner = null) {
            this.debugOwner = debugOwner;
        }

        public readonly object debugOwner;

        public abstract void addPointer(PointerDownEvent evt);

        public virtual void addScrollPointer(PointerScrollEvent evt) {
        }

        public virtual void dispose() {
        }

        public abstract string debugDescription { get; }

        protected T invokeCallback<T>(string name, RecognizerCallback<T> callback, Func<string> debugReport = null) {
            D.assert(callback != null);

            T result = default(T);
            try {
                D.assert(() => {
                    if (D.debugPrintRecognizerCallbacksTrace) {
                        var report = debugReport != null ? debugReport() : null;
                        // The 19 in the line below is the width of the prefix used by
                        // _debugLogDiagnostic in arena.dart.
                        var prefix = D.debugPrintGestureArenaDiagnostics ? new string(' ', 19) + "❙ " : "";
                        Debug.LogFormat("{0}this calling {1} callback.{2}",
                            prefix, name, report.isNotEmpty() ? " " + report : "");
                    }

                    return true;
                });

                result = callback();
            }
            catch (Exception ex) {
                UIWidgetsError.reportError(new UIWidgetsErrorDetails(
                    exception: ex,
                    library: "gesture",
                    context: "while handling a gesture",
                    informationCollector: information => {
                        information.AppendLine("Handler: " + name);
                        information.AppendLine("Recognizer:");
                        information.AppendLine("  " + this);
                    }
                ));
            }

            return result;
        }

        public abstract void acceptGesture(int pointer);
        public abstract void rejectGesture(int pointer);

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<object>("debugOwner", this.debugOwner,
                defaultValue: Diagnostics.kNullDefaultValue));
        }
    }

    public abstract class OneSequenceGestureRecognizer : GestureRecognizer {
        protected OneSequenceGestureRecognizer(object debugOwner = null) : base(debugOwner) {
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
                GestureBinding.instance.pointerRouter.removeRoute(pointer, this.handleEvent);
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

            return GestureBinding.instance.gestureArena.add(pointer, this);
        }

        protected void startTrackingScrollerPointer(int pointer) {
            GestureBinding.instance.pointerRouter.addRoute(pointer, this.handleEvent);
        }

        protected void startTrackingPointer(int pointer) {
            GestureBinding.instance.pointerRouter.addRoute(pointer, this.handleEvent);
            this._trackedPointers.Add(pointer);
            D.assert(!this._entries.ContainsKey(pointer));
            this._entries[pointer] = this._addPointerToArena(pointer);
        }

        protected void stopTrackingPointer(int pointer) {
            if (this._trackedPointers.Contains(pointer)) {
                GestureBinding.instance.pointerRouter.removeRoute(pointer, this.handleEvent);
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
            object debugOwner = null
        ) : base(debugOwner: debugOwner) {
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
                    this._timer = Window.instance.run(this.deadline.Value, this.didExceedDeadline);
                }
            }
        }

        protected override void handleEvent(PointerEvent evt) {
            D.assert(this.state != GestureRecognizerState.ready);
            if (this.state == GestureRecognizerState.possible && evt.pointer == this.primaryPointer) {
                if (evt is PointerMoveEvent && this._getDistance(evt) > Constants.kTouchSlop) {
                    this.resolve(GestureDisposition.rejected);
                    this.stopTrackingPointer(this.primaryPointer);
                }
                else {
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

        float _getDistance(PointerEvent evt) {
            Offset offset = evt.position - this.initialPosition;
            return offset.distance;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<GestureRecognizerState>("state", this.state));
        }
    }
}