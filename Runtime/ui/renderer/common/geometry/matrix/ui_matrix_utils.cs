using System;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public partial struct uiMatrix3 {
        public void mapPoints(ref uiOffset[] dst, ref uiOffset[] src) {
            D.assert(dst != null && src != null && dst.Length == src.Length);
            this._getMapPtsProc()(this, ref dst, ref src, src.Length);
        }

        public void mapPoints(ref uiOffset[] pts) {
            this.mapPoints(ref pts, ref pts);
        }

        delegate void MapPtsProc(uiMatrix3 mat, ref uiOffset[] dst, ref uiOffset[] src, int count);

        static readonly MapPtsProc[] gMapPtsProcs = {
            Identity_pts, Trans_pts,
            Scale_pts, Scale_pts,
            Affine_pts, Affine_pts,
            Affine_pts, Affine_pts,
            // repeat the persp proc 8 times
            Persp_pts, Persp_pts,
            Persp_pts, Persp_pts,
            Persp_pts, Persp_pts,
            Persp_pts, Persp_pts
        };

        static MapPtsProc GetMapPtsProc(TypeMask mask) {
            D.assert(((int) mask & ~kAllMasks) == 0);
            return gMapPtsProcs[(int) mask & kAllMasks];
        }

        MapPtsProc _getMapPtsProc() {
            return GetMapPtsProc(this._getType());
        }

        static void Identity_pts(uiMatrix3 m, ref uiOffset[] dst, ref uiOffset[] src, int count) {
            D.assert(m._getType() == 0);

            if (dst != src && count > 0) {
                Array.Copy(src, dst, count);
            }
        }

        static void Trans_pts(uiMatrix3 m, ref uiOffset[] dst, ref uiOffset[] src, int count) {
            D.assert(m._getType() <= TypeMask.kTranslate_Mask);
            if (count > 0) {
                var tx = m.getTranslateX();
                var ty = m.getTranslateY();
                for (int i = 0; i < count; ++i) {
                    dst[i] = new uiOffset(src[i].dx + tx, src[i].dy + ty);
                }
            }
        }

        static void Scale_pts(uiMatrix3 m, ref uiOffset[] dst, ref uiOffset[] src, int count) {
            D.assert(m._getType() <= (TypeMask.kScale_Mask | TypeMask.kTranslate_Mask));
            if (count > 0) {
                var tx = m.getTranslateX();
                var ty = m.getTranslateY();
                var sx = m.getScaleX();
                var sy = m.getScaleY();

                for (int i = 0; i < count; ++i) {
                    dst[i] = new uiOffset(src[i].dx * sx + tx, src[i].dy * sy + ty);
                }
            }
        }

        static void Persp_pts(uiMatrix3 m, ref uiOffset[] dst, ref uiOffset[] src, int count) {
            D.assert(m._hasPerspective());

            if (count > 0) {
                for (int i = 0; i < count; ++i) {
                    var sy = src[i].dy;
                    var sx = src[i].dx;
                    var x = uiScalarUtils.sdot(sx, m.kMScaleX, sy, m.kMSkewX) +
                            m.kMTransX;
                    var y = uiScalarUtils.sdot(sx, m.kMSkewY, sy, m.kMScaleY) +
                            m.kMTransY;
                    var z = uiScalarUtils.sdot(sx, m.kMPersp0, sy, m.kMPersp1) +
                            m.kMPersp2;
                    if (z != 0) {
                        z = 1 / z;
                    }

                    dst[i] = new uiOffset(x * z, y * z);
                }
            }
        }

        static void Affine_pts(uiMatrix3 m, ref uiOffset[] dst, ref uiOffset[] src, int count) {
            D.assert(m._getType() != TypeMask.kPerspective_Mask);
            if (count > 0) {
                var tx = m.getTranslateX();
                var ty = m.getTranslateY();
                var sx = m.getScaleX();
                var sy = m.getScaleY();
                var kx = m.getSkewX();
                var ky = m.getSkewY();

                for (int i = 0; i < count; ++i) {
                    dst[i] = new uiOffset(
                        src[i].dx * sx + src[i].dy * kx + tx,
                        src[i].dx * ky + src[i].dy * sy + ty);
                }
            }
        }
    }


    public partial struct uiMatrix3 {
        delegate void MapXYProc(uiMatrix3 mat, float x, float y, out float x1, out float y1);

        static readonly MapXYProc[] gMapXYProcs = {
            Identity_xy, Trans_xy,
            Scale_xy, ScaleTrans_xy,
            Rot_xy, RotTrans_xy,
            Rot_xy, RotTrans_xy,
            // repeat the persp proc 8 times
            Persp_xy, Persp_xy,
            Persp_xy, Persp_xy,
            Persp_xy, Persp_xy,
            Persp_xy, Persp_xy
        };

        static MapXYProc GetMapXYProc(TypeMask mask) {
            D.assert(((int) mask & ~kAllMasks) == 0);
            return gMapXYProcs[(int) mask & kAllMasks];
        }

        MapXYProc _getMapXYProc() {
            return GetMapXYProc(this._getType());
        }

        static void Identity_xy(uiMatrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert(0 == m._getType());

            resX = sx;
            resY = sy;
        }

        static void Trans_xy(uiMatrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert(m._getType() == TypeMask.kTranslate_Mask);

            resX = sx + m.kMTransX;
            resY = sy + m.kMTransY;
        }

        static void Scale_xy(uiMatrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert((m._getType() & (TypeMask.kScale_Mask | TypeMask.kAffine_Mask | TypeMask.kPerspective_Mask))
                     == TypeMask.kScale_Mask);
            D.assert(0 == m.kMTransX);
            D.assert(0 == m.kMTransY);

            resX = sx * m.kMScaleX;
            resY = sy * m.kMScaleY;
        }

        static void ScaleTrans_xy(uiMatrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert((m._getType() & (TypeMask.kScale_Mask | TypeMask.kAffine_Mask | TypeMask.kPerspective_Mask))
                     == TypeMask.kScale_Mask);

            resX = sx * m.kMScaleX + m.kMTransX;
            resY = sy * m.kMScaleY + m.kMTransY;
        }

        static void Rot_xy(uiMatrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert((m._getType() & (TypeMask.kAffine_Mask | TypeMask.kPerspective_Mask)) == TypeMask.kAffine_Mask);
            D.assert(0 == m.kMTransX);
            D.assert(0 == m.kMTransY);

            resX = uiScalarUtils.sdot(sx, m.kMScaleX, sy, m.kMSkewX);
            resY = uiScalarUtils.sdot(sx, m.kMSkewY, sy, m.kMScaleY);
        }

        static void RotTrans_xy(uiMatrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert((m._getType() & (TypeMask.kAffine_Mask | TypeMask.kPerspective_Mask)) == TypeMask.kAffine_Mask);

            resX = uiScalarUtils.sdot(sx, m.kMScaleX, sy, m.kMSkewX) + m.kMTransX;
            resY = uiScalarUtils.sdot(sx, m.kMSkewY, sy, m.kMScaleY) + m.kMTransY;
        }

        static void Persp_xy(uiMatrix3 m, float sx, float sy, out float resX, out float resY) {
            D.assert(m._hasPerspective());

            float x = uiScalarUtils.sdot(sx, m.kMScaleX, sy, m.kMSkewX) +
                      m.kMTransX;
            float y = uiScalarUtils.sdot(sx, m.kMSkewY, sy, m.kMScaleY) +
                      m.kMTransY;
            float z = uiScalarUtils.sdot(sx, m.kMPersp0, sy, m.kMPersp1) +
                      m.kMPersp2;
            if (z != 0) {
                z = 1 / z;
            }

            resX = x * z;
            resY = y * z;
        }
    }


    public partial struct uiMatrix3 {
        //static methods
        public static uiMatrix3 I() {
            var m = new uiMatrix3();
            m.reset();
            return m;
        }

        public static uiMatrix3 makeTrans(float dx, float dy) {
            var m = new uiMatrix3();
            m.setTranslate(dx, dy);
            return m;
        }

        public static uiMatrix3 makeScale(float sx, float sy) {
            var m = new uiMatrix3();
            m._setScale(sx, sy);
            return m;
        }

        public static uiMatrix3 makeRotate(float radians) {
            var m = new uiMatrix3();
            m.setRotate(radians);
            return m;
        }

        public static uiMatrix3 makeRotate(float radians, float px, float py) {
            var m = new uiMatrix3();
            m.setRotate(radians, px, py);
            return m;
        }

        public static uiMatrix3 makeTrans(uiOffset offset) {
            var m = new uiMatrix3();
            m.setTranslate(offset.dx, offset.dy);
            return m;
        }

        public static uiMatrix3 makeSkew(float dx, float dy) {
            var m = new uiMatrix3();
            m.setSkew(dx, dy);
            return m;
        }

        public static uiMatrix3 concat(uiMatrix3 a, uiMatrix3 b) {
            uiMatrix3 result = I();
            result._setConcat(a, b);
            return result;
        }

        public void setRotate(float radians, float px, float py) {
            float sinV, cosV;
            sinV = uiScalarUtils.ScalarSinCos(radians, out cosV);
            this.setSinCos(sinV, cosV, px, py);
        }

        public void setRotate(float radians) {
            float sinV, cosV;
            sinV = uiScalarUtils.ScalarSinCos(radians, out cosV);
            this.setSinCos(sinV, cosV);
        }

        public void setSkew(float kx, float ky, float px, float py) {
            this.kMScaleX = 1;
            this.kMSkewX = kx;
            this.kMTransX = -kx * py;

            this.kMSkewY = ky;
            this.kMScaleY = 1;
            this.kMTransY = -ky * px;

            this.kMPersp0 = this.kMPersp1 = 0;
            this.kMPersp2 = 1;

            this._setTypeMask(kUnknown_Mask | kOnlyPerspectiveValid_Mask);
        }

        public void setSkew(float kx, float ky) {
            this.kMScaleX = 1;
            this.kMSkewX = kx;
            this.kMTransX = 0;

            this.kMSkewY = ky;
            this.kMScaleY = 1;
            this.kMTransY = 0;

            this.kMPersp0 = this.kMPersp1 = 0;
            this.kMPersp2 = 1;

            this._setTypeMask(kUnknown_Mask | kOnlyPerspectiveValid_Mask);
        }

        public static bool equals(uiMatrix3? a, uiMatrix3? b) {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) {
                return true;
            }

            if (ReferenceEquals(a, b)) {
                return true;
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                return false;
            }

            var ma = a.Value;
            var mb = b.Value;

            return ma.kMScaleX == mb.kMScaleX && ma.kMSkewX == mb.kMSkewX && ma.kMTransX == mb.kMTransX &&
                   ma.kMSkewY == mb.kMSkewY && ma.kMScaleY == mb.kMScaleY && ma.kMTransY == mb.kMTransY &&
                   ma.kMPersp0 == mb.kMPersp0 && ma.kMPersp1 == mb.kMPersp1 && ma.kMPersp2 == mb.kMPersp2;
        }
    }


    public partial struct uiMatrix3 {
        public static uiMatrix3 fromMatrix3(Matrix3 mat3) {
            var uiMat3 = I();


            uiMat3.kMScaleX = mat3[0];
            uiMat3.kMSkewX = mat3[1];
            uiMat3.kMTransX = mat3[2];
            uiMat3.kMSkewY = mat3[3];
            uiMat3.kMScaleY = mat3[4];
            uiMat3.kMTransY = mat3[5];
            uiMat3.kMPersp0 = mat3[6];
            uiMat3.kMPersp1 = mat3[7];
            uiMat3.kMPersp2 = mat3[8];

            uiMat3._setTypeMask(kUnknown_Mask);
            return uiMat3;
        }

        static bool _only_scale_and_translate(int mask) {
            return 0 == (mask & (int) (TypeMask.kAffine_Mask | TypeMask.kPerspective_Mask));
        }

        static float _getitem(uiMatrix3 mat, int i) {
            switch (i) {
                case 0:
                    return mat.kMScaleX;
                case 1:
                    return mat.kMSkewX;
                case 2:
                    return mat.kMTransX;
                case 3:
                    return mat.kMSkewY;
                case 4:
                    return mat.kMScaleY;
                case 5:
                    return mat.kMTransY;
                case 6:
                    return mat.kMPersp0;
                case 7:
                    return mat.kMPersp1;
                case 8:
                    return mat.kMPersp2;
                default: {
                    return -1;
                }
            }
        }

        static float _rowcol3(uiMatrix3 row, int i, uiMatrix3 col, int j) {
            return _getitem(row, i) * _getitem(col, j) + _getitem(row, i + 1) * _getitem(col, j + 3) +
                   _getitem(row, i + 2) * _getitem(col, j + 6);
        }

        static float _muladdmul(float a, float b, float c, float d) {
            return (float) ((double) a * b + (double) c * d);
        }

        static uiMatrix3 ComputeInv(uiMatrix3 src, float invDet, bool isPersp) {
            uiMatrix3 dst = I();
            if (isPersp) {
                dst.kMScaleX =
                    uiScalarUtils.scross_dscale(src.kMScaleY, src.kMPersp2, src.kMTransY,
                        src.kMPersp1, invDet);
                dst.kMSkewX =
                    uiScalarUtils.scross_dscale(src.kMTransX, src.kMPersp1, src.kMSkewX,
                        src.kMPersp2, invDet);
                dst.kMTransX =
                    uiScalarUtils.scross_dscale(src.kMSkewX, src.kMTransY, src.kMTransX,
                        src.kMScaleY, invDet);

                dst.kMSkewY =
                    uiScalarUtils.scross_dscale(src.kMTransY, src.kMPersp0, src.kMSkewY,
                        src.kMPersp2, invDet);
                dst.kMScaleY =
                    uiScalarUtils.scross_dscale(src.kMScaleX, src.kMPersp2, src.kMTransX,
                        src.kMPersp0, invDet);
                dst.kMTransY =
                    uiScalarUtils.scross_dscale(src.kMTransX, src.kMSkewY, src.kMScaleX,
                        src.kMTransY, invDet);

                dst.kMPersp0 =
                    uiScalarUtils.scross_dscale(src.kMSkewY, src.kMPersp1, src.kMScaleY,
                        src.kMPersp0, invDet);
                dst.kMPersp1 =
                    uiScalarUtils.scross_dscale(src.kMSkewX, src.kMPersp0, src.kMScaleX,
                        src.kMPersp1, invDet);
                dst.kMPersp2 =
                    uiScalarUtils.scross_dscale(src.kMScaleX, src.kMScaleY, src.kMSkewX,
                        src.kMSkewY, invDet);
            }
            else {
                // not perspective
                dst.kMScaleX = src.kMScaleY * invDet;
                dst.kMSkewX = -src.kMSkewX * invDet;
                dst.kMTransX =
                    uiScalarUtils.dcross_dscale(src.kMSkewX, src.kMTransY, src.kMScaleY,
                        src.kMTransX, invDet);

                dst.kMSkewY = -src.kMSkewY * invDet;
                dst.kMScaleY = src.kMScaleX * invDet;
                dst.kMTransY =
                    uiScalarUtils.dcross_dscale(src.kMSkewY, src.kMTransX, src.kMScaleX,
                        src.kMTransY, invDet);

                dst.kMPersp0 = 0;
                dst.kMPersp1 = 0;
                dst.kMPersp2 = 1;
            }

            return dst;
        }

        //utils
        class uiScalarUtils {
            public const float kScalarNearlyZero = 1f / (1 << 12);

            public static bool ScalarNearlyZero(float x, float tolerance = kScalarNearlyZero) {
                D.assert(tolerance >= 0);
                return Mathf.Abs(x) <= tolerance;
            }

            public static bool ScalarNearlyEqual(float x, float y, float tolerance = kScalarNearlyZero) {
                D.assert(tolerance >= 0);
                return Mathf.Abs(x - y) <= tolerance;
            }

            public static bool ScalarIsInteger(float scalar) {
                return scalar == Mathf.FloorToInt(scalar);
            }

            public static float DegreesToRadians(float degrees) {
                return degrees * (Mathf.PI / 180);
            }

            public static float RadiansToDegrees(float radians) {
                return radians * (180 / Mathf.PI);
            }

            public static float ScalarSinCos(float radians, out float cosValue) {
                float sinValue = Mathf.Sin(radians);

                cosValue = Mathf.Cos(radians);
                if (ScalarNearlyZero(cosValue)) {
                    cosValue = 0;
                }

                if (ScalarNearlyZero(sinValue)) {
                    sinValue = 0;
                }

                return sinValue;
            }

            public static bool ScalarsAreFinite(uiMatrix3 m) {
                float prod = m.kMScaleX * m.kMSkewX * m.kMTransX * m.kMSkewY * m.kMScaleY * m.kMTransY * m.kMPersp0 *
                             m.kMPersp1 * m.kMPersp2;
                // At this point, prod will either be NaN or 0
                return prod == 0; // if prod is NaN, this check will return false
            }

            static byte[] _scalar_as_2s_compliment_vars = new byte[4];


            static unsafe int GetBytesToInt32(float value) {
                var intVal = *(int*) &value;
                fixed (byte* b = _scalar_as_2s_compliment_vars) {
                    *((int*) b) = intVal;
                }

                fixed (byte* pbyte = &_scalar_as_2s_compliment_vars[0]) {
                    return *((int*) pbyte);
                }
            }

            public static int ScalarAs2sCompliment(float x) {
                var result = GetBytesToInt32(x);
                if (result < 0) {
                    result &= 0x7FFFFFFF;
                    result = -result;
                }

                return result;
            }

            public static float sdot(float a, float b, float c, float d) {
                return a * b + c * d;
            }

            public static float sdot(float a, float b, float c, float d, float e, float f) {
                return a * b + c * d + e * f;
            }

            public static float scross(float a, float b, float c, float d) {
                return a * b - c * d;
            }

            public static double dcross(double a, double b, double c, double d) {
                return a * b - c * d;
            }

            public static float scross_dscale(float a, float b,
                float c, float d, double scale) {
                return (float) (scross(a, b, c, d) * scale);
            }

            public static float dcross_dscale(double a, double b,
                double c, double d, double scale) {
                return (float) (dcross(a, b, c, d) * scale);
            }

            public static bool is_degenerate_2x2(
                float scaleX, float skewX,
                float skewY, float scaleY) {
                float perp_dot = scaleX * scaleY - skewX * skewY;
                return ScalarNearlyZero(perp_dot,
                    kScalarNearlyZero * kScalarNearlyZero);
            }

            public static float inv_determinant(uiMatrix3 mat, int isPerspective) {
                double det;

                if (isPerspective != 0) {
                    det = mat.kMScaleX *
                          dcross(mat.kMScaleY, mat.kMPersp2,
                              mat.kMTransY, mat.kMPersp1)
                          +
                          mat.kMSkewX *
                          dcross(mat.kMTransY, mat.kMPersp0,
                              mat.kMSkewY, mat.kMPersp2)
                          +
                          mat.kMTransX *
                          dcross(mat.kMSkewY, mat.kMPersp1,
                              mat.kMScaleY, mat.kMPersp0);
                }
                else {
                    det = dcross(mat.kMScaleX, mat.kMScaleY,
                        mat.kMSkewX, mat.kMSkewY);
                }

                // Since the determinant is on the order of the cube of the matrix members,
                // compare to the cube of the default nearly-zero constant (although an
                // estimate of the condition number would be better if it wasn't so expensive).
                if (ScalarNearlyZero((float) det,
                    kScalarNearlyZero * kScalarNearlyZero * kScalarNearlyZero)) {
                    return 0;
                }

                return 1.0f / (float) det;
            }
        }
    }
}