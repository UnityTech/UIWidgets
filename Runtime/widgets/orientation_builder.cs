using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;

namespace Unity.UIWidgets.widgets {
    public delegate Widget OrientationWidgetBuilder(BuildContext context, Orientation orientation);

    public class OrientationBuilder : StatelessWidget {
        public OrientationBuilder(
            OrientationWidgetBuilder builder,
            Key key = null
        ) : base(key: key) {
            D.assert(builder != null);
            this.builder = builder;
        }

        public readonly OrientationWidgetBuilder builder;

        Widget _buildWithConstraints(BuildContext context, BoxConstraints constraints) {
            Orientation orientation =
                constraints.maxWidth > constraints.maxHeight ? Orientation.landscape : Orientation.portrait;
            return this.builder(context, orientation);
        }

        public override Widget build(BuildContext context) {
            return new LayoutBuilder(builder: this._buildWithConstraints);
        }
    }
}