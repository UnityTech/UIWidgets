namespace Unity.UIWidgets.painting {
    public abstract class Gradient {
        public abstract Gradient scale(double factor);

        public static Gradient lerp(Gradient a, Gradient b, double t) {
            return null;
        }
    }
}
