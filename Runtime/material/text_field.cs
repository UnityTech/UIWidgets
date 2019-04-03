using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public class TextField : StatefulWidget {

        public TextField(Key key = null, TextEditingController controller = null, FocusNode focusNode = null,
            InputDecoration decoration = null, bool noDecoration = false, TextInputType keyboardType = null,
            TextInputAction? textInputAction = null,
            TextCapitalization textCapitalization = TextCapitalization.none, TextStyle style = null,
            TextAlign textAlign = TextAlign.left, TextDirection textDirection = TextDirection.ltr,
            bool autofocus = false, bool obscureText = false, bool autocorrect = false, int? maxLines = 1,
            int? maxLength = null, bool maxLengthEnforced = true, ValueChanged<string> onChanged = null,
            VoidCallback onEditingComplete = null,
            ValueChanged<string> onSubmitted = null, List<TextInputFormatter> inputFormatters = null,
            bool? enabled = null, float? cursorWidth = 2.0f, Radius cursorRadius = null, Color cursorColor = null,
            Brightness? keyboardAppearance = null, EdgeInsets scrollPadding = null,
            bool enableInteractiveSelection = true,
            GestureTapCallback onTap = null
        ) : base(key: key) {
            D.assert(maxLines == null || maxLines > 0);
            D.assert(maxLength == null || maxLength > 0);

            this.controller = controller;
            this.focusNode = focusNode;
            this.decoration = noDecoration ? null : (decoration ?? new InputDecoration());
            this.textInputAction = textInputAction;
            this.textCapitalization = textCapitalization;
            this.style = style;
            this.textAlign = textAlign;
            this.textDirection = textDirection;
            this.autofocus = autofocus;
            this.obscureText = obscureText;
            this.autocorrect = autocorrect;
            this.maxLines = maxLines;
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
        }

        public readonly TextEditingController controller;

        public readonly FocusNode focusNode;

        public readonly InputDecoration decoration;

        public readonly TextInputType keyboardType;

        public readonly TextInputAction? textInputAction;

        public readonly TextCapitalization textCapitalization;

        public readonly TextStyle style;

        public readonly TextAlign textAlign;

        public readonly TextDirection textDirection;

        public readonly bool autofocus;

        public readonly bool obscureText;

        public readonly bool autocorrect;

        public readonly int? maxLines;

        public const long noMaxLength = 9007199254740992; // math.pow(2, 53);

        public readonly int? maxLength;

        public readonly bool maxLengthEnforced;

        public readonly ValueChanged<string> onChanged;

        public readonly VoidCallback onEditingComplete;

        public readonly ValueChanged<String> onSubmitted;

        public readonly List<TextInputFormatter> inputFormatters;

        public readonly bool? enabled;

        public readonly float? cursorWidth;

        public readonly Radius cursorRadius;

        public readonly Color cursorColor;

        public readonly Brightness? keyboardAppearance;

        public readonly EdgeInsets scrollPadding;

        public readonly bool enableInteractiveSelection;

        public readonly GestureTapCallback onTap;

        public override State createState() {
            return new _TextFieldState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(
                new DiagnosticsProperty<TextEditingController>("controller", this.controller, defaultValue: null));
            properties.add(new DiagnosticsProperty<FocusNode>("focusNode", this.focusNode, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool?>("enabled", this.enabled, defaultValue: null));
            properties.add(new DiagnosticsProperty<InputDecoration>("decoration", this.decoration));
            properties.add(new DiagnosticsProperty<TextInputType>("keyboardType", this.keyboardType,
                defaultValue: TextInputType.text));
            properties.add(new DiagnosticsProperty<TextStyle>("style", this.style, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool>("autofocus", this.autofocus, defaultValue: false));
            properties.add(new DiagnosticsProperty<bool>("obscureText", this.obscureText, defaultValue: false));
            properties.add(new DiagnosticsProperty<bool>("autocorrect", this.autocorrect, defaultValue: false));
            properties.add(new IntProperty("maxLines", this.maxLines, defaultValue: 1));
            properties.add(new IntProperty("maxLength", this.maxLength, defaultValue: null));
            properties.add(new FlagProperty("maxLengthEnforced", value: this.maxLengthEnforced,
                ifTrue: "max length enforced"));
            properties.add(new DiagnosticsProperty<GestureTapCallback>("onTap", this.onTap, defaultValue: null));
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
            InputDecoration effectiveDecoration = (this.widget.decoration ?? new InputDecoration())
                .applyDefaults(Theme.of(this.context).inputDecorationTheme)
                .copyWith(
                    enabled: this.widget.enabled
                );

            if (!this.needsCounter) {
                return effectiveDecoration;
            }

            int currentLength = this._effectiveController.value.text.Length;
            string counterText = $"{currentLength}";


            if (this.widget.maxLength != TextField.noMaxLength) {
                counterText += $"/{this.widget.maxLength}";
            }

            // Handle length exceeds maxLength
            if (this._effectiveController.value.text.Length > this.widget.maxLength) {
                ThemeData themeData = Theme.of(this.context);
                return effectiveDecoration.copyWith(
                    errorText: effectiveDecoration.errorText ?? "",
                    counterStyle: effectiveDecoration.errorStyle
                                  ?? themeData.textTheme.caption.copyWith(color: themeData.errorColor),
                    counterText: counterText
                );
            }

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
            if (cause == SelectionChangedCause.longPress) {
                // Feedback.forLongPress(context); todo add feedback
            }
        }

        InteractiveInkFeature _createInkFeature(TapDownDetails details) {
            MaterialInkController inkController = Material.of(this.context);
            BuildContext editableContext = this._editableTextKey.currentContext;
            RenderBox referenceBox =
                (RenderBox) (InputDecorator.containerOf(editableContext) ?? editableContext.findRenderObject());
            Offset position = referenceBox.globalToLocal(details.globalPosition);
            Color color = Theme.of(this.context).splashColor;

            InteractiveInkFeature splash = null;

            void handleRemoved() {
                if (this._splashes != null) {
                    D.assert(this._splashes.Contains(splash));
                    this._splashes.Remove(splash);
                    if (this._currentSplash == splash) this._currentSplash = null;
                    this.updateKeepAlive();
                } // else we're probably in deactivate()
            }

            splash = Theme.of(this.context).splashFactory.create(
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
            get {
                return this._editableTextKey.currentState.renderEditable;
            }
        } 

        void _handleTapDown(TapDownDetails details) {
            this._renderEditable.handleTapDown(details);
            this._startSplash(details);
        }

        void _handleTap() {
            if (this.widget.enableInteractiveSelection) {
                this._renderEditable.handleTap();
            }

            this._requestKeyboard();
            this._confirmCurrentSplash();
            if (this.widget.onTap != null) {
                this.widget.onTap();
            }
        }

        void _handleTapCancel() {
            this._cancelCurrentSplash();
        }

        void _handleLongPress() {
            if (this.widget.enableInteractiveSelection) {
                this._renderEditable.handleLongPress();
            }

            this._confirmCurrentSplash();
        }

        void _startSplash(TapDownDetails details) {
            if (this._effectiveFocusNode.hasFocus) {
                return;
            }

            InteractiveInkFeature splash = this._createInkFeature(details);
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
            ThemeData themeData = Theme.of(context);
            TextStyle style = this.widget.style ?? themeData.textTheme.subhead;
            Brightness keyboardAppearance = this.widget.keyboardAppearance ?? themeData.primaryColorBrightness;
            TextEditingController controller = this._effectiveController;
            FocusNode focusNode = this._effectiveFocusNode;
            List<TextInputFormatter> formatters = this.widget.inputFormatters ?? new List<TextInputFormatter>();
            if (this.widget.maxLength != null && this.widget.maxLengthEnforced) {
                formatters.Add(new LengthLimitingTextInputFormatter(this.widget.maxLength));
            }


            Widget child = new RepaintBoundary(
                child: new EditableText(
                    key: this._editableTextKey,
                    controller: controller,
                    focusNode: focusNode,
                    keyboardType: this.widget.keyboardType,
                    textInputAction: this.widget.textInputAction,
                    textCapitalization: this.widget.textCapitalization,
                    style: style,
                    textAlign: this.widget.textAlign,
                    textDirection: this.widget.textDirection,
                    autofocus: this.widget.autofocus,
                    obscureText: this.widget.obscureText,
                    autocorrect: this.widget.autocorrect,
                    maxLines: this.widget.maxLines,
                    selectionColor: themeData.textSelectionColor,
                    selectionControls: this.widget.enableInteractiveSelection
                        ? MaterialUtils.materialTextSelectionControls
                        : null,
                    onChanged: this.widget.onChanged,
                    onEditingComplete: this.widget.onEditingComplete,
                    onSubmitted: this.widget.onSubmitted,
                    onSelectionChanged: this._handleSelectionChanged,
                    inputFormatters: formatters,
                    rendererIgnoresPointer: true,
                    cursorWidth: this.widget.cursorWidth,
                    cursorRadius: this.widget.cursorRadius,
                    cursorColor: this.widget.cursorColor ?? Theme.of(context).cursorColor,
                    scrollPadding: this.widget.scrollPadding,
                    keyboardAppearance: keyboardAppearance,
                    enableInteractiveSelection: this.widget.enableInteractiveSelection
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
                            child: _child
                        );
                    },
                    child: child
                );
            }

            return new IgnorePointer(
                ignoring: !(this.widget.enabled ?? this.widget.decoration?.enabled ?? true),
                child: new GestureDetector(
                    behavior: HitTestBehavior.translucent,
                    onTapDown: this._handleTapDown,
                    onTap: this._handleTap,
                    onTapCancel: this._handleTapCancel,
                    onLongPress: this._handleLongPress,
                    child: child
                )
            );

        }
    }
}