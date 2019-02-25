using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.physics {
    public class PhysicsUtils {
        public static bool nearEqual(float? a, float? b, float epsilon) {
            D.assert(epsilon >= 0.0);
            if (a == null || b == null) {
                return a == b;
            }

            return (a > (b - epsilon)) && (a < (b + epsilon)) || a == b;
        }

        public static bool nearZero(float? a, float epsilon) {
            return nearEqual(a, 0.0f, epsilon);
        }
    }
}