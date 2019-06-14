using UnityEngine;

namespace Unity.UIWidgets.ui {
    static class XformUtils {
        public static float getAverageScale(Matrix3 matrix) {
            return (getScaleX(matrix) + getScaleY(matrix)) * 0.5f;
        }

        public static float getMaxScale(Matrix3 matrix) {
            return Mathf.Max(getScaleX(matrix), getScaleY(matrix));
        }

        public static float getScaleX(Matrix3 matrix) {
            // ignore perspective parameters for now.
            if (matrix.isIdentity()) {
                return 1.0f;
            }

            if (matrix.getSkewY() == 0) {
                return matrix.getScaleX();
            }
            
            var x = matrix.getScaleX();
            var y = matrix.getSkewY();
            
            return Mathf.Sqrt(x * x + y * y);
        }

        public static float getScaleY(Matrix3 matrix) {
            // ignore perspective parameters for now.
            if (matrix.isIdentity()) {
                return 1.0f;
            }

            if (matrix.getSkewX() == 0) {
                return matrix.getScaleY();
            }

            var x = matrix.getSkewX();
            var y = matrix.getScaleY();

            return Mathf.Sqrt(x * x + y * y);
        }

        public static float getScale(Matrix3 matrix) {
            var scaleX = getScaleX(matrix);
            var scaleY = getScaleY(matrix);

            if (scaleX == 1.0) {
                return scaleY;
            }

            if (scaleY == 1.0) {
                return scaleX;
            }

            // geometric mean of len0 and len1.
            return Mathf.Sqrt(scaleX * scaleY);
        }
        
        public static float mapRadius(Matrix3 matrix, float radius) {
            return getScale(matrix) * radius;
        }
    }
}