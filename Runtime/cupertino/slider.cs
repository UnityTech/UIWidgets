using System;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.cupertino {
    class SliderUtils {
        public const float _kPadding = 8.0f;
        public static readonly Color _kTrackColor = new Color(0xFFB5B5B5);
        public const float _kSliderHeight = 2.0f * (CupertinoThumbPainter.radius + _kPadding);
        public const float _kSliderWidth = 176.0f; // Matches Material Design slider.
        public static readonly TimeSpan _kDiscreteTransitionDuration = new TimeSpan(0, 0, 0, 0, 500);
        public const float _kAdjustmentUnit = 0.1f; // Matches iOS implementation of material slider.
    }

    public class CupertinoSlider : StatefulWidget {
        public CupertinoSlider(
            Key key = null,
            float? value = null,
            ValueChanged<float> onChanged = null,
            ValueChanged<float> onChangeStart = null,
            ValueChanged<float> onChangeEnd = null,
            float min = 0.0f,
            float max = 1.0f,
            int? divisions = null,
            Color activeColor = null
        ) : base(key: key) {
            D.assert(value != null);
            D.assert(onChanged != null);
            D.assert(value >= min && value <= max);
            D.assert(divisions == null || divisions > 0);
            this.value = value.Value;
            this.onChanged = onChanged;
            this.onChangeStart = onChangeStart;
            this.onChangeEnd = onChangeEnd;
            this.min = min;
            this.max = max;
            this.divisions = divisions;
            this.activeColor = activeColor;
        }

        public readonly float value;

        public readonly ValueChanged<float> onChanged;

        public readonly ValueChanged<float> onChangeStart;

        public readonly ValueChanged<float> onChangeEnd;

        public readonly float min;

        public readonly float max;

        public readonly int? divisions;

        public readonly Color activeColor;

        public override State createState() {
            return new _CupertinoSliderState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("value", this.value));
            properties.add(new FloatProperty("min", this.min));
            properties.add(new FloatProperty("max", this.max));
        }
    }

    class _CupertinoSliderState : TickerProviderStateMixin<CupertinoSlider> {
        void _handleChanged(float value) {
            D.assert(this.widget.onChanged != null);
            float lerpValue = MathUtils.lerpFloat(this.widget.min, this.widget.max, value);
            if (lerpValue != this.widget.value) {
                this.widget.onChanged(lerpValue);
            }
        }

        void _handleDragStart(float value) {
            D.assert(this.widget.onChangeStart != null);
            this.widget.onChangeStart(MathUtils.lerpFloat(this.widget.min, this.widget.max, value));
        }

        void _handleDragEnd(float value) {
            D.assert(this.widget.onChangeEnd != null);
            this.widget.onChangeEnd(MathUtils.lerpFloat(this.widget.min, this.widget.max, value));
        }

        public override Widget build(BuildContext context) {
            return new _CupertinoSliderRenderObjectWidget(
                value: (this.widget.value - this.widget.min) / (this.widget.max - this.widget.min),
                divisions: this.widget.divisions,
                activeColor: this.widget.activeColor ?? CupertinoTheme.of(context).primaryColor,
                onChanged: this.widget.onChanged != null ? (ValueChanged<float>) this._handleChanged : null,
                onChangeStart: this.widget.onChangeStart != null ? (ValueChanged<float>) this._handleDragStart : null,
                onChangeEnd: this.widget.onChangeEnd != null ? (ValueChanged<float>) this._handleDragEnd : null,
                vsync: this
            );
        }
    }

    class _CupertinoSliderRenderObjectWidget : LeafRenderObjectWidget {
        public _CupertinoSliderRenderObjectWidget(
            Key key = null,
            float? value = null,
            int? divisions = null,
            Color activeColor = null,
            ValueChanged<float> onChanged = null,
            ValueChanged<float> onChangeStart = null,
            ValueChanged<float> onChangeEnd = null,
            TickerProvider vsync = null
        ) : base(key: key) {
            this.value = value;
            this.divisions = divisions;
            this.activeColor = activeColor;
            this.onChanged = onChanged;
            this.onChangeStart = onChangeStart;
            this.onChangeEnd = onChangeEnd;
            this.vsync = vsync;
        }

        public readonly float? value;
        public readonly int? divisions;
        public readonly Color activeColor;
        public readonly ValueChanged<float> onChanged;
        public readonly ValueChanged<float> onChangeStart;
        public readonly ValueChanged<float> onChangeEnd;
        public readonly TickerProvider vsync;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderCupertinoSlider(
                value: this.value ?? 0.0f,
                divisions: this.divisions,
                activeColor: this.activeColor,
                onChanged: this.onChanged,
                onChangeStart: this.onChangeStart,
                onChangeEnd: this.onChangeEnd,
                vsync: this.vsync
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject _renderObject) {
            _RenderCupertinoSlider renderObject = _renderObject as _RenderCupertinoSlider;
            renderObject.value = this.value ?? 0.0f;
            renderObject.divisions = this.divisions;
            renderObject.activeColor = this.activeColor;
            renderObject.onChanged = this.onChanged;
            renderObject.onChangeStart = this.onChangeStart;
            renderObject.onChangeEnd = this.onChangeEnd;
        }
    }

    class _RenderCupertinoSlider : RenderConstrainedBox {
        public _RenderCupertinoSlider(
            float value,
            int? divisions = null,
            Color activeColor = null,
            ValueChanged<float> onChanged = null,
            ValueChanged<float> onChangeStart = null,
            ValueChanged<float> onChangeEnd = null,
            TickerProvider vsync = null
        ) : base(additionalConstraints: BoxConstraints.tightFor(width: SliderUtils._kSliderWidth,
            height: SliderUtils._kSliderHeight)) {
            D.assert(value >= 0.0f && value <= 1.0f);
            this._value = value;
            this._divisions = divisions;
            this._activeColor = activeColor;
            this._onChanged = onChanged;
            this.onChangeStart = onChangeStart;
            this.onChangeEnd = onChangeEnd;
            this._drag = new HorizontalDragGestureRecognizer();
            this._drag.onStart = this._handleDragStart;
            this._drag.onUpdate = this._handleDragUpdate;
            this._drag.onEnd = this._handleDragEnd;
            this._position = new AnimationController(
                value: value,
                duration: SliderUtils._kDiscreteTransitionDuration,
                vsync: vsync
            );
            this._position.addListener(this.markNeedsPaint);
        }

        public float value {
            get { return this._value; }
            set {
                D.assert(value >= 0.0f && value <= 1.0f);
                if (value == this._value) {
                    return;
                }

                this._value = value;
                if (this.divisions != null) {
                    this._position.animateTo(value, curve: Curves.fastOutSlowIn);
                }
                else {
                    this._position.setValue(value);
                }
            }
        }

        float _value;

        public int? divisions {
            get { return this._divisions; }
            set {
                if (value == this._divisions) {
                    return;
                }

                this._divisions = value;
                this.markNeedsPaint();
            }
        }

        int? _divisions;

        public Color activeColor {
            get { return this._activeColor; }
            set {
                if (value == this._activeColor) {
                    return;
                }

                this._activeColor = value;
                this.markNeedsPaint();
            }
        }

        Color _activeColor;

        public ValueChanged<float> onChanged {
            get { return this._onChanged; }
            set {
                if (value == this._onChanged) {
                    return;
                }

                this._onChanged = value;
            }
        }

        ValueChanged<float> _onChanged;

        public ValueChanged<float> onChangeStart;
        public ValueChanged<float> onChangeEnd;


        AnimationController _position;

        HorizontalDragGestureRecognizer _drag;
        float _currentDragValue = 0.0f;

        float _discretizedCurrentDragValue {
            get {
                float dragValue = this._currentDragValue.clamp(0.0f, 1.0f);
                if (this.divisions != null) {
                    dragValue = Mathf.Round(dragValue * this.divisions.Value) / this.divisions.Value;
                }

                return dragValue;
            }
        }

        public float _trackLeft {
            get { return SliderUtils._kPadding; }
        }

        public float _trackRight {
            get { return this.size.width - SliderUtils._kPadding; }
        }

        float _thumbCenter {
            get {
                float visualPosition = this._value;

                return MathUtils.lerpFloat(this._trackLeft + CupertinoThumbPainter.radius,
                    this._trackRight - CupertinoThumbPainter.radius,
                    visualPosition);
            }
        }

        public bool isInteractive {
            get { return this.onChanged != null; }
        }

        void _handleDragStart(DragStartDetails details) {
            this._startInteraction(details.globalPosition);
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            if (this.isInteractive) {
                float extent = Mathf.Max(SliderUtils._kPadding,
                    this.size.width - 2.0f * (SliderUtils._kPadding + CupertinoThumbPainter.radius));
                float? valueDelta = details.primaryDelta / extent;
                this._currentDragValue += valueDelta ?? 0.0f;

                this.onChanged(this._discretizedCurrentDragValue);
            }
        }

        void _handleDragEnd(DragEndDetails details) {
            this._endInteraction();
        }

        void _startInteraction(Offset globalPosition) {
            if (this.isInteractive) {
                if (this.onChangeStart != null) {
                    this.onChangeStart(this._discretizedCurrentDragValue);
                }

                this._currentDragValue = this._value;
                this.onChanged(this._discretizedCurrentDragValue);
            }
        }

        void _endInteraction() {
            if (this.onChangeEnd != null) {
                this.onChangeEnd(this._discretizedCurrentDragValue);
            }

            this._currentDragValue = 0.0f;
        }

        protected override bool hitTestSelf(Offset position) {
            return (position.dx - this._thumbCenter).abs() < CupertinoThumbPainter.radius + SliderUtils._kPadding;
        }

        public override void handleEvent(PointerEvent e, HitTestEntry entry) {
            D.assert(this.debugHandleEvent(e, entry));
            if (e is PointerDownEvent pointerDownEvent && this.isInteractive) {
                this._drag.addPointer(pointerDownEvent);
            }
        }

        CupertinoThumbPainter _thumbPainter = new CupertinoThumbPainter();

        public override
            void paint(PaintingContext context, Offset offset) {
            float visualPosition;
            Color leftColor;
            Color rightColor;
            visualPosition = this._position.value;
            leftColor = SliderUtils._kTrackColor;
            rightColor = this._activeColor;

            float trackCenter = offset.dy + this.size.height / 2.0f;
            float trackLeft = offset.dx + this._trackLeft;
            float trackTop = trackCenter - 1.0f;
            float trackBottom = trackCenter + 1.0f;
            float trackRight = offset.dx + this._trackRight;
            float trackActive = offset.dx + this._thumbCenter;

            Canvas canvas = context.canvas;

            if (visualPosition > 0.0f) {
                Paint paint = new Paint();
                paint.color = rightColor;
                canvas.drawRRect(RRect.fromLTRBXY(trackLeft, trackTop, trackActive, trackBottom, 1.0f, 1.0f), paint);
            }

            if (visualPosition < 1.0f) {
                Paint paint = new Paint();
                paint.color = leftColor;
                canvas.drawRRect(RRect.fromLTRBXY(trackActive, trackTop, trackRight, trackBottom, 1.0f, 1.0f), paint);
            }

            Offset thumbCenter = new Offset(trackActive, trackCenter);
            this._thumbPainter.paint(canvas,
                Rect.fromCircle(center: thumbCenter, radius: CupertinoThumbPainter.radius));
        }
    }
}