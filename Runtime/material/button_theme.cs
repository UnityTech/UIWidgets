using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    public enum ButtonTextTheme {
        normal,

        accent,

        primary
    }

    public enum ButtonBarLayoutBehavior {
        constrained,

        padded
    }

    public class ButtonTheme : InheritedWidget {
        public ButtonTheme(
            Key key = null,
            ButtonTextTheme textTheme = ButtonTextTheme.normal,
            ButtonBarLayoutBehavior layoutBehavior = ButtonBarLayoutBehavior.padded,
            float minWidth = 88.0f,
            float height = 36.0f,
            EdgeInsets padding = null,
            ShapeBorder shape = null,
            bool alignedDropdown = false,
            Color buttonColor = null,
            Color disabledColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            ColorScheme colorScheme = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            Widget child = null) : base(key: key, child: child) {
            D.assert(minWidth >= 0.0);
            D.assert(height >= 0.0);
            this.data = new ButtonThemeData(
                textTheme: textTheme,
                minWidth: minWidth,
                height: height,
                padding: padding,
                shape: shape,
                alignedDropdown: alignedDropdown,
                layoutBehavior: layoutBehavior,
                buttonColor: buttonColor,
                disabledColor: disabledColor,
                highlightColor: highlightColor,
                splashColor: splashColor,
                colorScheme: colorScheme,
                materialTapTargetSize: materialTapTargetSize);
        }

        public ButtonTheme(
            Key key = null,
            ButtonThemeData data = null,
            Widget child = null) : base(key: key, child: child) {
            D.assert(data != null);
            this.data = data;
        }

        public static ButtonTheme fromButtonThemeData(
            Key key = null,
            ButtonThemeData data = null,
            Widget child = null) {
            return new ButtonTheme(key, data, child);
        }

        public static ButtonTheme bar(
            Key key = null,
            ButtonTextTheme textTheme = ButtonTextTheme.accent,
            float minWidth = 64.0f,
            float height = 36.0f,
            EdgeInsets padding = null,
            ShapeBorder shape = null,
            bool alignedDropdown = false,
            Color buttonColor = null,
            Color disabledColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            ColorScheme colorScheme = null,
            Widget child = null,
            ButtonBarLayoutBehavior layoutBehavior = ButtonBarLayoutBehavior.padded
        ) {
            D.assert(minWidth >= 0.0);
            D.assert(height >= 0.0);
            return new ButtonTheme(key, new ButtonThemeData(
                textTheme: textTheme,
                minWidth: minWidth,
                height: height,
                padding: padding ?? EdgeInsets.symmetric(horizontal: 8.0f),
                shape: shape,
                alignedDropdown: alignedDropdown,
                layoutBehavior: layoutBehavior,
                buttonColor: buttonColor,
                disabledColor: disabledColor,
                highlightColor: highlightColor,
                splashColor: splashColor,
                colorScheme: colorScheme
            ), child);
        }


        public readonly ButtonThemeData data;

        public static ButtonThemeData of(BuildContext context) {
            ButtonTheme inheritedButtonTheme = (ButtonTheme) context.inheritFromWidgetOfExactType(typeof(ButtonTheme));
            ButtonThemeData buttonTheme = inheritedButtonTheme?.data;
            if (buttonTheme?.colorScheme == null) {
                ThemeData theme = Theme.of(context);
                buttonTheme = buttonTheme ?? theme.buttonTheme;
                if (buttonTheme.colorScheme == null) {
                    buttonTheme = buttonTheme.copyWith(
                        colorScheme: theme.buttonTheme.colorScheme ?? theme.colorScheme);
                    D.assert(buttonTheme.colorScheme != null);
                }
            }

            return buttonTheme;
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            return this.data != ((ButtonTheme) oldWidget).data;
        }
    }


    public class ButtonThemeData : Diagnosticable, IEquatable<ButtonThemeData> {
        public ButtonThemeData(
            ButtonTextTheme textTheme = ButtonTextTheme.normal,
            float minWidth = 88.0f,
            float height = 36.0f,
            EdgeInsets padding = null,
            ShapeBorder shape = null,
            ButtonBarLayoutBehavior layoutBehavior = ButtonBarLayoutBehavior.padded,
            bool alignedDropdown = false,
            Color buttonColor = null,
            Color disabledColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            ColorScheme colorScheme = null,
            MaterialTapTargetSize? materialTapTargetSize = null
        ) {
            D.assert(minWidth >= 0.0);
            D.assert(height >= 0.0);
            this.textTheme = textTheme;
            this.minWidth = minWidth;
            this.height = height;
            this.layoutBehavior = layoutBehavior;
            this.alignedDropdown = alignedDropdown;
            this.colorScheme = colorScheme;
            this._buttonColor = buttonColor;
            this._disabledColor = disabledColor;
            this._highlightColor = highlightColor;
            this._splashColor = splashColor;
            this._padding = padding;
            this._shape = shape;
            this._materialTapTargetSize = materialTapTargetSize;
        }


        public readonly float minWidth;

        public readonly float height;

        public readonly ButtonTextTheme textTheme;

        public readonly ButtonBarLayoutBehavior layoutBehavior;

        public BoxConstraints constraints {
            get {
                return new BoxConstraints(minWidth: this.minWidth,
                    minHeight: this.height);
            }
        }

        public EdgeInsets padding {
            get {
                if (this._padding != null) {
                    return this._padding;
                }

                switch (this.textTheme) {
                    case ButtonTextTheme.normal:
                    case ButtonTextTheme.accent:
                        return EdgeInsets.symmetric(horizontal: 16.0f);
                    case ButtonTextTheme.primary:
                        return EdgeInsets.symmetric(horizontal: 24.0f);
                }

                D.assert(false);
                return EdgeInsets.zero;
            }
        }

        readonly EdgeInsets _padding;

        public ShapeBorder shape {
            get {
                if (this._shape != null) {
                    return this._shape;
                }

                switch (this.textTheme) {
                    case ButtonTextTheme.normal:
                    case ButtonTextTheme.accent:
                        return new RoundedRectangleBorder(
                            borderRadius: BorderRadius.all(Radius.circular(2.0f)));
                    case ButtonTextTheme.primary:
                        return new RoundedRectangleBorder(
                            borderRadius: BorderRadius.all(Radius.circular(4.0f)));
                }

                return new RoundedRectangleBorder();
            }
        }

        readonly ShapeBorder _shape;

        public readonly bool alignedDropdown;

        readonly Color _buttonColor;

        readonly Color _disabledColor;

        readonly Color _highlightColor;

        readonly Color _splashColor;

        public readonly ColorScheme colorScheme;

        readonly MaterialTapTargetSize? _materialTapTargetSize;

        public Brightness getBrightness(MaterialButton button) {
            return button.colorBrightness ?? this.colorScheme.brightness;
        }

        public ButtonTextTheme getTextTheme(MaterialButton button) {
            return button.textTheme ?? this.textTheme;
        }

        Color _getDisabledColor(MaterialButton button) {
            return this.getBrightness(button) == Brightness.dark
                ? this.colorScheme.onSurface.withOpacity(0.30f)
                : this.colorScheme.onSurface.withOpacity(0.38f);
        }


        public Color getDisabledTextColor(MaterialButton button) {
            if (button.disabledTextColor != null) {
                return button.disabledTextColor;
            }

            return this._getDisabledColor(button);
        }


        Color getDisabledFillColor(MaterialButton button) {
            if (button.disabledColor != null) {
                return button.disabledColor;
            }

            if (this._disabledColor != null) {
                return this._disabledColor;
            }

            return this._getDisabledColor(button);
        }


        public Color getFillColor(MaterialButton button) {
            Color fillColor = button.enabled ? button.color : button.disabledColor;
            if (fillColor != null) {
                return fillColor;
            }

            if (button is FlatButton || button is OutlineButton || button.GetType() == typeof(MaterialButton)) {
                return null;
            }


            if (button.enabled && button is RaisedButton && this._buttonColor != null) {
                return this._buttonColor;
            }

            switch (this.getTextTheme(button)) {
                case ButtonTextTheme.normal:
                case ButtonTextTheme.accent:
                    return button.enabled ? this.colorScheme.primary : this.getDisabledFillColor(button);
                case ButtonTextTheme.primary:
                    return button.enabled
                        ? this._buttonColor ?? this.colorScheme.primary
                        : this.colorScheme.onSurface.withOpacity(0.12f);
            }

            D.assert(false);
            return null;
        }

        public Color getTextColor(MaterialButton button) {
            if (!button.enabled) {
                return this.getDisabledTextColor(button);
            }

            if (button.textColor != null) {
                return button.textColor;
            }

            switch (this.getTextTheme(button)) {
                case ButtonTextTheme.normal:
                    return this.getBrightness(button) == Brightness.dark ? Colors.white : Colors.black87;
                case ButtonTextTheme.accent:
                    return this.colorScheme.secondary;
                case ButtonTextTheme.primary: {
                    Color fillColor = this.getFillColor(button);
                    bool fillIsDark = fillColor != null
                        ? ThemeData.estimateBrightnessForColor(fillColor) == Brightness.dark
                        : this.getBrightness(button) == Brightness.dark;
                    if (fillIsDark) {
                        return Colors.white;
                    }

                    if (button is FlatButton || button is OutlineButton) {
                        return this.colorScheme.primary;
                    }

                    return Colors.black;
                }
            }

            D.assert(false);
            return null;
        }

        public Color getSplashColor(MaterialButton button) {
            if (button.splashColor != null) {
                return button.splashColor;
            }

            if (this._splashColor != null && (button is RaisedButton || button is OutlineButton)) {
                return this._splashColor;
            }

            if (this._splashColor != null && button is FlatButton) {
                switch (this.getTextTheme(button)) {
                    case ButtonTextTheme.normal:
                    case ButtonTextTheme.accent:
                        return this._splashColor;
                    case ButtonTextTheme.primary:
                        break;
                }
            }

            return this.getTextColor(button).withOpacity(0.12f);
        }

        public Color getHighlightColor(MaterialButton button) {
            if (button.highlightColor != null) {
                return button.highlightColor;
            }

            switch (this.getTextTheme(button)) {
                case ButtonTextTheme.normal:
                case ButtonTextTheme.accent:
                    return this._highlightColor ?? this.getTextColor(button).withOpacity(0.16f);
                case ButtonTextTheme.primary:
                    return Colors.transparent;
            }

            D.assert(false);
            return Colors.transparent;
        }


        public float getElevation(MaterialButton button) {
            if (button.elevation != null) {
                return button.elevation ?? 0.0f;
            }

            if (button is FlatButton) {
                return 0.0f;
            }

            return 2.0f;
        }


        public float getHighlightElevation(MaterialButton button) {
            if (button.highlightElevation != null) {
                return button.highlightElevation ?? 0.0f;
            }

            if (button is FlatButton) {
                return 0.0f;
            }

            if (button is OutlineButton) {
                return 0.0f;
            }
            return 8.0f;
        }


        public float getDisabledElevation(MaterialButton button) {
            if (button.disabledElevation != null) {
                return button.disabledElevation ?? 0.0f;
            }

            return 0.0f;
        }


        public EdgeInsets getPadding(MaterialButton button) {
            if (button.padding != null) {
                return button.padding;
            }

            if (button is MaterialButtonWithIconMixin) {
                return EdgeInsets.fromLTRB(12.0f, 0.0f, 16.0f, 0.0f);
            }

            if (this._padding != null) {
                return this._padding;
            }

            switch (this.getTextTheme(button)) {
                case ButtonTextTheme.normal:
                case ButtonTextTheme.accent:
                    return EdgeInsets.symmetric(horizontal: 16.0f);
                case ButtonTextTheme.primary:
                    return EdgeInsets.symmetric(horizontal: 24.0f);
            }

            D.assert(false);
            return EdgeInsets.zero;
        }

        public ShapeBorder getShape(MaterialButton button) {
            return button.shape ?? this.shape;
        }


        public TimeSpan getAnimationDuration(MaterialButton button) {
            return button.animationDuration ?? Constants.kThemeChangeDuration;
        }

        public BoxConstraints getConstraints(MaterialButton button) {
            return this.constraints;
        }


        public MaterialTapTargetSize getMaterialTapTargetSize(MaterialButton button) {
            return button.materialTapTargetSize ?? this._materialTapTargetSize ?? MaterialTapTargetSize.padded;
        }


        public ButtonThemeData copyWith(
            ButtonTextTheme? textTheme = null,
            ButtonBarLayoutBehavior? layoutBehavior = null,
            float? minWidth = null,
            float? height = null,
            EdgeInsets padding = null,
            ShapeBorder shape = null,
            bool? alignedDropdown = null,
            Color buttonColor = null,
            Color disabledColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            ColorScheme colorScheme = null,
            MaterialTapTargetSize? materialTapTargetSize = null) {
            return new ButtonThemeData(
                textTheme: textTheme ?? this.textTheme,
                layoutBehavior: layoutBehavior ?? this.layoutBehavior,
                minWidth: minWidth ?? this.minWidth,
                height: height ?? this.height,
                padding: padding ?? this.padding,
                shape: shape ?? this.shape,
                alignedDropdown: alignedDropdown ?? this.alignedDropdown,
                buttonColor: buttonColor ?? this._buttonColor,
                disabledColor: disabledColor ?? this._disabledColor,
                highlightColor: highlightColor ?? this._highlightColor,
                splashColor: splashColor ?? this._splashColor,
                colorScheme: colorScheme ?? this.colorScheme,
                materialTapTargetSize: materialTapTargetSize ?? this._materialTapTargetSize);
        }

        public bool Equals(ButtonThemeData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return this.textTheme == other.textTheme
                   && this.minWidth == other.minWidth
                   && this.height == other.height
                   && this.padding == other.padding
                   && this.shape == other.shape
                   && this.alignedDropdown == other.alignedDropdown
                   && this._buttonColor == other._buttonColor
                   && this._disabledColor == other._disabledColor
                   && this._highlightColor == other._highlightColor
                   && this._splashColor == other._splashColor
                   && this.colorScheme == other.colorScheme
                   && this._materialTapTargetSize == other._materialTapTargetSize;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((ButtonThemeData) obj);
        }

        public static bool operator ==(ButtonThemeData left, ButtonThemeData right) {
            return Equals(left, right);
        }

        public static bool operator !=(ButtonThemeData left, ButtonThemeData right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = this.textTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this.minWidth.GetHashCode();
                hashCode = (hashCode * 397) ^ this.height.GetHashCode();
                hashCode = (hashCode * 397) ^ this.padding.GetHashCode();
                hashCode = (hashCode * 397) ^ this.shape.GetHashCode();
                hashCode = (hashCode * 397) ^ this.alignedDropdown.GetHashCode();
                hashCode = (hashCode * 397) ^ (this._buttonColor != null ? this._buttonColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this._disabledColor != null ? this._disabledColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this._highlightColor != null ? this._highlightColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this._splashColor != null ? this._splashColor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ this.colorScheme.GetHashCode();
                hashCode = (hashCode * 397) ^ this._materialTapTargetSize.GetHashCode();
                return hashCode;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            ButtonThemeData defaultTheme = new ButtonThemeData();
            properties.add(new EnumProperty<ButtonTextTheme>("textTheme", this.textTheme,
                defaultValue: defaultTheme.textTheme));
            properties.add(new FloatProperty("minWidth", this.minWidth, defaultValue: defaultTheme.minWidth));
            properties.add(new FloatProperty("height", this.height, defaultValue: defaultTheme.height));
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding", this.padding,
                defaultValue: defaultTheme.padding));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", this.shape, defaultValue: defaultTheme.shape));
            properties.add(new FlagProperty("alignedDropdown",
                value: this.alignedDropdown,
                defaultValue: defaultTheme.alignedDropdown,
                ifTrue: "dropdown width matches button"
            ));
            properties.add(new DiagnosticsProperty<Color>("buttonColor", this._buttonColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("disabledColor", this._disabledColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("highlightColor", this._highlightColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("splashColor", this._splashColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<ColorScheme>("colorScheme", this.colorScheme,
                defaultValue: defaultTheme.colorScheme));
            properties.add(new DiagnosticsProperty<MaterialTapTargetSize?>("materialTapTargetSize",
                this._materialTapTargetSize, defaultValue: null));
        }
    }
}