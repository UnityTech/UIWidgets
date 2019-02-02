using System;
using System.Collections;
using System.Collections.Generic;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {

    public class IconButton : StatelessWidget {

        const double _kMinButtonSize = 48.0;

        public IconButton(
            Key key = null,
            double iconSize = 24.0,
            EdgeInsets padding = null,
            Alignment alignment = null,
            Widget icon = null,
            Color color = null,
            Color highlightColor = null,
            Color splashColor = null,
            Color disabledColor = null,
            VoidCallback onPressed = null,
            string tooltip = null
        ) : base(key : key) {
            D.assert(icon != null);
            this.iconSize = iconSize;
            this.alignment = alignment ?? Alignment.center;
            this.padding = padding ?? EdgeInsets.all(8.0);
        }

        public readonly double iconSize;

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
            MaterialDebug.debugCheckHasMaterial(context);
            Color currentColor;
            if (onPressed != null)
                currentColor = color;
            else
                currentColor = disabledColor ?? Theme.of(context).disabledColor;

            Widget result = new ConstrainedBox(
                constraints: new BoxConstraints(minWidth: _kMinButtonSize, minHeight: _kMinButtonSize),
                child: new Padding(
                  padding: padding,
                  child: new SizedBox(
                    height: iconSize,
                    width: iconSize,
                    child: new Align(
                      alignment: alignment,
                      child: IconTheme.merge(
                        data: new IconThemeData(
                          size: iconSize,
                          color: currentColor
                        ),
                        child: icon
                      )
                    )
                  )
                )
            );

            return new InkResponse(
              onTap: (GestureTapCallback)(() => onPressed()),
              child: result,
              highlightColor: highlightColor ?? Theme.of(context).highlightColor,
              splashColor: splashColor ?? Theme.of(context).splashColor,
              radius: Math.Max(
                Material.defaultSplashRadius,
                (iconSize + Math.Min(padding.horizontal, padding.vertical)) * 0.7
                // x 0.5 for diameter -> radius and + 40% overflow derived from other Material apps.
              )
        
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Widget>("icon", icon, showName: false));
            properties.add(new ObjectFlagProperty<VoidCallback>("onPressed", onPressed, ifNull: "disabled"));
            properties.add(new StringProperty("tooltip", tooltip, defaultValue: null, quoted: false));
        }
    }
}