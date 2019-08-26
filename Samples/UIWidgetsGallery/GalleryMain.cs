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
            FontManager.instance.addFont(Resources.Load<Font>("MaterialIcons-Regular"), "Material Icons");
            FontManager.instance.addFont(Resources.Load<Font>("GalleryIcons"), "GalleryIcons");
            
            FontManager.instance.addFont(Resources.Load<Font>("CupertinoIcons"), "CupertinoIcons");
            FontManager.instance.addFont(Resources.Load<Font>(path: "SF-Pro-Text-Regular"), ".SF Pro Text", FontWeight.w400);
            FontManager.instance.addFont(Resources.Load<Font>(path: "SF-Pro-Text-Semibold"), ".SF Pro Text", FontWeight.w600);
            FontManager.instance.addFont(Resources.Load<Font>(path: "SF-Pro-Text-Bold"), ".SF Pro Text", FontWeight.w700);
            
            base.OnEnable();
        }
    }
}
