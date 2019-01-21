using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public static class MatrixUtils {
        
        public static Offset getAsTranslation(this Matrix3 matrix3) {
            return matrix3.isTranslate() ? new Offset(matrix3[Matrix3.kMTransX], matrix3[Matrix3.kMTransY]) : null;
        }
        
        public static List<string> debugDescribeTransform(Matrix3 transform) {
            if (transform == null)
                return new List<string> {"null"};
            
            List<string> result = new List<string>(3);
            for (int i = 0; i < 3; i++) {
                result.Add($"[{i}] {transform[i * 3]}, {transform[i * 3 + 1]}, {transform[i * 3 + 2]}");
            }

            return result;
        }
    }

    public class TransformProperty : DiagnosticsProperty<Matrix3> {

        public TransformProperty(string name, Matrix3 value,
            bool showName = true,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.info
        ): base(name, value, showName: showName, defaultValue: defaultValue??Diagnostics.kNoDefaultValue, level: level) {
        }

        protected override string valueToString(TextTreeConfiguration parentConfiguration = null) {
            if (parentConfiguration != null && !parentConfiguration.lineBreakProperties) {
                return this.value == null ? "null" : this.value.ToString();
            }

            return string.Join("\n", MatrixUtils.debugDescribeTransform(this.value));
        }
    }
}
