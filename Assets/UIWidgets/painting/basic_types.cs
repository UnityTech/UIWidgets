using System;
using System.ComponentModel;

namespace UIWidgets.painting {
    public enum AxisDirection {
        up,
        right,
        down,
        left,
    }

    public enum Axis {
        horizontal,
        vertical,
    }

    public enum VerticalDirection {
        up,
        down,
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
    }

    /// The values in this enum are ordered such that they are in increasing order
    /// of cost. A value with index N implies all the values with index less than N.
    /// For example, [layout] (index 3) implies [paint] (2).
    public enum RenderComparison
    {
        identical,
        metadata,
        paint,
        layout,
    }
}