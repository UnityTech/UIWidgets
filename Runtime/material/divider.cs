using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class Divider : StatelessWidget {
        public Divider(
            Key key = null,
            float height = 16.0f,
            float indent = 0.0f,
            Color color = null) : base(key: key) {
            D.assert(height >= 0.0);
            this.height = height;
            this.indent = indent;
            this.color = color;
        }

        public readonly float height;

        public readonly float indent;

        public readonly Color color;

        public static BorderSide createBorderSide(BuildContext context, Color color = null, float width = 0.0f) {
            return new BorderSide(
                color: color ?? Theme.of(context).dividerColor,
                width: width);
        }

        public override Widget build(BuildContext context) {
            return new SizedBox(
                height: this.height,
                child: new Center(
                    child: new Container(
                        height: 0.0f,
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