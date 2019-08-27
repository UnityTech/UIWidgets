using System;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    public delegate void GestureLongPressCallback();

    public delegate void GestureLongPressUpCallback();

    public delegate void GestureLongPressStartCallback(LongPressStartDetails details);

    public delegate void GestureLongPressMoveUpdateCallback(LongPressMoveUpdateDetails details);

    public delegate void GestureLongPressEndCallback(LongPressEndDetails details);

    public class LongPressStartDetails {
        public LongPressStartDetails(
            Offset globalPosition = null
        ) {
            this.globalPosition = globalPosition ?? Offset.zero;
        }

        public readonly Offset globalPosition;
    }

    public class LongPressMoveUpdateDetails {
        public LongPressMoveUpdateDetails(
            Offset globalPosition = null,
            Offset offsetFromOrigin = null
        ) {
            this.globalPosition = globalPosition ?? Offset.zero;
            this.offsetFromOrigin = offsetFromOrigin ?? Offset.zero;
        }

        public readonly Offset globalPosition;

        public readonly Offset offsetFromOrigin;
    }

    public class LongPressEndDetails {
        public LongPressEndDetails(
            Offset globalPosition = null
        ) {
            this.globalPosition = globalPosition ?? Offset.zero;
        }

        public readonly Offset globalPosition;
    }


    public class LongPressGestureRecognizer : PrimaryPointerGestureRecognizer {
        public LongPressGestureRecognizer(
            float? postAcceptSlopTolerance = null,
            object debugOwner = null,
            PointerDeviceKind? kind = null) : base(
            deadline: Constants.kLongPressTimeout,
            postAcceptSlopTolerance: postAcceptSlopTolerance,
            kind: kind,
            debugOwner: debugOwner) { }

        bool _longPressAccepted = false;

        Offset _longPressOrigin;

        public GestureLongPressCallback onLongPress;

        public GestureLongPressStartCallback onLongPressStart;

        public GestureLongPressMoveUpdateCallback onLongPressMoveUpdate;

        public GestureLongPressUpCallback onLongPressUp;

        public GestureLongPressEndCallback onLongPressEnd;

        protected override void didExceedDeadline() {
            this.resolve(GestureDisposition.accepted);
            this._longPressAccepted = true;
            base.acceptGesture(this.primaryPointer);
            if (this.onLongPress != null) {
                this.invokeCallback<object>("onLongPress", () => {
                    this.onLongPress();
                    return null;
                });
            }

            if (this.onLongPressStart != null) {
                this.invokeCallback<object>("onLongPressStart",
                    () => {
                        this.onLongPressStart(new LongPressStartDetails(globalPosition: this._longPressOrigin));
                        return null;
                    });
            }
        }

        protected override void handlePrimaryPointer(PointerEvent evt) {
            if (evt is PointerUpEvent) {
                if (this._longPressAccepted) {
                    if (this.onLongPressUp != null) {
                        this.invokeCallback<object>("onLongPressUp", () => {
                            this.onLongPressUp();
                            return null;
                        });
                    }

                    if (this.onLongPressEnd != null) {
                        this.invokeCallback<object>("onLongPressEnd", () => {
                            this.onLongPressEnd(new LongPressEndDetails(globalPosition: evt.position));
                            return null;
                        });
                    }

                    this._longPressAccepted = true;
                }
                else {
                    this.resolve(GestureDisposition.rejected);
                }
            }
            else if (evt is PointerDownEvent || evt is PointerCancelEvent) {
                this._longPressAccepted = false;
                this._longPressOrigin = evt.position;
            }
            else if (evt is PointerMoveEvent && this._longPressAccepted && this.onLongPressMoveUpdate != null) {
                this.invokeCallback<object>("onLongPressMoveUpdate", () => {
                    this.onLongPressMoveUpdate(new LongPressMoveUpdateDetails(
                        globalPosition: evt.position,
                        offsetFromOrigin: evt.position - this._longPressOrigin
                    ));
                    return null;
                });
            }
        }

        public override void acceptGesture(int pointer) {
        }

        public override string debugDescription {
            get { return "long press"; }
        }
    }
}