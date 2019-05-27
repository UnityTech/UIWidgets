using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.gestures {
    class _Vector {
        internal _Vector(int size) {
            this._offset = 0;
            this._length = size;
            this._elements = CollectionUtils.CreateRepeatedList(0.0f, size);
        }

        _Vector(List<float> values, int offset, int length) {
            this._offset = offset;
            this._length = length;
            this._elements = values;
        }

        internal static _Vector fromVOL(List<float> values, int offset, int length) {
            return new _Vector(values, offset, length);
        }

        readonly int _offset;

        readonly int _length;

        readonly List<float> _elements;

        public float this[int i] {
            get { return this._elements[i + this._offset]; }
            set { this._elements[i + this._offset] = value; }
        }

        public static float operator *(_Vector a, _Vector b) {
            float result = 0.0f;
            for (int i = 0; i < a._length; i += 1) {
                result += a[i] * b[i];
            }

            return result;
        }

        public float norm() {
            return Mathf.Sqrt(this * this);
        }
    }

    class _Matrix {
        internal _Matrix(int rows, int cols) {
            this._columns = cols;
            this._elements = CollectionUtils.CreateRepeatedList(0.0f, rows * cols);
        }

        readonly int _columns;
        readonly List<float> _elements;

        public float this[int row, int col] {
            get { return this._elements[row * this._columns + col]; }
            set { this._elements[row * this._columns + col] = value; }
        }

        public _Vector getRow(int row) {
            return _Vector.fromVOL(
                this._elements,
                row * this._columns,
                this._columns
            );
        }
    }

    public class PolynomialFit {
        public PolynomialFit(int degree) {
            this.coefficients = CollectionUtils.CreateRepeatedList(0.0f, degree + 1);
        }

        public readonly List<float> coefficients;

        public float confidence;
    }

    public class LeastSquaresSolver {
        public LeastSquaresSolver(List<float> x, List<float> y, List<float> w) {
            D.assert(x != null && y != null && w != null);
            D.assert(x.Count == y.Count);
            D.assert(y.Count == w.Count);
            this.x = x;
            this.y = y;
            this.w = w;
        }

        public readonly List<float> x;

        public readonly List<float> y;

        public readonly List<float> w;

        /// Fits a polynomial of the given degree to the data points.
        public PolynomialFit solve(int degree) {
            if (degree > this.x.Count) {
                // Not enough data to fit a curve.
                return null;
            }

            PolynomialFit result = new PolynomialFit(degree);

            // Shorthands for the purpose of notation equivalence to original C++ code.
            int m = this.x.Count;
            int n = degree + 1;

            // Expand the X vector to a matrix A, pre-multiplied by the weights.
            _Matrix a = new _Matrix(n, m);
            for (int h = 0; h < m; h += 1) {
                a[0, h] = this.w[h];
                for (int i = 1; i < n; i += 1) {
                    a[i, h] = a[i - 1, h] * this.x[h];
                }
            }

            // Apply the Gram-Schmidt process to A to obtain its QR decomposition.

            // Orthonormal basis, column-major ordVectorer.
            _Matrix q = new _Matrix(n, m);
            // Upper triangular matrix, row-major order.
            _Matrix r = new _Matrix(n, n);
            for (int j = 0; j < n; j += 1) {
                for (int h = 0; h < m; h += 1) {
                    q[j, h] = a[j, h];
                }

                for (int i = 0; i < j; i += 1) {
                    float dot = q.getRow(j) * q.getRow(i);
                    for (int h = 0; h < m; h += 1) {
                        q[j, h] = q[j, h] - dot * q[i, h];
                    }
                }

                float norm = q.getRow(j).norm();
                if (norm < 0.000001f) {
                    // Vectors are linearly dependent or zero so no solution.
                    return null;
                }

                float inverseNorm = 1.0f / norm;
                for (int h = 0; h < m; h += 1) {
                    q[j, h] = q[j, h] * inverseNorm;
                }

                for (int i = 0; i < n; i += 1) {
                    r[j, i] = i < j ? 0.0f : q.getRow(j) * a.getRow(i);
                }
            }

            // Solve R B = Qt W Y to find B. This is easy because R is upper triangular.
            // We just work from bottom-right to top-left calculating B's coefficients.
            _Vector wy = new _Vector(m);
            for (int h = 0; h < m; h += 1) {
                wy[h] = this.y[h] * this.w[h];
            }

            for (int i = n - 1; i >= 0; i -= 1) {
                result.coefficients[i] = q.getRow(i) * wy;
                for (int j = n - 1; j > i; j -= 1) {
                    result.coefficients[i] -= r[i, j] * result.coefficients[j];
                }

                result.coefficients[i] /= r[i, i];
            }

            // Calculate the coefficient of determination (confidence) as:
            //   1 - (sumSquaredError / sumSquaredTotal)
            // ...where sumSquaredError is the residual sum of squares (variance of the
            // error), and sumSquaredTotal is the total sum of squares (variance of the
            // data) where each has been weighted.
            float yMean = 0.0f;
            for (int h = 0; h < m; h += 1) {
                yMean += this.y[h];
            }

            yMean /= m;

            float sumSquaredError = 0.0f;
            float sumSquaredTotal = 0.0f;
            for (int h = 0; h < m; h += 1) {
                float term = 1.0f;
                float err = this.y[h] - result.coefficients[0];
                for (int i = 1; i < n; i += 1) {
                    term *= this.x[h];
                    err -= term * result.coefficients[i];
                }

                sumSquaredError += this.w[h] * this.w[h] * err * err;
                float v = this.y[h] - yMean;
                sumSquaredTotal += this.w[h] * this.w[h] * v * v;
            }

            result.confidence = sumSquaredTotal <= 0.000001f ? 1.0f : 1.0f - (sumSquaredError / sumSquaredTotal);
            return result;
        }
    }
}