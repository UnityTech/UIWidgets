using System.Collections.Generic;
using Object = System.Object;

namespace UIWidgets.painting {
    public class ImageCache {
        private const int _kDefaultSize = 1000;
        private const int _kDefaultSizeBytes = 20 << 20; // 20 MiB

        public Dictionary<Object, ImageStreamCompleter> _pendingImages =
            new Dictionary<Object, ImageStreamCompleter>();

        public Dictionary<Object, _CachedImage> _cache = new Dictionary<Object, _CachedImage>();
        public LinkedList<Object> _lruKeys = new LinkedList<Object>();

        private int _maximumSize = _kDefaultSize;

        public int maximumSize {
            get { return _maximumSize; }
            set {
                if (value == maximumSize) {
                    return;
                }

                _maximumSize = value;
                if (maximumSize == 0) {
                    _cache.Clear();
                    _lruKeys.Clear();
                    _currentSizeBytes = 0;
                }
                else {
                    _checkCacheSize();
                }
            }
        }

        public int currentSize {
            get { return _cache.Count; }
        }

        private int _maximumSizeBytes = _kDefaultSizeBytes;

        public int maximumSizeBytes {
            get { return _maximumSizeBytes; }
            set {
                if (value == _maximumSizeBytes) {
                    return;
                }

                _maximumSizeBytes = value;
                if (_maximumSizeBytes == 0) {
                    _cache.Clear();
                    _lruKeys.Clear();
                    _currentSizeBytes = 0;
                }
                else {
                    _checkCacheSize();
                }
            }
        }

        private int _currentSizeBytes;

        public int currentSizeBytes {
            get { return _currentSizeBytes; }
        }

        public void clear() {
            _cache.Clear();
            _lruKeys.Clear();
            _currentSizeBytes = 0;
        }

        public delegate ImageStreamCompleter Loader();

        public ImageStreamCompleter putIfAbsent(Object key, Loader loader) {
            ImageStreamCompleter result;
            if (_pendingImages.TryGetValue(key, out result)) {
                return result;
            }

            _CachedImage image;
            if (_cache.TryGetValue(key, out image)) {
                // put to the MRU position
                _lruKeys.Remove(key);
                _lruKeys.AddLast(key);
            }

            if (image != null) {
                return image.completer;
            }

            result = loader();

            if (maximumSize > 0 && maximumSizeBytes > 0) {
                _pendingImages[key] = result;
                result.addListener((info, syncCall) => {
//                    int imageSize = info.image == null ? 0 : info.image.height * info.image.width * 4;
                    // now we use length or raw bytes array as image size
                    int imageSize = info.image == null ? 0 : info.image.rawData.Length;
                    _CachedImage cachedImage = new _CachedImage(result, imageSize);
                    if (maximumSizeBytes > 0 && imageSize > maximumSizeBytes) {
                        _maximumSize = imageSize + 1000;
                    }

                    _currentSizeBytes += imageSize;
                    _pendingImages.Remove(key);
                    _cache[key] = cachedImage;
                    _lruKeys.AddLast(key);
                    this._checkCacheSize();
                }, null);
            }

            return result;
        }

        void _checkCacheSize() {
            while (_currentSizeBytes > _maximumSizeBytes || _cache.Count > _maximumSize) {
                Object key = _lruKeys.First.Value; // get the LRU item
                _CachedImage image = _cache[key];
                bool removed = _cache.Remove(key);
                if (image != null && removed) {
                    _currentSizeBytes -= image.sizeBytes;
                    _lruKeys.Remove(key);
                }
            }
        }
    }

    public class _CachedImage {
        public _CachedImage(ImageStreamCompleter completer, int sizeBytes) {
            this.completer = completer;
            this.sizeBytes = sizeBytes;
        }

        public ImageStreamCompleter completer;
        public int sizeBytes;
    }
}