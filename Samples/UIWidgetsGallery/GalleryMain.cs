using UIWidgetsGallery.gallery;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.material;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery {
    public class GalleryMain : UIWidgetsPanel {
        protected override Widget createWidget() {
            return new GalleryApp();
        }
    }
}
