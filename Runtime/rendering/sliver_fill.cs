using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.rendering {
    public class RenderSliverFillViewport : RenderSliverFixedExtentBoxAdaptor {
        public RenderSliverFillViewport(
            RenderSliverBoxChildManager childManager = null,
            float viewportFraction = 1.0f
        ) :
            base(childManager: childManager) {
            D.assert(viewportFraction > 0.0);
            this._viewportFraction = viewportFraction;
        }

        public override float itemExtent {
            get { return this.constraints.viewportMainAxisExtent * this.viewportFraction; }
            set { }
        }

        float _viewportFraction;

        public float viewportFraction {
            get { return this._viewportFraction; }
            set {
                if (this._viewportFraction == value) {
                    return;
                }

                this._viewportFraction = value;
                this.markNeedsLayout();
            }
        }


        float _padding {
            get { return (1.0f - this.viewportFraction) * this.constraints.viewportMainAxisExtent * 0.5f; }
        }

        protected override float indexToLayoutOffset(float itemExtent, int index) {
            return this._padding + base.indexToLayoutOffset(itemExtent, index);
        }

        protected override int getMinChildIndexForScrollOffset(float scrollOffset, float itemExtent) {
            return base.getMinChildIndexForScrollOffset(Mathf.Max(scrollOffset - this._padding, 0.0f), itemExtent);
        }

        protected override int getMaxChildIndexForScrollOffset(float scrollOffset, float itemExtent) {
            return base.getMaxChildIndexForScrollOffset(Mathf.Max(scrollOffset - this._padding, 0.0f), itemExtent);
        }

        protected override float estimateMaxScrollOffset(SliverConstraints constraints,
            int firstIndex = 0,
            int lastIndex = 0,
            float leadingScrollOffset = 0.0f,
            float trailingScrollOffset = 0.0f
        ) {
            float padding = this._padding;
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
            RenderBox child = null
        ) : base(child: child) {
        }

        protected override void performLayout() {
            float extent = this.constraints.remainingPaintExtent - Mathf.Min(this.constraints.overlap, 0.0f);
            if (this.child != null) {
                this.child.layout(this.constraints.asBoxConstraints(minExtent: extent, maxExtent: extent),
                    parentUsesSize: true);
            }

            float paintedChildSize = this.calculatePaintOffset(this.constraints, from: 0.0f, to: extent);
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