using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class SliderTheme : InheritedWidget {
        public SliderTheme(
            Key key = null,
            SliderThemeData data = null,
            Widget child = null)
            : base(key: key, child: child) {
            D.assert(child != null);
            D.assert(data != null);
            this.data = data;
        }

        public readonly SliderThemeData data;

        public static SliderThemeData of(BuildContext context) {
            SliderTheme inheritedTheme = (SliderTheme) context.inheritFromWidgetOfExactType(typeof(SliderTheme));
            return inheritedTheme != null ? inheritedTheme.data : Theme.of(context).sliderTheme;
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget) {
            SliderTheme _oldWidget = (SliderTheme) oldWidget;
            return this.data != _oldWidget.data;
        }
    }


    public enum ShowValueIndicator {
        onlyForDiscrete,
        onlyForContinuous,
        always,
        never
    }

    public class SliderThemeData : Diagnosticable {
        public SliderThemeData(
            float? trackHeight = null,
            Color activeTrackColor = null,
            Color inactiveTrackColor = null,
            Color disabledActiveTrackColor = null,
            Color disabledInactiveTrackColor = null,
            Color activeTickMarkColor = null,
            Color inactiveTickMarkColor = null,
            Color disabledActiveTickMarkColor = null,
            Color disabledInactiveTickMarkColor = null,
            Color thumbColor = null,
            Color disabledThumbColor = null,
            Color overlayColor = null,
            Color valueIndicatorColor = null,
            SliderTrackShape trackShape = null,
            SliderTickMarkShape tickMarkShape = null,
            SliderComponentShape thumbShape = null,
            SliderComponentShape overlayShape = null,
            SliderComponentShape valueIndicatorShape = null,
            ShowValueIndicator? showValueIndicator = null,
            TextStyle valueIndicatorTextStyle = null
        ) {
            D.assert(trackHeight != null);
            D.assert(activeTrackColor != null);
            D.assert(inactiveTrackColor != null);
            D.assert(disabledActiveTrackColor != null);
            D.assert(disabledInactiveTrackColor != null);
            D.assert(activeTickMarkColor != null);
            D.assert(inactiveTickMarkColor != null);
            D.assert(disabledActiveTickMarkColor != null);
            D.assert(disabledInactiveTickMarkColor != null);
            D.assert(thumbColor != null);
            D.assert(disabledThumbColor != null);
            D.assert(overlayColor != null);
            D.assert(valueIndicatorColor != null);
            D.assert(trackShape != null);
            D.assert(tickMarkShape != null);
            D.assert(thumbShape != null);
            D.assert(overlayShape != null);
            D.assert(valueIndicatorShape != null);
            D.assert(valueIndicatorTextStyle != null);
            D.assert(showValueIndicator != null);
            this.trackHeight = trackHeight.Value;
            this.activeTrackColor = activeTrackColor;
            this.inactiveTrackColor = inactiveTrackColor;
            this.disabledActiveTrackColor = disabledActiveTrackColor;
            this.disabledInactiveTrackColor = disabledInactiveTrackColor;
            this.activeTickMarkColor = activeTickMarkColor;
            this.inactiveTickMarkColor = inactiveTickMarkColor;
            this.disabledActiveTickMarkColor = disabledActiveTickMarkColor;
            this.disabledInactiveTickMarkColor = disabledInactiveTickMarkColor;
            this.thumbColor = thumbColor;
            this.disabledThumbColor = disabledThumbColor;
            this.overlayColor = overlayColor;
            this.valueIndicatorColor = valueIndicatorColor;
            this.trackShape = trackShape;
            this.tickMarkShape = tickMarkShape;
            this.thumbShape = thumbShape;
            this.overlayShape = overlayShape;
            this.valueIndicatorShape = valueIndicatorShape;
            this.showValueIndicator = showValueIndicator.Value;
            this.valueIndicatorTextStyle = valueIndicatorTextStyle;
        }

        public static SliderThemeData fromPrimaryColors(
            Color primaryColor = null,
            Color primaryColorDark = null,
            Color primaryColorLight = null,
            TextStyle valueIndicatorTextStyle = null) {
            D.assert(primaryColor != null);
            D.assert(primaryColorDark != null);
            D.assert(primaryColorLight != null);
            D.assert(valueIndicatorTextStyle != null);

            const int activeTrackAlpha = 0xff;
            const int inactiveTrackAlpha = 0x3d; // 24% opacity
            const int disabledActiveTrackAlpha = 0x52; // 32% opacity
            const int disabledInactiveTrackAlpha = 0x1f; // 12% opacity
            const int activeTickMarkAlpha = 0x8a; // 54% opacity
            const int inactiveTickMarkAlpha = 0x8a; // 54% opacity
            const int disabledActiveTickMarkAlpha = 0x1f; // 12% opacity
            const int disabledInactiveTickMarkAlpha = 0x1f; // 12% opacity
            const int thumbAlpha = 0xff;
            const int disabledThumbAlpha = 0x52; // 32% opacity
            const int valueIndicatorAlpha = 0xff;

            const int overlayLightAlpha = 0x29;

            return new SliderThemeData(
                trackHeight: 2.0f,
                activeTrackColor: primaryColor.withAlpha(activeTrackAlpha),
                inactiveTrackColor: primaryColor.withAlpha(inactiveTrackAlpha),
                disabledActiveTrackColor: primaryColorDark.withAlpha(disabledActiveTrackAlpha),
                disabledInactiveTrackColor: primaryColorDark.withAlpha(disabledInactiveTrackAlpha),
                activeTickMarkColor: primaryColorLight.withAlpha(activeTickMarkAlpha),
                inactiveTickMarkColor: primaryColor.withAlpha(inactiveTickMarkAlpha),
                disabledActiveTickMarkColor: primaryColorLight.withAlpha(disabledActiveTickMarkAlpha),
                disabledInactiveTickMarkColor: primaryColorDark.withAlpha(disabledInactiveTickMarkAlpha),
                thumbColor: primaryColor.withAlpha(thumbAlpha),
                disabledThumbColor: primaryColorDark.withAlpha(disabledThumbAlpha),
                overlayColor: primaryColor.withAlpha(overlayLightAlpha),
                valueIndicatorColor: primaryColor.withAlpha(valueIndicatorAlpha),
                trackShape: new RectangularSliderTrackShape(),
                tickMarkShape: new RoundSliderTickMarkShape(),
                thumbShape: new RoundSliderThumbShape(),
                overlayShape: new RoundSliderOverlayShape(),
                valueIndicatorShape: new PaddleSliderValueIndicatorShape(),
                valueIndicatorTextStyle: valueIndicatorTextStyle,
                showValueIndicator: ShowValueIndicator.onlyForDiscrete
            );
        }

        public readonly float trackHeight;

        public readonly Color activeTrackColor;

        public readonly Color inactiveTrackColor;

        public readonly Color disabledActiveTrackColor;

        public readonly Color disabledInactiveTrackColor;

        public readonly Color activeTickMarkColor;

        public readonly Color inactiveTickMarkColor;

        public readonly Color disabledActiveTickMarkColor;

        public readonly Color disabledInactiveTickMarkColor;

        public readonly Color thumbColor;

        public readonly Color disabledThumbColor;

        public readonly Color overlayColor;

        public readonly Color valueIndicatorColor;

        public readonly SliderTrackShape trackShape;

        public readonly SliderTickMarkShape tickMarkShape;

        public readonly SliderComponentShape overlayShape;

        public readonly SliderComponentShape thumbShape;

        public readonly SliderComponentShape valueIndicatorShape;

        public readonly ShowValueIndicator showValueIndicator;

        public readonly TextStyle valueIndicatorTextStyle;

        public SliderThemeData copyWith(
            float? trackHeight = null,
            Color activeTrackColor = null,
            Color inactiveTrackColor = null,
            Color disabledActiveTrackColor = null,
            Color disabledInactiveTrackColor = null,
            Color activeTickMarkColor = null,
            Color inactiveTickMarkColor = null,
            Color disabledActiveTickMarkColor = null,
            Color disabledInactiveTickMarkColor = null,
            Color thumbColor = null,
            Color disabledThumbColor = null,
            Color overlayColor = null,
            Color valueIndicatorColor = null,
            SliderTrackShape trackShape = null,
            SliderTickMarkShape tickMarkShape = null,
            SliderComponentShape thumbShape = null,
            SliderComponentShape overlayShape = null,
            SliderComponentShape valueIndicatorShape = null,
            ShowValueIndicator? showValueIndicator = null,
            TextStyle valueIndicatorTextStyle = null
        ) {
            return new SliderThemeData(
                trackHeight: trackHeight ?? this.trackHeight,
                activeTrackColor: activeTrackColor ?? this.activeTrackColor,
                inactiveTrackColor: inactiveTrackColor ?? this.inactiveTrackColor,
                disabledActiveTrackColor: disabledActiveTrackColor ?? this.disabledActiveTrackColor,
                disabledInactiveTrackColor: disabledInactiveTrackColor ?? this.disabledInactiveTrackColor,
                activeTickMarkColor: activeTickMarkColor ?? this.activeTickMarkColor,
                inactiveTickMarkColor: inactiveTickMarkColor ?? this.inactiveTickMarkColor,
                disabledActiveTickMarkColor: disabledActiveTickMarkColor ?? this.disabledActiveTickMarkColor,
                disabledInactiveTickMarkColor: disabledInactiveTickMarkColor ?? this.disabledInactiveTickMarkColor,
                thumbColor: thumbColor ?? this.thumbColor,
                disabledThumbColor: disabledThumbColor ?? this.disabledThumbColor,
                overlayColor: overlayColor ?? this.overlayColor,
                valueIndicatorColor: valueIndicatorColor ?? this.valueIndicatorColor,
                trackShape: trackShape ?? this.trackShape,
                tickMarkShape: tickMarkShape ?? this.tickMarkShape,
                thumbShape: thumbShape ?? this.thumbShape,
                overlayShape: overlayShape ?? this.overlayShape,
                valueIndicatorShape: valueIndicatorShape ?? this.valueIndicatorShape,
                showValueIndicator: showValueIndicator ?? this.showValueIndicator,
                valueIndicatorTextStyle: valueIndicatorTextStyle ?? this.valueIndicatorTextStyle
            );
        }

        public static SliderThemeData lerp(SliderThemeData a, SliderThemeData b, float t) {
            D.assert(a != null);
            D.assert(b != null);
            return new SliderThemeData(
                trackHeight: MathUtils.lerpFloat(a.trackHeight, b.trackHeight, t),
                activeTrackColor: Color.lerp(a.activeTrackColor, b.activeTrackColor, t),
                inactiveTrackColor: Color.lerp(a.inactiveTrackColor, b.inactiveTrackColor, t),
                disabledActiveTrackColor: Color.lerp(a.disabledActiveTrackColor, b.disabledActiveTrackColor, t),
                disabledInactiveTrackColor: Color.lerp(a.disabledInactiveTrackColor, b.disabledInactiveTrackColor, t),
                activeTickMarkColor: Color.lerp(a.activeTickMarkColor, b.activeTickMarkColor, t),
                inactiveTickMarkColor: Color.lerp(a.inactiveTickMarkColor, b.inactiveTickMarkColor, t),
                disabledActiveTickMarkColor: Color.lerp(a.disabledActiveTickMarkColor, b.disabledActiveTickMarkColor,
                    t),
                disabledInactiveTickMarkColor: Color.lerp(a.disabledInactiveTickMarkColor,
                    b.disabledInactiveTickMarkColor, t),
                thumbColor: Color.lerp(a.thumbColor, b.thumbColor, t),
                disabledThumbColor: Color.lerp(a.disabledThumbColor, b.disabledThumbColor, t),
                overlayColor: Color.lerp(a.overlayColor, b.overlayColor, t),
                valueIndicatorColor: Color.lerp(a.valueIndicatorColor, b.valueIndicatorColor, t),
                trackShape: t < 0.5 ? a.trackShape : b.trackShape,
                tickMarkShape: t < 0.5 ? a.tickMarkShape : b.tickMarkShape,
                thumbShape: t < 0.5 ? a.thumbShape : b.thumbShape,
                overlayShape: t < 0.5 ? a.overlayShape : b.overlayShape,
                valueIndicatorShape: t < 0.5 ? a.valueIndicatorShape : b.valueIndicatorShape,
                showValueIndicator: t < 0.5 ? a.showValueIndicator : b.showValueIndicator,
                valueIndicatorTextStyle: TextStyle.lerp(a.valueIndicatorTextStyle, b.valueIndicatorTextStyle, t)
            );
        }

        public bool Equals(SliderThemeData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return other.trackHeight == this.trackHeight
                   && other.activeTrackColor == this.activeTrackColor
                   && other.inactiveTrackColor == this.inactiveTrackColor
                   && other.disabledActiveTrackColor == this.disabledActiveTrackColor
                   && other.disabledInactiveTrackColor == this.disabledInactiveTrackColor
                   && other.activeTickMarkColor == this.activeTickMarkColor
                   && other.inactiveTickMarkColor == this.inactiveTickMarkColor
                   && other.disabledActiveTickMarkColor == this.disabledActiveTickMarkColor
                   && other.disabledInactiveTickMarkColor == this.disabledInactiveTickMarkColor
                   && other.thumbColor == this.thumbColor
                   && other.disabledThumbColor == this.disabledThumbColor
                   && other.overlayColor == this.overlayColor
                   && other.valueIndicatorColor == this.valueIndicatorColor
                   && other.trackShape == this.trackShape
                   && other.tickMarkShape == this.tickMarkShape
                   && other.thumbShape == this.thumbShape
                   && other.overlayShape == this.overlayShape
                   && other.valueIndicatorShape == this.valueIndicatorShape
                   && other.showValueIndicator == this.showValueIndicator
                   && other.valueIndicatorTextStyle == this.valueIndicatorTextStyle;
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

            return this.Equals((SliderThemeData) obj);
        }

        public static bool operator ==(SliderThemeData left, SliderThemeData right) {
            return Equals(left, right);
        }

        public static bool operator !=(SliderThemeData left, SliderThemeData right) {
            return !Equals(left, right);
        }

        int? _cachedHashCode = null;

        public override int GetHashCode() {
            if (this._cachedHashCode != null) {
                return this._cachedHashCode.Value;
            }

            unchecked {
                var hashCode = this.trackHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ this.activeTrackColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.inactiveTrackColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.disabledActiveTrackColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.disabledInactiveTrackColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.activeTickMarkColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.inactiveTickMarkColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.disabledActiveTickMarkColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.disabledInactiveTickMarkColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.thumbColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.disabledThumbColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.overlayColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.valueIndicatorColor.GetHashCode();
                hashCode = (hashCode * 397) ^ this.trackShape.GetHashCode();
                hashCode = (hashCode * 397) ^ this.tickMarkShape.GetHashCode();
                hashCode = (hashCode * 397) ^ this.thumbShape.GetHashCode();
                hashCode = (hashCode * 397) ^ this.overlayShape.GetHashCode();
                hashCode = (hashCode * 397) ^ this.valueIndicatorShape.GetHashCode();
                hashCode = (hashCode * 397) ^ this.showValueIndicator.GetHashCode();
                hashCode = (hashCode * 397) ^ this.valueIndicatorTextStyle.GetHashCode();

                this._cachedHashCode = hashCode;
                return hashCode;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            ThemeData defaultTheme = ThemeData.fallback();
            SliderThemeData defaultData = fromPrimaryColors(
                primaryColor: defaultTheme.primaryColor,
                primaryColorDark: defaultTheme.primaryColorDark,
                primaryColorLight: defaultTheme.primaryColorLight,
                valueIndicatorTextStyle: defaultTheme.accentTextTheme.body2
            );
            properties.add(new DiagnosticsProperty<Color>("activeTrackColor", this.activeTrackColor,
                defaultValue: defaultData.activeTrackColor));
            properties.add(new DiagnosticsProperty<Color>("activeTrackColor", this.activeTrackColor,
                defaultValue: defaultData.activeTrackColor));
            properties.add(new DiagnosticsProperty<Color>("inactiveTrackColor", this.inactiveTrackColor,
                defaultValue: defaultData.inactiveTrackColor));
            properties.add(new DiagnosticsProperty<Color>("disabledActiveTrackColor", this.disabledActiveTrackColor,
                defaultValue: defaultData.disabledActiveTrackColor, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("disabledInactiveTrackColor", this.disabledInactiveTrackColor,
                defaultValue: defaultData.disabledInactiveTrackColor, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("activeTickMarkColor", this.activeTickMarkColor,
                defaultValue: defaultData.activeTickMarkColor, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("inactiveTickMarkColor", this.inactiveTickMarkColor,
                defaultValue: defaultData.inactiveTickMarkColor, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("disabledActiveTickMarkColor",
                this.disabledActiveTickMarkColor, defaultValue: defaultData.disabledActiveTickMarkColor,
                level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("disabledInactiveTickMarkColor",
                this.disabledInactiveTickMarkColor, defaultValue: defaultData.disabledInactiveTickMarkColor,
                level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("thumbColor", this.thumbColor,
                defaultValue: defaultData.thumbColor));
            properties.add(new DiagnosticsProperty<Color>("disabledThumbColor", this.disabledThumbColor,
                defaultValue: defaultData.disabledThumbColor, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("overlayColor", this.overlayColor,
                defaultValue: defaultData.overlayColor, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("valueIndicatorColor", this.valueIndicatorColor,
                defaultValue: defaultData.valueIndicatorColor));
            properties.add(new DiagnosticsProperty<SliderTrackShape>("trackShape", this.trackShape,
                defaultValue: defaultData.trackShape, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<SliderTickMarkShape>("tickMarkShape", this.tickMarkShape,
                defaultValue: defaultData.tickMarkShape, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<SliderComponentShape>("thumbShape", this.thumbShape,
                defaultValue: defaultData.thumbShape, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<SliderComponentShape>("overlayShape", this.overlayShape,
                defaultValue: defaultData.overlayShape, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<SliderComponentShape>("valueIndicatorShape",
                this.valueIndicatorShape, defaultValue: defaultData.valueIndicatorShape, level: DiagnosticLevel.debug));
            properties.add(new EnumProperty<ShowValueIndicator>("showValueIndicator", this.showValueIndicator,
                defaultValue: defaultData.showValueIndicator));
            properties.add(new DiagnosticsProperty<TextStyle>("valueIndicatorTextStyle", this.valueIndicatorTextStyle,
                defaultValue: defaultData.valueIndicatorTextStyle));
        }
    }

    public abstract class SliderTrackShape {
        public SliderTrackShape() {
        }

        public abstract Rect getPreferredRect(
            RenderBox parentBox = null,
            Offset offset = null,
            SliderThemeData sliderTheme = null,
            bool? isEnabled = null,
            bool? isDiscrete = null);

        public abstract void paint(
            PaintingContext context,
            Offset offset,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset thumbCenter = null,
            bool? isEnabled = null,
            bool? isDiscrete = null
        );
    }

    public abstract class SliderTickMarkShape {
        public SliderTickMarkShape() {
        }

        public abstract Size getPreferredSize(
            SliderThemeData sliderTheme = null,
            bool? isEnabled = null);

        public abstract void paint(
            PaintingContext context,
            Offset offset,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset thumbCenter = null,
            bool? isEnabled = null);

        public static readonly SliderTickMarkShape noTickMark = new _EmptySliderTickMarkShape();
    }


    class _EmptySliderTickMarkShape : SliderTickMarkShape {
        public override Size getPreferredSize(
            SliderThemeData sliderTheme = null,
            bool? isEnabled = null) {
            return Size.zero;
        }

        public override void paint(
            PaintingContext context,
            Offset offset,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset thumbCenter = null,
            bool? isEnabled = null) {
        }
    }

    public abstract class SliderComponentShape {
        public SliderComponentShape() {
        }

        public abstract Size getPreferredSize(
            bool? isEnabled,
            bool? isDiscrete);

        public abstract void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool? isDiscrete = null,
            TextPainter labelPainter = null,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            float? value = null);

        public static readonly SliderComponentShape noThumb = new _EmptySliderComponentShape();

        public static readonly SliderComponentShape noOverlay = new _EmptySliderComponentShape();
    }

    class _EmptySliderComponentShape : SliderComponentShape {
        public override Size getPreferredSize(
            bool? isEnabled,
            bool? isDiscrete) {
            return Size.zero;
        }

        public override void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool? isDiscrete = null,
            TextPainter labelPainter = null,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            float? value = null) {
        }
    }

    public class RectangularSliderTrackShape : SliderTrackShape {
        public RectangularSliderTrackShape(
            float disabledThumbGapWidth = 2.0f) {
            this.disabledThumbGapWidth = disabledThumbGapWidth;
        }

        public readonly float disabledThumbGapWidth;


        public override Rect getPreferredRect(
            RenderBox parentBox = null,
            Offset offset = null,
            SliderThemeData sliderTheme = null,
            bool? isEnabled = null,
            bool? isDiscrete = null) {
            float overlayWidth = sliderTheme.overlayShape.getPreferredSize(isEnabled, isDiscrete).width;
            float trackHeight = sliderTheme.trackHeight;
            D.assert(overlayWidth >= 0);
            D.assert(trackHeight >= 0);
            D.assert(parentBox.size.width >= overlayWidth);
            D.assert(parentBox.size.height >= trackHeight);

            float trackLeft = offset.dx + overlayWidth / 2f;
            float trackTop = offset.dy + (parentBox.size.height - trackHeight) / 2f;
            float trackWidth = parentBox.size.width - overlayWidth;
            return Rect.fromLTWH(trackLeft, trackTop, trackWidth, trackHeight);
        }

        public override void paint(
            PaintingContext context,
            Offset offset,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset thumbCenter = null,
            bool? isEnabled = null,
            bool? isDiscrete = null) {
            if (sliderTheme.trackHeight == 0) {
                return;
            }

            ColorTween activeTrackColorTween = new ColorTween(begin: sliderTheme.disabledActiveTrackColor,
                end: sliderTheme.activeTrackColor);
            ColorTween inactiveTrackColorTween = new ColorTween(begin: sliderTheme.disabledInactiveTrackColor,
                end: sliderTheme.inactiveTrackColor);
            Paint activePaint = new Paint {color = activeTrackColorTween.evaluate(enableAnimation)};
            Paint inactivePaint = new Paint {color = inactiveTrackColorTween.evaluate(enableAnimation)};
            Paint leftTrackPaint = activePaint;
            Paint rightTrackPaint = inactivePaint;

            float horizontalAdjustment = 0.0f;
            if (!isEnabled.Value) {
                float disabledThumbRadius = sliderTheme.thumbShape.getPreferredSize(false, isDiscrete).width / 2.0f;
                float gap = this.disabledThumbGapWidth * (1.0f - enableAnimation.value);
                horizontalAdjustment = disabledThumbRadius + gap;
            }

            Rect trackRect = this.getPreferredRect(
                parentBox: parentBox,
                offset: offset,
                sliderTheme: sliderTheme,
                isEnabled: isEnabled,
                isDiscrete: isDiscrete
            );

            Rect leftTrackSegment = Rect.fromLTRB(trackRect.left, trackRect.top, thumbCenter.dx - horizontalAdjustment,
                trackRect.bottom);
            context.canvas.drawRect(leftTrackSegment, leftTrackPaint);

            Rect rightTrackSegment = Rect.fromLTRB(thumbCenter.dx + horizontalAdjustment, trackRect.top,
                trackRect.right, trackRect.bottom);
            context.canvas.drawRect(rightTrackSegment, rightTrackPaint);
        }
    }

    public class RoundSliderTickMarkShape : SliderTickMarkShape {
        public RoundSliderTickMarkShape(
            float? tickMarkRadius = null) {
            this.tickMarkRadius = tickMarkRadius;
        }

        public readonly float? tickMarkRadius;


        public override Size getPreferredSize(
            SliderThemeData sliderTheme = null,
            bool? isEnabled = null
        ) {
            return Size.fromRadius(this.tickMarkRadius ?? sliderTheme.trackHeight / 2f);
        }


        public override void paint(
            PaintingContext context,
            Offset center,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            Animation<float> enableAnimation = null,
            Offset thumbCenter = null,
            bool? isEnabled = null
        ) {
            Color begin;
            Color end;

            bool isTickMarkRightOfThumb = center.dx > thumbCenter.dx;
            begin = isTickMarkRightOfThumb
                ? sliderTheme.disabledInactiveTickMarkColor
                : sliderTheme.disabledActiveTickMarkColor;
            end = isTickMarkRightOfThumb ? sliderTheme.inactiveTickMarkColor : sliderTheme.activeTickMarkColor;

            Paint paint = new Paint {color = new ColorTween(begin: begin, end: end).evaluate(enableAnimation)};

            float tickMarkRadius = this.getPreferredSize(
                                       isEnabled: isEnabled,
                                       sliderTheme: sliderTheme
                                   ).width / 2f;
            context.canvas.drawCircle(center, tickMarkRadius, paint);
        }
    }

    public class RoundSliderThumbShape : SliderComponentShape {
        public RoundSliderThumbShape(
            float enabledThumbRadius = 6.0f,
            float? disabledThumbRadius = null
        ) {
            this.enabledThumbRadius = enabledThumbRadius;
            this.disabledThumbRadius = disabledThumbRadius;
        }

        public readonly float enabledThumbRadius;

        public readonly float? disabledThumbRadius;

        float _disabledThumbRadius {
            get { return this.disabledThumbRadius ?? this.enabledThumbRadius * 2f / 3f; }
        }


        public override Size getPreferredSize(bool? isEnabled, bool? isDiscrete) {
            return Size.fromRadius(isEnabled.Value ? this.enabledThumbRadius : this._disabledThumbRadius);
        }


        public override void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool? isDiscrete = null,
            TextPainter labelPainter = null,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            float? value = null
        ) {
            Canvas canvas = context.canvas;
            FloatTween radiusTween = new FloatTween(
                begin: this._disabledThumbRadius,
                end: this.enabledThumbRadius
            );
            ColorTween colorTween = new ColorTween(
                begin: sliderTheme.disabledThumbColor,
                end: sliderTheme.thumbColor
            );
            canvas.drawCircle(
                center,
                radiusTween.evaluate(enableAnimation),
                new Paint {color = colorTween.evaluate(enableAnimation)}
            );
        }
    }

    public class RoundSliderOverlayShape : SliderComponentShape {
        public RoundSliderOverlayShape(
            float overlayRadius = 16.0f) {
            this.overlayRadius = overlayRadius;
        }

        public readonly float overlayRadius;

        public override Size getPreferredSize(bool? isEnabled, bool? isDiscrete) {
            return Size.fromRadius(this.overlayRadius);
        }

        public override void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool? isDiscrete = null,
            TextPainter labelPainter = null,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            float? value = null
        ) {
            Canvas canvas = context.canvas;
            FloatTween radiusTween = new FloatTween(
                begin: 0.0f,
                end: this.overlayRadius
            );
            canvas.drawCircle(
                center,
                radiusTween.evaluate(activationAnimation),
                new Paint {color = sliderTheme.overlayColor}
            );
        }
    }

    public class PaddleSliderValueIndicatorShape : SliderComponentShape {
        public PaddleSliderValueIndicatorShape() {
        }

        const float _topLobeRadius = 16.0f;
        const float _labelTextDesignSize = 14.0f;
        const float _bottomLobeRadius = 6.0f;
        const float _bottomLobeStartAngle = -1.1f * Mathf.PI / 4.0f;
        const float _bottomLobeEndAngle = 1.1f * 5f * Mathf.PI / 4.0f;
        const float _labelPadding = 8.0f;
        const float _distanceBetweenTopBottomCenters = 40.0f;
        static readonly Offset _topLobeCenter = new Offset(0.0f, -_distanceBetweenTopBottomCenters);
        const float _topNeckRadius = 14.0f;

        const float _neckTriangleHypotenuse = _topLobeRadius + _topNeckRadius;

        const float _twoSeventyDegrees = 3.0f * Mathf.PI / 2.0f;
        const float _ninetyDegrees = Mathf.PI / 2.0f;
        const float _thirtyDegrees = Mathf.PI / 6.0f;

        static readonly Size _preferredSize =
            Size.fromHeight(_distanceBetweenTopBottomCenters + _topLobeRadius + _bottomLobeRadius);

        const bool _debuggingLabelLocation = false;

        static Path _bottomLobePath;
        static Offset _bottomLobeEnd;


        public override Size getPreferredSize(
            bool? isEnabled,
            bool? isDiscrete) {
            return _preferredSize;
        }

        static void _addArc(Path path, Offset center, float radius, float startAngle, float endAngle) {
            Rect arcRect = Rect.fromCircle(center: center, radius: radius);
            path.arcTo(arcRect, startAngle, endAngle - startAngle, false);
        }

        static void _generateBottomLobe() {
            const float bottomNeckRadius = 4.5f;
            const float bottomNeckStartAngle = _bottomLobeEndAngle - Mathf.PI;
            const float bottomNeckEndAngle = 0.0f;

            Path path = new Path();
            Offset bottomKnobStart = new Offset(
                _bottomLobeRadius * Mathf.Cos(_bottomLobeStartAngle),
                _bottomLobeRadius * Mathf.Sin(_bottomLobeStartAngle)
            );
            Offset bottomNeckRightCenter = bottomKnobStart +
                                           new Offset(
                                               bottomNeckRadius * Mathf.Cos(bottomNeckStartAngle),
                                               -bottomNeckRadius * Mathf.Sin(bottomNeckStartAngle)
                                           );
            Offset bottomNeckLeftCenter = new Offset(
                -bottomNeckRightCenter.dx,
                bottomNeckRightCenter.dy
            );

            Offset bottomNeckStartRight = new Offset(
                bottomNeckRightCenter.dx - bottomNeckRadius,
                bottomNeckRightCenter.dy
            );

            path.moveTo(bottomNeckStartRight.dx, bottomNeckStartRight.dy);
            _addArc(
                path,
                bottomNeckRightCenter,
                bottomNeckRadius,
                Mathf.PI - bottomNeckEndAngle,
                Mathf.PI - bottomNeckStartAngle
            );
            _addArc(
                path,
                Offset.zero,
                _bottomLobeRadius,
                _bottomLobeStartAngle,
                _bottomLobeEndAngle
            );
            _addArc(
                path,
                bottomNeckLeftCenter,
                bottomNeckRadius,
                bottomNeckStartAngle,
                bottomNeckEndAngle
            );

            _bottomLobeEnd = new Offset(
                -bottomNeckStartRight.dx,
                bottomNeckStartRight.dy
            );

            _bottomLobePath = path;
        }


        Offset _addBottomLobe(Path path) {
            if (_bottomLobePath == null || _bottomLobeEnd == null) {
                _generateBottomLobe();
            }

            path.addPath(_bottomLobePath, Offset.zero);
            return _bottomLobeEnd;
        }

        float _getIdealOffset(
            RenderBox parentBox,
            float halfWidthNeeded,
            float scale,
            Offset center
        ) {
            const float edgeMargin = 4.0f;
            Rect topLobeRect = Rect.fromLTWH(
                -_topLobeRadius - halfWidthNeeded,
                -_topLobeRadius - _distanceBetweenTopBottomCenters,
                2.0f * (_topLobeRadius + halfWidthNeeded),
                2.0f * _topLobeRadius
            );

            Offset topLeft = (topLobeRect.topLeft * scale) + center;
            Offset bottomRight = (topLobeRect.bottomRight * scale) + center;
            float shift = 0.0f;
            if (topLeft.dx < edgeMargin) {
                shift = edgeMargin - topLeft.dx;
            }

            if (bottomRight.dx > parentBox.size.width - edgeMargin) {
                shift = parentBox.size.width - bottomRight.dx - edgeMargin;
            }

            shift = scale == 0.0f ? 0.0f : shift / scale;
            return shift;
        }

        void _drawValueIndicator(
            RenderBox parentBox,
            Canvas canvas,
            Offset center,
            Paint paint,
            float scale,
            TextPainter labelPainter
        ) {
            canvas.save();
            canvas.translate(center.dx, center.dy);

            float textScaleFactor = labelPainter.height / _labelTextDesignSize;
            float overallScale = scale * textScaleFactor;
            canvas.scale(overallScale, overallScale);
            float inverseTextScale = textScaleFactor != 0 ? 1.0f / textScaleFactor : 0.0f;
            float labelHalfWidth = labelPainter.width / 2.0f;

            float halfWidthNeeded = Mathf.Max(
                0.0f,
                inverseTextScale * labelHalfWidth - (_topLobeRadius - _labelPadding)
            );

            float shift = this._getIdealOffset(parentBox, halfWidthNeeded, overallScale, center);
            float leftWidthNeeded;
            float rightWidthNeeded;
            if (shift < 0.0) {
                shift = Mathf.Max(shift, -halfWidthNeeded);
            }
            else {
                shift = Mathf.Min(shift, halfWidthNeeded);
            }

            rightWidthNeeded = halfWidthNeeded + shift;
            leftWidthNeeded = halfWidthNeeded - shift;

            Path path = new Path();
            Offset bottomLobeEnd = this._addBottomLobe(path);

            float neckTriangleBase = _topNeckRadius - bottomLobeEnd.dx;

            float leftAmount = Mathf.Max(0.0f, Mathf.Min(1.0f, leftWidthNeeded / neckTriangleBase));
            float rightAmount = Mathf.Max(0.0f, Mathf.Min(1.0f, rightWidthNeeded / neckTriangleBase));

            float leftTheta = (1.0f - leftAmount) * _thirtyDegrees;
            float rightTheta = (1.0f - rightAmount) * _thirtyDegrees;
            Offset neckLeftCenter = new Offset(
                -neckTriangleBase,
                _topLobeCenter.dy + Mathf.Cos(leftTheta) * _neckTriangleHypotenuse
            );
            Offset neckRightCenter = new Offset(
                neckTriangleBase,
                _topLobeCenter.dy + Mathf.Cos(rightTheta) * _neckTriangleHypotenuse
            );

            float leftNeckArcAngle = _ninetyDegrees - leftTheta;
            float rightNeckArcAngle = Mathf.PI + _ninetyDegrees - rightTheta;

            float neckStretchBaseline = bottomLobeEnd.dy - Mathf.Max(neckLeftCenter.dy, neckRightCenter.dy);
            float t = Mathf.Pow(inverseTextScale, 3.0f);
            float stretch = (neckStretchBaseline * t).clamp(0.0f, 10.0f * neckStretchBaseline);
            Offset neckStretch = new Offset(0.0f, neckStretchBaseline - stretch);

            D.assert(() => {
                if (!_debuggingLabelLocation) {
                    return true;
                }

#pragma warning disable 0162
                Offset leftCenter = _topLobeCenter - new Offset(leftWidthNeeded, 0.0f) + neckStretch;
                Offset rightCenter = _topLobeCenter + new Offset(rightWidthNeeded, 0.0f) + neckStretch;
                Rect valueRect = Rect.fromLTRB(
                    leftCenter.dx - _topLobeRadius,
                    leftCenter.dy - _topLobeRadius,
                    rightCenter.dx + _topLobeRadius,
                    rightCenter.dy + _topLobeRadius
                );
                Paint outlinePaint = new Paint();
                outlinePaint.color = new Color(0xffff0000);
                outlinePaint.style = PaintingStyle.stroke;
                outlinePaint.strokeWidth = 1.0f;
                canvas.drawRect(valueRect, outlinePaint);
                return true;
#pragma warning restore 0162
            });

            _addArc(
                path,
                neckLeftCenter + neckStretch,
                _topNeckRadius,
                0.0f,
                -leftNeckArcAngle
            );
            _addArc(
                path,
                _topLobeCenter - new Offset(leftWidthNeeded, 0.0f) + neckStretch,
                _topLobeRadius,
                _ninetyDegrees + leftTheta,
                _twoSeventyDegrees
            );
            _addArc(
                path,
                _topLobeCenter + new Offset(rightWidthNeeded, 0.0f) + neckStretch,
                _topLobeRadius,
                _twoSeventyDegrees,
                _twoSeventyDegrees + Mathf.PI - rightTheta
            );
            _addArc(
                path,
                neckRightCenter + neckStretch,
                _topNeckRadius,
                rightNeckArcAngle,
                Mathf.PI
            );
            canvas.drawPath(path, paint);

            canvas.save();
            canvas.translate(shift, -_distanceBetweenTopBottomCenters + neckStretch.dy);
            canvas.scale(inverseTextScale, inverseTextScale);
            labelPainter.paint(canvas, Offset.zero - new Offset(labelHalfWidth, labelPainter.height / 2.0f));
            canvas.restore();
            canvas.restore();
        }

        public override void paint(
            PaintingContext context,
            Offset center,
            Animation<float> activationAnimation = null,
            Animation<float> enableAnimation = null,
            bool? isDiscrete = null,
            TextPainter labelPainter = null,
            RenderBox parentBox = null,
            SliderThemeData sliderTheme = null,
            float? value = null) {
            ColorTween enableColor = new ColorTween(
                begin: sliderTheme.disabledThumbColor,
                end: sliderTheme.valueIndicatorColor
            );
            this._drawValueIndicator(
                parentBox,
                context.canvas,
                center,
                new Paint {color = enableColor.evaluate(enableAnimation)},
                activationAnimation.value,
                labelPainter
            );
        }
    }
}