using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.rendering {
    public class RenderSliverFillViewport : RenderSliverFixedExtentBoxAdaptor {
        public RenderSliverFillViewport(
            RenderSliverBoxChildManager childManager = null,
            double viewportFraction = 1.0
        ) :
            base(childManager: childManager) {
            D.assert(viewportFraction > 0.0);
            this._viewportFraction = viewportFraction;
        }

        public override double itemExtent {
            get { return this.constraints.viewportMainAxisExtent * this.viewportFraction; }
            set { }
        }

        double _viewportFraction;

        public double viewportFraction {
            get { return this._viewportFraction; }
            set {
                if (this._viewportFraction == value) {
                    return;
                }

                this._viewportFraction = value;
                this.markNeedsLayout();
            }
        }


        double _padding {
            get { return (1.0 - this.viewportFraction) * this.constraints.viewportMainAxisExtent * 0.5; }
        }

        protected override double indexToLayoutOffset(double itemExtent, int index) {
            return this._padding + base.indexToLayoutOffset(itemExtent, index);
        }

        protected override int getMinChildIndexForScrollOffset(double scrollOffset, double itemExtent) {
            return base.getMinChildIndexForScrollOffset(Math.Max(scrollOffset - this._padding, 0.0), itemExtent);
        }

        protected override int getMaxChildIndexForScrollOffset(double scrollOffset, double itemExtent) {
            return base.getMaxChildIndexForScrollOffset(Math.Max(scrollOffset - this._padding, 0.0), itemExtent);
        }

        protected override double estimateMaxScrollOffset(SliverConstraints constraints,
            int firstIndex = 0,
            int lastIndex = 0,
            double leadingScrollOffset = 0.0,
            double trailingScrollOffset = 0.0
        ) {
            double padding = this._padding;
            return this.childManager.estimateMaxScrollOffset(
                       constraints,
                       firstIndex: firstIndex,
                       lastIndex: lastIndex,
                       leadingScrollOffset: leadingScrollOffset - padding,
                       trailingScrollOffset: trailingScrollOffset - padding
                   ) + padding + padding;
        }
    }

    public class RenderSliverFillRemaining : RenderSliverSingleBoxAdapter {
        public RenderSliverFillRemaining(
            RenderBox child
        ) : base(child: child) {
        }

        protected override void performLayout() {
            double extent = this.constraints.remainingPaintExtent - Math.Min(this.constraints.overlap, 0.0);
            if (this.child != null) {
                this.child.layout(this.constraints.asBoxConstraints(minExtent: extent, maxExtent: extent),
                    parentUsesSize: true);
            }

            double paintedChildSize = this.calculatePaintOffset(this.constraints, from: 0.0, to: extent);
            Debug.Log("size" + paintedChildSize);
            D.assert(paintedChildSize.isFinite());
            D.assert(paintedChildSize >= 0.0);
            this.geometry = new SliverGeometry(
                scrollExtent: this.constraints.viewportMainAxisExtent,
                paintExtent: paintedChildSize,
                maxPaintExtent: paintedChildSize,
                hasVisualOverflow: extent > this.constraints.remainingPaintExtent ||
                                   this.constraints.scrollOffset > 0.0
            );
            if (this.child != null) {
                this.setChildParentData(this.child, this.constraints, this.geometry);
            }
        }
    }
}