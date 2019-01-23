using System;
using Unity.UIWidgets.ui;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.material {
    
    public class ThemeDataUtils {
        public static readonly Color _kLightThemeHighlightColor = new Color(0x66BCBCBC);
        
        public static readonly Color _kLightThemeSplashColor = new Color(0x66C8C8C8);
        
        public static readonly Color _kDarkThemeHighlightColor = new Color(0x40CCCCCC);
        
        public static readonly Color _kDarkThemeSplashColor = new Color(0x40CCCCCC);
    }

    public class ThemeUtils {
        public static readonly TimeSpan kThemeAnimationDuration = new TimeSpan(0, 0, 0, 0, 200);
    }
    
    public class MaterialUtils {
        public static Dictionary<MaterialType, BorderRadius> kMaterialEdges =
            new Dictionary<MaterialType, BorderRadius>() {
                {MaterialType.canvas, null},
                {MaterialType.card, BorderRadius.all(2.0)},
                {MaterialType.circle, null},
                {MaterialType.button, BorderRadius.all(2.0)},
                {MaterialType.transparency, null}
            };
    }
    
    public class InkHighlightUtils {
        public static readonly TimeSpan _kHighlightFadeDuration = new TimeSpan(0, 0, 0, 0, 200);
    }

    public class InkSplashUtils {
        public static readonly TimeSpan _kUnconfirmedSplashDuration = new TimeSpan(0, 0, 0, 1, 0);
        
        public static readonly TimeSpan _kSplashFadeDuration = new TimeSpan(0, 0, 0, 0, 200);

        public static double _kSplashInitialSize = 0.0;

        public static double _kSplashConfirmedVelocity = 1.0;

        public static RectCallback _getClipCallback(RenderBox referenceBox, bool containedInkWell,
            RectCallback rectCallback) {
            if (rectCallback != null) {
                D.assert(containedInkWell);
                return rectCallback;
            }

            if (containedInkWell)
                return () => Offset.zero & referenceBox.size;
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

        public static double _getSplashRadiusForPositionInSize(Size bounds, Offset position) {
            double d1 = (position - bounds.topLeft(Offset.zero)).distance;
            double d2 = (position - bounds.topRight(Offset.zero)).distance;
            double d3 = (position - bounds.bottomLeft(Offset.zero)).distance;
            double d4 = (position - bounds.bottomRight(Offset.zero)).distance;
            return Math.Max(Math.Max(d1, d2), Math.Max(d3, d4)).ceil();
        }
    }
}