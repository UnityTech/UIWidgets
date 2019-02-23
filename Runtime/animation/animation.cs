using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.animation {
    public enum AnimationStatus {
        dismissed,
        forward,
        reverse,
        completed,
    }

    public delegate void AnimationStatusListener(AnimationStatus status);

    public abstract class Animation<T> : ValueListenable<T> {
        public abstract void addListener(VoidCallback listener);

        public abstract void removeListener(VoidCallback listener);

        public abstract void addStatusListener(AnimationStatusListener listener);

        public abstract void removeStatusListener(AnimationStatusListener listener);

        public abstract AnimationStatus status { get; }

        public abstract T value { get; }

        public bool isDismissed {
            get { return this.status == AnimationStatus.dismissed; }
        }

        public bool isCompleted {
            get { return this.status == AnimationStatus.completed; }
        }

        public override string ToString() {
            return $"{Diagnostics.describeIdentity(this)}({this.toStringDetails()})";
        }

        public virtual string toStringDetails() {
            string icon = null;
            switch (this.status) {
                case AnimationStatus.forward:
                    icon = "\u25B6"; // >
                    break;
                case AnimationStatus.reverse:
                    icon = "\u25C0"; // <
                    break;
                case AnimationStatus.completed:
                    icon = "\u23ED"; // >>|
                    break;
                case AnimationStatus.dismissed:
                    icon = "\u23EE"; // |<<
                    break;
            }

            D.assert(icon != null);
            return icon;
        }

        public Animation<U> drive<U>(Animatable<U> child) {
            D.assert(this is Animation<float>);
            return child.animate(this as Animation<float>);
        }
    }
}