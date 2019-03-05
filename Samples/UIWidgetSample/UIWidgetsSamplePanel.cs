using Unity.UIWidgets.animation;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.widgets;

namespace UIWidgetsSample {
    public class UIWidgetsSamplePanel: UIWidgetsPanel {
        
        protected virtual PageRouteFactory pageRouteBuilder {
            get {
                return (RouteSettings settings, WidgetBuilder builder) =>
                    new PageRouteBuilder(
                        settings: settings,
                        pageBuilder: (BuildContext context, Animation<float> animation,
                            Animation<float> secondaryAnimation) => builder(context)
                    );
            }
        }

    }
}