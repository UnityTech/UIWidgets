using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    class _ProgressIndicatorContants {
        public const float _kLinearProgressIndicatorHeight = 6.0f;
        public const float _kMinCircularProgressIndicatorSize = 36.0f;
        public const int _kIndeterminateLinearDuration = 1800;

        public static readonly Animatable<float> _kStrokeHeadTween = new CurveTween(
            curve: new Interval(0.0f, 0.5f, curve: Curves.fastOutSlowIn)
        ).chain(new CurveTween(
            curve: new SawTooth(5)
        ));

        public static readonly Animatable<float> _kStrokeTailTween = new CurveTween(
            curve: new Interval(0.5f, 1.0f, curve: Curves.fastOutSlowIn)
        ).chain(new CurveTween(
            curve: new SawTooth(5)
        ));

        public static readonly Animatable<int> _kStepTween = new StepTween(begin: 0, end: 5);

        public static readonly Animatable<float> _kRotationTween = new CurveTween(curve: new SawTooth(5));
    }

    public abstract class ProgressIndicator : StatefulWidget {
        public ProgressIndicator(
            Key key = null,
            float? value = null,
            Color backgroundColor = null,
            Animation<Color> valueColor = null
        ) : base(key: key) {
            this.value = value;
            this.backgroundColor = backgroundColor;
            this.valueColor = valueColor;
        }

        public readonly float? value;

        public readonly Color backgroundColor;

        public readonly Animation<Color> valueColor;

        public Color _getBackgroundColor(BuildContext context) {
            return this.backgroundColor ?? Theme.of(context).backgroundColor;
        }

        public Color _getValueColor(BuildContext context) {
            return this.valueColor?.value ?? Theme.of(context).accentColor;
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new PercentProperty("value", this.value ?? 0.0f, showName: false,
                ifNull: "<indeterminate>"));
        }
    }

    class _LinearProgressIndicatorPainter : AbstractCustomPainter {
        public _LinearProgressIndicatorPainter(
            Color backgroundColor = null,
            Color valueColor = null,
            float? value = null,
            float? animationValue = null
        ) {
            this.backgroundColor = backgroundColor;
            this.valueColor = valueColor;
            this.value = value;
            this.animationValue = animationValue;
        }

        public readonly Color backgroundColor;
        public readonly Color valueColor;
        public readonly float? value;
        public readonly float? animationValue;

        static readonly Curve line1Head = new Interval(
            0.0f,
            750.0f / _ProgressIndicatorContants._kIndeterminateLinearDuration,
            curve: new Cubic(0.2f, 0.0f, 0.8f, 1.0f)
        );

        static readonly Curve line1Tail = new Interval(
            333.0f / _ProgressIndicatorContants._kIndeterminateLinearDuration,
            (333.0f + 750.0f) / _ProgressIndicatorContants._kIndeterminateLinearDuration,
            curve: new Cubic(0.4f, 0.0f, 1.0f, 1.0f)
        );

        static readonly Curve line2Head = new Interval(
            1000.0f / _ProgressIndicatorContants._kIndeterminateLinearDuration,
            (1000.0f + 567.0f) / _ProgressIndicatorContants._kIndeterminateLinearDuration,
            curve: new Cubic(0.0f, 0.0f, 0.65f, 1.0f)
        );

        static readonly Curve line2Tail = new Interval(
            1267.0f / _ProgressIndicatorContants._kIndeterminateLinearDuration,
            (1267.0f + 533.0f) / _ProgressIndicatorContants._kIndeterminateLinearDuration,
            curve: new Cubic(0.10f, 0.0f, 0.45f, 1.0f)
        );

        public override void paint(Canvas canvas, Size size) {
            Paint paint = new Paint();
            paint.color = this.backgroundColor;
            paint.style = PaintingStyle.fill;
            canvas.drawRect(Offset.zero & size, paint);

            paint.color = this.valueColor;

            void drawBar(float x, float width) {
                if (width <= 0.0f) {
                    return;
                }

                float left = x;
                canvas.drawRect(new Offset(left, 0.0f) & new Size(width, size.height), paint);
            }

            if (this.value != null) {
                drawBar(0.0f, this.value.Value.clamp(0.0f, 1.0f) * size.width);
            }
            else {
                float x1 = size.width * line1Tail.transform(this.animationValue ?? 0.0f);
                float width1 = size.width * line1Head.transform(this.animationValue ?? 0.0f) - x1;

                float x2 = size.width * line2Tail.transform(this.animationValue ?? 0.0f);
                float width2 = size.width * line2Head.transform(this.animationValue ?? 0.0f) - x2;

                drawBar(x1, width1);
                drawBar(x2, width2);
            }
        }

        public override bool shouldRepaint(CustomPainter oldPainter) {
            D.assert(oldPainter is _LinearProgressIndicatorPainter);
            _LinearProgressIndicatorPainter painter = oldPainter as _LinearProgressIndicatorPainter;
            return painter.backgroundColor != this.backgroundColor
                   || painter.valueColor != this.valueColor
                   || painter.value != this.value
                   || painter.animationValue != this.animationValue;
        }
    }

    public class LinearProgressIndicator : ProgressIndicator {
        public LinearProgressIndicator(
            Key key = null,
            float? value = null,
            Color backgroundColor = null,
            Animation<Color> valueColor = null
        ) : base(
            key: key,
            value: value,
            backgroundColor: backgroundColor,
            valueColor: valueColor
        ) {
        }

        public override State createState() {
            return new _LinearProgressIndicatorState();
        }
    }

    class _LinearProgressIndicatorState : SingleTickerProviderStateMixin<LinearProgressIndicator> {
        AnimationController _controller;

        public _LinearProgressIndicatorState() {
        }

        public override void initState() {
            base.initState();
            this._controller = new AnimationController(
                duration: new TimeSpan(0, 0, 0, 0, _ProgressIndicatorContants._kIndeterminateLinearDuration),
                vsync: this
            );
            if (this.widget.value == null) {
                this._controller.repeat();
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (this.widget.value == null && !this._controller.isAnimating) {
                this._controller.repeat();
            }
            else if (this.widget.value != null && this._controller.isAnimating) {
                this._controller.stop();
            }
        }

        public override void dispose() {
            this._controller.dispose();
            base.dispose();
        }

        Widget _buildIndicator(BuildContext context, float animationValue) {
            return new Container(
                constraints: new BoxConstraints(
                    minWidth: float.PositiveInfinity,
                    minHeight: _ProgressIndicatorContants._kLinearProgressIndicatorHeight
                ),
                child: new CustomPaint(
                    painter: new _LinearProgressIndicatorPainter(
                        backgroundColor: this.widget._getBackgroundColor(context),
                        valueColor: this.widget._getValueColor(context),
                        value: this.widget.value,
                        animationValue: animationValue
                    )
                )
            );
        }

        public override Widget build(BuildContext context) {
            if (this.widget.value != null) {
                return this._buildIndicator(context, this._controller.value);
            }

            return new AnimatedBuilder(
                animation: this._controller.view,
                builder: (BuildContext _context, Widget child) => {
                    return this._buildIndicator(_context, this._controller.value);
                }
            );
        }
    }

    class _CircularProgressIndicatorPainter : AbstractCustomPainter {
        public _CircularProgressIndicatorPainter(
            Color backgroundColor = null,
            Color valueColor = null,
            float? value = null,
            float? headValue = null,
            float? tailValue = null,
            int? stepValue = null,
            float? rotationValue = null,
            float? strokeWidth = null
        ) {
            this.backgroundColor = backgroundColor;
            this.valueColor = valueColor;
            this.value = value;
            this.headValue = headValue;
            this.tailValue = tailValue;
            this.stepValue = stepValue;
            this.rotationValue = rotationValue;
            this.strokeWidth = strokeWidth;
            this.arcStart = value != null
                ? _startAngle
                : _startAngle + tailValue * 3 / 2 * Mathf.PI + rotationValue * Mathf.PI * 1.7f -
                  stepValue * 0.8f * Mathf.PI;
            this.arcSweep = value != null
                ? value.Value.clamp(0.0f, 1.0f) * _sweep
                : Mathf.Max(headValue * 3 / 2 * Mathf.PI - tailValue * 3 / 2 * Mathf.PI ?? 0.0f, _epsilon);
        }

        public readonly Color backgroundColor;
        public readonly Color valueColor;
        public readonly float? value;
        public readonly float? headValue;
        public readonly float? tailValue;
        public readonly int? stepValue;
        public readonly float? rotationValue;
        public readonly float? strokeWidth;
        public readonly float? arcStart;
        public readonly float? arcSweep;

        const float _twoPi = Mathf.PI * 2.0f;
        const float _epsilon = .001f;

        const float _sweep = _twoPi - _epsilon;
        const float _startAngle = -Mathf.PI / 2.0f;

        public override void paint(Canvas canvas, Size size) {
            Paint paint = new Paint();
            paint.color = this.valueColor;
            paint.strokeWidth = this.strokeWidth ?? 0.0f;
            paint.style = PaintingStyle.stroke;

            if (this.backgroundColor != null) {
                Paint backgroundPaint = new Paint() {
                    color = this.backgroundColor,
                    strokeWidth = this.strokeWidth ?? 0.0f,
                    style = PaintingStyle.stroke
                };
                canvas.drawArc(Offset.zero & size, 0, _sweep, false, backgroundPaint);
            }

            if (this.value == null)
            {
                paint.strokeCap = StrokeCap.square;
            }

            canvas.drawArc(Offset.zero & size, this.arcStart ?? 0.0f, this.arcSweep ?? 0.0f, false, paint);
        }

        public override bool shouldRepaint(CustomPainter oldPainter) {
            D.assert(oldPainter is _CircularProgressIndicatorPainter);
            _CircularProgressIndicatorPainter painter = oldPainter as _CircularProgressIndicatorPainter;
            return painter.backgroundColor != this.backgroundColor
                   || painter.valueColor != this.valueColor
                   || painter.value != this.value
                   || painter.headValue != this.headValue
                   || painter.tailValue != this.tailValue
                   || painter.stepValue != this.stepValue
                   || painter.rotationValue != this.rotationValue
                   || painter.strokeWidth != this.strokeWidth;
        }
    }

    public class CircularProgressIndicator : ProgressIndicator {
        public CircularProgressIndicator(
            Key key = null,
            float? value = null,
            Color backgroundColor = null,
            Animation<Color> valueColor = null,
            float strokeWidth = 4.0f
        ) : base(
            key: key,
            value: value,
            backgroundColor: backgroundColor,
            valueColor: valueColor
        ) {
            this.strokeWidth = strokeWidth;
        }

        public readonly float? strokeWidth;

        public override State createState() {
            return new _CircularProgressIndicatorState();
        }
    }


    class _CircularProgressIndicatorState : SingleTickerProviderStateMixin<CircularProgressIndicator> {
        protected AnimationController _controller;

        public _CircularProgressIndicatorState() {
        }

        public override void initState() {
            base.initState();
            this._controller = new AnimationController(
                duration: new TimeSpan(0, 0, 0, 5),
                vsync: this
            );
            if (this.widget.value == null) {
                this._controller.repeat();
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (this.widget.value == null && !this._controller.isAnimating) {
                this._controller.repeat();
            }
            else if (this.widget.value != null && this._controller.isAnimating) {
                this._controller.stop();
            }
        }

        public override void dispose() {
            this._controller.dispose();
            base.dispose();
        }

        Widget _buildIndicator(BuildContext context, float headValue, float tailValue, int stepValue,
            float rotationValue) {
            return new Container(
                constraints: new BoxConstraints(
                    minWidth: _ProgressIndicatorContants._kMinCircularProgressIndicatorSize,
                    minHeight: _ProgressIndicatorContants._kMinCircularProgressIndicatorSize
                ),
                child: new CustomPaint(
                    painter: new _CircularProgressIndicatorPainter(
                        backgroundColor: this.widget.backgroundColor,
                        valueColor: this.widget._getValueColor(context),
                        value: this.widget.value,
                        headValue: headValue,
                        tailValue: tailValue,
                        stepValue: stepValue,
                        rotationValue: rotationValue,
                        strokeWidth: this.widget.strokeWidth
                    )
                )
            );
        }

        protected Widget _buildAnimation() {
            return new AnimatedBuilder(
                animation: this._controller,
                builder: (BuildContext context, Widget child) => {
                    return this._buildIndicator(
                        context,
                        _ProgressIndicatorContants._kStrokeHeadTween.evaluate(this._controller),
                        _ProgressIndicatorContants._kStrokeTailTween.evaluate(this._controller),
                        _ProgressIndicatorContants._kStepTween.evaluate(this._controller),
                        _ProgressIndicatorContants._kRotationTween.evaluate(this._controller)
                    );
                }
            );
        }

        public override Widget build(BuildContext context) {
            if (this.widget.value != null) {
                return this._buildIndicator(context, 0.0f, 0.0f, 0, 0.0f);
            }

            return this._buildAnimation();
        }
    }

    class _RefreshProgressIndicatorPainter : _CircularProgressIndicatorPainter {
        public _RefreshProgressIndicatorPainter(
            Color valueColor = null,
            float? value = null,
            float? headValue = null,
            float? tailValue = null,
            int? stepValue = null,
            float? rotationValue = null,
            float? strokeWidth = null,
            float? arrowheadScale = null
        ) : base(
            valueColor: valueColor,
            value: value,
            headValue: headValue,
            tailValue: tailValue,
            stepValue: stepValue,
            rotationValue: rotationValue,
            strokeWidth: strokeWidth
        ) {
            this.arrowheadScale = arrowheadScale;
        }

        public readonly float? arrowheadScale;

        void paintArrowhead(Canvas canvas, Size size) {
            float arcEnd = this.arcStart + this.arcSweep ?? 0.0f;
            float ux = Mathf.Cos(arcEnd);
            float uy = Mathf.Sin(arcEnd);

            D.assert(size.width == size.height);
            float radius = size.width / 2.0f;
            float? arrowheadPointX = radius + ux * radius + -uy * this.strokeWidth * 2.0f * this.arrowheadScale;
            float? arrowheadPointY = radius + uy * radius + ux * this.strokeWidth * 2.0f * this.arrowheadScale;
            float? arrowheadRadius = this.strokeWidth * 1.5f * this.arrowheadScale;
            float? innerRadius = radius - arrowheadRadius;
            float? outerRadius = radius + arrowheadRadius;

            Path path = new Path();
            path.moveTo(radius + ux * innerRadius ?? 0.0f, radius + uy * innerRadius ?? 0.0f);
            path.lineTo(radius + ux * outerRadius ?? 0.0f, radius + uy * outerRadius ?? 0.0f);
            path.lineTo(arrowheadPointX ?? 0.0f, arrowheadPointY ?? 0.0f);
            path.close();
            Paint paint = new Paint();
            paint.color = this.valueColor;
            paint.strokeWidth = this.strokeWidth ?? 0.0f;
            paint.style = PaintingStyle.fill;
            canvas.drawPath(path, paint);
        }

        public override void paint(Canvas canvas, Size size) {
            base.paint(canvas, size);
            if (this.arrowheadScale > 0.0) {
                this.paintArrowhead(canvas, size);
            }
        }
    }

    public class RefreshProgressIndicator : CircularProgressIndicator {
        public RefreshProgressIndicator(
            Key key = null,
            float? value = null,
            Color backgroundColor = null,
            Animation<Color> valueColor = null,
            float strokeWidth = 2.0f
        ) : base(
            key: key,
            value: value,
            backgroundColor: backgroundColor,
            valueColor: valueColor,
            strokeWidth: strokeWidth
        ) {
        }

        public override State createState() {
            return new _RefreshProgressIndicatorState();
        }
    }

    class _RefreshProgressIndicatorState : _CircularProgressIndicatorState {
        const float _indicatorSize = 40.0f;

        public _RefreshProgressIndicatorState() {
        }

        public override Widget build(BuildContext context) {
            if (this.widget.value != null) {
                this._controller.setValue(this.widget.value / 10.0f ?? 0.0f);
            }
            else if (!this._controller.isAnimating) {
                this._controller.repeat();
            }

            return this._buildAnimation();
        }

        Widget _buildIndicator(BuildContext context, float headValue, float tailValue, int stepValue,
            float rotationValue) {
            float arrowheadScale =
                this.widget.value == null ? 0.0f : (this.widget.value * 2.0f).Value.clamp(0.0f, 1.0f);
            return new Container(
                width: _indicatorSize,
                height: _indicatorSize,
                margin: EdgeInsets.all(4.0f),
                child: new Material(
                    type: MaterialType.circle,
                    color: this.widget.backgroundColor ?? Theme.of(context).canvasColor,
                    elevation: 2.0f,
                    child: new Padding(
                        padding: EdgeInsets.all(12.0f),
                        child: new CustomPaint(
                            painter: new _RefreshProgressIndicatorPainter(
                                valueColor: this.widget._getValueColor(context),
                                value: null,
                                headValue: headValue,
                                tailValue: tailValue,
                                stepValue: stepValue,
                                rotationValue: rotationValue,
                                strokeWidth: this.widget.strokeWidth,
                                arrowheadScale: arrowheadScale
                            )
                        )
                    )
                )
            );
        }
    }
}