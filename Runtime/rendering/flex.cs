using System;
using System.Collections.Generic;
using UIWidgets.Runtime.rendering;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public enum FlexFit {
        tight,
        loose,
    }

    public class FlexParentData : ContainerParentDataMixinBoxParentData<RenderBox> {
        public int flex;

        public FlexFit fit;
    }

    public enum MainAxisSize {
        min,
        max,
    }

    public enum MainAxisAlignment {
        start,
        end,
        center,
        spaceBetween,
        spaceAround,
        spaceEvenly,
    }

    public enum CrossAxisAlignment {
        start,
        end,
        center,
        stretch,
        baseline,
    }

    public delegate float _ChildSizingFunction(RenderBox child, float extent);

    public class RenderFlex : RenderBoxContainerDefaultsMixinContainerRenderObjectMixinRenderBox<RenderBox,
        FlexParentData> {
        public RenderFlex(
            List<RenderBox> children = null,
            Axis direction = Axis.horizontal,
            MainAxisSize mainAxisSize = MainAxisSize.max,
            MainAxisAlignment mainAxisAlignment = MainAxisAlignment.start,
            CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.center,
            TextDirection textDirection = TextDirection.ltr,
            VerticalDirection verticalDirection = VerticalDirection.down,
            TextBaseline textBaseline = TextBaseline.alphabetic
        ) {
            this._direction = direction;
            this._mainAxisAlignment = mainAxisAlignment;
            this._mainAxisSize = mainAxisSize;
            this._crossAxisAlignment = crossAxisAlignment;
            this._textDirection = textDirection;
            this._verticalDirection = verticalDirection;
            this._textBaseline = textBaseline;

            this.addAll(children);
        }

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

        public Axis _direction;

        public MainAxisSize mainAxisSize {
            get { return this._mainAxisSize; }
            set {
                if (this._mainAxisSize == value) {
                    return;
                }

                this._mainAxisSize = value;
                this.markNeedsLayout();
            }
        }

        public MainAxisSize _mainAxisSize;

        public MainAxisAlignment mainAxisAlignment {
            get { return this._mainAxisAlignment; }
            set {
                if (this._mainAxisAlignment == value) {
                    return;
                }

                this._mainAxisAlignment = value;
                this.markNeedsLayout();
            }
        }

        public MainAxisAlignment _mainAxisAlignment;

        public CrossAxisAlignment crossAxisAlignment {
            get { return this._crossAxisAlignment; }
            set {
                if (this._crossAxisAlignment == value) {
                    return;
                }

                this._crossAxisAlignment = value;
                this.markNeedsLayout();
            }
        }

        public CrossAxisAlignment _crossAxisAlignment;

        public TextDirection textDirection {
            get { return this._textDirection; }
            set {
                if (this._textDirection == value) {
                    return;
                }

                this._textDirection = value;
                this.markNeedsLayout();
            }
        }

        public TextDirection _textDirection;

        public VerticalDirection verticalDirection {
            get { return this._verticalDirection; }
            set {
                if (this._verticalDirection == value) {
                    return;
                }

                this._verticalDirection = value;
                this.markNeedsLayout();
            }
        }

        public VerticalDirection _verticalDirection;

        public TextBaseline textBaseline {
            get { return this._textBaseline; }
            set {
                if (this._textBaseline == value) {
                    return;
                }

                this._textBaseline = value;
                this.markNeedsLayout();
            }
        }

        public TextBaseline _textBaseline;

        public float _overflow;

        public override void setupParentData(RenderObject child) {
            if (!(child.parentData is FlexParentData)) {
                child.parentData = new FlexParentData();
            }
        }

        public float _getIntrinsicSize(
            Axis sizingDirection,
            float extent,
            _ChildSizingFunction childSize
        ) {
            if (this._direction == sizingDirection) {
                float totalFlex = 0.0f;
                float inflexibleSpace = 0.0f;
                float maxFlexFractionSoFar = 0.0f;

                RenderBox child = this.firstChild;
                while (child != null) {
                    int flex = this._getFlex(child);
                    totalFlex += flex;
                    if (flex > 0) {
                        float flexFraction = childSize(child, extent) / this._getFlex(child);
                        maxFlexFractionSoFar = Mathf.Max(maxFlexFractionSoFar, flexFraction);
                    }
                    else {
                        inflexibleSpace += childSize(child, extent);
                    }

                    var childParentData = (FlexParentData) child.parentData;
                    child = childParentData.nextSibling;
                }

                return maxFlexFractionSoFar * totalFlex + inflexibleSpace;
            }
            else {
                float availableMainSpace = extent;
                int totalFlex = 0;
                float inflexibleSpace = 0.0f;
                float maxCrossSize = 0.0f;
                RenderBox child = this.firstChild;
                while (child != null) {
                    int flex = this._getFlex(child);
                    totalFlex += flex;
                    if (flex == 0) {
                        float mainSize = 0.0f;
                        float crossSize = 0.0f;

                        switch (this._direction) {
                            case Axis.horizontal:
                                mainSize = child.getMaxIntrinsicWidth(float.PositiveInfinity);
                                crossSize = childSize(child, mainSize);
                                break;
                            case Axis.vertical:
                                mainSize = child.getMaxIntrinsicHeight(float.PositiveInfinity);
                                crossSize = childSize(child, mainSize);
                                break;
                        }

                        inflexibleSpace += mainSize;
                        maxCrossSize = Mathf.Max(maxCrossSize, crossSize);
                    }

                    var childParentData = (FlexParentData) child.parentData;
                    child = childParentData.nextSibling;
                }

                float spacePerFlex = Mathf.Max(0.0f, (availableMainSpace - inflexibleSpace) / totalFlex);

                child = this.firstChild;
                while (child != null) {
                    int flex = this._getFlex(child);
                    if (flex > 0) {
                        maxCrossSize = Mathf.Max(maxCrossSize, childSize(child, spacePerFlex * flex));
                    }

                    var childParentData = (FlexParentData) child.parentData;
                    child = childParentData.nextSibling;
                }

                return maxCrossSize;
            }
        }

        protected override float computeMinIntrinsicWidth(float height) {
            return this._getIntrinsicSize(
                sizingDirection: Axis.horizontal,
                extent: height,
                childSize: (RenderBox child, float extent) => child.getMinIntrinsicWidth(extent)
            );
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return this._getIntrinsicSize(
                sizingDirection: Axis.horizontal,
                extent: height,
                childSize: (RenderBox child, float extent) => child.getMaxIntrinsicWidth(extent)
            );
        }

        protected override float computeMinIntrinsicHeight(float width) {
            return this._getIntrinsicSize(
                sizingDirection: Axis.vertical,
                extent: width,
                childSize: (RenderBox child, float extent) => child.getMinIntrinsicHeight(extent)
            );
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return this._getIntrinsicSize(
                sizingDirection: Axis.vertical,
                extent: width,
                childSize: (RenderBox child, float extent) => child.getMaxIntrinsicHeight(extent)
            );
        }

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            if (this._direction == Axis.horizontal) {
                return this.defaultComputeDistanceToHighestActualBaseline(baseline);
            }

            return this.defaultComputeDistanceToFirstActualBaseline(baseline);
        }

        public int _getFlex(RenderBox child) {
            var childParentData = (FlexParentData) child.parentData;
            return childParentData.flex;
        }

        public FlexFit _getFit(RenderBox child) {
            var childParentData = (FlexParentData) child.parentData;
            return childParentData.fit;
        }

        public float _getCrossSize(RenderBox child) {
            switch (this._direction) {
                case Axis.horizontal:
                    return child.size.height;
                case Axis.vertical:
                    return child.size.width;
            }

            return 0;
        }

        public float _getMainSize(RenderBox child) {
            switch (this._direction) {
                case Axis.horizontal:
                    return child.size.width;
                case Axis.vertical:
                    return child.size.height;
            }

            return 0;
        }

        protected override void performLayout() {
            int totalFlex = 0;
            int totalChildren = 0;
            float maxMainSize = this._direction == Axis.horizontal
                ? this.constraints.maxWidth
                : this.constraints.maxHeight;
            bool canFlex = maxMainSize < float.PositiveInfinity;

            float crossSize = 0.0f;
            float allocatedSize = 0.0f;
            RenderBox child = this.firstChild;
            RenderBox lastFlexChild = null;
            while (child != null) {
                var childParentData = (FlexParentData) child.parentData;
                totalChildren++;
                int flex = this._getFlex(child);
                if (flex > 0) {
                    totalFlex += childParentData.flex;
                    lastFlexChild = child;
                }
                else {
                    BoxConstraints innerConstraints = null;
                    if (this.crossAxisAlignment == CrossAxisAlignment.stretch) {
                        switch (this._direction) {
                            case Axis.horizontal:
                                innerConstraints = new BoxConstraints(
                                    minHeight: this.constraints.maxHeight,
                                    maxHeight: this.constraints.maxHeight);
                                break;
                            case Axis.vertical:
                                innerConstraints = new BoxConstraints(
                                    minWidth: this.constraints.maxWidth,
                                    maxWidth: this.constraints.maxWidth);
                                break;
                        }
                    }
                    else {
                        switch (this._direction) {
                            case Axis.horizontal:
                                innerConstraints = new BoxConstraints(
                                    maxHeight: this.constraints.maxHeight);
                                break;
                            case Axis.vertical:
                                innerConstraints = new BoxConstraints(
                                    maxWidth: this.constraints.maxWidth);
                                break;
                        }
                    }

                    child.layout(innerConstraints, parentUsesSize: true);
                    allocatedSize += this._getMainSize(child);
                    crossSize = Mathf.Max(crossSize, this._getCrossSize(child));
                }

                child = childParentData.nextSibling;
            }

            float freeSpace = Mathf.Max(0.0f, (canFlex ? maxMainSize : 0.0f) - allocatedSize);
            float allocatedFlexSpace = 0.0f;
            float maxBaselineDistance = 0.0f;
            if (totalFlex > 0 || this.crossAxisAlignment == CrossAxisAlignment.baseline) {
                float spacePerFlex = canFlex && totalFlex > 0 ? (freeSpace / totalFlex) : float.NaN;
                child = this.firstChild;
                float maxSizeAboveBaseline = 0;
                float maxSizeBelowBaseline = 0;
                while (child != null) {
                    int flex = this._getFlex(child);
                    if (flex > 0) {
                        float maxChildExtent = canFlex
                            ? (child == lastFlexChild ? (freeSpace - allocatedFlexSpace) : spacePerFlex * flex)
                            : float.PositiveInfinity;
                        float minChildExtent = 0.0f;
                        switch (this._getFit(child)) {
                            case FlexFit.tight:
                                minChildExtent = maxChildExtent;
                                break;
                            case FlexFit.loose:
                                minChildExtent = 0.0f;
                                break;
                        }

                        BoxConstraints innerConstraints = null;
                        if (this.crossAxisAlignment == CrossAxisAlignment.stretch) {
                            switch (this._direction) {
                                case Axis.horizontal:
                                    innerConstraints = new BoxConstraints(
                                        minWidth: minChildExtent,
                                        maxWidth: maxChildExtent,
                                        minHeight: this.constraints.maxHeight,
                                        maxHeight: this.constraints.maxHeight);
                                    break;
                                case Axis.vertical:
                                    innerConstraints = new BoxConstraints(
                                        minWidth: this.constraints.maxWidth,
                                        maxWidth: this.constraints.maxWidth,
                                        minHeight: minChildExtent,
                                        maxHeight: maxChildExtent);
                                    break;
                            }
                        }
                        else {
                            switch (this._direction) {
                                case Axis.horizontal:
                                    innerConstraints = new BoxConstraints(
                                        minWidth: minChildExtent,
                                        maxWidth: maxChildExtent,
                                        maxHeight: this.constraints.maxHeight);
                                    break;
                                case Axis.vertical:
                                    innerConstraints = new BoxConstraints(
                                        maxWidth: this.constraints.maxWidth,
                                        minHeight: minChildExtent,
                                        maxHeight: maxChildExtent);
                                    break;
                            }
                        }

                        child.layout(innerConstraints, parentUsesSize: true);
                        float childSize = this._getMainSize(child);
                        allocatedSize += childSize;
                        allocatedFlexSpace += maxChildExtent;
                        crossSize = Mathf.Max(crossSize, this._getCrossSize(child));
                    }

                    if (this.crossAxisAlignment == CrossAxisAlignment.baseline) {
                        float? distance = child.getDistanceToBaseline(this.textBaseline, onlyReal: true);
                        if (distance != null) {
                            maxBaselineDistance = Mathf.Max(maxBaselineDistance, distance.Value);
                            maxSizeAboveBaseline = Mathf.Max(distance.Value, maxSizeAboveBaseline);
                            maxSizeBelowBaseline = Mathf.Max(child.size.height - distance.Value, maxSizeBelowBaseline);
                            crossSize = maxSizeAboveBaseline + maxSizeBelowBaseline;
                        }
                    }

                    var childParentData = (FlexParentData) child.parentData;
                    child = childParentData.nextSibling;
                }
            }

            float idealSize = canFlex && this.mainAxisSize == MainAxisSize.max ? maxMainSize : allocatedSize;
            float actualSize = 0.0f;
            float actualSizeDelta = 0.0f;
            switch (this._direction) {
                case Axis.horizontal:
                    this.size = this.constraints.constrain(new Size(idealSize, crossSize));
                    actualSize = this.size.width;
                    crossSize = this.size.height;
                    break;
                case Axis.vertical:
                    this.size = this.constraints.constrain(new Size(crossSize, idealSize));
                    actualSize = this.size.height;
                    crossSize = this.size.width;
                    break;
            }

            actualSizeDelta = actualSize - allocatedSize;
            this._overflow = Mathf.Max(0.0f, -actualSizeDelta);

            float remainingSpace = Mathf.Max(0.0f, actualSizeDelta);
            float leadingSpace = 0.0f;
            float betweenSpace = 0.0f;
            bool flipMainAxis = !_startIsTopLeft(this.direction, this.textDirection, this.verticalDirection);
            switch (this._mainAxisAlignment) {
                case MainAxisAlignment.start:
                    leadingSpace = 0.0f;
                    betweenSpace = 0.0f;
                    break;
                case MainAxisAlignment.end:
                    leadingSpace = remainingSpace;
                    betweenSpace = 0.0f;
                    break;
                case MainAxisAlignment.center:
                    leadingSpace = remainingSpace / 2.0f;
                    betweenSpace = 0.0f;
                    break;
                case MainAxisAlignment.spaceBetween:
                    leadingSpace = 0.0f;
                    betweenSpace = totalChildren > 1 ? remainingSpace / (totalChildren - 1) : 0.0f;
                    break;
                case MainAxisAlignment.spaceAround:
                    betweenSpace = totalChildren > 0 ? remainingSpace / totalChildren : 0.0f;
                    leadingSpace = betweenSpace / 2.0f;
                    break;
                case MainAxisAlignment.spaceEvenly:
                    betweenSpace = totalChildren > 0 ? remainingSpace / (totalChildren + 1) : 0.0f;
                    leadingSpace = betweenSpace;
                    break;
            }

            // Position elements
            float childMainPosition = flipMainAxis ? actualSize - leadingSpace : leadingSpace;
            child = this.firstChild;
            while (child != null) {
                var childParentData = (FlexParentData) child.parentData;
                float childCrossPosition = 0.0f;
                switch (this._crossAxisAlignment) {
                    case CrossAxisAlignment.start:
                    case CrossAxisAlignment.end:
                        childCrossPosition =
                            _startIsTopLeft(
                                AxisUtils.flipAxis(this.direction), this.textDirection, this.verticalDirection)
                            == (this._crossAxisAlignment == CrossAxisAlignment.start)
                                ? 0.0f
                                : crossSize - this._getCrossSize(child);
                        break;
                    case CrossAxisAlignment.center:
                        childCrossPosition = crossSize / 2.0f - this._getCrossSize(child) / 2.0f;
                        break;
                    case CrossAxisAlignment.stretch:
                        childCrossPosition = 0.0f;
                        break;
                    case CrossAxisAlignment.baseline:
                        childCrossPosition = 0.0f;
                        if (this._direction == Axis.horizontal) {
                            float? distance = child.getDistanceToBaseline(this.textBaseline, onlyReal: true);
                            if (distance != null) {
                                childCrossPosition = maxBaselineDistance - distance.Value;
                            }
                        }

                        break;
                }

                if (flipMainAxis) {
                    childMainPosition -= this._getMainSize(child);
                }

                switch (this._direction) {
                    case Axis.horizontal:
                        childParentData.offset = new Offset(childMainPosition, childCrossPosition);
                        break;
                    case Axis.vertical:
                        childParentData.offset = new Offset(childCrossPosition, childMainPosition);
                        break;
                }

                if (flipMainAxis) {
                    childMainPosition -= betweenSpace;
                }
                else {
                    childMainPosition += this._getMainSize(child) + betweenSpace;
                }

                child = childParentData.nextSibling;
            }
        }

        static bool _startIsTopLeft(Axis direction, TextDirection textDirection,
            VerticalDirection verticalDirection) {
            switch (direction) {
                case Axis.horizontal:
                    switch (textDirection) {
                        case TextDirection.ltr:
                            return true;
                        case TextDirection.rtl:
                            return false;
                    }

                    break;
                case Axis.vertical:
                    switch (verticalDirection) {
                        case VerticalDirection.down:
                            return true;
                        case VerticalDirection.up:
                            return false;
                    }

                    break;
            }

            return true;
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this._overflow <= 0.0f) {
                this.defaultPaint(context, offset);
                return;
            }

            if (this.size.isEmpty) {
                return;
            }

            context.pushClipRect(this.needsCompositing, offset, Offset.zero & this.size, this.defaultPaint);

            D.assert(() => {
                string debugOverflowHints =
                    $"The overflowing {this.GetType()} has an orientation of {this._direction}.\n" +
                    $"The edge of the {this.GetType()} that is overflowing has been marked " +
                    "in the rendering with a yellow and black striped pattern. This is " +
                    $"usually caused by the contents being too big for the {this.GetType()}. " +
                    "Consider applying a flex factor (e.g. using an Expanded widget) to " +
                    $"force the children of the {this.GetType()} to fit within the available " +
                    "space instead of being sized to their natural size.\n" +
                    "This is considered an error condition because it indicates that there " +
                    "is content that cannot be seen. If the content is legitimately bigger " +
                    "than the available space, consider clipping it with a ClipRect widget " +
                    "before putting it in the flex, or using a scrollable container rather " +
                    "than a Flex, like a ListView.";

                Rect overflowChildRect;
                switch (this._direction) {
                    case Axis.horizontal:
                        overflowChildRect = Rect.fromLTWH(0.0f, 0.0f, this.size.width + this._overflow, 0.0f);
                        break;
                    case Axis.vertical:
                        overflowChildRect = Rect.fromLTWH(0.0f, 0.0f, 0.0f, this.size.height + this._overflow);
                        break;
                    default:
                        throw new Exception("Unknown direction: " + this._direction);
                }

                DebugOverflowIndicatorMixin.paintOverflowIndicator(this, context, offset, Offset.zero & this.size,
                    overflowChildRect, overflowHints: debugOverflowHints);
                return true;
            });
        }

        protected override bool hitTestChildren(HitTestResult result, Offset position = null) {
            return this.defaultHitTestChildren(result, position: position);
        }
    }
}