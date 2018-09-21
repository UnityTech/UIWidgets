using System;
using System.Diagnostics;

namespace UIWidgets.foundation {
    public static class D {
        [Conditional("UIWidgets_DEBUG")]
        public static void assert(Func<bool> result, string message = null) {
            D.assert(result(), message);
        }

        [Conditional("UIWidgets_DEBUG")]
        public static void assert(bool result, string message = null) {
            if (!result) {
                throw new AssertionError(message);
            }
        }

        public static bool debugPrintGestureArenaDiagnostics = true;

        public static bool debugPrintHitTestResults = false;
        
        public static bool debugPaintPointersEnabled = false;
        
        public static bool debugPrintRecognizerCallbacksTrace = false;
        
        public static bool debugPrintBeginFrameBanner = false;

        public static bool debugPrintEndFrameBanner = false;
        
        public static bool debugPrintScheduleFrameStacks = false;
    }

    [Serializable]
    public class AssertionError : Exception {
        public AssertionError(string message) : base(message) {
        }
    }
}