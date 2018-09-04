using System;
using UIWidgets.painting;
using UIWidgets.ui;
using UnityEngine;

namespace UIWidgets.rendering {
    public class RenderSliverPadding : RenderObjectWithChildMixinRenderSliver<RenderSliver> {
        RenderSliverPadding(
            EdgeInsets padding,
            RenderSliver child
        ) {
            this._padding = padding;
            this.child = child;
        }

        public EdgeInsets padding {
            get { return this._padding; }
            set {
                if (this._padding == value) {
                    return;
                }

                this._padding = value;
                this.markNeedsLayout();
            }
        }

        public EdgeInsets _padding;

        public double beforePadding {
            get {
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

                return 0.0;
            }
        }

        public double afterPadding {
            get {
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

                return 0.0;
            }
        }

        public double mainAxisPadding {
            get { return this._padding.along(this.constraints.axis); }
        }

        public double crossAxisPadding {
            get {
                switch (this.constraints.axis) {
                    case Axis.horizontal:
                        return this._padding.vertical;
                    case Axis.vertical:
                        return this._padding.horizontal;
                }

                return 0.0;
            }
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is SliverPhysicalParentData)) {
                child.parentData = new SliverPhysicalParentData();
            }
        }

        public override void performLayout() {
            double beforePadding = this.beforePadding;
            double afterPadding = this.afterPadding;
            double mainAxisPadding = this.mainAxisPadding;
            double crossAxisPadding = this.crossAxisPadding;
            if (this.child == null) {
                this.geometry = new SliverGeometry(
                    scrollExtent: mainAxisPadding,
                    paintExtent: Math.Min(mainAxisPadding, this.constraints.remainingPaintExtent),
                    maxPaintExtent: mainAxisPadding
                );
                return;
            }

            this.child.layout(
                this.constraints.copyWith(
                    scrollOffset: Math.Max(0.0, this.constraints.scrollOffset - beforePadding),
                    cacheOrigin: Math.Min(0.0, this.constraints.cacheOrigin + beforePadding),
                    overlap: 0.0,
                    remainingPaintExtent: this.constraints.remainingPaintExtent -
                                          this.calculatePaintOffset(this.constraints, from: 0.0, to: beforePadding),
                    remainingCacheExtent: this.constraints.remainingCacheExtent -
                                          this.calculateCacheOffset(this.constraints, from: 0.0, to: beforePadding),
                    crossAxisExtent: Math.Max(0.0, this.constraints.crossAxisExtent - crossAxisPadding)
                ),
                parentUsesSize: true
            );

            SliverGeometry childLayoutGeometry = this.child.geometry;
            if (childLayoutGeometry.scrollOffsetCorrection != 0.0) {
                this.geometry = new SliverGeometry(
                    scrollOffsetCorrection: childLayoutGeometry.scrollOffsetCorrection
                );
                return;
            }

            double beforePaddingPaintExtent = this.calculatePaintOffset(
                this.constraints,
                from: 0.0,
                to: beforePadding
            );

            double afterPaddingPaintExtent = this.calculatePaintOffset(
                this.constraints,
                from: beforePadding + childLayoutGeometry.scrollExtent,
                to: mainAxisPadding + childLayoutGeometry.scrollExtent
            );

            double mainAxisPaddingPaintExtent = beforePaddingPaintExtent + afterPaddingPaintExtent;
            double beforePaddingCacheExtent = this.calculateCacheOffset(
                this.constraints,
                from: 0.0,
                to: beforePadding
            );
            double afterPaddingCacheExtent = this.calculateCacheOffset(
                this.constraints,
                from: beforePadding + childLayoutGeometry.scrollExtent,
                to: mainAxisPadding + childLayoutGeometry.scrollExtent
            );

            double mainAxisPaddingCacheExtent = afterPaddingCacheExtent + beforePaddingCacheExtent;
            double paintExtent = Math.Min(
                beforePaddingPaintExtent + Math.Max(childLayoutGeometry.paintExtent,
                    childLayoutGeometry.layoutExtent + afterPaddingPaintExtent),
                this.constraints.remainingPaintExtent
            );

            this.geometry = new SliverGeometry(
                scrollExtent: mainAxisPadding + childLayoutGeometry.scrollExtent,
                paintExtent: paintExtent,
                layoutExtent: Math.Min(mainAxisPaddingPaintExtent + childLayoutGeometry.layoutExtent, paintExtent),
                cacheExtent: Math.Min(mainAxisPaddingCacheExtent + childLayoutGeometry.cacheExtent,
                    this.constraints.remainingCacheExtent),
                maxPaintExtent: mainAxisPadding + childLayoutGeometry.maxPaintExtent,
                hitTestExtent: Math.Max(
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
                        new Offset(this.calculatePaintOffset(this.constraints, from: 0.0, to: this._padding.left),
                            this._padding.top);
                    break;
                case AxisDirection.down:
                    childParentData.paintOffset = new Offset(this._padding.left,
                        this.calculatePaintOffset(this.constraints, from: 0.0, to: this._padding.top));
                    break;
                case AxisDirection.left:
                    childParentData.paintOffset = new Offset(
                        this.calculatePaintOffset(this.constraints,
                            from: this._padding.right + childLayoutGeometry.scrollExtent,
                            to: this._padding.right + childLayoutGeometry.scrollExtent + this._padding.left),
                        this._padding.top);
                    break;
            }
        }

        public override double childMainAxisPosition(RenderObject child) {
            return this.calculatePaintOffset(this.constraints, from: 0.0, to: this.beforePadding);
        }

        public override double childCrossAxisPosition(RenderObject child) {
            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(
                this.constraints.axisDirection, this.constraints.growthDirection)) {
                case AxisDirection.up:
                case AxisDirection.down:
                    return this._padding.left;
                case AxisDirection.left:
                case AxisDirection.right:
                    return this._padding.top;
            }

            return 0.0;
        }

        public override double childScrollOffset(RenderObject child) {
            return this.beforePadding;
        }

        public override void applyPaintTransform(RenderObject child, ref Matrix4x4 transform) {
            var childParentData = (SliverPhysicalParentData) child.parentData;
            childParentData.applyPaintTransform(ref transform);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null && this.child.geometry.visible) {
                var childParentData = (SliverPhysicalParentData) this.child.parentData;
                context.paintChild(this.child, offset + childParentData.paintOffset);
            }
        }
    }
}