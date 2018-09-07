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
                throw new Exception(message ?? "assertion failed.");
            }
        }

        public static bool debugPrintGestureArenaDiagnostics = true;

        public static bool debugPrintHitTestResults = true;
        
        public static bool debugPaintPointersEnabled = false;
    }
}