using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    static class IconButtonUtils {
        public const float _kMinButtonSize = 48.0f;
    }


    public class IconButton : StatelessWidget {
        public IconButton(
            Key key = null,
            float iconSize = 24.0f,
            EdgeInsets padding = null,
            Alignment alignment = null,
            Widget icon = null,
            Color color = null,
            Color highlightColor = null,
            Color splashColor = null,
            Color disableColor = null,
            VoidCallback onPressed = null,
            string tooltip = null) : base(key: key) {
            D.assert(icon != null);

            this.iconSize = iconSize;
            this.padding = padding ?? EdgeInsets.all(8.0f);
            this.alignment = alignment ?? Alignment.center;
            this.icon = icon;
            this.color = color;
            this.highlightColor = highlightColor;
            this.splashColor = splashColor;
            this.disabledColor = disableColor;
            this.onPressed = onPressed;
            this.tooltip = tooltip;
        }

        public readonly float iconSize;

        public readonly EdgeInsets padding;

        public readonly Alignment alignment;

        public readonly Widget icon;

        public readonly Color color;

        public readonly Color splashColor;

        public readonly Color highlightColor;

        public readonly Color disabledColor;

        public readonly VoidCallback onPressed;

        public readonly string tooltip;

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            Color currentColor;
            if (this.onPressed != null) {
                currentColor = this.color;
            }
            else {
                currentColor = this.disabledColor ?? Theme.of(context).disabledColor;
            }

            Widget result = new ConstrainedBox(
                constraints: new BoxConstraints(minWidth: IconButtonUtils._kMinButtonSize,
                    minHeight: IconButtonUtils._kMinButtonSize),
                child: new Padding(
                    padding: this.padding,
                    child: new SizedBox(
                        height: this.iconSize,
                        width: this.iconSize,
                        child: new Align(
                            alignment: this.alignment,
                            child: IconTheme.merge(
                                data: new IconThemeData(
                                    size: this.iconSize,
                                    color: currentColor),
                                child: this.icon)
                        )
                    )
                )
            );

            if (this.tooltip != null) {
                result = new Tooltip(
                    message: this.tooltip,
                    child: result);
            }

            return new InkResponse(
                onTap: () => {
                    if (this.onPressed != null) {
                        this.onPressed();
                    }
                },
                child: result,
                highlightColor: this.highlightColor ?? Theme.of(context).highlightColor,
                splashColor: this.splashColor ?? Theme.of(context).splashColor,
                radius: Mathf.Max(
                    Material.defaultSplashRadius,
                    (this.iconSize + Mathf.Min(this.padding.horizontal, this.padding.vertical)) * 0.7f)
            );
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Widget>("icon", this.icon, showName: false));
            properties.add(new ObjectFlagProperty<VoidCallback>("onPressed", this.onPressed, ifNull: "disabled"));
            properties.add(new StringProperty("tooltip", this.tooltip, defaultValue: null, quoted: false));
        }
    }
}