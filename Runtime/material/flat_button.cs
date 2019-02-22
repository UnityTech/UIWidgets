using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class FlatButton : MaterialButton {
        public FlatButton(
            Key key = null,
            VoidCallback onPressed = null,
            ValueChanged<bool> onHighlightChanged = null,
            ButtonTextTheme? textTheme = null,
            Color textColor = null,
            Color disabledTextColor = null,
            Color color = null,
            Color disabledColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            Brightness? colorBrightness = null,
            EdgeInsets padding = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = Clip.none,
            MaterialTapTargetSize? materialTapTargetSize = null,
            Widget child = null) : base(
            key: key,
            onPressed: onPressed,
            onHighlightChanged: onHighlightChanged,
            textTheme: textTheme,
            textColor: textColor,
            disabledTextColor: disabledTextColor,
            color: color,
            disabledColor: disabledColor,
            highlightColor: highlightColor,
            splashColor: splashColor,
            colorBrightness: colorBrightness,
            padding: padding,
            shape: shape,
            clipBehavior: clipBehavior,
            materialTapTargetSize: materialTapTargetSize,
            child: child) {
        }

        public static FlatButton icon(
            Key key = null,
            VoidCallback onPressed = null,
            ValueChanged<bool> onHighlightChanged = null,
            ButtonTextTheme? textTheme = null,
            Color textColor = null,
            Color disabledTextColor = null,
            Color color = null,
            Color disabledColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            Brightness? colorBrightness = null,
            EdgeInsets padding = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            Widget icon = null,
            Widget label = null) {
            D.assert(icon != null);
            D.assert(label != null);

            return new _FlatButtonWithIcon(
                key: key,
                onPressed: onPressed,
                onHighlightChanged: onHighlightChanged,
                textTheme: textTheme,
                textColor: textColor,
                disabledTextColor: disabledTextColor,
                color: color,
                disabledColor: disabledColor,
                highlightColor: highlightColor,
                splashColor: splashColor,
                colorBrightness: colorBrightness,
                padding: padding,
                shape: shape,
                clipBehavior: clipBehavior,
                materialTapTargetSize: materialTapTargetSize,
                icon: icon,
                label: label
            );
        }

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            ButtonThemeData buttonTheme = ButtonTheme.of(context);

            return new RawMaterialButton(
                onPressed: this.onPressed,
                onHighlightChanged: this.onHighlightChanged,
                clipBehavior: this.clipBehavior ?? Clip.none,
                fillColor: buttonTheme.getFillColor(this),
                textStyle: theme.textTheme.button.copyWith(color: buttonTheme.getTextColor(this)),
                highlightColor: buttonTheme.getHighlightColor(this),
                splashColor: buttonTheme.getSplashColor(this),
                elevation: buttonTheme.getElevation(this),
                highlightElevation: buttonTheme.getHighlightElevation(this),
                disabledElevation: buttonTheme.getDisabledElevation(this),
                padding: buttonTheme.getPadding(this),
                constraints: buttonTheme.getConstraints(this),
                shape: buttonTheme.getShape(this),
                animationDuration: buttonTheme.getAnimationDuration(this),
                materialTapTargetSize: buttonTheme.getMaterialTapTargetSize(this),
                child: this.child
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new ObjectFlagProperty<VoidCallback>("onPressed", this.onPressed, ifNull: "disabled"));
            properties.add(new DiagnosticsProperty<ButtonTextTheme?>("textTheme", this.textTheme, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("textColor", this.textColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("disabledTextColor", this.disabledTextColor,
                defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("color", this.color, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("disabledColor", this.disabledColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("highlightColor", this.highlightColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("splashColor", this.splashColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<Brightness?>("colorBrightness", this.colorBrightness,
                defaultValue: null));
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding", this.padding, defaultValue: null));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", this.shape, defaultValue: null));
            properties.add(new DiagnosticsProperty<MaterialTapTargetSize?>("materialTapTargetSize",
                this.materialTapTargetSize, defaultValue: null));
        }
    }

    class _FlatButtonWithIcon : FlatButton, MaterialButtonWithIconMixin {
        public _FlatButtonWithIcon(
            Key key = null,
            VoidCallback onPressed = null,
            ValueChanged<bool> onHighlightChanged = null,
            ButtonTextTheme? textTheme = null,
            Color textColor = null,
            Color disabledTextColor = null,
            Color color = null,
            Color disabledColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            Brightness? colorBrightness = null,
            EdgeInsets padding = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            Widget icon = null,
            Widget label = null) : base(
            key: key,
            onPressed: onPressed,
            onHighlightChanged: onHighlightChanged,
            textTheme: textTheme,
            textColor: textColor,
            disabledTextColor: disabledTextColor,
            color: color,
            disabledColor: disabledColor,
            highlightColor: highlightColor,
            splashColor: splashColor,
            colorBrightness: colorBrightness,
            padding: padding,
            shape: shape,
            clipBehavior: clipBehavior,
            materialTapTargetSize: materialTapTargetSize,
            child: new Row(
                mainAxisSize: MainAxisSize.min,
                children: new List<Widget> {
                    icon,
                    new SizedBox(width: 8.0f),
                    label
                }
            )) {
            D.assert(icon != null);
            D.assert(label != null);
        }
    }
}