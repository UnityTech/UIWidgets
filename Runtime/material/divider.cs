using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class Divider : StatelessWidget {
        public Divider(
            Key key = null,
            double height = 16.0,
            double indent = 0.0,
            Color color = null) : base(key: key) {
            D.assert(height >= 0.0);
            this.height = height;
            this.indent = indent;
            this.color = color;
        }

        public readonly double height;

        public readonly double indent;

        public readonly Color color;

        public static BorderSide createBorderSide(BuildContext context, Color color = null, double width = 0.0) {
            return new BorderSide(
                color: color ?? Theme.of(context).dividerColor,
                width: width);
        }

        public override Widget build(BuildContext context) {
            return new SizedBox(
                height: this.height,
                child: new Center(
                    child: new Container(
                        height: 0.0,
                        margin: EdgeInsets.only(this.indent),
                        decoration: new BoxDecoration(
                            border: new Border(
                                bottom: createBorderSide(context, color: this.color))
                        )
                    )
                )
            );
        }
    }
}