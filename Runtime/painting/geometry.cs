using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.painting {
    public static class Geometry {
        public static Offset positionDependentBox(
            Size size = null,
            Size childSize = null,
            Offset target = null,
            bool? preferBelow = null,
            float verticalOffset = 0.0f,
            float margin = 10.0f) {
            D.assert(size != null);
            D.assert(childSize != null);
            D.assert(target != null);
            D.assert(preferBelow != null);

            bool fitsBelow = target.dy + verticalOffset + childSize.height <= size.height - margin;
            bool fitsAbove = target.dy - verticalOffset - childSize.height >= margin;
            bool tooltipBelow = (preferBelow ?? true) ? (fitsBelow || !fitsAbove) : !(fitsAbove || !fitsBelow);
            float y;
            if (tooltipBelow) {
                y = Mathf.Min(target.dy + verticalOffset, size.height - margin);
            }
            else {
                y = Mathf.Max(target.dy - verticalOffset - childSize.height, margin);
            }

            float x;
            if (size.width - margin * 2.0 < childSize.width) {
                x = (size.width - childSize.width) / 2.0f;
            }
            else {
                float normalizedTargetX = target.dx.clamp(margin, size.width - margin);
                float edge = margin + childSize.width / 2.0f;
                if (normalizedTargetX < edge) {
                    x = margin;
                }
                else if (normalizedTargetX > size.width - edge) {
                    x = size.width - margin - childSize.width;
                }
                else {
                    x = normalizedTargetX - childSize.width / 2.0f;
                }
            }

            return new Offset(x, y);
        }
    }
}