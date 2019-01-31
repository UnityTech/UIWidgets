namespace Unity.UIWidgets.gestures {
    public delegate void GestureLongPressCallback();

    public class LongPressGestureRecognizer : PrimaryPointerGestureRecognizer {
        public LongPressGestureRecognizer(object debugOwner = null) :
            base(deadline: Constants.kLongPressTimeout, debugOwner: debugOwner) {
        }

        public GestureLongPressCallback onLongPress;

        protected override void didExceedDeadline() {
            this.resolve(GestureDisposition.accepted);
            if (this.onLongPress != null) {
                this.invokeCallback<object>("onLongPress", () => {
                    this.onLongPress();
                    return null;
                });
            }
        }

        protected override void handlePrimaryPointer(PointerEvent evt) {
            if (evt is PointerUpEvent) {
                this.resolve(GestureDisposition.rejected);
            }
        }

        public override string debugDescription {
            get { return "long press"; }
        }
    }
}