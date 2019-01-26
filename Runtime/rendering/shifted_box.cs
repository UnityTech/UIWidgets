using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.rendering {
    public abstract class RenderShiftedBox : RenderObjectWithChildMixinRenderBox<RenderBox> {
        protected RenderShiftedBox(RenderBox child) {
            this.child = child;
        }

        protected override double computeMinIntrinsicWidth(double height) {
            if (this.child != null) {
                return this.child.getMinIntrinsicWidth(height);
            }

            return 0.0;
        }

        protected override double computeMaxIntrinsicWidth(double height) {
            if (this.child != null) {
                return this.child.getMaxIntrinsicWidth(height);
            }

            return 0.0;
        }

        protected override double computeMinIntrinsicHeight(double width) {
            if (this.child != null) {
                return this.child.getMinIntrinsicHeight(width);
            }

            return 0.0;
        }

        protected override double computeMaxIntrinsicHeight(double width) {
            if (this.child != null) {
                return this.child.getMaxIntrinsicHeight(width);
            }

            return 0.0;
        }

        protected override double? computeDistanceToActualBaseline(TextBaseline baseline) {
            double? result;

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

        protected override double computeMinIntrinsicWidth(double height) {
            if (this.child != null) {
                return this.child.getMinIntrinsicWidth(Math.Max(0.0, height - this._padding.vertical)) +
                       this._padding.horizontal;
            }

            return this._padding.horizontal;
        }

        protected override double computeMaxIntrinsicWidth(double height) {
            if (this.child != null) {
                return this.child.getMaxIntrinsicWidth(Math.Max(0.0, height - this._padding.vertical)) +
                       this._padding.horizontal;
            }

            return this._padding.horizontal;
        }

        protected override double computeMinIntrinsicHeight(double width) {
            if (this.child != null) {
                return this.child.getMinIntrinsicHeight(Math.Max(0.0, width - this._padding.horizontal)) +
                       this._padding.vertical;
            }

            return this._padding.vertical;
        }

        protected override double computeMaxIntrinsicHeight(double width) {
            if (this.child != null) {
                return this.child.getMaxIntrinsicHeight(Math.Max(0.0, width - this._padding.horizontal)) +
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
            double? widthFactor = null,
            double? heightFactor = null,
            Alignment alignment = null
        ) : base(alignment, child) {
            D.assert(widthFactor == null || widthFactor >= 0.0);
            D.assert(heightFactor == null || heightFactor >= 0.0);

            this._widthFactor = widthFactor;
            this._heightFactor = heightFactor;
        }

        public double? widthFactor {
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

        double? _widthFactor;

        public double? heightFactor {
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

        double? _heightFactor;

        protected override void performLayout() {
            bool shrinkWrapWidth = this._widthFactor != null || double.IsPositiveInfinity(this.constraints.maxWidth);
            bool shrinkWrapHeight = this._heightFactor != null || double.IsPositiveInfinity(this.constraints.maxHeight);

            if (this.child != null) {
                this.child.layout(this.constraints.loosen(), parentUsesSize: true);
                this.size = this.constraints.constrain(new Size(
                    shrinkWrapWidth ? this.child.size.width * (this._widthFactor ?? 1.0) : double.PositiveInfinity,
                    shrinkWrapHeight ? this.child.size.height * (this._heightFactor ?? 1.0) : double.PositiveInfinity));
                this.alignChild();
            }
            else {
                this.size = this.constraints.constrain(new Size(
                    shrinkWrapWidth ? 0.0 : double.PositiveInfinity,
                    shrinkWrapHeight ? 0.0 : double.PositiveInfinity));
            }
        }

        protected override void debugPaintSize(PaintingContext context, Offset offset) {
            base.debugPaintSize(context, offset);
            D.assert(() => {
                Paint paint;
                if (this.child != null && !this.child.size.isEmpty) {
//        Path path;
//        paint = Paint()
//          ..style = PaintingStyle.stroke
//          ..strokeWidth = 1.0
//          ..color = const Color(0xFFFFFF00);
//        path = Path();
//        final BoxParentData childParentData = child.parentData;
//        if (childParentData.offset.dy > 0.0) {
//          // vertical alignment arrows
//          final double headSize = math.min(childParentData.offset.dy * 0.2, 10.0);
//          path
//            ..moveTo(offset.dx + size.width / 2.0, offset.dy)
//            ..relativeLineTo(0.0, childParentData.offset.dy - headSize)
//            ..relativeLineTo(headSize, 0.0)
//            ..relativeLineTo(-headSize, headSize)
//            ..relativeLineTo(-headSize, -headSize)
//            ..relativeLineTo(headSize, 0.0)
//            ..moveTo(offset.dx + size.width / 2.0, offset.dy + size.height)
//            ..relativeLineTo(0.0, -childParentData.offset.dy + headSize)
//            ..relativeLineTo(headSize, 0.0)
//            ..relativeLineTo(-headSize, -headSize)
//            ..relativeLineTo(-headSize, headSize)
//            ..relativeLineTo(headSize, 0.0);
//          context.canvas.drawPath(path, paint);
//            }
//            if (childParentData.offset.dx > 0.0) {
//              // horizontal alignment arrows
//              final double headSize = math.min(childParentData.offset.dx * 0.2, 10.0);
//              path
//                ..moveTo(offset.dx, offset.dy + size.height / 2.0)
//                ..relativeLineTo(childParentData.offset.dx - headSize, 0.0)
//                ..relativeLineTo(0.0, headSize)
//                ..relativeLineTo(headSize, -headSize)
//                ..relativeLineTo(-headSize, -headSize)
//                ..relativeLineTo(0.0, headSize)
//                ..moveTo(offset.dx + size.width, offset.dy + size.height / 2.0)
//                ..relativeLineTo(-childParentData.offset.dx + headSize, 0.0)
//                ..relativeLineTo(0.0, headSize)
//                ..relativeLineTo(-headSize, -headSize)
//                ..relativeLineTo(headSize, -headSize)
//                ..relativeLineTo(0.0, headSize);
//              context.canvas.drawPath(path, paint);
//            }
                }
                else {
//        paint = Paint()
//          ..color = const Color(0x90909090);
//        context.canvas.drawRect(offset & size, paint);
                }

                return true;
            });
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DoubleProperty("widthFactor", this._widthFactor, ifNull: "expand"));
            properties.add(new DoubleProperty("heightFactor", this._heightFactor, ifNull: "expand"));
        }
    }

    public class RenderConstrainedOverflowBox : RenderAligningShiftedBox {
        public RenderConstrainedOverflowBox(
            RenderBox child = null,
            double? minWidth = null,
            double? maxWidth = null,
            double? minHeight = null,
            double? maxHeight = null,
            Alignment alignment = null
        ) : base(alignment, child) {
            this._minWidth = minWidth;
            this._maxWidth = maxWidth;
            this._minHeight = minHeight;
            this._maxHeight = maxHeight;
        }

        public double? minWidth {
            get { return this._minWidth; }
            set {
                if (this._minWidth == value) {
                    return;
                }

                this._minWidth = value;
                this.markNeedsLayout();
            }
        }

        public double? _minWidth;

        public double? maxWidth {
            get { return this._maxWidth; }
            set {
                if (this._maxWidth == value) {
                    return;
                }

                this._maxWidth = value;
                this.markNeedsLayout();
            }
        }

        public double? _maxWidth;

        public double? minHeight {
            get { return this._minHeight; }
            set {
                if (this._minHeight == value) {
                    return;
                }

                this._minHeight = value;
                this.markNeedsLayout();
            }
        }

        public double? _minHeight;

        public double? maxHeight {
            get { return this._maxHeight; }
            set {
                if (this._maxHeight == value) {
                    return;
                }

                this._maxHeight = value;
                this.markNeedsLayout();
            }
        }

        public double? _maxHeight;

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

        protected override double computeMinIntrinsicWidth(double height) {
            return this._requestedSize.width;
        }

        protected override double computeMaxIntrinsicWidth(double height) {
            return this._requestedSize.width;
        }

        protected override double computeMinIntrinsicHeight(double width) {
            return this._requestedSize.height;
        }

        protected override double computeMaxIntrinsicHeight(double width) {
            return this._requestedSize.height;
        }

        protected override double? computeDistanceToActualBaseline(TextBaseline baseline) {
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
            double? widthFactor = null,
            double? heightFactor = null,
            Alignment alignment = null
        ) : base(alignment, child) {
            this._widthFactor = widthFactor;
            this._heightFactor = heightFactor;
        }

        public double? widthFactor {
            get { return this._widthFactor; }
            set {
                if (this._widthFactor != value) {
                    return;
                }

                this._widthFactor = value;
                this.markNeedsLayout();
            }
        }

        public double? _widthFactor;

        public double? heightFactor {
            get { return this._heightFactor; }
            set {
                if (this._heightFactor != value) {
                    return;
                }

                this._heightFactor = value;
                this.markNeedsLayout();
            }
        }

        public double? _heightFactor;

        public BoxConstraints _getInnerConstraints(BoxConstraints constraints) {
            double minWidth = constraints.minWidth;
            double maxWidth = constraints.maxWidth;
            if (this._widthFactor != null) {
                double width = maxWidth * this._widthFactor.Value;
                minWidth = width;
                maxWidth = width;
            }

            double minHeight = constraints.minHeight;
            double maxHeight = constraints.maxHeight;
            if (this._heightFactor != null) {
                double height = maxHeight * this._heightFactor.Value;
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

        protected override double computeMinIntrinsicWidth(double height) {
            double result;
            if (this.child == null) {
                result = base.computeMinIntrinsicWidth(height);
            }
            else {
                result = this.child.getMinIntrinsicWidth(height * (this._heightFactor ?? 1.0));
            }

            return result / (this._widthFactor ?? 1.0);
        }

        protected override double computeMaxIntrinsicWidth(double height) {
            double result;
            if (this.child == null) {
                result = base.computeMaxIntrinsicWidth(height);
            }
            else {
                result = this.child.getMaxIntrinsicWidth(height * (this._heightFactor ?? 1.0));
            }

            return result / (this._widthFactor ?? 1.0);
        }

        protected override double computeMinIntrinsicHeight(double width) {
            double result;
            if (this.child == null) {
                result = base.computeMinIntrinsicHeight(width);
            }
            else {
                result = this.child.getMinIntrinsicHeight(width * (this._widthFactor ?? 1.0));
            }

            return result / (this._heightFactor ?? 1.0);
        }

        protected override double computeMaxIntrinsicHeight(double width) {
            double result;
            if (this.child == null) {
                result = base.computeMaxIntrinsicHeight(width);
            }
            else {
                result = this.child.getMaxIntrinsicHeight(width * (this._widthFactor ?? 1.0));
            }

            return result / (this._heightFactor ?? 1.0);
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


        protected override double computeMinIntrinsicWidth(double height) {
            double width = this._getSize(BoxConstraints.tightForFinite(height: height)).width;
            if (width.isFinite()) {
                return width;
            }

            return 0.0;
        }

        protected override double computeMaxIntrinsicWidth(double height) {
            double width = this._getSize(BoxConstraints.tightForFinite(height: height)).width;
            if (width.isFinite()) {
                return width;
            }

            return 0.0;
        }

        protected override double computeMinIntrinsicHeight(double width) {
            double height = this._getSize(BoxConstraints.tightForFinite(width: width)).height;
            if (height.isFinite()) {
                return height;
            }

            return 0.0;
        }

        protected override double computeMaxIntrinsicHeight(double width) {
            double height = this._getSize(BoxConstraints.tightForFinite(width: width)).height;
            if (height.isFinite()) {
                return height;
            }

            return 0.0;
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
            double baseline = 0.0,
            TextBaseline baselineType = TextBaseline.alphabetic
        ) : base(child) {
            this._baseline = baseline;
            this._baselineType = baselineType;
        }

        public double baseline {
            get { return this._baseline; }
            set {
                if (this._baseline != value) {
                    return;
                }

                this._baseline = value;
                this.markNeedsLayout();
            }
        }

        public double _baseline;


        public TextBaseline baselineType {
            get { return this._baselineType; }
            set {
                if (this._baselineType != value) {
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
                double? childBaseline = this.child.getDistanceToBaseline(this.baselineType);
                double actualBaseline = this.baseline;
                double top = actualBaseline - childBaseline.Value;
                var childParentData = (BoxParentData) this.child.parentData;
                childParentData.offset = new Offset(0.0, top);
                Size childSize = this.child.size;
                this.size = this.constraints.constrain(new Size(childSize.width, top + childSize.height));
            }
            else {
                this.performResize();
            }
        }
    }
}