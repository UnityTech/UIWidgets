using UIWidgets.foundation;

namespace UIWidgets.physics {
    public class PhysicsUtils {
        public static bool nearEqual(double a, double b, double epsilon) {
            D.assert(epsilon >= 0.0);
            return (a > (b - epsilon)) && (a < (b + epsilon)) || a == b;
        }

        public static bool nearZero(double a, double epsilon) {
            return nearEqual(a, 0.0, epsilon);
        }
    }
}