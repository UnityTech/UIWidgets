using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public static class MatrixUtils {
        public static Offset getAsTranslation(this Matrix3 matrix3) {
            return matrix3.isTranslate() ? new Offset(matrix3[Matrix3.kMTransX], matrix3[Matrix3.kMTransY]) : null;
        }

        public static List<string> debugDescribeTransform(Matrix3 transform) {
            if (transform == null) {
                return new List<string> {"null"};
            }

            List<string> result = new List<string>(3);
            for (int i = 0; i < 3; i++) {
                result.Add($"[{i}] {transform[i * 3]}, {transform[i * 3 + 1]}, {transform[i * 3 + 2]}");
            }

            return result;
        }

        public static Vector3 perspectiveTransform(this Matrix4x4 m4, Vector3 arg) {
            List<float> argStorage = new List<float> {arg[0], arg[1], arg[2]};
            float x_ = (m4[0] * argStorage[0]) +
                       (m4[4] * argStorage[1]) +
                       (m4[8] * argStorage[2]) +
                       m4[12];
            float y_ = (m4[1] * argStorage[0]) +
                       (m4[5] * argStorage[1]) +
                       (m4[9] * argStorage[2]) +
                       m4[13];
            float z_ = (m4[2] * argStorage[0]) +
                       (m4[6] * argStorage[1]) +
                       (m4[10] * argStorage[2]) +
                       m4[14];
            float w_ = 1.0f /
                       ((m4[3] * argStorage[0]) +
                        (m4[7] * argStorage[1]) +
                        (m4[11] * argStorage[2]) +
                        m4[15]);
            argStorage[0] = x_ * w_;
            argStorage[1] = y_ * w_;
            argStorage[2] = z_ * w_;
            return arg;
        }

        public static Offset transformPoint(Matrix4x4 transform, Offset point) {
            Vector3 position3 = new Vector3(point.dx, point.dy, 0.0f);
            Vector3 transformed3 = transform.perspectiveTransform(position3);
            return new Offset(transformed3.x, transformed3.y);
        }

        public static Rect transformRect(Matrix4x4 transform, Rect rect) {
            Offset point1 = transformPoint(transform, rect.topLeft);
            Offset point2 = transformPoint(transform, rect.topRight);
            Offset point3 = transformPoint(transform, rect.bottomLeft);
            Offset point4 = transformPoint(transform, rect.bottomRight);
            return Rect.fromLTRB(
                _min4(point1.dx, point2.dx, point3.dx, point4.dx),
                _min4(point1.dy, point2.dy, point3.dy, point4.dy),
                _max4(point1.dx, point2.dx, point3.dx, point4.dx),
                _max4(point1.dy, point2.dy, point3.dy, point4.dy)
            );
        }

        static float _min4(float a, float b, float c, float d) {
            return Mathf.Min(a, Mathf.Min(b, Mathf.Min(c, d)));
        }

        static float _max4(float a, float b, float c, float d) {
            return Mathf.Max(a, Mathf.Max(b, Mathf.Max(c, d)));
        }

        public static Matrix4x4 toMatrix4x4(this Matrix3 matrix3) {
            var matrix = Matrix4x4.identity;

            matrix[0, 0] = matrix3[0]; // row 0
            matrix[0, 1] = matrix3[1];
            matrix[0, 3] = matrix3[2];

            matrix[1, 0] = matrix3[3]; // row 1
            matrix[1, 1] = matrix3[4];
            matrix[1, 3] = matrix3[5];

            matrix[3, 0] = matrix3[6]; // row 2
            matrix[3, 1] = matrix3[7];
            matrix[3, 3] = matrix3[8];

            return matrix;
        }
    }

    public class TransformProperty : DiagnosticsProperty<Matrix3> {
        public TransformProperty(string name, Matrix3 value,
            bool showName = true,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(name, value, showName: showName, defaultValue: defaultValue ?? Diagnostics.kNoDefaultValue,
            level: level) { }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (parentConfiguration != null && !parentConfiguration.lineBreakProperties) {
                return this.value == null ? "null" : this.value.ToString();
            }

            return string.Join("\n", MatrixUtils.debugDescribeTransform(this.value));
        }
    }
}