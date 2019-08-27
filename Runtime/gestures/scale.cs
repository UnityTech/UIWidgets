using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    enum _ScaleState {
        ready,
        possible,
        accepted,
        started
    }

    public class ScaleStartDetails {
        public ScaleStartDetails(Offset focalPoint = null) {
            this.focalPoint = focalPoint ?? Offset.zero;
        }

        public readonly Offset focalPoint;

        public override string ToString() {
            return $"ScaleStartDetails(focalPoint: {this.focalPoint}";
        }
    }


    public class ScaleUpdateDetails {
        public ScaleUpdateDetails(
            Offset focalPoint = null,
            float scale = 1.0f,
            float horizontalScale = 1.0f,
            float verticalScale = 1.0f,
            float rotation = 0.0f
        ) {
            focalPoint = focalPoint ?? Offset.zero;

            D.assert(scale >= 0.0f);
            D.assert(horizontalScale >= 0.0f);
            D.assert(verticalScale >= 0.0f);

            this.focalPoint = focalPoint;
            this.scale = scale;
            this.horizontalScale = horizontalScale;
            this.verticalScale = verticalScale;
            this.rotation = rotation;
        }

        public readonly Offset focalPoint;

        public readonly float scale;

        public readonly float horizontalScale;

        public readonly float verticalScale;

        public readonly float rotation;

        public override string ToString() {
            return
                $"ScaleUpdateDetails(focalPoint: {this.focalPoint}, scale: {this.scale}, horizontalScale: {this.horizontalScale}, verticalScale: {this.verticalScale}, rotation: {this.rotation}";
        }
    }

    public class ScaleEndDetails {
        public ScaleEndDetails(Velocity velocity = null) {
            this.velocity = velocity ?? Velocity.zero;
        }

        public readonly Velocity velocity;

        public override string ToString() {
            return $"ScaleEndDetails(velocity: {this.velocity}";
        }
    }

    public delegate void GestureScaleStartCallback(ScaleStartDetails details);

    public delegate void GestureScaleUpdateCallback(ScaleUpdateDetails details);

    public delegate void GestureScaleEndCallback(ScaleEndDetails details);

    static class _ScaleGestureUtils {
        public static bool _isFlingGesture(Velocity velocity) {
            D.assert(velocity != null);
            float speedSquared = velocity.pixelsPerSecond.distanceSquared;
            return speedSquared > Constants.kMinFlingVelocity * Constants.kMinFlingVelocity;
        }
    }

    class _LineBetweenPointers {
        public _LineBetweenPointers(
            Offset pointerStartLocation = null,
            int pointerStartId = 0,
            Offset pointerEndLocation = null,
            int pointerEndId = 1) {
            pointerStartLocation = pointerStartLocation ?? Offset.zero;
            pointerEndLocation = pointerEndLocation ?? Offset.zero;

            D.assert(pointerStartId != pointerEndId);

            this.pointerStartLocation = pointerStartLocation;
            this.pointerStartId = pointerStartId;
            this.pointerEndLocation = pointerEndLocation;
            this.pointerEndId = pointerEndId;
        }

        public readonly Offset pointerStartLocation;

        public readonly int pointerStartId;

        public readonly Offset pointerEndLocation;

        public readonly int pointerEndId;
    }


    public class ScaleGestureRecognizer : OneSequenceGestureRecognizer {
        public ScaleGestureRecognizer(object debugOwner, PointerDeviceKind? kind = null) : base(debugOwner: debugOwner,
            kind: kind) { }

        public GestureScaleStartCallback onStart;

        public GestureScaleUpdateCallback onUpdate;

        public GestureScaleEndCallback onEnd;

        _ScaleState _state = _ScaleState.ready;

        Offset _initialFocalPoint;
        Offset _currentFocalPoint;
        float _initialSpan;
        float _currentSpan;
        float _initialHorizontalSpan;
        float _currentHorizontalSpan;
        float _initialVerticalSpan;
        float _currentVerticalSpan;
        _LineBetweenPointers _initialLine;
        _LineBetweenPointers _currentLine;
        Dictionary<int, Offset> _pointerLocations;
        List<int> _pointerQueue;
        readonly Dictionary<int, VelocityTracker> _velocityTrackers = new Dictionary<int, VelocityTracker>();

        float _scaleFactor {
            get { return this._initialSpan > 0.0f ? this._currentSpan / this._initialSpan : 1.0f; }
        }

        float _horizontalScaleFactor {
            get {
                return this._initialHorizontalSpan > 0.0f
                    ? this._currentHorizontalSpan / this._initialHorizontalSpan
                    : 1.0f;
            }
        }

        float _verticalScaleFactor {
            get {
                return this._initialVerticalSpan > 0.0f ? this._currentVerticalSpan / this._initialVerticalSpan : 1.0f;
            }
        }

        float _computeRotationFactor() {
            if (this._initialLine == null || this._currentLine == null) {
                return 0.0f;
            }

            float fx = this._initialLine.pointerStartLocation.dx;
            float fy = this._initialLine.pointerStartLocation.dy;
            float sx = this._initialLine.pointerEndLocation.dx;
            float sy = this._initialLine.pointerEndLocation.dy;

            float nfx = this._currentLine.pointerStartLocation.dx;
            float nfy = this._currentLine.pointerStartLocation.dy;
            float nsx = this._currentLine.pointerEndLocation.dx;
            float nsy = this._currentLine.pointerEndLocation.dy;

            float angle1 = Mathf.Atan2(fy - sy, fx - sx);
            float angle2 = Mathf.Atan2(nfy - nsy, nfx - nsx);

            return angle2 - angle1;
        }

        public override void addAllowedPointer(PointerDownEvent evt) {
            this.startTrackingPointer(evt.pointer);
            this._velocityTrackers[evt.pointer] = new VelocityTracker();
            if (this._state == _ScaleState.ready) {
                this._state = _ScaleState.possible;
                this._initialSpan = 0.0f;
                this._currentSpan = 0.0f;
                this._initialHorizontalSpan = 0.0f;
                this._currentHorizontalSpan = 0.0f;
                this._initialVerticalSpan = 0.0f;
                this._currentVerticalSpan = 0.0f;
                this._pointerLocations = new Dictionary<int, Offset>();
                this._pointerQueue = new List<int>();
            }
        }

        protected override void handleEvent(PointerEvent evt) {
            D.assert(this._state != _ScaleState.ready);
            bool didChangeConfiguration = false;
            bool shouldStartIfAccepted = false;

            if (evt is PointerMoveEvent) {
                VelocityTracker tracker = this._velocityTrackers[evt.pointer];
                D.assert(tracker != null);
                if (!evt.synthesized) {
                    tracker.addPosition(evt.timeStamp, evt.position);
                }

                this._pointerLocations[evt.pointer] = evt.position;
                shouldStartIfAccepted = true;
            }
            else if (evt is PointerDownEvent) {
                this._pointerLocations[evt.pointer] = evt.position;
                this._pointerQueue.Add(evt.pointer);
                didChangeConfiguration = true;
                shouldStartIfAccepted = true;
            }
            else if (evt is PointerUpEvent || evt is PointerCancelEvent) {
                this._pointerLocations.Remove(evt.pointer);
                this._pointerQueue.Remove(evt.pointer);
                didChangeConfiguration = true;
            }

            this._updateLines();
            this._update();

            if (!didChangeConfiguration || this._reconfigure(evt.pointer)) {
                this._advanceStateMachine(shouldStartIfAccepted);
            }

            this.stopTrackingIfPointerNoLongerDown(evt);
        }

        void _update() {
            int count = this._pointerLocations.Keys.Count;

            Offset focalPoint = Offset.zero;
            foreach (int pointer in this._pointerLocations.Keys) {
                focalPoint += this._pointerLocations[pointer];
            }

            this._currentFocalPoint = count > 0 ? focalPoint / count : Offset.zero;

            float totalDeviation = 0.0f;
            float totalHorizontalDeviation = 0.0f;
            float totalVerticalDeviation = 0.0f;

            foreach (int pointer in this._pointerLocations.Keys) {
                totalDeviation += (this._currentFocalPoint - this._pointerLocations[pointer]).distance;
                totalHorizontalDeviation += (this._currentFocalPoint.dx - this._pointerLocations[pointer].dx).abs();
                totalVerticalDeviation += (this._currentFocalPoint.dy - this._pointerLocations[pointer].dy).abs();
            }

            this._currentSpan = count > 0 ? totalDeviation / count : 0.0f;
            this._currentHorizontalSpan = count > 0 ? totalHorizontalDeviation / count : 0.0f;
            this._currentVerticalSpan = count > 0 ? totalVerticalDeviation / count : 0.0f;
        }

        void _updateLines() {
            int count = this._pointerLocations.Keys.Count;
            D.assert(this._pointerQueue.Count >= count);

            if (count < 2) {
                this._initialLine = this._currentLine;
            }
            else if (this._initialLine != null &&
                     this._initialLine.pointerStartId == this._pointerQueue[0] &&
                     this._initialLine.pointerEndId == this._pointerQueue[1]) {
                this._currentLine = new _LineBetweenPointers(
                    pointerStartId: this._pointerQueue[0],
                    pointerStartLocation: this._pointerLocations[this._pointerQueue[0]],
                    pointerEndId: this._pointerQueue[1],
                    pointerEndLocation: this._pointerLocations[this._pointerQueue[1]]
                );
            }
            else {
                this._initialLine = new _LineBetweenPointers(
                    pointerStartId: this._pointerQueue[0],
                    pointerStartLocation: this._pointerLocations[this._pointerQueue[0]],
                    pointerEndId: this._pointerQueue[1],
                    pointerEndLocation: this._pointerLocations[this._pointerQueue[1]]
                );
                this._currentLine = null;
            }
        }

        bool _reconfigure(int pointer) {
            this._initialFocalPoint = this._currentFocalPoint;
            this._initialSpan = this._currentSpan;
            this._initialLine = this._currentLine;
            this._initialHorizontalSpan = this._currentHorizontalSpan;
            this._initialVerticalSpan = this._currentVerticalSpan;
            if (this._state == _ScaleState.started) {
                if (this.onEnd != null) {
                    VelocityTracker tracker = this._velocityTrackers[pointer];
                    D.assert(tracker != null);

                    Velocity velocity = tracker.getVelocity();
                    if (_ScaleGestureUtils._isFlingGesture(velocity)) {
                        Offset pixelsPerSecond = velocity.pixelsPerSecond;
                        if (pixelsPerSecond.distanceSquared >
                            Constants.kMaxFlingVelocity * Constants.kMaxFlingVelocity) {
                            velocity = new Velocity(
                                pixelsPerSecond: (pixelsPerSecond / pixelsPerSecond.distance) *
                                                 Constants.kMaxFlingVelocity);
                        }

                        this.invokeCallback<object>("onEnd", () => {
                            this.onEnd(new ScaleEndDetails(velocity: velocity));
                            return null;
                        });
                    }
                    else {
                        this.invokeCallback<object>("onEnd", () => {
                            this.onEnd(new ScaleEndDetails(velocity: Velocity.zero));
                            return null;
                        });
                    }
                }

                this._state = _ScaleState.accepted;
                return false;
            }

            return true;
        }

        void _advanceStateMachine(bool shouldStartIfAccepted) {
            if (this._state == _ScaleState.ready) {
                this._state = _ScaleState.possible;
            }

            if (this._state == _ScaleState.possible) {
                float spanDelta = (this._currentSpan - this._initialSpan).abs();
                float focalPointDelta = (this._currentFocalPoint - this._initialFocalPoint).distance;
                if (spanDelta > Constants.kScaleSlop || focalPointDelta > Constants.kPanSlop) {
                    this.resolve(GestureDisposition.accepted);
                }
            }
            else if (this._state >= _ScaleState.accepted) {
                this.resolve(GestureDisposition.accepted);
            }

            if (this._state == _ScaleState.accepted && shouldStartIfAccepted) {
                this._state = _ScaleState.started;
                this._dispatchOnStartCallbackIfNeeded();
            }

            if (this._state == _ScaleState.started && this.onUpdate != null) {
                this.invokeCallback<object>("onUpdate", () => {
                    this.onUpdate(new ScaleUpdateDetails(
                        scale: this._scaleFactor,
                        horizontalScale: this._horizontalScaleFactor,
                        verticalScale: this._verticalScaleFactor,
                        focalPoint: this._currentFocalPoint,
                        rotation: this._computeRotationFactor()
                    ));
                    return null;
                });
            }
        }

        void _dispatchOnStartCallbackIfNeeded() {
            D.assert(this._state == _ScaleState.started);
            if (this.onStart != null) {
                this.invokeCallback<object>("onStart", () => {
                    this.onStart(new ScaleStartDetails(focalPoint: this._currentFocalPoint));
                    return null;
                });
            }
        }

        public override void acceptGesture(int pointer) {
            if (this._state == _ScaleState.possible) {
                this._state = _ScaleState.started;
                this._dispatchOnStartCallbackIfNeeded();
            }
        }

        public override void rejectGesture(int pointer) {
            this.stopTrackingPointer(pointer);
        }

        protected override void didStopTrackingLastPointer(int pointer) {
            switch (this._state) {
                case _ScaleState.possible:
                    this.resolve(GestureDisposition.rejected);
                    break;
                case _ScaleState.ready:
                    D.assert(false);
                    break;
                case _ScaleState.accepted:
                    break;
                case _ScaleState.started:
                    D.assert(false);
                    break;
            }

            this._state = _ScaleState.ready;
        }

        public override void dispose() {
            this._velocityTrackers.Clear();
            base.dispose();
        }

        public override string debugDescription {
            get { return "scale"; }
        }
    }
}