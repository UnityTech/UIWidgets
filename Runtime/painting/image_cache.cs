using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.painting {
    public class ImageCache {
        const int _kDefaultSize = 1000;
        const int _kDefaultSizeBytes = 100 << 20; // 100 MiB

        readonly Dictionary<object, ImageStreamCompleter> _pendingImages =
            new Dictionary<object, ImageStreamCompleter>();

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
            this._lruKeys.Clear();
            this._currentSizeBytes = 0;
        }

        public bool evict(object key) {
            D.assert(key != null);

            if (this._cache.TryGetValue(key, out var image)) {
                this._currentSizeBytes -= image.sizeBytes;
                this._cache.Remove(image.node);
                this._lruKeys.Remove(image.node);
                return true;
            }

            return false;
        }

        public ImageStreamCompleter putIfAbsent(object key, Func<ImageStreamCompleter> loader) {
            D.assert(key != null);
            D.assert(loader != null);

            if (this._pendingImages.TryGetValue(key, out var result)) {
                return result;
            }

            if (this._cache.TryGetValue(key, out var image)) {
                // put to the MRU position
                this._lruKeys.Remove(image.node);
                image.node = this._lruKeys.AddLast(key);
                return image.completer;
            }

            result = loader();

            if (this._maximumSize > 0 && this._maximumSizeBytes > 0) {
                D.assert(!this._pendingImages.ContainsKey(key));
                this._pendingImages[key] = result;

                ImageListener listener = null;
                listener = (info, syncCall) => {
                    result.removeListener(listener);

                    D.assert(this._pendingImages.ContainsKey(key));
                    this._pendingImages.Remove(key);

                    int imageSize = info?.image == null ? 0 : info.image.width & (info.image.height * 4);
                    _CachedImage cachedImage = new _CachedImage {
                        completer = result,
                        sizeBytes = imageSize,
                    };

                    // If the image is bigger than the maximum cache size, and the cache size
                    // is not zero, then increase the cache size to the size of the image plus
                    // some change.
                    if (this._maximumSizeBytes > 0 && imageSize > this._maximumSizeBytes) {
                        this._maximumSizeBytes = imageSize + 1000;
                    }

                    this._currentSizeBytes += imageSize;

                    D.assert(!this._cache.ContainsKey(key));
                    this._cache[key] = cachedImage;
                    cachedImage.node = this._lruKeys.AddLast(key);

                    this._checkCacheSize();
                };
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
        public ImageStreamCompleter completer;
        public int sizeBytes;
        public LinkedListNode<object> node;
    }
}