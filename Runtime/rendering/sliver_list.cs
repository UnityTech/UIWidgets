using System;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.rendering {
    public class RenderSliverList : RenderSliverMultiBoxAdaptor {
        public RenderSliverList(
            RenderSliverBoxChildManager childManager = null
        ) : base(childManager: childManager) {
        }

        protected override void performLayout() {
            this.childManager.didStartLayout();
            this.childManager.setDidUnderflow(false);

            float scrollOffset = this.constraints.scrollOffset + this.constraints.cacheOrigin;
            D.assert(scrollOffset >= 0.0);
            float remainingExtent = this.constraints.remainingCacheExtent;
            D.assert(remainingExtent >= 0.0);
            float targetEndScrollOffset = scrollOffset + remainingExtent;
            BoxConstraints childConstraints = this.constraints.asBoxConstraints();
            int leadingGarbage = 0;
            int trailingGarbage = 0;
            bool reachedEnd = false;

            if (this.firstChild == null) {
                if (!this.addInitialChild()) {
                    this.geometry = SliverGeometry.zero;
                    this.childManager.didFinishLayout();
                    return;
                }
            }

            RenderBox leadingChildWithLayout = null, trailingChildWithLayout = null;

            RenderBox earliestUsefulChild = this.firstChild;
            for (float earliestScrollOffset = this.childScrollOffset(earliestUsefulChild);
                earliestScrollOffset > scrollOffset;
                earliestScrollOffset = this.childScrollOffset(earliestUsefulChild)) {
                earliestUsefulChild = this.insertAndLayoutLeadingChild(childConstraints, parentUsesSize: true);

                if (earliestUsefulChild == null) {
                    var childParentData = (SliverMultiBoxAdaptorParentData) this.firstChild.parentData;
                    childParentData.layoutOffset = 0.0f;

                    if (scrollOffset == 0.0) {
                        earliestUsefulChild = this.firstChild;
                        leadingChildWithLayout = earliestUsefulChild;
                        trailingChildWithLayout = trailingChildWithLayout ?? earliestUsefulChild;
                        break;
                    }
                    else {
                        this.geometry = new SliverGeometry(
                            scrollOffsetCorrection: -scrollOffset
                        );
                        return;
                    }
                }
                else {
                    float firstChildScrollOffset = earliestScrollOffset - this.paintExtentOf(this.firstChild);
                    if (firstChildScrollOffset < -SliverGeometry.precisionErrorTolerance) {
                        float correction = 0.0f;
                        while (earliestUsefulChild != null) {
                            D.assert(this.firstChild == earliestUsefulChild);
                            correction += this.paintExtentOf(this.firstChild);
                            earliestUsefulChild =
                                this.insertAndLayoutLeadingChild(childConstraints, parentUsesSize: true);
                        }

                        this.geometry = new SliverGeometry(
                            scrollOffsetCorrection: correction - earliestScrollOffset
                        );
                        var childParentData = (SliverMultiBoxAdaptorParentData) this.firstChild.parentData;
                        childParentData.layoutOffset = 0.0f;
                        return;
                    }
                    else {
                        var childParentData = (SliverMultiBoxAdaptorParentData) earliestUsefulChild.parentData;
                        childParentData.layoutOffset = firstChildScrollOffset;
                        D.assert(earliestUsefulChild == this.firstChild);
                        leadingChildWithLayout = earliestUsefulChild;
                        trailingChildWithLayout = trailingChildWithLayout ?? earliestUsefulChild;
                    }
                }
            }

            D.assert(earliestUsefulChild == this.firstChild);
            D.assert(this.childScrollOffset(earliestUsefulChild) <= scrollOffset);

            if (leadingChildWithLayout == null) {
                earliestUsefulChild.layout(childConstraints, parentUsesSize: true);
                leadingChildWithLayout = earliestUsefulChild;
                trailingChildWithLayout = earliestUsefulChild;
            }

            bool inLayoutRange = true;
            RenderBox child = earliestUsefulChild;
            int index = this.indexOf(child);
            float endScrollOffset = this.childScrollOffset(child) + this.paintExtentOf(child);

            Func<bool> advance = () => {
                D.assert(child != null);
                if (child == trailingChildWithLayout) {
                    inLayoutRange = false;
                }

                child = this.childAfter(child);
                if (child == null) {
                    inLayoutRange = false;
                }

                index += 1;
                if (!inLayoutRange) {
                    if (child == null || this.indexOf(child) != index) {
                        child = this.insertAndLayoutChild(childConstraints,
                            after: trailingChildWithLayout,
                            parentUsesSize: true
                        );
                        if (child == null) {
                            return false;
                        }
                    }
                    else {
                        child.layout(childConstraints, parentUsesSize: true);
                    }

                    trailingChildWithLayout = child;
                }

                D.assert(child != null);
                var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                childParentData.layoutOffset = endScrollOffset;
                D.assert(childParentData.index == index);
                endScrollOffset = this.childScrollOffset(child) + this.paintExtentOf(child);
                return true;
            };

            while (endScrollOffset < scrollOffset) {
                leadingGarbage += 1;
                if (!advance()) {
                    D.assert(leadingGarbage == this.childCount);
                    D.assert(child == null);

                    this.collectGarbage(leadingGarbage - 1, 0);
                    D.assert(this.firstChild == this.lastChild);
                    float extent = this.childScrollOffset(this.lastChild) + this.paintExtentOf(this.lastChild);
                    this.geometry = new SliverGeometry(
                        scrollExtent: extent,
                        paintExtent: 0.0f,
                        maxPaintExtent: extent
                    );
                    return;
                }
            }

            while (endScrollOffset < targetEndScrollOffset) {
                if (!advance()) {
                    reachedEnd = true;
                    break;
                }
            }

            if (child != null) {
                child = this.childAfter(child);
                while (child != null) {
                    trailingGarbage += 1;
                    child = this.childAfter(child);
                }
            }

            this.collectGarbage(leadingGarbage, trailingGarbage);

            D.assert(this.debugAssertChildListIsNonEmptyAndContiguous());

            float? estimatedMaxScrollOffset;
            if (reachedEnd) {
                estimatedMaxScrollOffset = endScrollOffset;
            }
            else {
                estimatedMaxScrollOffset = this.childManager.estimateMaxScrollOffset(
                    this.constraints,
                    firstIndex: this.indexOf(this.firstChild),
                    lastIndex: this.indexOf(this.lastChild),
                    leadingScrollOffset: this.childScrollOffset(this.firstChild),
                    trailingScrollOffset: endScrollOffset
                );

                D.assert(estimatedMaxScrollOffset >= endScrollOffset - this.childScrollOffset(this.firstChild));
            }

            float paintExtent = this.calculatePaintOffset(
                this.constraints,
                from: this.childScrollOffset(this.firstChild),
                to: endScrollOffset
            );
            float cacheExtent = this.calculateCacheOffset(
                this.constraints,
                from: this.childScrollOffset(this.firstChild),
                to: endScrollOffset
            );
            float targetEndScrollOffsetForPaint =
                this.constraints.scrollOffset + this.constraints.remainingPaintExtent;
            this.geometry = new SliverGeometry(
                scrollExtent: estimatedMaxScrollOffset.Value,
                paintExtent: paintExtent,
                cacheExtent: cacheExtent,
                maxPaintExtent: estimatedMaxScrollOffset.Value,
                hasVisualOverflow: endScrollOffset > targetEndScrollOffsetForPaint ||
                                   this.constraints.scrollOffset > 0.0
            );

            if (estimatedMaxScrollOffset == endScrollOffset) {
                this.childManager.setDidUnderflow(true);
            }

            this.childManager.didFinishLayout();
        }
    }
}