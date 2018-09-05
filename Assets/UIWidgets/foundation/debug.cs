using System;
using System.Diagnostics;

namespace UIWidgets.foundation {
    public static class D {
        [Conditional("UIWidgets_DEBUG")]
        public static void assert(Func<bool> result) {
            D.assert(result());
        }

        [Conditional("UIWidgets_DEBUG")]
        public static void assert(bool result) {
            if (!result) {
                throw new Exception("assertion failed. check stacktrace.");
            }
        }

        public static bool debugPrintGestureArenaDiagnostics = true;

        public static bool debugPrintHitTestResults = true;
    }
}