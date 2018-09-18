using System;
using System.Collections.Generic;
using UIWidgets.painting;
using UIWidgets.ui;
using UnityEngine;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.rendering {
    public interface RenderAbstractViewport {
        RevealedOffset getOffsetToReveal(RenderObject target, double alignment, Rect rect = null);
    }

    public static class RenderAbstractViewportUtils {
        public static RenderAbstractViewport of(RenderObject obj) {
            while (obj != null) {
                if (obj is RenderAbstractViewport) {
                    return (RenderAbstractViewport) obj;
                }

                obj = (RenderObject) obj.parent;
            }

            return null;
        }

        public const double defaultCacheExtent = 250.0;
    }

    public class RevealedOffset {
        public RevealedOffset(double offset, Rect rect) {
            this.offset = offset;
            this.rect = rect;
        }

        public readonly double offset;
        public readonly Rect rect;
    }

    public abstract class RenderViewportBase<ParentDataClass> :
        ContainerRenderObjectMixinRenderBox<RenderSliver, ParentDataClass>,
        RenderAbstractViewport
        where ParentDataClass : ParentData, ContainerParentDataMixin<RenderSliver> {
        protected RenderViewportBase(
            AxisDirection axisDirection = AxisDirection.down,
            AxisDirection crossAxisDirection = AxisDirection.right,
            ViewportOffset offset = null,
            double? cacheExtent = null
        ) {
            this._axisDirection = axisDirection;
            this._crossAxisDirection = crossAxisDirection;
            this._offset = offset;
            this._cacheExtent = cacheExtent ?? RenderAbstractViewportUtils.defaultCacheExtent;
        }

        public AxisDirection axisDirection {
            get { return this._axisDirection; }
            set {
                if (value == this._axisDirection) {
                    return;
                }

                this._axisDirection = value;
                this.markNeedsLayout();
            }
        }

        public AxisDirection _axisDirection;

        public AxisDirection crossAxisDirection {
            get { return this._crossAxisDirection; }
            set {
                if (value == this._crossAxisDirection) {
                    return;
                }

                this._crossAxisDirection = value;
                this.markNeedsLayout();
            }
        }

        public AxisDirection _crossAxisDirection;

        public Axis axis {
            get { return AxisUtils.axisDirectionToAxis(this.axisDirection); }
        }

        public ViewportOffset offset {
            get { return this._offset; }
            set {
                if (object.Equals(value, this._offset)) {
                    return;
                }

                if (this.attached) {
                    this._offset.removeListener(this.markNeedsLayout);
                }

                this._offset = value;
                if (this.attached) {
                    this._offset.addListener(this.markNeedsLayout);
                }

                this.markNeedsLayout();
            }
        }

        public ViewportOffset _offset;

        public double cacheExtent {
            get { return this._cacheExtent; }
            set {
                if (value == this._cacheExtent) {
                    return;
                }

                this._cacheExtent = value;
                this.markNeedsLayout();
            }
        }

        public double _cacheExtent;

        public override void attach(object owner) {
            base.attach(owner);
            this._offset.addListener(this.markNeedsLayout);
        }

        public override void detach() {
            this._offset.removeListener(this.markNeedsLayout);
            base.detach();
        }

        public override double computeMinIntrinsicWidth(double height) {
            return 0.0;
        }

        public override double computeMaxIntrinsicWidth(double height) {
            return 0.0;
        }

        public override double computeMinIntrinsicHeight(double width) {
            return 0.0;
        }

        public override double computeMaxIntrinsicHeight(double width) {
            return 0.0;
        }

        public override bool isRepaintBoundary {
            get { return true; }
        }

        public double layoutChildSequence(
            RenderSliver child,
            double scrollOffset,
            double overlap,
            double layoutOffset,
            double remainingPaintExtent,
            double mainAxisExtent,
            double crossAxisExtent,
            GrowthDirection growthDirection,
            Func<RenderSliver, RenderSliver> advance,
            double remainingCacheExtent,
            double cacheOrigin
        ) {
            double initialLayoutOffset = layoutOffset;

            ScrollDirection adjustedUserScrollDirection =
                GrowthDirectionUtils.applyGrowthDirectionToScrollDirection(
                    this.offset.userScrollDirection, growthDirection);

            double maxPaintOffset = layoutOffset + overlap;

            while (child != null) {
                double sliverScrollOffset = scrollOffset <= 0.0 ? 0.0 : scrollOffset;
                double correctedCacheOrigin = Math.Max(cacheOrigin, -sliverScrollOffset);
                double cacheExtentCorrection = cacheOrigin - correctedCacheOrigin;

                child.layout(new SliverConstraints(
                    axisDirection: this.axisDirection,
                    growthDirection: growthDirection,
                    userScrollDirection: adjustedUserScrollDirection,
                    scrollOffset: sliverScrollOffset,
                    overlap: maxPaintOffset - layoutOffset,
                    remainingPaintExtent: Math.Max(0.0, remainingPaintExtent - layoutOffset + initialLayoutOffset),
                    crossAxisExtent: crossAxisExtent,
                    crossAxisDirection: this.crossAxisDirection,
                    viewportMainAxisExtent: mainAxisExtent,
                    remainingCacheExtent: Math.Max(0.0, remainingCacheExtent + cacheExtentCorrection),
                    cacheOrigin: correctedCacheOrigin
                ), parentUsesSize: true);

                var childLayoutGeometry = child.geometry;

                if (childLayoutGeometry.scrollOffsetCorrection != 0.0) {
                    return childLayoutGeometry.scrollOffsetCorrection;
                }

                double effectiveLayoutOffset = layoutOffset + childLayoutGeometry.paintOrigin;

                if (childLayoutGeometry.visible || scrollOffset > 0) {
                    this.updateChildLayoutOffset(child, effectiveLayoutOffset, growthDirection);
                } else {
                    this.updateChildLayoutOffset(child, -scrollOffset + initialLayoutOffset, growthDirection);
                }

                maxPaintOffset = Math.Max(effectiveLayoutOffset + childLayoutGeometry.paintExtent, maxPaintOffset);
                scrollOffset -= childLayoutGeometry.scrollExtent;
                layoutOffset += childLayoutGeometry.layoutExtent;

                if (childLayoutGeometry.cacheExtent != 0.0) {
                    remainingCacheExtent -= childLayoutGeometry.cacheExtent - cacheExtentCorrection;
                    cacheOrigin = Math.Min(correctedCacheOrigin + childLayoutGeometry.cacheExtent, 0.0);
                }

                this.updateOutOfBandData(growthDirection, childLayoutGeometry);

                child = advance(child);
            }

            return 0.0;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.firstChild == null) {
                return;
            }

            if (this.hasVisualOverflow) {
                context.pushClipRect(this.needsCompositing, offset, Offset.zero & this.size, this._paintContents);
            } else {
                this._paintContents(context, offset);
            }
        }

        public void _paintContents(PaintingContext context, Offset offset) {
            foreach (RenderSliver child in this.childrenInPaintOrder) {
                if (child.geometry.visible) {
                    context.paintChild(child, offset + this.paintOffsetOf(child));
                }
            }
        }

        public RevealedOffset getOffsetToReveal(RenderObject target, double alignment, Rect rect = null) {
            double leadingScrollOffset = 0.0;
            double targetMainAxisExtent = 0.0;
            RenderObject descendant;
            rect = rect ?? target.paintBounds;

            Matrix4x4 transform;

            if (target is RenderBox) {
                RenderBox targetBox = (RenderBox) target;

                RenderBox pivot = targetBox;
                while (pivot.parent is RenderBox) {
                    pivot = (RenderBox) pivot.parent;
                }

                RenderSliver pivotParent = (RenderSliver) pivot.parent;

                transform = targetBox.getTransformTo(pivot);
                Rect bounds = MatrixUtils.transformRect(transform, rect);

                double offset = 0.0;

                GrowthDirection growthDirection = pivotParent.constraints.growthDirection;
                switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(this.axisDirection, growthDirection)) {
                    case AxisDirection.up:
                        switch (growthDirection) {
                            case GrowthDirection.forward:
                                offset = bounds.bottom;
                                break;
                            case GrowthDirection.reverse:
                                offset = bounds.top;
                                break;
                        }

                        leadingScrollOffset = pivot.size.height - offset;
                        targetMainAxisExtent = bounds.height;
                        break;
                    case AxisDirection.right:
                        leadingScrollOffset = bounds.left;
                        targetMainAxisExtent = bounds.width;
                        break;
                    case AxisDirection.down:
                        leadingScrollOffset = bounds.top;
                        targetMainAxisExtent = bounds.height;
                        break;
                    case AxisDirection.left:
                        switch (growthDirection) {
                            case GrowthDirection.forward:
                                offset = bounds.right;
                                break;
                            case GrowthDirection.reverse:
                                offset = bounds.left;
                                break;
                        }

                        leadingScrollOffset = pivot.size.width - offset;
                        targetMainAxisExtent = bounds.width;
                        break;
                }

                descendant = pivot;
            } else if (target is RenderSliver) {
                RenderSliver targetSliver = (RenderSliver) target;
                leadingScrollOffset = 0.0;
                targetMainAxisExtent = targetSliver.geometry.scrollExtent;
                descendant = targetSliver;
            } else {
                return new RevealedOffset(offset: this.offset.pixels, rect: rect);
            }

            RenderObject child = descendant;
            while (child.parent is RenderSliver) {
                var parent = (RenderSliver) child.parent;
                leadingScrollOffset += parent.childScrollOffset(child);
                child = parent;
            }

            RenderSliver sliver = (RenderSliver) child;
            double extentOfPinnedSlivers = this.maxScrollObstructionExtentBefore(sliver);
            leadingScrollOffset = this.scrollOffsetOf(sliver, leadingScrollOffset);
            switch (sliver.constraints.growthDirection) {
                case GrowthDirection.forward:
                    leadingScrollOffset -= extentOfPinnedSlivers;
                    break;
                case GrowthDirection.reverse:
                    break;
            }

            double mainAxisExtent = 0.0;
            switch (this.axis) {
                case Axis.horizontal:
                    mainAxisExtent = this.size.width - extentOfPinnedSlivers;
                    break;
                case Axis.vertical:
                    mainAxisExtent = this.size.height - extentOfPinnedSlivers;
                    break;
            }

            double targetOffset = leadingScrollOffset - (mainAxisExtent - targetMainAxisExtent) * alignment;
            double offsetDifference = this.offset.pixels - targetOffset;

            transform = target.getTransformTo(this);
            this.applyPaintTransform(child, ref transform);
            Rect targetRect = MatrixUtils.transformRect(transform, rect);

            switch (this.axisDirection) {
                case AxisDirection.down:
                    targetRect = targetRect.translate(0.0, offsetDifference);
                    break;
                case AxisDirection.right:
                    targetRect = targetRect.translate(offsetDifference, 0.0);
                    break;
                case AxisDirection.up:
                    targetRect = targetRect.translate(0.0, -offsetDifference);
                    break;
                case AxisDirection.left:
                    targetRect = targetRect.translate(-offsetDifference, 0.0);
                    break;
            }

            return new RevealedOffset(offset: targetOffset, rect: targetRect);
        }

        public Offset computeAbsolutePaintOffset(RenderSliver child, double layoutOffset,
            GrowthDirection growthDirection) {
            switch (GrowthDirectionUtils.applyGrowthDirectionToAxisDirection(this.axisDirection, growthDirection)) {
                case AxisDirection.up:
                    return new Offset(0.0, this.size.height - (layoutOffset + child.geometry.paintExtent));
                case AxisDirection.right:
                    return new Offset(layoutOffset, 0.0);
                case AxisDirection.down:
                    return new Offset(0.0, layoutOffset);
                case AxisDirection.left:
                    return new Offset(this.size.width - (layoutOffset + child.geometry.paintExtent), 0.0);
            }

            return null;
        }


        public abstract bool hasVisualOverflow { get; }

        public abstract void updateOutOfBandData(GrowthDirection growthDirection, SliverGeometry childLayoutGeometry);

        public abstract void updateChildLayoutOffset(RenderSliver child, double layoutOffset,
            GrowthDirection growthDirection);

        public abstract Offset paintOffsetOf(RenderSliver child);

        public abstract double scrollOffsetOf(RenderSliver child, double scrollOffsetWithinChild);

        public abstract double maxScrollObstructionExtentBefore(RenderSliver child);

        public abstract IEnumerable<RenderSliver> childrenInPaintOrder { get; }
    }


    public class RenderViewport : RenderViewportBase<SliverPhysicalContainerParentData> {
        public RenderViewport(
            AxisDirection axisDirection = AxisDirection.down,
            AxisDirection crossAxisDirection = AxisDirection.right,
            ViewportOffset offset = null,
            double anchor = 0.0,
            List<RenderSliver> children = null,
            RenderSliver center = null,
            double? cacheExtent = null
        ) : base(axisDirection, crossAxisDirection, offset, cacheExtent) {
            this.addAll(children);
            if (center == null && this.firstChild != null) {
                this._center = this.firstChild;
            }
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is SliverPhysicalContainerParentData)) {
                child.parentData = new SliverPhysicalContainerParentData();
            }
        }

        public double anchor {
            get { return this._anchor; }
            set {
                if (value != this._anchor) {
                    return;
                }

                this._anchor = value;
                this.markNeedsLayout();
            }
        }

        public double _anchor;

        public RenderSliver center {
            get { return this._center; }
            set {
                if (value == this._center) {
                    return;
                }

                this._center = value;
                this.markNeedsLayout();
            }
        }

        public RenderSliver _center;

        public override bool sizedByParent {
            get { return true; }
        }

        public override void performResize() {
            this.size = this.constraints.biggest;

            switch (this.axis) {
                case Axis.vertical:
                    this.offset.applyViewportDimension(this.size.height);
                    break;
                case Axis.horizontal:
                    this.offset.applyViewportDimension(this.size.width);
                    break;
            }
        }

        public const int _maxLayoutCycles = 10;

        public double _minScrollExtent;
        public double _maxScrollExtent;
        public bool _hasVisualOverflow = false;

        public override void performLayout() {
            if (this.center == null) {
                this._minScrollExtent = 0.0;
                this._maxScrollExtent = 0.0;
                this._hasVisualOverflow = false;
                this.offset.applyContentDimensions(0.0, 0.0);
                return;
            }

            double mainAxisExtent = 0.0;
            double crossAxisExtent = 0.0;
            switch (this.axis) {
                case Axis.vertical:
                    mainAxisExtent = this.size.height;
                    crossAxisExtent = this.size.width;
                    break;
                case Axis.horizontal:
                    mainAxisExtent = this.size.width;
                    crossAxisExtent = this.size.height;
                    break;
            }

            double centerOffsetAdjustment = this.center.centerOffsetAdjustment;

            int count = 0;
            do {
                var correction = this._attemptLayout(mainAxisExtent, crossAxisExtent,
                    this.offset.pixels + centerOffsetAdjustment);
                if (correction != 0.0) {
                    this.offset.correctBy(correction);
                } else {
                    if (this.offset.applyContentDimensions(
                        Math.Min(0.0, this._minScrollExtent + mainAxisExtent * this.anchor),
                        Math.Max(0.0, this._maxScrollExtent - mainAxisExtent * (1.0 - this.anchor))
                    ))
                        break;
                }

                count += 1;
            } while (count < RenderViewport._maxLayoutCycles);
        }

        public double _attemptLayout(double mainAxisExtent, double crossAxisExtent, double correctedOffset) {
            this._minScrollExtent = 0.0;
            this._maxScrollExtent = 0.0;
            this._hasVisualOverflow = false;

            double centerOffset = mainAxisExtent * this.anchor - correctedOffset;
            double reverseDirectionRemainingPaintExtent = centerOffset.clamp(0.0, mainAxisExtent);
            double forwardDirectionRemainingPaintExtent = (mainAxisExtent - centerOffset).clamp(0.0, mainAxisExtent);

            double fullCacheExtent = mainAxisExtent + 2 * this.cacheExtent;
            double centerCacheOffset = centerOffset + this.cacheExtent;
            double reverseDirectionRemainingCacheExtent = centerCacheOffset.clamp(0.0, fullCacheExtent);
            double forwardDirectionRemainingCacheExtent =
                (fullCacheExtent - centerCacheOffset).clamp(0.0, fullCacheExtent);

            RenderSliver leadingNegativeChild = this.childBefore(this.center);

            if (leadingNegativeChild != null) {
                double result = this.layoutChildSequence(
                    child: leadingNegativeChild,
                    scrollOffset: Math.Max(mainAxisExtent, centerOffset) - mainAxisExtent,
                    overlap: 0.0,
                    layoutOffset: forwardDirectionRemainingPaintExtent,
                    remainingPaintExtent: reverseDirectionRemainingPaintExtent,
                    mainAxisExtent: mainAxisExtent,
                    crossAxisExtent: crossAxisExtent,
                    growthDirection: GrowthDirection.reverse,
                    advance: this.childBefore,
                    remainingCacheExtent: reverseDirectionRemainingCacheExtent,
                    cacheOrigin: (mainAxisExtent - centerOffset).clamp(-this.cacheExtent, 0.0)
                );
                if (result != 0.0) {
                    return -result;
                }
            }

            return this.layoutChildSequence(
                child: this.center,
                scrollOffset: Math.Max(0.0, -centerOffset),
                overlap: leadingNegativeChild == null ? Math.Min(0.0, -centerOffset) : 0.0,
                layoutOffset: centerOffset >= mainAxisExtent ? centerOffset : reverseDirectionRemainingPaintExtent,
                remainingPaintExtent: forwardDirectionRemainingPaintExtent,
                mainAxisExtent: mainAxisExtent,
                crossAxisExtent: crossAxisExtent,
                growthDirection: GrowthDirection.forward,
                advance: this.childAfter,
                remainingCacheExtent: forwardDirectionRemainingCacheExtent,
                cacheOrigin: centerOffset.clamp(-this.cacheExtent, 0.0)
            );
        }

        public override bool hasVisualOverflow {
            get { return this._hasVisualOverflow; }
        }

        public override void updateOutOfBandData(GrowthDirection growthDirection, SliverGeometry childLayoutGeometry) {
            switch (growthDirection) {
                case GrowthDirection.forward:
                    this._maxScrollExtent += childLayoutGeometry.scrollExtent;
                    break;
                case GrowthDirection.reverse:
                    this._minScrollExtent -= childLayoutGeometry.scrollExtent;
                    break;
            }

            if (childLayoutGeometry.hasVisualOverflow) {
                this._hasVisualOverflow = true;
            }
        }

        public override void updateChildLayoutOffset(RenderSliver child, double layoutOffset,
            GrowthDirection growthDirection) {
            var childParentData = (SliverPhysicalParentData) child.parentData;
            childParentData.paintOffset = this.computeAbsolutePaintOffset(child, layoutOffset, growthDirection);
        }

        public override Offset paintOffsetOf(RenderSliver child) {
            var childParentData = (SliverPhysicalParentData) child.parentData;
            return childParentData.paintOffset;
        }

        public override double scrollOffsetOf(RenderSliver child, double scrollOffsetWithinChild) {
            GrowthDirection growthDirection = child.constraints.growthDirection;
            switch (growthDirection) {
                case GrowthDirection.forward: {
                    double scrollOffsetToChild = 0.0;
                    RenderSliver current = this.center;
                    while (current != child) {
                        scrollOffsetToChild += current.geometry.scrollExtent;
                        current = this.childAfter(current);
                    }

                    return scrollOffsetToChild + scrollOffsetWithinChild;
                }
                case GrowthDirection.reverse: {
                    double scrollOffsetToChild = 0.0;
                    RenderSliver current = this.childBefore(this.center);
                    while (current != child) {
                        scrollOffsetToChild -= current.geometry.scrollExtent;
                        current = this.childBefore(current);
                    }

                    return scrollOffsetToChild - scrollOffsetWithinChild;
                }
            }

            return 0.0;
        }

        public override double maxScrollObstructionExtentBefore(RenderSliver child) {
            GrowthDirection growthDirection = child.constraints.growthDirection;
            switch (growthDirection) {
                case GrowthDirection.forward: {
                    double pinnedExtent = 0.0;
                    RenderSliver current = this.center;
                    while (current != child) {
                        pinnedExtent += current.geometry.maxScrollObstructionExtent;
                        current = this.childAfter(current);
                    }

                    return pinnedExtent;
                }
                case GrowthDirection.reverse: {
                    double pinnedExtent = 0.0;
                    RenderSliver current = this.childBefore(this.center);
                    while (current != child) {
                        pinnedExtent += current.geometry.maxScrollObstructionExtent;
                        current = this.childBefore(current);
                    }

                    return pinnedExtent;
                }
            }

            return 0.0;
        }

        public override void applyPaintTransform(RenderObject child, ref Matrix4x4 transform) {
            var childParentData = (SliverPhysicalParentData) child.parentData;
            childParentData.applyPaintTransform(ref transform);
        }

        public override IEnumerable<RenderSliver> childrenInPaintOrder {
            get {
                if (this.firstChild == null) {
                    yield break;
                }

                var child = this.firstChild;
                while (child != this.center) {
                    yield return child;
                    child = this.childAfter(child);
                }

                child = this.lastChild;
                while (true) {
                    yield return child;
                    if (child == this.center) {
                        yield break;
                    }

                    child = this.childBefore(child);
                }
            }
        }
    }

    public class RenderShrinkWrappingViewport : RenderViewportBase<SliverLogicalContainerParentData> {
        public RenderShrinkWrappingViewport(
            AxisDirection axisDirection = AxisDirection.down,
            AxisDirection crossAxisDirection = AxisDirection.right,
            ViewportOffset offset = null,
            List<RenderSliver> children = null
        ) : base(
            axisDirection: axisDirection,
            crossAxisDirection: crossAxisDirection,
            offset: offset) {
            this.addAll(children);
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is SliverLogicalContainerParentData)) {
                child.parentData = new SliverLogicalContainerParentData();
            }
        }

        public double _maxScrollExtent = 0.0;
        public double _shrinkWrapExtent = 0.0;
        public bool _hasVisualOverflow = false;

        public override void performLayout() {
            if (this.firstChild == null) {
                switch (this.axis) {
                    case Axis.vertical:
                        this.size = new Size(this.constraints.maxWidth, this.constraints.minHeight);
                        break;
                    case Axis.horizontal:
                        this.size = new Size(this.constraints.minWidth, this.constraints.maxHeight);
                        break;
                }

                this.offset.applyViewportDimension(0.0);
                this._maxScrollExtent = 0.0;
                this._shrinkWrapExtent = 0.0;
                this._hasVisualOverflow = false;
                this.offset.applyContentDimensions(0.0, 0.0);
                return;
            }

            double mainAxisExtent = 0.0;
            double crossAxisExtent = 0.0;
            switch (this.axis) {
                case Axis.vertical:
                    mainAxisExtent = this.constraints.maxHeight;
                    crossAxisExtent = this.constraints.maxWidth;
                    break;
                case Axis.horizontal:
                    mainAxisExtent = this.constraints.maxWidth;
                    crossAxisExtent = this.constraints.maxHeight;
                    break;
            }

            double effectiveExtent = 0.0;
            do {
                var correction = this._attemptLayout(mainAxisExtent, crossAxisExtent, this.offset.pixels);
                if (correction != 0.0) {
                    this.offset.correctBy(correction);
                } else {
                    switch (this.axis) {
                        case Axis.vertical:
                            effectiveExtent = this.constraints.constrainHeight(this._shrinkWrapExtent);
                            break;
                        case Axis.horizontal:
                            effectiveExtent = this.constraints.constrainWidth(this._shrinkWrapExtent);
                            break;
                    }

                    bool didAcceptViewportDimension = this.offset.applyViewportDimension(effectiveExtent);
                    bool didAcceptContentDimension =
                        this.offset.applyContentDimensions(0.0, Math.Max(0.0, this._maxScrollExtent - effectiveExtent));
                    if (didAcceptViewportDimension && didAcceptContentDimension) {
                        break;
                    }
                }
            } while (true);

            switch (this.axis) {
                case Axis.vertical:
                    this.size = this.constraints.constrainDimensions(crossAxisExtent, effectiveExtent);
                    break;
                case Axis.horizontal:
                    this.size = this.constraints.constrainDimensions(effectiveExtent, crossAxisExtent);
                    break;
            }
        }

        public double _attemptLayout(double mainAxisExtent, double crossAxisExtent, double correctedOffset) {
            this._maxScrollExtent = 0.0;
            this._shrinkWrapExtent = 0.0;
            this._hasVisualOverflow = false;
            return this.layoutChildSequence(
                child: this.firstChild,
                scrollOffset: Math.Max(0.0, correctedOffset),
                overlap: Math.Min(0.0, correctedOffset),
                layoutOffset: 0.0,
                remainingPaintExtent: mainAxisExtent,
                mainAxisExtent: mainAxisExtent,
                crossAxisExtent: crossAxisExtent,
                growthDirection: GrowthDirection.forward,
                advance: this.childAfter,
                remainingCacheExtent: mainAxisExtent + 2 * this.cacheExtent,
                cacheOrigin: -this.cacheExtent
            );
        }

        public override bool hasVisualOverflow {
            get { return this._hasVisualOverflow; }
        }

        public override void updateOutOfBandData(GrowthDirection growthDirection, SliverGeometry childLayoutGeometry) {
            this._maxScrollExtent += childLayoutGeometry.scrollExtent;
            if (childLayoutGeometry.hasVisualOverflow) {
                this._hasVisualOverflow = true;
            }

            this._shrinkWrapExtent += childLayoutGeometry.maxPaintExtent;
        }

        public override void updateChildLayoutOffset(RenderSliver child, double layoutOffset,
            GrowthDirection growthDirection) {
            var childParentData = (SliverLogicalParentData) child.parentData;
            childParentData.layoutOffset = layoutOffset;
        }

        public override Offset paintOffsetOf(RenderSliver child) {
            var childParentData = (SliverLogicalParentData) child.parentData;
            return this.computeAbsolutePaintOffset(child, childParentData.layoutOffset, GrowthDirection.forward);
        }

        public override double scrollOffsetOf(RenderSliver child, double scrollOffsetWithinChild) {
            double scrollOffsetToChild = 0.0;
            RenderSliver current = this.firstChild;
            while (current != child) {
                scrollOffsetToChild += current.geometry.scrollExtent;
                current = this.childAfter(current);
            }

            return scrollOffsetToChild + scrollOffsetWithinChild;
        }

        public override double maxScrollObstructionExtentBefore(RenderSliver child) {
            double pinnedExtent = 0.0;
            RenderSliver current = this.firstChild;
            while (current != child) {
                pinnedExtent += current.geometry.maxScrollObstructionExtent;
                current = this.childAfter(current);
            }

            return pinnedExtent;
        }

        public override void applyPaintTransform(RenderObject child, ref Matrix4x4 transform) {
            Offset offset = this.paintOffsetOf((RenderSliver) child);
            transform = Matrix4x4.Translate(offset.toVector()) * transform;
        }

        public override IEnumerable<RenderSliver> childrenInPaintOrder {
            get {
                RenderSliver child = this.firstChild;
                while (child != null) {
                    yield return child;
                    child = this.childAfter(child);
                }
            }
        }
    }
}