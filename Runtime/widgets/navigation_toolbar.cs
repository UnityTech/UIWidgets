using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.widgets {
    public class NavigationToolbar : StatelessWidget {
        public NavigationToolbar(Key key = null, Widget leading = null, Widget middle = null,
            Widget trailing = null, bool centerMiddle = true, double middleSpacing = kMiddleSpacing) : base(key) {
            this.leading = leading;
            this.middle = middle;
            this.trailing = trailing;
            this.centerMiddle = centerMiddle;
            this.middleSpacing = middleSpacing;
        }

        public const double kMiddleSpacing = 16.0;

        public readonly Widget leading;
        public readonly Widget middle;
        public readonly Widget trailing;
        public readonly bool centerMiddle;
        public readonly double middleSpacing;

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            List<Widget> children = new List<Widget>();

            if (this.leading != null) {
                children.Add(new LayoutId(id: _ToolbarSlot.leading, child: this.leading));
            }

            if (this.middle != null) {
                children.Add(new LayoutId(id: _ToolbarSlot.middle, child: this.middle));
            }

            if (this.trailing != null) {
                children.Add(new LayoutId(id: _ToolbarSlot.trailing, child: this.trailing));
            }

            TextDirection textDirection = Directionality.of(context);
            return new CustomMultiChildLayout(
                layoutDelegate: new _ToolbarLayout(
                    centerMiddle: this.centerMiddle,
                    middleSpacing: this.middleSpacing,
                    textDirection: textDirection
                ),
                children: children
            );
        }
    }

    enum _ToolbarSlot {
        leading,
        middle,
        trailing,
    }

    class _ToolbarLayout : MultiChildLayoutDelegate {
        public _ToolbarLayout(
            bool? centerMiddle = true,
            double? middleSpacing = null,
            TextDirection? textDirection = null
        ) {
            D.assert(textDirection != null);
            D.assert(middleSpacing != null);
            this.centerMiddle = centerMiddle ?? true;
            this.middleSpacing = middleSpacing ?? 0.0;
            this.textDirection = textDirection ?? TextDirection.ltr;
        }


        public readonly bool centerMiddle;

        public readonly double middleSpacing;

        public readonly TextDirection textDirection;

        public override void performLayout(Size size) {
            double leadingWidth = 0.0;
            double trailingWidth = 0.0;

            if (this.hasChild(_ToolbarSlot.leading)) {
                BoxConstraints constraints = new BoxConstraints(
                    minWidth: 0.0,
                    maxWidth: size.width / 3.0,
                    minHeight: size.height,
                    maxHeight: size.height
                );
                leadingWidth = this.layoutChild(_ToolbarSlot.leading, constraints).width;
                double leadingX = 0.0;
                switch (this.textDirection) {
                    case TextDirection.rtl:
                        leadingX = size.width - leadingWidth;
                        break;
                    case TextDirection.ltr:
                        leadingX = 0.0;
                        break;
                }

                this.positionChild(_ToolbarSlot.leading, new Offset(leadingX, 0.0));
            }

            if (this.hasChild(_ToolbarSlot.trailing)) {
                BoxConstraints constraints = BoxConstraints.loose(size);
                Size trailingSize = this.layoutChild(_ToolbarSlot.trailing, constraints);
                double trailingX = 0.0;
                switch (this.textDirection) {
                    case TextDirection.rtl:
                        trailingX = 0.0;
                        break;
                    case TextDirection.ltr:
                        trailingX = size.width - trailingSize.width;
                        break;
                }

                double trailingY = (size.height - trailingSize.height) / 2.0;
                trailingWidth = trailingSize.width;
                this.positionChild(_ToolbarSlot.trailing, new Offset(trailingX, trailingY));
            }

            if (this.hasChild(_ToolbarSlot.middle)) {
                double maxWidth = Math.Max(size.width - leadingWidth - trailingWidth - this.middleSpacing * 2.0, 0.0);
                BoxConstraints constraints = BoxConstraints.loose(size).copyWith(maxWidth: maxWidth);
                Size middleSize = this.layoutChild(_ToolbarSlot.middle, constraints);

                double middleStartMargin = leadingWidth + this.middleSpacing;
                double middleStart = middleStartMargin;
                double middleY = (size.height - middleSize.height) / 2.0;
                // If the centered middle will not fit between the leading and trailing
                // widgets, then align its left or right edge with the adjacent boundary.
                if (this.centerMiddle) {
                    middleStart = (size.width - middleSize.width) / 2.0;
                    if (middleStart + middleSize.width > size.width - trailingWidth) {
                        middleStart = size.width - trailingWidth - middleSize.width;
                    }
                    else if (middleStart < middleStartMargin) {
                        middleStart = middleStartMargin;
                    }
                }

                double middleX = 0.0;
                switch (this.textDirection) {
                    case TextDirection.rtl:
                        middleX = size.width - middleSize.width - middleStart;
                        break;
                    case TextDirection.ltr:
                        middleX = middleStart;
                        break;
                }

                this.positionChild(_ToolbarSlot.middle, new Offset(middleX, middleY));
            }
        }

        public override bool shouldRelayout(MultiChildLayoutDelegate oldDelegate) {
            return ((_ToolbarLayout) oldDelegate).centerMiddle != this.centerMiddle
                   || ((_ToolbarLayout) oldDelegate).middleSpacing != this.middleSpacing
                   || ((_ToolbarLayout) oldDelegate).textDirection != this.textDirection;
        }
    }
}