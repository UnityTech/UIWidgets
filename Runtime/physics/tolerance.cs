namespace Unity.UIWidgets.physics {
    public class Tolerance {
        public Tolerance(
            double distance = _epsilonDefault,
            double time = _epsilonDefault,
            double velocity = _epsilonDefault
        ) {
            this.distance = distance;
            this.time = time;
            this.velocity = velocity;
        }

        const double _epsilonDefault = 1e-3;

        public static readonly Tolerance defaultTolerance = new Tolerance();

        public readonly double distance;

        public readonly double time;

        public readonly double velocity;

        public override string ToString() {
            return string.Format("Tolerance(distance: ±{0}, time: ±{1}, velocity: ±{2})",
                this.distance, this.time, this.velocity);
        }
    }
}