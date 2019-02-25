namespace Unity.UIWidgets.physics {
    public class Tolerance {
        public Tolerance(
            float distance = _epsilonDefault,
            float time = _epsilonDefault,
            float velocity = _epsilonDefault
        ) {
            this.distance = distance;
            this.time = time;
            this.velocity = velocity;
        }

        const float _epsilonDefault = 1e-3f;

        public static readonly Tolerance defaultTolerance = new Tolerance();

        public readonly float distance;

        public readonly float time;

        public readonly float velocity;

        public override string ToString() {
            return $"Tolerance(distance: ±{this.distance}, time: ±{this.time}, velocity: ±{this.velocity})";
        }
    }
}