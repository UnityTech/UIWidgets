using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.utils {
    public static class AxisDirectionUtils {
        public static AxisDirection? getAxisDirectionFromAxisReverseAndDirectionality(
            BuildContext context,
            Axis axis,
            bool reverse) {
            switch (axis) {
                case Axis.horizontal:
                    return reverse ? AxisDirection.left : AxisDirection.right;
                case Axis.vertical:
                    return reverse ? AxisDirection.up : AxisDirection.down;
            }

            return null;
        }
    }
}