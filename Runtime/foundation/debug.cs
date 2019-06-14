using System;
using System.Diagnostics;
using System.Linq;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Debug = UnityEngine.Debug;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.foundation {
    public static class D {
        public static void logError(string message, Exception ex = null) {
            Debug.LogException(new AssertionError(message, ex));
        }

        public static bool debugEnabled {
            get {
#if UIWidgets_DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        [Conditional("UIWidgets_DEBUG")]
        public static void assert(Func<bool> result, Func<string> message = null) {
            if (!result()) {
                throw new AssertionError(message != null ? message() : "");
            }
        }

        [Conditional("UIWidgets_DEBUG")]
        public static void assert(bool result, Func<string> message = null) {
            if (!result) {
                throw new AssertionError(message != null ? message() : "");
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

        public static bool debugRepaintTextRainbowEnabled = false;

        public static bool debugPaintLayerBordersEnabled = false;

        public static bool debugPrintMarkNeedsLayoutStacks = false;

        public static bool debugPrintLayouts = false;

        public static bool debugDisableClipLayers = false;

        public static bool debugDisableOpacityLayers = false;

        public static bool debugPrintMarkNeedsPaintStacks = false;

        public static bool debugCheckIntrinsicSizes = false;

        public static bool debugPrintMouseHoverEvents = false;

        public static HSVColor debugCurrentRepaintColor =
            HSVColor.fromAHSV(0.4f, 60.0f, 1.0f, 1.0f);

        public static void _debugDrawDoubleRect(Canvas canvas, Rect outerRect, Rect innerRect, Color color) {
            Path path = new Path();
            path.addRect(outerRect);
            path.addRect(innerRect);
            path.winding(PathWinding.clockwise);
            var paint = new Paint {
                color = color
            };
            canvas.drawPath(path, paint);
        }

        public static void debugPaintPadding(Canvas canvas, Rect outerRect, Rect innerRect, float outlineWidth = 2.0f) {
            assert(() => {
                if (innerRect != null && !innerRect.isEmpty) {
                    _debugDrawDoubleRect(canvas, outerRect, innerRect, new Color(0x900090FF));
                    _debugDrawDoubleRect(canvas, innerRect.inflate(outlineWidth).intersect(outerRect), innerRect,
                        new Color(0xFF0090FF));
                }
                else {
                    Paint paint = new Paint();
                    paint.color = new Color(0x90909090);
                    canvas.drawRect(outerRect, paint);
                }

                return true;
            });
        }

        public static void setDebugPaint(bool? debugPaintSizeEnabled = null,
            bool? debugPaintBaselinesEnabled = null,
            bool? debugPaintPointersEnabled = null,
            bool? debugPaintLayerBordersEnabled = null,
            bool? debugRepaintRainbowEnabled = null) {
            bool needRepaint = false;
            if (debugPaintSizeEnabled != null && debugPaintSizeEnabled != D.debugPaintSizeEnabled) {
                D.debugPaintSizeEnabled = debugPaintSizeEnabled.Value;
                needRepaint = true;
            }

            if (debugPaintBaselinesEnabled != null && debugPaintBaselinesEnabled != D.debugPaintBaselinesEnabled) {
                D.debugPaintBaselinesEnabled = debugPaintBaselinesEnabled.Value;
                needRepaint = true;
            }

            if (debugPaintPointersEnabled != null && debugPaintPointersEnabled != D.debugPaintPointersEnabled) {
                D.debugPaintPointersEnabled = debugPaintPointersEnabled.Value;
                needRepaint = true;
            }

            if (debugPaintLayerBordersEnabled != null &&
                debugPaintLayerBordersEnabled != D.debugPaintLayerBordersEnabled) {
                D.debugPaintLayerBordersEnabled = debugPaintLayerBordersEnabled.Value;
                needRepaint = true;
            }

            if (debugRepaintRainbowEnabled != null && debugRepaintRainbowEnabled != D.debugRepaintRainbowEnabled) {
                D.debugRepaintRainbowEnabled = debugRepaintRainbowEnabled.Value;
                needRepaint = true;
            }

            if (needRepaint) {
                foreach (var adapter in WindowAdapter.windowAdapters) {
                    adapter._forceRepaint();
                }
            }
        }
    }

    [Serializable]
    public class AssertionError : Exception {
        readonly Exception innerException;

        public AssertionError(string message) : base(message) {
        }

        public AssertionError(string message, Exception innerException = null) : base(message) {
            this.innerException = innerException;
        }

        public override string StackTrace {
            get {
                if (this.innerException != null) {
                    return this.innerException.StackTrace;
                }

                var stackTrace = base.StackTrace;
                var lines = stackTrace.Split('\n');
                var strippedLines = lines.Skip(1);

                return string.Join("\n", strippedLines);
            }
        }
    }
}