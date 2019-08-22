using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public class MaterialButton : StatelessWidget {
        public MaterialButton(
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
            float? elevation = null,
            float? highlightElevation = null,
            float? disabledElevation = null,
            EdgeInsets padding = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = Clip.none,
            MaterialTapTargetSize? materialTapTargetSize = null,
            TimeSpan? animationDuration = null,
            float? minWidth = null,
            float? height = null,
            Widget child = null
        ) : base(key: key) {
            this.onPressed = onPressed;
            this.onHighlightChanged = onHighlightChanged;
            this.textTheme = textTheme;
            this.textColor = textColor;
            this.disabledTextColor = disabledTextColor;
            this.color = color;
            this.disabledColor = disabledColor;
            this.highlightColor = highlightColor;
            this.splashColor = splashColor;
            this.colorBrightness = colorBrightness;
            this.elevation = elevation;
            this.highlightElevation = highlightElevation;
            this.disabledElevation = disabledElevation;
            this.padding = padding;
            this.shape = shape;
            this.clipBehavior = clipBehavior;
            this.materialTapTargetSize = materialTapTargetSize;
            this.animationDuration = animationDuration;
            this.minWidth = minWidth;
            this.height = height;
            this.child = child;
        }

        public readonly VoidCallback onPressed;

        public readonly ValueChanged<bool> onHighlightChanged;

        public readonly ButtonTextTheme? textTheme;

        public readonly Color textColor;

        public readonly Color disabledTextColor;

        public readonly Color color;

        public readonly Color disabledColor;

        public readonly Color splashColor;

        public readonly Color highlightColor;

        public readonly float? elevation;

        public readonly float? highlightElevation;

        public readonly float? disabledElevation;

        public readonly Brightness? colorBrightness;

        public readonly Widget child;

        public bool enabled {
            get { return this.onPressed != null; }
        }

        public readonly EdgeInsets padding;

        public readonly ShapeBorder shape;

        public readonly Clip? clipBehavior;

        public readonly TimeSpan? animationDuration;

        public readonly MaterialTapTargetSize? materialTapTargetSize;

        public readonly float? minWidth;

        public readonly float? height;

        public override Widget build(BuildContext context) {
            ThemeData theme = Theme.of(context);
            ButtonThemeData buttonTheme = ButtonTheme.of(context);

            return new RawMaterialButton(
                onPressed: this.onPressed,
                onHighlightChanged: this.onHighlightChanged,
                fillColor: buttonTheme.getFillColor(this),
                textStyle: theme.textTheme.button.copyWith(color: buttonTheme.getTextColor(this)),
                highlightColor: this.highlightColor ?? theme.highlightColor,
                splashColor: this.splashColor ?? theme.splashColor,
                elevation: buttonTheme.getElevation(this),
                highlightElevation: buttonTheme.getHighlightElevation(this),
                padding: buttonTheme.getPadding(this),
                constraints: buttonTheme.getConstraints(this).copyWith(
                    minWidth: this.minWidth,
                    minHeight: this.height),
                shape: buttonTheme.getShape(this),
                clipBehavior: this.clipBehavior ?? Clip.none,
                animationDuration: buttonTheme.getAnimationDuration(this),
                child: this.child,
                materialTapTargetSize: this.materialTapTargetSize ?? theme.materialTapTargetSize);
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("enabled", value: this.enabled, ifFalse: "disabled"));
        }
    }


    public interface MaterialButtonWithIconMixin {
    }
}