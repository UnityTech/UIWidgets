using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.material {
    public static class ThemeDataUtils {
        public static readonly Color _kLightThemeHighlightColor = new Color(0x66BCBCBC);

        public static readonly Color _kLightThemeSplashColor = new Color(0x66C8C8C8);

        public static readonly Color _kDarkThemeHighlightColor = new Color(0x40CCCCCC);

        public static readonly Color _kDarkThemeSplashColor = new Color(0x40CCCCCC);
    }

    public static class ThemeUtils {
        public static readonly TimeSpan kThemeAnimationDuration = new TimeSpan(0, 0, 0, 0, 200);
    }

    public static class MaterialConstantsUtils {
        public static readonly Dictionary<MaterialType, BorderRadius> kMaterialEdges =
            new Dictionary<MaterialType, BorderRadius> {
                {MaterialType.canvas, null},
                {MaterialType.card, BorderRadius.circular(2.0)},
                {MaterialType.circle, null},
                {MaterialType.button, BorderRadius.circular(2.0)},
                {MaterialType.transparency, null}
            };
    }

    public static class InkHighlightUtils {
        public static readonly TimeSpan _kHighlightFadeDuration = new TimeSpan(0, 0, 0, 0, 200);
    }

    public static class InkSplashUtils {
        public static readonly TimeSpan _kUnconfirmedSplashDuration = new TimeSpan(0, 0, 0, 1, 0);

        public static readonly TimeSpan _kSplashFadeDuration = new TimeSpan(0, 0, 0, 0, 200);

        public const double _kSplashInitialSize = 0.0;

        public const double _kSplashConfirmedVelocity = 1.0;

        public static RectCallback _getClipCallback(RenderBox referenceBox, bool containedInkWell,
            RectCallback rectCallback) {
            if (rectCallback != null) {
                D.assert(containedInkWell);
                return rectCallback;
            }

            if (containedInkWell) {
                return () => Offset.zero & referenceBox.size;
            }

            return null;
        }

        public static double _getTargetRadius(RenderBox referenceBox, bool containedInkWell, RectCallback rectCallback,
            Offset position) {
            if (containedInkWell) {
                Size size = rectCallback != null ? rectCallback().size : referenceBox.size;
                return _getSplashRadiusForPositionInSize(size, position);
            }

            return Material.defaultSplashRadius;
        }

        static double _getSplashRadiusForPositionInSize(Size bounds, Offset position) {
            double d1 = (position - bounds.topLeft(Offset.zero)).distance;
            double d2 = (position - bounds.topRight(Offset.zero)).distance;
            double d3 = (position - bounds.bottomLeft(Offset.zero)).distance;
            double d4 = (position - bounds.bottomRight(Offset.zero)).distance;
            return Math.Max(Math.Max(d1, d2), Math.Max(d3, d4)).ceil();
        }
    }

    public static class InkRippleUtils {
        public static readonly TimeSpan _kUnconfirmedRippleDuration = new TimeSpan(0, 0, 1);
        public static readonly TimeSpan _kFadeInDuration = new TimeSpan(0, 0, 0, 0, 75);
        public static readonly TimeSpan _kRadiusDuration = new TimeSpan(0, 0, 0, 0, 225);
        public static readonly TimeSpan _kFadeOutDuration = new TimeSpan(0, 0, 0, 0, 375);
        public static readonly TimeSpan _kCancelDuration = new TimeSpan(0, 0, 0, 0, 75);

        public const double _kFadeOutIntervalStart = 225.0 / 375.0;

        public static RectCallback _getClipCallback(RenderBox referenceBox, bool containedInkWell,
            RectCallback rectCallback) {
            if (rectCallback != null) {
                D.assert(containedInkWell);
                return rectCallback;
            }

            if (containedInkWell) {
                return () => Offset.zero & referenceBox.size;
            }

            return null;
        }

        public static double _getTargetRadius(RenderBox referenceBox, bool containedInkWell, RectCallback rectCallback,
            Offset position) {
            Size size = rectCallback != null ? rectCallback().size : referenceBox.size;
            double d1 = size.bottomRight(Offset.zero).distance;
            double d2 = (size.topRight(Offset.zero) - size.bottomLeft(Offset.zero)).distance;
            return Math.Max(d1, d2) / 2.0;
        }
    }

    public static class ScrollbarUtils {
        public static readonly TimeSpan _kScrollbarFadeDuration = TimeSpan.FromMilliseconds(300);

        public static readonly TimeSpan _kScrollbarTimeToFade = TimeSpan.FromMilliseconds(600);

        public const double _kScrollbarThickness = 6.0;
    }


    public static class ArcUtils {
        public const double _kOnAxisDelta = 2.0;

        public static readonly List<_Diagonal> _allDiagonals = new List<_Diagonal> {
            new _Diagonal(_CornerId.topLeft, _CornerId.bottomRight),
            new _Diagonal(_CornerId.bottomRight, _CornerId.topLeft),
            new _Diagonal(_CornerId.topRight, _CornerId.bottomLeft),
            new _Diagonal(_CornerId.bottomLeft, _CornerId.topRight)
        };

        public delegate double _KeyFunc<T>(T input);


        public static T _maxBy<T>(List<T> input, _KeyFunc<T> keyFunc) {
            T maxValue = default(T);
            double? maxKey = null;
            foreach (T value in input) {
                double key = keyFunc(value);
                if (maxKey == null || key > maxKey) {
                    maxValue = value;
                    maxKey = key;
                }
            }

            return maxValue;
        }
    }

    public static class ExpansionTileUtils {
        public static readonly TimeSpan _kExpand = new TimeSpan(0, 0, 0, 0, 200);
    }

    public static class ExpansionPanelUtils {
        public const double _kPanelHeaderCollapsedHeight = 48.0;
        public const double _kPanelHeaderExpandedHeight = 64.0;
    }

    public static class IconButtonUtils {
        public const double _kMinButtonSize = 48.0;
    }

    public static class DrawerHeaderUtils {
        public const double _kDrawerHeaderHeight = 160.0 + 1.0;
    }

    public static class DrawerUtils {
        public const double _kWidth = 304.0;
        public const double _kEdgeDragWidth = 20.0;
        public const double _kMinFlingVelocity = 365.0;
        public static readonly TimeSpan _kBaseSettleDuration = new TimeSpan(0, 0, 0, 0, 246);
    }

    public static class TooltipUtils {
        public static readonly TimeSpan _kFadeDuration = new TimeSpan(0, 0, 0, 0, 200);
        public static readonly TimeSpan _kShowDuration = new TimeSpan(0, 0, 0, 0, 1500);
    }
}