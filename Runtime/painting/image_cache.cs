using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.painting {
    public class ImageCache {
        const int _kDefaultSize = 1000;
        const int _kDefaultSizeBytes = 100 << 20; // 100 MiB

        readonly Dictionary<object, _PendingImage> _pendingImages =
            new Dictionary<object, _PendingImage>();

        readonly Dictionary<object, _CachedImage> _cache = new Dictionary<object, _CachedImage>();
        readonly LinkedList<object> _lruKeys = new LinkedList<object>();

        int _maximumSize = _kDefaultSize;

        public int maximumSize {
            get { return this._maximumSize; }
            set {
                D.assert(value >= 0);
                if (value == this._maximumSize) {
                    return;
                }

                this._maximumSize = value;
                if (this._maximumSize == 0) {
                    this.clear();
                }
                else {
                    this._checkCacheSize();
                }
            }
        }

        public int currentSize {
            get { return this._cache.Count; }
        }

        int _maximumSizeBytes = _kDefaultSizeBytes;

        public int maximumSizeBytes {
            get { return this._maximumSizeBytes; }
            set {
                D.assert(value >= 0);
                if (value == this._maximumSizeBytes) {
                    return;
                }

                this._maximumSizeBytes = value;
                if (this._maximumSizeBytes == 0) {
                    this.clear();
                }
                else {
                    this._checkCacheSize();
                }
            }
        }

        int _currentSizeBytes;

        public int currentSizeBytes {
            get { return this._currentSizeBytes; }
        }

        public void clear() {
            this._cache.Clear();
            this._pendingImages.Clear();
            this._currentSizeBytes = 0;

            this._lruKeys.Clear();
        }

        public bool evict(object key) {
            D.assert(key != null);

            if (this._pendingImages.TryGetValue(key, out var pendingImage)) {
                pendingImage.removeListener();
                this._pendingImages.Remove(key);
                return true;
            }

            if (this._cache.TryGetValue(key, out var image)) {
                this._currentSizeBytes -= image.sizeBytes;
                this._cache.Remove(key);
                this._lruKeys.Remove(image.node);
                return true;
            }

            return false;
        }

        public ImageStreamCompleter putIfAbsent(object key, Func<ImageStreamCompleter> loader,
            ImageErrorListener onError = null) {
            D.assert(key != null);
            D.assert(loader != null);

            ImageStreamCompleter result = null;
            if (this._pendingImages.TryGetValue(key, out var pendingImage)) {
                result = pendingImage.completer;
                return result;
            }

            if (this._cache.TryGetValue(key, out var image)) {
                // put to the MRU position
                this._lruKeys.Remove(image.node);
                image.node = this._lruKeys.AddLast(key);
                return image.completer;
            }

            try {
                result = loader();
            }
            catch (Exception ex) {
                if (onError != null) {
                    onError(ex);
                }
                else {
                    throw;
                }
            }

            void listener(ImageInfo info, bool syncCall) {
                int imageSize = info?.image == null ? 0 : info.image.height * info.image.width * 4;
                _CachedImage cachedImage = new _CachedImage(result, imageSize);

                if (this.maximumSizeBytes > 0 && imageSize > this.maximumSizeBytes) {
                    this._maximumSizeBytes = imageSize + 1000;
                }

                this._currentSizeBytes += imageSize;

                if (this._pendingImages.TryGetValue(key, out var loadedPendingImage)) {
                    loadedPendingImage.removeListener();
                    this._pendingImages.Remove(key);
                }

                D.assert(!this._cache.ContainsKey(key));
                this._cache[key] = cachedImage;
                cachedImage.node = this._lruKeys.AddLast(key);
                this._checkCacheSize();
            }

            if (this.maximumSize > 0 && this.maximumSizeBytes > 0) {
                this._pendingImages[key] = new _PendingImage(result, listener);
                result.addListener(listener);
            }

            return result;
        }

        void _checkCacheSize() {
            while (this._currentSizeBytes > this._maximumSizeBytes || this._cache.Count > this._maximumSize) {
                var node = this._lruKeys.First;
                var key = node.Value; // get the LRU item

                D.assert(this._cache.ContainsKey(key));
                _CachedImage image = this._cache[key];

                D.assert(node == image.node);
                this._currentSizeBytes -= image.sizeBytes;
                this._cache.Remove(key);
                this._lruKeys.Remove(image.node);
            }

            D.assert(this._currentSizeBytes >= 0);
            D.assert(this._cache.Count <= this.maximumSize);
            D.assert(this._currentSizeBytes <= this.maximumSizeBytes);
        }
    }

    class _CachedImage {
        public _CachedImage(ImageStreamCompleter completer, int sizeBytes) {
            this.completer = completer;
            this.sizeBytes = sizeBytes;
        }

        public ImageStreamCompleter completer;
        public int sizeBytes;
        public LinkedListNode<object> node;
    }

    class _PendingImage {
        public _PendingImage(
            ImageStreamCompleter completer,
            ImageListener listener
        ) {
            this.completer = completer;
            this.listener = listener;
        }

        public readonly ImageStreamCompleter completer;

        public readonly ImageListener listener;

        public void removeListener() {
            this.completer.removeListener(this.listener);
        }
    }
}