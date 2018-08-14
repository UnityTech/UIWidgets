using UIWidgets.ui;
using UnityEngine;

namespace UIWidgets.painting {
    public class MatrixUtils {
        public static Offset transformPoint(Matrix4x4 transform, Offset point) {
            var position3 = new Vector3((float) point.dx, (float) point.dy, 0);
            var transformed3 = transform.MultiplyPoint(position3);
            return new Offset(transformed3.x, transformed3.y);
        }
    }
}