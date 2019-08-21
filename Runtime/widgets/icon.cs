using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    public class Icon : StatelessWidget {
        public Icon(IconData icon,
            Key key = null,
            float? size = null,
            Color color = null
        ) : base(key: key) {
            this.icon = icon;
            this.size = size;
            this.color = color;
        }

        public readonly IconData icon;

        public readonly float? size;

        public readonly Color color;

        public override Widget build(BuildContext context) {
            IconThemeData iconTheme = IconTheme.of(context);
            float iconSize = this.size ?? iconTheme.size.Value;

            if (this.icon == null) {
                return new SizedBox(width: iconSize, height: iconSize);
            }

            float iconOpacity = iconTheme.opacity.Value;
            Color iconColor = this.color ?? iconTheme.color;
            if (iconOpacity != 1.0) {
                iconColor = iconColor.withOpacity(iconColor.opacity * iconOpacity);
            }

            Widget iconWidget = new RichText(
                overflow: TextOverflow.visible,
                text: new TextSpan(
                    text: new string(new[] {(char) this.icon.codePoint}),
                    style: new TextStyle(
                        inherit: false,
                        color: iconColor,
                        fontSize: iconSize,
                        fontFamily: this.icon.fontFamily
                    )
                )
            );

            return new SizedBox(
                width: iconSize,
                height: iconSize,
                child: new Center(
                    child: iconWidget
                )
            );
        }
    }
}