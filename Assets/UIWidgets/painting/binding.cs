using UIWidgets.ui;

namespace UIWidgets.painting {
    public class PaintingBinding {

        public PaintingBinding(Window window) {
            this._window = window;
        }
        
        private static PaintingBinding _instance;
        public readonly Window _window;

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