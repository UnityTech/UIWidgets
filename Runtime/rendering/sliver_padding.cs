using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public class RenderSliverPadding : RenderObjectWithChildMixinRenderSliver<RenderSliver> {
        public RenderSliverPadding(
            EdgeInsets padding = null,
            RenderSliver child = null
        ) {
            D.assert(padding != null);
            D.assert(padding.isNonNegative);

            this._padding = padding;
            this.child = child;
        }

        public EdgeInsets padding {
            get { return this._padding; }
            set {
                D.assert(value != null);
                D.assert(value.isNonNegative);
                if (this._padding == value) {
                    return;
                }

                this._padding = value;
                this.markNeedsLayout();
            }
        }

        EdgeInsets _padding;

        public float beforePadding {
            get {
                D.assert(this.constraints != null);

                switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(
                    this.constraints.axisDirection, this.constraints.growthDirection)) {
                    case AxisDirection.up:
                        return this._padding.bottom;
                    case AxisDirection.right:
                        return this._padding.left;
                    case AxisDirection.down:
                        return this._padding.top;
                    case AxisDirection.left:
                        return this._padding.right;
                }

                return 0.0f;
            }
        }

        public float afterPadding {
            get {
                D.assert(this.constraints != null);

                switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(
                    this.constraints.axisDirection, this.constraints.growthDirection)) {
                    case AxisDirection.up:
                        return this._padding.top;
                    case AxisDirection.right:
                        return this._padding.right;
                    case AxisDirection.down:
                        return this._padding.bottom;
                    case AxisDirection.left:
                        return this._padding.left;
                }

                return 0.0f;
            }
        }

        public float mainAxisPadding {
            get {
                D.assert(this.constraints != null);

                return this._padding.along(this.constraints.axis);
            }
        }

        public float crossAxisPadding {
            get {
                D.assert(this.constraints != null);

                switch (this.constraints.axis) {
                    case Axis.horizontal:
                        return this._padding.vertical;
                    case Axis.vertical:
                        return this._padding.horizontal;
                }

                D.assert(false);
                return 0.0f;
            }
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is SliverPhysicalParentData)) {
                child.parentData = new SliverPhysicalParentData();
            }
        }

        protected override void performLayout() {
            float beforePadding = this.beforePadding;
            float afterPadding = this.afterPadding;
            float mainAxisPadding = this.mainAxisPadding;
            float crossAxisPadding = this.crossAxisPadding;
            if (this.child == null) {
                this.geometry = new SliverGeometry(
                    scrollExtent: mainAxisPadding,
                    paintExtent: Mathf.Min(mainAxisPadding, this.constraints.remainingPaintExtent),
                    maxPaintExtent: mainAxisPadding
                );
                return;
            }

            this.child.layout(
                this.constraints.copyWith(
                    scrollOffset: Mathf.Max(0.0f, this.constraints.scrollOffset - beforePadding),
                    cacheOrigin: Mathf.Min(0.0f, this.constraints.cacheOrigin + beforePadding),
                    overlap: 0.0f,
                    remainingPaintExtent: this.constraints.remainingPaintExtent -
                                          this.calculatePaintOffset(this.constraints, from: 0.0f, to: beforePadding),
                    remainingCacheExtent: this.constraints.remainingCacheExtent -
                                          this.calculateCacheOffset(this.constraints, from: 0.0f, to: beforePadding),
                    crossAxisExtent: Mathf.Max(0.0f, this.constraints.crossAxisExtent - crossAxisPadding)
                ),
                parentUsesSize: true
            );

            SliverGeometry childLayoutGeometry = this.child.geometry;
            if (childLayoutGeometry.scrollOffsetCorrection != null) {
                this.geometry = new SliverGeometry(
                    scrollOffsetCorrection: childLayoutGeometry.scrollOffsetCorrection
                );
                return;
            }

            float beforePaddingPaintExtent = this.calculatePaintOffset(
                this.constraints,
                from: 0.0f,
                to: beforePadding
            );

            float afterPaddingPaintExtent = this.calculatePaintOffset(
                this.constraints,
                from: beforePadding + childLayoutGeometry.scrollExtent,
                to: mainAxisPadding + childLayoutGeometry.scrollExtent
            );

            float mainAxisPaddingPaintExtent = beforePaddingPaintExtent + afterPaddingPaintExtent;
            float beforePaddingCacheExtent = this.calculateCacheOffset(
                this.constraints,
                from: 0.0f,
                to: beforePadding
            );
            float afterPaddingCacheExtent = this.calculateCacheOffset(
                this.constraints,
                from: beforePadding + childLayoutGeometry.scrollExtent,
                to: mainAxisPadding + childLayoutGeometry.scrollExtent
            );

            float mainAxisPaddingCacheExtent = afterPaddingCacheExtent + beforePaddingCacheExtent;
            float paintExtent = Mathf.Min(
                beforePaddingPaintExtent + Mathf.Max(childLayoutGeometry.paintExtent,
                    childLayoutGeometry.layoutExtent + afterPaddingPaintExtent),
                this.constraints.remainingPaintExtent
            );

            this.geometry = new SliverGeometry(
                scrollExtent: mainAxisPadding + childLayoutGeometry.scrollExtent,
                paintExtent: paintExtent,
                layoutExtent: Mathf.Min(mainAxisPaddingPaintExtent + childLayoutGeometry.layoutExtent,
                    paintExtent),
                cacheExtent: Mathf.Min(mainAxisPaddingCacheExtent + childLayoutGeometry.cacheExtent,
                    this.constraints.remainingCacheExtent),
                maxPaintExtent: mainAxisPadding + childLayoutGeometry.maxPaintExtent,
                hitTestExtent: Mathf.Max(
                    mainAxisPaddingPaintExtent + childLayoutGeometry.paintExtent,
                    beforePaddingPaintExtent + childLayoutGeometry.hitTestExtent
                ),
                hasVisualOverflow: childLayoutGeometry.hasVisualOverflow
            );

            var childParentData = (SliverPhysicalParentData) this.child.parentData;
            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(this.constraints.axisDirection,
                this.constraints.growthDirection)) {
                case AxisDirection.up:
                    childParentData.paintOffset = new Offset(this._padding.left,
                        this.calculatePaintOffset(this.constraints,
                            from: this._padding.bottom + childLayoutGeometry.scrollExtent,
                            to: this._padding.bottom + childLayoutGeometry.scrollExtent + this._padding.top));
                    break;
                case AxisDirection.right:
                    childParentData.paintOffset =
                        new Offset(this.calculatePaintOffset(this.constraints, from: 0.0f, to: this._padding.left),
                            this._padding.top);
                    break;
                case AxisDirection.down:
                    childParentData.paintOffset = new Offset(this._padding.left,
                        this.calculatePaintOffset(this.constraints, from: 0.0f, to: this._padding.top));
                    break;
                case AxisDirection.left:
                    childParentData.paintOffset = new Offset(
                        this.calculatePaintOffset(this.constraints,
                            from: this._padding.right + childLayoutGeometry.scrollExtent,
                            to: this._padding.right + childLayoutGeometry.scrollExtent + this._padding.left),
                        this._padding.top);
                    break;
            }

            D.assert(childParentData.paintOffset != null);
            D.assert(beforePadding == this.beforePadding);
            D.assert(afterPadding == this.afterPadding);
            D.assert(mainAxisPadding == this.mainAxisPadding);
            D.assert(crossAxisPadding == this.crossAxisPadding);
        }

        protected override bool hitTestChildren(HitTestResult result, float mainAxisPosition = 0.0f,
            float crossAxisPosition = 0.0f) {
            if (this.child != null && this.child.geometry.hitTestExtent > 0.0) {
                return this.child.hitTest(result,
                    mainAxisPosition: mainAxisPosition - this.childMainAxisPosition(this.child),
                    crossAxisPosition: crossAxisPosition - this.childCrossAxisPosition(this.child));
            }

            return false;
        }

        public override float childMainAxisPosition(RenderObject child) {
            D.assert(child != null);
            D.assert(child == this.child);

            return this.calculatePaintOffset(this.constraints, from: 0.0f, to: this.beforePadding);
        }

        public override float childCrossAxisPosition(RenderObject child) {
            D.assert(child != null);
            D.assert(child == this.child);
            D.assert(this.constraints != null);

            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(
                this.constraints.axisDirection, this.constraints.growthDirection)) {
                case AxisDirection.up:
                case AxisDirection.down:
                    return this._padding.left;
                case AxisDirection.left:
                case AxisDirection.right:
                    return this._padding.top;
            }

            return 0.0f;
        }

        public override float childScrollOffset(RenderObject child) {
            D.assert(child.parent == this);

            return this.beforePadding;
        }

        public override void applyPaintTransform(RenderObject child, Matrix3 transform) {
            D.assert(child != null);
            D.assert(child == this.child);

            var childParentData = (SliverPhysicalParentData) child.parentData;
            childParentData.applyPaintTransform(transform);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null && this.child.geometry.visible) {
                var childParentData = (SliverPhysicalParentData) this.child.parentData;
                context.paintChild(this.child, offset + childParentData.paintOffset);
            }
        }

        public override void debugPaint(PaintingContext context, Offset offset) {
            base.debugPaint(context, offset);
            D.assert(() => {
                if (D.debugPaintSizeEnabled) {
                    Size parentSize = this.getAbsoluteSizeRelativeToOrigin();
                    Rect outerRect = offset & parentSize;
                    Size childSize = null;
                    Rect innerRect = null;
                    if (this.child != null) {
                        childSize = this.child.getAbsoluteSizeRelativeToOrigin();
                        var childParentData = (SliverPhysicalParentData) this.child.parentData;
                        innerRect = (offset + childParentData.paintOffset) & childSize;
                        D.assert(innerRect.top >= outerRect.top);
                        D.assert(innerRect.left >= outerRect.left);
                        D.assert(innerRect.right <= outerRect.right);
                        D.assert(innerRect.bottom <= outerRect.bottom);
                    }

                    D.debugPaintPadding(context.canvas, outerRect, innerRect);
                }

                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding", this.padding));
        }
    }
}