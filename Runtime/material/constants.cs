using System;
using Unity.UIWidgets.painting;

namespace Unity.UIWidgets.material {
    
    public class Constants {

        public static double kToolbarHeight = 56.0;

        public static double kBottomNavigationBarHeight = 56.0;

        public static double kTextTabBarHeight = 48.0;
        
        public static TimeSpan kThemeChangeDuration = new TimeSpan(0, 0, 0, 0, 200);

        public static double kRadialReactionRadius = 20.0;

        public static TimeSpan kRadialReactionDuration = new TimeSpan(0, 0, 0, 0, 100);

        public static int kRadialReactionAlpha = 0x1F;
        
        public static TimeSpan kTabScrollDuration = new TimeSpan(0, 0, 0, 0, 300);
        
        public static EdgeInsets kTabLabelPadding = EdgeInsets.symmetric(horizontal: 16.0);

        public static EdgeInsets kMaterialListPadding = EdgeInsets.symmetric(vertical: 8.0);
    }
}