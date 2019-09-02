using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.widgets {
    public class SingleChildScrollView : StatelessWidget {
        public SingleChildScrollView(
            Key key = null,
            Axis scrollDirection = Axis.vertical,
            bool reverse = false,
            EdgeInsets padding = null,
            bool? primary = null,
            ScrollPhysics physics = null,
            ScrollController controller = null,
            Widget child = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            D.assert(!(controller != null && primary == true),
                () => "Primary ScrollViews obtain their ScrollController via inheritance from a PrimaryScrollController widget. " +
                "You cannot both set primary to true and pass an explicit controller.");
            this.scrollDirection = scrollDirection;
            this.reverse = reverse;
            this.padding = padding;
            this.primary = primary ?? controller == null && scrollDirection == Axis.vertical;
            this.physics = physics;
            this.controller = controller;
            this.child = child;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly Axis scrollDirection;

        public readonly bool reverse;

        public readonly EdgeInsets padding;

        public readonly ScrollController controller;

        public readonly bool primary;

        public readonly ScrollPhysics physics;

        public readonly Widget child;

        public readonly DragStartBehavior dragStartBehavior;

        AxisDirection _getDirection(BuildContext context) {
            return AxisDirectionUtils.getAxisDirectionFromAxisReverseAndDirectionality(context, this.scrollDirection,
                       this.reverse) ?? AxisDirection.down;
        }

        public override Widget build(BuildContext context) {
            AxisDirection axisDirection = this._getDirection(context);
            Widget contents = this.child;
            if (this.padding != null) {
                contents = new Padding(
                    padding: this.padding,
                    child: contents);
            }

            ScrollController scrollController = this.primary
                ? PrimaryScrollController.of(context)
                : this.controller;

            Scrollable scrollable = new Scrollable(
                dragStartBehavior: this.dragStartBehavior,
                axisDirection: axisDirection,
                controller: scrollController,
                physics: this.physics,
                viewportBuilder: (BuildContext subContext, ViewportOffset offset) => {
                    return new _SingleChildViewport(
                        axisDirection: axisDirection,
                        offset: offset,
                        child: contents);
                }
            );

            if (this.primary && scrollController != null) {
                return PrimaryScrollController.none(child: scrollable);
            }

            return scrollable;
        }
    }


    class _SingleChildViewport : SingleChildRenderObjectWidget {
        public _SingleChildViewport(
            Key key = null,
            AxisDirection axisDirection = AxisDirection.down,
            ViewportOffset offset = null,
            Widget child = null
        ) : base(key: key, child: child) {
            this.axisDirection = axisDirection;
            this.offset = offset;
        }

        public readonly AxisDirection axisDirection;

        public readonly ViewportOffset offset;


        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderSingleChildViewport(
                axisDirection: this.axisDirection,
                offset: this.offset
            );
        }


        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            _RenderSingleChildViewport _renderObject = (_RenderSingleChildViewport) renderObject;
            _renderObject.axisDirection = this.axisDirection;
            _renderObject.offset = this.offset;
        }
    }


    class _RenderSingleChildViewport : RenderObjectWithChildMixinRenderBox<RenderBox>, RenderAbstractViewport {
        public _RenderSingleChildViewport(
            AxisDirection axisDirection = AxisDirection.down,
            ViewportOffset offset = null,
            float cacheExtent = RenderViewportUtils.defaultCacheExtent,
            RenderBox child = null) {
            D.assert(offset != null);
            this._axisDirection = axisDirection;
            this._offset = offset;
            this._cacheExtent = cacheExtent;
            this.child = child;
        }

        public new RenderObject parent {
            get { return (RenderObject) base.parent; }
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

        AxisDirection _axisDirection;

        public Axis axis {
            get { return AxisUtils.axisDirectionToAxis(this.axisDirection); }
        }

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

        public float cacheExtent {
            get { return this._cacheExtent; }
            set {
                if (value == this._cacheExtent) {
                    return;
                }

                this._cacheExtent = value;
                this.markNeedsLayout();
            }
        }

        float _cacheExtent;

        void _hasScrolled() {
            this.markNeedsPaint();
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is ParentData)) {
                child.parentData = new ParentData();
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
                switch (this.axis) {
                    case Axis.horizontal:
                        return this.size.width;
                    case Axis.vertical:
                        return this.size.height;
                }

                D.assert(false);
                return 0.0f;
            }
        }

        float _minScrollExtent {
            get {
                D.assert(this.hasSize);
                return 0.0f;
            }
        }

        float _maxScrollExtent {
            get {
                D.assert(this.hasSize);
                if (this.child == null) {
                    return 0.0f;
                }

                switch (this.axis) {
                    case Axis.horizontal:
                        return Mathf.Max(0.0f, this.child.size.width - this.size.width);
                    case Axis.vertical:
                        return Mathf.Max(0.0f, this.child.size.height - this.size.height);
                }

                D.assert(false);
                return 0.0f;
            }
        }

        BoxConstraints _getInnerConstraints(BoxConstraints constraints) {
            switch (this.axis) {
                case Axis.horizontal:
                    return constraints.heightConstraints();
                case Axis.vertical:
                    return constraints.widthConstraints();
            }

            return null;
        }


        protected override float computeMinIntrinsicWidth(float height) {
            if (this.child != null) {
                return this.child.getMinIntrinsicWidth(height);
            }

            return 0.0f;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            if (this.child != null) {
                return this.child.getMaxIntrinsicWidth(height);
            }

            return 0.0f;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            if (this.child != null) {
                return this.child.getMinIntrinsicHeight(width);
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (this.child != null) {
                return this.child.getMaxIntrinsicHeight(width);
            }

            return 0.0f;
        }

        protected override void performLayout() {
            if (this.child == null) {
                this.size = this.constraints.smallest;
            }
            else {
                this.child.layout(this._getInnerConstraints(this.constraints), parentUsesSize: true);
                this.size = this.constraints.constrain(this.child.size);
            }

            this.offset.applyViewportDimension(this._viewportExtent);
            this.offset.applyContentDimensions(this._minScrollExtent, this._maxScrollExtent);
        }

        Offset _paintOffset {
            get { return this._paintOffsetForPosition(this.offset.pixels); }
        }

        Offset _paintOffsetForPosition(float position) {
            switch (this.axisDirection) {
                case AxisDirection.up:
                    return new Offset(0.0f, position - this.child.size.height + this.size.height);
                case AxisDirection.down:
                    return new Offset(0.0f, -position);
                case AxisDirection.left:
                    return new Offset(position - this.child.size.width + this.size.width, 0.0f);
                case AxisDirection.right:
                    return new Offset(-position, 0.0f);
            }

            return null;
        }

        bool _shouldClipAtPaintOffset(Offset paintOffset) {
            D.assert(this.child != null);
            return paintOffset < Offset.zero ||
                   !(Offset.zero & this.size).contains((paintOffset & this.child.size).bottomRight);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                Offset paintOffset = this._paintOffset;

                void paintContents(PaintingContext subContext, Offset SubOffset) {
                    subContext.paintChild(this.child, SubOffset + paintOffset);
                }

                if (this._shouldClipAtPaintOffset(paintOffset)) {
                    context.pushClipRect(this.needsCompositing, offset, Offset.zero & this.size, paintContents);
                }
                else {
                    paintContents(context, offset);
                }
            }
        }

        public override void applyPaintTransform(RenderObject child, Matrix3 transform) {
            Offset paintOffset = this._paintOffset;
            transform.preTranslate(paintOffset.dx, paintOffset.dy);
        }

        public override Rect describeApproximatePaintClip(RenderObject child) {
            if (child != null && this._shouldClipAtPaintOffset(this._paintOffset)) {
                return Offset.zero & this.size;
            }

            return null;
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position = null) {
            if (this.child != null) {
                Offset transformed = position + (-this._paintOffset);
                return this.child.hitTest(result, position: transformed);
            }

            return false;
        }


        public RevealedOffset getOffsetToReveal(RenderObject target, float alignment, Rect rect = null) {
            rect = rect ?? target.paintBounds;
            if (!(target is RenderBox)) {
                return new RevealedOffset(offset: this.offset.pixels, rect: rect);
            }

            RenderBox targetBox = (RenderBox) target;
            Matrix3 transform = targetBox.getTransformTo(this);
            Rect bounds = transform.mapRect(rect);
            Size contentSize = this.child.size;

            float leadingScrollOffset = 0.0f;
            float targetMainAxisExtent = 0.0f;
            float mainAxisExtent = 0.0f;

            switch (this.axisDirection) {
                case AxisDirection.up:
                    mainAxisExtent = this.size.height;
                    leadingScrollOffset = contentSize.height - bounds.bottom;
                    targetMainAxisExtent = bounds.height;
                    break;
                case AxisDirection.right:
                    mainAxisExtent = this.size.width;
                    leadingScrollOffset = bounds.left;
                    targetMainAxisExtent = bounds.width;
                    break;
                case AxisDirection.down:
                    mainAxisExtent = this.size.height;
                    leadingScrollOffset = bounds.top;
                    targetMainAxisExtent = bounds.height;
                    break;
                case AxisDirection.left:
                    mainAxisExtent = this.size.width;
                    leadingScrollOffset = contentSize.width - bounds.right;
                    targetMainAxisExtent = bounds.width;
                    break;
            }

            float targetOffset = leadingScrollOffset - (mainAxisExtent - targetMainAxisExtent) * alignment;
            Rect targetRect = bounds.shift(this._paintOffsetForPosition(targetOffset));
            return new RevealedOffset(offset: targetOffset, rect: targetRect);
        }

        public override void showOnScreen(
            RenderObject descendant = null,
            Rect rect = null,
            TimeSpan? duration = null,
            Curve curve = null
        ) {
            if (!this.offset.allowImplicitScrolling) {
                base.showOnScreen(
                    descendant: descendant,
                    rect: rect,
                    duration: duration,
                    curve: curve
                );
            }

            Rect newRect = RenderViewport.showInViewport(
                descendant: descendant,
                viewport: this,
                offset: this.offset,
                rect: rect,
                duration: duration,
                curve: curve);

            base.showOnScreen(
                rect: newRect,
                duration: duration,
                curve: curve);
        }
    }
}