using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.animation {
    public abstract class Curve {
        public float transform(float t) {
            D.assert(t >= 0.0f && t <= 1.0f);
            if (t == 0.0f || t == 1.0f) {
                return t;
            }

            return this.transformInternal(t);
        }

        protected virtual float transformInternal(float t) {
            throw new NotImplementedException();
        }

        public Curve flipped {
            get { return new FlippedCurve(this); }
        }

        public override string ToString() {
            return this.GetType().ToString();
        }
    }

    class _Linear : Curve {
        protected override float transformInternal(float t) {
            return t;
        }
    }

    public class SawTooth : Curve {
        public SawTooth(int count) {
            this.count = count;
        }

        public readonly int count;

        protected override float transformInternal(float t) {
            t *= this.count;
            return t - (int) t;
        }

        public override string ToString() {
            return $"{this.GetType()}({this.count})";
        }
    }

    public class Interval : Curve {
        public Interval(float begin, float end, Curve curve = null) {
            this.begin = begin;
            this.end = end;
            this.curve = curve ?? Curves.linear;
        }

        public readonly float begin;

        public readonly float end;

        public readonly Curve curve;

        protected override float transformInternal(float t) {
            D.assert(t >= 0.0 && t <= 1.0);
            D.assert(this.begin >= 0.0);
            D.assert(this.begin <= 1.0);
            D.assert(this.end >= 0.0);
            D.assert(this.end <= 1.0);
            D.assert(this.end >= this.begin);
            t = ((t - this.begin) / (this.end - this.begin)).clamp(0.0f, 1.0f);
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
        public Threshold(float threshold) {
            this.threshold = threshold;
        }

        public readonly float threshold;

        protected override float transformInternal(float t) {
            D.assert(this.threshold >= 0.0);
            D.assert(this.threshold <= 1.0);
            return t < this.threshold ? 0.0f : 1.0f;
        }
    }

    public class Cubic : Curve {
        public Cubic(float a, float b, float c, float d) {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        public readonly float a;

        public readonly float b;

        public readonly float c;

        public readonly float d;

        const float _cubicErrorBound = 0.001f;

        float _evaluateCubic(float a, float b, float m) {
            return 3 * a * (1 - m) * (1 - m) * m +
                   3 * b * (1 - m) * m * m +
                   m * m * m;
        }

        protected override float transformInternal(float t) {
            float start = 0.0f;
            float end = 1.0f;
            while (true) {
                float midpoint = (start + end) / 2;
                float estimate = this._evaluateCubic(this.a, this.c, midpoint);
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

        protected override float transformInternal(float t) {
            return 1.0f - this.curve.transform(1.0f - t);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.curve})";
        }
    }

    class _DecelerateCurve : Curve {
        internal _DecelerateCurve() {
        }

        protected override float transformInternal(float t) {
            t = 1.0f - t;
            return 1.0f - t * t;
        }
    }

    class _BounceInCurve : Curve {
        internal _BounceInCurve() {
        }

        protected override float transformInternal(float t) {
            return 1.0f - Curves._bounce(1.0f - t);
        }
    }

    class _BounceOutCurve : Curve {
        internal _BounceOutCurve() {
        }

        protected override float transformInternal(float t) {
            return Curves._bounce(t);
        }
    }

    class _BounceInOutCurve : Curve {
        internal _BounceInOutCurve() {
        }

        protected override float transformInternal(float t) {
            if (t < 0.5f) {
                return (1.0f - Curves._bounce(1.0f - t)) * 0.5f;
            }
            else {
                return Curves._bounce(t * 2.0f - 1.0f) * 0.5f + 0.5f;
            }
        }
    }

    public class ElasticInCurve : Curve {
        public ElasticInCurve(float period = 0.4f) {
            this.period = period;
        }

        public readonly float period;

        protected override float transformInternal(float t) {
            float s = this.period / 4.0f;
            t = t - 1.0f;
            return -Mathf.Pow(2.0f, 10.0f * t) * Mathf.Sin((t - s) * (Mathf.PI * 2.0f) / this.period);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.period})";
        }
    }

    public class ElasticOutCurve : Curve {
        public ElasticOutCurve(float period = 0.4f) {
            this.period = period;
        }

        public readonly float period;

        protected override float transformInternal(float t) {
            float s = this.period / 4.0f;
            return Mathf.Pow(2.0f, -10.0f * t) * Mathf.Sin((t - s) * (Mathf.PI * 2.0f) / this.period) + 1.0f;
        }

        public override string ToString() {
            return $"{this.GetType()}({this.period})";
        }
    }

    public class ElasticInOutCurve : Curve {
        public ElasticInOutCurve(float period = 0.4f) {
            this.period = period;
        }

        public readonly float period;

        protected override float transformInternal(float t) {
            float s = this.period / 4.0f;
            t = 2.0f * t - 1.0f;
            if (t < 0.0) {
                return -0.5f * Mathf.Pow(2.0f, 10.0f * t) * Mathf.Sin((t - s) * (Mathf.PI * 2.0f) / this.period);
            }
            else {
                return Mathf.Pow(2.0f, -10.0f * t) * Mathf.Sin((t - s) * (Mathf.PI * 2.0f) / this.period) * 0.5f +
                       1.0f;
            }
        }

        public override string ToString() {
            return $"{this.GetType()}({this.period})";
        }
    }

    public static class Curves {
        public static readonly Curve linear = new _Linear();

        public static readonly Curve decelerate = new _DecelerateCurve();

        public static readonly Cubic fastLinearToSlowEaseIn = new Cubic(0.18f, 1.0f, 0.04f, 1.0f);

        public static readonly Curve ease = new Cubic(0.25f, 0.1f, 0.25f, 1.0f);

        public static readonly Curve easeIn = new Cubic(0.42f, 0.0f, 1.0f, 1.0f);

        public static readonly Cubic easeInToLinear = new Cubic(0.67f, 0.03f, 0.65f, 0.09f);

        public static readonly Cubic easeInSine = new Cubic(0.47f, 0, 0.745f, 0.715f);

        public static readonly Cubic easeInQuad = new Cubic(0.55f, 0.085f, 0.68f, 0.53f);

        public static readonly Cubic easeInCubic = new Cubic(0.55f, 0.055f, 0.675f, 0.19f);

        public static readonly Cubic easeInQuart = new Cubic(0.895f, 0.03f, 0.685f, 0.22f);

        public static readonly Cubic easeInQuint = new Cubic(0.755f, 0.05f, 0.855f, 0.06f);

        public static readonly Cubic easeInExpo = new Cubic(0.95f, 0.05f, 0.795f, 0.035f);

        public static readonly Cubic easeInCirc = new Cubic(0.6f, 0.04f, 0.98f, 0.335f);

        public static readonly Cubic easeInBack = new Cubic(0.6f, -0.28f, 0.735f, 0.045f);

        public static readonly Curve easeOut = new Cubic(0.0f, 0.0f, 0.58f, 1.0f);

        public static readonly Cubic linearToEaseOut = new Cubic(0.35f, 0.91f, 0.33f, 0.97f);

        public static readonly Cubic easeOutSine = new Cubic(0.39f, 0.575f, 0.565f, 1.0f);

        public static readonly Cubic easeOutQuad = new Cubic(0.25f, 0.46f, 0.45f, 0.94f);

        public static readonly Cubic easeOutCubic = new Cubic(0.215f, 0.61f, 0.355f, 1.0f);

        public static readonly Cubic easeOutQuart = new Cubic(0.165f, 0.84f, 0.44f, 1.0f);

        public static readonly Cubic easeOutQuint = new Cubic(0.23f, 1.0f, 0.32f, 1.0f);

        public static readonly Cubic easeOutExpo = new Cubic(0.19f, 1.0f, 0.22f, 1.0f);

        public static readonly Cubic easeOutCirc = new Cubic(0.075f, 0.82f, 0.165f, 1.0f);

        public static readonly Cubic easeOutBack = new Cubic(0.175f, 0.885f, 0.32f, 1.275f);

        public static readonly Curve easeInOut = new Cubic(0.42f, 0.0f, 0.58f, 1.0f);

        public static readonly Cubic easeInOutSine = new Cubic(0.445f, 0.05f, 0.55f, 0.95f);

        public static readonly Cubic easeInOutQuad = new Cubic(0.455f, 0.03f, 0.515f, 0.955f);

        public static readonly Cubic easeInOutCubic = new Cubic(0.645f, 0.045f, 0.355f, 1.0f);

        public static readonly Cubic easeInOutQuart = new Cubic(0.77f, 0, 0.175f, 1.0f);

        public static readonly Cubic easeInOutQuint = new Cubic(0.86f, 0, 0.07f, 1.0f);

        public static readonly Cubic easeInOutExpo = new Cubic(1.0f, 0, 0, 1.0f);

        public static readonly Cubic easeInOutCirc = new Cubic(0.785f, 0.135f, 0.15f, 0.86f);

        public static readonly Cubic easeInOutBack = new Cubic(0.68f, -0.55f, 0.265f, 1.55f);

        public static readonly Cubic fastOutSlowIn = new Cubic(0.4f, 0.0f, 0.2f, 1.0f);
        
        public static readonly Cubic slowMiddle = new Cubic(0.15f, 0.85f, 0.85f, 0.15f);

        public static readonly Curve bounceIn = new _BounceInCurve();

        public static readonly Curve bounceOut = new _BounceOutCurve();

        public static readonly Curve bounceInOut = new _BounceInOutCurve();

        public static readonly Curve elasticIn = new ElasticInCurve();

        public static readonly Curve elasticOut = new ElasticOutCurve();

        public static readonly Curve elasticInOut = new ElasticInOutCurve();

        internal static float _bounce(float t) {
            if (t < 1.0f / 2.75f) {
                return 7.5625f * t * t;
            }
            else if (t < 2 / 2.75f) {
                t -= 1.5f / 2.75f;
                return 7.5625f * t * t + 0.75f;
            }
            else if (t < 2.5f / 2.75f) {
                t -= 2.25f / 2.75f;
                return 7.5625f * t * t + 0.9375f;
            }

            t -= 2.625f / 2.75f;
            return 7.5625f * t * t + 0.984375f;
        }
    }
}