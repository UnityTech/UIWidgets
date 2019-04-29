using System;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.gestures {
    public delegate void GestureLongPressCallback();

    public delegate void GestureLongPressUpCallback();

    public delegate void GestureLongPressDragStartCallback(GestureLongPressDragStartDetails details);

    public delegate void GestureLongPressDragUpdateCallback(GestureLongPressDragUpdateDetails details);

    public delegate void GestureLongPressDragUpCallback(GestureLongPressDragUpDetails details);

    public class GestureLongPressDragStartDetails {
        public GestureLongPressDragStartDetails(
            TimeSpan? sourceTimeStamp = null,
            Offset globalPosition = null
        ) {
            this.sourceTimeStamp = sourceTimeStamp;
            this.globalPosition = globalPosition ?? Offset.zero;
        }


        public readonly TimeSpan? sourceTimeStamp;

        public readonly Offset globalPosition;
    }

    public class GestureLongPressDragUpdateDetails {
        public GestureLongPressDragUpdateDetails(
            TimeSpan? sourceTimeStamp = null,
            Offset globalPosition = null,
            Offset offsetFromOrigin = null
        ) {
            this.sourceTimeStamp = sourceTimeStamp;
            this.globalPosition = globalPosition ?? Offset.zero;
            this.offsetFromOrigin = offsetFromOrigin ?? Offset.zero;
        }

        public readonly TimeSpan? sourceTimeStamp;

        public readonly Offset globalPosition;

        public readonly Offset offsetFromOrigin;
    }

    public class GestureLongPressDragUpDetails {
        public GestureLongPressDragUpDetails(
            TimeSpan? sourceTimeStamp = null,
            Offset globalPosition = null
        ) {
            this.sourceTimeStamp = sourceTimeStamp;
            this.globalPosition = globalPosition ?? Offset.zero;
        }

        public readonly TimeSpan? sourceTimeStamp;

        public readonly Offset globalPosition;
    }


    public class LongPressGestureRecognizer : PrimaryPointerGestureRecognizer {
        public LongPressGestureRecognizer(object debugOwner = null, PointerDeviceKind? kind = null) :
            base(deadline: Constants.kLongPressTimeout, debugOwner: debugOwner, kind: kind) {
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

    public class LongPressDragGestureRecognizer : PrimaryPointerGestureRecognizer {
        public LongPressDragGestureRecognizer(object debugOwner = null) : base(
            deadline: Constants.kLongPressTimeout,
            postAcceptSlopTolerance: null,
            debugOwner: debugOwner
        ) {
        }

        bool _longPressAccepted = false;

        Offset _longPressOrigin;

        TimeSpan? _longPressStartTimestamp;
        
        public GestureLongPressDragStartCallback onLongPressStart;

        public GestureLongPressDragUpdateCallback onLongPressDragUpdate;

        public GestureLongPressDragUpCallback onLongPressUp;

        protected override void didExceedDeadline() {
            this.resolve(GestureDisposition.accepted);
            this._longPressAccepted = true;
            base.acceptGesture(this.primaryPointer);
            if (this.onLongPressStart != null) {
                this.invokeCallback<object>("onLongPressStart", () => {
                    this.onLongPressStart(new GestureLongPressDragStartDetails(
                        sourceTimeStamp: this._longPressStartTimestamp,
                        globalPosition: this._longPressOrigin
                    ));
                    return null;
                });
            }
        }

        protected override void handlePrimaryPointer(PointerEvent e) {
            if (e is PointerUpEvent) {
                if (this._longPressAccepted == true && this.onLongPressUp != null) {
                    this._longPressAccepted = false;
                    this.invokeCallback<object>("onLongPressUp", () => {
                        this.onLongPressUp(new GestureLongPressDragUpDetails(
                            sourceTimeStamp: e.timeStamp,
                            globalPosition: e.position
                        ));
                        return null;
                    });
                }
                else {
                    this.resolve(GestureDisposition.rejected);
                }
            }
            else if (e is PointerDownEvent) {
                this._longPressAccepted = false;
                this._longPressStartTimestamp = e.timeStamp;
                this._longPressOrigin = e.position;
            }
            else if (e is PointerMoveEvent && this._longPressAccepted && this.onLongPressDragUpdate != null) {
                this.invokeCallback<object>("onLongPressDrag", () => {
                    this.onLongPressDragUpdate(new GestureLongPressDragUpdateDetails(
                        sourceTimeStamp: e.timeStamp,
                        globalPosition: e.position,
                        offsetFromOrigin: e.position - this._longPressOrigin
                    ));
                    return null;
                });
            }
        }

        public override void acceptGesture(int pointer) {
        }

        protected override void didStopTrackingLastPointer(int pointer) {
            this._longPressAccepted = false;
            this._longPressOrigin = null;
            this._longPressStartTimestamp = null;
            base.didStopTrackingLastPointer(pointer);
        }

        public override string debugDescription {
            get { return "long press drag"; }
        }
    }
}