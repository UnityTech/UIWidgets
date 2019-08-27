using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.rendering {
    public class ListBodyParentData : ContainerParentDataMixinBoxParentData<RenderBox> {
    }

    delegate float __ChildSizingFunction(RenderBox child);


    public class RenderListBody : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<RenderBox,
        ListBodyParentData> {
        public RenderListBody(
            List<RenderBox> children = null,
            AxisDirection axisDirection = AxisDirection.down) {
            this._axisDirection = axisDirection;
            this.addAll(children);
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is ListBodyParentData)) {
                child.parentData = new ListBodyParentData();
            }
        }

        public AxisDirection axisDirection {
            get { return this._axisDirection; }
            set {
                if (this._axisDirection == value) {
                    return;
                }

                this._axisDirection = value;
                this.markNeedsLayout();
            }
        }

        AxisDirection _axisDirection;

        public Axis mainAxis {
            get { return AxisUtils.axisDirectionToAxis(this.axisDirection); }
        }

        protected override void performLayout() {
            D.assert(() => {
                switch (this.mainAxis) {
                    case Axis.horizontal:
                        if (!this.constraints.hasBoundedWidth) {
                            return true;
                        }

                        break;
                    case Axis.vertical:
                        if (!this.constraints.hasBoundedHeight) {
                            return true;
                        }

                        break;
                }

                throw new UIWidgetsError(
                    "RenderListBody must have unlimited space along its main axis.\n" +
                    "RenderListBody does not clip or resize its children, so it must be " +
                    "placed in a parent that does not constrain the main " +
                    "axis. You probably want to put the RenderListBody inside a " +
                    "RenderViewport with a matching main axis.");
            });

            D.assert(() => {
                switch (this.mainAxis) {
                    case Axis.horizontal:
                        if (this.constraints.hasBoundedHeight) {
                            return true;
                        }

                        break;
                    case Axis.vertical:
                        if (this.constraints.hasBoundedWidth) {
                            return true;
                        }

                        break;
                }

                throw new UIWidgetsError(
                    "RenderListBody must have a bounded constraint for its cross axis.\n" +
                    "RenderListBody forces its children to expand to fit the RenderListBody\"s container, " +
                    "so it must be placed in a parent that constrains the cross " +
                    "axis to a finite dimension. If you are attempting to nest a RenderListBody with " +
                    "one direction inside one of another direction, you will want to " +
                    "wrap the inner one inside a box that fixes the dimension in that direction, " +
                    "for example, a RenderIntrinsicWidth or RenderIntrinsicHeight object. " +
                    "This is relatively expensive, however."
                );
            });

            float mainAxisExtent = 0.0f;
            RenderBox child = this.firstChild;

            BoxConstraints innerConstraints;
            float position;

            switch (this.axisDirection) {
                case AxisDirection.right:
                    innerConstraints = BoxConstraints.tightFor(height: this.constraints.maxHeight);
                    while (child != null) {
                        child.layout(innerConstraints, parentUsesSize: true);
                        ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                        childParentData.offset = new Offset(mainAxisExtent, 0.0f);
                        mainAxisExtent += child.size.width;
                        D.assert(child.parentData == childParentData);
                        child = childParentData.nextSibling;
                    }

                    this.size = this.constraints.constrain(new Size(mainAxisExtent,
                        this.constraints.maxHeight));
                    break;
                case AxisDirection.left:
                    innerConstraints = BoxConstraints.tightFor(height: this.constraints.maxHeight);
                    while (child != null) {
                        child.layout(innerConstraints, parentUsesSize: true);
                        ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                        mainAxisExtent += child.size.width;
                        D.assert(child.parentData == childParentData);
                        child = childParentData.nextSibling;
                    }

                    position = 0.0f;
                    child = this.firstChild;
                    while (child != null) {
                        ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                        position += child.size.width;
                        childParentData.offset = new Offset((mainAxisExtent - position), 0.0f);
                        D.assert(child.parentData == childParentData);
                        child = childParentData.nextSibling;
                    }

                    this.size = this.constraints.constrain(new Size(mainAxisExtent,
                        this.constraints.maxHeight));
                    break;
                case AxisDirection.down:
                    innerConstraints = BoxConstraints.tightFor(width: this.constraints.maxWidth);
                    while (child != null) {
                        child.layout(innerConstraints, parentUsesSize: true);
                        ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                        childParentData.offset = new Offset(0.0f, mainAxisExtent);
                        mainAxisExtent += child.size.height;
                        D.assert(child.parentData == childParentData);
                        child = childParentData.nextSibling;
                    }

                    this.size = this.constraints.constrain(new Size(this.constraints.maxWidth, mainAxisExtent));
                    break;
                case AxisDirection.up:
                    innerConstraints = BoxConstraints.tightFor(width: this.constraints.maxWidth);
                    while (child != null) {
                        child.layout(innerConstraints, parentUsesSize: true);
                        ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                        mainAxisExtent += child.size.height;
                        D.assert(child.parentData == childParentData);
                        child = childParentData.nextSibling;
                    }

                    position = 0.0f;
                    child = this.firstChild;
                    while (child != null) {
                        ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                        position += child.size.height;
                        childParentData.offset = new Offset(0.0f, mainAxisExtent - position);
                        D.assert(child.parentData == childParentData);
                        child = childParentData.nextSibling;
                    }

                    this.size = this.constraints.constrain(new Size(this.constraints.maxWidth, mainAxisExtent));
                    break;
            }

            D.assert(this.size.isFinite);
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<AxisDirection>("axisDirection", this.axisDirection));
        }

        float _getIntrinsicCrossAxis(__ChildSizingFunction childSize) {
            float extent = 0.0f;
            RenderBox child = this.firstChild;
            while (child != null) {
                extent = Mathf.Max(extent, childSize(child));
                ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                child = childParentData.nextSibling;
            }

            return extent;
        }

        float _getIntrinsicMainAxis(__ChildSizingFunction childSize) {
            float extent = 0.0f;
            RenderBox child = this.firstChild;
            while (child != null) {
                extent += childSize(child);
                ListBodyParentData childParentData = (ListBodyParentData) child.parentData;
                child = childParentData.nextSibling;
            }

            return extent;
        }


        protected override float computeMinIntrinsicWidth(float height) {
            switch (this.mainAxis) {
                case Axis.horizontal:
                    return this._getIntrinsicMainAxis((RenderBox child) => child.getMinIntrinsicWidth(height));
                case Axis.vertical:
                    return this._getIntrinsicCrossAxis((RenderBox child) => child.getMinIntrinsicWidth(height));
            }

            D.assert(false);
            return 0.0f;
        }


        protected override float computeMaxIntrinsicWidth(float height) {
            switch (this.mainAxis) {
                case Axis.horizontal:
                    return this._getIntrinsicMainAxis((RenderBox child) => child.getMaxIntrinsicWidth(height));
                case Axis.vertical:
                    return this._getIntrinsicCrossAxis((RenderBox child) => child.getMaxIntrinsicWidth(height));
            }

            D.assert(false);
            return 0.0f;
        }


        protected override float computeMinIntrinsicHeight(float width) {
            switch (this.mainAxis) {
                case Axis.horizontal:
                    return this._getIntrinsicMainAxis((RenderBox child) => child.getMinIntrinsicHeight(width));
                case Axis.vertical:
                    return this._getIntrinsicCrossAxis((RenderBox child) => child.getMinIntrinsicHeight(width));
            }

            D.assert(false);
            return 0.0f;
        }


        protected internal override float computeMaxIntrinsicHeight(float width) {
            switch (this.mainAxis) {
                case Axis.horizontal:
                    return this._getIntrinsicMainAxis((RenderBox child) => child.getMaxIntrinsicHeight(width));
                case Axis.vertical:
                    return this._getIntrinsicCrossAxis((RenderBox child) => child.getMaxIntrinsicHeight(width));
            }

            D.assert(false);
            return 0.0f;
        }


        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            return this.defaultComputeDistanceToFirstActualBaseline(baseline);
        }


        public override void paint(PaintingContext context, Offset offset) {
            this.defaultPaint(context, offset);
        }


        protected override bool hitTestChildren(HitTestResult result, Offset position = null) {
            return this.defaultHitTestChildren(result, position: position);
        }
    }
}