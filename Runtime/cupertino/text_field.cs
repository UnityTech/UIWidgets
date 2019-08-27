using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.cupertino {
    class CupertinoTextFieldUtils {
        public static readonly BorderSide _kDefaultRoundedBorderSide = new BorderSide(
            color: CupertinoColors.lightBackgroundGray,
            style: BorderStyle.solid,
            width: 0.0f
        );

        public static readonly Border _kDefaultRoundedBorder = new Border(
            top: _kDefaultRoundedBorderSide,
            bottom: _kDefaultRoundedBorderSide,
            left: _kDefaultRoundedBorderSide,
            right: _kDefaultRoundedBorderSide
        );

        public static readonly BoxDecoration _kDefaultRoundedBorderDecoration = new BoxDecoration(
            border: _kDefaultRoundedBorder,
            borderRadius: BorderRadius.all(Radius.circular(4.0f))
        );

        public static readonly Color _kSelectionHighlightColor = new Color(0x667FAACF);

        public static readonly Color _kInactiveTextColor = new Color(0xFFC2C2C2);

        public static readonly Color _kDisabledBackground = new Color(0xFFFAFAFA);

        public const int _iOSHorizontalCursorOffsetPixels = -2;
    }

    public enum OverlayVisibilityMode {
        never,
        editing,
        notEditing,
        always
    }

    public class CupertinoTextField : StatefulWidget {
        public CupertinoTextField(
            Key key = null,
            TextEditingController controller = null,
            FocusNode focusNode = null,
            BoxDecoration decoration = null,
            EdgeInsets padding = null,
            string placeholder = null,
            TextStyle placeholderStyle = null,
            Widget prefix = null,
            OverlayVisibilityMode prefixMode = OverlayVisibilityMode.always,
            Widget suffix = null,
            OverlayVisibilityMode suffixMode = OverlayVisibilityMode.always,
            OverlayVisibilityMode clearButtonMode = OverlayVisibilityMode.never,
            TextInputType keyboardType = null,
            TextInputAction? textInputAction = null,
            TextCapitalization textCapitalization = TextCapitalization.none,
            TextStyle style = null,
            StrutStyle strutStyle = null,
            TextAlign textAlign = TextAlign.left,
            bool autofocus = false,
            bool obscureText = false,
            bool autocorrect = true,
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
            float cursorWidth = 2.0f,
            Radius cursorRadius = null,
            Color cursorColor = null,
            Brightness? keyboardAppearance = null,
            EdgeInsets scrollPadding = null,
            DragStartBehavior dragStartBehavior = DragStartBehavior.start,
            ScrollPhysics scrollPhysics = null) : base(key: key) {
            D.assert(maxLines == null || maxLines > 0);
            D.assert(minLines == null || minLines > 0);
            D.assert(maxLines == null || minLines == null || maxLines >= minLines,
                () => "minLines can't be greater than maxLines");
            D.assert(!expands || (maxLines == null && minLines == null),
                () => "minLines and maxLines must be null when expands is true.");
            D.assert(maxLength == null || maxLength > 0);

            this.controller = controller;
            this.focusNode = focusNode;
            this.decoration = decoration ?? CupertinoTextFieldUtils._kDefaultRoundedBorderDecoration;
            this.padding = padding ?? EdgeInsets.all(6.0f);
            this.placeholder = placeholder;
            this.placeholderStyle = placeholderStyle ?? new TextStyle(
                                        fontWeight: FontWeight.w300,
                                        color: CupertinoTextFieldUtils._kInactiveTextColor
                                    );
            this.prefix = prefix;
            this.prefixMode = prefixMode;
            this.suffix = suffix;
            this.suffixMode = suffixMode;
            this.clearButtonMode = clearButtonMode;
            this.textInputAction = textInputAction;
            this.textCapitalization = textCapitalization;
            this.style = style;
            this.strutStyle = strutStyle;
            this.textAlign = textAlign;
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
            this.cursorRadius = cursorRadius ?? Radius.circular(2.0f);
            this.cursorColor = cursorColor;
            this.keyboardAppearance = keyboardAppearance;
            this.scrollPadding = scrollPadding ?? EdgeInsets.all(20.0f);
            this.dragStartBehavior = dragStartBehavior;
            this.scrollPhysics = scrollPhysics;
            this.keyboardType = keyboardType ?? (maxLines == 1 ? TextInputType.text : TextInputType.multiline);
        }

        public readonly TextEditingController controller;

        public readonly FocusNode focusNode;

        public readonly BoxDecoration decoration;

        public readonly EdgeInsets padding;

        public readonly string placeholder;

        public readonly TextStyle placeholderStyle;

        public readonly Widget prefix;

        public readonly OverlayVisibilityMode prefixMode;

        public readonly Widget suffix;

        public readonly OverlayVisibilityMode suffixMode;

        public readonly OverlayVisibilityMode clearButtonMode;

        public readonly TextInputType keyboardType;

        public readonly TextInputAction? textInputAction;

        public readonly TextCapitalization textCapitalization;

        public readonly TextStyle style;

        public readonly StrutStyle strutStyle;

        public readonly TextAlign textAlign;

        public readonly bool autofocus;

        public readonly bool obscureText;

        public readonly bool autocorrect;

        public readonly int? maxLines;

        public readonly int? minLines;

        public readonly bool expands;

        public readonly int? maxLength;

        public readonly bool maxLengthEnforced;

        public readonly ValueChanged<string> onChanged;

        public readonly VoidCallback onEditingComplete;

        public readonly ValueChanged<string> onSubmitted;

        public readonly List<TextInputFormatter> inputFormatters;

        public readonly bool? enabled;

        public readonly float cursorWidth;

        public readonly Radius cursorRadius;

        public readonly Color cursorColor;

        public readonly Brightness? keyboardAppearance;

        public readonly EdgeInsets scrollPadding;

        public readonly DragStartBehavior dragStartBehavior;

        public readonly ScrollPhysics scrollPhysics;

        public override State createState() {
            return new _CupertinoTextFieldState();
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);

            properties.add(
                new DiagnosticsProperty<TextEditingController>("controller", this.controller, defaultValue: null));
            properties.add(new DiagnosticsProperty<FocusNode>("focusNode", this.focusNode, defaultValue: null));
            properties.add(new DiagnosticsProperty<BoxDecoration>("decoration", this.decoration));
            properties.add(new DiagnosticsProperty<EdgeInsets>("padding", this.padding));
            properties.add(new StringProperty("placeholder", this.placeholder));
            properties.add(new DiagnosticsProperty<TextStyle>("placeholderStyle", this.placeholderStyle));
            properties.add(new DiagnosticsProperty<OverlayVisibilityMode>("prefix",
                this.prefix == null ? OverlayVisibilityMode.never : this.prefixMode));
            properties.add(new DiagnosticsProperty<OverlayVisibilityMode>("suffix",
                this.suffix == null ? OverlayVisibilityMode.never : this.suffixMode));
            properties.add(new DiagnosticsProperty<OverlayVisibilityMode>("clearButtonMode", this.clearButtonMode));
            properties.add(new DiagnosticsProperty<TextInputType>("keyboardType", this.keyboardType,
                defaultValue: TextInputType.text));
            properties.add(new DiagnosticsProperty<TextStyle>("style", this.style, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool>("autofocus", this.autofocus, defaultValue: false));
            properties.add(new DiagnosticsProperty<bool>("obscureText", this.obscureText, defaultValue: false));
            properties.add(new DiagnosticsProperty<bool>("autocorrect", this.autocorrect, defaultValue: false));
            properties.add(new IntProperty("maxLines", this.maxLines, defaultValue: 1));
            properties.add(new IntProperty("minLines", this.minLines, defaultValue: null));
            properties.add(new DiagnosticsProperty<bool>("expands", this.expands, defaultValue: false));
            properties.add(new IntProperty("maxLength", this.maxLength, defaultValue: null));
            properties.add(new FlagProperty("maxLengthEnforced", value: this.maxLengthEnforced,
                ifTrue: "max length enforced"));
            properties.add(new DiagnosticsProperty<Color>("cursorColor", this.cursorColor, defaultValue: null));
            properties.add(
                new DiagnosticsProperty<ScrollPhysics>("scrollPhysics", this.scrollPhysics, defaultValue: null));
        }
    }

    class _CupertinoTextFieldState : AutomaticKeepAliveClientMixin<CupertinoTextField> {
        GlobalKey<EditableTextState> _editableTextKey = GlobalKey<EditableTextState>.key();

        TextEditingController _controller;

        TextEditingController _effectiveController {
            get { return this.widget.controller ?? this._controller; }
        }

        FocusNode _focusNode;

        FocusNode _effectiveFocusNode {
            get { return this.widget.focusNode ?? this._focusNode ?? (this._focusNode = new FocusNode()); }
        }

        public override void initState() {
            base.initState();
            if (this.widget.controller == null) {
                this._controller = new TextEditingController();
                this._controller.addListener(this.updateKeepAlive);
            }
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget);
            CupertinoTextField _oldWidget = (CupertinoTextField) oldWidget;

            if (this.widget.controller == null && _oldWidget.controller != null) {
                this._controller = TextEditingController.fromValue(_oldWidget.controller.value);
                this._controller.addListener(this.updateKeepAlive);
            }
            else if (this.widget.controller != null && _oldWidget.controller == null) {
                this._controller = null;
            }

            bool isEnabled = this.widget.enabled ?? true;
            bool wasEnabled = _oldWidget.enabled ?? true;

            if (wasEnabled && !isEnabled) {
                this._effectiveFocusNode.unfocus();
            }
        }

        public override void dispose() {
            this._focusNode?.dispose();
            this._controller?.removeListener(this.updateKeepAlive);
            base.dispose();
        }

        void _requestKeyboard() {
            this._editableTextKey.currentState?.requestKeyboard();
        }

        RenderEditable _renderEditable {
            get { return this._editableTextKey.currentState.renderEditable; }
        }

        void _handleTapDown(TapDownDetails details) {
            this._renderEditable.handleTapDown(details);
        }


        void _handleSingleTapUp(TapUpDetails details) {
            this._renderEditable.selectWordEdge(cause: SelectionChangedCause.tap);
            this._requestKeyboard();
        }

        void _handleSingleLongTapStart(LongPressStartDetails details) {
            this._renderEditable.selectPositionAt(
                from: details.globalPosition,
                cause: SelectionChangedCause.longPress
            );
        }

        void _handleSingleLongTapMoveUpdate(LongPressMoveUpdateDetails details) {
            this._renderEditable.selectPositionAt(
                from: details.globalPosition,
                cause: SelectionChangedCause.longPress
            );
        }

        void _handleSingleLongTapEnd(LongPressEndDetails details) {
            this._editableTextKey.currentState.showToolbar();
        }

        void _handleDoubleTapDown(TapDownDetails details) {
            this._renderEditable.selectWord(cause: SelectionChangedCause.tap);
            this._editableTextKey.currentState.showToolbar();
        }

        void _handleMouseDragSelectionStart(DragStartDetails details) {
            this._renderEditable.selectPositionAt(
                from: details.globalPosition,
                cause: SelectionChangedCause.drag
            );
        }

        void _handleMouseDragSelectionUpdate(
            DragStartDetails startDetails,
            DragUpdateDetails updateDetails
        ) {
            this._renderEditable.selectPositionAt(
                from: startDetails.globalPosition,
                to: updateDetails.globalPosition,
                cause: SelectionChangedCause.drag
            );
        }

        void _handleMouseDragSelectionEnd(DragEndDetails details) {
            this._requestKeyboard();
        }

        void _handleSelectionChanged(TextSelection selection, SelectionChangedCause cause) {
            if (cause == SelectionChangedCause.longPress) {
                this._editableTextKey.currentState?.bringIntoView(selection.basePos);
            }
        }

        protected override bool wantKeepAlive {
            get { return this._controller?.text?.isNotEmpty() == true; }
        }

        bool _shouldShowAttachment(
            OverlayVisibilityMode attachment,
            bool hasText
        ) {
            switch (attachment) {
                case OverlayVisibilityMode.never:
                    return false;
                case OverlayVisibilityMode.always:
                    return true;
                case OverlayVisibilityMode.editing:
                    return hasText;
                case OverlayVisibilityMode.notEditing:
                    return !hasText;
            }

            D.assert(false);
            return false;
        }

        bool _showPrefixWidget(TextEditingValue text) {
            return this.widget.prefix != null && this._shouldShowAttachment(
                       attachment: this.widget.prefixMode,
                       hasText: text.text.isNotEmpty()
                   );
        }

        bool _showSuffixWidget(TextEditingValue text) {
            return this.widget.suffix != null && this._shouldShowAttachment(
                       attachment: this.widget.suffixMode,
                       hasText: text.text.isNotEmpty()
                   );
        }

        bool _showClearButton(TextEditingValue text) {
            return this._shouldShowAttachment(
                attachment: this.widget.clearButtonMode,
                hasText: text.text.isNotEmpty()
            );
        }

        Widget _addTextDependentAttachments(Widget editableText, TextStyle textStyle, TextStyle placeholderStyle) {
            D.assert(editableText != null);
            D.assert(textStyle != null);
            D.assert(placeholderStyle != null);

            if (this.widget.placeholder == null &&
                this.widget.clearButtonMode == OverlayVisibilityMode.never &&
                this.widget.prefix == null &&
                this.widget.suffix == null) {
                return editableText;
            }

            return new ValueListenableBuilder<TextEditingValue>(
                valueListenable: this._effectiveController,
                child: editableText,
                builder: (BuildContext context, TextEditingValue text, Widget child) => {
                    List<Widget> rowChildren = new List<Widget>();

                    if (this._showPrefixWidget(text)) {
                        rowChildren.Add(this.widget.prefix);
                    }

                    List<Widget> stackChildren = new List<Widget>();
                    if (this.widget.placeholder != null && text.text.isEmpty()) {
                        stackChildren.Add(
                            new Padding(
                                padding: this.widget.padding,
                                child: new Text(
                                    this.widget.placeholder,
                                    maxLines: 1,
                                    overflow: TextOverflow.ellipsis,
                                    style: placeholderStyle
                                )
                            )
                        );
                    }

                    stackChildren.Add(child);
                    rowChildren.Add(
                        new Expanded
                        (child: new Stack
                            (children: stackChildren)));

                    if (this._showSuffixWidget(text)) {
                        rowChildren.Add(this.widget.suffix);
                    }
                    else if (this._showClearButton(text)) {
                        rowChildren.Add(
                            new GestureDetector(
                                onTap: this.widget.enabled ?? true
                                    ? () => {
                                        bool textChanged = this._effectiveController.text.isNotEmpty();
                                        this._effectiveController.clear();
                                        if (this.widget.onChanged != null && textChanged) {
                                            this.widget.onChanged(this._effectiveController.text);
                                        }
                                    }
                                    : (GestureTapCallback) null,
                                child: new Padding(
                                    padding: EdgeInsets.symmetric(horizontal: 6.0f),
                                    child: new Icon(
                                        CupertinoIcons.clear_thick_circled,
                                        size: 18.0f,
                                        color: CupertinoTextFieldUtils._kInactiveTextColor
                                    )
                                )
                            )
                        );
                    }

                    return new Row(children: rowChildren);
                }
            );
        }

        public override Widget build(BuildContext context) {
            base.build(context);
            TextEditingController controller = this._effectiveController;
            List<TextInputFormatter> formatters = this.widget.inputFormatters ?? new List<TextInputFormatter>();
            bool enabled = this.widget.enabled ?? true;
            Offset cursorOffset =
                new Offset(
                    CupertinoTextFieldUtils._iOSHorizontalCursorOffsetPixels / MediaQuery.of(context).devicePixelRatio,
                    0);
            if (this.widget.maxLength != null && this.widget.maxLengthEnforced) {
                formatters.Add(new LengthLimitingTextInputFormatter(this.widget.maxLength));
            }

            CupertinoThemeData themeData = CupertinoTheme.of(context);
            TextStyle textStyle = themeData.textTheme.textStyle.merge(this.widget.style);
            TextStyle placeholderStyle = textStyle.merge(this.widget.placeholderStyle);
            Brightness keyboardAppearance = this.widget.keyboardAppearance ?? themeData.brightness;
            Color cursorColor = this.widget.cursorColor ?? themeData.primaryColor;

            Widget paddedEditable = new Padding(
                padding: this.widget.padding,
                child: new RepaintBoundary(
                    child: new EditableText(
                        key: this._editableTextKey,
                        controller: controller,
                        focusNode: this._effectiveFocusNode,
                        keyboardType: this.widget.keyboardType,
                        textInputAction: this.widget.textInputAction,
                        textCapitalization: this.widget.textCapitalization,
                        style: textStyle,
                        strutStyle: this.widget.strutStyle,
                        textAlign: this.widget.textAlign,
                        autofocus: this.widget.autofocus,
                        obscureText: this.widget.obscureText,
                        autocorrect: this.widget.autocorrect,
                        maxLines: this.widget.maxLines,
                        minLines: this.widget.minLines,
                        expands: this.widget.expands,
                        selectionColor: CupertinoTextFieldUtils._kSelectionHighlightColor,
                        selectionControls: CupertinoTextSelectionUtils.cupertinoTextSelectionControls,
                        onChanged: this.widget.onChanged,
                        onSelectionChanged: this._handleSelectionChanged,
                        onEditingComplete: this.widget.onEditingComplete,
                        onSubmitted: this.widget.onSubmitted,
                        inputFormatters: formatters,
                        rendererIgnoresPointer: true,
                        cursorWidth: this.widget.cursorWidth,
                        cursorRadius: this.widget.cursorRadius,
                        cursorColor: cursorColor,
                        cursorOpacityAnimates: true,
                        cursorOffset: cursorOffset,
                        paintCursorAboveText: true,
                        backgroundCursorColor: CupertinoColors.inactiveGray,
                        scrollPadding: this.widget.scrollPadding,
                        keyboardAppearance: keyboardAppearance,
                        dragStartBehavior: this.widget.dragStartBehavior,
                        scrollPhysics: this.widget.scrollPhysics
                    )
                )
            );

            return new IgnorePointer(
                ignoring: !enabled,
                child: new Container(
                    decoration: this.widget.decoration,
                    child: new Container(
                        color: enabled
                            ? null
                            : CupertinoTheme.of(context).brightness == Brightness.light
                                ? CupertinoTextFieldUtils._kDisabledBackground
                                : CupertinoColors.darkBackgroundGray,
                        child: new TextSelectionGestureDetector(
                            onTapDown: this._handleTapDown,
                            onSingleTapUp: this._handleSingleTapUp,
                            onSingleLongTapStart: this._handleSingleLongTapStart,
                            onSingleLongTapMoveUpdate: this._handleSingleLongTapMoveUpdate,
                            onSingleLongTapEnd: this._handleSingleLongTapEnd,
                            onDoubleTapDown: this._handleDoubleTapDown,
                            onDragSelectionStart: this._handleMouseDragSelectionStart,
                            onDragSelectionUpdate: this._handleMouseDragSelectionUpdate,
                            onDragSelectionEnd: this._handleMouseDragSelectionEnd,
                            behavior: HitTestBehavior.translucent,
                            child: this._addTextDependentAttachments(paddedEditable, textStyle, placeholderStyle)
                        )
                    )
                )
            );
        }
    }
}