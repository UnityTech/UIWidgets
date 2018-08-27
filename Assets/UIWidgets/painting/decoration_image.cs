using System;
using UIWidgets.ui;
using System.Collections.Generic;

namespace UIWidgets.painting {
    /// How to paint any portions of a box not covered by an image.
    public enum ImageRepeat {
        /// Repeat the image in both the x and y directions until the box is filled.
        repeat,

        /// Repeat the image in the x direction until the box is filled horizontally.
        repeatX,

        /// Repeat the image in the y direction until the box is filled vertically.
        repeatY,

        /// Leave uncovered portions of the box transparent.
        noRepeat,
    }

    public class DecorationImage {
        public DecorationImage() {
        }
    }

    public static class DecorationImageUtil {
        public static void paintImage(Canvas canvas, Rect rect, ui.Image image, BoxFit fit, Rect centerSlice,
            Alignment alignment = null,
            ImageRepeat repeat = ImageRepeat.noRepeat) {
            if (rect.isEmpty)
                return;
            alignment = alignment ?? Alignment.center;
            Size outputSize = rect.size;
            Size inputSize = new Size(image.width, image.height);
            Offset sliceBorder = null;
            if (centerSlice != null) {
                sliceBorder = new Offset(
                    centerSlice.left + inputSize.width - centerSlice.right,
                    centerSlice.top + inputSize.height - centerSlice.bottom
                );
                outputSize -= sliceBorder;
                inputSize -= sliceBorder;
            }

            fit = centerSlice == null ? BoxFit.scaleDown : BoxFit.fill;
            FittedSizes fittedSizes = FittedSizes.applyBoxFit(fit, inputSize, outputSize);
            Size sourceSize = fittedSizes.source;
            Size destinationSize = fittedSizes.destination;
            if (centerSlice != null) {
                outputSize += sliceBorder;
                destinationSize += sliceBorder;
            }

            if (repeat != ImageRepeat.noRepeat && destinationSize == outputSize) {
                repeat = ImageRepeat.noRepeat;
            }

            Paint paint = new Paint(); // ..isAntiAlias = false;
//            if (colorFilter != null)
//                paint.colorFilter = colorFilter;
            if (sourceSize != destinationSize) {
                // Use the "low" quality setting to scale the image, which corresponds to
                // bilinear interpolation, rather than the default "none" which corresponds
                // to nearest-neighbor.
//                paint.filterQuality = FilterQuality.low;
            }

            double halfWidthDelta = (outputSize.width - destinationSize.width) / 2.0;
            double halfHeightDelta = (outputSize.height - destinationSize.height) / 2.0;
            double dx = halfWidthDelta + alignment.x * halfWidthDelta;
            double dy = halfHeightDelta + alignment.y * halfHeightDelta;
            Offset destinationPosition = rect.topLeft.translate(dx, dy);
            Rect destinationRect = destinationPosition & destinationSize;
            bool needSave = repeat != ImageRepeat.noRepeat;
            if (needSave)
                canvas.save();
            if (repeat != ImageRepeat.noRepeat)
                canvas.clipRect(rect);
            if (centerSlice == null) {
                Rect sourceRect = alignment.inscribe(
                    fittedSizes.source, Offset.zero & inputSize
                );
                foreach (Rect tileRect in _generateImageTileRects(rect, destinationRect, repeat)) {
                    canvas.drawImageRect(sourceRect, tileRect, paint, image);
                }
            }
            else {
                // todo
                foreach (Rect tileRect in _generateImageTileRects(rect, destinationRect, repeat)) {
//                canvas.drawImageNine(image, centerSlice, tileRect, paint);
                }
            }

            if (needSave)
                canvas.restore();
        }

        public static List<Rect> _generateImageTileRects(Rect outputRect, Rect fundamentalRect,
            ImageRepeat repeat) {
            List<Rect> tileRects = new List<Rect>();
            if (repeat == ImageRepeat.noRepeat) {
                tileRects.Add(fundamentalRect);
                return tileRects;
            }

            int startX = 0;
            int startY = 0;
            int stopX = 0;
            int stopY = 0;
            double strideX = fundamentalRect.width;
            double strideY = fundamentalRect.height;

            if (repeat == ImageRepeat.repeat || repeat == ImageRepeat.repeatX) {
                startX = (int) Math.Floor((outputRect.left - fundamentalRect.left) / strideX);
                stopX = (int) Math.Ceiling((outputRect.right - fundamentalRect.right) / strideX);
            }

            if (repeat == ImageRepeat.repeat || repeat == ImageRepeat.repeatY) {
                startY = (int) Math.Floor((outputRect.top - fundamentalRect.top) / strideY);
                stopY = (int) Math.Ceiling((outputRect.bottom - fundamentalRect.bottom) / strideY);
            }

            for (int i = startX; i <= stopX; ++i) {
                for (int j = startY; j <= stopY; ++j)
                    tileRects.Add(fundamentalRect.shift(new Offset(i * strideX, j * strideY)));
            }

            return tileRects;
        }
    }
}