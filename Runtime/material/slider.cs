using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.material {
    public class Slider : StatefulWidget {
        public Slider(
            Key key = null,
            float? value = null,
            ValueChanged<float> onChanged = null,
            ValueChanged<float> onChangeStart = null,
            ValueChanged<float> onChangeEnd = null,
            float min = 0.0f,
            float max = 1.0f,
            int? divisions = null,
            string label = null,
            Color activeColor = null,
            Color inactiveColor = null
        ) : base(key: key) {
            D.assert(value != null);
            D.assert(min <= max);
            D.assert(value >= min && value <= max);
            D.assert(divisions == null || divisions > 0);
            this.value = value.Value;
            this.onChanged = onChanged;
            this.onChangeStart = onChangeStart;
            this.onChangeEnd = onChangeEnd;
            this.min = min;
            this.max = max;
            this.divisions = divisions;
            this.label = label;
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor;
        }

        public readonly float value;

        public readonly ValueChanged<float> onChanged;

        public readonly ValueChanged<float> onChangeStart;

        public readonly ValueChanged<float> onChangeEnd;

        public readonly float min;

        public readonly float max;

        public readonly int? divisions;

        public readonly string label;

        public readonly Color activeColor;

        public readonly Color inactiveColor;

        public override State createState() {
            return new _SliderState();
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("value", this.value));
            properties.add(new FloatProperty("min", this.min));
            properties.add(new FloatProperty("max", this.max));
        }
    }


    class _SliderState : TickerProviderStateMixin<Slider> {
        static TimeSpan enableAnimationDuration = new TimeSpan(0, 0, 0, 0, 75);
        static TimeSpan valueIndicatorAnimationDuration = new TimeSpan(0, 0, 0, 0, 100);

        public AnimationController overlayController;
        public AnimationController valueIndicatorController;
        public AnimationController enableController;
        public AnimationController positionController;
        public Timer interactionTimer;

        public override void initState() {
            base.initState();
            this.overlayController = new AnimationController(
                duration: Constants.kRadialReactionDuration,
                vsync: this
            );
            this.valueIndicatorController = new AnimationController(
                duration: valueIndicatorAnimationDuration,
                vsync: this
            );
            this.enableController = new AnimationController(
                duration: enableAnimationDuration,
                vsync: this
            );
            this.positionController = new AnimationController(
                duration: TimeSpan.Zero,
                vsync: this
            );
            this.enableController.setValue(this.widget.onChanged != null ? 1.0f : 0.0f);
            this.positionController.setValue(this._unlerp(this.widget.value));
        }

        public override void dispose() {
            this.interactionTimer?.cancel();
            this.overlayController.dispose();
            this.valueIndicatorController.dispose();
            this.enableController.dispose();
            this.positionController.dispose();
            base.dispose();
        }

        void _handleChanged(float value) {
            D.assert(this.widget.onChanged != null);
            float lerpValue = this._lerp(value);
            if (lerpValue != this.widget.value) {
                this.widget.onChanged(lerpValue);
            }
        }

        void _handleDragStart(float value) {
            D.assert(this.widget.onChangeStart != null);
            this.widget.onChangeStart(this._lerp(value));
        }

        void _handleDragEnd(float value) {
            D.assert(this.widget.onChangeEnd != null);
            this.widget.onChangeEnd(this._lerp(value));
        }

        float _lerp(float value) {
            D.assert(value >= 0.0f);
            D.assert(value <= 1.0f);
            return value * (this.widget.max - this.widget.min) + this.widget.min;
        }

        float _unlerp(float value) {
            D.assert(value <= this.widget.max);
            D.assert(value >= this.widget.min);
            return this.widget.max > this.widget.min
                ? (value - this.widget.min) / (this.widget.max - this.widget.min)
                : 0.0f;
        }

        public override Widget build(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));

            SliderThemeData sliderTheme = SliderTheme.of(context);

            if (this.widget.activeColor != null || this.widget.inactiveColor != null) {
                sliderTheme = sliderTheme.copyWith(
                    activeTrackColor: this.widget.activeColor,
                    inactiveTrackColor: this.widget.inactiveColor,
                    activeTickMarkColor: this.widget.inactiveColor,
                    inactiveTickMarkColor: this.widget.activeColor,
                    thumbColor: this.widget.activeColor,
                    valueIndicatorColor: this.widget.activeColor,
                    overlayColor: this.widget.activeColor?.withAlpha(0x29)
                );
            }

            return new _SliderRenderObjectWidget(
                value: this._unlerp(this.widget.value),
                divisions: this.widget.divisions,
                label: this.widget.label,
                sliderTheme: sliderTheme,
                mediaQueryData: MediaQuery.of(context),
                onChanged: (this.widget.onChanged != null) && (this.widget.max > this.widget.min)
                    ? this._handleChanged
                    : (ValueChanged<float>) null,
                onChangeStart: this.widget.onChangeStart != null ? this._handleDragStart : (ValueChanged<float>) null,
                onChangeEnd: this.widget.onChangeEnd != null ? this._handleDragEnd : (ValueChanged<float>) null,
                state: this
            );
        }
    }

    class _SliderRenderObjectWidget : LeafRenderObjectWidget {
        public _SliderRenderObjectWidget(
            Key key = null,
            float? value = null,
            int? divisions = null,
            string label = null,
            SliderThemeData sliderTheme = null,
            MediaQueryData mediaQueryData = null,
            ValueChanged<float> onChanged = null,
            ValueChanged<float> onChangeStart = null,
            ValueChanged<float> onChangeEnd = null,
            _SliderState state = null
        ) : base(key: key) {
            this.value = value.Value;
            this.divisions = divisions;
            this.label = label;
            this.sliderTheme = sliderTheme;
            this.mediaQueryData = mediaQueryData;
            this.onChanged = onChanged;
            this.onChangeStart = onChangeStart;
            this.onChangeEnd = onChangeEnd;
            this.state = state;
        }


        public readonly float value;
        public readonly int? divisions;
        public readonly string label;
        public readonly SliderThemeData sliderTheme;
        public readonly MediaQueryData mediaQueryData;
        public readonly ValueChanged<float> onChanged;
        public readonly ValueChanged<float> onChangeStart;
        public readonly ValueChanged<float> onChangeEnd;
        public readonly _SliderState state;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderSlider(
                value: this.value,
                divisions: this.divisions,
                label: this.label,
                sliderTheme: this.sliderTheme,
                theme: Theme.of(context),
                mediaQueryData: this.mediaQueryData,
                onChanged: this.onChanged,
                onChangeStart: this.onChangeStart,
                onChangeEnd: this.onChangeEnd,
                state: this.state,
                platform: Theme.of(context).platform
            );
        }


        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            _RenderSlider _renderObject = (_RenderSlider) renderObject;
            _renderObject.value = this.value;
            _renderObject.divisions = this.divisions;
            _renderObject.label = this.label;
            _renderObject.sliderTheme = this.sliderTheme;
            _renderObject.theme = Theme.of(context);
            _renderObject.mediaQueryData = this.mediaQueryData;
            _renderObject.onChanged = this.onChanged;
            _renderObject.onChangeStart = this.onChangeStart;
            _renderObject.onChangeEnd = this.onChangeEnd;
            _renderObject.platform = Theme.of(context).platform;
        }
    }

    class _RenderSlider : RenderBox {
        static float _positionAnimationDurationMilliSeconds = 75;
        static float _minimumInteractionTimeMilliSeconds = 500;

        const float _minPreferredTrackWidth = 144.0f;

        public _RenderSlider(
            float? value = null,
            int? divisions = null,
            string label = null,
            SliderThemeData sliderTheme = null,
            ThemeData theme = null,
            MediaQueryData mediaQueryData = null,
            RuntimePlatform? platform = null,
            ValueChanged<float> onChanged = null,
            ValueChanged<float> onChangeStart = null,
            ValueChanged<float> onChangeEnd = null,
            _SliderState state = null
        ) {
            D.assert(value != null && value >= 0.0 && value <= 1.0);
            D.assert(state != null);
            this.onChangeStart = onChangeStart;
            this.onChangeEnd = onChangeEnd;
            this._platform = platform;
            this._label = label;
            this._value = value.Value;
            this._divisions = divisions;
            this._sliderTheme = sliderTheme;
            this._theme = theme;
            this._mediaQueryData = mediaQueryData;
            this._onChanged = onChanged;
            this._state = state;

            this._updateLabelPainter();
            GestureArenaTeam team = new GestureArenaTeam();
            this._drag = new HorizontalDragGestureRecognizer {
                team = team,
                onStart = this._handleDragStart,
                onUpdate = this._handleDragUpdate,
                onEnd = this._handleDragEnd,
                onCancel = this._endInteraction
            };

            this._tap = new TapGestureRecognizer {
                team = team,
                onTapDown = this._handleTapDown,
                onTapUp = this._handleTapUp,
                onTapCancel = this._endInteraction
            };

            this._overlayAnimation = new CurvedAnimation(
                parent: this._state.overlayController,
                curve: Curves.fastOutSlowIn);

            this._valueIndicatorAnimation = new CurvedAnimation(
                parent: this._state.valueIndicatorController,
                curve: Curves.fastOutSlowIn);

            this._enableAnimation = new CurvedAnimation(
                parent: this._state.enableController,
                curve: Curves.easeInOut);
        }

        float _maxSliderPartWidth {
            get {
                float maxValue = 0;
                foreach (Size size in this._sliderPartSizes) {
                    if (size.width > maxValue) {
                        maxValue = size.width;
                    }
                }

                return maxValue;
            }
        }

        float _maxSliderPartHeight {
            get {
                float maxValue = 0;
                foreach (Size size in this._sliderPartSizes) {
                    if (size.width > maxValue) {
                        maxValue = size.width;
                    }
                }

                return maxValue;
            }
        }

        List<Size> _sliderPartSizes {
            get {
                return new List<Size> {
                    this._sliderTheme.overlayShape.getPreferredSize(this.isInteractive, this.isDiscrete),
                    this._sliderTheme.thumbShape.getPreferredSize(this.isInteractive, this.isDiscrete),
                    this._sliderTheme.tickMarkShape.getPreferredSize(isEnabled: this.isInteractive,
                        sliderTheme: this.sliderTheme)
                };
            }
        }

        float _minPreferredTrackHeight {
            get { return this._sliderTheme.trackHeight; }
        }

        _SliderState _state;
        Animation<float> _overlayAnimation;
        Animation<float> _valueIndicatorAnimation;
        Animation<float> _enableAnimation;
        TextPainter _labelPainter = new TextPainter();
        HorizontalDragGestureRecognizer _drag;
        TapGestureRecognizer _tap;
        bool _active = false;
        float _currentDragValue = 0.0f;

        Rect _trackRect {
            get {
                return this._sliderTheme.trackShape.getPreferredRect(
                    parentBox: this,
                    offset: Offset.zero,
                    sliderTheme: this._sliderTheme,
                    isDiscrete: false
                );
            }
        }

        bool isInteractive {
            get { return this.onChanged != null; }
        }

        bool isDiscrete {
            get { return this.divisions != null && this.divisions.Value > 0; }
        }

        public float value {
            get { return this._value; }
            set {
                D.assert(value >= 0.0f && value <= 1.0f);
                float convertedValue = this.isDiscrete ? this._discretize(value) : value;
                if (convertedValue == this._value) {
                    return;
                }

                this._value = convertedValue;
                if (this.isDiscrete) {
                    float distance = (this._value - this._state.positionController.value).abs();
                    this._state.positionController.duration = distance != 0.0f
                        ? new TimeSpan(0, 0, 0, 0, (int) (_positionAnimationDurationMilliSeconds * (1.0f / distance)))
                        : TimeSpan.Zero;
                    this._state.positionController.animateTo(convertedValue, curve: Curves.easeInOut);
                }
                else {
                    this._state.positionController.setValue(convertedValue);
                }
            }
        }

        float _value;

        public RuntimePlatform? platform {
            get { return this._platform; }
            set {
                if (this._platform == value) {
                    return;
                }

                this._platform = value;
            }
        }

        RuntimePlatform? _platform;

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

        public string label {
            get { return this._label; }
            set {
                if (value == this._label) {
                    return;
                }

                this._label = value;
                this._updateLabelPainter();
            }
        }

        string _label;

        public SliderThemeData sliderTheme {
            get { return this._sliderTheme; }
            set {
                if (value == this._sliderTheme) {
                    return;
                }

                this._sliderTheme = value;
                this.markNeedsPaint();
            }
        }

        SliderThemeData _sliderTheme;

        public ThemeData theme {
            get { return this._theme; }
            set {
                if (value == this._theme) {
                    return;
                }

                this._theme = value;
                this.markNeedsPaint();
            }
        }

        ThemeData _theme;

        public MediaQueryData mediaQueryData {
            get { return this._mediaQueryData; }
            set {
                if (value == this._mediaQueryData) {
                    return;
                }

                this._mediaQueryData = value;
                this._updateLabelPainter();
            }
        }

        MediaQueryData _mediaQueryData;

        public ValueChanged<float> onChanged {
            get { return this._onChanged; }
            set {
                if (value == this._onChanged) {
                    return;
                }

                bool wasInteractive = this.isInteractive;
                this._onChanged = value;
                if (wasInteractive != this.isInteractive) {
                    if (this.isInteractive) {
                        this._state.enableController.forward();
                    }
                    else {
                        this._state.enableController.reverse();
                    }

                    this.markNeedsPaint();
                }
            }
        }

        ValueChanged<float> _onChanged;

        public ValueChanged<float> onChangeStart;
        public ValueChanged<float> onChangeEnd;

        public bool showValueIndicator {
            get {
                bool showValueIndicator = false;
                switch (this._sliderTheme.showValueIndicator) {
                    case ShowValueIndicator.onlyForDiscrete:
                        showValueIndicator = this.isDiscrete;
                        break;
                    case ShowValueIndicator.onlyForContinuous:
                        showValueIndicator = !this.isDiscrete;
                        break;
                    case ShowValueIndicator.always:
                        showValueIndicator = true;
                        break;
                    case ShowValueIndicator.never:
                        showValueIndicator = false;
                        break;
                }

                return showValueIndicator;
            }
        }

        float _adjustmentUnit {
            get {
                switch (this._platform) {
                    case RuntimePlatform.IPhonePlayer:
                        return 0.1f;
                    default:
                        return 0.05f;
                }
            }
        }


        void _updateLabelPainter() {
            if (this.label != null) {
                this._labelPainter.text = new TextSpan(
                    style: this._sliderTheme.valueIndicatorTextStyle,
                    text: this.label
                );
                this._labelPainter.textScaleFactor = this._mediaQueryData.textScaleFactor;
                this._labelPainter.layout();
            }
            else {
                this._labelPainter.text = null;
            }

            this.markNeedsLayout();
        }

        public override void attach(object owner) {
            base.attach(owner);
            this._overlayAnimation.addListener(this.markNeedsPaint);
            this._valueIndicatorAnimation.addListener(this.markNeedsPaint);
            this._enableAnimation.addListener(this.markNeedsPaint);
            this._state.positionController.addListener(this.markNeedsPaint);
        }

        public override void detach() {
            this._overlayAnimation.removeListener(this.markNeedsPaint);
            this._valueIndicatorAnimation.removeListener(this.markNeedsPaint);
            this._enableAnimation.removeListener(this.markNeedsPaint);
            this._state.positionController.removeListener(this.markNeedsPaint);
            base.detach();
        }

        float _getValueFromVisualPosition(float visualPosition) {
            return visualPosition;
        }

        float _getValueFromGlobalPosition(Offset globalPosition) {
            float visualPosition =
                (this.globalToLocal(globalPosition).dx - this._trackRect.left) / this._trackRect.width;
            return this._getValueFromVisualPosition(visualPosition);
        }

        float _discretize(float value) {
            float result = value.clamp(0.0f, 1.0f);
            if (this.isDiscrete) {
                result = (result * this.divisions.Value).round() * 1.0f / this.divisions.Value;
            }

            return result;
        }

        void _startInteraction(Offset globalPosition) {
            if (this.isInteractive) {
                this._active = true;

                if (this.onChangeStart != null) {
                    this.onChangeStart(this._discretize(this.value));
                }

                this._currentDragValue = this._getValueFromGlobalPosition(globalPosition);
                this.onChanged(this._discretize(this._currentDragValue));
                this._state.overlayController.forward();
                if (this.showValueIndicator) {
                    this._state.valueIndicatorController.forward();
                    this._state.interactionTimer?.cancel();
                    this._state.interactionTimer = Window.instance.run(
                        new TimeSpan(0, 0, 0, 0,
                            (int) (_minimumInteractionTimeMilliSeconds * SchedulerBinding.instance.timeDilation)),
                        () => {
                            this._state.interactionTimer = null;
                            if (!this._active &&
                                this._state.valueIndicatorController.status == AnimationStatus.completed) {
                                this._state.valueIndicatorController.reverse();
                            }
                        }
                    );
                }
            }
        }

        void _endInteraction() {
            if (this._active && this._state.mounted) {
                if (this.onChangeEnd != null) {
                    this.onChangeEnd(this._discretize(this._currentDragValue));
                }

                this._active = false;
                this._currentDragValue = 0.0f;
                this._state.overlayController.reverse();
                if (this.showValueIndicator && this._state.interactionTimer == null) {
                    this._state.valueIndicatorController.reverse();
                }
            }
        }

        void _handleDragStart(DragStartDetails details) {
            this._startInteraction(details.globalPosition);
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            if (this.isInteractive) {
                float valueDelta = details.primaryDelta.Value / this._trackRect.width;
                this._currentDragValue += valueDelta;
                this.onChanged(this._discretize(this._currentDragValue));
            }
        }

        void _handleDragEnd(DragEndDetails details) {
            this._endInteraction();
        }

        void _handleTapDown(TapDownDetails details) {
            this._startInteraction(details.globalPosition);
        }

        void _handleTapUp(TapUpDetails details) {
            this._endInteraction();
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        public override void handleEvent(PointerEvent evt, HitTestEntry entry) {
            D.assert(this.debugHandleEvent(evt, entry));
            if (evt is PointerDownEvent && this.isInteractive) {
                this._drag.addPointer((PointerDownEvent) evt);
                this._tap.addPointer((PointerDownEvent) evt);
            }
        }


        protected override float computeMinIntrinsicWidth(float height) {
            return _minPreferredTrackWidth + this._maxSliderPartWidth;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            return _minPreferredTrackWidth + this._maxSliderPartWidth;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            return Mathf.Max(this._minPreferredTrackHeight, this._maxSliderPartHeight);
        }

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return Mathf.Max(this._minPreferredTrackHeight, this._maxSliderPartHeight);
        }

        protected override bool sizedByParent {
            get { return true; }
        }

        protected override void performResize() {
            this.size = new Size(
                this.constraints.hasBoundedWidth
                    ? this.constraints.maxWidth
                    : _minPreferredTrackWidth + this._maxSliderPartWidth,
                this.constraints.hasBoundedHeight
                    ? this.constraints.maxHeight
                    : Mathf.Max(this._minPreferredTrackHeight, this._maxSliderPartHeight)
            );
        }

        public override void paint(PaintingContext context, Offset offset) {
            float value = this._state.positionController.value;
            float visualPosition = value;

            Rect trackRect = this._sliderTheme.trackShape.getPreferredRect(
                parentBox: this,
                offset: offset,
                sliderTheme: this._sliderTheme,
                isDiscrete: this.isDiscrete
            );

            Offset thumbCenter = new Offset(trackRect.left + visualPosition * trackRect.width, trackRect.center.dy);

            this._sliderTheme.trackShape.paint(
                context,
                offset,
                parentBox: this,
                sliderTheme: this._sliderTheme,
                enableAnimation: this._enableAnimation,
                thumbCenter: thumbCenter,
                isDiscrete: this.isDiscrete,
                isEnabled: this.isInteractive
            );

            if (!this._overlayAnimation.isDismissed) {
                this._sliderTheme.overlayShape.paint(
                    context,
                    thumbCenter,
                    activationAnimation: this._overlayAnimation,
                    enableAnimation: this._enableAnimation,
                    isDiscrete: this.isDiscrete,
                    labelPainter: this._labelPainter,
                    parentBox: this,
                    sliderTheme: this._sliderTheme,
                    value: this._value
                );
            }

            if (this.isDiscrete) {
                float tickMarkWidth = this._sliderTheme.tickMarkShape.getPreferredSize(
                    isEnabled: this.isInteractive,
                    sliderTheme: this._sliderTheme
                ).width;

                float adjustedTrackWidth = trackRect.width - tickMarkWidth;
                if (adjustedTrackWidth / this.divisions.Value >= 3.0f * tickMarkWidth) {
                    float dy = trackRect.center.dy;
                    for (int i = 0; i <= this.divisions; i++) {
                        float tickValue = i / this.divisions.Value;
                        float dx = trackRect.left + tickValue * adjustedTrackWidth + tickMarkWidth / 2;
                        Offset tickMarkOffset = new Offset(dx, dy);
                        this._sliderTheme.tickMarkShape.paint(
                            context,
                            tickMarkOffset,
                            parentBox: this,
                            sliderTheme: this._sliderTheme,
                            enableAnimation: this._enableAnimation,
                            thumbCenter: thumbCenter,
                            isEnabled: this.isInteractive
                        );
                    }
                }
            }

            if (this.isInteractive && this.label != null && !this._valueIndicatorAnimation.isDismissed) {
                if (this.showValueIndicator) {
                    this._sliderTheme.valueIndicatorShape.paint(
                        context,
                        thumbCenter,
                        activationAnimation: this._valueIndicatorAnimation,
                        enableAnimation: this._enableAnimation,
                        isDiscrete: this.isDiscrete,
                        labelPainter: this._labelPainter,
                        parentBox: this,
                        sliderTheme: this._sliderTheme,
                        value: this._value
                    );
                }
            }

            this._sliderTheme.thumbShape.paint(
                context,
                thumbCenter,
                activationAnimation: this._valueIndicatorAnimation,
                enableAnimation: this._enableAnimation,
                isDiscrete: this.isDiscrete,
                labelPainter: this._labelPainter,
                parentBox: this,
                sliderTheme: this._sliderTheme,
                value: this._value
            );
        }
    }
}