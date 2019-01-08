using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class BouncingScrollSimulation : Simulation {
        public BouncingScrollSimulation(
            double position,
            double velocity,
            double leadingExtent,
            double trailingExtent,
            SpringDescription spring,
            Tolerance tolerance = null
        ) : base(tolerance: tolerance) {
            D.assert(leadingExtent <= trailingExtent);
            D.assert(spring != null);

            this.leadingExtent = leadingExtent;
            this.trailingExtent = trailingExtent;
            this.spring = spring;

            if (position < leadingExtent) {
                this._springSimulation = this._underscrollSimulation(position, velocity);
                this._springTime = double.NegativeInfinity;
            } else if (position > trailingExtent) {
                this._springSimulation = this._overscrollSimulation(position, velocity);
                this._springTime = double.NegativeInfinity;
            } else {
                this._frictionSimulation = new FrictionSimulation(0.135, position, velocity);
                double finalX = this._frictionSimulation.finalX;
                if (velocity > 0.0 && finalX > trailingExtent) {
                    this._springTime = this._frictionSimulation.timeAtX(trailingExtent);
                    this._springSimulation = this._overscrollSimulation(
                        trailingExtent,
                        Math.Min(this._frictionSimulation.dx(this._springTime),
                            maxSpringTransferVelocity)
                    );
                    D.assert(this._springTime.isFinite());
                } else if (velocity < 0.0 && finalX < leadingExtent) {
                    this._springTime = this._frictionSimulation.timeAtX(leadingExtent);
                    this._springSimulation = this._underscrollSimulation(
                        leadingExtent,
                        Math.Min(this._frictionSimulation.dx(this._springTime),
                            maxSpringTransferVelocity)
                    );
                    D.assert(this._springTime.isFinite());
                } else {
                    this._springTime = double.PositiveInfinity;
                }
            }
        }

        const double maxSpringTransferVelocity = 5000.0;

        public readonly double leadingExtent;

        public readonly double trailingExtent;

        public readonly SpringDescription spring;

        readonly FrictionSimulation _frictionSimulation;
        readonly Simulation _springSimulation;
        readonly double _springTime;
        double _timeOffset = 0.0;

        Simulation _underscrollSimulation(double x, double dx) {
            return new ScrollSpringSimulation(this.spring, x, this.leadingExtent, dx);
        }

        Simulation _overscrollSimulation(double x, double dx) {
            return new ScrollSpringSimulation(this.spring, x, this.trailingExtent, dx);
        }

        Simulation _simulation(double time) {
            Simulation simulation;
            if (time > this._springTime) {
                this._timeOffset = this._springTime.isFinite() ? this._springTime : 0.0;
                simulation = this._springSimulation;
            } else {
                this._timeOffset = 0.0;
                simulation = this._frictionSimulation;
            }

            simulation.tolerance = this.tolerance;
            return simulation;
        }

        public override double x(double time) {
            return this._simulation(time).x(time - this._timeOffset);
        }

        public override double dx(double time) {
            return this._simulation(time).dx(time - this._timeOffset);
        }

        public override bool isDone(double time) {
            return this._simulation(time).isDone(time - this._timeOffset);
        }

        public override string ToString() {
            return $"{this.GetType()}(leadingExtent: {this.leadingExtent}, trailingExtent: {this.trailingExtent})";
        }
    }

    public class ClampingScrollSimulation : Simulation {
        public ClampingScrollSimulation(
            double position,
            double velocity,
            double friction = 0.015,
            Tolerance tolerance = null
        ) : base(tolerance: tolerance) {
            D.assert(_flingVelocityPenetration(0.0) ==
                     _initialVelocityPenetration);
            this.position = position;
            this.velocity = velocity;
            this.friction = friction;

            this._duration = this._flingDuration(velocity);
            this._distance = (velocity * this._duration / _initialVelocityPenetration).abs();
        }

        public readonly double position;

        public readonly double velocity;

        public readonly double friction;

        readonly double _duration;

        readonly double _distance;

        static readonly double _kDecelerationRate = Math.Log(0.78) / Math.Log(0.9);

        static double _decelerationForFriction(double friction) {
            return friction * 61774.04968;
        }

        double _flingDuration(double velocity) {
            double scaledFriction = this.friction * _decelerationForFriction(0.84);

            double deceleration = Math.Log(0.35 * velocity.abs() / scaledFriction);

            return Math.Exp(deceleration / (_kDecelerationRate - 1.0));
        }

        const double _initialVelocityPenetration = 3.065;

        static double _flingDistancePenetration(double t) {
            return (1.2 * t * t * t) - (3.27 * t * t) + (_initialVelocityPenetration * t);
        }

        static double _flingVelocityPenetration(double t) {
            return (3.6 * t * t) - (6.54 * t) + _initialVelocityPenetration;
        }

        public override double x(double time) {
            double t = (time / this._duration).clamp(0.0, 1.0);
            return this.position + this._distance * _flingDistancePenetration(t) *
                   this.velocity.sign();
        }

        public override double dx(double time) {
            double t = (time / this._duration).clamp(0.0, 1.0);
            return this._distance * _flingVelocityPenetration(t) * this.velocity.sign() /
                   this._duration;
        }

        public override bool isDone(double time) {
            return time >= this._duration;
        }
    }
}
