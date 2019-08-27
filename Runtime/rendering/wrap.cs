using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.rendering {
    public enum WrapAlignment {
        start,
        end,
        center,
        spaceBetween,
        spaceAround,
        spaceEvenly
    }

    public enum WrapCrossAlignment {
        start,
        end,
        center
    }

    class _RunMetrics {
        public _RunMetrics(float mainAxisExtent, float crossAxisExtent, int childCount) {
            this.mainAxisExtent = mainAxisExtent;
            this.crossAxisExtent = crossAxisExtent;
            this.childCount = childCount;
        }

        public readonly float mainAxisExtent;
        public readonly float crossAxisExtent;
        public readonly int childCount;
    }

    public class WrapParentData : ContainerBoxParentData<RenderBox> {
        public int _runIndex = 0;
    }

    public class RenderWrap : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<RenderBox, WrapParentData> {
        public RenderWrap(
            List<RenderBox> children = null,
            Axis direction = Axis.horizontal,
            WrapAlignment alignment = WrapAlignment.start,
            float spacing = 0.0f,
            WrapAlignment runAlignment = WrapAlignment.start,
            float runSpacing = 0.0f,
            WrapCrossAlignment crossAxisAlignment = WrapCrossAlignment.start,
            TextDirection? textDirection = null,
            VerticalDirection verticalDirection = VerticalDirection.down
        ) {
            this._direction = direction;
            this._alignment = alignment;
            this._spacing = spacing;
            this._runAlignment = runAlignment;
            this._runSpacing = runSpacing;
            this._crossAxisAlignment = crossAxisAlignment;
            this._textDirection = textDirection;
            this._verticalDirection = verticalDirection;

            this.addAll(children);
        }

        Axis _direction;

        public Axis direction {
            get { return this._direction; }
            set {
                if (this._direction == value) {
                    return;
                }

                this._direction = value;
                this.markNeedsLayout();
            }
        }

        WrapAlignment _alignment;

        public WrapAlignment alignment {
            get { return this._alignment; }
            set {
                if (this._alignment == value) {
                    return;
                }

                this._alignment = value;
                this.markNeedsLayout();
            }
        }

        float _spacing;

        public float spacing {
            get { return this._spacing; }
            set {
                if (this._spacing == value) {
                    return;
                }

                this._spacing = value;
                this.markNeedsLayout();
            }
        }

        WrapAlignment _runAlignment;

        public WrapAlignment runAlignment {
            get { return this._runAlignment; }
            set {
                if (this._runAlignment == value) {
                    return;
                }

                this._runAlignment = value;
                this.markNeedsLayout();
            }
        }

        float _runSpacing;

        public float runSpacing {
            get { return this._runSpacing; }
            set {
                if (this._runSpacing == value) {
                    return;
                }

                this._runSpacing = value;
                this.markNeedsLayout();
            }
        }

        WrapCrossAlignment _crossAxisAlignment;

        public WrapCrossAlignment crossAxisAlignment {
            get { return this._crossAxisAlignment; }
            set {
                if (this._crossAxisAlignment == value) {
                    return;
                }

                this._crossAxisAlignment = value;
                this.markNeedsLayout();
            }
        }

        TextDirection? _textDirection;

        public TextDirection? textDirection {
            get { return this._textDirection; }
            set {
                if (this._textDirection != value) {
                    this._textDirection = value;
                    this.markNeedsLayout();
                }
            }
        }

        VerticalDirection _verticalDirection;

        public VerticalDirection verticalDirection {
            get { return this._verticalDirection; }
            set {
                if (this._verticalDirection != value) {
                    this._verticalDirection = value;
                    this.markNeedsLayout();
                }
            }
        }

        bool _debugHasNecessaryDirections {
            get {
                if (this.firstChild != null && this.lastChild != this.firstChild) {
                    // i.e. there"s more than one child
                    switch (this.direction) {
                        case Axis.horizontal:
                            D.assert(this.textDirection != null,
                                () => $"Horizontal {this.GetType()} with multiple children has a null textDirection, so the layout order is undefined.");
                            break;
                        case Axis.vertical:
                            break;
                    }
                }

                if (this.alignment == WrapAlignment.start || this.alignment == WrapAlignment.end) {
                    switch (this.direction) {
                        case Axis.horizontal:
                            D.assert(this.textDirection != null,
                                () => $"Horizontal {this.GetType()} with alignment {this.alignment} has a null textDirection, so the alignment cannot be resolved.");
                            break;
                        case Axis.vertical:
                            break;
                    }
                }

                if (this.runAlignment == WrapAlignment.start || this.runAlignment == WrapAlignment.end) {
                    switch (this.direction) {
                        case Axis.horizontal:
                            break;
                        case Axis.vertical:
                            D.assert(this.textDirection != null,
                                () => $"Vertical {this.GetType()} with runAlignment {this.runAlignment} has a null textDirection, so the alignment cannot be resolved.");
                            break;
                    }
                }

                if (this.crossAxisAlignment == WrapCrossAlignment.start ||
                    this.crossAxisAlignment == WrapCrossAlignment.end) {
                    switch (this.direction) {
                        case Axis.horizontal:
                            break;
                        case Axis.vertical:
                            D.assert(this.textDirection != null,
                                () => $"Vertical {this.GetType()} with crossAxisAlignment {this.crossAxisAlignment} has a null textDirection, so the alignment cannot be resolved.");
                            break;
                    }
                }

                return true;
            }
        }

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is WrapParentData)) {
                child.parentData = new WrapParentData();
            }
        }

        float _computeIntrinsicHeightForWidth(float width) {
            D.assert(this.direction == Axis.horizontal);
            int runCount = 0;
            float height = 0.0f;
            float runWidth = 0.0f;
            float runHeight = 0.0f;
            int childCount = 0;
            RenderBox child = this.firstChild;
            while (child != null) {
                float childWidth = child.getMaxIntrinsicWidth(float.PositiveInfinity);
                float childHeight = child.getMaxIntrinsicHeight(childWidth);
                if (runWidth + childWidth > width) {
                    height += runHeight;
                    if (runCount > 0) {
                        height += this.runSpacing;
                    }

                    runCount += 1;
                    runWidth = 0.0f;
                    runHeight = 0.0f;
                    childCount = 0;
                }

                runWidth += childWidth;
                runHeight = Mathf.Max(runHeight, childHeight);
                if (childCount > 0) {
                    runWidth += this.spacing;
                }

                childCount += 1;
                child = this.childAfter(child);
            }

            if (childCount > 0) {
                height += runHeight + this.runSpacing;
            }

            return height;
        }

        float _computeIntrinsicWidthForHeight(float height) {
            D.assert(this.direction == Axis.vertical);
            int runCount = 0;
            float width = 0.0f;
            float runHeight = 0.0f;
            float runWidth = 0.0f;
            int childCount = 0;
            RenderBox child = this.firstChild;
            while (child != null) {
                float childHeight = child.getMaxIntrinsicHeight(float.PositiveInfinity);
                float childWidth = child.getMaxIntrinsicWidth(childHeight);
                if (runHeight + childHeight > height) {
                    width += runWidth;
                    if (runCount > 0) {
                        width += this.runSpacing;
                    }

                    runCount += 1;
                    runHeight = 0.0f;
                    runWidth = 0.0f;
                    childCount = 0;
                }

                runHeight += childHeight;
                runWidth = Mathf.Max(runWidth, childWidth);
                if (childCount > 0) {
                    runHeight += this.spacing;
                }

                childCount += 1;
                child = this.childAfter(child);
            }

            if (childCount > 0) {
                width += runWidth + this.runSpacing;
            }

            return width;
        }

        protected override float computeMinIntrinsicWidth(float height) {
            switch (this.direction) {
                case Axis.horizontal:
                    float width = 0.0f;
                    RenderBox child = this.firstChild;
                    while (child != null) {
                        width = Mathf.Max(width, child.getMinIntrinsicWidth(float.PositiveInfinity));
                        child = this.childAfter(child);
                    }

                    return width;
                case Axis.vertical:
                    return this._computeIntrinsicWidthForHeight(height);
            }

            throw new Exception("Unknown axis: " + this.direction);
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            switch (this.direction) {
                case Axis.horizontal:
                    float width = 0.0f;
                    RenderBox child = this.firstChild;
                    while (child != null) {
                        width += child.getMaxIntrinsicWidth(float.PositiveInfinity);
                        child = this.childAfter(child);
                    }

                    return width;
                case Axis.vertical:
                    return this._computeIntrinsicWidthForHeight(height);
            }

            throw new Exception("Unknown axis: " + this.direction);
        }

        protected override float computeMinIntrinsicHeight(float width) {
            switch (this.direction) {
                case Axis.horizontal:
                    return this._computeIntrinsicHeightForWidth(width);
                case Axis.vertical:
                    float height = 0.0f;
                    RenderBox child = this.firstChild;
                    while (child != null) {
                        height = Mathf.Max(height, child.getMinIntrinsicHeight(float.PositiveInfinity));
                        child = this.childAfter(child);
                    }

                    return height;
            }

            throw new Exception("Unknown axis: " + this.direction);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            switch (this.direction) {
                case Axis.horizontal:
                    return this._computeIntrinsicHeightForWidth(width);
                case Axis.vertical:
                    float height = 0.0f;
                    RenderBox child = this.firstChild;
                    while (child != null) {
                        height += child.getMaxIntrinsicHeight(float.PositiveInfinity);
                        child = this.childAfter(child);
                    }

                    return height;
            }

            throw new Exception("Unknown axis: " + this.direction);
        }

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            return this.defaultComputeDistanceToHighestActualBaseline(baseline);
        }

        float _getMainAxisExtent(RenderBox child) {
            switch (this.direction) {
                case Axis.horizontal:
                    return child.size.width;
                case Axis.vertical:
                    return child.size.height;
            }

            return 0.0f;
        }

        float _getCrossAxisExtent(RenderBox child) {
            switch (this.direction) {
                case Axis.horizontal:
                    return child.size.height;
                case Axis.vertical:
                    return child.size.width;
            }

            return 0.0f;
        }

        Offset _getOffset(float mainAxisOffset, float crossAxisOffset) {
            switch (this.direction) {
                case Axis.horizontal:
                    return new Offset(mainAxisOffset, crossAxisOffset);
                case Axis.vertical:
                    return new Offset(crossAxisOffset, mainAxisOffset);
            }

            return Offset.zero;
        }

        float _getChildCrossAxisOffset(bool flipCrossAxis, float runCrossAxisExtent, float childCrossAxisExtent) {
            float freeSpace = runCrossAxisExtent - childCrossAxisExtent;
            switch (this.crossAxisAlignment) {
                case WrapCrossAlignment.start:
                    return flipCrossAxis ? freeSpace : 0.0f;
                case WrapCrossAlignment.end:
                    return flipCrossAxis ? 0.0f : freeSpace;
                case WrapCrossAlignment.center:
                    return freeSpace / 2.0f;
            }

            return 0.0f;
        }

        bool _hasVisualOverflow = false;

        protected override void performLayout() {
            D.assert(this._debugHasNecessaryDirections);
            this._hasVisualOverflow = false;
            RenderBox child = this.firstChild;
            if (child == null) {
                this.size = this.constraints.smallest;
                return;
            }

            BoxConstraints childConstraints;
            float mainAxisLimit = 0.0f;
            bool flipMainAxis = false;
            bool flipCrossAxis = false;
            switch (this.direction) {
                case Axis.horizontal:
                    childConstraints = new BoxConstraints(maxWidth: this.constraints.maxWidth);
                    mainAxisLimit = this.constraints.maxWidth;
                    if (this.textDirection == TextDirection.rtl) {
                        flipMainAxis = true;
                    }

                    if (this.verticalDirection == VerticalDirection.up) {
                        flipCrossAxis = true;
                    }

                    break;
                case Axis.vertical:
                    childConstraints = new BoxConstraints(maxHeight: this.constraints.maxHeight);
                    mainAxisLimit = this.constraints.maxHeight;
                    if (this.verticalDirection == VerticalDirection.up) {
                        flipMainAxis = true;
                    }

                    if (this.textDirection == TextDirection.rtl) {
                        flipCrossAxis = true;
                    }

                    break;
                default:
                    throw new Exception("Unknown axis: " + this.direction);
            }

            D.assert(childConstraints != null);
            float spacing = this.spacing;
            float runSpacing = this.runSpacing;
            List<_RunMetrics> runMetrics = new List<_RunMetrics> { };
            float mainAxisExtent = 0.0f;
            float crossAxisExtent = 0.0f;
            float runMainAxisExtent = 0.0f;
            float runCrossAxisExtent = 0.0f;
            int childCount = 0;
            while (child != null) {
                child.layout(childConstraints, parentUsesSize: true);
                float childMainAxisExtent = this._getMainAxisExtent(child);
                float childCrossAxisExtent = this._getCrossAxisExtent(child);
                if (childCount > 0 && runMainAxisExtent + spacing + childMainAxisExtent > mainAxisLimit) {
                    mainAxisExtent = Mathf.Max(mainAxisExtent, runMainAxisExtent);
                    crossAxisExtent += runCrossAxisExtent;
                    if (runMetrics.isNotEmpty()) {
                        crossAxisExtent += runSpacing;
                    }

                    runMetrics.Add(new _RunMetrics(runMainAxisExtent, runCrossAxisExtent, childCount));
                    runMainAxisExtent = 0.0f;
                    runCrossAxisExtent = 0.0f;
                    childCount = 0;
                }

                runMainAxisExtent += childMainAxisExtent;
                if (childCount > 0) {
                    runMainAxisExtent += spacing;
                }

                runCrossAxisExtent = Mathf.Max(runCrossAxisExtent, childCrossAxisExtent);
                childCount += 1;
                D.assert(child.parentData is WrapParentData);
                WrapParentData childParentData = child.parentData as WrapParentData;
                childParentData._runIndex = runMetrics.Count;
                child = childParentData.nextSibling;
            }

            if (childCount > 0) {
                mainAxisExtent = Mathf.Max(mainAxisExtent, runMainAxisExtent);
                crossAxisExtent += runCrossAxisExtent;
                if (runMetrics.isNotEmpty()) {
                    crossAxisExtent += runSpacing;
                }

                runMetrics.Add(new _RunMetrics(runMainAxisExtent, runCrossAxisExtent, childCount));
            }

            int runCount = runMetrics.Count;
            D.assert(runCount > 0);

            float containerMainAxisExtent = 0.0f;
            float containerCrossAxisExtent = 0.0f;

            switch (this.direction) {
                case Axis.horizontal:
                    this.size = this.constraints.constrain(new Size(mainAxisExtent, crossAxisExtent));
                    containerMainAxisExtent = this.size.width;
                    containerCrossAxisExtent = this.size.height;
                    break;
                case Axis.vertical:
                    this.size = this.constraints.constrain(new Size(crossAxisExtent, mainAxisExtent));
                    containerMainAxisExtent = this.size.height;
                    containerCrossAxisExtent = this.size.width;
                    break;
            }

            this._hasVisualOverflow =
                containerMainAxisExtent < mainAxisExtent || containerCrossAxisExtent < crossAxisExtent;

            float crossAxisFreeSpace = Mathf.Max(0.0f, containerCrossAxisExtent - crossAxisExtent);
            float runLeadingSpace = 0.0f;
            float runBetweenSpace = 0.0f;
            switch (this.runAlignment) {
                case WrapAlignment.start:
                    break;
                case WrapAlignment.end:
                    runLeadingSpace = crossAxisFreeSpace;
                    break;
                case WrapAlignment.center:
                    runLeadingSpace = crossAxisFreeSpace / 2.0f;
                    break;
                case WrapAlignment.spaceBetween:
                    runBetweenSpace = runCount > 1 ? crossAxisFreeSpace / (runCount - 1) : 0.0f;
                    break;
                case WrapAlignment.spaceAround:
                    runBetweenSpace = crossAxisFreeSpace / runCount;
                    runLeadingSpace = runBetweenSpace / 2.0f;
                    break;
                case WrapAlignment.spaceEvenly:
                    runBetweenSpace = crossAxisFreeSpace / (runCount + 1);
                    runLeadingSpace = runBetweenSpace;
                    break;
            }

            runBetweenSpace += runSpacing;
            float crossAxisOffset = flipCrossAxis ? containerCrossAxisExtent - runLeadingSpace : runLeadingSpace;

            child = this.firstChild;
            for (int i = 0; i < runCount; ++i) {
                _RunMetrics metrics = runMetrics[i];
                runMainAxisExtent = metrics.mainAxisExtent;
                runCrossAxisExtent = metrics.crossAxisExtent;
                childCount = metrics.childCount;

                float mainAxisFreeSpace = Mathf.Max(0.0f, containerMainAxisExtent - runMainAxisExtent);
                float childLeadingSpace = 0.0f;
                float childBetweenSpace = 0.0f;

                switch (this.alignment) {
                    case WrapAlignment.start:
                        break;
                    case WrapAlignment.end:
                        childLeadingSpace = mainAxisFreeSpace;
                        break;
                    case WrapAlignment.center:
                        childLeadingSpace = mainAxisFreeSpace / 2.0f;
                        break;
                    case WrapAlignment.spaceBetween:
                        childBetweenSpace = childCount > 1 ? mainAxisFreeSpace / (childCount - 1) : 0.0f;
                        break;
                    case WrapAlignment.spaceAround:
                        childBetweenSpace = mainAxisFreeSpace / childCount;
                        childLeadingSpace = childBetweenSpace / 2.0f;
                        break;
                    case WrapAlignment.spaceEvenly:
                        childBetweenSpace = mainAxisFreeSpace / (childCount + 1);
                        childLeadingSpace = childBetweenSpace;
                        break;
                }

                childBetweenSpace += spacing;
                float childMainPosition =
                    flipMainAxis ? containerMainAxisExtent - childLeadingSpace : childLeadingSpace;

                if (flipCrossAxis) {
                    crossAxisOffset -= runCrossAxisExtent;
                }

                while (child != null) {
                    D.assert(child.parentData is WrapParentData);
                    WrapParentData childParentData = child.parentData as WrapParentData;
                    if (childParentData._runIndex != i) {
                        break;
                    }

                    float childMainAxisExtent = this._getMainAxisExtent(child);
                    float childCrossAxisExtent = this._getCrossAxisExtent(child);
                    float childCrossAxisOffset =
                        this._getChildCrossAxisOffset(flipCrossAxis, runCrossAxisExtent, childCrossAxisExtent);
                    if (flipMainAxis) {
                        childMainPosition -= childMainAxisExtent;
                    }

                    childParentData.offset = this._getOffset(childMainPosition, crossAxisOffset + childCrossAxisOffset);
                    if (flipMainAxis) {
                        childMainPosition -= childBetweenSpace;
                    }
                    else {
                        childMainPosition += childMainAxisExtent + childBetweenSpace;
                    }

                    child = childParentData.nextSibling;
                }

                if (flipCrossAxis) {
                    crossAxisOffset -= runBetweenSpace;
                }
                else {
                    crossAxisOffset += runCrossAxisExtent + runBetweenSpace;
                }
            }
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position = null) {
            return this.defaultHitTestChildren(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this._hasVisualOverflow) {
                context.pushClipRect(this.needsCompositing, offset, Offset.zero & this.size, this.defaultPaint);
            }
            else {
                this.defaultPaint(context, offset);
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<Axis>("direction", this.direction));
            properties.add(new EnumProperty<WrapAlignment>("alignment", this.alignment));
            properties.add(new FloatProperty("spacing", this.spacing));
            properties.add(new EnumProperty<WrapAlignment>("runAlignment", this.runAlignment));
            properties.add(new FloatProperty("runSpacing", this.runSpacing));
            properties.add(new FloatProperty("crossAxisAlignment", this.runSpacing));
            properties.add(new EnumProperty<TextDirection?>("textDirection", this.textDirection, defaultValue: null));
            properties.add(new EnumProperty<VerticalDirection>("verticalDirection", this.verticalDirection,
                defaultValue: VerticalDirection.down));
        }
    }
}