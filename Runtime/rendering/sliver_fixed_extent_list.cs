using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.rendering {
    public abstract class RenderSliverFixedExtentBoxAdaptor : RenderSliverMultiBoxAdaptor {
        protected RenderSliverFixedExtentBoxAdaptor(
            RenderSliverBoxChildManager childManager = null
        ) : base(childManager: childManager) {
        }

        public abstract float itemExtent { get; set; }

        protected virtual float indexToLayoutOffset(float itemExtent, int index) {
            return itemExtent * index;
        }

        protected virtual int getMinChildIndexForScrollOffset(float scrollOffset, float itemExtent) {
            return itemExtent > 0.0 ? Mathf.Max(0, (int) (scrollOffset / itemExtent)) : 0;
        }

        protected virtual int getMaxChildIndexForScrollOffset(float scrollOffset, float itemExtent) {
            return itemExtent > 0.0 ? Mathf.Max(0, (int) Mathf.Ceil(scrollOffset / itemExtent) - 1) : 0;
        }

        protected virtual float estimateMaxScrollOffset(SliverConstraints constraints,
            int firstIndex = 0,
            int lastIndex = 0,
            float leadingScrollOffset = 0.0f,
            float trailingScrollOffset = 0.0f
        ) {
            return this.childManager.estimateMaxScrollOffset(
                constraints,
                firstIndex: firstIndex,
                lastIndex: lastIndex,
                leadingScrollOffset: leadingScrollOffset,
                trailingScrollOffset: trailingScrollOffset
            );
        }

        protected float computeMaxScrollOffset(SliverConstraints constraints, float itemExtent) {
            return this.childManager.childCount.Value * itemExtent;
        }

        protected override void performLayout() {
            this.childManager.didStartLayout();
            this.childManager.setDidUnderflow(false);

            float itemExtent = this.itemExtent;

            float scrollOffset = this.constraints.scrollOffset + this.constraints.cacheOrigin;
            D.assert(scrollOffset >= 0.0);
            float remainingExtent = this.constraints.remainingCacheExtent;
            D.assert(remainingExtent >= 0.0);
            float targetEndScrollOffset = scrollOffset + remainingExtent;

            BoxConstraints childConstraints = this.constraints.asBoxConstraints(
                minExtent: itemExtent,
                maxExtent: itemExtent
            );

            int firstIndex = this.getMinChildIndexForScrollOffset(scrollOffset, itemExtent);
            int? targetLastIndex = targetEndScrollOffset.isFinite()
                ? this.getMaxChildIndexForScrollOffset(targetEndScrollOffset, itemExtent)
                : (int?) null;

            if (this.firstChild != null) {
                int oldFirstIndex = this.indexOf(this.firstChild);
                int oldLastIndex = this.indexOf(this.lastChild);
                int leadingGarbage = (firstIndex - oldFirstIndex).clamp(0, this.childCount);
                int trailingGarbage =
                    targetLastIndex == null ? 0 : (oldLastIndex - targetLastIndex.Value).clamp(0, this.childCount);
                this.collectGarbage(leadingGarbage, trailingGarbage);
            }
            else {
                this.collectGarbage(0, 0);
            }

            if (this.firstChild == null) {
                if (!this.addInitialChild(index: firstIndex,
                    layoutOffset: this.indexToLayoutOffset(itemExtent, firstIndex))) {
                    float max = this.computeMaxScrollOffset(this.constraints, itemExtent);
                    this.geometry = new SliverGeometry(
                        scrollExtent: max,
                        maxPaintExtent: max
                    );
                    this.childManager.didFinishLayout();
                    return;
                }
            }

            RenderBox trailingChildWithLayout = null;

            for (int index = this.indexOf(this.firstChild) - 1; index >= firstIndex; --index) {
                RenderBox child = this.insertAndLayoutLeadingChild(childConstraints);
                if (child == null) {
                    this.geometry = new SliverGeometry(scrollOffsetCorrection: index * itemExtent);
                    return;
                }

                var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                childParentData.layoutOffset = this.indexToLayoutOffset(itemExtent, index);
                D.assert(childParentData.index == index);
                trailingChildWithLayout = trailingChildWithLayout ?? child;
            }

            if (trailingChildWithLayout == null) {
                this.firstChild.layout(childConstraints);
                var childParentData = (SliverMultiBoxAdaptorParentData) this.firstChild.parentData;
                childParentData.layoutOffset = this.indexToLayoutOffset(itemExtent, firstIndex);
                trailingChildWithLayout = this.firstChild;
            }

            while (targetLastIndex == null || this.indexOf(trailingChildWithLayout) < targetLastIndex) {
                RenderBox child = this.childAfter(trailingChildWithLayout);
                if (child == null) {
                    child = this.insertAndLayoutChild(childConstraints, after: trailingChildWithLayout);
                    if (child == null) {
                        break;
                    }
                }
                else {
                    child.layout(childConstraints);
                }

                trailingChildWithLayout = child;
                D.assert(child != null);
                var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                childParentData.layoutOffset = this.indexToLayoutOffset(itemExtent, childParentData.index);
            }

            int lastIndex = this.indexOf(this.lastChild);
            float leadingScrollOffset = this.indexToLayoutOffset(itemExtent, firstIndex);
            float trailingScrollOffset = this.indexToLayoutOffset(itemExtent, lastIndex + 1);

            D.assert(firstIndex == 0 || this.childScrollOffset(this.firstChild) <= scrollOffset);
            D.assert(this.debugAssertChildListIsNonEmptyAndContiguous());
            D.assert(this.indexOf(this.firstChild) == firstIndex);
            D.assert(targetLastIndex == null || lastIndex <= targetLastIndex);


            float estimatedMaxScrollOffset = this.estimateMaxScrollOffset(
                this.constraints,
                firstIndex: firstIndex,
                lastIndex: lastIndex,
                leadingScrollOffset: leadingScrollOffset,
                trailingScrollOffset: trailingScrollOffset
            );

            float paintExtent = this.calculatePaintOffset(
                this.constraints,
                from: leadingScrollOffset,
                to: trailingScrollOffset
            );

            float cacheExtent = this.calculateCacheOffset(
                this.constraints,
                from: leadingScrollOffset,
                to: trailingScrollOffset
            );

            float targetEndScrollOffsetForPaint =
                this.constraints.scrollOffset + this.constraints.remainingPaintExtent;
            int? targetLastIndexForPaint = targetEndScrollOffsetForPaint.isFinite()
                ? this.getMaxChildIndexForScrollOffset(targetEndScrollOffsetForPaint, itemExtent)
                : (int?) null;
            this.geometry = new SliverGeometry(
                scrollExtent: estimatedMaxScrollOffset,
                paintExtent: paintExtent,
                cacheExtent: cacheExtent,
                maxPaintExtent: estimatedMaxScrollOffset,
                hasVisualOverflow: (targetLastIndexForPaint != null && lastIndex >= targetLastIndexForPaint)
                                   || this.constraints.scrollOffset > 0.0
            );

            if (estimatedMaxScrollOffset == trailingScrollOffset) {
                this.childManager.setDidUnderflow(true);
            }

            this.childManager.didFinishLayout();
        }
    }

    public class RenderSliverFixedExtentList : RenderSliverFixedExtentBoxAdaptor {
        public RenderSliverFixedExtentList(
            RenderSliverBoxChildManager childManager = null,
            float itemExtent = 0.0f
        ) : base(childManager: childManager) {
            this._itemExtent = itemExtent;
        }

        public override float itemExtent {
            get { return this._itemExtent; }
            set {
                if (this._itemExtent == value) {
                    return;
                }

                this._itemExtent = value;
                this.markNeedsLayout();
            }
        }

        float _itemExtent;
    }
}