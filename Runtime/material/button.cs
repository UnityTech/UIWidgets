using System;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class RawMaterialButton : StatefulWidget {
        public RawMaterialButton(
            Key key = null,
            VoidCallback onPressed = null,
            ValueChanged<bool> onHighlightChanged = null,
            TextStyle textStyle = null,
            Color fillColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            double elevation = 2.0,
            double highlightElevation = 8.0,
            double disabledElevation = 0.0,
            EdgeInsets padding = null,
            BoxConstraints constraints = null,
            ShapeBorder shape = null,
            TimeSpan? animationDuration = null,
            Clip clipBehavior = Clip.none,
            MaterialTapTargetSize? materialTapTargetSize = null,
            Widget child = null) : base(key: key) {
            MaterialTapTargetSize _materialTapTargetSize = materialTapTargetSize ?? MaterialTapTargetSize.padded;
            shape = shape ?? new RoundedRectangleBorder();
            padding = padding ?? EdgeInsets.zero;
            constraints = constraints ?? new BoxConstraints(minWidth: 88.0, minHeight: 36.0);
            TimeSpan _animationDuration = animationDuration ?? Constants.kThemeChangeDuration;

            this.onPressed = onPressed;
            this.onHighlightChanged = onHighlightChanged;
            this.textStyle = textStyle;
            this.fillColor = fillColor;
            this.highlightColor = highlightColor;
            this.splashColor = splashColor;
            this.elevation = elevation;
            this.highlightElevation = highlightElevation;
            this.disabledElevation = disabledElevation;
            this.padding = padding;
            this.constraints = constraints;
            this.shape = shape;
            this.animationDuration = _animationDuration;
            this.clipBehavior = clipBehavior;
            this.materialTapTargetSize = _materialTapTargetSize;
            this.child = child;
        }

        public readonly VoidCallback onPressed;

        public readonly ValueChanged<bool> onHighlightChanged;

        public readonly TextStyle textStyle;

        public readonly Color fillColor;

        public readonly Color highlightColor;

        public readonly Color splashColor;

        public readonly double elevation;

        public readonly double highlightElevation;

        public readonly double disabledElevation;

        public readonly EdgeInsets padding;

        public readonly BoxConstraints constraints;

        public readonly ShapeBorder shape;

        public readonly TimeSpan animationDuration;

        public readonly Widget child;

        public bool enabled {
            get { return this.onPressed != null; }
        }

        public readonly MaterialTapTargetSize materialTapTargetSize;

        public readonly Clip clipBehavior;

        public override State createState() {
            return new _RawMaterialButtonState();
        }
    }


    class _RawMaterialButtonState : State<RawMaterialButton> {
        bool _highlight = false;

        void _handleHighlightChanged(bool value) {
            this.setState(() => {
                this._highlight = value;
                if (this.widget.onHighlightChanged != null) {
                    this.widget.onHighlightChanged(value);
                }
            });
        }

        public override Widget build(BuildContext context) {
            double elevation = this.widget.enabled
                ? (this._highlight ? this.widget.highlightElevation : this.widget.elevation)
                : this.widget.disabledElevation;

            Widget result = new ConstrainedBox(
                constraints: this.widget.constraints,
                child: new Material(
                    elevation: elevation,
                    textStyle: this.widget.textStyle,
                    shape: this.widget.shape,
                    color: this.widget.fillColor,
                    type: this.widget.fillColor == null ? MaterialType.transparency : MaterialType.button,
                    animationDuration: this.widget.animationDuration,
                    clipBehavior: this.widget.clipBehavior,
                    child: new InkWell(
                        onHighlightChanged: this._handleHighlightChanged,
                        splashColor: this.widget.splashColor,
                        highlightColor: this.widget.highlightColor,
                        onTap: () => {
                            if (this.widget.onPressed != null) {
                                this.widget.onPressed();
                            }
                        },
                        customBorder: this.widget.shape,
                        child: IconTheme.merge(
                            data: new IconThemeData(color: this.widget.textStyle?.color),
                            child: new Container(
                                padding: this.widget.padding,
                                child: new Center(
                                    widthFactor: 1.0,
                                    heightFactor: 1.0,
                                    child: this.widget.child)
                            )
                        )
                    )
                )
            );

            return result;
        }
    }
}