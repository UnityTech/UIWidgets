namespace UIWidgets.painting {
    public class PaintingBinding {
        private static PaintingBinding _instance;

        public static PaintingBinding instance {
            get { return _instance; }
        }

        private ImageCache _imageCache;

        public ImageCache imageCache {
            get { return _imageCache; }
        }

        public ImageCache createImageCache() {
            return new ImageCache();
        }

        public void initInstances() {
            _instance = this;
            _imageCache = createImageCache();
        }
    }
}