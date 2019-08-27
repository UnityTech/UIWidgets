using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public class RelativeRect : IEquatable<RelativeRect> {
        RelativeRect(float left, float top, float right, float bottom) {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public readonly float left;
        public readonly float top;
        public readonly float right;
        public readonly float bottom;

        public static RelativeRect fromLTRB(float left, float top, float right, float bottom) {
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
            return fromLTRB(
                rect.left - container.left,
                rect.top - container.top,
                container.right - rect.right,
                container.bottom - rect.bottom
            );
        }

        public static readonly RelativeRect fill = fromLTRB(0.0f, 0.0f, 0.0f, 0.0f);

        public bool hasInsets {
            get { return this.left > 0.0 || this.top > 0.0 || this.right > 0.0 || this.bottom > 0.0; }
        }

        public RelativeRect shift(Offset offset) {
            return fromLTRB(
                this.left + offset.dx,
                this.top + offset.dy,
                this.right - offset.dx,
                this.bottom - offset.dy);
        }

        public RelativeRect inflate(float delta) {
            return fromLTRB(
                this.left - delta,
                this.top - delta,
                this.right - delta,
                this.bottom - delta);
        }

        public RelativeRect deflate(float delta) {
            return this.inflate(-delta);
        }

        public RelativeRect intersect(RelativeRect other) {
            return fromLTRB(
                Mathf.Max(this.left, other.left),
                Mathf.Max(this.top, other.top),
                Mathf.Max(this.right, other.right),
                Mathf.Max(this.bottom, other.bottom)
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

        public static RelativeRect lerp(RelativeRect a, RelativeRect b, float t) {
            if (a == null && b == null) {
                return null;
            }

            if (a == null) {
                return fromLTRB(b.left * t, b.top * t, b.right * t, b.bottom * t);
            }

            if (b == null) {
                float k = 1.0f - t;
                return fromLTRB(b.left * k, b.top * k, b.right * k, b.bottom * k);
            }

            return fromLTRB(
                MathUtils.lerpFloat(a.left, b.left, t),
                MathUtils.lerpFloat(a.top, b.top, t),
                MathUtils.lerpFloat(a.right, b.right, t),
                MathUtils.lerpFloat(a.bottom, b.bottom, t)
            );
        }

        public bool Equals(RelativeRect other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.left.Equals(other.left)
                   && this.top.Equals(other.top)
                   && this.right.Equals(other.right)
                   && this.bottom.Equals(other.bottom);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

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
            return Equals(a, b);
        }

        public static bool operator !=(RelativeRect a, RelativeRect b) {
            return !(a == b);
        }
    }

    public class StackParentData : ContainerParentDataMixinBoxParentData<RenderBox> {
        public float? top;
        public float? right;
        public float? bottom;
        public float? left;
        public float? width;
        public float? height;

        public bool isPositioned {
            get {
                return this.top != null || this.right != null || this.bottom != null || this.left != null ||
                       this.width != null || this.height != null;
            }
        }

        RelativeRect rect {
            get {
                return RelativeRect.fromLTRB(this.left ?? 0.0f, this.top ?? 0.0f, this.right ?? 0.0f,
                    this.bottom ?? 0.0f);
            }
            set {
                this.top = value.top;
                this.right = value.right;
                this.bottom = value.bottom;
                this.left = value.left;
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
            StackFit? fit,
            Overflow? overflow,
            List<RenderBox> children = null,
            Alignment alignment = null) {
            this._alignment = alignment ?? Alignment.topLeft;
            this._fit = fit ?? StackFit.loose;
            this._overflow = overflow ?? Overflow.clip;
            this.addAll(children);
        }

        bool _hasVisualOverflow = false;

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is StackParentData)) {
                child.parentData = new StackParentData();
            }
        }

        Alignment _alignment;

        public Alignment alignment {
            get { return this._alignment; }
            set {
                if (this._alignment == value) {
                    return;
                }

                this._alignment = value;
                this.markNeedsLayout();
            }
        }

        StackFit _fit;

        public StackFit fit {
            get { return this._fit; }
            set {
                if (this._fit == value) {
                    return;
                }

                this._fit = value;
                this.markNeedsLayout();
            }
        }

        Overflow _overflow;

        public Overflow overflow {
            get { return this._overflow; }
            set {
                if (this._overflow == value) {
                    return;
                }

                this._overflow = value;
                this.markNeedsPaint();
            }
        }

        public delegate float mainChildSizeGetter(RenderBox child);

        float _getIntrinsicDimension(mainChildSizeGetter getter) {
            float extent = 0.0f;
            RenderBox child = this.firstChild;
            while (child != null) {
                StackParentData childParentData = (StackParentData) child.parentData;
                if (!childParentData.isPositioned) {
                    extent = Mathf.Max(extent, getter(child));
                }

                D.assert(child.parentData == childParentData);
                if (childParentData != null) {
                    child = childParentData.nextSibling;
                }
            }

            return extent;
        }

        protected override float computeMinIntrinsicWidth(float height) {
            return this._getIntrinsicDimension((RenderBox child) => child.getMinIntrinsicWidth(height));
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return this._getIntrinsicDimension((RenderBox child) => child.getMaxIntrinsicWidth(height));
        }

        protected override float computeMinIntrinsicHeight(float width) {
            return this._getIntrinsicDimension((RenderBox child) => child.getMinIntrinsicHeight(width));
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return this._getIntrinsicDimension((RenderBox child) => child.getMaxIntrinsicHeight(width));
        }

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            return this.defaultComputeDistanceToHighestActualBaseline(baseline);
        }

        protected override void performLayout() {
            this._hasVisualOverflow = false;
            bool hasNonPositionedChildren = false;
            if (this.childCount == 0) {
                this.size = this.constraints.biggest;
                return;
            }

            float width = this.constraints.minWidth;
            float height = this.constraints.minHeight;

            BoxConstraints nonPositionedConstraints = null;
            switch (this.fit) {
                case StackFit.loose:
                    nonPositionedConstraints = this.constraints.loosen();
                    break;
                case StackFit.expand:
                    nonPositionedConstraints = BoxConstraints.tight(this.constraints.biggest);
                    break;
                case StackFit.passthrough:
                    nonPositionedConstraints = this.constraints;
                    break;
            }


            RenderBox child = this.firstChild;
            while (child != null) {
                StackParentData childParentData = (StackParentData) child.parentData;

                if (!childParentData.isPositioned) {
                    hasNonPositionedChildren = true;

                    child.layout(nonPositionedConstraints, parentUsesSize: true);

                    Size childSize = child.size;
                    width = Mathf.Max(width, childSize.width);
                    height = Mathf.Max(height, childSize.height);
                }

                child = childParentData.nextSibling;
            }

            if (hasNonPositionedChildren) {
                this.size = new Size(width, height);
                D.assert(this.size.width == this.constraints.constrainWidth(width));
                D.assert(this.size.height == this.constraints.constrainHeight(height));
            }
            else {
                this.size = this.constraints.biggest;
            }

            child = this.firstChild;
            while (child != null) {
                StackParentData childParentData = (StackParentData) child.parentData;

                if (!childParentData.isPositioned) {
                    childParentData.offset = this._alignment.alongOffset(this.size - child.size);
                }
                else {
                    BoxConstraints childConstraints = new BoxConstraints();

                    if (childParentData.left != null && childParentData.right != null) {
                        childConstraints =
                            childConstraints.tighten(
                                width: this.size.width - childParentData.right - childParentData.left);
                    }
                    else if (childParentData.width != null) {
                        childConstraints = childConstraints.tighten(width: childParentData.width);
                    }

                    if (childParentData.top != null && childParentData.bottom != null) {
                        childConstraints =
                            childConstraints.tighten(
                                height: this.size.height - childParentData.bottom - childParentData.top);
                    }
                    else if (childParentData.height != null) {
                        childConstraints = childConstraints.tighten(height: childParentData.height);
                    }

                    child.layout(childConstraints, parentUsesSize: true);

                    float x;
                    if (childParentData.left != null) {
                        x = childParentData.left.Value;
                    }
                    else if (childParentData.right != null) {
                        x = this.size.width - childParentData.right.Value - child.size.width;
                    }
                    else {
                        x = this._alignment.alongOffset(this.size - child.size).dx;
                    }

                    if (x < 0.0 || x + child.size.width > this.size.width) {
                        this._hasVisualOverflow = true;
                    }

                    float y;
                    if (childParentData.top != null) {
                        y = childParentData.top.Value;
                    }
                    else if (childParentData.bottom != null) {
                        y = this.size.height - childParentData.bottom.Value - child.size.height;
                    }
                    else {
                        y = this._alignment.alongOffset(this.size - child.size).dy;
                    }

                    if (y < 0.0 || y + child.size.height > this.size.height) {
                        this._hasVisualOverflow = true;
                    }

                    childParentData.offset = new Offset(x, y);
                }

                D.assert(child.parentData == childParentData);
                child = childParentData.nextSibling;
            }
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position = null) {
            return this.defaultHitTestChildren(result, position: position);
        }

        public virtual void paintStack(PaintingContext context, Offset offset) {
            this.defaultPaint(context, offset);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this._overflow == Overflow.clip && this._hasVisualOverflow) {
                context.pushClipRect(this.needsCompositing, offset, Offset.zero & this.size, this.paintStack);
            }
            else {
                this.paintStack(context, offset);
            }
        }

        public override Rect describeApproximatePaintClip(RenderObject childRaw) {
            return this._hasVisualOverflow ? Offset.zero & this.size : null;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Alignment>("alignment", this.alignment));
            properties.add(new EnumProperty<StackFit>("fit", this.fit));
            properties.add(new EnumProperty<Overflow>("overflow", this.overflow));
        }
    }

    class RenderIndexedStack : RenderStack {
        public RenderIndexedStack(
            List<RenderBox> children = null,
            Alignment alignment = null,
            int? index = 0
        ) : base(fit: null, overflow: null, children: children, alignment: alignment ?? Alignment.topLeft) {
            this._index = index;
        }

        public int? index {
            get { return this._index; }
            set {
                if (this._index != value) {
                    this._index = value;
                    this.markNeedsLayout();
                }
            }
        }

        int? _index;

        RenderBox _childAtIndex() {
            D.assert(this.index != null);
            RenderBox child = this.firstChild;
            int i = 0;
            while (child != null && i < this.index) {
                StackParentData childParentData = (StackParentData) child.parentData;
                child = childParentData.nextSibling;
                i += 1;
            }

            D.assert(i == this.index);
            D.assert(child != null);
            return child;
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position) {
            if (this.firstChild == null || this.index == null) {
                return false;
            }

            D.assert(position != null);
            RenderBox child = this._childAtIndex();
            StackParentData childParentData = (StackParentData) child.parentData;
            return child.hitTest(result, position: position - childParentData.offset);
        }

        public override void paintStack(PaintingContext context, Offset offset) {
            if (this.firstChild == null || this.index == null) {
                return;
            }

            RenderBox child = this._childAtIndex();
            StackParentData childParentData = (StackParentData) child.parentData;
            context.paintChild(child, childParentData.offset + offset);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new IntProperty("index", this.index));
        }
    }
}