using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.physics {
    public class ClampedSimulation : Simulation {
        public ClampedSimulation(Simulation simulation,
            float xMin = float.NegativeInfinity,
            float xMax = float.PositiveInfinity,
            float dxMin = float.NegativeInfinity,
            float dxMax = float.PositiveInfinity
        ) {
            D.assert(simulation != null);
            D.assert(xMax >= xMin);
            D.assert(dxMax >= dxMin);

            this.simulation = simulation;
            this.xMin = xMin;
            this.dxMin = dxMin;
            this.dxMax = dxMax;
        }

        public readonly Simulation simulation;

        public readonly float xMin;

        public readonly float xMax;

        public readonly float dxMin;

        public readonly float dxMax;

        public override float x(float time) {
            return this.simulation.x(time).clamp(this.xMin, this.xMax);
        }

        public override float dx(float time) {
            return this.simulation.dx(time).clamp(this.dxMin, this.dxMax);
        }

        public override bool isDone(float time) {
            return this.simulation.isDone(time);
        }
    }
}