using System;
using System.Collections.Generic;
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
            var bounds = canvas.getTotalMatrix().mapRect(this.logicalRect).roundOut(this.devicePixelRatio);

            D.assert(() => {
                var textureWidth = Mathf.CeilToInt(bounds.width * this.devicePixelRatio);
                var textureHeight = Mathf.CeilToInt(bounds.height * this.devicePixelRatio);

                D.assert(this.image.width == textureWidth);
                D.assert(this.image.height == textureHeight);
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
        internal _RasterCacheKey(Picture picture, Matrix3 matrix, float devicePixelRatio) {
            D.assert(picture != null);
            D.assert(matrix != null);
            this.picture = picture;
            this.matrix = new Matrix3(matrix);

            D.assert(() => {
                var x = this.matrix[2] * devicePixelRatio;
                var y = this.matrix[5] * devicePixelRatio;
                this.matrix[2] = (x - (int) x) / devicePixelRatio; // x
                this.matrix[5] = (y - (int) y) / devicePixelRatio; // y

                D.assert(Mathf.Abs(this.matrix[2]) <= 1e-5);
                D.assert(Mathf.Abs(this.matrix[5]) <= 1e-5);
                return true;
            });

            this.matrix[2] = 0.0f;
            this.matrix[5] = 0.0f;
            this.devicePixelRatio = devicePixelRatio;
        }

        public readonly Picture picture;

        public readonly Matrix3 matrix;

        public readonly float devicePixelRatio;

        public bool Equals(_RasterCacheKey other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(this.picture, other.picture) &&
                   Equals(this.matrix, other.matrix) &&
                   this.devicePixelRatio.Equals(other.devicePixelRatio);
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
            Picture picture, Matrix3 transform, float devicePixelRatio, bool isComplex,
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

            _RasterCacheKey cacheKey = new _RasterCacheKey(picture, transform, devicePixelRatio);

            var entry = this._cache.putIfAbsent(cacheKey, () => new _RasterCacheEntry());

            entry.accessCount = (entry.accessCount + 1).clamp(0, this.threshold);
            entry.usedThisFrame = true;

            if (entry.accessCount < this.threshold) {
                return null;
            }

            if (entry.image == null) {
                D.assert(this._meshPool != null);
                entry.image =
                    this._rasterizePicture(picture, transform, devicePixelRatio, this._meshPool);
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

            return true;
        }

        RasterCacheResult _rasterizePicture(Picture picture, Matrix3 transform, float devicePixelRatio,
            MeshPool meshPool) {
            var bounds = transform.mapRect(picture.paintBounds).roundOut(devicePixelRatio);

            var desc = new RenderTextureDescriptor(
                Mathf.CeilToInt((bounds.width * devicePixelRatio)),
                Mathf.CeilToInt((bounds.height * devicePixelRatio)),
                RenderTextureFormat.Default, 24) {
                useMipMap = false,
                autoGenerateMips = false,
            };

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