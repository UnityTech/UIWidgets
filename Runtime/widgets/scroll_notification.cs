using System.Collections.Generic;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {
    public abstract class ScrollNotification : ViewportNotificationMixinLayoutChangedNotification {
        protected ScrollNotification(
            ScrollMetrics metrics = null,
            BuildContext context = null
        ) {
            this.metrics = metrics;
            this.context = context;
        }

        public readonly ScrollMetrics metrics;

        public readonly BuildContext context;

        protected override void debugFillDescription(List<string> description) {
            base.debugFillDescription(description);
            description.Add(this.metrics.ToString());
        }

        public static bool defaultScrollNotificationPredicate(ScrollNotification notification) {
            return notification.depth == 0;
        }
    }

    public class ScrollStartNotification : ScrollNotification {
        public ScrollStartNotification(
            ScrollMetrics metrics = null,
            BuildContext context = null,
            DragStartDetails dragDetails = null
        ) : base(metrics: metrics, context: context) {
            this.dragDetails = dragDetails;
        }

        public readonly DragStartDetails dragDetails;

        protected override void debugFillDescription(List<string> description) {
            base.debugFillDescription(description);
            if (this.dragDetails != null) {
                description.Add(this.dragDetails.ToString());
            }
        }
    }

    public class ScrollUpdateNotification : ScrollNotification {
        public ScrollUpdateNotification(
            ScrollMetrics metrics = null,
            BuildContext context = null,
            DragUpdateDetails dragDetails = null,
            float scrollDelta = 0
        ) : base(metrics: metrics, context: context) {
            this.dragDetails = dragDetails;
            this.scrollDelta = scrollDelta;
        }

        public readonly DragUpdateDetails dragDetails;

        public readonly float scrollDelta;

        protected override void debugFillDescription(List<string> description) {
            base.debugFillDescription(description);
            description.Add($"scrollDelta: {this.scrollDelta}");
            if (this.dragDetails != null) {
                description.Add(this.dragDetails.ToString());
            }
        }
    }

    public class OverscrollNotification : ScrollNotification {
        public OverscrollNotification(
            ScrollMetrics metrics = null,
            BuildContext context = null,
            DragUpdateDetails dragDetails = null,
            float overscroll = 0,
            float velocity = 0
        ) : base(metrics: metrics, context: context) {
            this.dragDetails = dragDetails;
            this.overscroll = overscroll;
            this.velocity = velocity;
        }

        public readonly DragUpdateDetails dragDetails;

        public readonly float overscroll;

        public readonly float velocity;

        protected override void debugFillDescription(List<string> description) {
            base.debugFillDescription(description);
            description.Add($"overscroll: {this.overscroll:F1}");
            description.Add($"velocity: {this.velocity:F1}");
            if (this.dragDetails != null) {
                description.Add(this.dragDetails.ToString());
            }
        }
    }

    public class ScrollEndNotification : ScrollNotification {
        public ScrollEndNotification(
            ScrollMetrics metrics = null,
            BuildContext context = null,
            DragEndDetails dragDetails = null,
            float overscroll = 0,
            float velocity = 0
        ) : base(metrics: metrics, context: context) {
            this.dragDetails = dragDetails;
        }

        public readonly DragEndDetails dragDetails;

        protected override void debugFillDescription(List<string> description) {
            base.debugFillDescription(description);
            if (this.dragDetails != null) {
                description.Add(this.dragDetails.ToString());
            }
        }
    }

    public class UserScrollNotification : ScrollNotification {
        public UserScrollNotification(
            ScrollMetrics metrics = null,
            BuildContext context = null,
            ScrollDirection direction = ScrollDirection.idle
        ) : base(metrics: metrics, context: context) {
            this.direction = direction;
        }

        public readonly ScrollDirection direction;

        protected override void debugFillDescription(List<string> description) {
            base.debugFillDescription(description);
            description.Add($"direction: {this.direction}");
        }
    }

    public delegate bool ScrollNotificationPredicate(ScrollNotification notification);
}