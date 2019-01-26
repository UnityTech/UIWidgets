using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.animation {
    public abstract class Curve {
        public abstract double transform(double t);

        public Curve flipped {
            get { return new FlippedCurve(this); }
        }

        public override string ToString() {
            return this.GetType().ToString();
        }
    }

    class _Linear : Curve {
        public override double transform(double t) {
            return t;
        }
    }

    public class SawTooth : Curve {
        public SawTooth(int count) {
            this.count = count;
        }

        public readonly int count;

        public override double transform(double t) {
            D.assert(t >= 0.0 && t <= 1.0);
            if (t == 1.0) {
                return 1.0;
            }

            t *= this.count;
            return t - (int) t;
        }

        public override string ToString() {
            return $"{this.GetType()}({this.count})";
        }
    }

    public class Interval : Curve {
        public Interval(double begin, double end, Curve curve = null) {
            this.begin = begin;
            this.end = end;
            this.curve = curve ?? Curves.linear;
        }

        public readonly double begin;

        public readonly double end;

        public readonly Curve curve;

        public override double transform(double t) {
            D.assert(t >= 0.0 && t <= 1.0);
            D.assert(this.begin >= 0.0);
            D.assert(this.begin <= 1.0);
            D.assert(this.end >= 0.0);
            D.assert(this.end <= 1.0);
            D.assert(this.end >= this.begin);
            if (t == 0.0 || t == 1.0) {
                return t;
            }

            t = ((t - this.begin) / (this.end - this.begin)).clamp(0.0, 1.0);
            if (t == 0.0 || t == 1.0) {
                return t;
            }

            return this.curve.transform(t);
        }

        public override string ToString() {
            if (!(this.curve is _Linear)) {
                return $"{this.GetType()}({this.begin}\u22EF{this.end}\u27A9{this.curve}";
            }

            return $"{this.GetType()}({this.begin}\u22EF{this.end})";
        }
    }

    public class Threshold : Curve {
        public Threshold(double threshold) {
            this.threshold = threshold;
        }

        public readonly double threshold;

        public override double transform(double t) {
            D.assert(t >= 0.0 && t <= 1.0);
            D.assert(this.threshold >= 0.0);
            D.assert(this.threshold <= 1.0);
            if (t == 0.0 || t == 1.0) {
                return t;
            }

            return t < this.threshold ? 0.0 : 1.0;
        }
    }

    public class Cubic : Curve {
        public Cubic(double a, double b, double c, double d) {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        public readonly double a;

        public readonly double b;

        public readonly double c;

        public readonly double d;

        const double _cubicErrorBound = 0.001;

        double _evaluateCubic(double a, double b, double m) {
            return 3 * a * (1 - m) * (1 - m) * m +
                   3 * b * (1 - m) * m * m +
                   m * m * m;
        }

        public override double transform(double t) {
            D.assert(t >= 0.0 && t <= 1.0);
            double start = 0.0;
            double end = 1.0;
            while (true) {
                double midpoint = (start + end) / 2;
                double estimate = this._evaluateCubic(this.a, this.c, midpoint);
                if ((t - estimate).abs() < _cubicErrorBound) {
                    return this._evaluateCubic(this.b, this.d, midpoint);
                }

                if (estimate < t) {
                    start = midpoint;
                }
                else {
                    end = midpoint;
                }
            }
        }

        public override string ToString() {
            return $"{this.GetType()}({this.a:F2}, {this.b:F2}, {this.c:F2}, {this.d:F2})";
        }
    }

    public class FlippedCurve : Curve {
        public FlippedCurve(Curve curve) {
            D.assert(curve != null);
            this.curve = curve;
        }

        public readonly Curve curve;

        public override double transform(double t) {
            return 1.0 - this.curve.transform(1.0 - t);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.curve})";
        }
    }

    class _DecelerateCurve : Curve {
        internal _DecelerateCurve() {
        }

        public override double transform(double t) {
            D.assert(t >= 0.0 && t <= 1.0);
            t = 1.0 - t;
            return 1.0 - t * t;
        }
    }

    class _BounceInCurve : Curve {
        internal _BounceInCurve() {
        }

        public override double transform(double t) {
            D.assert(t >= 0.0 && t <= 1.0);
            return 1.0 - Curves._bounce(1.0 - t);
        }
    }

    class _BounceOutCurve : Curve {
        internal _BounceOutCurve() {
        }

        public override double transform(double t) {
            D.assert(t >= 0.0 && t <= 1.0);
            return Curves._bounce(t);
        }
    }

    class _BounceInOutCurve : Curve {
        internal _BounceInOutCurve() {
        }

        public override double transform(double t) {
            D.assert(t >= 0.0 && t <= 1.0);
            if (t < 0.5) {
                return (1.0 - Curves._bounce(1.0 - t)) * 0.5;
            }
            else {
                return Curves._bounce(t * 2.0 - 1.0) * 0.5 + 0.5;
            }
        }
    }

    public class ElasticInCurve : Curve {
        public ElasticInCurve(double period = 0.4) {
            this.period = period;
        }

        public readonly double period;

        public override double transform(double t) {
            D.assert(t >= 0.0 && t <= 1.0);
            double s = this.period / 4.0;
            t = t - 1.0;
            return -Math.Pow(2.0, 10.0 * t) * Math.Sin((t - s) * (Math.PI * 2.0) / this.period);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.period})";
        }
    }

    public class ElasticOutCurve : Curve {
        public ElasticOutCurve(double period = 0.4) {
            this.period = period;
        }

        public readonly double period;

        public override double transform(double t) {
            D.assert(t >= 0.0 && t <= 1.0);
            double s = this.period / 4.0;
            return Math.Pow(2.0, -10.0 * t) * Math.Sin((t - s) * (Math.PI * 2.0) / this.period) + 1.0;
        }

        public override string ToString() {
            return $"{this.GetType()}({this.period})";
        }
    }

    public class ElasticInOutCurve : Curve {
        public ElasticInOutCurve(double period = 0.4) {
            this.period = period;
        }

        public readonly double period;

        public override double transform(double t) {
            D.assert(t >= 0.0 && t <= 1.0);
            double s = this.period / 4.0;
            t = 2.0 * t - 1.0;
            if (t < 0.0) {
                return -0.5 * Math.Pow(2.0, 10.0 * t) * Math.Sin((t - s) * (Math.PI * 2.0) / this.period);
            }
            else {
                return Math.Pow(2.0, -10.0 * t) * Math.Sin((t - s) * (Math.PI * 2.0) / this.period) * 0.5 + 1.0;
            }
        }

        public override string ToString() {
            return $"{this.GetType()}({this.period})";
        }
    }

    public static class Curves {
        public static readonly Curve linear = new _Linear();

        public static readonly Curve decelerate = new _DecelerateCurve();

        public static readonly Curve ease = new Cubic(0.25, 0.1, 0.25, 1.0);

        public static readonly Curve easeIn = new Cubic(0.42, 0.0, 1.0, 1.0);

        public static readonly Curve easeOut = new Cubic(0.0, 0.0, 0.58, 1.0);

        public static readonly Curve easeInOut = new Cubic(0.42, 0.0, 0.58, 1.0);

        public static readonly Curve fastOutSlowIn = new Cubic(0.4, 0.0, 0.2, 1.0);

        public static readonly Curve bounceIn = new _BounceInCurve();

        public static readonly Curve bounceOut = new _BounceOutCurve();

        public static readonly Curve bounceInOut = new _BounceInOutCurve();

        public static readonly Curve elasticIn = new ElasticInCurve();

        public static readonly Curve elasticOut = new ElasticOutCurve();

        public static readonly Curve elasticInOut = new ElasticInOutCurve();

        internal static double _bounce(double t) {
            if (t < 1.0 / 2.75) {
                return 7.5625 * t * t;
            }
            else if (t < 2 / 2.75) {
                t -= 1.5 / 2.75;
                return 7.5625 * t * t + 0.75;
            }
            else if (t < 2.5 / 2.75) {
                t -= 2.25 / 2.75;
                return 7.5625 * t * t + 0.9375;
            }

            t -= 2.625 / 2.75;
            return 7.5625 * t * t + 0.984375;
        }
    }
}