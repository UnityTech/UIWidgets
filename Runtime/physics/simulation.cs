namespace Unity.UIWidgets.physics {
    public abstract class Simulation {
        protected Simulation(Tolerance tolerance = null) {
            this.tolerance = tolerance ?? Tolerance.defaultTolerance;
        }

        public abstract float x(float time);

        public abstract float dx(float time);

        public abstract bool isDone(float time);

        public Tolerance tolerance;
    }
}