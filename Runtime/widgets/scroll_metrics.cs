using System;
using Unity.UIWidgets.painting;
using UnityEngine;

namespace Unity.UIWidgets.widgets {
    public interface ScrollMetrics {
        float minScrollExtent { get; }

        float maxScrollExtent { get; }

        float pixels { get; }

        float viewportDimension { get; }

        AxisDirection axisDirection { get; }
    }

    public static class ScrollMetricsUtils {
        public static ScrollMetrics copyWith(ScrollMetrics it,
            float? minScrollExtent = null,
            float? maxScrollExtent = null,
            float? pixels = null,
            float? viewportDimension = null,
            AxisDirection? axisDirection = null,
            float? viewportFraction = null
        ) {
            if (it is IPageMetrics) {
                return new PageMetrics(
                    minScrollExtent: minScrollExtent ?? it.minScrollExtent,
                    maxScrollExtent: maxScrollExtent ?? it.maxScrollExtent,
                    pixels: pixels ?? it.pixels,
                    viewportDimension: viewportDimension ?? it.viewportDimension,
                    axisDirection: axisDirection ?? it.axisDirection,
                    viewportFraction: viewportFraction ?? ((IPageMetrics) it).viewportFraction
                );
            }

            if (it is IFixedExtentMetrics) {
                return new FixedExtentMetrics(
                    minScrollExtent: minScrollExtent ?? it.minScrollExtent,
                    maxScrollExtent: maxScrollExtent ?? it.maxScrollExtent,
                    pixels: pixels ?? it.pixels,
                    viewportDimension: viewportDimension ?? it.viewportDimension,
                    axisDirection: axisDirection ?? it.axisDirection,
                    itemIndex: ((IFixedExtentMetrics) it).itemIndex
                );
            }

            return new FixedScrollMetrics(
                minScrollExtent: minScrollExtent ?? it.minScrollExtent,
                maxScrollExtent: maxScrollExtent ?? it.maxScrollExtent,
                pixels: pixels ?? it.pixels,
                viewportDimension: viewportDimension ?? it.viewportDimension,
                axisDirection: axisDirection ?? it.axisDirection
            );
        }

        public static Axis axis(this ScrollMetrics it) {
            return AxisUtils.axisDirectionToAxis(it.axisDirection);
        }

        public static bool outOfRange(this ScrollMetrics it) {
            return it.pixels < it.minScrollExtent || it.pixels > it.maxScrollExtent;
        }

        public static bool atEdge(this ScrollMetrics it) {
            return it.pixels == it.minScrollExtent || it.pixels == it.maxScrollExtent;
        }

        public static float extentBefore(this ScrollMetrics it) {
            return Mathf.Max(it.pixels - it.minScrollExtent, 0.0f);
        }

        public static float extentInside(this ScrollMetrics it) {
            return Mathf.Min(it.pixels, it.maxScrollExtent) -
                   Mathf.Max(it.pixels, it.minScrollExtent) +
                   Mathf.Min(it.viewportDimension, it.maxScrollExtent - it.minScrollExtent);
        }

        public static float extentAfter(this ScrollMetrics it) {
            return Mathf.Max(it.maxScrollExtent - it.pixels, 0.0f);
        }
    }

    public class FixedScrollMetrics : ScrollMetrics {
        public FixedScrollMetrics(
            float minScrollExtent = 0.0f,
            float maxScrollExtent = 0.0f,
            float pixels = 0.0f,
            float viewportDimension = 0.0f,
            AxisDirection axisDirection = AxisDirection.down
        ) {
            this.minScrollExtent = minScrollExtent;
            this.maxScrollExtent = maxScrollExtent;
            this.pixels = pixels;
            this.viewportDimension = viewportDimension;
            this.axisDirection = axisDirection;
        }

        public float minScrollExtent { get; private set; }

        public float maxScrollExtent { get; private set; }

        public float pixels { get; private set; }

        public float viewportDimension { get; private set; }

        public AxisDirection axisDirection { get; private set; }

        public override string ToString() {
            return $"{this.GetType()}({this.extentBefore():F1})..[{this.extentInside():F1}]..{this.extentAfter():F1})";
        }
    }
}