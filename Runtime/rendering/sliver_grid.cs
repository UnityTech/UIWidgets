using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace com.unity.uiwidgets.Runtime.rendering {
    public class SliverGridGeometry {
        public SliverGridGeometry(
            float? scrollOffset = null,
            float? crossAxisOffset = null,
            float? mainAxisExtent = null,
            float? crossAxisExtent = null
        ) {
            this.scrollOffset = scrollOffset;
            this.crossAxisOffset = crossAxisOffset;
            this.mainAxisExtent = mainAxisExtent;
            this.crossAxisExtent = crossAxisExtent;
        }

        public readonly float? scrollOffset;

        public readonly float? crossAxisOffset;

        public readonly float? mainAxisExtent;

        public readonly float? crossAxisExtent;

        public float? trailingScrollOffset {
            get { return this.scrollOffset + this.mainAxisExtent; }
        }

        public BoxConstraints getBoxConstraints(SliverConstraints constraints) {
            return constraints.asBoxConstraints(
                minExtent: this.mainAxisExtent ?? 0.0f,
                maxExtent: this.mainAxisExtent ?? 0.0f,
                crossAxisExtent: this.crossAxisExtent ?? 0.0f
            );
        }

        public override string ToString() {
            return "SliverGridGeometry(" +
                   "scrollOffset: $scrollOffset, " +
                   "crossAxisOffset: $crossAxisOffset, " +
                   "mainAxisExtent: $mainAxisExtent, " +
                   "crossAxisExtent: $crossAxisExtent" +
                   ")";
        }
    }

    public abstract class SliverGridLayout {
        public SliverGridLayout() {
        }

        public abstract int getMinChildIndexForScrollOffset(float scrollOffset);

        public abstract int getMaxChildIndexForScrollOffset(float scrollOffset);

        public abstract SliverGridGeometry getGeometryForChildIndex(int index);

        public abstract float computeMaxScrollOffset(int childCount);
    }

    public class SliverGridRegularTileLayout : SliverGridLayout {
        public SliverGridRegularTileLayout(
            int? crossAxisCount = null,
            float? mainAxisStride = null,
            float? crossAxisStride = null,
            float? childMainAxisExtent = null,
            float? childCrossAxisExtent = null,
            bool? reverseCrossAxis = null
        ) {
            D.assert(crossAxisCount != null && crossAxisCount > 0);
            D.assert(mainAxisStride != null && mainAxisStride >= 0);
            D.assert(crossAxisStride != null && crossAxisStride >= 0);
            D.assert(childMainAxisExtent != null && childMainAxisExtent >= 0);
            D.assert(childCrossAxisExtent != null && childCrossAxisExtent >= 0);
            D.assert(reverseCrossAxis != null);
            this.crossAxisCount = crossAxisCount;
            this.mainAxisStride = mainAxisStride;
            this.crossAxisStride = crossAxisStride;
            this.childMainAxisExtent = childMainAxisExtent;
            this.childCrossAxisExtent = childCrossAxisExtent;
            this.reverseCrossAxis = reverseCrossAxis;
        }

        public readonly int? crossAxisCount;

        public readonly float? mainAxisStride;

        public readonly float? crossAxisStride;

        public readonly float? childMainAxisExtent;

        public readonly float? childCrossAxisExtent;

        public readonly bool? reverseCrossAxis;

        public override int getMinChildIndexForScrollOffset(float scrollOffset) {
            return (this.mainAxisStride > 0.0f
                       ? this.crossAxisCount * ((int) (scrollOffset / this.mainAxisStride))
                       : 0) ?? 0;
        }

        public override int getMaxChildIndexForScrollOffset(float scrollOffset) {
            if (this.mainAxisStride > 0.0f) {
                int? mainAxisCount = (scrollOffset / this.mainAxisStride)?.ceil();
                return Mathf.Max(0, (this.crossAxisCount * mainAxisCount - 1) ?? 0);
            }

            return 0;
        }

        float _getOffsetFromStartInCrossAxis(float crossAxisStart) {
            if (this.reverseCrossAxis == true) {
                return (this.crossAxisCount * this.crossAxisStride - crossAxisStart - this.childCrossAxisExtent
                        - (this.crossAxisStride - this.childCrossAxisExtent)) ??
                       0.0f;
            }

            return crossAxisStart;
        }

        public override SliverGridGeometry getGeometryForChildIndex(int index) {
            float? crossAxisStart = (index % this.crossAxisCount) * this.crossAxisStride;
            return new SliverGridGeometry(
                scrollOffset: (index / this.crossAxisCount) * this.mainAxisStride,
                crossAxisOffset:
                this._getOffsetFromStartInCrossAxis(crossAxisStart ?? 0.0f),
                mainAxisExtent:
                this.childMainAxisExtent,
                crossAxisExtent:
                this.childCrossAxisExtent
            );
        }

        public override float computeMaxScrollOffset(int childCount) {
            int? mainAxisCount = ((childCount - 1) / this.crossAxisCount) + 1;
            float? mainAxisSpacing = this.mainAxisStride - this.childMainAxisExtent;
            return (this.mainAxisStride * mainAxisCount - mainAxisSpacing) ?? 0.0f;
        }
    }

    public abstract class SliverGridDelegate {
        public abstract SliverGridLayout getLayout(SliverConstraints constraints);

        public abstract bool shouldRelayout(SliverGridDelegate oldDelegate);
    }

    public class SliverGridDelegateWithFixedCrossAxisCount : SliverGridDelegate {
        public SliverGridDelegateWithFixedCrossAxisCount(
            int crossAxisCount,
            float mainAxisSpacing = 0.0f,
            float crossAxisSpacing = 0.0f,
            float childAspectRatio = 1.0f
        ) {
            D.assert(crossAxisCount > 0);
            D.assert(mainAxisSpacing >= 0);
            D.assert(crossAxisSpacing >= 0);
            D.assert(childAspectRatio > 0);
            this.crossAxisCount = crossAxisCount;
            this.mainAxisSpacing = mainAxisSpacing;
            this.crossAxisSpacing = crossAxisSpacing;
            this.childAspectRatio = childAspectRatio;
        }

        public readonly int crossAxisCount;

        public readonly float mainAxisSpacing;

        public readonly float crossAxisSpacing;

        public readonly float childAspectRatio;

        bool _debugAssertIsValid() {
            D.assert(this.crossAxisCount > 0);
            D.assert(this.mainAxisSpacing >= 0.0f);
            D.assert(this.crossAxisSpacing >= 0.0f);
            D.assert(this.childAspectRatio > 0.0f);
            return true;
        }

        public override SliverGridLayout getLayout(SliverConstraints constraints) {
            D.assert(this._debugAssertIsValid());
            float usableCrossAxisExtent =
                constraints.crossAxisExtent - this.crossAxisSpacing * (this.crossAxisCount - 1);
            float childCrossAxisExtent = usableCrossAxisExtent / this.crossAxisCount;
            float childMainAxisExtent = childCrossAxisExtent / this.childAspectRatio;
            return new SliverGridRegularTileLayout(
                crossAxisCount: this.crossAxisCount,
                mainAxisStride: childMainAxisExtent + this.mainAxisSpacing,
                crossAxisStride: childCrossAxisExtent + this.crossAxisSpacing,
                childMainAxisExtent: childMainAxisExtent,
                childCrossAxisExtent: childCrossAxisExtent,
                reverseCrossAxis: AxisUtils.axisDirectionIsReversed(constraints.crossAxisDirection)
            );
        }

        public override bool shouldRelayout(SliverGridDelegate _oldDelegate) {
            SliverGridDelegateWithFixedCrossAxisCount oldDelegate =
                _oldDelegate as SliverGridDelegateWithFixedCrossAxisCount;
            return oldDelegate.crossAxisCount != this.crossAxisCount
                   || oldDelegate.mainAxisSpacing != this.mainAxisSpacing
                   || oldDelegate.crossAxisSpacing != this.crossAxisSpacing
                   || oldDelegate.childAspectRatio != this.childAspectRatio;
        }
    }

    public class SliverGridDelegateWithMaxCrossAxisExtent : SliverGridDelegate {
        public SliverGridDelegateWithMaxCrossAxisExtent(
            float maxCrossAxisExtent,
            float mainAxisSpacing = 0.0f,
            float crossAxisSpacing = 0.0f,
            float childAspectRatio = 1.0f
        ) {
            D.assert(maxCrossAxisExtent >= 0);
            D.assert(mainAxisSpacing >= 0);
            D.assert(crossAxisSpacing >= 0);
            D.assert(childAspectRatio > 0);
            this.maxCrossAxisExtent = maxCrossAxisExtent;
            this.mainAxisSpacing = mainAxisSpacing;
            this.crossAxisSpacing = crossAxisSpacing;
            this.childAspectRatio = childAspectRatio;
        }

        public readonly float maxCrossAxisExtent;

        public readonly float mainAxisSpacing;

        public readonly float crossAxisSpacing;

        public readonly float childAspectRatio;

        bool _debugAssertIsValid() {
            D.assert(this.maxCrossAxisExtent > 0.0f);
            D.assert(this.mainAxisSpacing >= 0.0f);
            D.assert(this.crossAxisSpacing >= 0.0f);
            D.assert(this.childAspectRatio > 0.0f);
            return true;
        }

        public override SliverGridLayout getLayout(SliverConstraints constraints) {
            D.assert(this._debugAssertIsValid());
            int crossAxisCount =
                (constraints.crossAxisExtent / (this.maxCrossAxisExtent + this.crossAxisSpacing)).ceil();
            float usableCrossAxisExtent = constraints.crossAxisExtent - this.crossAxisSpacing * (crossAxisCount - 1);
            float childCrossAxisExtent = usableCrossAxisExtent / crossAxisCount;
            float childMainAxisExtent = childCrossAxisExtent / this.childAspectRatio;
            return new SliverGridRegularTileLayout(
                crossAxisCount: crossAxisCount,
                mainAxisStride: childMainAxisExtent + this.mainAxisSpacing,
                crossAxisStride: childCrossAxisExtent + this.crossAxisSpacing,
                childMainAxisExtent: childMainAxisExtent,
                childCrossAxisExtent: childCrossAxisExtent,
                reverseCrossAxis: AxisUtils.axisDirectionIsReversed(constraints.crossAxisDirection)
            );
        }

        public override bool shouldRelayout(SliverGridDelegate _oldDelegate) {
            SliverGridDelegateWithMaxCrossAxisExtent oldDelegate =
                _oldDelegate as SliverGridDelegateWithMaxCrossAxisExtent;
            return oldDelegate.maxCrossAxisExtent != this.maxCrossAxisExtent
                   || oldDelegate.mainAxisSpacing != this.mainAxisSpacing
                   || oldDelegate.crossAxisSpacing != this.crossAxisSpacing
                   || oldDelegate.childAspectRatio != this.childAspectRatio;
        }
    }

    public class SliverGridParentData : SliverMultiBoxAdaptorParentData {
        public float crossAxisOffset;

        public override string ToString() {
            return "crossAxisOffset=$crossAxisOffset; ${base.ToString()}";
        }
    }

    public class RenderSliverGrid : RenderSliverMultiBoxAdaptor {
        public RenderSliverGrid(
            RenderSliverBoxChildManager childManager,
            SliverGridDelegate gridDelegate
        ) : base(childManager: childManager) {
            D.assert(gridDelegate != null);
            this._gridDelegate = gridDelegate;
        }


        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is SliverGridParentData)) {
                child.parentData = new SliverGridParentData();
            }
        }

        public SliverGridDelegate gridDelegate {
            get { return this._gridDelegate; }
            set {
                D.assert(value != null);
                if (this._gridDelegate == value) {
                    return;
                }

                if (value.GetType() != this._gridDelegate.GetType() ||
                    value.shouldRelayout(this._gridDelegate)) {
                    this.markNeedsLayout();
                }

                this._gridDelegate = value;
            }
        }

        SliverGridDelegate _gridDelegate;

        public override float childCrossAxisPosition(RenderObject child) {
            SliverGridParentData childParentData = (SliverGridParentData) child.parentData;
            return childParentData.crossAxisOffset;
        }

        protected override void performLayout() {
            this.childManager.didStartLayout();
            this.childManager.setDidUnderflow(false);

            float scrollOffset = this.constraints.scrollOffset + this.constraints.cacheOrigin;
            D.assert(scrollOffset >= 0.0f);
            float remainingExtent = this.constraints.remainingCacheExtent;
            D.assert(remainingExtent >= 0.0f);
            float targetEndScrollOffset = scrollOffset + remainingExtent;

            SliverGridLayout layout = this._gridDelegate.getLayout(this.constraints);

            int firstIndex = layout.getMinChildIndexForScrollOffset(scrollOffset);
            int? targetLastIndex = targetEndScrollOffset.isFinite()
                ? (int?) layout.getMaxChildIndexForScrollOffset(targetEndScrollOffset)
                : null;

            if (this.firstChild != null) {
                int oldFirstIndex = this.indexOf(this.firstChild);
                int oldLastIndex = this.indexOf(this.lastChild);
                int leadingGarbage = (firstIndex - oldFirstIndex).clamp(0, this.childCount);
                int trailingGarbage = targetLastIndex == null
                    ? 0
                    : ((oldLastIndex - targetLastIndex) ?? 0).clamp(0, this.childCount);
                this.collectGarbage(leadingGarbage, trailingGarbage);
            }
            else {
                this.collectGarbage(0, 0);
            }

            SliverGridGeometry firstChildGridGeometry = layout.getGeometryForChildIndex(firstIndex);
            float? leadingScrollOffset = firstChildGridGeometry.scrollOffset;
            float? trailingScrollOffset = firstChildGridGeometry.trailingScrollOffset;

            if (this.firstChild == null) {
                if (!this.addInitialChild(index: firstIndex,
                    layoutOffset: firstChildGridGeometry.scrollOffset ?? 0.0f)) {
                    float max = layout.computeMaxScrollOffset(this.childManager.childCount ?? 0);
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
                SliverGridGeometry gridGeometry = layout.getGeometryForChildIndex(index);
                RenderBox child = this.insertAndLayoutLeadingChild(
                    gridGeometry.getBoxConstraints(this.constraints)
                );
                SliverGridParentData childParentData = child.parentData as SliverGridParentData;
                childParentData.layoutOffset = gridGeometry.scrollOffset ?? 0.0f;
                childParentData.crossAxisOffset = gridGeometry.crossAxisOffset ?? 0.0f;
                D.assert(childParentData.index == index);
                trailingChildWithLayout = trailingChildWithLayout ?? child;
                trailingScrollOffset =
                    Mathf.Max(trailingScrollOffset ?? 0.0f, gridGeometry.trailingScrollOffset ?? 0.0f);
            }

            if (trailingChildWithLayout == null) {
                this.firstChild.layout(firstChildGridGeometry.getBoxConstraints(this.constraints));
                SliverGridParentData childParentData = this.firstChild.parentData as SliverGridParentData;
                childParentData.layoutOffset = firstChildGridGeometry.scrollOffset ?? 0.0f;
                childParentData.crossAxisOffset = firstChildGridGeometry.crossAxisOffset ?? 0.0f;
                trailingChildWithLayout = this.firstChild;
            }

            for (int index = this.indexOf(trailingChildWithLayout) + 1;
                targetLastIndex == null || index <= targetLastIndex;
                ++index) {
                SliverGridGeometry gridGeometry = layout.getGeometryForChildIndex(index);
                BoxConstraints childConstraints = gridGeometry.getBoxConstraints(this.constraints);
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
                SliverGridParentData childParentData = child.parentData as SliverGridParentData;
                childParentData.layoutOffset = gridGeometry.scrollOffset ?? 0.0f;
                childParentData.crossAxisOffset = gridGeometry.crossAxisOffset ?? 0.0f;
                D.assert(childParentData.index == index);
                trailingScrollOffset =
                    Mathf.Max(trailingScrollOffset ?? 0.0f, gridGeometry.trailingScrollOffset ?? 0.0f);
            }

            int lastIndex = this.indexOf(this.lastChild);

            D.assert(this.childScrollOffset(this.firstChild) <= scrollOffset);
            D.assert(this.debugAssertChildListIsNonEmptyAndContiguous());
            D.assert(this.indexOf(this.firstChild) == firstIndex);
            D.assert(targetLastIndex == null || lastIndex <= targetLastIndex);

            float estimatedTotalExtent = this.childManager.estimateMaxScrollOffset(this.constraints,
                firstIndex: firstIndex,
                lastIndex: lastIndex,
                leadingScrollOffset: leadingScrollOffset ?? 0.0f,
                trailingScrollOffset: trailingScrollOffset ?? 0.0f
            );

            float paintExtent = this.calculatePaintOffset(this.constraints,
                from: leadingScrollOffset ?? 0.0f,
                to: trailingScrollOffset ?? 0.0f
            );
            float cacheExtent = this.calculateCacheOffset(this.constraints,
                from: leadingScrollOffset ?? 0.0f,
                to: trailingScrollOffset ?? 0.0f
            );

            this.geometry = new SliverGeometry(
                scrollExtent: estimatedTotalExtent,
                paintExtent: paintExtent,
                maxPaintExtent: estimatedTotalExtent,
                cacheExtent: cacheExtent,
                hasVisualOverflow: true
            );

            if (estimatedTotalExtent == trailingScrollOffset) {
                this.childManager.setDidUnderflow(true);
            }

            this.childManager.didFinishLayout();
        }
    }
}