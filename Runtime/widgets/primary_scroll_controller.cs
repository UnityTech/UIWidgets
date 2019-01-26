using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public class PrimaryScrollController : InheritedWidget {
        public PrimaryScrollController(
            Key key = null,
            ScrollController controller = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(controller != null);
            this.controller = controller;
        }

        PrimaryScrollController(
            Key key = null,
            Widget child = null
        ) : base(key: key, child: child) {
        }

        public static PrimaryScrollController none(
            Key key = null,
            Widget child = null
        ) {
            return new PrimaryScrollController(key, child);
        }

        public readonly ScrollController controller;

        public static ScrollController of(BuildContext context) {
            PrimaryScrollController result =
                (PrimaryScrollController) context.inheritFromWidgetOfExactType(typeof(PrimaryScrollController));
            return result == null ? null : result.controller;
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return this.controller != ((PrimaryScrollController) oldWidget).controller;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<ScrollController>("controller", this.controller,
                ifNull: "no controller", showName: false));
        }
    }
}