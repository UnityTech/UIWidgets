using System;

namespace UIWidgets.gestures {
    public static class Constants {
        public const double kTouchSlop = 18.0;
        
        public const double kPanSlop = kTouchSlop * 2.0;

        public static readonly TimeSpan kPressTimeout = new TimeSpan(0, 0, 0, 100);
        
        public const double kMinFlingVelocity = 50.0;

        public const double kMaxFlingVelocity = 8000.0;
    }
}