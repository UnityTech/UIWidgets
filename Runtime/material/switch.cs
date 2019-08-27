using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using ImageUtils = Unity.UIWidgets.widgets.ImageUtils;

namespace Unity.UIWidgets.material {
    enum _SwitchType {
        material,
        adaptive
    }

    public class Switch : StatefulWidget {
        internal const float _kTrackHeight = 14.0f;
        internal const float _kTrackWidth = 33.0f;
        internal const float _kTrackRadius = _kTrackHeight / 2.0f;
        internal const float _kThumbRadius = 10.0f;
        internal const float _kSwitchWidth = _kTrackWidth - 2 * _kTrackRadius + 2 * Constants.kRadialReactionRadius;
        internal const float _kSwitchHeight = 2 * Constants.kRadialReactionRadius + 8.0f;
        internal const float _kSwitchHeightCollapsed = 2 * Constants.kRadialReactionRadius;

        public Switch(
            Key key = null,
            bool? value = null,
            ValueChanged<bool?> onChanged = null,
            Color activeColor = null,
            Color activeTrackColor = null,
            Color inactiveThumbColor = null,
            Color inactiveTrackColor = null,
            ImageProvider activeThumbImage = null,
            ImageProvider inactiveThumbImage = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : this(
            key: key,
            value: value,
            onChanged: onChanged,
            activeColor: activeColor,
            activeTrackColor: activeTrackColor,
            inactiveThumbColor: inactiveThumbColor,
            inactiveTrackColor: inactiveTrackColor,
            activeThumbImage: activeThumbImage,
            inactiveThumbImage: inactiveThumbImage,
            materialTapTargetSize: materialTapTargetSize,
            switchType: _SwitchType.material,
            dragStartBehavior: dragStartBehavior
        ) {
        }

        Switch(
            Key key = null,
            bool? value = null,
            ValueChanged<bool?> onChanged = null,
            Color activeColor = null,
            Color activeTrackColor = null,
            Color inactiveThumbColor = null,
            Color inactiveTrackColor = null,
            ImageProvider activeThumbImage = null,
            ImageProvider inactiveThumbImage = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            _SwitchType switchType = _SwitchType.material,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start
        ) : base(key: key) {
            D.assert(value != null);
            this.value = value.Value;
            D.assert(onChanged != null);
            this.onChanged = onChanged;
            this.activeColor = activeColor;
            this.activeTrackColor = activeTrackColor;
            this.inactiveThumbColor = inactiveThumbColor;
            this.inactiveTrackColor = inactiveTrackColor;
            this.activeThumbImage = activeThumbImage;
            this.inactiveThumbImage = inactiveThumbImage;
            this.materialTapTargetSize = materialTapTargetSize;
            this._switchType = switchType;
            this.dragStartBehavior = dragStartBehavior;
        }

        public static Switch adaptive(
            Key key = null,
            bool? value = null,
            ValueChanged<bool?> onChanged = null,
            Color activeColor = null,
            Color activeTrackColor = null,
            Color inactiveThumbColor = null,
            Color inactiveTrackColor = null,
            ImageProvider activeThumbImage = null,
            ImageProvider inactiveThumbImage = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.down
        ) {
            return new Switch(key: key,
                value: value,
                onChanged: onChanged,
                activeColor: activeColor,
                activeTrackColor: activeTrackColor,
                inactiveThumbColor: inactiveThumbColor,
                inactiveTrackColor: inactiveTrackColor,
                activeThumbImage: activeThumbImage,
                inactiveThumbImage: inactiveThumbImage,
                materialTapTargetSize: materialTapTargetSize,
                switchType: _SwitchType.adaptive
            );
        }

        public readonly bool value;

        public readonly ValueChanged<bool?> onChanged;

        public readonly Color activeColor;

        public readonly Color activeTrackColor;

        public readonly Color inactiveThumbColor;

        public readonly Color inactiveTrackColor;

        public readonly ImageProvider activeThumbImage;

        public readonly ImageProvider inactiveThumbImage;

        public readonly MaterialTapTargetSize? materialTapTargetSize;

        internal readonly _SwitchType _switchType;

        public readonly DragStartBehavior dragStartBehavior;

        public override State createState() {
            return new _SwitchState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FlagProperty("value", value: this.value, ifTrue: "on", ifFalse: "off", showName: true));
            properties.add(
                new ObjectFlagProperty<ValueChanged<bool?>>("onChanged", this.onChanged, ifNull: "disabled"));
        }
    }

    class _SwitchState : TickerProviderStateMixin<Switch> {
        Size getSwitchSize(ThemeData theme) {
            switch (this.widget.materialTapTargetSize ?? theme.materialTapTargetSize) {
                case MaterialTapTargetSize.padded:
                    return new Size(Switch._kSwitchWidth, Switch._kSwitchHeight);
                case MaterialTapTargetSize.shrinkWrap:
                    return new Size(Switch._kSwitchWidth, Switch._kSwitchHeightCollapsed);
            }

            D.assert(false);
            return null;
        }

        Widget buildMaterialSwitch(BuildContext context) {
            D.assert(MaterialD.debugCheckHasMaterial(context));
            ThemeData theme = Theme.of(context);
            bool isDark = theme.brightness == Brightness.dark;

            Color activeThumbColor = this.widget.activeColor ?? theme.toggleableActiveColor;
            Color activeTrackColor = this.widget.activeTrackColor ?? activeThumbColor.withAlpha(0x80);

            Color inactiveThumbColor;
            Color inactiveTrackColor;
            if (this.widget.onChanged != null) {
                Color black32 = new Color(0x52000000); // Black with 32% opacity
                inactiveThumbColor = this.widget.inactiveThumbColor ??
                                     (isDark ? Colors.grey.shade400 : Colors.grey.shade50);
                inactiveTrackColor = this.widget.inactiveTrackColor ?? (isDark ? Colors.white30 : black32);
            }
            else {
                inactiveThumbColor = this.widget.inactiveThumbColor ??
                                     (isDark ? Colors.grey.shade800 : Colors.grey.shade400);
                inactiveTrackColor = this.widget.inactiveTrackColor ?? (isDark ? Colors.white10 : Colors.black12);
            }

            return new _SwitchRenderObjectWidget(
                dragStartBehavior: this.widget.dragStartBehavior,
                value: this.widget.value,
                activeColor: activeThumbColor,
                inactiveColor: inactiveThumbColor,
                activeThumbImage: this.widget.activeThumbImage,
                inactiveThumbImage: this.widget.inactiveThumbImage,
                activeTrackColor: activeTrackColor,
                inactiveTrackColor: inactiveTrackColor,
                configuration: ImageUtils.createLocalImageConfiguration(context),
                onChanged: this.widget.onChanged,
                additionalConstraints: BoxConstraints.tight(this.getSwitchSize(theme)),
                vsync: this
            );
        }

//        Widget buildCupertinoSwitch(BuildContext context) {
//            Size size = this.getSwitchSize(Theme.of(context));
//            return new Container(
//                width: size.width, // Same size as the Material switch.
//                height: size.height,
//                alignment: Alignment.center,
//                child: CupertinoSwitch(
//                    value: this.widget.value,
//                    onChanged: this.widget.onChanged,
//                    activeColor: this.widget.activeColor
//                )
//            );
//        }

        public override Widget build(BuildContext context) {
            switch (this.widget._switchType) {
                case _SwitchType.material:
                    return this.buildMaterialSwitch(context);

                case _SwitchType.adaptive: {
                    return this.buildMaterialSwitch(context);
//                    ThemeData theme = Theme.of(context);
//                    D.assert(theme.platform != null);
//                    switch (theme.platform) {
//                        case TargetPlatform.android:
//                            return buildMaterialSwitch(context);
//                        case TargetPlatform.iOS:
//                            return buildCupertinoSwitch(context);
//                    }
//                    break;
                }
            }

            D.assert(false);
            return null;
        }
    }

    class _SwitchRenderObjectWidget : LeafRenderObjectWidget {
        public _SwitchRenderObjectWidget(
            Key key = null,
            bool? value = null,
            Color activeColor = null,
            Color inactiveColor = null,
            ImageProvider activeThumbImage = null,
            ImageProvider inactiveThumbImage = null,
            Color activeTrackColor = null,
            Color inactiveTrackColor = null,
            ImageConfiguration configuration = null,
            ValueChanged<bool?> onChanged = null,
            TickerProvider vsync = null,
            BoxConstraints additionalConstraints = null,
            DragStartBehavior? dragStartBehavior = null
        ) : base(key: key) {
            D.assert(value != null);
            this.value = value.Value;
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor;
            this.activeThumbImage = activeThumbImage;
            this.inactiveThumbImage = inactiveThumbImage;
            this.activeTrackColor = activeTrackColor;
            this.inactiveTrackColor = inactiveTrackColor;
            this.configuration = configuration;
            this.onChanged = onChanged;
            this.vsync = vsync;
            this.additionalConstraints = additionalConstraints;
            this.dragStartBehavior = dragStartBehavior;
        }

        public readonly bool value;
        public readonly Color activeColor;
        public readonly Color inactiveColor;
        public readonly ImageProvider activeThumbImage;
        public readonly ImageProvider inactiveThumbImage;
        public readonly Color activeTrackColor;
        public readonly Color inactiveTrackColor;
        public readonly ImageConfiguration configuration;
        public readonly ValueChanged<bool?> onChanged;
        public readonly TickerProvider vsync;
        public readonly BoxConstraints additionalConstraints;
        public readonly DragStartBehavior? dragStartBehavior;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderSwitch(
                dragStartBehavior: this.dragStartBehavior,
                value: this.value,
                activeColor: this.activeColor,
                inactiveColor: this.inactiveColor,
                activeThumbImage: this.activeThumbImage,
                inactiveThumbImage: this.inactiveThumbImage,
                activeTrackColor: this.activeTrackColor,
                inactiveTrackColor: this.inactiveTrackColor,
                configuration: this.configuration,
                onChanged: this.onChanged,
                additionalConstraints: this.additionalConstraints,
                vsync: this.vsync
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObjectRaw) {
            _RenderSwitch renderObject = (_RenderSwitch) renderObjectRaw;

            renderObject.value = this.value;
            renderObject.activeColor = this.activeColor;
            renderObject.inactiveColor = this.inactiveColor;
            renderObject.activeThumbImage = this.activeThumbImage;
            renderObject.inactiveThumbImage = this.inactiveThumbImage;
            renderObject.activeTrackColor = this.activeTrackColor;
            renderObject.inactiveTrackColor = this.inactiveTrackColor;
            renderObject.configuration = this.configuration;
            renderObject.onChanged = this.onChanged;
            renderObject.additionalConstraints = this.additionalConstraints;
            renderObject.dragStartBehavior = this.dragStartBehavior;
            renderObject.vsync = this.vsync;
        }
    }

    class _RenderSwitch : RenderToggleable {
        public _RenderSwitch(
            bool? value = null,
            Color activeColor = null,
            Color inactiveColor = null,
            ImageProvider activeThumbImage = null,
            ImageProvider inactiveThumbImage = null,
            Color activeTrackColor = null,
            Color inactiveTrackColor = null,
            ImageConfiguration configuration = null,
            BoxConstraints additionalConstraints = null,
            ValueChanged<bool?> onChanged = null,
            TickerProvider vsync = null,
            DragStartBehavior? dragStartBehavior = null
        ) : base(
            value: value,
            tristate: false,
            activeColor: activeColor,
            inactiveColor: inactiveColor,
            onChanged: onChanged,
            additionalConstraints: additionalConstraints,
            vsync: vsync
        ) {
            this._activeThumbImage = activeThumbImage;
            this._inactiveThumbImage = inactiveThumbImage;
            this._activeTrackColor = activeTrackColor;
            this._inactiveTrackColor = inactiveTrackColor;
            this._configuration = configuration;
            this._drag = new HorizontalDragGestureRecognizer {
                onStart = this._handleDragStart,
                onUpdate = this._handleDragUpdate,
                onEnd = this._handleDragEnd,
                dragStartBehavior = dragStartBehavior ?? DragStartBehavior.down
            };
        }

        public ImageProvider activeThumbImage {
            get { return this._activeThumbImage; }
            set {
                if (value == this._activeThumbImage) {
                    return;
                }

                this._activeThumbImage = value;
                this.markNeedsPaint();
            }
        }

        ImageProvider _activeThumbImage;

        public ImageProvider inactiveThumbImage {
            get { return this._inactiveThumbImage; }
            set {
                if (value == this._inactiveThumbImage) {
                    return;
                }

                this._inactiveThumbImage = value;
                this.markNeedsPaint();
            }
        }

        ImageProvider _inactiveThumbImage;

        public Color activeTrackColor {
            get { return this._activeTrackColor; }
            set {
                D.assert(value != null);
                if (value == this._activeTrackColor) {
                    return;
                }

                this._activeTrackColor = value;
                this.markNeedsPaint();
            }
        }

        Color _activeTrackColor;

        public Color inactiveTrackColor {
            get { return this._inactiveTrackColor; }
            set {
                D.assert(value != null);
                if (value == this._inactiveTrackColor) {
                    return;
                }

                this._inactiveTrackColor = value;
                this.markNeedsPaint();
            }
        }

        Color _inactiveTrackColor;

        public ImageConfiguration configuration {
            get { return this._configuration; }
            set {
                D.assert(value != null);
                if (value == this._configuration) {
                    return;
                }

                this._configuration = value;
                this.markNeedsPaint();
            }
        }

        ImageConfiguration _configuration;

        public DragStartBehavior? dragStartBehavior {
            get { return this._drag.dragStartBehavior; }
            set { this._drag.dragStartBehavior = value ?? DragStartBehavior.down; }
        }


        public override void detach() {
            this._cachedThumbPainter?.Dispose();
            this._cachedThumbPainter = null;
            base.detach();
        }

        float _trackInnerLength {
            get { return this.size.width - 2.0f * Constants.kRadialReactionRadius; }
        }

        HorizontalDragGestureRecognizer _drag;

        void _handleDragStart(DragStartDetails details) {
            if (this.isInteractive) {
                this.reactionController.forward();
            }
        }

        void _handleDragUpdate(DragUpdateDetails details) {
            if (this.isInteractive) {
                this.position.curve = null;
                this.position.reverseCurve = null;
                float delta = details.primaryDelta.Value / this._trackInnerLength;
                this.positionController.setValue(this.positionController.value + delta);
            }
        }

        void _handleDragEnd(DragEndDetails details) {
            if (this.position.value >= 0.5) {
                this.positionController.forward();
            }
            else {
                this.positionController.reverse();
            }

            this.reactionController.reverse();
        }

        public override void handleEvent(PointerEvent evt, HitTestEntry entry) {
            D.assert(this.debugHandleEvent(evt, entry));
            if (evt is PointerDownEvent && this.onChanged != null) {
                this._drag.addPointer((PointerDownEvent) evt);
            }

            base.handleEvent(evt, entry);
        }

        Color _cachedThumbColor;
        ImageProvider _cachedThumbImage;
        BoxPainter _cachedThumbPainter;

        BoxDecoration _createDefaultThumbDecoration(Color color, ImageProvider image) {
            return new BoxDecoration(
                color: color,
                image: image == null ? null : new DecorationImage(image: image),
                shape: BoxShape.circle,
                boxShadow: ShadowConstants.kElevationToShadow[1]
            );
        }

        bool _isPainting = false;

        void _handleDecorationChanged() {
            if (!this._isPainting) {
                this.markNeedsPaint();
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            Canvas canvas = context.canvas;

            bool isEnabled = this.onChanged != null;
            float currentValue = this.position.value;

            float visualPosition = currentValue;

            Color trackColor = isEnabled
                ? Color.lerp(this.inactiveTrackColor, this.activeTrackColor, currentValue)
                : this.inactiveTrackColor;

            Color thumbColor = isEnabled
                ? Color.lerp(this.inactiveColor, this.activeColor, currentValue)
                : this.inactiveColor;

            ImageProvider thumbImage = isEnabled
                ? (currentValue < 0.5 ? this.inactiveThumbImage : this.activeThumbImage)
                : this.inactiveThumbImage;

            // Paint the track
            Paint paint = new Paint {color = trackColor};
            float trackHorizontalPadding = Constants.kRadialReactionRadius - Switch._kTrackRadius;
            Rect trackRect = Rect.fromLTWH(
                offset.dx + trackHorizontalPadding,
                offset.dy + (this.size.height - Switch._kTrackHeight) / 2.0f,
                this.size.width - 2.0f * trackHorizontalPadding,
                Switch._kTrackHeight
            );
            RRect trackRRect = RRect.fromRectAndRadius(trackRect, Radius.circular(Switch._kTrackRadius));
            canvas.drawRRect(trackRRect, paint);

            Offset thumbPosition = new Offset(
                Constants.kRadialReactionRadius + visualPosition * this._trackInnerLength,
                this.size.height / 2.0f
            );

            this.paintRadialReaction(canvas, offset, thumbPosition);

            try {
                this._isPainting = true;
                BoxPainter thumbPainter;
                if (this._cachedThumbPainter == null || thumbColor != this._cachedThumbColor ||
                    thumbImage != this._cachedThumbImage) {
                    this._cachedThumbColor = thumbColor;
                    this._cachedThumbImage = thumbImage;
                    this._cachedThumbPainter = this._createDefaultThumbDecoration(thumbColor, thumbImage)
                        .createBoxPainter(this._handleDecorationChanged);
                }

                thumbPainter = this._cachedThumbPainter;

                float inset = 1.0f - (currentValue - 0.5f).abs() * 2.0f;
                float radius = Switch._kThumbRadius - inset;
                thumbPainter.paint(
                    canvas,
                    thumbPosition + offset - new Offset(radius, radius),
                    this.configuration.copyWith(size: Size.fromRadius(radius))
                );
            }
            finally {
                this._isPainting = false;
            }
        }
    }
}