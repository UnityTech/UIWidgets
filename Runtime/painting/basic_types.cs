using System;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public enum RenderComparison {
        identical,
        metadata,
        function,
        paint,
        layout,
    }

    public enum Axis {
        horizontal,
        vertical,
    }

    public enum VerticalDirection {
        up,
        down,
    }

    public enum AxisDirection {
        up,
        right,
        down,
        left,
    }

    public static class AxisUtils {
        public static Axis flipAxis(Axis direction) {
            switch (direction) {
                case Axis.horizontal:
                    return Axis.vertical;
                case Axis.vertical:
                    return Axis.horizontal;
            }

            throw new Exception("unknown axis");
        }

        public static Axis axisDirectionToAxis(AxisDirection axisDirection) {
            switch (axisDirection) {
                case AxisDirection.up:
                case AxisDirection.down:
                    return Axis.vertical;
                case AxisDirection.left:
                case AxisDirection.right:
                    return Axis.horizontal;
            }

            throw new Exception("unknown axisDirection");
        }

        public static AxisDirection textDirectionToAxisDirection(TextDirection textDirection) {
            switch (textDirection) {
                case TextDirection.rtl:
                    return AxisDirection.left;
                case TextDirection.ltr:
                    return AxisDirection.right;
            }

            throw new Exception("unknown textDirection");
        }

        public static AxisDirection flipAxisDirection(AxisDirection axisDirection) {
            switch (axisDirection) {
                case AxisDirection.up:
                    return AxisDirection.down;
                case AxisDirection.right:
                    return AxisDirection.left;
                case AxisDirection.down:
                    return AxisDirection.up;
                case AxisDirection.left:
                    return AxisDirection.right;
            }

            throw new Exception("unknown axisDirection");
        }

        public static bool axisDirectionIsReversed(AxisDirection axisDirection) {
            switch (axisDirection) {
                case AxisDirection.up:
                case AxisDirection.left:
                    return true;
                case AxisDirection.down:
                case AxisDirection.right:
                    return false;
            }

            throw new Exception("unknown axisDirection");
        }
    }
}