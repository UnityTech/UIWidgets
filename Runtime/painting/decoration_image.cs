using Unity.UIWidgets.ui;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public enum ImageRepeat {
        repeat,
        repeatX,
        repeatY,
        noRepeat,
    }

    public class DecorationImage {
        public DecorationImage() {
        }
    }

    public static class ImageUtils {
        public static void paintImage(
            Canvas canvas = null,
            Rect rect = null,
            Image image = null,
            double scale = 1.0,
            ColorFilter colorFilter = null,
            BoxFit? fit = null,
            Alignment alignment = null,
            Rect centerSlice = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            bool invertColors = false,
            FilterMode filterMode = FilterMode.Point
        ) {
            D.assert(canvas != null);
            D.assert(rect != null);
            D.assert(image != null);
            alignment = alignment ?? Alignment.center;

            if (rect.isEmpty) {
                return;
            }

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

            fit = fit ?? (centerSlice == null ? BoxFit.scaleDown : BoxFit.fill);
            D.assert(centerSlice == null || (fit != BoxFit.none && fit != BoxFit.cover));
            FittedSizes fittedSizes = FittedSizes.applyBoxFit(fit.Value, inputSize / scale, outputSize);
            Size sourceSize = fittedSizes.source * scale;
            Size destinationSize = fittedSizes.destination;
            if (centerSlice != null) {
                outputSize += sliceBorder;
                destinationSize += sliceBorder;
                D.assert(sourceSize == inputSize,
                    "centerSlice was used with a BoxFit that does not guarantee that the image is fully visible.");
            }

            if (repeat != ImageRepeat.noRepeat && destinationSize == outputSize) {
                repeat = ImageRepeat.noRepeat;
            }

            Paint paint = new Paint();
            if (colorFilter != null) {
                paint.colorFilter = colorFilter;
            }
            if (sourceSize != destinationSize) {
                paint.filterMode = filterMode;
            }
            paint.invertColors = invertColors;

            double halfWidthDelta = (outputSize.width - destinationSize.width) / 2.0;
            double halfHeightDelta = (outputSize.height - destinationSize.height) / 2.0;
            double dx = halfWidthDelta + alignment.x * halfWidthDelta;
            double dy = halfHeightDelta + alignment.y * halfHeightDelta;
            Offset destinationPosition = rect.topLeft.translate(dx, dy);
            Rect destinationRect = destinationPosition & destinationSize;
            bool needSave = repeat != ImageRepeat.noRepeat;
            if (needSave) {
                canvas.save();
            }

            if (repeat != ImageRepeat.noRepeat) {
                canvas.clipRect(rect);
            }

            if (centerSlice == null) {
                Rect sourceRect = alignment.inscribe(
                    sourceSize, Offset.zero & inputSize
                );
                foreach (Rect tileRect in _generateImageTileRects(rect, destinationRect, repeat)) {
                    canvas.drawImageRect(image, sourceRect, tileRect, paint);
                }
            } else {
                foreach (Rect tileRect in _generateImageTileRects(rect, destinationRect, repeat)) {
                    canvas.drawImageNine(image, centerSlice, tileRect, paint);
                }
            }

            if (needSave) {
                canvas.restore();
            }
        }

        static IEnumerable<Rect> _generateImageTileRects(Rect outputRect, Rect fundamentalRect,
            ImageRepeat repeat) {
            if (repeat == ImageRepeat.noRepeat) {
                yield return fundamentalRect;
                yield break;
            }

            int startX = 0;
            int startY = 0;
            int stopX = 0;
            int stopY = 0;
            double strideX = fundamentalRect.width;
            double strideY = fundamentalRect.height;

            if (repeat == ImageRepeat.repeat || repeat == ImageRepeat.repeatX) {
                startX = ((outputRect.left - fundamentalRect.left) / strideX).floor();
                stopX = ((outputRect.right - fundamentalRect.right) / strideX).ceil();
            }

            if (repeat == ImageRepeat.repeat || repeat == ImageRepeat.repeatY) {
                startY = ((outputRect.top - fundamentalRect.top) / strideY).floor();
                stopY = ((outputRect.bottom - fundamentalRect.bottom) / strideY).ceil();
            }

            for (int i = startX; i <= stopX; ++i) {
                for (int j = startY; j <= stopY; ++j)
                    yield return fundamentalRect.shift(new Offset(i * strideX, j * strideY));
            }
        }
    }
}