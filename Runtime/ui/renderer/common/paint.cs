using UnityEngine;

namespace Unity.UIWidgets.ui {
    public struct uiMaskFilter {
        uiMaskFilter(BlurStyle style, float sigma) {
            this.style = style;
            this.sigma = sigma;
        }

        public static uiMaskFilter blur(BlurStyle style, float sigma) {
            return new uiMaskFilter(style, sigma);
        }

        public readonly BlurStyle style;
        public readonly float sigma;
    }

    public struct uiColorFilter {
        uiColorFilter(uiColor color, BlendMode blendMode) {
            this.color = color;
            this.blendMode = blendMode;
        }

        public static uiColorFilter mode(uiColor color, BlendMode blendMode) {
            return new uiColorFilter(color, blendMode);
        }

        public readonly uiColor color;
        public readonly BlendMode blendMode;
    }

    public interface uiImageFilter {
    }

    public static class uiImageFilterHelper {
        public static uiImageFilter blur(float sigmaX = 0.0f, float sigmaY = 0.0f) {
            return new _uiBlurImageFilter(sigmaX, sigmaY);
        }

        public static uiImageFilter matrix(uiMatrix3 transform, FilterMode filterMode = FilterMode.Bilinear) {
            return new _uiMatrixImageFilter(transform, filterMode);
        }
    }

    struct _uiBlurImageFilter : uiImageFilter {
        public _uiBlurImageFilter(float sigmaX, float sigmaY) {
            this.sigmaX = sigmaX;
            this.sigmaY = sigmaY;
        }

        public readonly float sigmaX;
        public readonly float sigmaY;
    }

    struct _uiMatrixImageFilter : uiImageFilter {
        public _uiMatrixImageFilter(uiMatrix3 transform, FilterMode filterMode) {
            this.transform = transform;
            this.filterMode = filterMode;
        }

        public readonly uiMatrix3 transform;
        public readonly FilterMode filterMode;
    }

    public struct uiPaint {
        static readonly uiColor _kColorDefault = new uiColor(0xFFFFFFFF);

        public uiColor color;
        public BlendMode blendMode;
        public PaintingStyle style;
        public float strokeWidth;
        public StrokeCap strokeCap;
        public StrokeJoin strokeJoin;
        public float strokeMiterLimit;
        public FilterMode filterMode;
        public uiColorFilter? colorFilter;
        public uiMaskFilter? maskFilter;
        public uiImageFilter backdrop;
        public PaintShader shader;
        public bool invertColors;

        public uiPaint(
            uiColor? color = null,
            BlendMode blendMode = BlendMode.srcOver,
            PaintingStyle style = PaintingStyle.fill,
            float strokeWidth = 0f,
            StrokeCap strokeCap = StrokeCap.butt,
            StrokeJoin strokeJoin = StrokeJoin.miter,
            float strokeMiterLimit = 4.0f,
            FilterMode filterMode = FilterMode.Bilinear,
            uiColorFilter? colorFilter = null,
            uiMaskFilter? maskFilter = null,
            uiImageFilter backdrop = null,
            PaintShader shader = null,
            bool invertColors = false
        ) {
            this.color = color ?? _kColorDefault;
            this.blendMode = blendMode;
            this.style = style;
            this.strokeWidth = strokeWidth;
            this.strokeCap = strokeCap;
            this.strokeJoin = strokeJoin;
            this.strokeMiterLimit = strokeMiterLimit;
            this.filterMode = filterMode;
            this.colorFilter = colorFilter;
            this.maskFilter = maskFilter;
            this.backdrop = backdrop;
            this.shader = shader;
            this.invertColors = invertColors;
        }

        public uiPaint(uiPaint paint) {
            this.color = paint.color;
            this.blendMode = paint.blendMode;
            this.style = paint.style;
            this.strokeWidth = paint.strokeWidth;
            this.strokeCap = paint.strokeCap;
            this.strokeJoin = paint.strokeJoin;
            this.strokeMiterLimit = paint.strokeMiterLimit;
            this.filterMode = paint.filterMode;
            this.colorFilter = paint.colorFilter;
            this.maskFilter = paint.maskFilter;
            this.backdrop = paint.backdrop;
            this.shader = paint.shader;
            this.invertColors = paint.invertColors;
        }

        public static uiPaint shapeOnly(uiPaint paint) {
            return new uiPaint(
                style: paint.style,
                strokeWidth: paint.strokeWidth,
                strokeCap: paint.strokeCap,
                strokeJoin: paint.strokeJoin,
                strokeMiterLimit: paint.strokeMiterLimit
            );
        }

        public static uiPaint fromPaint(Paint paint) {
            uiImageFilter filter = null;
            if (paint.backdrop is _BlurImageFilter) {
                var blurFilter = (_BlurImageFilter) paint.backdrop;
                filter = uiImageFilterHelper.blur(blurFilter.sigmaX, blurFilter.sigmaY);
            }
            else if (paint.backdrop is _MatrixImageFilter) {
                var matrixFilter = (_MatrixImageFilter) paint.backdrop;
                filter = uiImageFilterHelper.matrix(uiMatrix3.fromMatrix3(matrixFilter.transform),
                    matrixFilter.filterMode);
            }

            return new uiPaint(
                color: paint.color == null ? (uiColor?) null : uiColor.fromColor(paint.color),
                blendMode: paint.blendMode,
                style: paint.style,
                strokeWidth: paint.strokeWidth,
                strokeCap: paint.strokeCap,
                strokeJoin: paint.strokeJoin,
                strokeMiterLimit: paint.strokeMiterLimit,
                filterMode: paint.filterMode,
                colorFilter: paint.colorFilter == null
                    ? (uiColorFilter?) null
                    : uiColorFilter.mode(uiColor.fromColor(paint.colorFilter.color), paint.colorFilter.blendMode),
                maskFilter: paint.maskFilter == null
                    ? (uiMaskFilter?) null
                    : uiMaskFilter.blur(paint.maskFilter.style, paint.maskFilter.sigma),
                backdrop: filter,
                shader: paint.shader,
                invertColors: paint.invertColors
            );
        }
    }
}