using UIWidgetsGallery.gallery;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace UIWidgetsGallery {
    public class GalleryMain : UIWidgetsPanel {
        protected override Widget createWidget() {
            return new GalleryApp();
        }
        
        protected override void OnEnable() {
            base.OnEnable();
            FontManager.instance.addFont(Resources.Load<Font>("MaterialIcons-Regular"));
            FontManager.instance.addFont(Resources.Load<Font>("GalleryIcons"));
        }
    }
}
