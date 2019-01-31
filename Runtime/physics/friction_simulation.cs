using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.physics {
    public class FrictionSimulation : Simulation {
        public FrictionSimulation(
            double drag, double position, double velocity,
            Tolerance tolerance = null
        ) : base(tolerance: tolerance) {
            this._drag = drag;
            this._dragLog = Math.Log(drag);
            this._x = position;
            this._v = velocity;
        }

        public static FrictionSimulation through(double startPosition, double endPosition, double startVelocity,
            double endVelocity) {
            D.assert(startVelocity == 0.0 || endVelocity == 0.0 || startVelocity.sign() == endVelocity.sign());
            D.assert(startVelocity.abs() >= endVelocity.abs());
            D.assert((endPosition - startPosition).sign() == startVelocity.sign());

            return new FrictionSimulation(
                _dragFor(startPosition, endPosition, startVelocity, endVelocity),
                startPosition,
                startVelocity,
                tolerance: new Tolerance(velocity: endVelocity.abs())
            );
        }

        readonly double _drag;
        readonly double _dragLog;
        readonly double _x;
        readonly double _v;

        static double _dragFor(double startPosition, double endPosition, double startVelocity, double endVelocity) {
            return Math.Pow(Math.E, (startVelocity - endVelocity) / (startPosition - endPosition));
        }

        public override double x(double time) {
            return this._x + this._v * Math.Pow(this._drag, time) / this._dragLog - this._v / this._dragLog;
        }

        public override double dx(double time) {
            return this._v * Math.Pow(this._drag, time);
        }

        public double finalX {
            get { return this._x - this._v / this._dragLog; }
        }

        public double timeAtX(double x) {
            if (x == this._x) {
                return 0.0;
            }

            if (this._v == 0.0 || (this._v > 0 ? (x < this._x || x > this.finalX) : (x > this._x || x < this.finalX))) {
                return double.PositiveInfinity;
            }

            return Math.Log(this._dragLog * (x - this._x) / this._v + 1.0) / this._dragLog;
        }

        public override bool isDone(double time) {
            return this.dx(time).abs() < this.tolerance.velocity;
        }
    }

    public class BoundedFrictionSimulation : FrictionSimulation {
        BoundedFrictionSimulation(
            double drag,
            double position,
            double velocity,
            double _minX,
            double _maxX
        ) : base(drag, position, velocity) {
            D.assert(position.clamp(_minX, _maxX) == position);
            this._minX = _minX;
            this._maxX = _maxX;
        }

        readonly double _minX;

        readonly double _maxX;

        public override double x(double time) {
            return base.x(time).clamp(this._minX, this._maxX);
        }

        public override bool isDone(double time) {
            return base.isDone(time) ||
                   (this.x(time) - this._minX).abs() < this.tolerance.distance ||
                   (this.x(time) - this._maxX).abs() < this.tolerance.distance;
        }
    }
}