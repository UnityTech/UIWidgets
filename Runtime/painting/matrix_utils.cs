using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

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
            level: level) {
        }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (parentConfiguration != null && !parentConfiguration.lineBreakProperties) {
                return this.value == null ? "null" : this.value.ToString();
            }

            return string.Join("\n", MatrixUtils.debugDescribeTransform(this.value));
        }
    }
}