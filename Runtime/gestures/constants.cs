using System;

namespace Unity.UIWidgets.gestures {
    public static class Constants {
        public const float kTouchSlop = 18.0f;

        public const float kDoubleTapTouchSlop = kTouchSlop;

        public const float kDoubleTapSlop = 100.0f;

        public const float kPanSlop = kTouchSlop * 2.0f;

        public const float kScaleSlop = kTouchSlop;

        public static readonly TimeSpan kPressTimeout = new TimeSpan(0, 0, 0, 0, 100);

        public static readonly TimeSpan kDoubleTapTimeout = new TimeSpan(0, 0, 0, 0, 300);

        public static readonly TimeSpan kDoubleTapMinTime = new TimeSpan(0, 0, 0, 0, 40);

        public static readonly TimeSpan kLongPressTimeout = new TimeSpan(0, 0, 0, 0, 500);

        public const float kMinFlingVelocity = 50.0f;

        public const float kMaxFlingVelocity = 8000.0f;
    }
}