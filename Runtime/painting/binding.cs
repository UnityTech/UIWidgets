using Unity.UIWidgets.gestures;

namespace Unity.UIWidgets.painting {
    public class PaintingBinding : GestureBinding {
        public new static PaintingBinding instance {
            get { return (PaintingBinding) GestureBinding.instance; }
            set { GestureBinding.instance = value; }
        }

        public PaintingBinding() {
        }

        ImageCache _imageCache;

        public ImageCache imageCache {
            get {
                if (this._imageCache == null) {
                    this._imageCache = this.createImageCache();
                }

                return this._imageCache;
            }
        }

        protected virtual ImageCache createImageCache() {
            return new ImageCache();
        }
    }
}