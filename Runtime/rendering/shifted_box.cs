using UIWidgets.Runtime.rendering;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public abstract class RenderShiftedBox : RenderObjectWithChildMixinRenderBox<RenderBox> {
        protected RenderShiftedBox(RenderBox child) {
            this.child = child;
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

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            float? result;

            if (this.child != null) {
                D.assert(!this.debugNeedsLayout);

                result = this.child.getDistanceToActualBaseline(baseline);
                if (result != null) {
                    var childParentData = (BoxParentData) this.child.parentData;
                    result += childParentData.offset.dy;
                }
            }
            else {
                result = base.computeDistanceToActualBaseline(baseline);
            }

            return result;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child != null) {
                var childParentData = (BoxParentData) this.child.parentData;
                context.paintChild(this.child, childParentData.offset + offset);
            }
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position = null) {
            if (this.child != null) {
                var childParentData = (BoxParentData) this.child.parentData;
                return this.child.hitTest(result, position - childParentData.offset);
            }

            return false;
        }
    }

    public class RenderPadding : RenderShiftedBox {
        public RenderPadding(
            EdgeInsets padding = null,
            RenderBox child = null
        ) : base(child) {
            D.assert(padding != null);
            D.assert(padding.isNonNegative);

            this._padding = padding;
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

        protected override float computeMinIntrinsicWidth(float height) {
            if (this.child != null) {
                return this.child.getMinIntrinsicWidth(Mathf.Max(0.0f, height - this._padding.vertical)) +
                       this._padding.horizontal;
            }

            return this._padding.horizontal;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            if (this.child != null) {
                return this.child.getMaxIntrinsicWidth(Mathf.Max(0.0f, height - this._padding.vertical)) +
                       this._padding.horizontal;
            }

            return this._padding.horizontal;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            if (this.child != null) {
                return this.child.getMinIntrinsicHeight(Mathf.Max(0.0f, width - this._padding.horizontal)) +
                       this._padding.vertical;
            }

            return this._padding.vertical;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            if (this.child != null) {
                return this.child.getMaxIntrinsicHeight(Mathf.Max(0.0f, width - this._padding.horizontal)) +
                       this._padding.vertical;
            }

            return this._padding.vertical;
        }

        protected override void performLayout() {
            if (this.child == null) {
                this.size = this.constraints.constrain(this._padding.inflateSize(Size.zero));
                return;
            }

            var innerConstraints = this.constraints.deflate(this._padding);
            this.child.layout(innerConstraints, parentUsesSize: true);

            var childParentData = (BoxParentData) this.child.parentData;
            childParentData.offset = this._padding.topLeft;
            this.size = this.constraints.constrain(this._padding.inflateSize(this.child.size));
        }

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            base.debugPaintSize(context, offset);
            D.assert(() => {
                Rect outerRect = offset & this.size;
                D.debugPaintPadding(context.canvas, outerRect,
                    this.child != null ? this._padding.deflateRect(outerRect) : null);
                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding", this.padding));
        }
    }

    public abstract class RenderAligningShiftedBox : RenderShiftedBox {
        protected RenderAligningShiftedBox(
            Alignment alignment = null,
            RenderBox child = null
        ) : base(child) {
            this._alignment = alignment ?? Alignment.center;
        }

        public Alignment alignment {
            get { return this._alignment; }
            set {
                D.assert(value != null);
                if (this._alignment == value) {
                    return;
                }

                this._alignment = value;
                this.markNeedsLayout();
            }
        }

        Alignment _alignment;

        protected void alignChild() {
            D.assert(this.child != null);
            D.assert(!this.child.debugNeedsLayout);
            D.assert(this.child.hasSize);
            D.assert(this.hasSize);

            var childParentData = (BoxParentData) this.child.parentData;
            childParentData.offset = this._alignment.alongOffset(this.size - this.child.size);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Alignment>("alignment", this.alignment));
        }
    }

    public class RenderPositionedBox : RenderAligningShiftedBox {
        public RenderPositionedBox(
            RenderBox child = null,
            float? widthFactor = null,
            float? heightFactor = null,
            Alignment alignment = null
        ) : base(alignment, child) {
            D.assert(widthFactor == null || widthFactor >= 0.0);
            D.assert(heightFactor == null || heightFactor >= 0.0);

            this._widthFactor = widthFactor;
            this._heightFactor = heightFactor;
        }

        public float? widthFactor {
            get { return this._widthFactor; }
            set {
                D.assert(value == null || value >= 0.0);
                if (this._widthFactor == value) {
                    return;
                }

                this._widthFactor = value;
                this.markNeedsLayout();
            }
        }

        float? _widthFactor;

        public float? heightFactor {
            get { return this._heightFactor; }
            set {
                D.assert(value == null || value >= 0.0);
                if (this._heightFactor == value) {
                    return;
                }

                this._heightFactor = value;
                this.markNeedsLayout();
            }
        }

        float? _heightFactor;

        protected override void performLayout() {
            bool shrinkWrapWidth = this._widthFactor != null || float.IsPositiveInfinity(this.constraints.maxWidth);
            bool shrinkWrapHeight = this._heightFactor != null || float.IsPositiveInfinity(this.constraints.maxHeight);

            if (this.child != null) {
                this.child.layout(this.constraints.loosen(), parentUsesSize: true);
                this.size = this.constraints.constrain(new Size(
                    shrinkWrapWidth ? this.child.size.width * (this._widthFactor ?? 1.0f) : float.PositiveInfinity,
                    shrinkWrapHeight ? this.child.size.height * (this._heightFactor ?? 1.0f) : float.PositiveInfinity));
                this.alignChild();
            }
            else {
                this.size = this.constraints.constrain(new Size(
                    shrinkWrapWidth ? 0.0f : float.PositiveInfinity,
                    shrinkWrapHeight ? 0.0f : float.PositiveInfinity));
            }
        }

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
           base.debugPaintSize(context, offset);
            D.assert(() => {
                Paint paint;
                if (this.child != null && !this.child.size.isEmpty) {
                    Path path;
                    paint = new Paint {
                        style = PaintingStyle.stroke,
                        strokeWidth = 1.0f,
                        color = new ui.Color(0xFFFFFF00)
                    };

                    BoxParentData childParentData = (BoxParentData) this.child.parentData;
                    if (childParentData.offset.dy > 0) {
                        float headSize = Mathf.Min(childParentData.offset.dy * 0.2f, 10.0f);

                        float x = offset.dx + this.size.width / 2.0f;
                        float y = offset.dy;
                        path = new Path();
                        path.moveTo(x, y);
                        path.lineTo(x += 0.0f, y += childParentData.offset.dy - headSize);
                        path.lineTo(x += headSize, y += 0.0f);
                        path.lineTo(x += -headSize, y += headSize);
                        path.lineTo(x += -headSize, y += -headSize);
                        path.lineTo(x += headSize, y += 0.0f);

                        x = offset.dx + this.size.width / 2.0f;
                        y = offset.dy + this.size.height;
                        path.moveTo(x, y);
                        path.lineTo(x += 0.0f, y += -childParentData.offset.dy + headSize);
                        path.lineTo(x += headSize, y += 0.0f);
                        path.lineTo(x += -headSize, y += -headSize);
                        path.lineTo(x += -headSize, y += headSize);
                        path.lineTo(x += headSize, y += 0.0f);
                        context.canvas.drawPath(path, paint);
                    }

                    if (childParentData.offset.dx > 0.0) {

                        float headSize = Mathf.Min(childParentData.offset.dx * 0.2f, 10.0f);
                        float x = offset.dx;
                        float y = offset.dy + this.size.height / 2.0f;
                        path = new Path();
                        path.moveTo(x, y);
                        path.lineTo(x += childParentData.offset.dx - headSize, y += 0.0f);
                        path.lineTo(x += 0.0f, y += headSize);
                        path.lineTo(x += headSize, y += -headSize);
                        path.lineTo(x += -headSize, y += -headSize);
                        path.lineTo(x += 0.0f, y += headSize);
                        
                        path.moveTo(x = offset.dx + this.size.width, y = offset.dy + this.size.height / 2.0f);
                        path.lineTo(x += -childParentData.offset.dx + headSize, y += 0.0f);
                        path.lineTo(x += 0.0f, y += headSize);
                        path.lineTo(x += -headSize, y += -headSize);
                        path.lineTo(x += headSize, y += -headSize);
                        path.lineTo(x += 0.0f, y += headSize);
                        path.close();
                        context.canvas.drawPath(path, paint);
                    }
                } else {
                    paint = new Paint {
                        color = new ui.Color(0x90909090),
                    };
                    context.canvas.drawRect(offset & this.size, paint);
                }

                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("widthFactor", this._widthFactor, ifNull: "expand"));
            properties.add(new FloatProperty("heightFactor", this._heightFactor, ifNull: "expand"));
        }
    }

    public class RenderConstrainedOverflowBox : RenderAligningShiftedBox {
        public RenderConstrainedOverflowBox(
            RenderBox child = null,
            float? minWidth = null,
            float? maxWidth = null,
            float? minHeight = null,
            float? maxHeight = null,
            Alignment alignment = null
        ) : base(alignment, child) {
            this._minWidth = minWidth;
            this._maxWidth = maxWidth;
            this._minHeight = minHeight;
            this._maxHeight = maxHeight;
        }

        public float? minWidth {
            get { return this._minWidth; }
            set {
                if (this._minWidth == value) {
                    return;
                }

                this._minWidth = value;
                this.markNeedsLayout();
            }
        }

        public float? _minWidth;

        public float? maxWidth {
            get { return this._maxWidth; }
            set {
                if (this._maxWidth == value) {
                    return;
                }

                this._maxWidth = value;
                this.markNeedsLayout();
            }
        }

        public float? _maxWidth;

        public float? minHeight {
            get { return this._minHeight; }
            set {
                if (this._minHeight == value) {
                    return;
                }

                this._minHeight = value;
                this.markNeedsLayout();
            }
        }

        public float? _minHeight;

        public float? maxHeight {
            get { return this._maxHeight; }
            set {
                if (this._maxHeight == value) {
                    return;
                }

                this._maxHeight = value;
                this.markNeedsLayout();
            }
        }

        public float? _maxHeight;

        public BoxConstraints _getInnerConstraints(BoxConstraints constraints) {
            return new BoxConstraints(
                minWidth: this._minWidth ?? constraints.minWidth,
                maxWidth: this._maxWidth ?? constraints.maxWidth,
                minHeight: this._minHeight ?? constraints.minHeight,
                maxHeight: this._maxHeight ?? constraints.maxHeight
            );
        }

        protected override bool sizedByParent {
            get { return true; }
        }

        protected override void performResize() {
            this.size = this.constraints.biggest;
        }

        protected override void performLayout() {
            if (this.child != null) {
                this.child.layout(this._getInnerConstraints(this.constraints), parentUsesSize: true);
                this.alignChild();
            }
        }
    }

    public class RenderUnconstrainedBox : RenderAligningShiftedBox {
        public RenderUnconstrainedBox(
            Alignment alignment = null,
            Axis? constrainedAxis = null,
            RenderBox child = null
        ) : base(alignment, child) {
            this._constrainedAxis = constrainedAxis;
        }

        public Axis? constrainedAxis {
            get { return this._constrainedAxis; }
            set {
                if (this._constrainedAxis == value) {
                    return;
                }

                this._constrainedAxis = value;
                this.markNeedsLayout();
            }
        }

        public Axis? _constrainedAxis;

        public Rect _overflowContainerRect = Rect.zero;
        public Rect _overflowChildRect = Rect.zero;
        public bool _isOverflowing = false;

        protected override void performLayout() {
            if (this.child != null) {
                BoxConstraints childConstraints = null;
                if (this.constrainedAxis != null) {
                    switch (this.constrainedAxis) {
                        case Axis.horizontal:
                            childConstraints = new BoxConstraints(
                                maxWidth: this.constraints.maxWidth,
                                minWidth: this.constraints.minWidth);
                            break;
                        case Axis.vertical:
                            childConstraints = new BoxConstraints(
                                maxHeight: this.constraints.maxHeight,
                                minHeight: this.constraints.minHeight);
                            break;
                    }
                }
                else {
                    childConstraints = new BoxConstraints();
                }

                this.child.layout(childConstraints, parentUsesSize: true);
                this.size = this.constraints.constrain(this.child.size);
                this.alignChild();
                var childParentData = (BoxParentData) this.child.parentData;
                this._overflowContainerRect = Offset.zero & this.size;
                this._overflowChildRect = childParentData.offset & this.child.size;
            }
            else {
                this.size = this.constraints.smallest;
                this._overflowContainerRect = Rect.zero;
                this._overflowChildRect = Rect.zero;
            }

            this._isOverflowing = RelativeRect.fromRect(
                this._overflowContainerRect, this._overflowChildRect).hasInsets;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this.child == null || this.size.isEmpty) {
                return;
            }

            if (!this._isOverflowing) {
                base.paint(context, offset);
                return;
            }

            context.pushClipRect(this.needsCompositing, offset, Offset.zero & this.size, base.paint);
            D.assert(() => {
                DebugOverflowIndicatorMixin.paintOverflowIndicator(this, context, offset, this._overflowContainerRect, this._overflowChildRect);
                return true;
            });
        }
    }

    public class RenderSizedOverflowBox : RenderAligningShiftedBox {
        public RenderSizedOverflowBox(
            RenderBox child = null,
            Size requestedSize = null,
            Alignment alignment = null
        ) : base(alignment, child) {
            this._requestedSize = requestedSize;
        }

        public Size requestedSize {
            get { return this._requestedSize; }
            set {
                if (this._requestedSize == value) {
                    return;
                }

                this._requestedSize = value;
                this.markNeedsLayout();
            }
        }

        public Size _requestedSize;

        protected override float computeMinIntrinsicWidth(float height) {
            return this._requestedSize.width;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return this._requestedSize.width;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            return this._requestedSize.height;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return this._requestedSize.height;
        }

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            if (this.child != null) {
                return this.child.getDistanceToActualBaseline(baseline);
            }

            return base.computeDistanceToActualBaseline(baseline);
        }

        protected override void performLayout() {
            this.size = this.constraints.constrain(this._requestedSize);
            if (this.child != null) {
                this.child.layout(this.constraints);
                this.alignChild();
            }
        }
    }

    public class RenderFractionallySizedOverflowBox : RenderAligningShiftedBox {
        public RenderFractionallySizedOverflowBox(
            RenderBox child = null,
            float? widthFactor = null,
            float? heightFactor = null,
            Alignment alignment = null
        ) : base(alignment, child) {
            this._widthFactor = widthFactor;
            this._heightFactor = heightFactor;
        }

        public float? widthFactor {
            get { return this._widthFactor; }
            set {
                if (this._widthFactor == value) {
                    return;
                }

                this._widthFactor = value;
                this.markNeedsLayout();
            }
        }

        public float? _widthFactor;

        public float? heightFactor {
            get { return this._heightFactor; }
            set {
                if (this._heightFactor == value) {
                    return;
                }

                this._heightFactor = value;
                this.markNeedsLayout();
            }
        }

        public float? _heightFactor;

        public BoxConstraints _getInnerConstraints(BoxConstraints constraints) {
            float minWidth = constraints.minWidth;
            float maxWidth = constraints.maxWidth;
            if (this._widthFactor != null) {
                float width = maxWidth * this._widthFactor.Value;
                minWidth = width;
                maxWidth = width;
            }

            float minHeight = constraints.minHeight;
            float maxHeight = constraints.maxHeight;
            if (this._heightFactor != null) {
                float height = maxHeight * this._heightFactor.Value;
                minHeight = height;
                maxHeight = height;
            }

            return new BoxConstraints(
                minWidth: minWidth,
                maxWidth: maxWidth,
                minHeight: minHeight,
                maxHeight: maxHeight
            );
        }

        protected override float computeMinIntrinsicWidth(float height) {
            float result;
            if (this.child == null) {
                result = base.computeMinIntrinsicWidth(height);
            }
            else {
                result = this.child.getMinIntrinsicWidth(height * (this._heightFactor ?? 1.0f));
            }

            return result / (this._widthFactor ?? 1.0f);
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            float result;
            if (this.child == null) {
                result = base.computeMaxIntrinsicWidth(height);
            }
            else {
                result = this.child.getMaxIntrinsicWidth(height * (this._heightFactor ?? 1.0f));
            }

            return result / (this._widthFactor ?? 1.0f);
        }

        protected override float computeMinIntrinsicHeight(float width) {
            float result;
            if (this.child == null) {
                result = base.computeMinIntrinsicHeight(width);
            }
            else {
                result = this.child.getMinIntrinsicHeight(width * (this._widthFactor ?? 1.0f));
            }

            return result / (this._heightFactor ?? 1.0f);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            float result;
            if (this.child == null) {
                result = base.computeMaxIntrinsicHeight(width);
            }
            else {
                result = this.child.getMaxIntrinsicHeight(width * (this._widthFactor ?? 1.0f));
            }

            return result / (this._heightFactor ?? 1.0f);
        }

        protected override void performLayout() {
            if (this.child != null) {
                this.child.layout(this._getInnerConstraints(this.constraints), parentUsesSize: true);
                this.size = this.constraints.constrain(this.child.size);
                this.alignChild();
            }
            else {
                this.size = this.constraints.constrain(
                    this._getInnerConstraints(this.constraints).constrain(Size.zero));
            }
        }
    }

    public abstract class SingleChildLayoutDelegate {
        public SingleChildLayoutDelegate(Listenable _relayout = null) {
            this._relayout = _relayout;
        }

        public readonly Listenable _relayout;

        public virtual Size getSize(BoxConstraints constraints) {
            return constraints.biggest;
        }

        public virtual BoxConstraints getConstraintsForChild(BoxConstraints constraints) {
            return constraints;
        }

        public virtual Offset getPositionForChild(Size size, Size childSize) {
            return Offset.zero;
        }

        public abstract bool shouldRelayout(SingleChildLayoutDelegate oldDelegate);
    }

    public class RenderCustomSingleChildLayoutBox : RenderShiftedBox {
        public RenderCustomSingleChildLayoutBox(RenderBox child = null,
            SingleChildLayoutDelegate layoutDelegate = null) : base(child) {
            D.assert(layoutDelegate != null);
            this._delegate = layoutDelegate;
        }

        public SingleChildLayoutDelegate layoutDelegate {
            get { return this._delegate; }
            set {
                var newDelegate = value;
                D.assert(newDelegate != null);
                if (this._delegate == newDelegate) {
                    return;
                }

                SingleChildLayoutDelegate oldDelegate = this._delegate;
                if (newDelegate.GetType() != oldDelegate.GetType() || newDelegate.shouldRelayout(oldDelegate)) {
                    this.markNeedsLayout();
                }

                this._delegate = newDelegate;
                if (this.attached) {
                    oldDelegate?._relayout?.removeListener(this.markNeedsLayout);
                    newDelegate?._relayout?.addListener(this.markNeedsLayout);
                }
            }
        }

        SingleChildLayoutDelegate _delegate;

        public override void attach(object owner) {
            base.attach(owner);
            this._delegate?._relayout?.addListener(this.markNeedsLayout);
        }

        public override void detach() {
            this._delegate?._relayout?.removeListener(this.markNeedsLayout);
            base.detach();
        }

        Size _getSize(BoxConstraints constraints) {
            return constraints.constrain(this._delegate.getSize(constraints));
        }


        protected override float computeMinIntrinsicWidth(float height) {
            float width = this._getSize(BoxConstraints.tightForFinite(height: height)).width;
            if (width.isFinite()) {
                return width;
            }

            return 0.0f;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            float width = this._getSize(BoxConstraints.tightForFinite(height: height)).width;
            if (width.isFinite()) {
                return width;
            }

            return 0.0f;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            float height = this._getSize(BoxConstraints.tightForFinite(width: width)).height;
            if (height.isFinite()) {
                return height;
            }

            return 0.0f;
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            float height = this._getSize(BoxConstraints.tightForFinite(width: width)).height;
            if (height.isFinite()) {
                return height;
            }

            return 0.0f;
        }

        protected override void performLayout() {
            this.size = this._getSize(this.constraints);
            if (this.child != null) {
                BoxConstraints childConstraints = this.layoutDelegate.getConstraintsForChild(this.constraints);
                D.assert(childConstraints.debugAssertIsValid(isAppliedConstraint: true));
                this.child.layout(childConstraints, parentUsesSize: !childConstraints.isTight);
                BoxParentData childParentData = (BoxParentData) this.child.parentData;
                childParentData.offset = this.layoutDelegate.getPositionForChild(this.size,
                    childConstraints.isTight ? childConstraints.smallest : this.child.size);
            }
        }
    }

    public class RenderBaseline : RenderShiftedBox {
        public RenderBaseline(
            RenderBox child = null,
            float baseline = 0.0f,
            TextBaseline baselineType = TextBaseline.alphabetic
        ) : base(child) {
            this._baseline = baseline;
            this._baselineType = baselineType;
        }

        public float baseline {
            get { return this._baseline; }
            set {
                if (this._baseline == value) {
                    return;
                }

                this._baseline = value;
                this.markNeedsLayout();
            }
        }

        public float _baseline;


        public TextBaseline baselineType {
            get { return this._baselineType; }
            set {
                if (this._baselineType == value) {
                    return;
                }

                this._baselineType = value;
                this.markNeedsLayout();
            }
        }

        public TextBaseline _baselineType;

        protected override void performLayout() {
            if (this.child != null) {
                this.child.layout(this.constraints.loosen(), parentUsesSize: true);
                float? childBaseline = this.child.getDistanceToBaseline(this.baselineType);
                float actualBaseline = this.baseline;
                float top = actualBaseline - childBaseline.Value;
                var childParentData = (BoxParentData) this.child.parentData;
                childParentData.offset = new Offset(0.0f, top);
                Size childSize = this.child.size;
                this.size = this.constraints.constrain(new Size(childSize.width, top + childSize.height));
            }
            else {
                this.performResize();
            }
        }
    }
}