using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.physics {
    public class ClampedSimulation : Simulation {
        public ClampedSimulation(Simulation simulation,
            double xMin = double.NegativeInfinity,
            double xMax = double.PositiveInfinity,
            double dxMin = double.NegativeInfinity,
            double dxMax = double.PositiveInfinity
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

        public readonly double xMin;

        public readonly double xMax;

        public readonly double dxMin;

        public readonly double dxMax;

        public override double x(double time) {
            return this.simulation.x(time).clamp(this.xMin, this.xMax);
        }

        public override double dx(double time) {
            return this.simulation.dx(time).clamp(this.dxMin, this.dxMax);
        }

        public override bool isDone(double time) {
            return this.simulation.isDone(time);
        }
    }
}