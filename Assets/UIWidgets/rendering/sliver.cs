using System;
using UIWidgets.painting;
using UIWidgets.ui;
using UnityEngine;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.rendering {
    public enum GrowthDirection {
        forward,
        reverse,
    }

    public static class GrowthDirectionUtils {
        public static AxisDirection applyGrowthDirectionToAxisDirection(
            AxisDirection axisDirection, GrowthDirection growthDirection) {
            switch (growthDirection) {
                case GrowthDirection.forward:
                    return axisDirection;
                case GrowthDirection.reverse:
                    return AxisUtils.flipAxisDirection(axisDirection);
            }

            throw new Exception("unknown growthDirection");
        }

        public static ScrollDirection applyGrowthDirectionToScrollDirection(
            ScrollDirection scrollDirection, GrowthDirection growthDirection) {
            switch (growthDirection) {
                case GrowthDirection.forward:
                    return scrollDirection;
                case GrowthDirection.reverse:
                    return ScrollDirectionUtils.flipScrollDirection(scrollDirection);
            }

            throw new Exception("unknown growthDirection");
        }
    }

    public class SliverConstraints : Constraints, IEquatable<SliverConstraints> {
        public SliverConstraints(
            AxisDirection axisDirection,
            GrowthDirection growthDirection,
            ScrollDirection userScrollDirection,
            double scrollOffset,
            double overlap,
            double remainingPaintExtent,
            double crossAxisExtent,
            AxisDirection crossAxisDirection,
            double viewportMainAxisExtent,
            double remainingCacheExtent,
            double cacheOrigin
        ) {
            this.axisDirection = axisDirection;
            this.growthDirection = growthDirection;
            this.userScrollDirection = userScrollDirection;
            this.scrollOffset = scrollOffset;
            this.overlap = overlap;
            this.remainingPaintExtent = remainingPaintExtent;
            this.crossAxisExtent = crossAxisExtent;
            this.crossAxisDirection = crossAxisDirection;
            this.viewportMainAxisExtent = viewportMainAxisExtent;
            this.remainingCacheExtent = remainingCacheExtent;
            this.cacheOrigin = cacheOrigin;
        }

        public SliverConstraints copyWith(
            AxisDirection? axisDirection = null,
            GrowthDirection? growthDirection = null,
            ScrollDirection? userScrollDirection = null,
            double? scrollOffset = null,
            double? overlap = null,
            double? remainingPaintExtent = null,
            double? crossAxisExtent = null,
            AxisDirection? crossAxisDirection = null,
            double? viewportMainAxisExtent = null,
            double? remainingCacheExtent = null,
            double? cacheOrigin = null
        ) {
            return new SliverConstraints(
                axisDirection: axisDirection ?? this.axisDirection,
                growthDirection: growthDirection ?? this.growthDirection,
                userScrollDirection: userScrollDirection ?? this.userScrollDirection,
                scrollOffset: scrollOffset ?? this.scrollOffset,
                overlap: overlap ?? this.overlap,
                remainingPaintExtent: remainingPaintExtent ?? this.remainingPaintExtent,
                crossAxisExtent: crossAxisExtent ?? this.crossAxisExtent,
                crossAxisDirection: crossAxisDirection ?? this.crossAxisDirection,
                viewportMainAxisExtent: viewportMainAxisExtent ?? this.viewportMainAxisExtent,
                remainingCacheExtent: remainingCacheExtent ?? this.remainingCacheExtent,
                cacheOrigin: cacheOrigin ?? this.cacheOrigin
            );
        }

        public readonly AxisDirection axisDirection;

        public readonly GrowthDirection growthDirection;

        public readonly ScrollDirection userScrollDirection;

        public readonly double scrollOffset;

        public readonly double overlap;

        public readonly double remainingPaintExtent;

        public readonly double crossAxisExtent;

        public readonly AxisDirection crossAxisDirection;

        public readonly double viewportMainAxisExtent;

        public readonly double cacheOrigin;

        public readonly double remainingCacheExtent;

        public Axis axis {
            get { return AxisUtils.axisDirectionToAxis(this.axisDirection); }
        }

        public GrowthDirection normalizedGrowthDirection {
            get {
                switch (this.axisDirection) {
                    case AxisDirection.down:
                    case AxisDirection.right:
                        return this.growthDirection;
                    case AxisDirection.up:
                    case AxisDirection.left:
                        switch (this.growthDirection) {
                            case GrowthDirection.forward:
                                return GrowthDirection.reverse;
                            case GrowthDirection.reverse:
                                return GrowthDirection.forward;
                        }

                        throw new Exception("unknown growthDirection");
                }

                throw new Exception("unknown axisDirection");
            }
        }

        public override bool isTight {
            get { return false; }
        }

        public override bool isNormalized {
            get {
                return this.scrollOffset >= 0.0
                       && this.crossAxisExtent >= 0.0
                       && AxisUtils.axisDirectionToAxis(this.axisDirection) !=
                       AxisUtils.axisDirectionToAxis(this.crossAxisDirection)
                       && this.viewportMainAxisExtent >= 0.0
                       && this.remainingPaintExtent >= 0.0;
            }
        }

        public BoxConstraints asBoxConstraints(
            double minExtent = 0.0,
            double maxExtent = double.PositiveInfinity,
            double? crossAxisExtent = null
        ) {
            crossAxisExtent = crossAxisExtent ?? this.crossAxisExtent;
            switch (this.axis) {
                case Axis.horizontal:
                    return new BoxConstraints(
                        minHeight: crossAxisExtent.Value,
                        maxHeight: crossAxisExtent.Value,
                        minWidth: minExtent,
                        maxWidth: maxExtent
                    );
                case Axis.vertical:
                    return new BoxConstraints(
                        minWidth: crossAxisExtent.Value,
                        maxWidth: crossAxisExtent.Value,
                        minHeight: minExtent,
                        maxHeight: maxExtent
                    );
            }

            return null;
        }

        public bool Equals(SliverConstraints other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.axisDirection == other.axisDirection
                   && this.growthDirection == other.growthDirection
                   && this.userScrollDirection == other.userScrollDirection
                   && this.scrollOffset.Equals(other.scrollOffset)
                   && this.overlap.Equals(other.overlap)
                   && this.remainingPaintExtent.Equals(other.remainingPaintExtent)
                   && this.crossAxisExtent.Equals(other.crossAxisExtent)
                   && this.crossAxisDirection == other.crossAxisDirection
                   && this.viewportMainAxisExtent.Equals(other.viewportMainAxisExtent)
                   && this.cacheOrigin.Equals(other.cacheOrigin)
                   && this.remainingCacheExtent.Equals(other.remainingCacheExtent);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((SliverConstraints) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) this.axisDirection;
                hashCode = (hashCode * 397) ^ (int) this.growthDirection;
                hashCode = (hashCode * 397) ^ (int) this.userScrollDirection;
                hashCode = (hashCode * 397) ^ this.scrollOffset.GetHashCode();
                hashCode = (hashCode * 397) ^ this.overlap.GetHashCode();
                hashCode = (hashCode * 397) ^ this.remainingPaintExtent.GetHashCode();
                hashCode = (hashCode * 397) ^ this.crossAxisExtent.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) this.crossAxisDirection;
                hashCode = (hashCode * 397) ^ this.viewportMainAxisExtent.GetHashCode();
                hashCode = (hashCode * 397) ^ this.cacheOrigin.GetHashCode();
                hashCode = (hashCode * 397) ^ this.remainingCacheExtent.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SliverConstraints left, SliverConstraints right) {
            return Equals(left, right);
        }

        public static bool operator !=(SliverConstraints left, SliverConstraints right) {
            return !Equals(left, right);
        }
    }

    public class SliverGeometry {
        public SliverGeometry(
            double scrollExtent = 0.0,
            double paintExtent = 0.0,
            double paintOrigin = 0.0,
            double? layoutExtent = null,
            double maxPaintExtent = 0.0,
            double maxScrollObstructionExtent = 0.0,
            double? hitTestExtent = null,
            bool? visible = null,
            bool hasVisualOverflow = false,
            double scrollOffsetCorrection = 0.0,
            double? cacheExtent = null
        ) {
            this.scrollExtent = scrollExtent;
            this.paintExtent = paintExtent;
            this.paintOrigin = paintOrigin;
            this.layoutExtent = layoutExtent ?? paintExtent;
            this.maxPaintExtent = maxPaintExtent;
            this.maxScrollObstructionExtent = maxScrollObstructionExtent;
            this.hitTestExtent = hitTestExtent ?? paintExtent;
            this.visible = visible ?? paintExtent > 0.0;
            this.hasVisualOverflow = hasVisualOverflow;
            this.scrollOffsetCorrection = scrollOffsetCorrection;
            this.cacheExtent = cacheExtent ?? layoutExtent ?? paintExtent;
        }

        public static readonly SliverGeometry zero = new SliverGeometry();

        public readonly double scrollExtent;
        public readonly double paintOrigin;
        public readonly double paintExtent;
        public readonly double layoutExtent;
        public readonly double maxPaintExtent;
        public readonly double maxScrollObstructionExtent;
        public readonly double hitTestExtent;
        public readonly bool visible;
        public readonly bool hasVisualOverflow;
        public readonly double scrollOffsetCorrection;
        public readonly double cacheExtent;
    }

    public class SliverPhysicalParentData : ParentData {
        public Offset paintOffset = Offset.zero;

        public void applyPaintTransform(ref Matrix4x4 transform) {
            transform = Matrix4x4.Translate(this.paintOffset.toVector()) * transform;
        }
    }

    public class SliverPhysicalContainerParentData : ContainerParentDataMixinSliverPhysicalParentData<RenderSliver> {
    }

    public class SliverLogicalParentData : ParentData {
        public double layoutOffset = 0.0;
    }

    public class SliverLogicalContainerParentData : ContainerParentDataMixinSliverLogicalParentData<RenderSliver> {
    }


    public abstract class RenderSliver : RenderObject {
        public new SliverConstraints constraints {
            get { return (SliverConstraints) base.constraints; }
        }

        public SliverGeometry geometry {
            get { return this._geometry; }
            set { this._geometry = value; }
        }

        public SliverGeometry _geometry;

        public override Rect paintBounds {
            get {
                switch (this.constraints.axis) {
                    case Axis.horizontal:
                        return Rect.fromLTWH(
                            0.0, 0.0,
                            this.geometry.paintExtent,
                            this.constraints.crossAxisExtent
                        );
                    case Axis.vertical:
                        return Rect.fromLTWH(
                            0.0, 0.0,
                            this.constraints.crossAxisExtent,
                            this.geometry.paintExtent
                        );
                }

                return null;
            }
        }

        public override void performResize() {
        }

        public double centerOffsetAdjustment {
            get { return 0.0; }
        }

        public double calculatePaintOffset(SliverConstraints constraints, double from, double to) {
            double a = constraints.scrollOffset;
            double b = constraints.scrollOffset + constraints.remainingPaintExtent;
            return (to.clamp(a, b) - from.clamp(a, b)).clamp(0.0, constraints.remainingPaintExtent);
        }

        public double calculateCacheOffset(SliverConstraints constraints, double from, double to) {
            double a = constraints.scrollOffset + constraints.cacheOrigin;
            double b = constraints.scrollOffset + constraints.remainingCacheExtent;
            return (to.clamp(a, b) - from.clamp(a, b)).clamp(0.0, constraints.remainingCacheExtent);
        }

        public virtual double childMainAxisPosition(RenderObject child) {
            return 0.0;
        }

        public virtual double childCrossAxisPosition(RenderObject child) {
            return 0.0;
        }

        public virtual double childScrollOffset(RenderObject child) {
            return 0.0;
        }

        public override void applyPaintTransform(RenderObject child, ref Matrix4x4 transform) {
        }

        public Size getAbsoluteSizeRelativeToOrigin() {
            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(
                this.constraints.axisDirection, this.constraints.growthDirection)) {
                case AxisDirection.up:
                    return new Size(this.constraints.crossAxisExtent, -this.geometry.paintExtent);
                case AxisDirection.right:
                    return new Size(this.geometry.paintExtent, this.constraints.crossAxisExtent);
                case AxisDirection.down:
                    return new Size(this.constraints.crossAxisExtent, this.geometry.paintExtent);
                case AxisDirection.left:
                    return new Size(-this.geometry.paintExtent, this.constraints.crossAxisExtent);
            }

            return null;
        }
    }

    public static class RenderSliverHelpers {
        public static bool _getRightWayUp(SliverConstraints constraints) {
            bool rightWayUp = true;
            switch (constraints.axisDirection) {
                case AxisDirection.up:
                case AxisDirection.left:
                    rightWayUp = false;
                    break;
                case AxisDirection.down:
                case AxisDirection.right:
                    rightWayUp = true;
                    break;
            }

            switch (constraints.growthDirection) {
                case GrowthDirection.forward:
                    break;
                case GrowthDirection.reverse:
                    rightWayUp = !rightWayUp;
                    break;
            }

            return rightWayUp;
        }

        public static void applyPaintTransformForBoxChild(this RenderSliver it, RenderBox child,
            ref Matrix4x4 transform) {
            bool rightWayUp = RenderSliverHelpers._getRightWayUp(it.constraints);
            double delta = it.childMainAxisPosition(child);
            double crossAxisDelta = it.childCrossAxisPosition(child);
            switch (it.constraints.axis) {
                case Axis.horizontal:
                    if (!rightWayUp) {
                        delta = it.geometry.paintExtent - child.size.width - delta;
                    }

                    transform = Matrix4x4.Translate(new Vector2((float) delta, (float) crossAxisDelta)) * transform;
                    break;
                case Axis.vertical:
                    if (!rightWayUp) {
                        delta = it.geometry.paintExtent - child.size.height - delta;
                    }

                    transform = Matrix4x4.Translate(new Vector2((float) crossAxisDelta, (float) delta)) * transform;
                    break;
            }
        }
    }

    public abstract class RenderSliverSingleBoxAdapter : RenderObjectWithChildMixinRenderSliver<RenderBox> {
        public RenderSliverSingleBoxAdapter(
            RenderBox child = null
        ) {
            this.child = child;
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is SliverPhysicalParentData)) {
                child.parentData = new SliverPhysicalParentData();
            }
        }

        public void setChildParentData(RenderObject child, SliverConstraints constraints, SliverGeometry geometry) {
            var childParentData = (SliverPhysicalParentData) child.parentData;
            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(constraints.axisDirection,
                constraints.growthDirection)) {
                case AxisDirection.up:
                    childParentData.paintOffset = new Offset(0.0,
                        -(geometry.scrollExtent - (geometry.paintExtent + constraints.scrollOffset)));
                    break;
                case AxisDirection.right:
                    childParentData.paintOffset = new Offset(-constraints.scrollOffset, 0.0);
                    break;
                case AxisDirection.down:
                    childParentData.paintOffset = new Offset(0.0, -constraints.scrollOffset);
                    break;
                case AxisDirection.left:
                    childParentData.paintOffset =
                        new Offset(-(geometry.scrollExtent - (geometry.paintExtent + constraints.scrollOffset)), 0.0);
                    break;
            }
        }

        public override double childMainAxisPosition(RenderObject child) {
            return -this.constraints.scrollOffset;
        }

        public override void applyPaintTransform(RenderObject child, ref Matrix4x4 transform) {
            var childParentData = (SliverPhysicalParentData) child.parentData;
            childParentData.applyPaintTransform(ref transform);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null && this.geometry.visible) {
                var childParentData = (SliverPhysicalParentData) this.child.parentData;
                context.paintChild(this.child, offset + childParentData.paintOffset);
            }
        }
    }

    public class RenderSliverToBoxAdapter : RenderSliverSingleBoxAdapter {
        public RenderSliverToBoxAdapter(
            RenderBox child = null
        ) : base(child) {
        }

        public override void performLayout() {
            if (this.child == null) {
                this.geometry = SliverGeometry.zero;
                return;
            }

            this.child.layout(this.constraints.asBoxConstraints(), parentUsesSize: true);

            double childExtent = 0.0;
            switch (this.constraints.axis) {
                case Axis.horizontal:
                    childExtent = this.child.size.width;
                    break;
                case Axis.vertical:
                    childExtent = this.child.size.height;
                    break;
            }

            double paintedChildSize = this.calculatePaintOffset(this.constraints, from: 0.0, to: childExtent);
            double cacheExtent = this.calculateCacheOffset(this.constraints, from: 0.0, to: childExtent);

            this.geometry = new SliverGeometry(
                scrollExtent: childExtent,
                paintExtent: paintedChildSize,
                cacheExtent: cacheExtent,
                maxPaintExtent: childExtent,
                hitTestExtent: paintedChildSize,
                hasVisualOverflow: childExtent > this.constraints.remainingPaintExtent
                                   || this.constraints.scrollOffset > 0.0
            );

            this.setChildParentData(this.child, this.constraints, this.geometry);
        }
    }
}