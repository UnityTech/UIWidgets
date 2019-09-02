using System.Collections.Generic;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {

    public delegate Widget InputCounterWidgetBuilder(
        BuildContext buildContext,
        int? currentLength,
        int? maxLength,
        bool? isFocused);
        
    public class TextField : StatefulWidget {
        public TextField(Key key = null,
            TextEditingController controller = null,
            FocusNode focusNode = null,
            InputDecoration decoration = null,
            bool noDecoration = false,
            TextInputType keyboardType = null,
            TextInputAction? textInputAction = null,
            TextCapitalization textCapitalization = TextCapitalization.none,
            TextStyle style = null,
            StrutStyle strutStyle = null,
            TextAlign textAlign = TextAlign.left,
            TextDirection textDirection = TextDirection.ltr,
            bool autofocus = false,
            bool obscureText = false,
            bool autocorrect = false,
            int? maxLines = 1,
            int? minLines = null,
            bool expands = false,
            int? maxLength = null,
            bool maxLengthEnforced = true,
            ValueChanged<string> onChanged = null,
            VoidCallback onEditingComplete = null,
            ValueChanged<string> onSubmitted = null,
            List<TextInputFormatter> inputFormatters = null,
            bool? enabled = null,
            float? cursorWidth = 2.0f,
            Radius cursorRadius = null,
            Color cursorColor = null,
            Brightness? keyboardAppearance = null,
            EdgeInsets scrollPadding = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            bool? enableInteractiveSelection = null,
            GestureTapCallback onTap = null,
            InputCounterWidgetBuilder buildCounter = null,
            ScrollPhysics scrollPhysics = null
        ) : base(key: key) {
            D.assert(maxLines == null || maxLines > 0);
            D.assert(minLines == null || minLines > 0);
            D.assert((maxLines == null) || (minLines == null) || (maxLines >= minLines),
                () => "minLines can't be greater than maxLines");
            D.assert(!expands || (maxLines == null && minLines == null),
                () => "minLines and maxLines must be null when expands is true.");
            D.assert(maxLength == null || maxLength == TextField.noMaxLength || maxLength > 0);

            this.controller = controller;
            this.focusNode = focusNode;
            this.decoration = noDecoration ? null : (decoration ?? new InputDecoration());
            this.textInputAction = textInputAction;
            this.textCapitalization = textCapitalization;
            this.style = style;
            this.strutStyle = strutStyle;
            this.textAlign = textAlign;
            this.textDirection = textDirection;
            this.autofocus = autofocus;
            this.obscureText = obscureText;
            this.autocorrect = autocorrect;
            this.maxLines = maxLines;
            this.minLines = minLines;
            this.expands = expands;
            this.maxLength = maxLength;
            this.maxLengthEnforced = maxLengthEnforced;
            this.onChanged = onChanged;
            this.onEditingComplete = onEditingComplete;
            this.onSubmitted = onSubmitted;
            this.inputFormatters = inputFormatters;
            this.enabled = enabled;
            this.cursorWidth = cursorWidth;
            this.cursorColor = cursorColor;
            this.cursorRadius = cursorRadius;
            this.onSubmitted = onSubmitted;
            this.keyboardAppearance = keyboardAppearance;
            this.enableInteractiveSelection = enableInteractiveSelection;
            this.onTap = onTap;
            this.keyboardType = keyboardType ?? (maxLines == 1 ? TextInputType.text : TextInputType.multiline);
            this.scrollPadding = scrollPadding ?? EdgeInsets.all(20.0f);
            this.dragStartBehavior = dragStartBehavior;
            this.buildCounter = buildCounter;
            this.scrollPhysics = scrollPhysics;
        }

        public readonly TextEditingController controller;

        public readonly FocusNode focusNode;

        public readonly InputDecoration decoration;

        public readonly TextInputType keyboardType;

        public readonly TextInputAction? textInputAction;

        public readonly TextCapitalization textCapitalization;

        public readonly TextStyle style;

        public readonly StrutStyle strutStyle;

        public readonly TextAlign textAlign;

        public readonly TextDirection textDirection;

        public readonly bool autofocus;

        public readonly bool obscureText;

        public readonly bool autocorrect;

        public readonly int? maxLines;

        public readonly int? minLines;

        public readonly bool expands;
        
        public const long noMaxLength = -1;

        public readonly int? maxLength;

        public readonly bool maxLengthEnforced;

        public readonly ValueChanged<string> onChanged;

        public readonly VoidCallback onEditingComplete;

        public readonly ValueChanged<string> onSubmitted;

        public readonly List<TextInputFormatter> inputFormatters;

        public readonly bool? enabled;

        public readonly float? cursorWidth;

        public readonly Radius cursorRadius;

        public readonly Color cursorColor;

        public readonly Brightness? keyboardAppearance;

        public readonly EdgeInsets scrollPadding;

        public readonly bool? enableInteractiveSelection;
        
        public readonly DragStartBehavior dragStartBehavior;

        public readonly ScrollPhysics scrollPhysics;
        
        public bool selectionEnabled {
            get {
                return this.enableInteractiveSelection ?? !this.obscureText;
            }
        }

        public readonly GestureTapCallback onTap;

        public readonly InputCounterWidgetBuilder buildCounter;

        public override State createState() {
            return new _TextFieldState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(
                new DiagnosticsProperty<TextEditingController>("controller", this.controller, defaultValue: null));
            properties.add(new DiagnosticsProperty<FocusNode>("focusNode", this.focusNode, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool?>("enabled", this.enabled, defaultValue: null));
            properties.add(new DiagnosticsProperty<InputDecoration>("decoration", this.decoration, defaultValue: new InputDecoration()));
            properties.add(new DiagnosticsProperty<TextInputType>("keyboardType", this.keyboardType,
                defaultValue: TextInputType.text));
            properties.add(new DiagnosticsProperty<TextStyle>("style", this.style, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool>("autofocus", this.autofocus, defaultValue: false));
            properties.add(new DiagnosticsProperty<bool>("obscureText", this.obscureText, defaultValue: false));
            properties.add(new DiagnosticsProperty<bool>("autocorrect", this.autocorrect, defaultValue: true));
            properties.add(new IntProperty("maxLines", this.maxLines, defaultValue: 1));
            properties.add(new IntProperty("minLines", this.minLines, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool>("expands", this.expands, defaultValue: false));
            properties.add(new IntProperty("maxLength", this.maxLength, defaultValue: null));
            properties.add(new FlagProperty("maxLengthEnforced", value: this.maxLengthEnforced, defaultValue: true,
                ifFalse: "maxLength not enforced"));
            properties.add(new EnumProperty<TextInputAction?>("textInputAction", this.textInputAction, defaultValue: null));
            properties.add(new EnumProperty<TextCapitalization>("textCapitalization", this.textCapitalization, defaultValue: TextCapitalization.none));
            properties.add(new EnumProperty<TextAlign>("textAlign", this.textAlign, defaultValue: TextAlign.left));
            properties.add(new EnumProperty<TextDirection>("textDirection", this.textDirection, defaultValue: null));
            properties.add(new FloatProperty("cursorWidth", this.cursorWidth, defaultValue: 2.0f));
            properties.add(new DiagnosticsProperty<Radius>("cursorRadius", this.cursorRadius, defaultValue: null));
            properties.add(new DiagnosticsProperty<Color>("cursorColor", this.cursorColor, defaultValue: null));
            properties.add(new DiagnosticsProperty<Brightness?>("keyboardAppearance", this.keyboardAppearance, defaultValue: null));
            properties.add(new DiagnosticsProperty<EdgeInsets>("scrollPadding", this.scrollPadding, defaultValue: EdgeInsets.all(20.0f)));
            properties.add(new FlagProperty("selectionEnabled", value: this.selectionEnabled, defaultValue: true, ifFalse: "selection disabled"));
            properties.add(new DiagnosticsProperty<ScrollPhysics>("scrollPhysics", this.scrollPhysics, defaultValue: null));
        }
    }

    class _TextFieldState : AutomaticKeepAliveClientMixin<TextField> {
        readonly GlobalKey<EditableTextState> _editableTextKey = new LabeledGlobalKey<EditableTextState>();

        HashSet<InteractiveInkFeature> _splashes;
        InteractiveInkFeature _currentSplash;

        TextEditingController _controller;

        TextEditingController _effectiveController {
            get { return this.widget.controller ?? this._controller; }
        }

        FocusNode _focusNode;

        FocusNode _effectiveFocusNode {
            get {
                if (this.widget.focusNode != null) {
                    return this.widget.focusNode;
                }

                if (this._focusNode != null) {
                    return this._focusNode;
                }

                this._focusNode = new FocusNode();
                return this._focusNode;
            }
        }

        bool needsCounter {
            get {
                return this.widget.maxLength != null
                       && this.widget.decoration != null
                       && this.widget.decoration.counterText == null;
            }
        }

        InputDecoration _getEffectiveDecoration() {
            MaterialLocalizations localizations = MaterialLocalizations.of(this.context);
            ThemeData themeData = Theme.of(this.context);
            InputDecoration effectiveDecoration = (this.widget.decoration ?? new InputDecoration())
                .applyDefaults(themeData.inputDecorationTheme)
                .copyWith(
                    enabled: this.widget.enabled,
                    hintMaxLines: this.widget.decoration?.hintMaxLines ?? this.widget.maxLines
                );

            if (effectiveDecoration.counter != null || effectiveDecoration.counterText != null) {
                return effectiveDecoration;
            }

            Widget counter;
            int currentLength = this._effectiveController.value.text.Length;
            if (effectiveDecoration.counter == null
                && effectiveDecoration.counterText == null
                && this.widget.buildCounter != null) {
                bool isFocused = this._effectiveFocusNode.hasFocus;
                counter = this.widget.buildCounter(
                    this.context,
                    currentLength: currentLength,
                    maxLength: this.widget.maxLength,
                    isFocused: isFocused
                );
                return effectiveDecoration.copyWith(counter: counter);
            }

            if (this.widget.maxLength == null) {
                return effectiveDecoration;
            }

            string counterText = $"{currentLength}";

            if (this.widget.maxLength > 0) {
                counterText += $"/{this.widget.maxLength}";
                if (this._effectiveController.value.text.Length > this.widget.maxLength) {
                    return effectiveDecoration.copyWith(
                        errorText: effectiveDecoration.errorText ?? "",
                        counterStyle: effectiveDecoration.errorStyle
                                      ?? themeData.textTheme.caption.copyWith(color: themeData.errorColor),
                        counterText: counterText
                    );
                }
            }

            // Handle length exceeds maxLength

            return effectiveDecoration.copyWith(
                counterText: counterText
            );
        }

        public override void initState() {
            base.initState();
            if (this.widget.controller == null) {
                this._controller = new TextEditingController();
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            if (this.widget.controller == null && ((TextField) oldWidget).controller != null) {
                this._controller = TextEditingController.fromValue(((TextField) oldWidget).controller.value);
            }
            else if (this.widget.controller != null && ((TextField) oldWidget).controller == null) {
                this._controller = null;
            }

            bool isEnabled = this.widget.enabled ?? this.widget.decoration?.enabled ?? true;
            bool wasEnabled = ((TextField) oldWidget).enabled ?? ((TextField) oldWidget).decoration?.enabled ?? true;
            if (wasEnabled && !isEnabled) {
                this._effectiveFocusNode.unfocus();
            }
        }

        public override void dispose() {
            this._focusNode?.dispose();
            base.dispose();
        }

        void _requestKeyboard() {
            this._editableTextKey.currentState?.requestKeyboard();
        }

        void _handleSelectionChanged(TextSelection selection, SelectionChangedCause cause) {
            switch (Theme.of(this.context).platform) {
                case RuntimePlatform.IPhonePlayer:
                    if (cause == SelectionChangedCause.longPress) {
                        this._editableTextKey.currentState?.bringIntoView(selection.basePos);
                    }
                    return;
                case RuntimePlatform.Android:
                    break;
            }
        }

        InteractiveInkFeature _createInkFeature(Offset globalPosition) {
            MaterialInkController inkController = Material.of(this.context);
            ThemeData themeData = Theme.of(this.context);
            BuildContext editableContext = this._editableTextKey.currentContext;
            RenderBox referenceBox =
                (RenderBox) (InputDecorator.containerOf(editableContext) ?? editableContext.findRenderObject());
            Offset position = referenceBox.globalToLocal(globalPosition);
            Color color = themeData.splashColor;

            InteractiveInkFeature splash = null;

            void handleRemoved() {
                if (this._splashes != null) {
                    D.assert(this._splashes.Contains(splash));
                    this._splashes.Remove(splash);
                    if (this._currentSplash == splash) {
                        this._currentSplash = null;
                    }

                    this.updateKeepAlive();
                } // else we're probably in deactivate()
            }

            splash = themeData.splashFactory.create(
                controller: inkController,
                referenceBox: referenceBox,
                position: position,
                color: color,
                containedInkWell: true,
                borderRadius: BorderRadius.zero,
                onRemoved: handleRemoved
            );

            return splash;
        }

        RenderEditable _renderEditable {
            get { return this._editableTextKey.currentState.renderEditable; }
        }

        void _handleTapDown(TapDownDetails details) {
            this._renderEditable.handleTapDown(details);
            this._startSplash(details.globalPosition);
        }

        void _handleSingleTapUp(TapUpDetails details) {
            if (this.widget.enableInteractiveSelection == true) {
                this._renderEditable.handleTap();
            }

            this._requestKeyboard();
            this._confirmCurrentSplash();
            if (this.widget.onTap != null) {
                this.widget.onTap();
            }
        }

        void _handleSingleTapCancel() {
            this._cancelCurrentSplash();
        }

        void _handleSingleLongTapStart(LongPressStartDetails details) {
            if (this.widget.selectionEnabled) {
                switch (Theme.of(this.context).platform) {
                    case RuntimePlatform.IPhonePlayer:
                        this._renderEditable.selectPositionAt(
                            from: details.globalPosition,
                            cause: SelectionChangedCause.longPress
                        );
                        break;
                    case RuntimePlatform.Android:
                        this._renderEditable.selectWord(cause: SelectionChangedCause.longPress);
                        Feedback.forLongPress(this.context);
                        break;
                }
            }
            this._confirmCurrentSplash();
        }

        void _handleSingleLongTapMoveUpdate(LongPressMoveUpdateDetails details) {
            if (this.widget.selectionEnabled) {
                switch (Theme.of(this.context).platform) {
                    case RuntimePlatform.IPhonePlayer:
                        this._renderEditable.selectPositionAt(
                            from: details.globalPosition,
                            cause: SelectionChangedCause.longPress
                        );
                        break;
                    case RuntimePlatform.Android:
                        this._renderEditable.selectWordsInRange(
                            from: details.globalPosition - details.offsetFromOrigin,
                            to: details.globalPosition,
                            cause: SelectionChangedCause.longPress);
                        Feedback.forLongPress(this.context);
                        break;
                }
            }
        }

        void _handleSingleLongTapEnd(LongPressEndDetails details) {
            this._editableTextKey.currentState.showToolbar();
        }

        void _handleDoubleTapDown(TapDownDetails details) {
            if (this.widget.selectionEnabled) {
                this._renderEditable.selectWord(cause: SelectionChangedCause.doubleTap);
                this._editableTextKey.currentState.showToolbar();
            }
        }

        void _handleMouseDragSelectionStart(DragStartDetails details) {
            this._renderEditable.selectPositionAt(
                from: details.globalPosition,
                cause: SelectionChangedCause.drag);

            this._startSplash(details.globalPosition);
        }

        void _handleMouseDragSelectionUpdate(DragStartDetails startDetails,
            DragUpdateDetails updateDetails) {
            this._renderEditable.selectPositionAt(
                from: startDetails.globalPosition,
                to: updateDetails.globalPosition,
                cause: SelectionChangedCause.drag);
        }


        void _startSplash(Offset globalPosition) {
            if (this._effectiveFocusNode.hasFocus) {
                return;
            }

            InteractiveInkFeature splash = this._createInkFeature(globalPosition);
            this._splashes = this._splashes ?? new HashSet<InteractiveInkFeature>();
            this._splashes.Add(splash);
            this._currentSplash = splash;
            this.updateKeepAlive();
        }

        void _confirmCurrentSplash() {
            this._currentSplash?.confirm();
            this._currentSplash = null;
        }

        void _cancelCurrentSplash() {
            this._currentSplash?.cancel();
        }

        protected override bool wantKeepAlive {
            get { return this._splashes != null && this._splashes.isNotEmpty(); }
        }

        public override void deactivate() {
            if (this._splashes != null) {
                HashSet<InteractiveInkFeature> splashes = this._splashes;
                this._splashes = null;
                foreach (InteractiveInkFeature splash in splashes) {
                    splash.dispose();
                }

                this._currentSplash = null;
            }

            D.assert(this._currentSplash == null);
            base.deactivate();
        }

        public override Widget build(BuildContext context) {
            base.build(context); // See AutomaticKeepAliveClientMixin.
            D.assert(MaterialD.debugCheckHasMaterial(context));
            D.assert(WidgetsD.debugCheckHasDirectionality(context));
            D.assert(
              !(this.widget.style != null && this.widget.style.inherit == false &&
                (this.widget.style.fontSize == null || this.widget.style.textBaseline == null)),
              () => "inherit false style must supply fontSize and textBaseline"
            );
            ThemeData themeData = Theme.of(context);
            TextStyle style = themeData.textTheme.subhead.merge(this.widget.style);
            Brightness keyboardAppearance = this.widget.keyboardAppearance ?? themeData.primaryColorBrightness;
            TextEditingController controller = this._effectiveController;
            FocusNode focusNode = this._effectiveFocusNode;
            List<TextInputFormatter> formatters = this.widget.inputFormatters ?? new List<TextInputFormatter>();
            if (this.widget.maxLength != null && this.widget.maxLengthEnforced) {
                formatters.Add(new LengthLimitingTextInputFormatter(this.widget.maxLength));
            }
            
            // bool forcePressEnabled = false; // TODO: wait for force press is ready
            TextSelectionControls textSelectionControls = MaterialUtils.materialTextSelectionControls;;
            bool paintCursorAboveText = false;
            bool cursorOpacityAnimates = false;
            Offset cursorOffset = null;
            Color cursorColor = this.widget.cursorColor ?? themeData.cursorColor;
            Radius cursorRadius = this.widget.cursorRadius;

            Widget child = new RepaintBoundary(
                child: new EditableText(
                    key: this._editableTextKey,
                    controller: controller,
                    focusNode: focusNode,
                    keyboardType: this.widget.keyboardType,
                    textInputAction: this.widget.textInputAction,
                    textCapitalization: this.widget.textCapitalization,
                    style: style,
                    strutStyle: this.widget.strutStyle,
                    textAlign: this.widget.textAlign,
                    textDirection: this.widget.textDirection,
                    autofocus: this.widget.autofocus,
                    obscureText: this.widget.obscureText,
                    autocorrect: this.widget.autocorrect,
                    maxLines: this.widget.maxLines,
                    minLines: this.widget.minLines,
                    expands: this.widget.expands,
                    selectionColor: themeData.textSelectionColor,
                    selectionControls: this.widget.selectionEnabled ? textSelectionControls : null,
                    onChanged: this.widget.onChanged,
                    onSelectionChanged: this._handleSelectionChanged,
                    onEditingComplete: this.widget.onEditingComplete,
                    onSubmitted: this.widget.onSubmitted,
                    inputFormatters: formatters,
                    rendererIgnoresPointer: true,
                    cursorWidth: this.widget.cursorWidth,
                    cursorRadius: cursorRadius,
                    cursorColor: cursorColor,
                    cursorOpacityAnimates: cursorOpacityAnimates,
                    cursorOffset: cursorOffset,
                    paintCursorAboveText: paintCursorAboveText,
                    backgroundCursorColor: new Color(0xFF8E8E93),// TODO: CupertinoColors.inactiveGray,
                    scrollPadding: this.widget.scrollPadding,
                    keyboardAppearance: keyboardAppearance,
                    enableInteractiveSelection: this.widget.enableInteractiveSelection == true,
                    dragStartBehavior: this.widget.dragStartBehavior,
                    scrollPhysics: this.widget.scrollPhysics
                )
            );

            if (this.widget.decoration != null) {
                child = new AnimatedBuilder(
                    animation: ListenableUtils.merge(new List<Listenable> {focusNode, controller}),
                    builder:
                    (_context, _child) => {
                        return new InputDecorator(
                            decoration: this._getEffectiveDecoration(),
                            baseStyle: this.widget.style,
                            textAlign: this.widget.textAlign,
                            isFocused: focusNode.hasFocus,
                            isEmpty: controller.value.text.isEmpty(),
                            expands: this.widget.expands,
                            child: _child
                        );
                    },
                    child: child
                );
            }

            return new IgnorePointer(
                ignoring: !(this.widget.enabled ?? this.widget.decoration?.enabled ?? true),
                child: new TextSelectionGestureDetector(
                    onTapDown: this._handleTapDown,
                    // onForcePressStart: forcePressEnabled ? this._handleForcePressStarted : null, // TODO: Remove this when force press is added
                    onSingleTapUp: this._handleSingleTapUp,
                    onSingleTapCancel: this._handleSingleTapCancel,
                    onSingleLongTapStart: this._handleSingleLongTapStart,
                    onSingleLongTapMoveUpdate: this._handleSingleLongTapMoveUpdate,
                    onSingleLongTapEnd: this._handleSingleLongTapEnd,
                    onDoubleTapDown: this._handleDoubleTapDown,
                    onDragSelectionStart: this._handleMouseDragSelectionStart,
                    onDragSelectionUpdate: this._handleMouseDragSelectionUpdate,
                    behavior: HitTestBehavior.translucent,
                    child: child
                )
            );
        }
    }
}