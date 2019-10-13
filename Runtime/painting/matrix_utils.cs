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

        public static Matrix4x4 createCylindricalProjectionTransform(
            float radius,
            float angle,
            float perspective = 0.001f,
            Axis orientation = Axis.vertical
        ) {
            D.assert(perspective >= 0 && perspective <= 1.0);

            Matrix4x4 result = Matrix4x4.identity;
            result[3, 2] = -perspective;
            result[2, 3] = -radius;
            result[3, 3] = perspective * radius + 1.0f;

            result *= (
                          orientation == Axis.horizontal
                              ? rotationY(angle)
                              : rotationX(angle)
                      ) * translationValues(0.0f, 0.0f, radius);

            return result;
        }

        public static Matrix4x4 rotationY(float angle) {
            var matrix = Matrix4x4.zero;
            matrix[15] = 1.0f;
            float c = Mathf.Cos(angle);
            float s = Mathf.Sin(angle);
            matrix[0] = c;
            matrix[1] = 0.0f;
            matrix[2] = -s;
            matrix[4] = 0.0f;
            matrix[5] = 1.0f;
            matrix[6] = 0.0f;
            matrix[8] = s;
            matrix[9] = 0.0f;
            matrix[10] = c;
            matrix[3] = 0.0f;
            matrix[7] = 0.0f;
            matrix[11] = 0.0f;
            return matrix;
        }

        public static Matrix4x4 rotationX(float angle) {
            var matrix = Matrix4x4.zero;
            matrix[15] = 1.0f;
            float c = Mathf.Cos(angle);
            float s = Mathf.Sin(angle);
            matrix[0] = 1.0f;
            matrix[1] = 0.0f;
            matrix[2] = 0.0f;
            matrix[4] = 0.0f;
            matrix[5] = c;
            matrix[6] = s;
            matrix[8] = 0.0f;
            matrix[9] = -s;
            matrix[10] = c;
            matrix[3] = 0.0f;
            matrix[7] = 0.0f;
            matrix[11] = 0.0f;
            return matrix;
        }

        public static Matrix4x4 translationValues(float x, float y, float z) {
            var matrix = Matrix4x4.identity;
            matrix[14] = z;
            matrix[13] = y;
            matrix[12] = x;
            return matrix;
        }

        public static Matrix4x4 translate(this Matrix4x4 m, Vector4 x, float y = 0.0f, float z = 0.0f) {
            float tw = x.w;
            float tx = x.x;
            float ty = x.y;
            float tz = x.z;

            float t1 = m[0] * tx + m[4] * ty + m[8] * tz + m[12] * tw;
            float t2 = m[1] * tx + m[5] * ty + m[9] * tz + m[13] * tw;
            float t3 = m[2] * tx + m[6] * ty + m[10] * tz + m[14] * tw;
            float t4 = m[3] * tx + m[7] * ty + m[11] * tz + m[15] * tw;
            m[12] = t1;
            m[13] = t2;
            m[14] = t3;
            m[15] = t4;
            return m;
        }

        public static Matrix4x4 translate(this Matrix4x4 m, Vector3 x, float y = 0.0f, float z = 0.0f) {
            float tw = 1.0f;
            float tx = x.x;
            float ty = x.y;
            float tz = x.z;

            float t1 = m[0] * tx + m[4] * ty + m[8] * tz + m[12] * tw;
            float t2 = m[1] * tx + m[5] * ty + m[9] * tz + m[13] * tw;
            float t3 = m[2] * tx + m[6] * ty + m[10] * tz + m[14] * tw;
            float t4 = m[3] * tx + m[7] * ty + m[11] * tz + m[15] * tw;
            m[12] = t1;
            m[13] = t2;
            m[14] = t3;
            m[15] = t4;
            return m;
        }

        public static Matrix4x4 translate(this Matrix4x4 m, float x, float y = 0.0f, float z = 0.0f) {
            float tw = 1.0f;
            float tx = x;
            float ty = y;
            float tz = z;

            float t1 = m[0] * tx + m[4] * ty + m[8] * tz + m[12] * tw;
            float t2 = m[1] * tx + m[5] * ty + m[9] * tz + m[13] * tw;
            float t3 = m[2] * tx + m[6] * ty + m[10] * tz + m[14] * tw;
            float t4 = m[3] * tx + m[7] * ty + m[11] * tz + m[15] * tw;
            m[12] = t1;
            m[13] = t2;
            m[14] = t3;
            m[15] = t4;
            return m;
        }

        public static void multiply(this Matrix3 m, Matrix3 arg) {
            float m00 = m[0];
            float m01 = m[3];
            float m02 = m[6];
            float m10 = m[1];
            float m11 = m[4];
            float m12 = m[7];
            float m20 = m[2];
            float m21 = m[5];
            float m22 = m[8];
            List<float> argStorage = new List<float> {
                arg[0], arg[1], arg[2], arg[3], arg[4], arg[5], arg[6], arg[7], arg[8]
            };
            float n00 = argStorage[0];
            float n01 = argStorage[3];
            float n02 = argStorage[6];
            float n10 = argStorage[1];
            float n11 = argStorage[4];
            float n12 = argStorage[7];
            float n20 = argStorage[2];
            float n21 = argStorage[5];
            float n22 = argStorage[8];
            m[0] = (m00 * n00) + (m01 * n10) + (m02 * n20);
            m[3] = (m00 * n01) + (m01 * n11) + (m02 * n21);
            m[6] = (m00 * n02) + (m01 * n12) + (m02 * n22);
            m[1] = (m10 * n00) + (m11 * n10) + (m12 * n20);
            m[4] = (m10 * n01) + (m11 * n11) + (m12 * n21);
            m[7] = (m10 * n02) + (m11 * n12) + (m12 * n22);
            m[2] = (m20 * n00) + (m21 * n10) + (m22 * n20);
            m[5] = (m20 * n01) + (m21 * n11) + (m22 * n21);
            m[8] = (m20 * n02) + (m21 * n12) + (m22 * n22);
        }

        public static void multiply(this Matrix4x4 m, Matrix4x4 arg) {
            float m00 = m[0];
            float m01 = m[4];
            float m02 = m[8];
            float m03 = m[12];
            float m10 = m[1];
            float m11 = m[5];
            float m12 = m[9];
            float m13 = m[13];
            float m20 = m[2];
            float m21 = m[6];
            float m22 = m[10];
            float m23 = m[14];
            float m30 = m[3];
            float m31 = m[7];
            float m32 = m[11];
            float m33 = m[15];
            List<float> argStorage = new List<float> {
                arg.m00, arg.m01, arg.m02, arg.m03,
                arg.m10, arg.m11, arg.m12, arg.m13,
                arg.m20, arg.m21, arg.m22, arg.m23,
                arg.m30, arg.m31, arg.m32, arg.m33,
            };
            float n00 = argStorage[0];
            float n01 = argStorage[4];
            float n02 = argStorage[8];
            float n03 = argStorage[12];
            float n10 = argStorage[1];
            float n11 = argStorage[5];
            float n12 = argStorage[9];
            float n13 = argStorage[13];
            float n20 = argStorage[2];
            float n21 = argStorage[6];
            float n22 = argStorage[10];
            float n23 = argStorage[14];
            float n30 = argStorage[3];
            float n31 = argStorage[7];
            float n32 = argStorage[11];
            float n33 = argStorage[15];
            m[0] = (m00 * n00) + (m01 * n10) + (m02 * n20) + (m03 * n30);
            m[4] = (m00 * n01) + (m01 * n11) + (m02 * n21) + (m03 * n31);
            m[8] = (m00 * n02) + (m01 * n12) + (m02 * n22) + (m03 * n32);
            m[12] = (m00 * n03) + (m01 * n13) + (m02 * n23) + (m03 * n33);
            m[1] = (m10 * n00) + (m11 * n10) + (m12 * n20) + (m13 * n30);
            m[5] = (m10 * n01) + (m11 * n11) + (m12 * n21) + (m13 * n31);
            m[9] = (m10 * n02) + (m11 * n12) + (m12 * n22) + (m13 * n32);
            m[13] = (m10 * n03) + (m11 * n13) + (m12 * n23) + (m13 * n33);
            m[2] = (m20 * n00) + (m21 * n10) + (m22 * n20) + (m23 * n30);
            m[6] = (m20 * n01) + (m21 * n11) + (m22 * n21) + (m23 * n31);
            m[10] = (m20 * n02) + (m21 * n12) + (m22 * n22) + (m23 * n32);
            m[14] = (m20 * n03) + (m21 * n13) + (m22 * n23) + (m23 * n33);
            m[3] = (m30 * n00) + (m31 * n10) + (m32 * n20) + (m33 * n30);
            m[7] = (m30 * n01) + (m31 * n11) + (m32 * n21) + (m33 * n31);
            m[11] = (m30 * n02) + (m31 * n12) + (m32 * n22) + (m33 * n32);
            m[15] = (m30 * n03) + (m31 * n13) + (m32 * n23) + (m33 * n33);
        }

        public static void scale(this Matrix4x4 m, object x, float? y = null, float? z = null) {
            float sx = 0f;
            float sy = 0f;
            float sz = 0f;
            float sw = x is Vector4 _xv4 ? _xv4.w : 1.0f;
            if (x is Vector3 xv3) {
                sx = xv3.x;
                sy = xv3.y;
                sz = xv3.z;
            }
            else if (x is Vector4 xv4) {
                sx = xv4.x;
                sy = xv4.y;
                sz = xv4.z;
            }
            else if (x is float xf) {
                sx = xf;
                sy = y ?? xf;
                sz = z ?? xf;
            }

            m[0] *= sx;
            m[1] *= sx;
            m[2] *= sx;
            m[3] *= sx;
            m[4] *= sy;
            m[5] *= sy;
            m[6] *= sy;
            m[7] *= sy;
            m[8] *= sz;
            m[9] *= sz;
            m[10] *= sz;
            m[11] *= sz;
            m[12] *= sw;
            m[13] *= sw;
            m[14] *= sw;
            m[15] *= sw;
        }

        public static Rect inverseTransformRect(Matrix4x4 transform, Rect rect) {
            D.assert(rect != null);
            D.assert(transform.determinant != 0.0);
            if (transform == Matrix4x4.identity)
                return rect;
            var copy = transform;
            copy.invert();
            transform = copy;
            return transformRect(transform, rect);
        }

        public static float invert(this Matrix4x4 m) {
            return m.copyInverse();
        }

        public static float copyInverse(this Matrix4x4 m) {
            List<float> argStorage = new List<float> {
                m.m00, m.m01, m.m02, m.m03,
                m.m10, m.m11, m.m12, m.m13,
                m.m20, m.m21, m.m22, m.m23,
                m.m30, m.m31, m.m32, m.m33,
            };
            float a00 = argStorage[0];
            float a01 = argStorage[1];
            float a02 = argStorage[2];
            float a03 = argStorage[3];
            float a10 = argStorage[4];
            float a11 = argStorage[5];
            float a12 = argStorage[6];
            float a13 = argStorage[7];
            float a20 = argStorage[8];
            float a21 = argStorage[9];
            float a22 = argStorage[10];
            float a23 = argStorage[11];
            float a30 = argStorage[12];
            float a31 = argStorage[13];
            float a32 = argStorage[14];
            float a33 = argStorage[15];
            float b00 = a00 * a11 - a01 * a10;
            float b01 = a00 * a12 - a02 * a10;
            float b02 = a00 * a13 - a03 * a10;
            float b03 = a01 * a12 - a02 * a11;
            float b04 = a01 * a13 - a03 * a11;
            float b05 = a02 * a13 - a03 * a12;
            float b06 = a20 * a31 - a21 * a30;
            float b07 = a20 * a32 - a22 * a30;
            float b08 = a20 * a33 - a23 * a30;
            float b09 = a21 * a32 - a22 * a31;
            float b10 = a21 * a33 - a23 * a31;
            float b11 = a22 * a33 - a23 * a32;
            float det =
                (b00 * b11 - b01 * b10 + b02 * b09 + b03 * b08 - b04 * b07 + b05 * b06);
            if (det == 0.0f) {
                m[0] = argStorage[0];
                m[1] = argStorage[1];
                m[2] = argStorage[2];
                m[3] = argStorage[3];
                m[4] = argStorage[4];
                m[5] = argStorage[5];
                m[6] = argStorage[6];
                m[7] = argStorage[7];
                m[8] = argStorage[8];
                m[9] = argStorage[9];
                m[10] = argStorage[10];
                m[11] = argStorage[11];
                m[12] = argStorage[12];
                m[13] = argStorage[13];
                m[14] = argStorage[14];
                m[15] = argStorage[15];
                return 0.0f;
            }

            float invDet = 1.0f / det;
            m[0] = (a11 * b11 - a12 * b10 + a13 * b09) * invDet;
            m[1] = (-a01 * b11 + a02 * b10 - a03 * b09) * invDet;
            m[2] = (a31 * b05 - a32 * b04 + a33 * b03) * invDet;
            m[3] = (-a21 * b05 + a22 * b04 - a23 * b03) * invDet;
            m[4] = (-a10 * b11 + a12 * b08 - a13 * b07) * invDet;
            m[5] = (a00 * b11 - a02 * b08 + a03 * b07) * invDet;
            m[6] = (-a30 * b05 + a32 * b02 - a33 * b01) * invDet;
            m[7] = (a20 * b05 - a22 * b02 + a23 * b01) * invDet;
            m[8] = (a10 * b10 - a11 * b08 + a13 * b06) * invDet;
            m[9] = (-a00 * b10 + a01 * b08 - a03 * b06) * invDet;
            m[10] = (a30 * b04 - a31 * b02 + a33 * b00) * invDet;
            m[11] = (-a20 * b04 + a21 * b02 - a23 * b00) * invDet;
            m[12] = (-a10 * b09 + a11 * b07 - a12 * b06) * invDet;
            m[13] = (a00 * b09 - a01 * b07 + a02 * b06) * invDet;
            m[14] = (-a30 * b03 + a31 * b01 - a32 * b00) * invDet;
            m[15] = (a20 * b03 - a21 * b01 + a22 * b00) * invDet;
            return det;
        }

        public static Matrix3 toMatrix3(this Matrix4x4 m) {
            var m3 = Matrix3.I();
            m3[0] = m[0];
            m3[1] = m[1];
            m3[2] = m[2];
            m3[3] = m[3];
            m3[4] = m[4];
            m3[5] = m[5];
            m3[6] = m[6];
            m3[7] = m[7];
            m3[8] = m[8];
            return m3;
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