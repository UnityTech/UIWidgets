namespace Unity.UIWidgets.physics {
    public abstract class Simulation {
        protected Simulation(Tolerance tolerance = null) {
            this.tolerance = tolerance ?? Tolerance.defaultTolerance;
        }

        public abstract double x(double time);

        public abstract double dx(double time);

        public abstract bool isDone(double time);

        public Tolerance tolerance;
    }
}