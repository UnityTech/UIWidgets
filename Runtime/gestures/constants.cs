using System;

namespace Unity.UIWidgets.gestures {
    public static class Constants {
        public const double kTouchSlop = 18.0;

        public const double kDoubleTapTouchSlop = kTouchSlop;

        public const double kDoubleTapSlop = 100.0;

        public const double kPanSlop = kTouchSlop * 2.0;

        public static readonly TimeSpan kPressTimeout = new TimeSpan(0, 0, 0, 0, 100);

        public static readonly TimeSpan kDoubleTapTimeout = new TimeSpan(0, 0, 0, 0, 300);

        public static readonly TimeSpan kLongPressTimeout = new TimeSpan(0, 0, 0, 0, 500);

        public const double kMinFlingVelocity = 50.0;

        public const double kMaxFlingVelocity = 8000.0;
    }
}