using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.physics {
    public class FrictionSimulation : Simulation {
        public FrictionSimulation(
            float drag, float position, float velocity,
            Tolerance tolerance = null
        ) : base(tolerance: tolerance) {
            this._drag = drag;
            this._dragLog = Mathf.Log(drag);
            this._x = position;
            this._v = velocity;
        }

        public static FrictionSimulation through(float startPosition, float endPosition, float startVelocity,
            float endVelocity) {
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

        readonly float _drag;
        readonly float _dragLog;
        readonly float _x;
        readonly float _v;

        static float _dragFor(float startPosition, float endPosition, float startVelocity, float endVelocity) {
            return Mathf.Pow((float) Math.E, (startVelocity - endVelocity) / (startPosition - endPosition));
        }

        public override float x(float time) {
            return this._x + this._v * Mathf.Pow(this._drag, time) / this._dragLog - this._v / this._dragLog;
        }

        public override float dx(float time) {
            return this._v * Mathf.Pow(this._drag, time);
        }

        public float finalX {
            get { return this._x - this._v / this._dragLog; }
        }

        public float timeAtX(float x) {
            if (x == this._x) {
                return 0.0f;
            }

            if (this._v == 0.0 || (this._v > 0 ? (x < this._x || x > this.finalX) : (x > this._x || x < this.finalX))) {
                return float.PositiveInfinity;
            }

            return Mathf.Log(this._dragLog * (x - this._x) / this._v + 1.0f) / this._dragLog;
        }

        public override bool isDone(float time) {
            return this.dx(time).abs() < this.tolerance.velocity;
        }
    }

    public class BoundedFrictionSimulation : FrictionSimulation {
        BoundedFrictionSimulation(
            float drag,
            float position,
            float velocity,
            float _minX,
            float _maxX
        ) : base(drag, position, velocity) {
            D.assert(position.clamp(_minX, _maxX) == position);
            this._minX = _minX;
            this._maxX = _maxX;
        }

        readonly float _minX;

        readonly float _maxX;

        public override float x(float time) {
            return base.x(time).clamp(this._minX, this._maxX);
        }

        public override bool isDone(float time) {
            return base.isDone(time) ||
                   (this.x(time) - this._minX).abs() < this.tolerance.distance ||
                   (this.x(time) - this._maxX).abs() < this.tolerance.distance;
        }
    }
}