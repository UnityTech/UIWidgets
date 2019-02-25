using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.physics {
    public class GravitySimulation : Simulation {
        public GravitySimulation(
            float acceleration,
            float distance,
            float endDistance,
            float velocity
        ) {
            D.assert(endDistance >= 0);
            this._a = acceleration;
            this._x = distance;
            this._v = velocity;
            this._end = endDistance;
        }

        readonly float _x;
        readonly float _v;
        readonly float _a;
        readonly float _end;

        public override float x(float time) {
            return this._x + this._v * time + 0.5f * this._a * time * time;
        }

        public override float dx(float time) {
            return this._v + time * this._a;
        }

        public override bool isDone(float time) {
            return this.x(time).abs() >= this._end;
        }
    }
}