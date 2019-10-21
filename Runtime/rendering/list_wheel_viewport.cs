using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    delegate float ___ChildSizingFunction(RenderBox child);

    public interface IListWheelChildManager {
        int? childCount { get; }
        bool childExistsAt(int index);
        void createChild(int index, RenderBox after);
        void removeChild(RenderBox child);
    }

    public class ListWheelParentData : ContainerBoxParentData<RenderBox> {
        public int index;
    }

    public class RenderListWheelViewport : ContainerRenderObjectMixinRenderBox<RenderBox, ListWheelParentData>,
        RenderAbstractViewport {
        public RenderListWheelViewport(
            IListWheelChildManager childManager,
            ViewportOffset offset,
            float itemExtent,
            float diameterRatio = defaultDiameterRatio,
            float perspective = defaultPerspective,
            float offAxisFraction = 0.0f,
            bool useMagnifier = false,
            float magnification = 1.0f,
            bool clipToSize = true,
            bool renderChildrenOutsideViewport = false,
            List<RenderBox> children = null
        ) {
            D.assert(childManager != null);
            D.assert(offset != null);
            D.assert(diameterRatio > 0, () => diameterRatioZeroMessage);
            D.assert(perspective > 0);
            D.assert(perspective <= 0.01f, () => perspectiveTooHighMessage);
            D.assert(magnification > 0);
            D.assert(itemExtent > 0);
            D.assert(
                !renderChildrenOutsideViewport || !clipToSize,
                () => clipToSizeAndRenderChildrenOutsideViewportConflict
            );

            this.childManager = childManager;
            this._offset = offset;
            this._diameterRatio = diameterRatio;
            this._perspective = perspective;
            this._offAxisFraction = offAxisFraction;
            this._useMagnifier = useMagnifier;
            this._magnification = magnification;
            this._itemExtent = itemExtent;
            this._clipToSize = clipToSize;
            this._renderChildrenOutsideViewport = renderChildrenOutsideViewport;
            this.addAll(children);
        }

        public const float defaultDiameterRatio = 2.0f;

        public const float defaultPerspective = 0.003f;

        public const string diameterRatioZeroMessage = "You can't set a diameterRatio " +
                                                       "of 0 or of a negative number. It would imply a cylinder of 0 in diameter " +
                                                       "in which case nothing will be drawn.";

        public const string perspectiveTooHighMessage = "A perspective too high will " +
                                                        "be clipped in the z-axis and therefore not renderable. Value must be " +
                                                        "between 0 and 0.0f1.";

        public const string clipToSizeAndRenderChildrenOutsideViewportConflict =
            "Cannot renderChildrenOutsideViewport and clipToSize since children " +
            "rendered outside will be clipped anyway.";

        public readonly IListWheelChildManager childManager;

        public ViewportOffset offset {
            get { return this._offset; }
            set {
                D.assert(value != null);
                if (value == this._offset) {
                    return;
                }

                if (this.attached) {
                    this._offset.removeListener(this._hasScrolled);
                }

                this._offset = value;
                if (this.attached) {
                    this._offset.addListener(this._hasScrolled);
                }

                this.markNeedsLayout();
            }
        }

        ViewportOffset _offset;

        public float diameterRatio {
            get { return this._diameterRatio; }
            set {
                D.assert(
                    value > 0,
                    () => diameterRatioZeroMessage
                );

                this._diameterRatio = value;
                this.markNeedsPaint();
            }
        }

        float _diameterRatio;

        public float perspective {
            get { return this._perspective; }
            set {
                D.assert(value > 0);
                D.assert(
                    value <= 0.01f,
                    () => perspectiveTooHighMessage
                );
                if (value == this._perspective) {
                    return;
                }

                this._perspective = value;
                this.markNeedsPaint();
            }
        }

        float _perspective;

        public float offAxisFraction {
            get { return this._offAxisFraction; }
            set {
                if (value == this._offAxisFraction) {
                    return;
                }

                this._offAxisFraction = value;
                this.markNeedsPaint();
            }
        }

        float _offAxisFraction = 0.0f;

        public bool useMagnifier {
            get { return this._useMagnifier; }
            set {
                if (value == this._useMagnifier) {
                    return;
                }

                this._useMagnifier = value;
                this.markNeedsPaint();
            }
        }

        bool _useMagnifier = false;

        public float magnification {
            get { return this._magnification; }
            set {
                D.assert(value > 0);
                if (value == this._magnification) {
                    return;
                }

                this._magnification = value;
                this.markNeedsPaint();
            }
        }

        float _magnification = 1.0f;

        public float itemExtent {
            get { return this._itemExtent; }
            set {
                D.assert(value > 0);
                if (value == this._itemExtent) {
                    return;
                }

                this._itemExtent = value;
                this.markNeedsLayout();
            }
        }

        float _itemExtent;

        public bool clipToSize {
            get { return this._clipToSize; }
            set {
                D.assert(
                    !this.renderChildrenOutsideViewport || !this.clipToSize,
                    () => clipToSizeAndRenderChildrenOutsideViewportConflict
                );
                if (value == this._clipToSize) {
                    return;
                }

                this._clipToSize = value;
                this.markNeedsPaint();
            }
        }

        bool _clipToSize;

        public bool renderChildrenOutsideViewport {
            get { return this._renderChildrenOutsideViewport; }
            set {
                D.assert(
                    !this.renderChildrenOutsideViewport || !this.clipToSize,
                    () => clipToSizeAndRenderChildrenOutsideViewportConflict
                );
                if (value == this._renderChildrenOutsideViewport) {
                    return;
                }

                this._renderChildrenOutsideViewport = value;
                this.markNeedsLayout();
            }
        }

        bool _renderChildrenOutsideViewport;


        void _hasScrolled() {
            this.markNeedsLayout();
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is ListWheelParentData)) {
                child.parentData = new ListWheelParentData();
            }
        }

        public override void attach(object owner) {
            base.attach(owner);
            this._offset.addListener(this._hasScrolled);
        }

        public override void detach() {
            this._offset.removeListener(this._hasScrolled);
            base.detach();
        }

        public override bool isRepaintBoundary {
            get { return true; }
        }

        float _viewportExtent {
            get {
                D.assert(this.hasSize);
                return this.size.height;
            }
        }

        float _minEstimatedScrollExtent {
            get {
                D.assert(this.hasSize);
                if (this.childManager.childCount == null) {
                    return float.NegativeInfinity;
                }

                return 0.0f;
            }
        }

        float _maxEstimatedScrollExtent {
            get {
                D.assert(this.hasSize);
                if (this.childManager.childCount == null) {
                    return float.PositiveInfinity;
                }

                return Mathf.Max(0.0f, ((this.childManager.childCount ?? 0) - 1) * this._itemExtent);
            }
        }

        float _topScrollMarginExtent {
            get {
                D.assert(this.hasSize);
                return -this.size.height / 2.0f + this._itemExtent / 2.0f;
            }
        }

        float _getUntransformedPaintingCoordinateY(float layoutCoordinateY) {
            return layoutCoordinateY - this._topScrollMarginExtent - this.offset.pixels;
        }

        float _maxVisibleRadian {
            get {
                if (this._diameterRatio < 1.0f) {
                    return Mathf.PI / 2.0f;
                }

                return Mathf.Asin(1.0f / this._diameterRatio);
            }
        }

        float _getIntrinsicCrossAxis(___ChildSizingFunction childSize) {
            float extent = 0.0f;
            RenderBox child = this.firstChild;
            while (child != null) {
                extent = Mathf.Max(extent, childSize(child));
                child = this.childAfter(child);
            }

            return extent;
        }

        protected override float computeMinIntrinsicWidth(float height) {
            return this._getIntrinsicCrossAxis(
                (RenderBox child) => child.getMinIntrinsicWidth(height)
            );
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return this._getIntrinsicCrossAxis(
                (RenderBox child) => child.getMaxIntrinsicWidth(height)
            );
        }

        protected override float computeMinIntrinsicHeight(float width) {
            if (this.childManager.childCount == null) {
                return 0.0f;
            }

            return (this.childManager.childCount ?? 0) * this._itemExtent;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (this.childManager.childCount == null) {
                return 0.0f;
            }

            return (this.childManager.childCount ?? 0) * this._itemExtent;
        }

        protected override bool sizedByParent {
            get { return true; }
        }

        protected override void performResize() {
            this.size = this.constraints.biggest;
        }

        public int indexOf(RenderBox child) {
            D.assert(child != null);
            ListWheelParentData childParentData = (ListWheelParentData) child.parentData;
            return childParentData.index;
        }

        public int scrollOffsetToIndex(float scrollOffset) {
            return (scrollOffset / this.itemExtent).floor();
        }

        public float indexToScrollOffset(int index) {
            return index * this.itemExtent;
        }

        void _createChild(int index,
            RenderBox after = null
        ) {
            this.invokeLayoutCallback<BoxConstraints>((BoxConstraints constraints) => {
                D.assert(this.constraints == this.constraints);
                this.childManager.createChild(index, after: after);
            });
        }

        void _destroyChild(RenderBox child) {
            this.invokeLayoutCallback<BoxConstraints>((BoxConstraints constraints) => {
                D.assert(this.constraints == this.constraints);
                this.childManager.removeChild(child);
            });
        }

        void _layoutChild(RenderBox child, BoxConstraints constraints, int index) {
            child.layout(constraints, parentUsesSize: true);
            ListWheelParentData childParentData = (ListWheelParentData) child.parentData;
            float crossPosition = this.size.width / 2.0f - child.size.width / 2.0f;
            childParentData.offset = new Offset(crossPosition, this.indexToScrollOffset(index));
        }

        protected override void performLayout() {
            BoxConstraints childConstraints = this.constraints.copyWith(
                minHeight: this._itemExtent,
                maxHeight: this._itemExtent,
                minWidth: 0.0f
            );

            float visibleHeight = this.size.height;
            if (this.renderChildrenOutsideViewport) {
                visibleHeight *= 2;
            }

            float firstVisibleOffset = this.offset.pixels + this._itemExtent / 2 - visibleHeight / 2;
            float lastVisibleOffset = firstVisibleOffset + visibleHeight;

            int targetFirstIndex = this.scrollOffsetToIndex(firstVisibleOffset);
            int targetLastIndex = this.scrollOffsetToIndex(lastVisibleOffset);

            if (targetLastIndex * this._itemExtent == lastVisibleOffset) {
                targetLastIndex--;
            }

            while (!this.childManager.childExistsAt(targetFirstIndex) && targetFirstIndex <= targetLastIndex) {
                targetFirstIndex++;
            }

            while (!this.childManager.childExistsAt(targetLastIndex) && targetFirstIndex <= targetLastIndex) {
                targetLastIndex--;
            }

            if (targetFirstIndex > targetLastIndex) {
                while (this.firstChild != null) {
                    this._destroyChild(this.firstChild);
                }

                return;
            }


            if (this.childCount > 0 &&
                (this.indexOf(this.firstChild) > targetLastIndex || this.indexOf(this.lastChild) < targetFirstIndex)) {
                while (this.firstChild != null) {
                    this._destroyChild(this.firstChild);
                }
            }


            if (this.childCount == 0) {
                this._createChild(targetFirstIndex);
                this._layoutChild(this.firstChild, childConstraints, targetFirstIndex);
            }

            int currentFirstIndex = this.indexOf(this.firstChild);
            int currentLastIndex = this.indexOf(this.lastChild);

            while (currentFirstIndex < targetFirstIndex) {
                this._destroyChild(this.firstChild);
                currentFirstIndex++;
            }

            while (currentLastIndex > targetLastIndex) {
                this._destroyChild(this.lastChild);
                currentLastIndex--;
            }

            RenderBox child = this.firstChild;
            while (child != null) {
                child.layout(childConstraints, parentUsesSize: true);
                child = this.childAfter(child);
            }

            while (currentFirstIndex > targetFirstIndex) {
                this._createChild(currentFirstIndex - 1);
                this._layoutChild(this.firstChild, childConstraints, --currentFirstIndex);
            }

            while (currentLastIndex < targetLastIndex) {
                this._createChild(currentLastIndex + 1, after: this.lastChild);
                this._layoutChild(this.lastChild, childConstraints, ++currentLastIndex);
            }

            this.offset.applyViewportDimension(this._viewportExtent);

            float minScrollExtent = this.childManager.childExistsAt(targetFirstIndex - 1)
                ? this._minEstimatedScrollExtent
                : this.indexToScrollOffset(targetFirstIndex);
            float maxScrollExtent = this.childManager.childExistsAt(targetLastIndex + 1)
                ? this._maxEstimatedScrollExtent
                : this.indexToScrollOffset(targetLastIndex);
            this.offset.applyContentDimensions(minScrollExtent, maxScrollExtent);
        }

        bool _shouldClipAtCurrentOffset() {
            float highestUntransformedPaintY = this._getUntransformedPaintingCoordinateY(0.0f);
            return highestUntransformedPaintY < 0.0f
                   || this.size.height < highestUntransformedPaintY + this._maxEstimatedScrollExtent + this._itemExtent;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.childCount > 0) {
                if (this._clipToSize && this._shouldClipAtCurrentOffset()) {
                    context.pushClipRect(
                        this.needsCompositing,
                        offset,
                        Offset.zero & this.size, this._paintVisibleChildren
                    );
                }
                else {
                    this._paintVisibleChildren(context, offset);
                }
            }
        }

        void _paintVisibleChildren(PaintingContext context, Offset offset) {
            RenderBox childToPaint = this.firstChild;
            ListWheelParentData childParentData = (ListWheelParentData) childToPaint?.parentData;

            while (childParentData != null) {
                this._paintTransformedChild(childToPaint, context, offset, childParentData.offset);
                childToPaint = this.childAfter(childToPaint);
                childParentData = (ListWheelParentData) childToPaint?.parentData;
            }
        }

        void _paintTransformedChild(RenderBox child, PaintingContext context, Offset offset, Offset layoutOffset) {
            Offset untransformedPaintingCoordinates = offset + new Offset(
                                                          layoutOffset.dx,
                                                          this._getUntransformedPaintingCoordinateY(layoutOffset.dy)
                                                      );


            float fractionalY = (untransformedPaintingCoordinates.dy + this._itemExtent / 2.0f) / this.size.height;

            float angle = -(fractionalY - 0.5f) * 2.0f * this._maxVisibleRadian;
            if (angle > Mathf.PI / 2.0f || angle < -Mathf.PI / 2.0f) {
                return;
            }

            var radius = this.size.height * this._diameterRatio / 2.0f;
            var deltaY = radius * Mathf.Sin(angle);

            Matrix3 transform = Matrix3.I();
            // Matrix4x4 transform2 = MatrixUtils.createCylindricalProjectionTransform(
            //     radius: this.size.height * this._diameterRatio / 2.0f,
            //     angle: angle,
            //     perspective: this._perspective
            // );

            // Offset offsetToCenter = new Offset(untransformedPaintingCoordinates.dx, -this._topScrollMarginExtent);

            Offset offsetToCenter =
                new Offset(untransformedPaintingCoordinates.dx, -deltaY - this._topScrollMarginExtent);

            if (!this.useMagnifier) {
                this._paintChildCylindrically(context, offset, child, transform, offsetToCenter);
            }
            else {
                this._paintChildWithMagnifier(
                    context,
                    offset,
                    child,
                    transform,
                    offsetToCenter,
                    untransformedPaintingCoordinates
                );
            }
        }

        void _paintChildWithMagnifier(
            PaintingContext context,
            Offset offset,
            RenderBox child,
            // Matrix4x4 cylindricalTransform,
            Matrix3 cylindricalTransform,
            Offset offsetToCenter,
            Offset untransformedPaintingCoordinates
        ) {
            float magnifierTopLinePosition = this.size.height / 2 - this._itemExtent * this._magnification / 2;
            float magnifierBottomLinePosition = this.size.height / 2 + this._itemExtent * this._magnification / 2;

            bool isAfterMagnifierTopLine = untransformedPaintingCoordinates.dy
                                           >= magnifierTopLinePosition - this._itemExtent * this._magnification;
            bool isBeforeMagnifierBottomLine = untransformedPaintingCoordinates.dy
                                               <= magnifierBottomLinePosition;

            if (isAfterMagnifierTopLine && isBeforeMagnifierBottomLine) {
                Rect centerRect = Rect.fromLTWH(
                    0.0f,
                    magnifierTopLinePosition, this.size.width, this._itemExtent * this._magnification);
                Rect topHalfRect = Rect.fromLTWH(
                    0.0f,
                    0.0f, this.size.width,
                    magnifierTopLinePosition);
                Rect bottomHalfRect = Rect.fromLTWH(
                    0.0f,
                    magnifierBottomLinePosition, this.size.width,
                    magnifierTopLinePosition);

                context.pushClipRect(
                    false,
                    offset,
                    centerRect,
                    (PaintingContext context1, Offset offset1) => {
                        context1.pushTransform(
                            false,
                            offset1,
                            cylindricalTransform,
                            // this._centerOriginTransform(cylindricalTransform),
                            (PaintingContext context2, Offset offset2) => {
                                context2.paintChild(
                                    child,
                                    offset2 + untransformedPaintingCoordinates);
                            });
                    });

                context.pushClipRect(
                    false,
                    offset,
                    untransformedPaintingCoordinates.dy <= magnifierTopLinePosition
                        ? topHalfRect
                        : bottomHalfRect,
                    (PaintingContext context1, Offset offset1) => {
                        this._paintChildCylindrically(
                            context1,
                            offset1,
                            child,
                            cylindricalTransform,
                            offsetToCenter
                        );
                    }
                );
            }
            else {
                this._paintChildCylindrically(
                    context,
                    offset,
                    child,
                    cylindricalTransform,
                    offsetToCenter
                );
            }
        }

        void _paintChildCylindrically(
            PaintingContext context,
            Offset offset,
            RenderBox child,
            // Matrix4x4 cylindricalTransform,
            Matrix3 cylindricalTransform,
            Offset offsetToCenter
        ) {
            context.pushTransform(
                false,
                offset,
                cylindricalTransform,
                // this._centerOriginTransform(cylindricalTransform),
                (PaintingContext _context, Offset _offset) => { _context.paintChild(child, _offset + offsetToCenter); }
            );
        }

        public override Rect describeApproximatePaintClip(RenderObject child) {
            if (child != null && this._shouldClipAtCurrentOffset()) {
                return Offset.zero & this.size;
            }

            return null;
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position = null
        ) {
            return false;
        }

        public RevealedOffset getOffsetToReveal(RenderObject target, float alignment,
            Rect rect = null
        ) {
            rect = rect ?? target.paintBounds;

            RenderObject child = target;
            while (child.parent != this) {
                child = (RenderObject) child.parent;
            }

            ListWheelParentData parentData = (ListWheelParentData) child.parentData;
            float targetOffset = parentData.offset.dy;
            Matrix4x4 transform = target.getTransformTo(this).toMatrix4x4();
            Rect bounds = MatrixUtils.transformRect(transform, rect);
            Rect targetRect = bounds.translate(0.0f, (this.size.height - this.itemExtent) / 2);

            return new RevealedOffset(offset: targetOffset, rect: targetRect);
        }

        public new RenderObject parent {
            get { return (RenderObject) base.parent; }
        }

        public new void showOnScreen(
            RenderObject descendant = null,
            Rect rect = null,
            TimeSpan? duration = null,
            Curve curve = null
        ) {
            duration = duration ?? TimeSpan.Zero;
            curve = curve ?? Curves.ease;
            if (descendant != null) {
                RevealedOffset revealedOffset = this.getOffsetToReveal(descendant, 0.5f, rect: rect);
                if (duration == TimeSpan.Zero) {
                    this.offset.jumpTo(revealedOffset.offset);
                }
                else {
                    this.offset.animateTo(revealedOffset.offset, duration: (TimeSpan) duration, curve: curve);
                }

                rect = revealedOffset.rect;
            }

            base.showOnScreen(
                rect: rect,
                duration: duration,
                curve: curve
            );
        }
    }
}