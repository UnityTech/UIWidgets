using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Gradient = Unity.UIWidgets.ui.Gradient;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgets.Runtime.rendering {
    enum _OverflowSide {
        left,
        top,
        bottom,
        right
    }

    class _OverflowRegionData {
        public _OverflowRegionData(
            Rect rect = null,
            string label = "",
            Offset labelOffset = null,
            float rotation = 0.0f,
            _OverflowSide? side = null
        ) {
            this.rect = rect;
            this.label = label;
            this.labelOffset = labelOffset ?? Offset.zero;
            this.rotation = rotation;
            this.side = side;
        }

        public readonly Rect rect;
        public readonly string label;
        public readonly Offset labelOffset;
        public readonly float rotation;
        public readonly _OverflowSide? side;
    }

    public static class DebugOverflowIndicatorMixin {
        static readonly Color _black = new Color(0xBF000000);
        static readonly Color _yellow = new Color(0xBFFFFF00);
        const float _indicatorFraction = 0.1f;
        const float _indicatorFontSizePixels = 7.5f;
        const float _indicatorLabelPaddingPixels = 1.0f;

        static readonly TextStyle _indicatorTextStyle = new TextStyle(
            color: new Color(0xFF900000),
            fontSize: _indicatorFontSizePixels,
            fontWeight: FontWeight.w800
        );

        static readonly Paint _indicatorPaint = new Paint();

        static readonly Paint _labelBackgroundPaint = new Paint();

        static readonly List<TextPainter> _indicatorLabel = new List<TextPainter>(4);

        static DebugOverflowIndicatorMixin() {
            _indicatorPaint.shader = Gradient.linear(
                new Offset(0.0f, 0.0f),
                new Offset(10.0f, 10.0f),
                new List<Color> {_black, _yellow, _yellow, _black},
                new List<float> {0.25f, 0.25f, 0.75f, 0.75f},
                TileMode.repeated
            );
            _labelBackgroundPaint.color = new Color(0xFFFFFFFF);
            for (int i = 0; i < 4; i++) {
                _indicatorLabel.Add(new TextPainter(new TextSpan(""), textDirection: TextDirection.ltr));
            }
        }

        static readonly Dictionary<RenderObject, bool> _overflowReportNeeded = new Dictionary<RenderObject, Boolean>();

        static string _formatPixels(float value) {
            D.assert(value > 0.0f);
            string pixels;
            if (value > 10.0f) {
                pixels = value.ToString("0");
            }
            else if (value > 1.0f) {
                pixels = value.ToString("0.0");
            }
            else {
                pixels = value.ToString("0.000");
            }

            return pixels;
        }

        static List<_OverflowRegionData> _calculateOverflowRegions(RelativeRect overflow, Rect containerRect) {
            List<_OverflowRegionData> regions = new List<_OverflowRegionData> { };
            if (overflow.left > 0.0f) {
                Rect markerRect = Rect.fromLTWH(
                    0.0f,
                    0.0f,
                    containerRect.width * _indicatorFraction,
                    containerRect.height
                );
                regions.Add(new _OverflowRegionData(
                    rect: markerRect,
                    label: "LEFT OVERFLOWED BY ${_formatPixels(overflow.left)} PIXELS",
                    labelOffset: markerRect.centerLeft +
                                 new Offset(_indicatorFontSizePixels + _indicatorLabelPaddingPixels, 0.0f),
                    rotation: Mathf.PI / 2.0f,
                    side: _OverflowSide.left
                ));
            }

            if (overflow.right > 0.0f) {
                Rect markerRect = Rect.fromLTWH(
                    containerRect.width * (1.0f - _indicatorFraction),
                    0.0f,
                    containerRect.width * _indicatorFraction,
                    containerRect.height
                );
                regions.Add(new _OverflowRegionData(
                    rect: markerRect,
                    label: $"RIGHT OVERFLOWED BY {_formatPixels(overflow.right)} PIXELS",
                    labelOffset: markerRect.centerRight -
                                 new Offset(_indicatorFontSizePixels + _indicatorLabelPaddingPixels, 0.0f),
                    rotation: -Mathf.PI / 2.0f,
                    side: _OverflowSide.right
                ));
            }

            if (overflow.top > 0.0f) {
                Rect markerRect = Rect.fromLTWH(
                    0.0f,
                    0.0f,
                    containerRect.width,
                    containerRect.height * _indicatorFraction
                );
                regions.Add(new _OverflowRegionData(
                    rect: markerRect,
                    label: $"TOP OVERFLOWED BY {_formatPixels(overflow.top)} PIXELS",
                    labelOffset: markerRect.topCenter + new Offset(0.0f, _indicatorLabelPaddingPixels),
                    rotation: 0.0f,
                    side: _OverflowSide.top
                ));
            }

            if (overflow.bottom > 0.0f) {
                Rect markerRect = Rect.fromLTWH(
                    0.0f,
                    containerRect.height * (1.0f - _indicatorFraction),
                    containerRect.width,
                    containerRect.height * _indicatorFraction
                );
                regions.Add(new _OverflowRegionData(
                    rect: markerRect,
                    label: $"BOTTOM OVERFLOWED BY {_formatPixels(overflow.bottom)} PIXELS",
                    labelOffset: markerRect.bottomCenter -
                                 new Offset(0.0f, _indicatorFontSizePixels + _indicatorLabelPaddingPixels),
                    rotation: 0.0f,
                    side: _OverflowSide.bottom
                ));
            }

            return regions;
        }

        static void _reportOverflow(RenderObject renderObject, RelativeRect overflow, string overflowHints) {
            overflowHints = overflowHints ?? $"The edge of the {renderObject.GetType()} that is " +
                            "overflowing has been marked in the rendering with a yellow and black " +
                            "striped pattern. This is usually caused by the contents being too big " +
                            $"for the {renderObject.GetType()}.\n" +
                            "This is considered an error condition because it indicates that there " +
                            "is content that cannot be seen. If the content is legitimately bigger " +
                            "than the available space, consider clipping it with a ClipRect widget " +
                            $"before putting it in the {renderObject.GetType()}, or using a scrollable " +
                            "container, like a ListView.";

            List<string> overflows = new List<string> { };
            if (overflow.left > 0.0f) {
                overflows.Add($"{_formatPixels(overflow.left)} pixels on the left");
            }

            if (overflow.top > 0.0f) {
                overflows.Add($"{_formatPixels(overflow.top)} pixels on the top");
            }

            if (overflow.bottom > 0.0f) {
                overflows.Add($"{_formatPixels(overflow.bottom)} pixels on the bottom");
            }

            if (overflow.right > 0.0f) {
                overflows.Add($"{_formatPixels(overflow.right)} pixels on the right");
            }

            string overflowText = "";
            D.assert(overflows.isNotEmpty(),
                () => $"Somehow {renderObject.GetType()} didn't actually overflow like it thought it did.");
            switch (overflows.Count) {
                case 1:
                    overflowText = overflows.first();
                    break;
                case 2:
                    overflowText = $"{overflows.first()} and {overflows.last()}";
                    break;
                default:
                    overflows[overflows.Count - 1] = $"and {overflows[overflows.Count - 1]}";
                    overflowText = string.Join(", ", overflow);
                    break;
            }

            UIWidgetsError.reportError(
                new UIWidgetsErrorDetails(
                    exception: new Exception($"A {renderObject.GetType()} overflowed by {overflowText}."),
                    library: "rendering library",
                    context: "during layout",
                    informationCollector: (information) => {
                        information.AppendLine(overflowHints);
                        information.AppendLine($"The specific {renderObject.GetType()} in question is:");
                        information.AppendLine($"  {renderObject.toStringShallow(joiner: "\n  ")}");
                        information.AppendLine(string.Concat(Enumerable.Repeat("◢◤", 32)));
                    }
                )
            );
        }

        public static void paintOverflowIndicator(
            RenderObject renderObject,
            PaintingContext context,
            Offset offset,
            Rect containerRect,
            Rect childRect,
            string overflowHints = null
        ) {
            RelativeRect overflow = RelativeRect.fromRect(containerRect, childRect);

            if (overflow.left <= 0.0f &&
                overflow.right <= 0.0f &&
                overflow.top <= 0.0f &&
                overflow.bottom <= 0.0f) {
                return;
            }

            List<_OverflowRegionData> overflowRegions = _calculateOverflowRegions(overflow, containerRect);
            foreach (_OverflowRegionData region in overflowRegions) {
                context.canvas.drawRect(region.rect.shift(offset), _indicatorPaint);

                if (_indicatorLabel[(int) region.side].text?.text != region.label) {
                    _indicatorLabel[(int) region.side].text = new TextSpan(
                        text: region.label,
                        style: _indicatorTextStyle
                    );
                    _indicatorLabel[(int) region.side].layout();
                }

                Offset labelOffset = region.labelOffset + offset;
                Offset centerOffset = new Offset(-_indicatorLabel[(int) region.side].width / 2.0f, 0.0f);
                Rect textBackgroundRect = centerOffset & _indicatorLabel[(int) region.side].size;
                context.canvas.save();
                context.canvas.translate(labelOffset.dx, labelOffset.dy);
                context.canvas.rotate(region.rotation);
                context.canvas.drawRect(textBackgroundRect, _labelBackgroundPaint);
                _indicatorLabel[(int) region.side].paint(context.canvas, centerOffset);
                context.canvas.restore();
            }

            bool containsKey = _overflowReportNeeded.TryGetValue(renderObject, out var overflowReportNeeded);
            overflowReportNeeded |= !containsKey;
            if (overflowReportNeeded) {
                _overflowReportNeeded[renderObject] = false;
                _reportOverflow(renderObject, overflow, overflowHints);
            }
        }
    }
}