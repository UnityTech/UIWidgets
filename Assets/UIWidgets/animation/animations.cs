using UIWidgets.foundation;
using UIWidgets.ui;

namespace UIWidgets.animation {
    class _AlwaysCompleteAnimation : Animation<double> {
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


        public override double value {
            get { return 1.0; }
        }

        public override string ToString() {
            return "kAlwaysCompleteAnimation";
        }
    }

    class _AlwaysDismissedAnimation : Animation<double> {
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


        public override double value {
            get { return 0.0; }
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
            return string.Format("{0} {1}; paused", base.toStringDetails(), this.value);
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

    public abstract class AnimationDouble : Animation<double> {
    }

    public class ProxyAnimation :
        AnimationLocalStatusListenersMixinAnimationLocalListenersMixinAnimationLazyListenerMixinAnimationDouble {
        public ProxyAnimation(Animation<double> animation = null) {
            this._parent = animation;
            if (this._parent == null) {
                this._status = AnimationStatus.dismissed;
                this._value = 0.0;
            }
        }

        AnimationStatus _status;

        double _value;

        public Animation<double> parent {
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

        Animation<double> _parent;

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

        public override double value {
            get { return this._parent != null ? this._parent.value : this._value; }
        }

        public override string ToString() {
            if (this.parent == null) {
                return string.Format("{0}(null; {1} {2:F3}", this.GetType(), this.toStringDetails(), this.value);
            }

            return string.Format("{0}\u27A9{1}", this.parent, this.GetType());
        }
    }


    public class ReverseAnimation : AnimationLocalStatusListenersMixinAnimationLazyListenerMixinAnimationDouble {
        public ReverseAnimation(Animation<double> parent) {
            D.assert(parent != null);
            this._parent = parent;
        }

        public Animation<double> parent {
            get { return this._parent; }
        }

        readonly Animation<double> _parent;

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

        public override double value {
            get { return 1.0 - this.parent.value; }
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

    public static class Animations {
        
        public static readonly Animation<double> kAlwaysCompleteAnimation = new _AlwaysCompleteAnimation();

        public static readonly Animation<double> kAlwaysDismissedAnimation = new _AlwaysDismissedAnimation();
    }
}