using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
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

    public class DecorationImage : IEquatable<DecorationImage> {
        public DecorationImage(
            ImageProvider image = null,
            ColorFilter colorFilter = null,
            BoxFit? fit = null,
            Alignment alignment = null,
            Rect centerSlice = null,
            ImageRepeat repeat = ImageRepeat.noRepeat
        ) {
            D.assert(image != null);
            this.image = image;
            this.colorFilter = colorFilter;
            this.fit = fit;
            this.alignment = alignment ?? Alignment.center;
            this.centerSlice = centerSlice;
            this.repeat = repeat;
        }

        public readonly ImageProvider image;
        public readonly ColorFilter colorFilter;
        public readonly BoxFit? fit;
        public readonly Alignment alignment;
        public readonly Rect centerSlice;
        public readonly ImageRepeat repeat;

        public DecorationImagePainter createPainter(VoidCallback onChanged) {
            D.assert(onChanged != null);
            return new DecorationImagePainter(this, onChanged);
        }

        public bool Equals(DecorationImage other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.image, other.image) && Equals(this.colorFilter, other.colorFilter) &&
                   this.fit == other.fit && Equals(this.alignment, other.alignment) &&
                   Equals(this.centerSlice, other.centerSlice) && this.repeat == other.repeat;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((DecorationImage) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.image != null ? this.image.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.colorFilter != null ? this.colorFilter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.fit.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.alignment != null ? this.alignment.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.centerSlice != null ? this.centerSlice.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) this.repeat;
                return hashCode;
            }
        }

        public static bool operator ==(DecorationImage left, DecorationImage right) {
            return Equals(left, right);
        }

        public static bool operator !=(DecorationImage left, DecorationImage right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            var properties = new List<string>();
            properties.Add($"{this.image}");

            if (this.colorFilter != null) {
                properties.Add($"{this.colorFilter}");
            }

            if (this.fit != null &&
                !(this.fit == BoxFit.fill && this.centerSlice != null) &&
                !(this.fit == BoxFit.scaleDown && this.centerSlice == null)) {
                properties.Add($"{this.fit}");
            }

            properties.Add($"{this.alignment}");

            if (this.centerSlice != null) {
                properties.Add($"centerSlice: {this.centerSlice}");
            }

            if (this.repeat != ImageRepeat.noRepeat) {
                properties.Add($"{this.repeat}");
            }

            return $"{this.GetType()}({string.Join(", ", properties)})";
        }
    }

    public class DecorationImagePainter : IDisposable {
        internal DecorationImagePainter(DecorationImage details, VoidCallback onChanged) {
            D.assert(details != null);
            this._details = details;
            this._onChanged = onChanged;
        }

        readonly DecorationImage _details;

        readonly VoidCallback _onChanged;

        ImageStream _imageStream;

        ImageInfo _image;

        public void paint(Canvas canvas, Rect rect, Path clipPath, ImageConfiguration configuration) {
            D.assert(canvas != null);
            D.assert(rect != null);
            D.assert(configuration != null);

            ImageStream newImageStream = this._details.image.resolve(configuration);
            if (newImageStream.key != this._imageStream?.key) {
                this._imageStream?.removeListener(this._imageListener);
                this._imageStream = newImageStream;
                this._imageStream.addListener(this._imageListener);
            }

            if (this._image == null) {
                return;
            }

            if (clipPath != null) {
                canvas.save();
                canvas.clipPath(clipPath);
            }

            ImageUtils.paintImage(
                canvas: canvas,
                rect: rect,
                image: this._image.image,
                scale: this._image.scale,
                colorFilter: this._details.colorFilter,
                fit: this._details.fit,
                alignment: this._details.alignment,
                centerSlice: this._details.centerSlice,
                repeat: this._details.repeat
            );

            if (clipPath != null) {
                canvas.restore();
            }
        }

        void _imageListener(ImageInfo value, bool synchronousCall) {
            if (this._image == value) {
                return;
            }

            this._image = value;

            D.assert(this._onChanged != null);
            if (!synchronousCall) {
                this._onChanged();
            }
        }

        public void Dispose() {
            this._imageStream?.removeListener(this._imageListener);
        }

        public override string ToString() {
            return $"{this.GetType()}(stream: {this._imageStream}, image: {this._image}) for {this._details}";
        }
    }

    public static class ImageUtils {
        public static void paintImage(
            Canvas canvas = null,
            Rect rect = null,
            Image image = null,
            float scale = 1.0f,
            ColorFilter colorFilter = null,
            BoxFit? fit = null,
            Alignment alignment = null,
            Rect centerSlice = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            bool invertColors = false,
            FilterMode filterMode = FilterMode.Bilinear
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
            D.assert(centerSlice == null || (fit != BoxFit.none && fit != BoxFit.cover),
                () => $"centerSlice was used with a BoxFit {fit} that is not supported.");
            FittedSizes fittedSizes = FittedSizes.applyBoxFit(fit.Value, inputSize / scale, outputSize);
            Size sourceSize = fittedSizes.source * scale;
            Size destinationSize = fittedSizes.destination;
            if (centerSlice != null) {
                outputSize += sliceBorder;
                destinationSize += sliceBorder;
                D.assert(sourceSize == inputSize,
                    () =>
                        $"centerSlice was used with a BoxFit {fit} that does not guarantee that the image is fully visible.");
            }

            if (repeat != ImageRepeat.noRepeat && destinationSize == outputSize) {
                repeat = ImageRepeat.noRepeat;
            }

            Paint paint = new Paint();
            if (colorFilter != null) {
                paint.colorFilter = colorFilter;
                paint.color = colorFilter.color;
                paint.blendMode = colorFilter.blendMode;
            }

            if (sourceSize != destinationSize) {
                paint.filterMode = filterMode;
            }

            paint.invertColors = invertColors;

            float halfWidthDelta = (outputSize.width - destinationSize.width) / 2.0f;
            float halfHeightDelta = (outputSize.height - destinationSize.height) / 2.0f;
            float dx = halfWidthDelta + alignment.x * halfWidthDelta;
            float dy = halfHeightDelta + alignment.y * halfHeightDelta;
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
                if (repeat == ImageRepeat.noRepeat) {
                    canvas.drawImageRect(image, sourceRect, destinationRect, paint);
                }
                else {
                    foreach (Rect tileRect in _generateImageTileRects(rect, destinationRect, repeat)) {
                        canvas.drawImageRect(image, sourceRect, tileRect, paint);
                    }
                }
            }
            else {
                if (repeat == ImageRepeat.noRepeat) {
                    canvas.drawImageNine(image, centerSlice, destinationRect, paint);
                }
                else {
                    foreach (Rect tileRect in _generateImageTileRects(rect, destinationRect, repeat)) {
                        canvas.drawImageNine(image, centerSlice, tileRect, paint);
                    }
                }
            }

            if (needSave) {
                canvas.restore();
            }
        }

        static IEnumerable<Rect> _generateImageTileRects(Rect outputRect, Rect fundamentalRect,
            ImageRepeat repeat) {
            int startX = 0;
            int startY = 0;
            int stopX = 0;
            int stopY = 0;
            float strideX = fundamentalRect.width;
            float strideY = fundamentalRect.height;

            if (repeat == ImageRepeat.repeat || repeat == ImageRepeat.repeatX) {
                startX = ((outputRect.left - fundamentalRect.left) / strideX).floor();
                stopX = ((outputRect.right - fundamentalRect.right) / strideX).ceil();
            }

            if (repeat == ImageRepeat.repeat || repeat == ImageRepeat.repeatY) {
                startY = ((outputRect.top - fundamentalRect.top) / strideY).floor();
                stopY = ((outputRect.bottom - fundamentalRect.bottom) / strideY).ceil();
            }

            for (int i = startX; i <= stopX; ++i) {
                for (int j = startY; j <= stopY; ++j) {
                    yield return fundamentalRect.shift(new Offset(i * strideX, j * strideY));
                }
            }
        }
    }
}