using System;
using System.Collections.Generic;
using UIWidgets.painting;
using UIWidgets.ui;
using UIWidgets.foundation;
using UIWidgets.gestures;
using UnityEngine;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.rendering {
    public class RelativeRect : IEquatable<RelativeRect> {
        private RelativeRect(double left, double top, double right, double bottom) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public readonly double left;
        public readonly double top;
        public readonly double right;
        public readonly double bottom;

        public static RelativeRect fromLTRB(double left, double top, double right, double bottom) {
            return new RelativeRect(left, top, right, bottom);
        }

        public static RelativeRect fromSize(Rect rect, Size container) {
            return new RelativeRect(
                rect.left,
                rect.top,
                container.width - rect.right,
                container.height - rect.bottom);
        }

        public static RelativeRect fromRect(Rect rect, Rect container) {
            return RelativeRect.fromLTRB(
                rect.left - container.left,
                rect.top - container.top,
                container.right - rect.right,
                container.bottom - rect.bottom
            );
        }

        public static readonly RelativeRect fill = RelativeRect.fromLTRB(0.0, 0.0, 0.0, 0.0);

        public bool hasInsets {
            get { return this.left > 0.0 || this.top > 0.0 || this.right > 0.0 || this.bottom > 0.0; }
        }

        public RelativeRect shift(Offset offset) {
            return RelativeRect.fromLTRB(
                this.left + offset.dx,
                this.top + offset.dy,
                this.right - offset.dx,
                this.bottom - offset.dy);
        }

        public RelativeRect inflate(double delta) {
            return RelativeRect.fromLTRB(
                this.left - delta,
                this.top - delta,
                this.right - delta,
                this.bottom - delta);
        }

        public RelativeRect deflate(double delta) {
            return this.inflate(-delta);
        }

        public RelativeRect intersect(RelativeRect other) {
            return RelativeRect.fromLTRB(
                Math.Max(this.left, other.left),
                Math.Max(this.top, other.top),
                Math.Max(this.right, other.right),
                Math.Max(this.bottom, other.bottom)
            );
        }

        public Rect toRect(Rect container) {
            return Rect.fromLTRB(
                this.left + container.left,
                this.top + container.top,
                container.right - this.right,
                container.bottom - this.bottom);
        }

        public Rect toSize(Size container) {
            return Rect.fromLTRB(
                this.left,
                this.top,
                container.width - this.right,
                container.height - this.bottom);
        }

        public bool Equals(RelativeRect other) {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.left.Equals(other.left)
                   && this.top.Equals(other.top)
                   && this.right.Equals(other.right)
                   && this.bottom.Equals(other.bottom);
        }

        public override bool Equals(object obj) {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((RelativeRect) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.left.GetHashCode();
                hashCode = (hashCode * 397) ^ this.top.GetHashCode();
                hashCode = (hashCode * 397) ^ this.right.GetHashCode();
                hashCode = (hashCode * 397) ^ this.bottom.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(RelativeRect a, RelativeRect b) {
            return object.Equals(a, b);
        }

        public static bool operator !=(RelativeRect a, RelativeRect b) {
            return !(a == b);
        }
    }

    public class StackParentData : ContainerParentDataMixinBoxParentData<RenderBox> {
        public double? top;
        public double? right;
        public double? bottom;
        public double? left;
        public double? width;
        public double? height;

        public bool isPositioned {
            get { return top != null || right != null || bottom != null || left != null || width != null || height != null; }
        }

        private RelativeRect rect {
            get { return RelativeRect.fromLTRB(left ?? 0.0, top ?? 0.0, right ?? 0.0, bottom ?? 0.0); }
            set {
                top = value.top;
                right = value.right;
                bottom = value.bottom;
                left = value.left;
            }
        }
    }

    public enum StackFit {
        loose,
        expand,
        passthrough,
    }

    public enum Overflow {
        visible,
        clip,
    }

    public class RenderStack : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<RenderBox,
        StackParentData> {
        public RenderStack(
            TextDirection? textDirection,
            StackFit? fit,
            Overflow? overflow,
            List<RenderBox> children = null,
            AlignmentDirectional alignment = null) {
            this._alignment = alignment ?? AlignmentDirectional.topStart;
            this._textDirection = textDirection ?? TextDirection.ltr;
            this._fit = fit ?? StackFit.loose;
            this._overflow = overflow ?? Overflow.clip;
            addAll(children);
        }

        bool _hasVisualOverflow = false;

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is StackParentData)) {
                child.parentData = new StackParentData();
            }
        }

        Alignment _resolvedAlignment;

        void _resolve() {
            if (_resolvedAlignment != null)
                return;
            _resolvedAlignment = alignment.resolve(textDirection);
        }

        void _markNeedResolution() {
            _resolvedAlignment = null;
            markNeedsLayout();
        }

        private AlignmentDirectional _alignment;

        public AlignmentDirectional alignment {
            get { return _alignment; }
            set {
                if (_alignment == value)
                    return;
                _alignment = value;
                _markNeedResolution();
            }
        }

        private TextDirection _textDirection;

        public TextDirection textDirection {
            get { return _textDirection; }
            set {
                if (_textDirection == value)
                    return;
                _textDirection = value;
                _markNeedResolution();
            }
        }

        private StackFit _fit;

        public StackFit fit {
            get { return _fit; }
            set {
                if (_fit == value)
                    return;
                _fit = value;
                markNeedsLayout();
            }
        }

        private Overflow _overflow;

        public Overflow overflow {
            get { return _overflow; }
            set {
                if (_overflow == value)
                    return;
                _overflow = value;
                markNeedsPaint();
            }
        }

        public delegate double mainChildSizeGetter(RenderBox child);

        double _getIntrinsicDimension(mainChildSizeGetter getter) {
            double extent = 0.0;
            RenderBox child = firstChild;
            while (child != null) {
                StackParentData childParentData = (StackParentData) child.parentData;
                if (!childParentData.isPositioned)
                    extent = Math.Max(extent, getter(child));
                D.assert(child.parentData == childParentData);
                if (childParentData != null) child = childParentData.nextSibling;
            }

            return extent;
        }

        protected override double computeMinIntrinsicWidth(double height) {
            return _getIntrinsicDimension((RenderBox child) => child.getMinIntrinsicWidth(height));
        }

        protected override double computeMaxIntrinsicWidth(double height) {
            return _getIntrinsicDimension((RenderBox child) => child.getMaxIntrinsicWidth(height));
        }

        protected override double computeMinIntrinsicHeight(double width) {
            return _getIntrinsicDimension((RenderBox child) => child.getMinIntrinsicHeight(width));
        }

        protected override double computeMaxIntrinsicHeight(double width) {
            return _getIntrinsicDimension((RenderBox child) => child.getMaxIntrinsicHeight(width));
        }

        protected override double? computeDistanceToActualBaseline(TextBaseline baseline) {
            return defaultComputeDistanceToHighestActualBaseline(baseline);
        }

        protected override void performLayout() {
            _resolve();
            D.assert(_resolvedAlignment != null);
            _hasVisualOverflow = false;
            bool hasNonPositionedChildren = false;
            if (childCount == 0) {
                size = constraints.biggest;
                return;
            }

            double width = constraints.minWidth;
            double height = constraints.minHeight;

            BoxConstraints nonPositionedConstraints = null;
            switch (fit) {
                case StackFit.loose:
                    nonPositionedConstraints = constraints.loosen();
                    break;
                case StackFit.expand:
                    nonPositionedConstraints = BoxConstraints.tight(constraints.biggest);
                    break;
                case StackFit.passthrough:
                    nonPositionedConstraints = constraints;
                    break;
            }


            RenderBox child = firstChild;
            while (child != null) {
                StackParentData childParentData = (StackParentData) child.parentData;

                if (!childParentData.isPositioned) {
                    hasNonPositionedChildren = true;

                    child.layout(nonPositionedConstraints, parentUsesSize: true);

                    Size childSize = child.size;
                    width = Math.Max(width, childSize.width);
                    height = Math.Max(height, childSize.height);
                }

                child = childParentData.nextSibling;
            }

            if (hasNonPositionedChildren) {
                size = new Size(width, height);
                D.assert(size.width == constraints.constrainWidth(width));
                D.assert(size.height == constraints.constrainHeight(height));
            } else {
                size = constraints.biggest;
            }

            child = firstChild;
            while (child != null) {
                StackParentData childParentData = (StackParentData) child.parentData;

                if (!childParentData.isPositioned) {
                    childParentData.offset = _resolvedAlignment.alongOffset(size - child.size);
                } else {
                    BoxConstraints childConstraints = new BoxConstraints();

                    if (childParentData.left != null && childParentData.right != null)
                        childConstraints =
                            childConstraints.tighten(width: size.width - childParentData.right - childParentData.left);
                    else if (childParentData.width != null)
                        childConstraints = childConstraints.tighten(width: childParentData.width);

                    if (childParentData.top != null && childParentData.bottom != null)
                        childConstraints =
                            childConstraints.tighten(
                                height: size.height - childParentData.bottom - childParentData.top);
                    else if (childParentData.height != null)
                        childConstraints = childConstraints.tighten(height: childParentData.height);

                    child.layout(childConstraints, parentUsesSize: true);

                    double x;
                    if (childParentData.left != null) {
                        x = childParentData.left.Value;
                    }
                    else if (childParentData.right != null) {
                        x = size.width - childParentData.right.Value - child.size.width;
                    }
                    else {
                        x = _resolvedAlignment.alongOffset(size - child.size).dx;
                    }

                    if (x < 0.0 || x + child.size.width > size.width)
                        _hasVisualOverflow = true;

                    double y;
                    if (childParentData.top != null) {
                        y = childParentData.top.Value;
                    }
                    else if (childParentData.bottom != null) {
                        y = size.height - childParentData.bottom.Value - child.size.height;
                    }
                    else {
                        y = _resolvedAlignment.alongOffset(size - child.size).dy;
                    }

                    if (y < 0.0 || y + child.size.height > size.height)
                        _hasVisualOverflow = true;

                    childParentData.offset = new Offset(x, y);
                }

                D.assert(child.parentData == childParentData);
                child = childParentData.nextSibling;
            }
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position = null) {
            return defaultHitTestChildren(result, position: position);
        }
        
        public void paintStack(PaintingContext context, Offset offset) {
            defaultPaint(context, offset);
        }
        
        public override void paint(PaintingContext context, Offset offset) {
            if (_overflow == Overflow.clip && _hasVisualOverflow) {
                context.pushClipRect(needsCompositing, offset, Offset.zero & size, paintStack);
            } else {
                paintStack(context, offset);
            }
        }

        public override Rect describeApproximatePaintClip(RenderObject childRaw) {
            return _hasVisualOverflow ? Offset.zero & size : null;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<AlignmentDirectional>("alignment", alignment));
            properties.add(new EnumProperty<StackFit>("fit", fit));
            properties.add(new EnumProperty<Overflow>("overflow", overflow));
        }
    }
}