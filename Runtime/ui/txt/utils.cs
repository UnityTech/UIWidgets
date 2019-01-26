using System;

namespace Unity.UIWidgets.ui {
    class Utils {
        public static double PixelCorrectRound(double v) {
            return Math.Round(v * Window.instance.devicePixelRatio) / Window.instance.devicePixelRatio;
        }
    }
}