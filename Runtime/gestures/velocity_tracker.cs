using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    public class Velocity : IEquatable<Velocity> {
        public Velocity(
            Offset pixelsPerSecond = null
        ) {
            this.pixelsPerSecond = pixelsPerSecond ?? Offset.zero;
        }

        public static readonly Velocity zero = new Velocity();

        public readonly Offset pixelsPerSecond;

        public static Velocity operator -(Velocity a) {
            return new Velocity(pixelsPerSecond: -a.pixelsPerSecond);
        }

        public static Velocity operator -(Velocity a, Velocity b) {
            return new Velocity(
                pixelsPerSecond: a.pixelsPerSecond - b.pixelsPerSecond);
        }

        public static Velocity operator +(Velocity a, Velocity b) {
            return new Velocity(
                pixelsPerSecond: a.pixelsPerSecond + b.pixelsPerSecond);
        }

        public Velocity clampMagnitude(float minValue, float maxValue) {
            D.assert(minValue >= 0.0);
            D.assert(maxValue >= 0.0 && maxValue >= minValue);
            float valueSquared = this.pixelsPerSecond.distanceSquared;
            if (valueSquared > maxValue * maxValue) {
                return new Velocity(pixelsPerSecond: (this.pixelsPerSecond / this.pixelsPerSecond.distance) * maxValue);
            }

            if (valueSquared < minValue * minValue) {
                return new Velocity(pixelsPerSecond: (this.pixelsPerSecond / this.pixelsPerSecond.distance) * minValue);
            }

            return this;
        }

        public bool Equals(Velocity other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.pixelsPerSecond, other.pixelsPerSecond);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((Velocity) obj);
        }

        public override int GetHashCode() {
            return (this.pixelsPerSecond != null ? this.pixelsPerSecond.GetHashCode() : 0);
        }

        public static bool operator ==(Velocity left, Velocity right) {
            return Equals(left, right);
        }

        public static bool operator !=(Velocity left, Velocity right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return $"Velocity({this.pixelsPerSecond.dx:F1}, {this.pixelsPerSecond.dy:F1})";
        }
    }

    public class VelocityEstimate {
        public VelocityEstimate(
            Offset pixelsPerSecond,
            float confidence,
            TimeSpan duration,
            Offset offset
        ) {
            D.assert(pixelsPerSecond != null);
            D.assert(offset != null);
            this.pixelsPerSecond = pixelsPerSecond;
            this.confidence = confidence;
            this.duration = duration;
            this.offset = offset;
        }

        public readonly Offset pixelsPerSecond;

        public readonly float confidence;

        public readonly TimeSpan duration;

        public readonly Offset offset;

        public override string ToString() {
            return
                $"VelocityEstimate({this.pixelsPerSecond.dx:F1}, {this.pixelsPerSecond.dy:F1}; offset: {this.offset}, duration: {this.duration}, confidence: {this.confidence:F1})";
        }
    }

    class _PointAtTime {
        internal _PointAtTime(Offset point, TimeSpan time) {
            D.assert(point != null);
            this.point = point;
            this.time = time;
        }

        public readonly Offset point;

        public readonly TimeSpan time;

        public override string ToString() {
            return $"_PointAtTime({this.point} at {this.time})";
        }
    }

    public class VelocityTracker {
        const int _assumePointerMoveStoppedMilliseconds = 40;
        const int _historySize = 20;
        const int _horizonMilliseconds = 100;
        const int _minSampleSize = 3;

        readonly List<_PointAtTime> _samples =
            CollectionUtils.CreateRepeatedList<_PointAtTime>(null, _historySize);

        int _index = 0;

        public void addPosition(TimeSpan time, Offset position) {
            this._index += 1;
            if (this._index == _historySize) {
                this._index = 0;
            }

            this._samples[this._index] = new _PointAtTime(position, time);
        }

        public VelocityEstimate getVelocityEstimate() {
            List<float> x = new List<float>();
            List<float> y = new List<float>();
            List<float> w = new List<float>();
            List<float> time = new List<float>();
            int sampleCount = 0;
            int index = this._index;

            _PointAtTime newestSample = this._samples[index];
            if (newestSample == null) {
                return null;
            }

            _PointAtTime previousSample = newestSample;
            _PointAtTime oldestSample = newestSample;

            do {
                _PointAtTime sample = this._samples[index];
                if (sample == null) {
                    break;
                }

                float age = (float) (newestSample.time - sample.time).TotalMilliseconds;
                float delta = Mathf.Abs((float) (sample.time - previousSample.time).TotalMilliseconds);
                previousSample = sample;
                if (age > _horizonMilliseconds ||
                    delta > _assumePointerMoveStoppedMilliseconds) {
                    break;
                }

                oldestSample = sample;
                Offset position = sample.point;
                x.Add(position.dx);
                y.Add(position.dy);
                w.Add(1.0f);
                time.Add(-age);
                index = (index == 0 ? _historySize : index) - 1;

                sampleCount += 1;
            } while (sampleCount < _historySize);

            if (sampleCount >= _minSampleSize) {
                LeastSquaresSolver xSolver = new LeastSquaresSolver(time, x, w);
                PolynomialFit xFit = xSolver.solve(2);
                if (xFit != null) {
                    LeastSquaresSolver ySolver = new LeastSquaresSolver(time, y, w);
                    PolynomialFit yFit = ySolver.solve(2);
                    if (yFit != null) {
                        return new VelocityEstimate(
                            pixelsPerSecond: new Offset(xFit.coefficients[1] * 1000, yFit.coefficients[1] * 1000),
                            confidence: xFit.confidence * yFit.confidence,
                            duration: newestSample.time - oldestSample.time,
                            offset: newestSample.point - oldestSample.point
                        );
                    }
                }
            }

            return new VelocityEstimate(
                pixelsPerSecond: Offset.zero,
                confidence: 1.0f,
                duration: newestSample.time - oldestSample.time,
                offset: newestSample.point - oldestSample.point
            );
        }

        public Velocity getVelocity() {
            VelocityEstimate estimate = this.getVelocityEstimate();
            if (estimate == null || estimate.pixelsPerSecond == Offset.zero) {
                return Velocity.zero;
            }

            return new Velocity(pixelsPerSecond: estimate.pixelsPerSecond);
        }
    }
}