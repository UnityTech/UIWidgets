using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;

namespace Unity.UIWidgets.widgets {
    public class ScrollBehavior {
        public virtual Widget buildViewportChrome(BuildContext context, Widget child, AxisDirection axisDirection) {
            return child;
        }

        public virtual ScrollPhysics getScrollPhysics(BuildContext context) {
            return new BouncingScrollPhysics();
        }

        public virtual bool shouldNotify(ScrollBehavior oldDelegate) {
            return false;
        }

        public override string ToString() {
            return this.GetType().ToString();
        }
    }

    public class ScrollConfiguration : InheritedWidget {
        public ScrollConfiguration(
            Key key = null,
            ScrollBehavior behavior = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(behavior != null);
            this.behavior = behavior;
        }

        public readonly ScrollBehavior behavior;

        public static ScrollBehavior of(BuildContext context) {
            ScrollConfiguration configuration =
                (ScrollConfiguration) context.inheritFromWidgetOfExactType(typeof(ScrollConfiguration));
            if (configuration != null) {
                return configuration.behavior;
            }

            return new ScrollBehavior();
        }

        public override bool updateShouldNotify(InheritedWidget oldWidgetRaw) {
            var oldWidget = (ScrollConfiguration) oldWidgetRaw;

            D.assert(this.behavior != null);
            return this.behavior.GetType() != oldWidget.behavior.GetType()
                   || this.behavior != oldWidget.behavior && this.behavior.shouldNotify(oldWidget.behavior);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<ScrollBehavior>("behavior", this.behavior));
        }
    }
}