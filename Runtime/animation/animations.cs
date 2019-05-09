using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.animation {
    class _AlwaysCompleteAnimation : Animation<float> {
        internal _AlwaysCompleteAnimation() {
        }

        public override void addListener(VoidCallback listener) {
        }

        public override void removeListener(VoidCallback listener) {
        }

        public override void addStatusListener(AnimationStatusListener listener) {
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
        }

        public override AnimationStatus status {
            get { return AnimationStatus.completed; }
        }


        public override float value {
            get { return 1.0f; }
        }

        public override string ToString() {
            return "kAlwaysCompleteAnimation";
        }
    }

    class _AlwaysDismissedAnimation : Animation<float> {
        internal _AlwaysDismissedAnimation() {
        }

        public override void addListener(VoidCallback listener) {
        }

        public override void removeListener(VoidCallback listener) {
        }

        public override void addStatusListener(AnimationStatusListener listener) {
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
        }

        public override AnimationStatus status {
            get { return AnimationStatus.dismissed; }
        }


        public override float value {
            get { return 0.0f; }
        }

        public override string ToString() {
            return "kAlwaysDismissedAnimation";
        }
    }

    public class AlwaysStoppedAnimation<T> : Animation<T> {
        public AlwaysStoppedAnimation(T value) {
            this._value = value;
        }

        public override T value {
            get { return this._value; }
        }

        readonly T _value;

        public override void addListener(VoidCallback listener) {
        }

        public override void removeListener(VoidCallback listener) {
        }

        public override void addStatusListener(AnimationStatusListener listener) {
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
        }

        public override AnimationStatus status {
            get { return AnimationStatus.forward; }
        }

        public override string toStringDetails() {
            return $"{base.toStringDetails()} {this.value}; paused";
        }
    }

    public abstract class AnimationWithParentMixin<TParent, T> : Animation<T> {
        public abstract Animation<TParent> parent { get; }

        public override void addListener(VoidCallback listener) {
            this.parent.addListener(listener);
        }

        public override void removeListener(VoidCallback listener) {
            this.parent.removeListener(listener);
        }

        public override void addStatusListener(AnimationStatusListener listener) {
            this.parent.addStatusListener(listener);
        }

        public override void removeStatusListener(AnimationStatusListener listener) {
            this.parent.removeStatusListener(listener);
        }

        public override AnimationStatus status {
            get { return this.parent.status; }
        }
    }

    public class ProxyAnimation :
        AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationLazyListenerMixinAnimation<float> {
        public ProxyAnimation(Animation<float> animation = null) {
            this._parent = animation;
            if (this._parent == null) {
                this._status = AnimationStatus.dismissed;
                this._value = 0.0f;
            }
        }

        AnimationStatus _status;

        float _value;

        public Animation<float> parent {
            get { return this._parent; }
            set {
                if (value == this._parent) {
                    return;
                }

                if (this._parent != null) {
                    this._status = this._parent.status;
                    this._value = this._parent.value;
                    if (this.isListening) {
                        this.didStopListening();
                    }
                }

                this._parent = value;
                if (this._parent != null) {
                    if (this.isListening) {
                        this.didStartListening();
                    }

                    if (this._value != this._parent.value) {
                        this.notifyListeners();
                    }

                    if (this._status != this._parent.status) {
                        this.notifyStatusListeners(this._parent.status);
                    }

                    this._status = AnimationStatus.dismissed;
                    this._value = 0;
                }
            }
        }

        Animation<float> _parent;

        protected override void didStartListening() {
            if (this._parent != null) {
                this._parent.addListener(this.notifyListeners);
                this._parent.addStatusListener(this.notifyStatusListeners);
            }
        }

        protected override void didStopListening() {
            if (this._parent != null) {
                this._parent.removeListener(this.notifyListeners);
                this._parent.removeStatusListener(this.notifyStatusListeners);
            }
        }

        public override AnimationStatus status {
            get { return this._parent != null ? this._parent.status : this._status; }
        }

        public override float value {
            get { return this._parent != null ? this._parent.value : this._value; }
        }

        public override string ToString() {
            if (this.parent == null) {
                return $"{this.GetType()}(null; {this.toStringDetails()} {this.value:F3}";
            }

            return $"{this.parent}\u27A9{this.GetType()}";
        }
    }


    public class ReverseAnimation : AnimationLocalStatusListenersMixinAnimationLazyListenerMixinAnimation<float> {
        public ReverseAnimation(Animation<float> parent) {
            D.assert(parent != null);
            this._parent = parent;
        }

        public Animation<float> parent {
            get { return this._parent; }
        }

        readonly Animation<float> _parent;

        public override void addListener(VoidCallback listener) {
            this.didRegisterListener();
            this.parent.addListener(listener);
        }

        public override void removeListener(VoidCallback listener) {
            this.parent.removeListener(listener);
            this.didUnregisterListener();
        }

        protected override void didStartListening() {
            this.parent.addStatusListener(this._statusChangeHandler);
        }

        protected override void didStopListening() {
            this.parent.removeStatusListener(this._statusChangeHandler);
        }

        void _statusChangeHandler(AnimationStatus status) {
            this.notifyStatusListeners(this._reverseStatus(status));
        }

        public override AnimationStatus status {
            get { return this._reverseStatus(this.parent.status); }
        }

        public override float value {
            get { return 1.0f - this.parent.value; }
        }

        AnimationStatus _reverseStatus(AnimationStatus status) {
            switch (status) {
                case AnimationStatus.forward: return AnimationStatus.reverse;
                case AnimationStatus.reverse: return AnimationStatus.forward;
                case AnimationStatus.completed: return AnimationStatus.dismissed;
                case AnimationStatus.dismissed: return AnimationStatus.completed;
            }

            D.assert(false);
            return default(AnimationStatus);
        }

        public override string ToString() {
            return this.parent + "\u27AA" + this.GetType();
        }
    }

    public class CurvedAnimation : AnimationWithParentMixin<float, float> {
        public CurvedAnimation(
            Animation<float> parent = null,
            Curve curve = null,
            Curve reverseCurve = null
        ) {
            D.assert(parent != null);
            D.assert(curve != null);
            this._parent = parent;
            this.curve = curve;
            this.reverseCurve = reverseCurve;

            this._updateCurveDirection(parent.status);
            parent.addStatusListener(this._updateCurveDirection);
        }

        public override Animation<float> parent {
            get { return this._parent; }
        }

        readonly Animation<float> _parent;

        public Curve curve;

        public Curve reverseCurve;

        AnimationStatus? _curveDirection;

        void _updateCurveDirection(AnimationStatus status) {
            switch (status) {
                case AnimationStatus.dismissed:
                case AnimationStatus.completed:
                    this._curveDirection = null;
                    break;
                case AnimationStatus.forward:
                    this._curveDirection = this._curveDirection ?? AnimationStatus.forward;
                    break;
                case AnimationStatus.reverse:
                    this._curveDirection = this._curveDirection ?? AnimationStatus.reverse;
                    break;
            }
        }

        bool _useForwardCurve {
            get {
                return this.reverseCurve == null ||
                       (this._curveDirection ?? this.parent.status) != AnimationStatus.reverse;
            }
        }

        public override float value {
            get {
                Curve activeCurve = this._useForwardCurve ? this.curve : this.reverseCurve;

                float t = this.parent.value;
                if (activeCurve == null) {
                    return t;
                }

                if (t == 0.0 || t == 1.0) {
                    D.assert(() => {
                        float transformedValue = activeCurve.transform(t);
                        float roundedTransformedValue = transformedValue.round();
                        if (roundedTransformedValue != t) {
                            throw new UIWidgetsError(
                                string.Format(
                                    "Invalid curve endpoint at {0}.\n" +
                                    "Curves must map 0.0 to near zero and 1.0 to near one but " +
                                    "{1} mapped {0} to {2}, which " +
                                    "is near {3}.",
                                    t, activeCurve.GetType(), transformedValue, roundedTransformedValue)
                            );
                        }

                        return true;
                    });
                    return t;
                }

                return activeCurve.transform(t);
            }
        }

        public override string ToString() {
            if (this.reverseCurve == null) {
                return this.parent + "\u27A9" + this.curve;
            }

            if (this._useForwardCurve) {
                return this.parent + "\u27A9" + this.curve + "\u2092\u2099/" + this.reverseCurve;
            }

            return this.parent + "\u27A9" + this.curve + "/" + this.reverseCurve + "\u2092\u2099";
        }
    }

    enum _TrainHoppingMode {
        minimize,
        maximize
    }

    public class TrainHoppingAnimation :
        AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationEagerListenerMixinAnimation<float> {
        public TrainHoppingAnimation(
            Animation<float> currentTrain = null,
            Animation<float> nextTrain = null,
            VoidCallback onSwitchedTrain = null) {
            D.assert(currentTrain != null);
            this._currentTrain = currentTrain;
            this._nextTrain = nextTrain;
            this.onSwitchedTrain = onSwitchedTrain;

            if (this._nextTrain != null) {
                if (this._currentTrain.value > this._nextTrain.value) {
                    this._mode = _TrainHoppingMode.maximize;
                }
                else {
                    this._mode = _TrainHoppingMode.minimize;
                    if (this._currentTrain.value == this._nextTrain.value) {
                        this._currentTrain = this._nextTrain;
                        this._nextTrain = null;
                    }
                }
            }

            this._currentTrain.addStatusListener(this._statusChangeHandler);
            this._currentTrain.addListener(this._valueChangeHandler);
            if (this._nextTrain != null) {
                this._nextTrain.addListener(this._valueChangeHandler);
            }
        }

        public Animation<float> currentTrain {
            get { return this._currentTrain; }
        }

        Animation<float> _currentTrain;
        Animation<float> _nextTrain;
        _TrainHoppingMode _mode;

        public VoidCallback onSwitchedTrain;

        AnimationStatus? _lastStatus;

        void _statusChangeHandler(AnimationStatus status) {
            D.assert(this._currentTrain != null);

            if (status != this._lastStatus) {
                this.notifyListeners();
                this._lastStatus = status;
            }

            D.assert(this._lastStatus != null);
        }

        public override AnimationStatus status {
            get { return this._currentTrain.status; }
        }

        float? _lastValue;

        void _valueChangeHandler() {
            D.assert(this._currentTrain != null);

            bool hop = false;
            if (this._nextTrain != null) {
                switch (this._mode) {
                    case _TrainHoppingMode.minimize:
                        hop = this._nextTrain.value <= this._currentTrain.value;
                        break;
                    case _TrainHoppingMode.maximize:
                        hop = this._nextTrain.value >= this._currentTrain.value;
                        break;
                }

                if (hop) {
                    this._currentTrain.removeStatusListener(this._statusChangeHandler);
                    this._currentTrain.removeListener(this._valueChangeHandler);
                    this._currentTrain = this._nextTrain;
                    this._nextTrain = null;
                    this._currentTrain.addStatusListener(this._statusChangeHandler);
                    this._statusChangeHandler(this._currentTrain.status);
                }
            }

            float newValue = this.value;
            if (newValue != this._lastValue) {
                this.notifyListeners();
                this._lastValue = newValue;
            }

            D.assert(this._lastValue != null);

            if (hop && this.onSwitchedTrain != null) {
                this.onSwitchedTrain();
            }
        }

        public override float value {
            get { return this._currentTrain.value; }
        }

        public override void dispose() {
            D.assert(this._currentTrain != null);

            this._currentTrain.removeStatusListener(this._statusChangeHandler);
            this._currentTrain.removeListener(this._valueChangeHandler);
            this._currentTrain = null;

            if (this._nextTrain != null) {
                this._nextTrain.removeListener(this._valueChangeHandler);
                this._nextTrain = null;
            }

            base.dispose();
        }

        public override string ToString() {
            if (this._nextTrain != null) {
                return $"{this.currentTrain}\u27A9{this.GetType()}(next: {this._nextTrain})";
            }

            return $"{this.currentTrain}\u27A9{this.GetType()}(no next)";
        }
    }

    public abstract class CompoundAnimation<T> :
        AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationLazyListenerMixinAnimation<T> {
        public CompoundAnimation(
            Animation<T> first = null,
            Animation<T> next = null
        ) {
            D.assert(first != null);
            D.assert(next != null);
            this.first = first;
            this.next = next;
        }

        public readonly Animation<T> first;

        public readonly Animation<T> next;

        protected override void didStartListening() {
            this.first.addListener(this._maybeNotifyListeners);
            this.first.addStatusListener(this._maybeNotifyStatusListeners);
            this.next.addListener(this._maybeNotifyListeners);
            this.next.addStatusListener(this._maybeNotifyStatusListeners);
        }

        protected override void didStopListening() {
            this.first.removeListener(this._maybeNotifyListeners);
            this.first.removeStatusListener(this._maybeNotifyStatusListeners);
            this.next.removeListener(this._maybeNotifyListeners);
            this.next.removeStatusListener(this._maybeNotifyStatusListeners);
        }

        public override AnimationStatus status {
            get {
                if (this.next.status == AnimationStatus.forward || this.next.status == AnimationStatus.reverse) {
                    return this.next.status;
                }

                return this.first.status;
            }
        }

        public override string ToString() {
            return $"{this.GetType()}({this.first}, {this.next})";
        }

        AnimationStatus _lastStatus;

        void _maybeNotifyStatusListeners(AnimationStatus _) {
            if (this.status != this._lastStatus) {
                this._lastStatus = this.status;
                this.notifyStatusListeners(this.status);
            }
        }

        T _lastValue;

        void _maybeNotifyListeners() {
            if (!Equals(this.value, this._lastValue)) {
                this._lastValue = this.value;
                this.notifyListeners();
            }
        }
    }

    public class AnimationMean : CompoundAnimation<float> {
        public AnimationMean(
            Animation<float> left = null,
            Animation<float> right = null
        ) : base(first: left, next: right) {
        }

        public override float value {
            get { return (this.first.value + this.next.value) / 2.0f; }
        }
    }

    public class AnimationMax : CompoundAnimation<float> {
        public AnimationMax(
            Animation<float> left = null,
            Animation<float> right = null
        ) : base(first: left, next: right) {
        }

        public override float value {
            get { return Mathf.Max(this.first.value, this.next.value); }
        }
    }

    public class AnimationMin : CompoundAnimation<float> {
        public AnimationMin(
            Animation<float> left = null,
            Animation<float> right = null
        ) : base(first: left, next: right) {
        }

        public override float value {
            get { return Mathf.Min(this.first.value, this.next.value); }
        }
    }

    public static class Animations {
        public static readonly Animation<float> kAlwaysCompleteAnimation = new _AlwaysCompleteAnimation();

        public static readonly Animation<float> kAlwaysDismissedAnimation = new _AlwaysDismissedAnimation();
    }
}