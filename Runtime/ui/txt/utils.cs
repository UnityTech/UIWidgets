using UnityEngine;

namespace Unity.UIWidgets.ui {
    class Utils {
        public static float PixelCorrectRound(float v) {
            return Mathf.Round(v * Window.instance.devicePixelRatio) / Window.instance.devicePixelRatio;
        }
    }
}