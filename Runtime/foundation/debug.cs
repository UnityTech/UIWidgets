using System;
using System.Diagnostics;
using System.Linq;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.foundation {
    public static class D {
        [Conditional("UIWidgets_DEBUG")]
        public static void assert(Func<bool> result, string message = null) {
            assert(result(), message);
        }

        [Conditional("UIWidgets_DEBUG")]
        public static void assert(bool result, string message = null) {
            if (!result) {
                throw new AssertionError(message);
            }
        }

        public static bool debugPrintGestureArenaDiagnostics = false;

        public static bool debugPrintHitTestResults = false;

        public static bool debugPaintPointersEnabled = false;

        public static bool debugPaintBaselinesEnabled = false;

        public static bool debugPrintRecognizerCallbacksTrace = false;

        public static bool debugPrintBeginFrameBanner = false;

        public static bool debugPrintEndFrameBanner = false;

        public static bool debugPrintScheduleFrameStacks = false;

        public static bool debugPaintSizeEnabled = false;

        public static bool debugRepaintRainbowEnabled = false;

        public static bool debugPaintLayerBordersEnabled = false;

        public static bool debugPrintMarkNeedsLayoutStacks = false;

        public static bool debugPrintLayouts = false;

        public static bool debugDisableClipLayers = false;

        public static bool debugDisableOpacityLayers = false;

        public static bool debugPrintMarkNeedsPaintStacks = false;

        public static bool debugCheckIntrinsicSizes = false;

        // public static Color debugCurrentRepaintColor = Color.fromfromAHSV(0.4, 60.0, 1.0, 1.0);;

        public static void _debugDrawDoubleRect(Canvas canvas, Rect outerRect, Rect innerRect, Color color) {
//            final Path path = new Path()
//                ..fillType = PathFillType.evenOdd
//                ..addRect(outerRect)
//                ..addRect(innerRect);
//            final Paint paint = new Paint()
//                ..color = color;
//            canvas.drawPath(path, paint);
        }

        public static void debugPaintPadding(Canvas canvas, Rect outerRect, Rect innerRect, double outlineWidth = 2.0) {
            assert(() => {
                if (innerRect != null && !innerRect.isEmpty) {
                    _debugDrawDoubleRect(canvas, outerRect, innerRect, new Color(0x900090FF));
                }
                else {
                    _debugDrawDoubleRect(canvas, innerRect.inflate(outlineWidth).intersect(outerRect), innerRect,
                        new Color(0xFF0090FF));
                    Paint paint = new Paint();
                    paint.color = new Color(0x90909090);
//                    canvas.drawRect(outerRect, BorderWidth.zero, BorderRadius.zero, paint);
                }

                return true;
            });
        }
    }

    [Serializable]
    public class AssertionError : Exception {
        public AssertionError(string message) : base(message) {
        }

        public override string StackTrace {
            get {
                var stackTrace = base.StackTrace;
                var lines = stackTrace.Split('\n');
                var strippedLines = lines.Skip(1);

                return string.Join("\n", strippedLines);
            }
        }
    }
}