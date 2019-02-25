using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.animation {
    enum _AnimationDirection {
        forward,
        reverse,
    }

    public class AnimationController :
        AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationEagerListenerMixinAnimation<float> {
        public AnimationController(
            float? value = null,
            TimeSpan? duration = null,
            string debugLabel = null,
            float lowerBound = 0.0f,
            float upperBound = 1.0f,
            TickerProvider vsync = null
        ) {
            D.assert(upperBound >= lowerBound);
            D.assert(vsync != null);
            this._direction = _AnimationDirection.forward;

            this.duration = duration;
            this.debugLabel = debugLabel;
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;

            this._ticker = vsync.createTicker(this._tick);
            this._internalSetValue(value ?? lowerBound);
        }

        AnimationController(
            float value = 0.0f,
            TimeSpan? duration = null,
            string debugLabel = null,
            TickerProvider vsync = null
        ) {
            D.assert(vsync != null);
            this.lowerBound = float.NegativeInfinity;
            this.upperBound = float.PositiveInfinity;
            this._direction = _AnimationDirection.forward;

            this.duration = duration;
            this.debugLabel = debugLabel;

            this._ticker = vsync.createTicker(this._tick);
            this._internalSetValue(value);
        }

        public static AnimationController unbounded(
            float value = 0.0f,
            TimeSpan? duration = null,
            string debugLabel = null,
            TickerProvider vsync = null
        ) {
            return new AnimationController(value, duration, debugLabel, vsync);
        }

        public readonly float lowerBound;

        public readonly float upperBound;

        public readonly string debugLabel;

        public Animation<float> view {
            get { return this; }
        }

        public TimeSpan? duration;

        Ticker _ticker;

        public void resync(TickerProvider vsync) {
            Ticker oldTicker = this._ticker;
            this._ticker = vsync.createTicker(this._tick);
            this._ticker.absorbTicker(oldTicker);
        }

        Simulation _simulation;

        public override float value {
            get { return this._value; }
        }

        float _value;

        public void setValue(float newValue) {
            this.stop();
            this._internalSetValue(newValue);
            this.notifyListeners();
            this._checkStatusChanged();
        }


        public void reset() {
            this.setValue(this.lowerBound);
        }

        public float velocity {
            get {
                if (!this.isAnimating) {
                    return 0.0f;
                }

                return this._simulation.dx((float) this.lastElapsedDuration.Value.Ticks / TimeSpan.TicksPerSecond);
            }
        }

        void _internalSetValue(float newValue) {
            this._value = newValue.clamp(this.lowerBound, this.upperBound);
            if (this._value == this.lowerBound) {
                this._status = AnimationStatus.dismissed;
            }
            else if (this._value == this.upperBound) {
                this._status = AnimationStatus.completed;
            }
            else {
                this._status = (this._direction == _AnimationDirection.forward)
                    ? AnimationStatus.forward
                    : AnimationStatus.reverse;
            }
        }

        TimeSpan? lastElapsedDuration {
            get { return this._lastElapsedDuration; }
        }

        TimeSpan? _lastElapsedDuration;

        public bool isAnimating {
            get { return this._ticker != null && this._ticker.isActive; }
        }

        _AnimationDirection _direction;

        public override AnimationStatus status {
            get { return this._status; }
        }

        AnimationStatus _status;

        public TickerFuture forward(float? from = null) {
            D.assert(() => {
                if (this.duration == null) {
                    throw new UIWidgetsError(
                        "AnimationController.forward() called with no default Duration.\n" +
                        "The \"duration\" property should be set, either in the constructor or later, before " +
                        "calling the forward() function."
                    );
                }

                return true;
            });
            this._direction = _AnimationDirection.forward;
            if (from != null) {
                this.setValue(from.Value);
            }

            return this._animateToInternal(this.upperBound);
        }

        public TickerFuture reverse(float? from = null) {
            D.assert(() => {
                if (this.duration == null) {
                    throw new UIWidgetsError(
                        "AnimationController.reverse() called with no default Duration.\n" +
                        "The \"duration\" property should be set, either in the constructor or later, before " +
                        "calling the reverse() function."
                    );
                }

                return true;
            });
            this._direction = _AnimationDirection.reverse;
            if (from != null) {
                this.setValue(from.Value);
            }

            return this._animateToInternal(this.lowerBound);
        }

        public TickerFuture animateTo(float target, TimeSpan? duration = null, Curve curve = null) {
            curve = curve ?? Curves.linear;

            this._direction = _AnimationDirection.forward;
            return this._animateToInternal(target, duration: duration, curve: curve);
        }

        TickerFuture _animateToInternal(float target, TimeSpan? duration = null, Curve curve = null) {
            curve = curve ?? Curves.linear;

            TimeSpan? simulationDuration = duration;
            if (simulationDuration == null) {
                D.assert(() => {
                    if (this.duration == null) {
                        throw new UIWidgetsError(
                            "AnimationController.animateTo() called with no explicit Duration and no default Duration.\n" +
                            "Either the \"duration\" argument to the animateTo() method should be provided, or the " +
                            "\"duration\" property should be set, either in the constructor or later, before " +
                            "calling the animateTo() function."
                        );
                    }

                    return true;
                });
                float range = this.upperBound - this.lowerBound;
                float remainingFraction = range.isFinite() ? (target - this._value).abs() / range : 1.0f;
                simulationDuration = TimeSpan.FromTicks((long) (this.duration.Value.Ticks * remainingFraction));
            }
            else if (target == this.value) {
                simulationDuration = TimeSpan.Zero;
            }

            this.stop();

            if (simulationDuration == TimeSpan.Zero) {
                if (this._value != target) {
                    this._value = target.clamp(this.lowerBound, this.upperBound);
                    this.notifyListeners();
                }

                this._status = (this._direction == _AnimationDirection.forward)
                    ? AnimationStatus.completed
                    : AnimationStatus.dismissed;
                this._checkStatusChanged();
                return TickerFutureImpl.complete();
            }

            D.assert(simulationDuration > TimeSpan.Zero);
            D.assert(!this.isAnimating);
            return this._startSimulation(
                new _InterpolationSimulation(this._value, target, simulationDuration.Value, curve));
        }

        public TickerFuture repeat(float? min = null, float? max = null, TimeSpan? period = null) {
            min = min ?? this.lowerBound;
            max = max ?? this.upperBound;
            period = period ?? this.duration;
            D.assert(() => {
                if (period == null) {
                    throw new UIWidgetsError(
                        "AnimationController.repeat() called without an explicit period and with no default Duration.\n" +
                        "Either the \"period\" argument to the repeat() method should be provided, or the " +
                        "\"duration\" property should be set, either in the constructor or later, before " +
                        "calling the repeat() function."
                    );
                }

                return true;
            });
            return this.animateWith(new _RepeatingSimulation(min.Value, max.Value, period.Value));
        }

        public TickerFuture fling(float velocity = 1.0f) {
            this._direction = velocity < 0.0 ? _AnimationDirection.reverse : _AnimationDirection.forward;
            float target = velocity < 0.0f
                ? this.lowerBound - _kFlingTolerance.distance
                : this.upperBound + _kFlingTolerance.distance;
            Simulation simulation = new SpringSimulation(_kFlingSpringDescription, this.value,
                target, velocity);
            simulation.tolerance = _kFlingTolerance;
            return this.animateWith(simulation);
        }


        public TickerFuture animateWith(Simulation simulation) {
            this.stop();
            return this._startSimulation(simulation);
        }

        TickerFuture _startSimulation(Simulation simulation) {
            D.assert(simulation != null);
            D.assert(!this.isAnimating);
            this._simulation = simulation;
            this._lastElapsedDuration = TimeSpan.Zero;
            this._value = simulation.x(0.0f).clamp(this.lowerBound, this.upperBound);
            var result = this._ticker.start();
            this._status = (this._direction == _AnimationDirection.forward)
                ? AnimationStatus.forward
                : AnimationStatus.reverse;
            this._checkStatusChanged();
            return result;
        }

        public void stop(bool canceled = true) {
            this._simulation = null;
            this._lastElapsedDuration = null;
            this._ticker.stop(canceled: canceled);
        }

        public override void dispose() {
            D.assert(() => {
                if (this._ticker == null) {
                    throw new UIWidgetsError(
                        "AnimationController.dispose() called more than once.\n" +
                        "A given " + this.GetType() + " cannot be disposed more than once.\n" +
                        "The following " + this.GetType() + " object was disposed multiple times:\n" +
                        "  " + this);
                }

                return true;
            });
            this._ticker.dispose();
            this._ticker = null;
            base.dispose();
        }

        AnimationStatus _lastReportedStatus = AnimationStatus.dismissed;

        void _checkStatusChanged() {
            AnimationStatus newStatus = this.status;
            if (this._lastReportedStatus != newStatus) {
                this._lastReportedStatus = newStatus;
                this.notifyStatusListeners(newStatus);
            }
        }

        void _tick(TimeSpan elapsed) {
            this._lastElapsedDuration = elapsed;
            float elapsedInSeconds = (float) elapsed.Ticks / TimeSpan.TicksPerSecond;
            D.assert(elapsedInSeconds >= 0.0);
            this._value = this._simulation.x(elapsedInSeconds).clamp(this.lowerBound, this.upperBound);
            if (this._simulation.isDone(elapsedInSeconds)) {
                this._status = (this._direction == _AnimationDirection.forward)
                    ? AnimationStatus.completed
                    : AnimationStatus.dismissed;
                this.stop(canceled: false);
            }

            this.notifyListeners();
            this._checkStatusChanged();
        }

        public override string toStringDetails() {
            string paused = this.isAnimating ? "" : "; paused";
            string ticker = this._ticker == null ? "; DISPOSED" : (this._ticker.muted ? "; silenced" : "");
            string label = this.debugLabel == null ? "" : "; for " + this.debugLabel;
            string more = $"{base.toStringDetails()} {this.value:F3}";
            return more + paused + ticker + label;
        }

        static readonly SpringDescription _kFlingSpringDescription = SpringDescription.withDampingRatio(
            mass: 1.0f,
            stiffness: 500.0f,
            ratio: 1.0f
        );

        static readonly Tolerance _kFlingTolerance = new Tolerance(
            velocity: float.PositiveInfinity,
            distance: 0.01f
        );
    }


    class _InterpolationSimulation : Simulation {
        internal _InterpolationSimulation(float begin, float end, TimeSpan duration, Curve curve) {
            this._begin = begin;
            this._end = end;
            this._curve = curve;

            D.assert(duration.Ticks > 0);
            this._durationInSeconds = (float) duration.Ticks / TimeSpan.TicksPerSecond;
        }

        readonly float _durationInSeconds;
        readonly float _begin;
        readonly float _end;
        readonly Curve _curve;

        public override float x(float timeInSeconds) {
            float t = (timeInSeconds / this._durationInSeconds).clamp(0.0f, 1.0f);
            if (t == 0.0f) {
                return this._begin;
            }
            else if (t == 1.0f) {
                return this._end;
            }
            else {
                return this._begin + (this._end - this._begin) * this._curve.transform(t);
            }
        }

        public override float dx(float timeInSeconds) {
            float epsilon = this.tolerance.time;
            return (this.x(timeInSeconds + epsilon) - this.x(timeInSeconds - epsilon)) / (2 * epsilon);
        }

        public override bool isDone(float timeInSeconds) {
            return timeInSeconds > this._durationInSeconds;
        }
    }

    class _RepeatingSimulation : Simulation {
        internal _RepeatingSimulation(float min, float max, TimeSpan period) {
            this._min = min;
            this._max = max;
            this._periodInSeconds = (float) period.Ticks / TimeSpan.TicksPerSecond;
            D.assert(this._periodInSeconds > 0.0f);
        }

        readonly float _min;
        readonly float _max;
        readonly float _periodInSeconds;

        public override float x(float timeInSeconds) {
            D.assert(timeInSeconds >= 0.0f);
            float t = (timeInSeconds / this._periodInSeconds) % 1.0f;
            return MathUtils.lerpFloat(this._min, this._max, t);
        }

        public override float dx(float timeInSeconds) {
            return (this._max - this._min) / this._periodInSeconds;
        }

        public override bool isDone(float timeInSeconds) {
            return false;
        }
    }
}