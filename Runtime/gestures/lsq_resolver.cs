using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.gestures {
    class _Vector {
        internal _Vector(int size) {
            this._offset = 0;
            this._length = size;
            this._elements = Enumerable.Repeat(0.0, size).ToList();
        }

        _Vector(List<double> values, int offset, int length) {
            this._offset = offset;
            this._length = length;
            this._elements = values;
        }

        internal static _Vector fromVOL(List<double> values, int offset, int length) {
            return new _Vector(values, offset, length);
        }

        readonly int _offset;

        readonly int _length;

        readonly List<double> _elements;

        public double this[int i] {
            get { return this._elements[i + this._offset]; }
            set { this._elements[i + this._offset] = value; }
        }

        public static double operator *(_Vector a, _Vector b) {
            double result = 0.0;
            for (int i = 0; i < a._length; i += 1) {
                result += a[i] * b[i];
            }

            return result;
        }

        public double norm() {
            return Math.Sqrt(this * this);
        }
    }

    class _Matrix {
        internal _Matrix(int rows, int cols) {
            this._columns = cols;
            this._elements = Enumerable.Repeat(0.0, rows * cols).ToList();
        }

        readonly int _columns;
        readonly List<double> _elements;

        public double this[int row, int col] {
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
            this.coefficients = Enumerable.Repeat(0.0, degree + 1).ToList();
        }

        public readonly List<double> coefficients;

        public double confidence;
    }

    public class LeastSquaresSolver {
        public LeastSquaresSolver(List<double> x, List<double> y, List<double> w) {
            D.assert(x != null && y != null && w != null);
            D.assert(x.Count == y.Count);
            D.assert(y.Count == w.Count);
            this.x = x;
            this.y = y;
            this.w = w;
        }

        public readonly List<double> x;

        public readonly List<double> y;

        public readonly List<double> w;

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
                    double dot = q.getRow(j) * q.getRow(i);
                    for (int h = 0; h < m; h += 1) {
                        q[j, h] = q[j, h] - dot * q[i, h];
                    }
                }

                double norm = q.getRow(j).norm();
                if (norm < 0.000001) {
                    // Vectors are linearly dependent or zero so no solution.
                    return null;
                }

                double inverseNorm = 1.0 / norm;
                for (int h = 0; h < m; h += 1) {
                    q[j, h] = q[j, h] * inverseNorm;
                }

                for (int i = 0; i < n; i += 1) {
                    r[j, i] = i < j ? 0.0 : q.getRow(j) * a.getRow(i);
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
            double yMean = 0.0;
            for (int h = 0; h < m; h += 1) {
                yMean += this.y[h];
            }

            yMean /= m;

            double sumSquaredError = 0.0;
            double sumSquaredTotal = 0.0;
            for (int h = 0; h < m; h += 1) {
                double term = 1.0;
                double err = this.y[h] - result.coefficients[0];
                for (int i = 1; i < n; i += 1) {
                    term *= this.x[h];
                    err -= term * result.coefficients[i];
                }

                sumSquaredError += this.w[h] * this.w[h] * err * err;
                double v = this.y[h] - yMean;
                sumSquaredTotal += this.w[h] * this.w[h] * v * v;
            }

            result.confidence = sumSquaredTotal <= 0.000001 ? 1.0 : 1.0 - (sumSquaredError / sumSquaredTotal);
            return result;
        }
    }
}