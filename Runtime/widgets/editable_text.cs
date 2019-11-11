using System;
using System.Collections.Generic;
using RSG;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    public delegate void SelectionChangedCallback(TextSelection selection, SelectionChangedCause cause);

    public class TextEditingController : ValueNotifier<TextEditingValue> {
        public TextEditingController(string text = null) : base(text == null
            ? TextEditingValue.empty
            : new TextEditingValue(text)) {
        }

        TextEditingController(TextEditingValue value) : base(value ?? TextEditingValue.empty) {
        }

        public static TextEditingController fromValue(TextEditingValue value) {
            return new TextEditingController(value);
        }

        public string text {
            get { return this.value.text; }

            set {
                this.value = this.value.copyWith(
                    text: value,
                    selection: TextSelection.collapsed(-1),
                    composing: TextRange.empty);
            }
        }

        public TextSelection selection {
            get { return this.value.selection; }

            set {
                if (value.start > this.text.Length || value.end > this.text.Length) {
                    throw new UIWidgetsError($"invalid text selection: {value}");
                }

                this.value = this.value.copyWith(selection: value, composing: TextRange.empty);
            }
        }

        public void clear() {
            this.value = TextEditingValue.empty;
        }

        public void clearComposing() {
            this.value = this.value.copyWith(composing: TextRange.empty);
        }
    }

    public class EditableText : StatefulWidget {
        public EditableText(
            TextEditingController controller = null,
            FocusNode focusNode = null, TextStyle style = null,
            StrutStyle strutStyle = null,
            Color cursorColor = null,
            Color backgroundCursorColor = null,
            bool obscureText = false,
            bool autocorrect = false,
            TextAlign textAlign = TextAlign.left,
            TextDirection? textDirection = null,
            float? textScaleFactor = null,
            int? maxLines = 1,
            int? minLines = null,
            bool expands = false,
            bool autofocus = false,
            Color selectionColor = null,
            TextSelectionControls selectionControls = null,
            TextInputType keyboardType = null,
            TextInputAction? textInputAction = null,
            TextCapitalization textCapitalization = TextCapitalization.none,
            ValueChanged<string> onChanged = null,
            VoidCallback onEditingComplete = null,
            ValueChanged<string> onSubmitted = null,
            SelectionChangedCallback onSelectionChanged = null,
            List<TextInputFormatter> inputFormatters = null,
            bool rendererIgnoresPointer = false,
            EdgeInsets scrollPadding = null,
            bool unityTouchKeyboard = false,
            Key key = null,
            float? cursorWidth = 2.0f,
            Radius cursorRadius = null,
            bool cursorOpacityAnimates = false,
            Offset cursorOffset = null,
            bool paintCursorAboveText = false,
            Brightness? keyboardAppearance = Brightness.light,
            DragStartBehavior dragStartBehavior = DragStartBehavior.down,
            bool? enableInteractiveSelection = null,
            ScrollPhysics scrollPhysics = null,
            GlobalKeyEventHandlerDelegate globalKeyEventHandler = null
        ) : base(key) {
            D.assert(controller != null);
            D.assert(focusNode != null);
            D.assert(style != null);
            D.assert(cursorColor != null);
            D.assert(maxLines == null || maxLines > 0);
            D.assert(minLines == null || minLines > 0);
            D.assert((maxLines == null) || (minLines == null) || (maxLines >= minLines),
                () => "minLines can't be greater than maxLines");
            D.assert(!expands || (maxLines == null && minLines == null),
                () => "minLines and maxLines must be null when expands is true.");
            // D.assert(backgroundCursorColor != null); // TODO: remove comment
            this.keyboardType = keyboardType ?? (maxLines == 1 ? TextInputType.text : TextInputType.multiline);

            this.scrollPadding = scrollPadding ?? EdgeInsets.all(20.0f);
            this.controller = controller;
            this.focusNode = focusNode;
            this.obscureText = obscureText;
            this.autocorrect = autocorrect;
            this.style = style;
            this._strutStyle = strutStyle;
            this.textAlign = textAlign;
            this.textDirection = textDirection;
            this.textScaleFactor = textScaleFactor;
            this.textInputAction = textInputAction;
            this.textCapitalization = textCapitalization;
            this.cursorColor = cursorColor;
            this.backgroundCursorColor = backgroundCursorColor ?? Colors.grey; // TODO: remove ??
            this.maxLines = maxLines;
            this.minLines = minLines;
            this.expands = expands;
            this.autofocus = autofocus;
            this.selectionColor = selectionColor;
            this.onChanged = onChanged;
            this.onSubmitted = onSubmitted;
            this.onSelectionChanged = onSelectionChanged;
            this.onEditingComplete = onEditingComplete;
            this.rendererIgnoresPointer = rendererIgnoresPointer;
            this.selectionControls = selectionControls;
            this.unityTouchKeyboard = unityTouchKeyboard;
            if (maxLines == 1) {
                this.inputFormatters = new List<TextInputFormatter>();
                this.inputFormatters.Add(BlacklistingTextInputFormatter.singleLineFormatter);
                if (inputFormatters != null) {
                    this.inputFormatters.AddRange(inputFormatters);
                }
            }
            else {
                this.inputFormatters = inputFormatters;
            }

            this.cursorWidth = cursorWidth;
            this.cursorRadius = cursorRadius;
            this.cursorOpacityAnimates = cursorOpacityAnimates;
            this.cursorOffset = cursorOffset;
            this.paintCursorAboveText = paintCursorAboveText;
            this.keyboardAppearance = keyboardAppearance;
            this.enableInteractiveSelection = enableInteractiveSelection;
            this.dragStartBehavior = dragStartBehavior;
            this.scrollPhysics = scrollPhysics;
            this.globalKeyEventHandler = globalKeyEventHandler;
        }

        public readonly TextEditingController controller;
        public readonly FocusNode focusNode;
        public readonly bool obscureText;
        public readonly bool autocorrect;
        public readonly TextStyle style;
        public StrutStyle strutStyle {
            get {
                if (this._strutStyle == null) {
                    return this.style != null
                        ? StrutStyle.fromTextStyle(this.style, forceStrutHeight: true)
                        : StrutStyle.disabled;
                }

                return this._strutStyle.inheritFromTextStyle(this.style);
            }
        }

        readonly StrutStyle _strutStyle;
        public readonly TextAlign textAlign;
        public readonly TextDirection? textDirection;
        public readonly TextCapitalization textCapitalization;
        public readonly float? textScaleFactor;
        public readonly Color cursorColor;
        public readonly Color backgroundCursorColor;
        public readonly int? maxLines;
        public readonly int? minLines;
        public readonly bool expands;
        public readonly bool autofocus;
        public readonly Color selectionColor;
        public readonly TextSelectionControls selectionControls;
        public readonly TextInputType keyboardType;
        public readonly TextInputAction? textInputAction;
        public readonly ValueChanged<string> onChanged;
        public readonly VoidCallback onEditingComplete;
        public readonly ValueChanged<string> onSubmitted;
        public readonly SelectionChangedCallback onSelectionChanged;
        public readonly List<TextInputFormatter> inputFormatters;
        public readonly bool rendererIgnoresPointer;
        public readonly bool unityTouchKeyboard;
        public readonly float? cursorWidth;
        public readonly Radius cursorRadius;
        public readonly bool cursorOpacityAnimates;
        public readonly Offset cursorOffset;
        public readonly bool paintCursorAboveText;
        public readonly Brightness? keyboardAppearance;
        public readonly EdgeInsets scrollPadding;
        public readonly DragStartBehavior dragStartBehavior;
        public readonly bool? enableInteractiveSelection;
        public readonly ScrollPhysics scrollPhysics;
        public readonly GlobalKeyEventHandlerDelegate globalKeyEventHandler;

        public bool selectionEnabled {
            get { return this.enableInteractiveSelection ?? !this.obscureText; }
        }

        public override State createState() {
            return new EditableTextState();
        }

        public static bool debugDeterministicCursor = false;

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<TextEditingController>("controller", this.controller));
            properties.add(new DiagnosticsProperty<FocusNode>("focusNode", this.focusNode));
            properties.add(new DiagnosticsProperty<bool>("obscureText", this.obscureText, defaultValue: false));
            properties.add(new DiagnosticsProperty<bool>("autocorrect", this.autocorrect, defaultValue: true));
            if (this.style != null) {
                this.style.debugFillProperties(properties);
            }

            properties.add(new EnumProperty<TextAlign>("textAlign", this.textAlign,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new EnumProperty<TextDirection?>("textDirection", this.textDirection,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<float?>("textScaleFactor", this.textScaleFactor,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<int?>("maxLines", this.maxLines, defaultValue: 1));
            properties.add(new DiagnosticsProperty<int?>("minLines", this.minLines,
                defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<bool>("expands", this.expands, defaultValue: false));
            properties.add(new DiagnosticsProperty<bool>("autofocus", this.autofocus, defaultValue: false));
            properties.add(
                new DiagnosticsProperty<TextInputType>("keyboardType", this.keyboardType, defaultValue: null));
            properties.add(new DiagnosticsProperty<ScrollPhysics>("scrollPhysics", this.scrollPhysics,
                defaultValue: Diagnostics.kNullDefaultValue));
        }
    }

    public class EditableTextState : AutomaticKeepAliveClientWithTickerProviderStateMixin<EditableText>,
        WidgetsBindingObserver, TextInputClient,
        TextSelectionDelegate {
        const int _kObscureShowLatestCharCursorTicks = 3;
        static TimeSpan _kCursorBlinkHalfPeriod = TimeSpan.FromMilliseconds(500);
        static TimeSpan _kCursorBlinkWaitForStart = TimeSpan.FromMilliseconds(150);

        Timer _cursorTimer;
        bool _targetCursorVisibility = false;
        ValueNotifier<bool> _cursorVisibilityNotifier = new ValueNotifier<bool>(false);
        GlobalKey _editableKey = GlobalKey.key();

        TextInputConnection _textInputConnection;
        TextSelectionOverlay _selectionOverlay;

        public ScrollController _scrollController = new ScrollController();
        AnimationController _cursorBlinkOpacityController;

        LayerLink _layerLink = new LayerLink();
        bool _didAutoFocus = false;

        static readonly TimeSpan _fadeDuration = TimeSpan.FromMilliseconds(250);

        static readonly TimeSpan _floatingCursorResetTime = TimeSpan.FromMilliseconds(125);

        AnimationController _floatingCursorResetController;

        protected override bool wantKeepAlive {
            get { return this.widget.focusNode.hasFocus; }
        }


        Color _cursorColor {
            get { return this.widget.cursorColor.withOpacity(this._cursorBlinkOpacityController.value); }
        }

        public override void initState() {
            base.initState();
            this.widget.controller.addListener(this._didChangeTextEditingValue);
            this.widget.focusNode.addListener(this._handleFocusChanged);
            this._scrollController.addListener(() => { this._selectionOverlay?.updateForScroll(); });
            this._cursorBlinkOpacityController = new AnimationController(vsync: this, duration: _fadeDuration);
            this._cursorBlinkOpacityController.addListener(this._onCursorColorTick);
            this._floatingCursorResetController = new AnimationController(vsync: this);
            this._floatingCursorResetController.addListener(this._onFloatingCursorResetTick);
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            if (!this._didAutoFocus && this.widget.autofocus) {
                FocusScope.of(this.context).autofocus(this.widget.focusNode);
                this._didAutoFocus = true;
            }
        }

        public override void didUpdateWidget(StatefulWidget old) {
            EditableText oldWidget = (EditableText) old;
            base.didUpdateWidget(oldWidget);
            if (this.widget.controller != oldWidget.controller) {
                oldWidget.controller.removeListener(this._didChangeTextEditingValue);
                this.widget.controller.addListener(this._didChangeTextEditingValue);
                this._updateRemoteEditingValueIfNeeded();
                this._updateImePosIfNeed();
            }

            if (this.widget.focusNode != oldWidget.focusNode) {
                oldWidget.focusNode.removeListener(this._handleFocusChanged);
                this.widget.focusNode.addListener(this._handleFocusChanged);
                this.updateKeepAlive();
            }
        }

        public override void dispose() {
            this.widget.controller.removeListener(this._didChangeTextEditingValue);
            this._cursorBlinkOpacityController.removeListener(this._onCursorColorTick);
            this._floatingCursorResetController.removeListener(this._onFloatingCursorResetTick);
            this._closeInputConnectionIfNeeded();
            D.assert(!this._hasInputConnection);
            this._stopCursorTimer();
            D.assert(this._cursorTimer == null);
            this._selectionOverlay?.dispose();
            this._selectionOverlay = null;
            this.widget.focusNode.removeListener(this._handleFocusChanged);
            base.dispose();
        }

        TextEditingValue _lastKnownRemoteTextEditingValue;

        public void updateEditingValue(TextEditingValue value, bool isIMEInput) {
            if (value.text != this._value.text) {
                this._hideSelectionOverlayIfNeeded();
                this._showCaretOnScreen();
                if (this.widget.obscureText && value.text.Length == this._value.text.Length + 1) {
                    this._obscureShowCharTicksPending = !this._unityKeyboard() ? _kObscureShowLatestCharCursorTicks : 0;
                    this._obscureLatestCharIndex = this._value.selection.baseOffset;
                }
            }

            this._lastKnownRemoteTextEditingValue = value;
            this._formatAndSetValue(value, isIMEInput);

            this._stopCursorTimer(resetCharTicks: false);
            this._startCursorTimer();
        }

        public void performAction(TextInputAction action) {
            switch (action) {
                case TextInputAction.newline:
                    if (!this._isMultiline) {
                        this._finalizeEditing(true);
                    }

                    break;
                case TextInputAction.done:
                case TextInputAction.go:
                case TextInputAction.send:
                case TextInputAction.search:
                    this._finalizeEditing(true);
                    break;
                default:
                    this._finalizeEditing(false);
                    break;
            }
        }

        Rect _startCaretRect;

        TextPosition _lastTextPosition;

        Offset _pointOffsetOrigin;

        Offset _lastBoundedOffset;

        Offset _floatingCursorOffset {
            get { return new Offset(0, this.renderEditable.preferredLineHeight / 2); }
        }

        public void updateFloatingCursor(RawFloatingCursorPoint point) {
            switch (point.state) {
                case FloatingCursorDragState.Start:
                    if (this._floatingCursorResetController.isAnimating) {
                        this._floatingCursorResetController.stop();
                        this._onFloatingCursorResetTick();
                    }
                    TextPosition currentTextPosition =
                        new TextPosition(offset: this.renderEditable.selection.baseOffset);
                    this._startCaretRect = this.renderEditable.getLocalRectForCaret(currentTextPosition);
                    this.renderEditable.setFloatingCursor(point.state,
                        this._startCaretRect.center - this._floatingCursorOffset, currentTextPosition);
                    break;
                case FloatingCursorDragState.Update:
                    // We want to send in points that are centered around a (0,0) origin, so we cache the
                    // position on the first update call.
                    if (this._pointOffsetOrigin != null) {
                        Offset centeredPoint = point.offset - this._pointOffsetOrigin;
                        Offset rawCursorOffset =
                            this._startCaretRect.center + centeredPoint - this._floatingCursorOffset;
                        this._lastBoundedOffset =
                            this.renderEditable.calculateBoundedFloatingCursorOffset(rawCursorOffset);
                        this._lastTextPosition = this.renderEditable.getPositionForPoint(
                            this.renderEditable.localToGlobal(this._lastBoundedOffset + this._floatingCursorOffset));
                        this.renderEditable.setFloatingCursor(point.state, this._lastBoundedOffset,
                            this._lastTextPosition);
                    }
                    else {
                        this._pointOffsetOrigin = point.offset;
                    }

                    break;
                case FloatingCursorDragState.End:
                    this._floatingCursorResetController.setValue(0.0f);
                    this._floatingCursorResetController.animateTo(1.0f, duration: _floatingCursorResetTime,
                        curve: Curves.decelerate);
                    break;
            }
        }

        public RawInputKeyResponse globalInputKeyHandler(RawKeyEvent evt) {
            return this.widget.globalKeyEventHandler?.Invoke(evt, true) ?? RawInputKeyResponse.convert(evt);
        }

        void _onFloatingCursorResetTick() {
            Offset finalPosition = this.renderEditable.getLocalRectForCaret(this._lastTextPosition).centerLeft -
                                   this._floatingCursorOffset;
            if (this._floatingCursorResetController.isCompleted) {
                this.renderEditable.setFloatingCursor(FloatingCursorDragState.End, finalPosition,
                    this._lastTextPosition);
                if (this._lastTextPosition.offset != this.renderEditable.selection.baseOffset) {
                    this._handleSelectionChanged(TextSelection.collapsed(offset: this._lastTextPosition.offset),
                        this.renderEditable, SelectionChangedCause.forcePress);
                }

                this._startCaretRect = null;
                this._lastTextPosition = null;
                this._pointOffsetOrigin = null;
                this._lastBoundedOffset = null;
            }
            else {
                float lerpValue = this._floatingCursorResetController.value;
                float lerpX = MathUtils.lerpFloat(this._lastBoundedOffset.dx, finalPosition.dx, lerpValue);
                float lerpY = MathUtils.lerpFloat(this._lastBoundedOffset.dy, finalPosition.dy, lerpValue);

                this.renderEditable.setFloatingCursor(FloatingCursorDragState.Update, new Offset(lerpX, lerpY),
                    this._lastTextPosition, resetLerpValue: lerpValue);
            }
        }

        void _finalizeEditing(bool shouldUnfocus) {
            if (this.widget.onEditingComplete != null) {
                this.widget.onEditingComplete();
            }
            else {
                this.widget.controller.clearComposing();
                if (shouldUnfocus) {
                    this.widget.focusNode.unfocus();
                }
            }

            if (this.widget.onSubmitted != null) {
                this.widget.onSubmitted(this._value.text);
            }
        }

        void _updateRemoteEditingValueIfNeeded() {
            if (!this._hasInputConnection) {
                return;
            }

            var localValue = this._value;
            if (localValue == this._lastKnownRemoteTextEditingValue) {
                return;
            }

            this._lastKnownRemoteTextEditingValue = localValue;
            this._textInputConnection.setEditingState(localValue);
        }

        TextEditingValue _value {
            get { return this.widget.controller.value; }
            set { this.widget.controller.value = value; }
        }

        bool _hasFocus {
            get { return this.widget.focusNode.hasFocus; }
        }

        bool _isMultiline {
            get { return this.widget.maxLines != 1; }
        }

        // Calculate the new scroll offset so the cursor remains visible.
        float _getScrollOffsetForCaret(Rect caretRect) {
            float caretStart;
            float caretEnd;
            if (this._isMultiline) {
                // The caret is vertically centered within the line. Expand the caret's
                // height so that it spans the line because we're going to ensure that the entire
                // expanded caret is scrolled into view.
                float lineHeight = this.renderEditable.preferredLineHeight;
                float caretOffset = (lineHeight - caretRect.height) / 2;
                caretStart = caretRect.top - caretOffset;
                caretEnd = caretRect.bottom + caretOffset;
            }
            else {
                caretStart = caretRect.left;
                caretEnd = caretRect.right;
            }

            float scrollOffset = this._scrollController.offset;
            float viewportExtent = this._scrollController.position.viewportDimension;
            if (caretStart < 0.0) {
                scrollOffset += caretStart;
            }
            else if (caretEnd >= viewportExtent) {
                scrollOffset += caretEnd - viewportExtent;
            }

            return scrollOffset;
        }

        // Calculates where the `caretRect` would be if `_scrollController.offset` is set to `scrollOffset`.
        Rect _getCaretRectAtScrollOffset(Rect caretRect, float scrollOffset) {
            float offsetDiff = this._scrollController.offset - scrollOffset;
            return this._isMultiline ? caretRect.translate(0.0f, offsetDiff) : caretRect.translate(offsetDiff, 0.0f);
        }

        bool _hasInputConnection {
            get { return this._textInputConnection != null && this._textInputConnection.attached; }
        }

        void _openInputConnection() {
            if (!this._hasInputConnection) {
                TextEditingValue localValue = this._value;
                this._lastKnownRemoteTextEditingValue = localValue;
                this._textInputConnection = TextInput.attach(this, new TextInputConfiguration(
                    inputType: this.widget.keyboardType,
                    obscureText: this.widget.obscureText,
                    autocorrect: this.widget.autocorrect,
                    inputAction: this.widget.textInputAction ?? ((this.widget.keyboardType == TextInputType.multiline)
                                     ? TextInputAction.newline
                                     : TextInputAction.done),
                    textCapitalization: this.widget.textCapitalization,
                    keyboardAppearance: this.widget.keyboardAppearance ?? Brightness.light,
                    unityTouchKeyboard: this.widget.unityTouchKeyboard
                ));
                this._textInputConnection.setEditingState(localValue);
                this._updateImePosIfNeed();
            }

            this._textInputConnection.show();
        }

        void _closeInputConnectionIfNeeded() {
            if (this._hasInputConnection) {
                this._textInputConnection.close();
                this._textInputConnection = null;
                this._lastKnownRemoteTextEditingValue = null;
            }
        }

        void _openOrCloseInputConnectionIfNeeded() {
            if (this._hasFocus && this.widget.focusNode.consumeKeyboardToken()) {
                this._openInputConnection();
            }
            else if (!this._hasFocus) {
                this._closeInputConnectionIfNeeded();
                this.widget.controller.clearComposing();
            }
        }

        public void requestKeyboard() {
            if (this._hasFocus) {
                this._openInputConnection();
            }
            else {
                List<FocusScopeNode> ancestorScopes = FocusScope.ancestorsOf(this.context);
                for (int i = ancestorScopes.Count - 1; i >= 1; i -= 1) {
                    ancestorScopes[i].setFirstFocus(ancestorScopes[i - 1]);
                }

                FocusScope.of(this.context).requestFocus(this.widget.focusNode);
            }
        }

        void _hideSelectionOverlayIfNeeded() {
            this._selectionOverlay?.hide();
            this._selectionOverlay = null;
        }

        void _updateOrDisposeSelectionOverlayIfNeeded() {
            if (this._selectionOverlay != null) {
                if (this._hasFocus) {
                    this._selectionOverlay.update(this._value);
                }
                else {
                    this._selectionOverlay.dispose();
                    this._selectionOverlay = null;
                }
            }
        }


        void _handleSelectionChanged(TextSelection selection, RenderEditable renderObject,
            SelectionChangedCause cause) {
            this.widget.controller.selection = selection;
            this.requestKeyboard();

            this._hideSelectionOverlayIfNeeded();

            if (this.widget.selectionControls != null && Application.isMobilePlatform && !this._unityKeyboard()) {
                this._selectionOverlay = new TextSelectionOverlay(
                    context: this.context,
                    value: this._value,
                    debugRequiredFor: this.widget,
                    layerLink: this._layerLink,
                    renderObject: renderObject,
                    selectionControls: this.widget.selectionControls,
                    selectionDelegate: this,
                    dragStartBehavior: this.widget.dragStartBehavior
                );
                bool longPress = cause == SelectionChangedCause.longPress;
                if (cause != SelectionChangedCause.keyboard && (this._value.text.isNotEmpty() || longPress)) {
                    this._selectionOverlay.showHandles();
                }

                if (longPress || cause == SelectionChangedCause.doubleTap) {
                    this._selectionOverlay.showToolbar();
                }
            }

            if (this.widget.onSelectionChanged != null) {
                this.widget.onSelectionChanged(selection, cause);
            }
        }

        bool _textChangedSinceLastCaretUpdate = false;
        Rect _currentCaretRect;

        void _handleCaretChanged(Rect caretRect) {
            this._currentCaretRect = caretRect;
            // If the caret location has changed due to an update to the text or
            // selection, then scroll the caret into view.
            if (this._textChangedSinceLastCaretUpdate) {
                this._textChangedSinceLastCaretUpdate = false;
                this._showCaretOnScreen();
            }
        }

        // Animation configuration for scrolling the caret back on screen.
        static readonly TimeSpan _caretAnimationDuration = TimeSpan.FromMilliseconds(100);
        static readonly Curve _caretAnimationCurve = Curves.fastOutSlowIn;
        bool _showCaretOnScreenScheduled = false;

        void _showCaretOnScreen() {
            if (this._showCaretOnScreenScheduled) {
                return;
            }

            this._showCaretOnScreenScheduled = true;
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                this._showCaretOnScreenScheduled = false;
                if (this._currentCaretRect == null || !this._scrollController.hasClients) {
                    return;
                }

                float scrollOffsetForCaret = this._getScrollOffsetForCaret(this._currentCaretRect);
                this._scrollController.animateTo(scrollOffsetForCaret,
                    duration: _caretAnimationDuration,
                    curve: _caretAnimationCurve);

                Rect newCaretRect = this._getCaretRectAtScrollOffset(this._currentCaretRect, scrollOffsetForCaret);
                // Enlarge newCaretRect by scrollPadding to ensure that caret is not positioned directly at the edge after scrolling.
                Rect inflatedRect = Rect.fromLTRB(
                    newCaretRect.left - this.widget.scrollPadding.left,
                    newCaretRect.top - this.widget.scrollPadding.top,
                    newCaretRect.right + this.widget.scrollPadding.right,
                    newCaretRect.bottom + this.widget.scrollPadding.bottom
                );
                this._editableKey.currentContext.findRenderObject().showOnScreen(
                    rect: inflatedRect,
                    duration: _caretAnimationDuration,
                    curve: _caretAnimationCurve
                );
            });
        }


        double _lastBottomViewInset;

        public void didChangeMetrics() {
            if (this._lastBottomViewInset < Window.instance.viewInsets.bottom) {
                this._showCaretOnScreen();
            }

            this._lastBottomViewInset = Window.instance.viewInsets.bottom;
        }

        public void didChangeTextScaleFactor() {
        }

        public void didChangePlatformBrightness() {
        }

        public void didChangeLocales(List<Locale> locale) {
        }

        public IPromise<bool> didPopRoute() {
            return Promise<bool>.Resolved(false);
        }

        public IPromise<bool> didPushRoute(string route) {
            return Promise<bool>.Resolved(false);
        }

        void _formatAndSetValue(TextEditingValue value, bool isIMEInput = false) {
            var textChanged = this._value?.text != value?.text || isIMEInput;
            if (textChanged && this.widget.inputFormatters != null && this.widget.inputFormatters.isNotEmpty()) {
                foreach (var formatter in this.widget.inputFormatters) {
                    value = formatter.formatEditUpdate(this._value, value);
                }

                this._value = value;
                this._updateRemoteEditingValueIfNeeded();
                this._updateImePosIfNeed();
            }
            else {
                this._value = value;
            }

            if (textChanged && this.widget.onChanged != null) {
                this.widget.onChanged(value.text);
            }
        }

        void _onCursorColorTick() {
            this.renderEditable.cursorColor =
                this.widget.cursorColor.withOpacity(this._cursorBlinkOpacityController.value);
            this._cursorVisibilityNotifier.value = this._cursorBlinkOpacityController.value > 0;
        }

        public bool cursorCurrentlyVisible {
            get { return this._cursorBlinkOpacityController.value > 0; }
        }

        public TimeSpan cursorBlinkInterval {
            get { return _kCursorBlinkHalfPeriod; }
        }

        public TextSelectionOverlay selectionOverlay {
            get { return this._selectionOverlay; }
        }

        int _obscureShowCharTicksPending = 0;
        int _obscureLatestCharIndex;

        void _cursorTick() {
            this._targetCursorVisibility = !this._unityKeyboard() && !this._targetCursorVisibility;
            float targetOpacity = this._targetCursorVisibility ? 1.0f : 0.0f;
            if (this.widget.cursorOpacityAnimates) {
                this._cursorBlinkOpacityController.animateTo(targetOpacity, curve: Curves.easeOut);
            }
            else {
                this._cursorBlinkOpacityController.setValue(targetOpacity);
            }

            if (this._obscureShowCharTicksPending > 0) {
                this.setState(() => { this._obscureShowCharTicksPending--; });
            }
        }

        void _cursorWaitForStart() {
            D.assert(_kCursorBlinkHalfPeriod > _fadeDuration);
            this._cursorTimer?.cancel();
            this._cursorTimer = Window.instance.run(_kCursorBlinkHalfPeriod, this._cursorTick, periodic: true);
        }

        void _startCursorTimer() {
            this._targetCursorVisibility = true;
            this._cursorBlinkOpacityController.setValue(1.0f);
            if (EditableText.debugDeterministicCursor) {
                return;
            }

            if (this.widget.cursorOpacityAnimates) {
                this._cursorTimer =
                    Window.instance.run(_kCursorBlinkWaitForStart, this._cursorWaitForStart, periodic: true);
            }
            else {
                this._cursorTimer = Window.instance.run(_kCursorBlinkHalfPeriod, this._cursorTick, periodic: true);
            }
        }

        void _stopCursorTimer(bool resetCharTicks = true) {
            this._cursorTimer?.cancel();
            this._cursorTimer = null;
            this._targetCursorVisibility = false;
            this._cursorBlinkOpacityController.setValue(0.0f);
            if (EditableText.debugDeterministicCursor) {
                return;
            }

            if (resetCharTicks) {
                this._obscureShowCharTicksPending = 0;
            }

            if (this.widget.cursorOpacityAnimates) {
                this._cursorBlinkOpacityController.stop();
                this._cursorBlinkOpacityController.setValue(0.0f);
            }
        }

        void _startOrStopCursorTimerIfNeeded() {
            if (this._cursorTimer == null && this._hasFocus && this._value.selection.isCollapsed) {
                this._startCursorTimer();
            }
            else if (this._cursorTimer != null && (!this._hasFocus || !this._value.selection.isCollapsed)) {
                this._stopCursorTimer();
            }
        }


        void _didChangeTextEditingValue() {
            this._updateRemoteEditingValueIfNeeded();
            this._updateImePosIfNeed();
            this._startOrStopCursorTimerIfNeeded();
            this._updateOrDisposeSelectionOverlayIfNeeded();
            this._textChangedSinceLastCaretUpdate = true;
            this.setState(() => { });
        }

        void _handleFocusChanged() {
            this._openOrCloseInputConnectionIfNeeded();
            this._startOrStopCursorTimerIfNeeded();
            this._updateOrDisposeSelectionOverlayIfNeeded();
            if (this._hasFocus) {
                WidgetsBinding.instance.addObserver(this);
                this._lastBottomViewInset = Window.instance.viewInsets.bottom;
                this._showCaretOnScreen();
                if (!this._value.selection.isValid) {
                    this.widget.controller.selection = TextSelection.collapsed(offset: this._value.text.Length);
                }
            }
            else {
                WidgetsBinding.instance.removeObserver(this);
                this._value = new TextEditingValue(text: this._value.text);
            }

            this.updateKeepAlive();
        }


        TextDirection? _textDirection {
            get {
                TextDirection? result = this.widget.textDirection ?? Directionality.of(this.context);
                D.assert(result != null,
                    () =>
                        $"{this.GetType().FullName} created without a textDirection and with no ambient Directionality.");
                return result;
            }
        }

        public RenderEditable renderEditable {
            get { return (RenderEditable) this._editableKey.currentContext.findRenderObject(); }
        }

        public TextEditingValue textEditingValue {
            get { return this._value; }
            set {
                this._selectionOverlay?.update(value);
                this._formatAndSetValue(value);
            }
        }

        float _devicePixelRatio {
            get { return MediaQuery.of(this.context).devicePixelRatio; }
        }

        public void bringIntoView(TextPosition position) {
            this._scrollController.jumpTo(
                this._getScrollOffsetForCaret(this.renderEditable.getLocalRectForCaret(position)));
        }

        public bool showToolbar() {
            if (this._selectionOverlay == null) {
                return false;
            }

            this._selectionOverlay.showToolbar();
            return true;
        }

        public void hideToolbar() {
            this._selectionOverlay?.hide();
        }

        public override Widget build(BuildContext context) {
            D.assert(WidgetsD.debugCheckHasMediaQuery(context));
            FocusScope.of(context).reparentIfNeeded(this.widget.focusNode);
            base.build(context); // See AutomaticKeepAliveClientMixin.

            return new Scrollable(
                axisDirection: this._isMultiline ? AxisDirection.down : AxisDirection.right,
                controller: this._scrollController,
                physics: this.widget.scrollPhysics,
                dragStartBehavior: this.widget.dragStartBehavior,
                viewportBuilder: (BuildContext _context, ViewportOffset offset) =>
                    new CompositedTransformTarget(
                        link: this._layerLink,
                        child: new _Editable(
                            key: this._editableKey,
                            textSpan: this.buildTextSpan(),
                            value: this._value,
                            cursorColor: this._cursorColor,
                            backgroundCursorColor: this.widget.backgroundCursorColor,
                            showCursor: EditableText.debugDeterministicCursor
                                ? new ValueNotifier<bool>(true)
                                : this._cursorVisibilityNotifier,
                            hasFocus: this._hasFocus,
                            maxLines: this.widget.maxLines,
                            minLines: this.widget.minLines,
                            expands: this.widget.expands,
                            strutStyle: this.widget.strutStyle,
                            selectionColor: this.widget.selectionColor,
                            textScaleFactor: this.widget.textScaleFactor ?? MediaQuery.textScaleFactorOf(context),
                            textAlign: this.widget.textAlign,
                            textDirection: this._textDirection,
                            obscureText: this.widget.obscureText,
                            autocorrect: this.widget.autocorrect,
                            offset: offset,
                            onSelectionChanged: this._handleSelectionChanged,
                            onCaretChanged: this._handleCaretChanged,
                            rendererIgnoresPointer: this.widget.rendererIgnoresPointer,
                            cursorWidth: this.widget.cursorWidth,
                            cursorRadius: this.widget.cursorRadius,
                            cursorOffset: this.widget.cursorOffset,
                            paintCursorAboveText: this.widget.paintCursorAboveText,
                            enableInteractiveSelection: this.widget.enableInteractiveSelection == true,
                            textSelectionDelegate: this,
                            devicePixelRatio: this._devicePixelRatio,
                            globalKeyEventHandler : this.widget.globalKeyEventHandler
                        )
                    )
            );
        }

        public TextSpan buildTextSpan() {
            if (!this.widget.obscureText && this._value.composing.isValid) {
                TextStyle composingStyle = this.widget.style.merge(
                    new TextStyle(decoration: TextDecoration.underline)
                );

                return new TextSpan(
                    style: this.widget.style,
                    children: new List<TextSpan> {
                        new TextSpan(text: this._value.composing.textBefore(this._value.text)),
                        new TextSpan(
                            style: composingStyle,
                            text: this._value.composing.textInside(this._value.text)
                        ),
                        new TextSpan(text: this._value.composing.textAfter(this._value.text)),
                    });
            }

            var text = this._value.text;
            if (this.widget.obscureText) {
                text = new string(RenderEditable.obscuringCharacter, text.Length);
                int o = this._obscureShowCharTicksPending > 0 ? this._obscureLatestCharIndex : -1;
                if (o >= 0 && o < text.Length) {
                    text = text.Substring(0, o) + this._value.text.Substring(o, 1) + text.Substring(o + 1);
                }
            }

            return new TextSpan(style: this.widget.style, text: text);
        }

        // unity keyboard has a preview view with editing function, text selection & cursor function of this widget is disable
        // in the case
        bool _unityKeyboard() {
            return TouchScreenKeyboard.isSupported && this.widget.unityTouchKeyboard;
        }

        Offset _getImePos() {
            if (this._hasInputConnection && this._textInputConnection.imeRequired()) {
                var localPos = this.renderEditable.getLocalRectForCaret(this._value.selection.basePos).bottomLeft;
                return this.renderEditable.localToGlobal(localPos);
            }

            return null;
        }

        bool _imePosUpdateScheduled = false;

        void _updateImePosIfNeed() {
            if (!this._hasInputConnection || !this._textInputConnection.imeRequired()) {
                return;
            }

            if (this._imePosUpdateScheduled) {
                return;
            }

            this._imePosUpdateScheduled = true;
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                this._imePosUpdateScheduled = false;
                if (!this._hasInputConnection) {
                    return;
                }

                this._textInputConnection.setIMEPos(this._getImePos());
            });
        }
    }

    class _Editable : LeafRenderObjectWidget {
        public readonly TextSpan textSpan;
        public readonly TextEditingValue value;
        public readonly Color cursorColor;
        public readonly Color backgroundCursorColor;
        public readonly ValueNotifier<bool> showCursor;
        public readonly bool hasFocus;
        public readonly int? maxLines;
        public readonly int? minLines;
        public readonly bool expands;
        public readonly StrutStyle strutStyle;
        public readonly Color selectionColor;
        public readonly float textScaleFactor;
        public readonly TextAlign textAlign;
        public readonly TextDirection? textDirection;
        public readonly bool obscureText;
        public readonly bool autocorrect;
        public readonly ViewportOffset offset;
        public readonly SelectionChangedHandler onSelectionChanged;
        public readonly CaretChangedHandler onCaretChanged;
        public readonly bool rendererIgnoresPointer;
        public readonly float? cursorWidth;
        public readonly Radius cursorRadius;
        public readonly Offset cursorOffset;
        public readonly bool enableInteractiveSelection;
        public readonly TextSelectionDelegate textSelectionDelegate;
        public readonly bool? paintCursorAboveText;
        public readonly float? devicePixelRatio;
        public readonly GlobalKeyEventHandlerDelegate globalKeyEventHandler;

        public _Editable(TextSpan textSpan = null,
            TextEditingValue value = null,
            Color cursorColor = null,
            Color backgroundCursorColor = null,
            ValueNotifier<bool> showCursor = null,
            bool hasFocus = false,
            int? maxLines = null,
            int? minLines = null,
            bool expands = false,
            StrutStyle strutStyle = null,
            Color selectionColor = null,
            float textScaleFactor = 1.0f,
            TextDirection? textDirection = null,
            bool obscureText = false,
            TextAlign textAlign = TextAlign.left,
            bool autocorrect = false,
            ViewportOffset offset = null,
            SelectionChangedHandler onSelectionChanged = null,
            CaretChangedHandler onCaretChanged = null,
            bool rendererIgnoresPointer = false,
            Key key = null,
            TextSelectionDelegate textSelectionDelegate = null,
            float? cursorWidth = null,
            Radius cursorRadius = null,
            Offset cursorOffset = null,
            bool enableInteractiveSelection = true,
            bool? paintCursorAboveText = null,
            float? devicePixelRatio = null,
            GlobalKeyEventHandlerDelegate globalKeyEventHandler = null) : base(key) {
            this.textSpan = textSpan;
            this.value = value;
            this.cursorColor = cursorColor;
            this.backgroundCursorColor = backgroundCursorColor;
            this.showCursor = showCursor;
            this.hasFocus = hasFocus;
            this.maxLines = maxLines;
            this.minLines = minLines;
            this.expands = expands;
            this.strutStyle = strutStyle;
            this.selectionColor = selectionColor;
            this.textScaleFactor = textScaleFactor;
            this.textAlign = textAlign;
            this.textDirection = textDirection;
            this.obscureText = obscureText;
            this.autocorrect = autocorrect;
            this.offset = offset;
            this.onSelectionChanged = onSelectionChanged;
            this.onCaretChanged = onCaretChanged;
            this.rendererIgnoresPointer = rendererIgnoresPointer;
            this.textSelectionDelegate = textSelectionDelegate;
            this.cursorWidth = cursorWidth;
            this.cursorRadius = cursorRadius;
            this.cursorOffset = cursorOffset;
            this.enableInteractiveSelection = enableInteractiveSelection;
            this.paintCursorAboveText = paintCursorAboveText;
            this.devicePixelRatio = devicePixelRatio;
            this.globalKeyEventHandler = globalKeyEventHandler;
        }

        public override RenderObject createRenderObject(BuildContext context) {
            return new RenderEditable(
                text: this.textSpan,
                textDirection: this.textDirection ?? TextDirection.ltr,
                offset: this.offset,
                showCursor: this.showCursor,
                cursorColor: this.cursorColor,
                backgroundCursorColor: this.backgroundCursorColor,
                hasFocus: this.hasFocus,
                maxLines: this.maxLines,
                minLines: this.minLines,
                expands: this.expands,
                strutStyle: this.strutStyle,
                selectionColor: this.selectionColor,
                textScaleFactor: this.textScaleFactor,
                textAlign: this.textAlign,
                selection: this.value.selection,
                obscureText: this.obscureText,
                onSelectionChanged: this.onSelectionChanged,
                onCaretChanged: this.onCaretChanged,
                ignorePointer: this.rendererIgnoresPointer,
                cursorWidth: this.cursorWidth ?? 1.0f,
                cursorRadius: this.cursorRadius,
                cursorOffset: this.cursorOffset,
                enableInteractiveSelection: this.enableInteractiveSelection,
                textSelectionDelegate: this.textSelectionDelegate,
                paintCursorAboveText: this.paintCursorAboveText == true,
                devicePixelRatio: this.devicePixelRatio ?? 1.0f,
                globalKeyEventHandler : this.globalKeyEventHandler
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            var edit = (RenderEditable) renderObject;
            edit.text = this.textSpan;
            edit.cursorColor = this.cursorColor;
            edit.backgroundCursorColor = this.backgroundCursorColor;
            edit.showCursor = this.showCursor;
            edit.hasFocus = this.hasFocus;
            edit.maxLines = this.maxLines;
            edit.strutStyle = this.strutStyle;
            edit.selectionColor = this.selectionColor;
            edit.textScaleFactor = this.textScaleFactor;
            edit.textAlign = this.textAlign;
            edit.textDirection = this.textDirection;
            edit.selection = this.value.selection;
            edit.offset = this.offset;
            edit.onSelectionChanged = this.onSelectionChanged;
            edit.onCaretChanged = this.onCaretChanged;
            edit.ignorePointer = this.rendererIgnoresPointer;
            edit.obscureText = this.obscureText;
            edit.textSelectionDelegate = this.textSelectionDelegate;
            edit.cursorWidth = this.cursorWidth ?? 1.0f;
            edit.cursorRadius = this.cursorRadius;
            edit.cursorOffset = this.cursorOffset;
            edit.enableInteractiveSelection = this.enableInteractiveSelection;
            edit.paintCursorAboveText = this.paintCursorAboveText == true;
            edit.devicePixelRatio = this.devicePixelRatio ?? 1.0f;
            edit.globalKeyEventHandler = this.globalKeyEventHandler;
        }
    }
}