using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    public delegate T RecognizerCallback<T>();

    public enum DragStartBehavior {
        down,
        start
    }

    public abstract class GestureRecognizer : DiagnosticableTree, GestureArenaMember {
        protected GestureRecognizer(object debugOwner = null, PointerDeviceKind? kind = null) {
            this.debugOwner = debugOwner;
            this._kind = kind;
        }

        public readonly object debugOwner;

        readonly PointerDeviceKind? _kind;

        public void addPointer(PointerDownEvent evt) {
            if (this.isPointerAllowed(evt)) {
                this.addAllowedPointer(evt);
            }
            else {
                this.handleNonAllowedPointer(evt);
            }
        }

        public abstract void addAllowedPointer(PointerDownEvent evt);

        protected virtual void handleNonAllowedPointer(PointerDownEvent evt) {
        }

        protected virtual bool isPointerAllowed(PointerDownEvent evt) {
            return this._kind == null || this._kind == evt.kind;
        }

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
        protected OneSequenceGestureRecognizer(object debugOwner = null, PointerDeviceKind? kind = null) : base(
            debugOwner, kind) {
        }

        readonly Dictionary<int, GestureArenaEntry> _entries = new Dictionary<int, GestureArenaEntry>();

        readonly HashSet<int> _trackedPointers = new HashSet<int>();

        protected abstract void handleEvent(PointerEvent evt);

        public override void acceptGesture(int pointer) {
        }

        public override void rejectGesture(int pointer) {
        }

        protected abstract void didStopTrackingLastPointer(int pointer);

        protected virtual void didStopTrackingLastScrollerPointer(int pointer) {
        }

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

        protected void stopTrackingScrollerPointer(int pointer) {
            if (this._trackedPointers.isEmpty()) {
                this.didStopTrackingLastScrollerPointer(pointer);
            }
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
        accepted,
        defunct,
    }

    public abstract class PrimaryPointerGestureRecognizer : OneSequenceGestureRecognizer {
        protected PrimaryPointerGestureRecognizer(
            TimeSpan? deadline = null,
            object debugOwner = null,
            PointerDeviceKind? kind = null,
            float? preAcceptSlopTolerance = Constants.kTouchSlop,
            float? postAcceptSlopTolerance = Constants.kTouchSlop
        ) : base(debugOwner: debugOwner, kind: kind) {
            D.assert(preAcceptSlopTolerance == null || preAcceptSlopTolerance >= 0,
                () => "The preAcceptSlopTolerance must be positive or null");

            D.assert(postAcceptSlopTolerance == null || postAcceptSlopTolerance >= 0,
                () => "The postAcceptSlopTolerance must be positive or null");

            this.deadline = deadline;
            this.preAcceptSlopTolerance = preAcceptSlopTolerance;
            this.postAcceptSlopTolerance = postAcceptSlopTolerance;
        }

        public readonly TimeSpan? deadline;

        public readonly float? preAcceptSlopTolerance;

        public readonly float? postAcceptSlopTolerance;

        public GestureRecognizerState state = GestureRecognizerState.ready;

        public int primaryPointer;

        public Offset initialPosition;

        Timer _timer;

        public override void addAllowedPointer(PointerDownEvent evt) {
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

            if (evt.pointer == this.primaryPointer) {
                bool isPreAcceptSlopPastTolerance = this.state == GestureRecognizerState.possible &&
                                                    this.preAcceptSlopTolerance != null &&
                                                    this._getDistance(evt) > this.preAcceptSlopTolerance;
                bool isPostAcceptSlopPastTolerance = this.state == GestureRecognizerState.accepted &&
                                                     this.postAcceptSlopTolerance != null &&
                                                     this._getDistance(evt) > this.postAcceptSlopTolerance;

                if (evt is PointerMoveEvent && (isPreAcceptSlopPastTolerance || isPostAcceptSlopPastTolerance)) {
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

        public override void acceptGesture(int pointer) {
            if (pointer == this.primaryPointer && this.state == GestureRecognizerState.possible) {
                this.state = GestureRecognizerState.accepted;
            }
        }

        public override void rejectGesture(int pointer) {
            if (pointer == this.primaryPointer && (this.state == GestureRecognizerState.possible ||
                                                   this.state == GestureRecognizerState.accepted)) {
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