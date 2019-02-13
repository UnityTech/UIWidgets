using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public static class Geometry {
        public static Offset positionDependentBox(
            Size size = null,
            Size childSize = null,
            Offset target = null,
            bool? preferBelow = null,
            double verticalOffset = 0.0,
            double margin = 10.0) {
            D.assert(size != null);
            D.assert(childSize != null);
            D.assert(target != null);
            D.assert(preferBelow != null);

            bool fitsBelow = target.dy + verticalOffset + childSize.height <= size.height - margin;
            bool fitsAbove = target.dy - verticalOffset - childSize.height >= margin;
            bool tooltipBelow = (preferBelow ?? true) ? (fitsBelow || !fitsAbove) : !(fitsAbove || !fitsBelow);
            double y;
            if (tooltipBelow) {
                y = Math.Min(target.dy + verticalOffset, size.height - margin);
            }
            else {
                y = Math.Max(target.dy - verticalOffset - childSize.height, margin);
            }

            double x;
            if (size.width - margin * 2.0 < childSize.width) {
                x = (size.width - childSize.width) / 2.0;
            }
            else {
                double normalizedTargetX = target.dx.clamp(margin, size.width - margin);
                double edge = margin + childSize.width / 2.0;
                if (normalizedTargetX < edge) {
                    x = margin;
                }
                else if (normalizedTargetX > size.width - edge) {
                    x = size.width - margin - childSize.width;
                }
                else {
                    x = normalizedTargetX - childSize.width / 2.0;
                }
            }

            return new Offset(x, y);
        }
    }
}