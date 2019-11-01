using System;
using System.Collections.Generic;
using Unity.UIWidgets.editor;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.flow {
    public class RasterCacheResult {
        public RasterCacheResult(Image image, Rect logicalRect, float devicePixelRatio) {
            D.assert(image != null);
            D.assert(logicalRect != null);

            this.image = image;
            this.logicalRect = logicalRect;
            this.devicePixelRatio = devicePixelRatio;
        }

        public readonly Image image;

        public readonly Rect logicalRect;

        public readonly float devicePixelRatio;

        public void draw(Canvas canvas) {
            var boundRect = canvas.getTotalMatrix().mapRect(this.logicalRect);
            var bounds = boundRect.withDevicePixelRatio(this.devicePixelRatio);

            D.assert(() => {
                var boundsInPixel = boundRect.roundOutScale(this.devicePixelRatio);
                var textureWidth = Mathf.CeilToInt(boundsInPixel.width);
                var textureHeight = Mathf.CeilToInt(boundsInPixel.height);
                
                //it is possible that there is a minor difference between the bound size and the image size (1 pixel at
                //most) due to the roundOut operation when calculating the bounds if the elements in the canvas transform
                //is not all integer
                D.assert(Mathf.Abs(this.image.width - textureWidth) <= 1);
                D.assert(Mathf.Abs(this.image.height - textureHeight) <= 1);
                return true;
            });

            canvas.save();
            try {
                canvas.resetMatrix();
                canvas.drawImage(this.image, bounds.topLeft, new Paint());
            }
            finally {
                canvas.restore();
            }
        }
    }

    class _RasterCacheKey : IEquatable<_RasterCacheKey> {
        internal _RasterCacheKey(Picture picture, Matrix3 matrix, float devicePixelRatio, int antiAliasing) {
            D.assert(picture != null);
            D.assert(matrix != null);
            this.picture = picture;
            this.matrix = new Matrix3(matrix);

            //This Assertion ensures that the transform of the given view matrix, i.e., dx, dy must be both integers in Skia.
            //We disable it because in our PictureLayer.PreRoll and Paint function, we use alignToPixel() to align the view matrix
            //before creating RasterCache which cannot meet this constraint due to the involved devicePixelRatio.
            //Enable it when we find a way to fix this alignment issue
//            D.assert(() => {
//                var x = this.matrix[2] * devicePixelRatio;
//                var y = this.matrix[5] * devicePixelRatio;
//                this.matrix[2] = (x - (int) x) / devicePixelRatio; // x
//                this.matrix[5] = (y - (int) y) / devicePixelRatio; // y
//
//                D.assert(Mathf.Abs(this.matrix[2]) <= 1e-5);
//                D.assert(Mathf.Abs(this.matrix[5]) <= 1e-5);
//                return true;
//            });

            this.matrix[2] = 0.0f;
            this.matrix[5] = 0.0f;
            this.devicePixelRatio = devicePixelRatio;
            this.antiAliasing = antiAliasing;
        }

        public readonly Picture picture;

        public readonly Matrix3 matrix;

        public readonly float devicePixelRatio;
        
        public readonly int antiAliasing;

        public bool Equals(_RasterCacheKey other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.picture, other.picture) &&
                   Equals(this.matrix, other.matrix) &&
                   this.devicePixelRatio.Equals(other.devicePixelRatio) &&
                   this.antiAliasing.Equals(other.antiAliasing);
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

            return this.Equals((_RasterCacheKey) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (this.picture != null ? this.picture.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.matrix != null ? this.matrix.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.devicePixelRatio.GetHashCode();
                hashCode = (hashCode * 397) ^ this.antiAliasing.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(_RasterCacheKey left, _RasterCacheKey right) {
            return Equals(left, right);
        }

        public static bool operator !=(_RasterCacheKey left, _RasterCacheKey right) {
            return !Equals(left, right);
        }
    }

    class _RasterCacheEntry {
        public bool usedThisFrame = false;
        public int accessCount = 0;
        public RasterCacheResult image;
    }

    public class RasterCache {
        public RasterCache(int threshold = 3) {
            this.threshold = threshold;
            this._cache = new Dictionary<_RasterCacheKey, _RasterCacheEntry>();
        }

        public readonly int threshold;

        readonly Dictionary<_RasterCacheKey, _RasterCacheEntry> _cache;

        MeshPool _meshPool;

        public MeshPool meshPool {
            set { this._meshPool = value; }
        }

        public RasterCacheResult getPrerolledImage(
            Picture picture, Matrix3 transform, float devicePixelRatio, int antiAliasing, bool isComplex,
            bool willChange) {
            if (this.threshold == 0) {
                return null;
            }

            if (!_isPictureWorthRasterizing(picture, isComplex, willChange)) {
                return null;
            }

            if (!transform.invert(null)) {
                return null;
            }

            _RasterCacheKey cacheKey = new _RasterCacheKey(picture, transform, devicePixelRatio, antiAliasing);

            var entry = this._cache.putIfAbsent(cacheKey, () => new _RasterCacheEntry());

            entry.accessCount = (entry.accessCount + 1).clamp(0, this.threshold);
            entry.usedThisFrame = true;

            if (entry.accessCount < this.threshold) {
                return null;
            }

            if (entry.image == null) {
                D.assert(this._meshPool != null);
                entry.image =
                    this._rasterizePicture(picture, transform, devicePixelRatio, antiAliasing, this._meshPool);
            }

            return entry.image;
        }

        static bool _isPictureWorthRasterizing(Picture picture,
            bool isComplex, bool willChange) {
            if (willChange) {
                return false;
            }

            if (!_canRasterizePicture(picture)) {
                return false;
            }

            if (isComplex) {
                return true;
            }

            return picture.drawCmds.Count > 10;
        }

        static bool _canRasterizePicture(Picture picture) {
            if (picture == null) {
                return false;
            }

            if (Window.instance.windowConfig.disableRasterCache) {
                return false;
            }
            
            var bounds = picture.paintBounds;
            if (bounds.isEmpty) {
                return false;
            }

            if (!bounds.isFinite) {
                return false;
            }

            if (picture.isDynamic) {
                return false;
            }

            //https://forum.unity.com/threads/rendertexture-create-failed-rendertexture-too-big.58667/
            if (picture.paintBounds.size.width > WindowConfig.MaxRasterImageSize ||
                picture.paintBounds.size.height > WindowConfig.MaxRasterImageSize) {
                return false;
            }

            return true;
        }

        RasterCacheResult _rasterizePicture(Picture picture, Matrix3 transform, float devicePixelRatio,
            int antiAliasing, MeshPool meshPool) {
            var boundRect = transform.mapRect(picture.paintBounds);
            var bounds = boundRect.withDevicePixelRatio(devicePixelRatio);
            var boundsInPixel = boundRect.roundOutScale(devicePixelRatio);

            var desc = new RenderTextureDescriptor(
                Mathf.CeilToInt(boundsInPixel.width),
                Mathf.CeilToInt(boundsInPixel.height),
                RenderTextureFormat.Default, 24) {
                useMipMap = false,
                autoGenerateMips = false,
            };
            
            if (antiAliasing != 0) {
                desc.msaaSamples = antiAliasing;
            }

            var renderTexture = new RenderTexture(desc);
            renderTexture.hideFlags = HideFlags.HideAndDontSave;

            var canvas = new CommandBufferCanvas(renderTexture, devicePixelRatio, meshPool);
            canvas.translate(-bounds.left, -bounds.top);
            canvas.concat(transform);
            canvas.drawPicture(picture);
            canvas.flush();
            canvas.dispose();

            return new RasterCacheResult(new Image(renderTexture), picture.paintBounds, devicePixelRatio);
        }

        public void sweepAfterFrame() {
            var dead = new List<KeyValuePair<_RasterCacheKey, _RasterCacheEntry>>();
            foreach (var entry in this._cache) {
                if (!entry.Value.usedThisFrame) {
                    dead.Add(entry);
                }
                else {
                    entry.Value.usedThisFrame = false;
                }
            }

            foreach (var entry in dead) {
                this._cache.Remove(entry.Key);
                if (entry.Value.image != null) {
                    entry.Value.image.image.Dispose();
                }
            }
        }

        public void clear() {
            foreach (var entry in this._cache) {
                if (entry.Value.image != null) {
                    entry.Value.image.image.Dispose();
                }
            }

            this._cache.Clear();
        }
    }
}