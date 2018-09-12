using System;
using UIWidgets.foundation;
using UIWidgets.gestures;
using UIWidgets.painting;
using UIWidgets.physics;
using UIWidgets.scheduler;
using UIWidgets.ui;

namespace UIWidgets.widgets {
    public interface ScrollActivityDelegate {
        AxisDirection axisDirection { get; }

        double setPixels(double pixels);

        void applyUserOffset(double delta);

        void goIdle();

        void goBallistic(double velocity);
    }

    public abstract class ScrollActivity {
        public ScrollActivity(ScrollActivityDelegate @delegate) {
            this._delegate = @delegate;
        }

        public ScrollActivityDelegate @delegate {
            get { return this._delegate; }
        }

        ScrollActivityDelegate _delegate;

        public void updateDelegate(ScrollActivityDelegate value) {
            D.assert(this._delegate != value);
            this._delegate = value;
        }

        public virtual void resetActivity() {
        }

        public virtual void dispatchScrollStartNotification(ScrollMetrics metrics, BuildContext context) {
            new ScrollStartNotification(metrics: metrics, context: context).dispatch(context);
        }

        public virtual void dispatchScrollUpdateNotification(ScrollMetrics metrics, BuildContext context,
            double scrollDelta) {
            new ScrollUpdateNotification(metrics: metrics, context: context, scrollDelta: scrollDelta)
                .dispatch(context);
        }

        public virtual void dispatchOverscrollNotification(ScrollMetrics metrics, BuildContext context,
            double overscroll) {
            new OverscrollNotification(metrics: metrics, context: context, overscroll: overscroll).dispatch(context);
        }

        public virtual void dispatchScrollEndNotification(ScrollMetrics metrics, BuildContext context) {
            new ScrollEndNotification(metrics: metrics, context: context).dispatch(context);
        }

        public virtual void applyNewDimensions() {
        }

        public abstract bool shouldIgnorePointer { get; }

        public abstract bool isScrolling { get; }

        public abstract double velocity { get; }

        public virtual void dispose() {
            this._delegate = null;
        }

        public override string ToString() {
            return Diagnostics.describeIdentity(this);
        }
    }

    public class IdleScrollActivity : ScrollActivity {
        public IdleScrollActivity(ScrollActivityDelegate @delegate) : base(@delegate) {
        }

        public override void applyNewDimensions() {
            this.@delegate.goBallistic(0.0);
        }

        public override bool shouldIgnorePointer {
            get { return false; }
        }

        public override bool isScrolling {
            get { return false; }
        }

        public override double velocity {
            get { return 0.0; }
        }
    }

    public interface ScrollHoldController {
        void cancel();
    }

    public class HoldScrollActivity : ScrollActivity, ScrollHoldController {
        public HoldScrollActivity(
            ScrollActivityDelegate @delegate = null,
            VoidCallback onHoldCanceled = null
        ) : base(@delegate) {
            this.onHoldCanceled = onHoldCanceled;
        }

        public readonly VoidCallback onHoldCanceled;

        public override bool shouldIgnorePointer {
            get { return false; }
        }

        public override bool isScrolling {
            get { return false; }
        }

        public override double velocity {
            get { return 0.0; }
        }

        public void cancel() {
            this.@delegate.goBallistic(0.0);
        }

        public override void dispose() {
            if (this.onHoldCanceled != null)
                this.onHoldCanceled();
            base.dispose();
        }
    }

    public class ScrollDragController : Drag {
        public ScrollDragController(
            ScrollActivityDelegate @delegate = null,
            DragStartDetails details = null,
            VoidCallback onDragCanceled = null,
            double? carriedVelocity = null,
            double? motionStartDistanceThreshold = null
        ) {
            D.assert(@delegate != null);
            D.assert(details != null);
            D.assert(
                motionStartDistanceThreshold == null || motionStartDistanceThreshold > 0.0,
                "motionStartDistanceThreshold must be a positive number or null"
            );

            this._delegate = @delegate;
            this._lastDetails = details;
            this._retainMomentum = carriedVelocity != null && carriedVelocity != 0.0;
            this._lastNonStationaryTimestamp = details.sourceTimeStamp;
            this._offsetSinceLastStop = motionStartDistanceThreshold == null ? (double?) null : 0.0;

            this.onDragCanceled = onDragCanceled;
            this.carriedVelocity = carriedVelocity;
            this.motionStartDistanceThreshold = motionStartDistanceThreshold;
        }

        public ScrollActivityDelegate @delegate {
            get { return this._delegate; }
        }

        ScrollActivityDelegate _delegate;

        public readonly VoidCallback onDragCanceled;

        public readonly double? carriedVelocity;

        public readonly double? motionStartDistanceThreshold;

        DateTime _lastNonStationaryTimestamp;

        bool _retainMomentum;

        double? _offsetSinceLastStop;

        public static readonly TimeSpan momentumRetainStationaryDurationThreshold = new TimeSpan(0, 0, 0, 0, 20);

        public static readonly TimeSpan motionStoppedDurationThreshold = new TimeSpan(0, 0, 0, 0, 50);

        const double _bigThresholdBreakDistance = 24.0;

        bool _reversed {
            get { return AxisUtils.axisDirectionIsReversed(this.@delegate.axisDirection); }
        }

        public void updateDelegate(ScrollActivityDelegate value) {
            D.assert(this._delegate != value);
            this._delegate = value;
        }

        void _maybeLoseMomentum(double offset, DateTime? timestamp) {
            if (this._retainMomentum &&
                offset == 0.0 &&
                (timestamp == null ||
                 timestamp - this._lastNonStationaryTimestamp > momentumRetainStationaryDurationThreshold)) {
                this._retainMomentum = false;
            }
        }

        double _adjustForScrollStartThreshold(double offset, DateTime? timestamp) {
            if (timestamp == null) {
                return offset;
            }

            if (offset == 0.0) {
                if (this.motionStartDistanceThreshold != null &&
                    this._offsetSinceLastStop == null &&
                    timestamp - this._lastNonStationaryTimestamp > motionStoppedDurationThreshold) {
                    this._offsetSinceLastStop = 0.0;
                }

                return 0.0;
            } else {
                if (this._offsetSinceLastStop == null) {
                    return offset;
                } else {
                    this._offsetSinceLastStop += offset;
                    if (this._offsetSinceLastStop.Value.abs() > this.motionStartDistanceThreshold) {
                        this._offsetSinceLastStop = null;
                        if (offset.abs() > _bigThresholdBreakDistance) {
                            return offset;
                        } else {
                            return Math.Min(
                                       this.motionStartDistanceThreshold.Value / 3.0,
                                       offset.abs()
                                   ) * offset.sign();
                        }
                    } else {
                        return 0.0;
                    }
                }
            }
        }

        public void update(DragUpdateDetails details) {
            D.assert(details.primaryDelta != null);
            this._lastDetails = details;
            double offset = details.primaryDelta.Value;
            if (offset != 0.0) {
                this._lastNonStationaryTimestamp = details.sourceTimeStamp;
            }

            this._maybeLoseMomentum(offset, details.sourceTimeStamp);
            offset = this._adjustForScrollStartThreshold(offset, details.sourceTimeStamp);
            if (offset == 0.0) {
                return;
            }

            if (this._reversed) {
                offset = -offset;
            }

            this.@delegate.applyUserOffset(offset);
        }

        public void end(DragEndDetails details) {
            D.assert(details.primaryVelocity != null);
            double velocity = -details.primaryVelocity.Value;
            if (this._reversed) {
                velocity = -velocity;
            }

            this._lastDetails = details;

            if (this._retainMomentum && velocity.sign() == this.carriedVelocity.Value.sign()) {
                velocity += this.carriedVelocity.Value;
            }

            this.@delegate.goBallistic(velocity);
        }

        public void cancel() {
            this.@delegate.goBallistic(0.0);
        }

        public virtual void dispose() {
            this._lastDetails = null;
            if (this.onDragCanceled != null) {
                this.onDragCanceled();
            }
        }

        public object lastDetails {
            get { return this._lastDetails; }
        }

        object _lastDetails;

        public override string ToString() {
            return Diagnostics.describeIdentity(this);
        }
    }

    public class DragScrollActivity : ScrollActivity {
        public DragScrollActivity(
            ScrollActivityDelegate @delegate,
            ScrollDragController controller
        ) : base(@delegate) {
            this._controller = controller;
        }

        ScrollDragController _controller;

        public override void dispatchScrollStartNotification(ScrollMetrics metrics, BuildContext context) {
            object lastDetails = this._controller.lastDetails;
            D.assert(lastDetails is DragStartDetails);
            new ScrollStartNotification(metrics: metrics, context: context, dragDetails: (DragStartDetails) lastDetails)
                .dispatch(context);
        }

        public override void dispatchScrollUpdateNotification(ScrollMetrics metrics, BuildContext context,
            double scrollDelta) {
            object lastDetails = this._controller.lastDetails;
            D.assert(lastDetails is DragUpdateDetails);
            new ScrollUpdateNotification(metrics: metrics, context: context, scrollDelta: scrollDelta,
                dragDetails: (DragUpdateDetails) lastDetails).dispatch(context);
        }

        public override void dispatchOverscrollNotification(ScrollMetrics metrics, BuildContext context,
            double overscroll) {
            object lastDetails = this._controller.lastDetails;
            D.assert(lastDetails is DragUpdateDetails);
            new OverscrollNotification(metrics: metrics, context: context, overscroll: overscroll,
                dragDetails: (DragUpdateDetails) lastDetails).dispatch(context);
        }

        public override void dispatchScrollEndNotification(ScrollMetrics metrics, BuildContext context) {
            object lastDetails = this._controller.lastDetails;
            new ScrollEndNotification(
                metrics: metrics,
                context: context,
                dragDetails: lastDetails is DragEndDetails ? (DragEndDetails) lastDetails : null
            ).dispatch(context);
        }

        public override bool shouldIgnorePointer {
            get { return true; }
        }

        public override bool isScrolling {
            get { return true; }
        }

        public override double velocity {
            get { return 0.0; }
        }

        public override void dispose() {
            this._controller = null;
            base.dispose();
        }

        public override string ToString() {
            return string.Format("{0}({1})", Diagnostics.describeIdentity(this), this._controller);
        }
    }
}