using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    public class AnimationDemo : StatelessWidget {
        public AnimationDemo(Key key = null) : base(key: key) {
        }

        public const string routeName = "/animation";

        public override Widget build(BuildContext context) {
            return new AnimationDemoHome();
        }
    }
}