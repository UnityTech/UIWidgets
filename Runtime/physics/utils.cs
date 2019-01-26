using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.physics {
    public class PhysicsUtils {
        public static bool nearEqual(double? a, double? b, double epsilon) {
            D.assert(epsilon >= 0.0);
            if (a == null || b == null) {
                return a == b;
            }

            return (a > (b - epsilon)) && (a < (b + epsilon)) || a == b;
        }

        public static bool nearZero(double? a, double epsilon) {
            return nearEqual(a, 0.0, epsilon);
        }
    }
}