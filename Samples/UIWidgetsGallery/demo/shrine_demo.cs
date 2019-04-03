using Unity.UIWidgets.animation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.gallery {
    class ShrineDemoUtils {
        public static Widget buildShrine(BuildContext context, Widget child) {
            return new Theme(
                data: new ThemeData(
                    primarySwatch: Colors.grey,
                    iconTheme: new IconThemeData(color: new Color(0xFF707070)),
                    platform: Theme.of(context).platform
                ),
                child: new ShrineTheme(child: child)
            );
        }
    }

    public class ShrinePageRoute<T> : MaterialPageRoute {
        public ShrinePageRoute(
            WidgetBuilder builder,
            RouteSettings settings
        ) : base(builder: builder, settings: settings) {
        }

        public override Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation) {
            return ShrineDemoUtils.buildShrine(context, base.buildPage(context, animation, secondaryAnimation));
        }
    }

    public class ShrineDemo : StatelessWidget {
        public const string routeName = "/shrine";

        public override Widget build(BuildContext context) {
            return ShrineDemoUtils.buildShrine(context, new ShrineHome());
        }
    }
}