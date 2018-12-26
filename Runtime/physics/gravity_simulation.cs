using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.physics {
    public class GravitySimulation : Simulation {
        public GravitySimulation(
            double acceleration,
            double distance,
            double endDistance,
            double velocity
        ) {
            D.assert(endDistance >= 0);
            this._a = acceleration;
            this._x = distance;
            this._v = velocity;
            this._end = endDistance;
        }

        readonly double _x;
        readonly double _v;
        readonly double _a;
        readonly double _end;

        public override double x(double time) {
            return this._x + this._v * time + 0.5 * this._a * time * time;
        }

        public override double dx(double time) {
            return this._v + time * this._a;
        }

        public override bool isDone(double time) {
            return this.x(time).abs() >= this._end;
        }
    }
}