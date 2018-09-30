using System;
using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.painting;
using UIWidgets.ui;
using UnityEditor;
using UnityEngine;
using Canvas = UIWidgets.ui.Canvas;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.flow {
    public class RasterCacheResult {
        public RasterCacheResult(Image image, Rect bounds) {
            D.assert(image != null);
            D.assert(bounds != null);

            this.image = image;
            this.bounds = bounds;
        }

        public readonly Image image;

        public readonly Rect bounds;

        public void draw(Canvas canvas) {
            var bounds = canvas.getMatrix().transformRect(this.bounds).roundOut();

            D.assert(() => {
                var textureWidth = (int) Math.Ceiling(
                    bounds.width * EditorGUIUtility.pixelsPerPoint); // todo: use window.pixelsPerPoint;
                var textureHeight = (int) Math.Ceiling(
                    bounds.height * EditorGUIUtility.pixelsPerPoint);

                D.assert(this.image.width == textureWidth);
                D.assert(this.image.height == textureHeight);
                return true;
            });

            canvas.save();
            try {
                canvas.setMatrix(Matrix4x4.identity);
                canvas.drawImageRect(this.image, bounds);
            }
            finally {
                canvas.restore();
            }
        }
    }

    class _RasterCacheKey : IEquatable<_RasterCacheKey> {
        internal _RasterCacheKey(Picture picture, ref Matrix4x4 matrix) {
            D.assert(picture != null);
            this.picture = picture;
            this.matrix = matrix;
            this.matrix.m03 = this.matrix.m03 - (int) this.matrix.m03; // x
            this.matrix.m13 = this.matrix.m13 - (int) this.matrix.m13; // y
            
            D.assert(this.matrix.m03 == 0);
            D.assert(this.matrix.m13 == 0);
        }

        public readonly Picture picture;

        public readonly Matrix4x4 matrix;

        public bool Equals(_RasterCacheKey other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.picture.Equals(other.picture) && this.matrix.Equals(other.matrix);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((_RasterCacheKey) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.picture.GetHashCode() * 397) ^ this.matrix.GetHashCode();
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

        public RasterCacheResult getPrerolledImage(
            Picture picture, ref Matrix4x4 transform, bool isComplex, bool willChange) {
            if (this.threshold == 0) {
                return null;
            }

            if (!_isPictureWorthRasterizing(picture, isComplex, willChange)) {
                return null;
            }

            if (transform.m33 == 0 || transform.determinant == 0) {
                return null;
            }

            _RasterCacheKey cacheKey = new _RasterCacheKey(picture, ref transform);

            var entry = this._cache.putIfAbsent(cacheKey, () => new _RasterCacheEntry());

            entry.accessCount = (entry.accessCount + 1).clamp(0, this.threshold);
            entry.usedThisFrame = true;

            if (entry.accessCount < this.threshold) {
                return null;
            }

            if (entry.image == null) {
                entry.image = this._rasterizePicture(picture, ref transform);
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

            return true;
        }

        RasterCacheResult _rasterizePicture(Picture picture, ref Matrix4x4 transform) {
            var bounds = transform.transformRect(picture.paintBounds).roundOut();

            var textureWidth = (int) Math.Ceiling(
                bounds.width * EditorGUIUtility.pixelsPerPoint); // todo: use window.pixelsPerPoint;
            var textureHeight = (int) Math.Ceiling(
                bounds.height * EditorGUIUtility.pixelsPerPoint);

            var texture = RenderTexture.GetTemporary(
                textureWidth, textureHeight, 32,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

            var oldTexture = RenderTexture.active;
            RenderTexture.active = texture;

            GL.PushMatrix();
            GL.LoadPixelMatrix((float) bounds.left, (float) bounds.right, (float) bounds.bottom, (float) bounds.top);
            GL.Clear(true, true, new UnityEngine.Color(0, 0, 0, 0));

            try {
                var canvas = new CanvasImpl();
                canvas.concat(transform);
                canvas.drawPicture(picture);

                Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
                tex.ReadPixels(new UnityEngine.Rect(0, 0, textureWidth, textureHeight), 0, 0, false);
                tex.Apply();

                return new RasterCacheResult(new Image(texture: tex), picture.paintBounds);
            }
            finally {
                GL.PopMatrix();
                RenderTexture.active = oldTexture;
                RenderTexture.ReleaseTemporary(texture);
            }
        }

        public void sweepAfterFrame() {
            var dead = new List<KeyValuePair<_RasterCacheKey, _RasterCacheEntry>>();
            foreach (var entry in this._cache) {
                if (!entry.Value.usedThisFrame) {
                    dead.Add(entry);
                } else {
                    entry.Value.usedThisFrame = false;
                }
            }

            foreach (var entry in dead) {
                this._cache.Remove(entry.Key);
            }
        }

        public void clear() {
            this._cache.Clear();
        }
    }
}