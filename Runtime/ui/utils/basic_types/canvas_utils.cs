using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    static class uiXformUtils {
        public static float getScaleX(uiMatrix3 matrix) {
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

        public static float getScaleY(uiMatrix3 matrix) {
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

        public static float getScale(uiMatrix3 matrix) {
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
    }

    static class BlurUtils {
        static readonly Dictionary<_GaussianKernelKey, float[]> _gaussianKernels
            = new Dictionary<_GaussianKernelKey, float[]>();

        public static float[] get1DGaussianKernel(float gaussianSigma, int radius) {
            var width = 2 * radius + 1;
            D.assert(width <= 25);

            var key = new _GaussianKernelKey(gaussianSigma, radius);
            return _gaussianKernels.putIfAbsent(key, () => {
                var kernel = new float[25];
                float twoSigmaSqrd = 2.0f * gaussianSigma * gaussianSigma;

                if (ScalarUtils.ScalarNearlyZero(twoSigmaSqrd)) {
                    for (int i = 0; i < width; ++i) {
                        kernel[i] = 0.0f;
                    }

                    return kernel;
                }

                float denom = 1.0f / twoSigmaSqrd;

                float sum = 0.0f;
                for (int i = 0; i < width; ++i) {
                    float x = i - radius;
                    // Note that the constant term (1/(sqrt(2*pi*sigma^2)) of the Gaussian
                    // is dropped here, since we renormalize the kernel below.
                    kernel[i] = Mathf.Exp(-x * x * denom);
                    sum += kernel[i];
                }

                // Normalize the kernel
                float scale = 1.0f / sum;
                for (int i = 0; i < width; ++i) {
                    kernel[i] *= scale;
                }

                return kernel;
            });
        }

        public static float adjustSigma(float sigma, out int scaleFactor, out int radius) {
            scaleFactor = 1;

            const int maxTextureSize = 16384;
            const float MAX_BLUR_SIGMA = 4.0f;

            while (sigma > MAX_BLUR_SIGMA) {
                scaleFactor *= 2;
                sigma *= 0.5f;

                if (scaleFactor > maxTextureSize) {
                    scaleFactor = maxTextureSize;
                    sigma = MAX_BLUR_SIGMA;
                }
            }

            radius = Mathf.CeilToInt(sigma * 3.0f);
            D.assert(radius <= 3 * MAX_BLUR_SIGMA);
            return sigma;
        }

        class _GaussianKernelKey : IEquatable<_GaussianKernelKey> {
            public readonly float gaussianSigma;
            public readonly int radius;

            public _GaussianKernelKey(float gaussianSigma, int radius) {
                this.gaussianSigma = gaussianSigma;
                this.radius = radius;
            }

            public bool Equals(_GaussianKernelKey other) {
                if (ReferenceEquals(null, other)) {
                    return false;
                }

                if (ReferenceEquals(this, other)) {
                    return true;
                }

                return this.gaussianSigma.Equals(other.gaussianSigma) &&
                       this.radius == other.radius;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }

                if (ReferenceEquals(this, obj)) {
                    return true;
                }

                if (obj.GetType() != this.GetType()) {
                    return false;
                }

                return this.Equals((_GaussianKernelKey) obj);
            }

            public override int GetHashCode() {
                unchecked {
                    var hashCode = this.gaussianSigma.GetHashCode();
                    hashCode = (hashCode * 397) ^ this.radius;
                    return hashCode;
                }
            }

            public static bool operator ==(_GaussianKernelKey left, _GaussianKernelKey right) {
                return Equals(left, right);
            }

            public static bool operator !=(_GaussianKernelKey left, _GaussianKernelKey right) {
                return !Equals(left, right);
            }

            public override string ToString() {
                return $"_GaussianKernelKey(gaussianSigma: {this.gaussianSigma:F1}, radius: {this.radius})";
            }
        }
    }

    static class ImageMeshGenerator {
        static readonly List<int> _imageTriangles = new List<int>(12) {
            0, 1, 2, 0, 2, 1,
            0, 2, 3, 0, 3, 2,
        };

        static readonly List<int> _imageNineTriangles = new List<int> {
            0, 4, 1, 1, 4, 5,
            0, 1, 4, 1, 5, 4,
            1, 5, 2, 2, 5, 6,
            1, 2, 5, 2, 6, 5,
            2, 6, 3, 3, 6, 7,
            2, 3, 6, 3, 7, 6,
            4, 8, 5, 5, 8, 9,
            4, 5, 8, 5, 9, 8,
            5, 9, 6, 6, 9, 10,
            5, 6, 9, 6, 10, 9,
            6, 10, 7, 7, 10, 11,
            6, 7, 10, 7, 11, 10,
            8, 12, 9, 9, 12, 13,
            8, 9, 12, 9, 13, 12,
            9, 13, 10, 10, 13, 14,
            9, 10, 13, 10, 14, 13,
            10, 14, 11, 11, 14, 15,
            10, 11, 14, 11, 15, 14,
        };

        public static uiMeshMesh imageMesh(uiMatrix3? matrix,
            Offset srcTL, Offset srcBL, Offset srcBR, Offset srcTR,
            uiRect dst) {
            var vertices = ItemPoolManager.alloc<uiList<Vector3>>();
            vertices.SetCapacity(4);

            var uv = ItemPoolManager.alloc<uiList<Vector2>>();
            uv.SetCapacity(4);

            vertices.Add(new Vector2(dst.left, dst.top));
            uv.Add(new Vector2(srcTL.dx, 1.0f - srcTL.dy));
            vertices.Add(new Vector2(dst.left, dst.bottom));
            uv.Add(new Vector2(srcBL.dx, 1.0f - srcBL.dy));
            vertices.Add(new Vector2(dst.right, dst.bottom));
            uv.Add(new Vector2(srcBR.dx, 1.0f - srcBR.dy));
            vertices.Add(new Vector2(dst.right, dst.top));
            uv.Add(new Vector2(srcTR.dx, 1.0f - srcTR.dy));

            var _triangles = ItemPoolManager.alloc<uiList<int>>();
            _triangles.AddRange(_imageTriangles);

            return uiMeshMesh.create(matrix, vertices, _triangles, uv);
        }
        
        public static uiMeshMesh imageMesh(uiMatrix3? matrix, uiRect src, uiRect dst) {
            var vertices = ItemPoolManager.alloc<uiList<Vector3>>();
            vertices.SetCapacity(4);

            var uv = ItemPoolManager.alloc<uiList<Vector2>>();
            uv.SetCapacity(4);

            float uvx0 = src.left;
            float uvx1 = src.right;
            float uvy0 = 1.0f - src.top;
            float uvy1 = 1.0f - src.bottom;

            vertices.Add(new Vector2(dst.left, dst.top));
            uv.Add(new Vector2(uvx0, uvy0));
            vertices.Add(new Vector2(dst.left, dst.bottom));
            uv.Add(new Vector2(uvx0, uvy1));
            vertices.Add(new Vector2(dst.right, dst.bottom));
            uv.Add(new Vector2(uvx1, uvy1));
            vertices.Add(new Vector2(dst.right, dst.top));
            uv.Add(new Vector2(uvx1, uvy0));
            
            var _triangles = ItemPoolManager.alloc<uiList<int>>();
            _triangles.AddRange(_imageTriangles);

            return uiMeshMesh.create(matrix, vertices, _triangles, uv);
        }

        public static uiMeshMesh imageNineMesh(uiMatrix3? matrix, uiRect src, uiRect center, int srcWidth, int srcHeight, uiRect dst) {
            float x0 = dst.left;
            float x3 = dst.right;
            float x1 = x0 + ((center.left - src.left) * srcWidth);
            float x2 = x3 - ((src.right - center.right) * srcWidth);

            float y0 = dst.top;
            float y3 = dst.bottom;
            float y1 = y0 + ((center.top - src.top) * srcHeight);
            float y2 = y3 - ((src.bottom - center.bottom) * srcHeight);

            float tx0 = src.left;
            float tx1 = center.left;
            float tx2 = center.right;
            float tx3 = src.right;
            float ty0 = 1 - src.top;
            float ty1 = 1 - center.top;
            float ty2 = 1 - center.bottom;
            float ty3 = 1 - src.bottom;

            var vertices = ItemPoolManager.alloc<uiList<Vector3>>();
            vertices.SetCapacity(16);

            var uv = ItemPoolManager.alloc<uiList<Vector2>>();
            uv.SetCapacity(16);

            vertices.Add(new Vector2(x0, y0));
            uv.Add(new Vector2(tx0, ty0));
            vertices.Add(new Vector2(x1, y0));
            uv.Add(new Vector2(tx1, ty0));
            vertices.Add(new Vector2(x2, y0));
            uv.Add(new Vector2(tx2, ty0));
            vertices.Add(new Vector2(x3, y0));
            uv.Add(new Vector2(tx3, ty0));
            vertices.Add(new Vector2(x0, y1));
            uv.Add(new Vector2(tx0, ty1));
            vertices.Add(new Vector2(x1, y1));
            uv.Add(new Vector2(tx1, ty1));
            vertices.Add(new Vector2(x2, y1));
            uv.Add(new Vector2(tx2, ty1));
            vertices.Add(new Vector2(x3, y1));
            uv.Add(new Vector2(tx3, ty1));
            vertices.Add(new Vector2(x0, y2));
            uv.Add(new Vector2(tx0, ty2));
            vertices.Add(new Vector2(x1, y2));
            uv.Add(new Vector2(tx1, ty2));
            vertices.Add(new Vector2(x2, y2));
            uv.Add(new Vector2(tx2, ty2));
            vertices.Add(new Vector2(x3, y2));
            uv.Add(new Vector2(tx3, ty2));
            vertices.Add(new Vector2(x0, y3));
            uv.Add(new Vector2(tx0, ty3));
            vertices.Add(new Vector2(x1, y3));
            uv.Add(new Vector2(tx1, ty3));
            vertices.Add(new Vector2(x2, y3));
            uv.Add(new Vector2(tx2, ty3));
            vertices.Add(new Vector2(x3, y3));
            uv.Add(new Vector2(tx3, ty3));
            
            var _triangles = ItemPoolManager.alloc<uiList<int>>();
            _triangles.AddRange(_imageNineTriangles);
            
            return uiMeshMesh.create(matrix, vertices, _triangles, uv);
        }        
    }
}