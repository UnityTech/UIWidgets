using System;

namespace UIWidgets.rendering {
    public class RenderSliverList : RenderSliverMultiBoxAdaptor {
        RenderSliverList(
            RenderSliverBoxChildManager childManager = null
        ) : base(childManager: childManager) {
        }

        public override void performLayout() {
            this.childManager.didStartLayout();
            this.childManager.setDidUnderflow(false);

            double scrollOffset = this.constraints.scrollOffset + this.constraints.cacheOrigin;
            double remainingExtent = this.constraints.remainingCacheExtent;
            double targetEndScrollOffset = scrollOffset + remainingExtent;
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
            for (double earliestScrollOffset = this.childScrollOffset(earliestUsefulChild);
                earliestScrollOffset > scrollOffset;
                earliestScrollOffset = this.childScrollOffset(earliestUsefulChild)) {
                earliestUsefulChild = this.insertAndLayoutLeadingChild(childConstraints, parentUsesSize: true);

                if (earliestUsefulChild == null) {
                    var childParentData = (SliverMultiBoxAdaptorParentData) this.firstChild.parentData;
                    childParentData.layoutOffset = 0.0;

                    if (scrollOffset == 0.0) {
                        earliestUsefulChild = this.firstChild;
                        leadingChildWithLayout = earliestUsefulChild;
                        trailingChildWithLayout = trailingChildWithLayout ?? earliestUsefulChild;
                        break;
                    } else {
                        this.geometry = new SliverGeometry(
                            scrollOffsetCorrection: -scrollOffset
                        );
                        return;
                    }
                } else {
                    double firstChildScrollOffset = earliestScrollOffset - this.paintExtentOf(this.firstChild);
                    if (firstChildScrollOffset < 0.0) {
                        double correction = 0.0;
                        while (earliestUsefulChild != null) {
                            correction += this.paintExtentOf(firstChild);
                            earliestUsefulChild =
                                this.insertAndLayoutLeadingChild(childConstraints, parentUsesSize: true);
                        }

                        this.geometry = new SliverGeometry(
                            scrollOffsetCorrection: correction - earliestScrollOffset
                        );
                        var childParentData = (SliverMultiBoxAdaptorParentData) this.firstChild.parentData;
                        childParentData.layoutOffset = 0.0;
                        return;
                    } else {
                        var childParentData = (SliverMultiBoxAdaptorParentData) earliestUsefulChild.parentData;
                        childParentData.layoutOffset = firstChildScrollOffset;
                        leadingChildWithLayout = earliestUsefulChild;
                        trailingChildWithLayout = trailingChildWithLayout ?? earliestUsefulChild;
                    }
                }
            }

            if (leadingChildWithLayout == null) {
                earliestUsefulChild.layout(childConstraints, parentUsesSize: true);
                leadingChildWithLayout = earliestUsefulChild;
                trailingChildWithLayout = earliestUsefulChild;
            }

            bool inLayoutRange = true;
            RenderBox child = earliestUsefulChild;
            int index = this.indexOf(child);
            double endScrollOffset = this.childScrollOffset(child) + this.paintExtentOf(child);

            Func<bool> advance = () => {
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
                        child = insertAndLayoutChild(childConstraints,
                            after: trailingChildWithLayout,
                            parentUsesSize: true
                        );
                        if (child == null) {
                            return false;
                        }
                    } else {
                        child.layout(childConstraints, parentUsesSize: true);
                    }

                    trailingChildWithLayout = child;
                }

                var childParentData = (SliverMultiBoxAdaptorParentData) child.parentData;
                childParentData.layoutOffset = endScrollOffset;
                endScrollOffset = this.childScrollOffset(child) + this.paintExtentOf(child);
                return true;
            };

            while (endScrollOffset < scrollOffset) {
                leadingGarbage += 1;
                if (!advance()) {
                    this.collectGarbage(leadingGarbage - 1, 0);
                    double extent = this.childScrollOffset(this.lastChild) + this.paintExtentOf(this.lastChild);
                    this.geometry = new SliverGeometry(
                        scrollExtent: extent,
                        paintExtent: 0.0,
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

            double estimatedMaxScrollOffset;
            if (reachedEnd) {
                estimatedMaxScrollOffset = endScrollOffset;
            } else {
                estimatedMaxScrollOffset = this.childManager.estimateMaxScrollOffset(
                    this.constraints,
                    firstIndex: this.indexOf(this.firstChild),
                    lastIndex: this.indexOf(this.lastChild),
                    leadingScrollOffset: this.childScrollOffset(this.firstChild),
                    trailingScrollOffset: endScrollOffset
                );
            }

            double paintExtent = this.calculatePaintOffset(
                this.constraints,
                from: this.childScrollOffset(this.firstChild),
                to: endScrollOffset
            );
            double cacheExtent = this.calculateCacheOffset(
                this.constraints,
                from: this.childScrollOffset(this.firstChild),
                to: endScrollOffset
            );
            double targetEndScrollOffsetForPaint =
                this.constraints.scrollOffset + this.constraints.remainingPaintExtent;
            this.geometry = new SliverGeometry(
                scrollExtent: estimatedMaxScrollOffset,
                paintExtent: paintExtent,
                cacheExtent: cacheExtent,
                maxPaintExtent: estimatedMaxScrollOffset,
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