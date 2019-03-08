using System;
using Unity.UIWidgets.painting;

namespace Unity.UIWidgets.material {
    public static class Constants {
        public const float kToolbarHeight = 56.0f;

        public const float kBottomNavigationBarHeight = 56.0f;

        public const float kTextTabBarHeight = 48.0f;

        public static readonly TimeSpan kThemeChangeDuration = new TimeSpan(0, 0, 0, 0, 200);

        public const float kRadialReactionRadius = 20.0f;

        public static readonly TimeSpan kRadialReactionDuration = new TimeSpan(0, 0, 0, 0, 100);

        public const int kRadialReactionAlpha = 0x1F;

        public static readonly TimeSpan kTabScrollDuration = new TimeSpan(0, 0, 0, 0, 300);

        public static readonly EdgeInsets kTabLabelPadding = EdgeInsets.symmetric(horizontal: 16.0f);

        public static readonly EdgeInsets kMaterialListPadding = EdgeInsets.symmetric(vertical: 8.0f);
    }
}