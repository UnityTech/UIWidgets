using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public class IconTheme : InheritedWidget {
        public IconTheme(
            Key key = null,
            IconThemeData data = null,
            Widget child = null
        ) : base(key: key, child: child) {
            D.assert(data != null);
            D.assert(child != null);

            this.data = data;
        }

        public static Widget merge(
            Key key = null,
            IconThemeData data = null,
            Widget child = null
        ) {
            return new Builder(
                builder: context => new IconTheme(
                    key: key,
                    data: _getInheritedIconThemeData(context).merge(data),
                    child: child
                )
            );
        }

        public readonly IconThemeData data;

        public static IconThemeData of(BuildContext context) {
            IconThemeData iconThemeData = _getInheritedIconThemeData(context);
            return iconThemeData.isConcrete ? iconThemeData : IconThemeData.fallback().merge(iconThemeData);
        }

        static IconThemeData _getInheritedIconThemeData(BuildContext context) {
            IconTheme iconTheme = (IconTheme) context.inheritFromWidgetOfExactType(typeof(IconTheme));
            if (iconTheme != null) {
                return iconTheme.data;
            }

            return IconThemeData.fallback();
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return this.data != ((IconTheme) oldWidget).data;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<IconThemeData>("data", this.data, showName: false));
        }
    }
}