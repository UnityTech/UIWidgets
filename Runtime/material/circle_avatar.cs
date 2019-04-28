using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace UIWidgetsGallery.gallery {
    public class CircleAvatar : StatelessWidget {
        public CircleAvatar(
            Key key = null,
            Widget child = null,
            Color backgroundColor = null,
            ImageProvider backgroundImage = null,
            Color foregroundColor = null,
            float? radius = null,
            float? minRadius = null,
            float? maxRadius = null
        ) : base(key: key) {
            D.assert(radius == null || (minRadius == null && maxRadius == null));
            this.child = child;
            this.backgroundColor = backgroundColor;
            this.backgroundImage = backgroundImage;
            this.foregroundColor = foregroundColor;
            this.radius = radius;
            this.minRadius = minRadius;
            this.maxRadius = maxRadius;
        }

        public readonly Widget child;

        public readonly Color backgroundColor;

        public readonly Color foregroundColor;

        public readonly ImageProvider backgroundImage;

        public readonly float? radius;

        public readonly float? minRadius;

        public readonly float? maxRadius;

        const float _defaultRadius = 20.0f;

        const float _defaultMinRadius = 0.0f;

        const float _defaultMaxRadius = float.PositiveInfinity;

        float _minDiameter {
            get {
                if (this.radius == null && this.minRadius == null && this.maxRadius == null) {
                    return _defaultRadius * 2.0f;
                }

                return 2.0f * (this.radius ?? this.minRadius ?? _defaultMinRadius);
            }
        }

        float _maxDiameter {
            get {
                if (this.radius == null && this.minRadius == null && this.maxRadius == null) {
                    return _defaultRadius * 2.0f;
                }

                return 2.0f * (this.radius ?? this.maxRadius ?? _defaultMaxRadius);
            }
        }

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            ThemeData theme = Theme.of(context);
            TextStyle textStyle = theme.primaryTextTheme.subhead.copyWith(color: this.foregroundColor);
            Color effectiveBackgroundColor = this.backgroundColor;
            if (effectiveBackgroundColor == null) {
                switch (ThemeData.estimateBrightnessForColor(textStyle.color)) {
                    case Brightness.dark:
                        effectiveBackgroundColor = theme.primaryColorLight;
                        break;
                    case Brightness.light:
                        effectiveBackgroundColor = theme.primaryColorDark;
                        break;
                }
            }
            else if (this.foregroundColor == null) {
                switch (ThemeData.estimateBrightnessForColor(this.backgroundColor)) {
                    case Brightness.dark:
                        textStyle = textStyle.copyWith(color: theme.primaryColorLight);
                        break;
                    case Brightness.light:
                        textStyle = textStyle.copyWith(color: theme.primaryColorDark);
                        break;
                }
            }

            float minDiameter = this._minDiameter;
            float maxDiameter = this._maxDiameter;
            return new AnimatedContainer(
                constraints: new BoxConstraints(
                    minHeight: minDiameter,
                    minWidth: minDiameter,
                    maxWidth: maxDiameter,
                    maxHeight: maxDiameter
                ),
                duration: Constants.kThemeChangeDuration,
                decoration: new BoxDecoration(
                    color: effectiveBackgroundColor,
                    image: this.backgroundImage != null
                        ? new DecorationImage(image: this.backgroundImage, fit: BoxFit.cover)
                        : null,
                    shape: BoxShape.circle
                ),
                child: this.child == null
                    ? null
                    : new Center(
                        child: new MediaQuery(
                            data: MediaQuery.of(context).copyWith(textScaleFactor: 1.0f),
                            child: new IconTheme(
                                data: theme.iconTheme.copyWith(color: textStyle.color),
                                child: new DefaultTextStyle(
                                    style: textStyle,
                                    child: this.child
                                )
                            )
                        )
                    )
            );
        }
    }
}