using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.physics;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public class BouncingScrollSimulation : Simulation {
        public BouncingScrollSimulation(
            float position,
            float velocity,
            float leadingExtent,
            float trailingExtent,
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
                this._springTime = float.NegativeInfinity;
            }
            else if (position > trailingExtent) {
                this._springSimulation = this._overscrollSimulation(position, velocity);
                this._springTime = float.NegativeInfinity;
            }
            else {
                this._frictionSimulation = new FrictionSimulation(0.135f, position, velocity);
                float finalX = this._frictionSimulation.finalX;
                if (velocity > 0.0f && finalX > trailingExtent) {
                    this._springTime = this._frictionSimulation.timeAtX(trailingExtent);
                    this._springSimulation = this._overscrollSimulation(
                        trailingExtent,
                        Mathf.Min(this._frictionSimulation.dx(this._springTime),
                            maxSpringTransferVelocity)
                    );
                    D.assert(this._springTime.isFinite());
                }
                else if (velocity < 0.0f && finalX < leadingExtent) {
                    this._springTime = this._frictionSimulation.timeAtX(leadingExtent);
                    this._springSimulation = this._underscrollSimulation(
                        leadingExtent,
                        Mathf.Min(this._frictionSimulation.dx(this._springTime),
                            maxSpringTransferVelocity)
                    );
                    D.assert(this._springTime.isFinite());
                }
                else {
                    this._springTime = float.PositiveInfinity;
                }
            }
        }

        const float maxSpringTransferVelocity = 5000.0f;

        public readonly float leadingExtent;

        public readonly float trailingExtent;

        public readonly SpringDescription spring;

        readonly FrictionSimulation _frictionSimulation;
        readonly Simulation _springSimulation;
        readonly float _springTime;
        float _timeOffset = 0.0f;

        Simulation _underscrollSimulation(float x, float dx) {
            return new ScrollSpringSimulation(this.spring, x, this.leadingExtent, dx);
        }

        Simulation _overscrollSimulation(float x, float dx) {
            return new ScrollSpringSimulation(this.spring, x, this.trailingExtent, dx);
        }

        Simulation _simulation(float time) {
            Simulation simulation;
            if (time > this._springTime) {
                this._timeOffset = this._springTime.isFinite() ? this._springTime : 0.0f;
                simulation = this._springSimulation;
            }
            else {
                this._timeOffset = 0.0f;
                simulation = this._frictionSimulation;
            }

            simulation.tolerance = this.tolerance;
            return simulation;
        }

        public override float x(float time) {
            return this._simulation(time).x(time - this._timeOffset);
        }

        public override float dx(float time) {
            return this._simulation(time).dx(time - this._timeOffset);
        }

        public override bool isDone(float time) {
            return this._simulation(time).isDone(time - this._timeOffset);
        }

        public override string ToString() {
            return $"{this.GetType()}(leadingExtent: {this.leadingExtent}, trailingExtent: {this.trailingExtent})";
        }
    }

    public class ClampingScrollSimulation : Simulation {
        public ClampingScrollSimulation(
            float position,
            float velocity,
            float friction = 0.015f,
            Tolerance tolerance = null
        ) : base(tolerance: tolerance) {
            D.assert(_flingVelocityPenetration(0.0f) ==
                     _initialVelocityPenetration);
            this.position = position;
            this.velocity = velocity;
            this.friction = friction;

            this._duration = this._flingDuration(velocity);
            this._distance = (velocity * this._duration / _initialVelocityPenetration).abs();
        }

        public readonly float position;

        public readonly float velocity;

        public readonly float friction;

        readonly float _duration;

        readonly float _distance;

        static readonly float _kDecelerationRate = Mathf.Log(0.78f) / Mathf.Log(0.9f);

        static float _decelerationForFriction(float friction) {
            return friction * 61774.04968f;
        }

        float _flingDuration(float velocity) {
            float scaledFriction = this.friction * _decelerationForFriction(0.84f);

            float deceleration = Mathf.Log(0.35f * velocity.abs() / scaledFriction);

            return Mathf.Exp(deceleration / (_kDecelerationRate - 1.0f));
        }

        const float _initialVelocityPenetration = 3.065f;

        static float _flingDistancePenetration(float t) {
            return (1.2f * t * t * t) - (3.27f * t * t) + (_initialVelocityPenetration * t);
        }

        static float _flingVelocityPenetration(float t) {
            return (3.6f * t * t) - (6.54f * t) + _initialVelocityPenetration;
        }

        public override float x(float time) {
            float t = (time / this._duration).clamp(0.0f, 1.0f);
            return (this.position + this._distance * _flingDistancePenetration(t) *
                            this.velocity.sign());
        }

        public override float dx(float time) {
            float t = (time / this._duration).clamp(0.0f, 1.0f);
            return (this._distance * _flingVelocityPenetration(t) * this.velocity.sign() /
                            this._duration);
        }

        public override bool isDone(float time) {
            return time >= this._duration;
        }
    }
}