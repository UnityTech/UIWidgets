using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.material {
    class OutlineButtonConstants {
        public static readonly TimeSpan _kPressDuration = new TimeSpan(0, 0, 0, 0, 150);

        public static readonly TimeSpan _kElevationDuration = new TimeSpan(0, 0, 0, 0, 75);
    }

    public class OutlineButton : MaterialButton {
        public OutlineButton(
            Key key = null,
            VoidCallback onPressed = null,
            ButtonTextTheme? textTheme = null,
            Color textColor = null,
            Color disabledTextColor = null,
            Color color = null,
            Color highlightColor = null,
            Color splashColor = null,
            float? highlightElevation = null,
            BorderSide borderSide = null,
            Color disabledBorderColor = null,
            Color highlightedBorderColor = null,
            EdgeInsets padding = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            Widget child = null
        ) :
            base(
                key: key,
                onPressed: onPressed,
                textTheme: textTheme,
                textColor: textColor,
                disabledTextColor: disabledTextColor,
                color: color,
                highlightColor: highlightColor,
                splashColor: splashColor,
                highlightElevation: highlightElevation,
                padding: padding,
                shape: shape,
                clipBehavior: clipBehavior,
                child: child
            ) {
            D.assert(highlightElevation == null || highlightElevation >= 0.0f);
            this.highlightedBorderColor = highlightedBorderColor;
            this.disabledBorderColor = disabledBorderColor;
            this.borderSide = borderSide;
        }

        public static OutlineButton icon(
            Key key = null,
            VoidCallback onPressed = null,
            ButtonTextTheme? textTheme = null,
            Color textColor = null,
            Color disabledTextColor = null,
            Color color = null,
            Color highlightColor = null,
            Color splashColor = null,
            float? highlightElevation = null,
            Color highlightedBorderColor = null,
            Color disabledBorderColor = null,
            BorderSide borderSide = null,
            EdgeInsets padding = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            Widget icon = null,
            Widget label = null
        ) {
            return new _OutlineButtonWithIcon(
                key,
                onPressed,
                textTheme,
                textColor,
                disabledTextColor,
                color,
                highlightColor,
                splashColor,
                highlightElevation,
                highlightedBorderColor,
                disabledBorderColor,
                borderSide,
                padding,
                shape,
                clipBehavior,
                icon,
                label
            );
        }

        public readonly Color highlightedBorderColor;

        public readonly Color disabledBorderColor;

        public readonly BorderSide borderSide;

        public override Widget build(BuildContext context) {
            ButtonThemeData buttonTheme = ButtonTheme.of(context);
            return new _OutlineButton(
                onPressed: this.onPressed,
                brightness: buttonTheme.getBrightness(this),
                textTheme: this.textTheme,
                textColor: buttonTheme.getTextColor(this),
                disabledTextColor: buttonTheme.getDisabledTextColor(this),
                color: this.color,
                highlightColor: buttonTheme.getHighlightColor(this),
                splashColor: buttonTheme.getSplashColor(this),
                highlightElevation: buttonTheme.getHighlightElevation(this),
                borderSide: this.borderSide,
                disabledBorderColor: this.disabledBorderColor,
                highlightedBorderColor: this.highlightedBorderColor ?? buttonTheme.colorScheme.primary,
                padding: buttonTheme.getPadding(this),
                shape: buttonTheme.getShape(this),
                clipBehavior: this.clipBehavior,
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
            properties.add(new DiagnosticsProperty<Color>("highlightColor", this.highlightColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("splashColor", this.splashColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<float?>("highlightElevation", this.highlightElevation,
                defaultValue: null));
            properties.add(new DiagnosticsProperty<BorderSide>("borderSide", this.borderSide, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("disabledBorderColor", this.disabledBorderColor,
                defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("highlightedBorderColor", this.highlightedBorderColor,
                defaultValue: null));
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding", this.padding, defaultValue: null));
            properties.add(new DiagnosticsProperty<ShapeBorder>("shape", this.shape, defaultValue: null));
        }
    }

    class _OutlineButtonWithIcon : OutlineButton, MaterialButtonWithIconMixin {
        public _OutlineButtonWithIcon(
            Key key = null,
            VoidCallback onPressed = null,
            ButtonTextTheme? textTheme = null,
            Color textColor = null,
            Color disabledTextColor = null,
            Color color = null,
            Color highlightColor = null,
            Color splashColor = null,
            float? highlightElevation = null,
            Color highlightedBorderColor = null,
            Color disabledBorderColor = null,
            BorderSide borderSide = null,
            EdgeInsets padding = null,
            ShapeBorder shape = null,
            Clip clipBehavior = Clip.none,
            Widget icon = null,
            Widget label = null
        ) :
            base(
                key: key,
                onPressed: onPressed,
                textTheme: textTheme,
                textColor: textColor,
                disabledTextColor: disabledTextColor,
                color: color,
                highlightColor: highlightColor,
                splashColor: splashColor,
                highlightElevation: highlightElevation,
                disabledBorderColor: disabledBorderColor,
                highlightedBorderColor: highlightedBorderColor,
                borderSide: borderSide,
                padding: padding,
                shape: shape,
                clipBehavior: clipBehavior,
                child: new Row(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget> {
                        icon,
                        new SizedBox(width: 8.0f),
                        label
                    }
                )
            ) {
            D.assert(highlightElevation == null || highlightElevation >= 0.0f);
            D.assert(icon != null);
            D.assert(label != null);
        }
    }

    class _OutlineButton : StatefulWidget {
        public _OutlineButton(
            Key key = null,
            VoidCallback onPressed = null,
            Brightness? brightness = null,
            ButtonTextTheme? textTheme = null,
            Color textColor = null,
            Color disabledTextColor = null,
            Color color = null,
            Color highlightColor = null,
            Color splashColor = null,
            float? highlightElevation = null,
            BorderSide borderSide = null,
            Color disabledBorderColor = null,
            Color highlightedBorderColor = null,
            EdgeInsets padding = null,
            ShapeBorder shape = null,
            Clip? clipBehavior = Clip.none,
            Widget child = null
        ) : base(key: key) {
            D.assert(highlightElevation != null && highlightElevation >= 0.0f);
            D.assert(highlightedBorderColor != null);
            this.onPressed = onPressed;
            this.brightness = brightness;
            this.textTheme = textTheme;
            this.textColor = textColor;
            this.disabledTextColor = disabledTextColor;
            this.color = color;
            this.highlightColor = highlightColor;
            this.splashColor = splashColor;
            this.highlightElevation = highlightElevation;
            this.borderSide = borderSide;
            this.disabledBorderColor = disabledBorderColor;
            this.highlightedBorderColor = highlightedBorderColor;
            this.padding = padding;
            this.shape = shape;
            this.clipBehavior = clipBehavior;
            this.child = child;
        }

        public readonly VoidCallback onPressed;
        public readonly Brightness? brightness;
        public readonly ButtonTextTheme? textTheme;
        public readonly Color textColor;
        public readonly Color disabledTextColor;
        public readonly Color color;
        public readonly Color highlightColor;
        public readonly Color splashColor;
        public readonly float? highlightElevation;
        public readonly BorderSide borderSide;
        public readonly Color disabledBorderColor;
        public readonly Color highlightedBorderColor;
        public readonly EdgeInsets padding;
        public readonly ShapeBorder shape;
        public readonly Clip? clipBehavior;
        public readonly Widget child;

        public bool enabled {
            get { return this.onPressed != null; }
        }

        public override State createState() {
            return new _OutlineButtonState();
        }
    }


    class _OutlineButtonState : SingleTickerProviderStateMixin<_OutlineButton> {
        AnimationController _controller;
        Animation<float> _fillAnimation;
        Animation<float> _elevationAnimation;
        bool _pressed = false;

        public override void initState() {
            base.initState();


            this._controller = new AnimationController(
                duration: OutlineButtonConstants._kPressDuration,
                vsync: this
            );
            this._fillAnimation = new CurvedAnimation(
                parent: this._controller,
                curve: new Interval(0.0f, 0.5f,
                    curve: Curves.fastOutSlowIn
                )
            );
            this._elevationAnimation = new CurvedAnimation(
                parent: this._controller,
                curve: new Interval(0.5f, 0.5f),
                reverseCurve: new Interval(1.0f, 1.0f)
            );
        }

        public override void didUpdateWidget(StatefulWidget _oldWidget) {
            _OutlineButton oldWidget = _oldWidget as _OutlineButton;
            base.didUpdateWidget(oldWidget);
            if (this._pressed && !this.widget.enabled) {
                this._pressed = false;
                this._controller.reverse();
            }
        }

        void _handleHighlightChanged(bool value) {
            if (this._pressed == value) {
                return;
            }

            this.setState(() => {
                this._pressed = value;
                if (value) {
                    this._controller.forward();
                }
                else {
                    this._controller.reverse();
                }
            });
        }

        public override void dispose() {
            this._controller.dispose();
            base.dispose();
        }

        Color _getFillColor() {
            if (this.widget.highlightElevation == null || this.widget.highlightElevation == 0.0) {
                return Colors.transparent;
            }

            Color color = this.widget.color ?? Theme.of(this.context).canvasColor;
            ColorTween colorTween = new ColorTween(
                begin: color.withAlpha(0x00),
                end: color.withAlpha(0xFF)
            );
            return colorTween.evaluate(this._fillAnimation);
        }

        BorderSide _getOutline() {
            if (this.widget.borderSide?.style == BorderStyle.none) {
                return this.widget.borderSide;
            }

            Color specifiedColor = this.widget.enabled
                ? (this._pressed ? this.widget.highlightedBorderColor : null) ?? this.widget.borderSide?.color
                : this.widget.disabledBorderColor;

            Color themeColor = Theme.of(this.context).colorScheme.onSurface.withOpacity(0.12f);

            return new BorderSide(
                color: specifiedColor ?? themeColor,
                width: this.widget.borderSide?.width ?? 1.0f
            );
        }

        float _getHighlightElevation() {
            if (this.widget.highlightElevation == null || this.widget.highlightElevation == 0.0f) {
                return 0.0f;
            }

            return new FloatTween(
                begin: 0.0f,
                end: this.widget.highlightElevation ?? 2.0f
            ).evaluate(this._elevationAnimation);
        }

        public override Widget build(BuildContext context) {
            return new AnimatedBuilder(
                animation: this._controller,
                builder: (BuildContext _context, Widget child) => {
                    return new RaisedButton(
                        textColor: this.widget.textColor,
                        disabledTextColor: this.widget.disabledTextColor,
                        color: this._getFillColor(),
                        splashColor: this.widget.splashColor,
                        highlightColor: this.widget.highlightColor,
                        disabledColor: Colors.transparent,
                        onPressed: this.widget.onPressed,
                        elevation: 0.0f,
                        disabledElevation: 0.0f,
                        highlightElevation: this._getHighlightElevation(),
                        onHighlightChanged: this._handleHighlightChanged,
                        padding:
                        this.widget.padding,
                        shape: new _OutlineBorder(
                            shape: this.widget.shape,
                            side: this._getOutline()
                        ),
                        clipBehavior:
                        this.widget.clipBehavior,
                        animationDuration: OutlineButtonConstants._kElevationDuration,
                        child:
                        this.widget.child
                    );
                }
            );
        }
    }

    class _OutlineBorder : ShapeBorder {
        public _OutlineBorder(
            ShapeBorder shape,
            BorderSide side
        ) {
            D.assert(shape != null);
            D.assert(side != null);
            this.shape = shape;
            this.side = side;
        }

        public readonly ShapeBorder shape;
        public readonly BorderSide side;

        public override EdgeInsets dimensions {
            get { return EdgeInsets.all(this.side.width); }
        }

        public override ShapeBorder scale(float t) {
            return new _OutlineBorder(
                shape: this.shape.scale(t),
                side: this.side.scale(t)
            );
        }

        public override ShapeBorder lerpFrom(ShapeBorder a, float t) {
            if (a is _OutlineBorder) {
                return new _OutlineBorder(
                    side: BorderSide.lerp((a as _OutlineBorder).side, this.side, t),
                    shape: lerp((a as _OutlineBorder).shape, this.shape, t)
                );
            }

            return base.lerpFrom(a, t);
        }

        public override ShapeBorder lerpTo(ShapeBorder b, float t) {
            if (b is _OutlineBorder) {
                return new _OutlineBorder(
                    side: BorderSide.lerp(this.side, (b as _OutlineBorder).side, t),
                    shape: lerp(this.shape, (b as _OutlineBorder).shape, t)
                );
            }

            return base.lerpTo(b, t);
        }

        public override Path getInnerPath(Rect rect) {
            return this.shape.getInnerPath(rect.deflate(this.side.width));
        }

        public override Path getOuterPath(Rect rect) {
            return this.shape.getOuterPath(rect);
        }

        public override void paint(Canvas canvas, Rect rect) {
            switch (this.side.style) {
                case BorderStyle.none:
                    break;
                case BorderStyle.solid:
                    canvas.drawPath(this.shape.getOuterPath(rect), this.side.toPaint());
                    break;
            }
        }

        public static bool operator ==(_OutlineBorder left, _OutlineBorder right) {
            return left.Equals(right);
        }

        public static bool operator !=(_OutlineBorder left, _OutlineBorder right) {
            return !left.Equals(right);
        }

        public bool Equals(_OutlineBorder other) {
            return this.side == other.side && this.shape == other.shape;
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

            return this.Equals((_OutlineBorder) obj);
        }

        public override int GetHashCode() {
            return (this.shape.GetHashCode() * 397) ^ this.side.GetHashCode();
        }
    }
}