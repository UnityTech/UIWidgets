using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.animation {
    public abstract class Animatable<T> {
        public abstract T evaluate(Animation<double> animation);

        public Animation<T> animate(Animation<double> parent) {
            return new _AnimatedEvaluation<T>(parent, this);
        }

        public Animatable<T> chain(Animatable<double> parent) {
            return new _ChainedEvaluation<T>(parent, this);
        }
    }

    class _AnimatedEvaluation<T> : AnimationWithParentMixin<double, T> {
        internal _AnimatedEvaluation(Animation<double> parent, Animatable<T> evaluatable) {
            this._parent = parent;
            this._evaluatable = evaluatable;
        }

        public override Animation<double> parent {
            get { return this._parent; }
        }

        readonly Animation<double> _parent;

        readonly Animatable<T> _evaluatable;

        public override T value {
            get { return this._evaluatable.evaluate(this.parent); }
        }

        public override string ToString() {
            return $"{this.parent}\u27A9{this._evaluatable}\u27A9{this.value}";
        }

        public override string toStringDetails() {
            return base.toStringDetails() + " " + this._evaluatable;
        }
    }


    class _ChainedEvaluation<T> : Animatable<T> {
        internal _ChainedEvaluation(Animatable<double> parent, Animatable<T> evaluatable) {
            this._parent = parent;
            this._evaluatable = evaluatable;
        }

        readonly Animatable<double> _parent;

        readonly Animatable<T> _evaluatable;

        public override T evaluate(Animation<double> animation) {
            double value = this._parent.evaluate(animation);
            return this._evaluatable.evaluate(new AlwaysStoppedAnimation<double>(value));
        }

        public override string ToString() {
            return $"{this._parent}\u27A9{this._evaluatable}";
        }
    }

    public abstract class Tween<T> : Animatable<T>, IEquatable<Tween<T>> {
        protected Tween(T begin, T end) {
            this.begin = begin;
            this.end = end;
        }

        public T begin;

        public T end;

        public abstract T lerp(double t);

        public override T evaluate(Animation<double> animation) {
            double t = animation.value;
            if (t == 0.0) {
                return this.begin;
            }

            if (t == 1.0) {
                return this.end;
            }

            return this.lerp(t);
        }

        public override string ToString() {
            return $"{this.GetType()}({this.begin} \u2192 {this.end})";
        }

        public bool Equals(Tween<T> other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return EqualityComparer<T>.Default.Equals(this.begin, other.begin) &&
                   EqualityComparer<T>.Default.Equals(this.end, other.end);
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

            return this.Equals((Tween<T>) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (EqualityComparer<T>.Default.GetHashCode(this.begin) * 397) ^
                       EqualityComparer<T>.Default.GetHashCode(this.end);
            }
        }

        public static bool operator ==(Tween<T> left, Tween<T> right) {
            return Equals(left, right);
        }

        public static bool operator !=(Tween<T> left, Tween<T> right) {
            return !Equals(left, right);
        }
    }

    public class ReverseTween<T> : Tween<T> {
        public ReverseTween(Tween<T> parent) : base(begin: parent.end, end: parent.begin) {
            this.parent = parent;
        }

        public readonly Tween<T> parent;

        public override T lerp(double t) {
            return this.parent.lerp(1.0 - t);
        }
    }

    public class ColorTween : Tween<Color> {
        public ColorTween(Color begin = null, Color end = null) : base(begin: begin, end: end) {
        }

        public override Color lerp(double t) {
            return Color.lerp(this.begin, this.end, t);
        }
    }

    public class SizeTween : Tween<Size> {
        public SizeTween(Size begin = null, Size end = null) : base(begin: begin, end: end) {
        }

        public override Size lerp(double t) {
            return Size.lerp(this.begin, this.end, t);
        }
    }

    public class RectTween : Tween<Rect> {
        public RectTween(Rect begin = null, Rect end = null) : base(begin: begin, end: end) {
        }

        public override Rect lerp(double t) {
            return Rect.lerp(this.begin, this.end, t);
        }
    }

    public class IntTween : Tween<int> {
        public IntTween(int begin, int end) : base(begin: begin, end: end) {
        }

        public override int lerp(double t) {
            return (this.begin + (this.end - this.begin) * t).round();
        }
    }

    public class DoubleTween : Tween<double> {
        public DoubleTween(double begin, double end) : base(begin: begin, end: end) {
        }

        public override double lerp(double t) {
            return this.begin + (this.end - this.begin) * t;
        }
    }

    public class StepTween : Tween<int> {
        public StepTween(int begin, int end) : base(begin: begin, end: end) {
        }

        public override int lerp(double t) {
            return (this.begin + (this.end - this.begin) * t).floor();
        }
    }

    public class OffsetTween : Tween<Offset> {
        public OffsetTween(Offset begin, Offset end) : base(begin: begin, end: end) {
        }

        public override Offset lerp(double t) {
            return (this.begin + (this.end - this.begin) * t);
        }
    }

    public class CurveTween : Animatable<double> {
        public CurveTween(Curve curve = null) {
            D.assert(curve != null);
            this.curve = curve;
        }

        public readonly Curve curve;

        public override double evaluate(Animation<double> animation) {
            double t = animation.value;
            if (t == 0.0 || t == 1.0) {
                D.assert(this.curve.transform(t).round() == t);
                return t;
            }

            return this.curve.transform(t);
        }

        public override string ToString() {
            return $"{this.GetType()}(curve: {this.curve})";
        }
    }
}