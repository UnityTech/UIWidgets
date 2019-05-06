using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.animation {
    public abstract class Animatable<T> {
        public abstract T evaluate(Animation<float> animation);

        public Animation<T> animate(Animation<float> parent) {
            return new _AnimatedEvaluation<T>(parent, this);
        }

        public Animatable<T> chain(Animatable<float> parent) {
            return new _ChainedEvaluation<T>(parent, this);
        }
    }

    class _AnimatedEvaluation<T> : AnimationWithParentMixin<float, T> {
        internal _AnimatedEvaluation(Animation<float> parent, Animatable<T> evaluatable) {
            this._parent = parent;
            this._evaluatable = evaluatable;
        }

        public override Animation<float> parent {
            get { return this._parent; }
        }

        readonly Animation<float> _parent;

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
        internal _ChainedEvaluation(Animatable<float> parent, Animatable<T> evaluatable) {
            this._parent = parent;
            this._evaluatable = evaluatable;
        }

        readonly Animatable<float> _parent;

        readonly Animatable<T> _evaluatable;

        public override T evaluate(Animation<float> animation) {
            float value = this._parent.evaluate(animation);
            return this._evaluatable.evaluate(new AlwaysStoppedAnimation<float>(value));
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

        public virtual T begin { get; set; }

        public virtual T end { get; set; }

        public abstract T lerp(float t);

        public override T evaluate(Animation<float> animation) {
            float t = animation.value;
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

        public override T lerp(float t) {
            return this.parent.lerp(1.0f - t);
        }
    }

    public class ColorTween : Tween<Color> {
        public ColorTween(Color begin = null, Color end = null) : base(begin: begin, end: end) {
        }

        public override Color lerp(float t) {
            return Color.lerp(this.begin, this.end, t);
        }
    }

    public class SizeTween : Tween<Size> {
        public SizeTween(Size begin = null, Size end = null) : base(begin: begin, end: end) {
        }

        public override Size lerp(float t) {
            return Size.lerp(this.begin, this.end, t);
        }
    }

    public class RectTween : Tween<Rect> {
        public RectTween(Rect begin = null, Rect end = null) : base(begin: begin, end: end) {
        }

        public override Rect lerp(float t) {
            return Rect.lerp(this.begin, this.end, t);
        }
    }

    public class IntTween : Tween<int> {
        public IntTween(int begin, int end) : base(begin: begin, end: end) {
        }

        public override int lerp(float t) {
            return (this.begin + (this.end - this.begin) * t).round();
        }
    }

    public class NullableFloatTween : Tween<float?> {
        public NullableFloatTween(float? begin = null, float? end = null) : base(begin: begin, end: end) {
        }

        public override float? lerp(float t) {
            D.assert(this.begin != null);
            D.assert(this.end != null);
            return this.begin + (this.end - this.begin) * t;
        }
    }

    public class FloatTween : Tween<float> {
        public FloatTween(float begin, float end) : base(begin: begin, end: end) {
        }

        public override float lerp(float t) {
            return this.begin + (this.end - this.begin) * t;
        }
    }

    public class StepTween : Tween<int> {
        public StepTween(int begin, int end) : base(begin: begin, end: end) {
        }

        public override int lerp(float t) {
            return (this.begin + (this.end - this.begin) * t).floor();
        }
    }

    public class OffsetTween : Tween<Offset> {
        public OffsetTween(Offset begin, Offset end) : base(begin: begin, end: end) {
        }

        public override Offset lerp(float t) {
            return (this.begin + (this.end - this.begin) * t);
        }
    }

    class ConstantTween<T> : Tween<T> {
        public ConstantTween(T value) : base(begin: value, end: value) {
        }

        public override T lerp(float t) {
            return this.begin;
        }

        public override string ToString() {
            return $"{this.GetType()}(value: {this.begin})";
        }
    }

    public class CurveTween : Animatable<float> {
        public CurveTween(Curve curve = null) {
            D.assert(curve != null);
            this.curve = curve;
        }

        public readonly Curve curve;

        public override float evaluate(Animation<float> animation) {
            float t = animation.value;
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