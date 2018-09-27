using System;
using System.Collections.Generic;
using RSG;
using UIWidgets.animation;
using UIWidgets.foundation;
using UIWidgets.gestures;
using UIWidgets.painting;
using UIWidgets.rendering;
using UIWidgets.service;
using UIWidgets.ui;
using UnityEngine;
using Color = UIWidgets.ui.Color;
using Rect = UIWidgets.ui.Rect;
using TextStyle = UIWidgets.painting.TextStyle;

namespace UIWidgets.widgets
{
    public delegate void SelectionChangedCallback(TextSelection selection, SelectionChangedCause cause);

    public class TextEditingController : ValueNotifier<TextEditingValue>
    {
        public TextEditingController(string text) : base(text == null
            ? TextEditingValue.empty
            : new TextEditingValue(text))
        {
        }

        private TextEditingController(TextEditingValue value) : base(value ?? TextEditingValue.empty)
        {
        }

        public TextEditingController fromValue(TextEditingValue value)
        {
            return new TextEditingController(value);
        }

        public string text
        {
            get { return value.text; }

            set
            {
                this.value = this.value.copyWith(text: value, selection: TextSelection.collapsed(-1),
                    composing: TextRange.empty);
            }
        }

        public TextSelection selection
        {
            get { return value.selection; }

            set
            {
                if (value.start > text.Length || value.end > text.Length)
                {
                    throw new UIWidgetsError(string.Format("invalid text selection: {0}", value));
                }

                this.value = this.value.copyWith(selection: value, composing: TextRange.empty);
            }
        }

        public void clear()
        {
            value = TextEditingValue.empty;
        }

        public void clearComposing()
        {
            value = value.copyWith(composing: TextRange.empty);
        }
    }

    public class EditableText : StatefulWidget
    {
        public readonly TextEditingController controller;

        public readonly FocusNode focusNode;

        public readonly bool obscureText;

        public readonly bool autocorrect;

        public readonly TextStyle style;

        public readonly TextAlign textAlign;

        public readonly TextDirection? textDirection;

        public readonly double textScaleFactor;

        public readonly Color cursorColor;

        public readonly int maxLines;

        public readonly bool autofocus;

        public readonly Color selectionColor;

        public readonly ValueChanged<string> onChanged;
        public readonly ValueChanged<string> onSubmitted;

        public readonly SelectionChangedCallback onSelectionChanged;

        public readonly List<TextInputFormatter> inputFormatters;

        public readonly bool rendererIgnoresPointer;

        public EditableText(TextEditingController controller, FocusNode focusNode, TextStyle style,
            Color cursorColor, bool obscureText = false, bool autocorrect = false,
            TextAlign textAlign = TextAlign.left, TextDirection? textDirection = null,
            double textScaleFactor = 1.0, int maxLines = 1,
            bool autofocus = false, Color selectionColor = null, ValueChanged<string> onChanged = null,
            ValueChanged<string> onSubmitted = null, SelectionChangedCallback onSelectionChanged = null,
            List<TextInputFormatter> inputFormatters = null, bool rendererIgnoresPointer = false,
            Key key = null) : base(key)
        {
            D.assert(controller != null);
            D.assert(focusNode != null);
            D.assert(style != null);
            D.assert(cursorColor != null);
            this.controller = controller;
            this.focusNode = focusNode;
            this.obscureText = obscureText;
            this.autocorrect = autocorrect;
            this.style = style;
            this.textAlign = textAlign;
            this.textDirection = textDirection;
            this.textScaleFactor = textScaleFactor;
            this.cursorColor = cursorColor;
            this.maxLines = maxLines;
            this.autofocus = autofocus;
            this.selectionColor = selectionColor;
            this.onChanged = onChanged;
            this.onSubmitted = onSubmitted;
            this.onSelectionChanged = onSelectionChanged;
            this.rendererIgnoresPointer = rendererIgnoresPointer;
            if (maxLines == 1)
            {
                this.inputFormatters = new List<TextInputFormatter>();
                this.inputFormatters.Add(BlacklistingTextInputFormatter.singleLineFormatter);
                if (inputFormatters != null)
                {
                    this.inputFormatters.AddRange(inputFormatters);
                }
            }
            else
            {
                this.inputFormatters = inputFormatters;
            }
        }

        public override State createState()
        {
            return new EditableTextState();
        }
        
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<TextEditingController>("controller", controller));
            properties.add(new DiagnosticsProperty<FocusNode>("focusNode", focusNode));
            properties.add(new DiagnosticsProperty<bool>("obscureText", obscureText, defaultValue: false));
            properties.add(new DiagnosticsProperty<bool>("autocorrect", autocorrect, defaultValue: true));
            if (style != null)
            {
                style.debugFillProperties(properties);
            }
            properties.add(new EnumProperty<TextAlign>("textAlign", textAlign, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new EnumProperty<TextDirection?>("textDirection", textDirection, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<double>("textScaleFactor", textScaleFactor, defaultValue: Diagnostics.kNullDefaultValue));
            properties.add(new DiagnosticsProperty<int>("maxLines", maxLines, defaultValue: 1));
            properties.add(new DiagnosticsProperty<bool>("autofocus", autofocus, defaultValue: false));
        }
    }

    public class EditableTextState: State<EditableText>, TextInputClient
    {
        
        const int _kObscureShowLatestCharCursorTicks = 3;
        private ValueNotifier<bool> _showCursor = new ValueNotifier<bool>(true); // todo
        private GlobalKey _editableKey = GlobalKey.key();
        private bool _didAutoFocus = false;
        
        TextInputConnection _textInputConnection;
        private int _obscureShowCharTicksPending = 0;
        private int _obscureLatestCharIndex;
        
        bool _textChangedSinceLastCaretUpdate = false;
        public override void initState()
        {
            base.initState();
            widget.controller.addListener(_didChangeTextEditingValue);
            widget.focusNode.addListener(_handleFocusChanged);
        }

        public override void didChangeDependencies()
        {
            base.didChangeDependencies();
            if (!_didAutoFocus && widget.autofocus)
            {
                FocusScope.of(context).autofocus(widget.focusNode);
                _didAutoFocus = true;
            }
        }

        public override void didUpdateWidget(StatefulWidget old)
        {
            EditableText oldWidget = (EditableText) old;
            base.didUpdateWidget(oldWidget);
            if (widget.controller != oldWidget.controller)
            {
                oldWidget.controller.removeListener(_didChangeTextEditingValue);
                widget.controller.addListener(_didChangeTextEditingValue);
                _updateRemoteEditingValueIfNeeded();
            }
            if (widget.focusNode != oldWidget.focusNode) {
                oldWidget.focusNode.removeListener(_handleFocusChanged);
                widget.focusNode.addListener(_handleFocusChanged);
            }
        }

        public override void dispose()
        {
            widget.controller.removeListener(_didChangeTextEditingValue);
            _closeInputConnectionIfNeeded();
            D.assert(!_hasInputConnection);
            widget.focusNode.removeListener(_handleFocusChanged);
            base.dispose();
        }
        
        TextEditingValue _lastKnownRemoteTextEditingValue;
        
        public void updateEditingValue(TextEditingValue value) {
            if (value.text != _value.text) {
                // _hideSelectionOverlayIfNeeded();
                if (widget.obscureText && value.text.Length == _value.text.Length + 1) {
                    _obscureShowCharTicksPending = _kObscureShowLatestCharCursorTicks;
                    _obscureLatestCharIndex = _value.selection.baseOffset;
                }
            }

            _lastKnownRemoteTextEditingValue = value;
            _formatAndSetValue(value);
        }

        public TextEditingValue getValueForOperation(TextEditOp operation)
        {
            TextPosition newPosition = null;
            switch (operation)
            {
                    case TextEditOp.MoveLeft:
                    return _value.moveLeft();
                case TextEditOp.MoveRight:
                    return _value.moveRight();
                case TextEditOp.MoveUp:             
                    newPosition = this.renderEditable.getPositionUp(new TextPosition(_value.selection.start, _value.selection.affinity));
                    return _value.copyWith(selection: TextSelection.fromPosition(newPosition));
               case TextEditOp.MoveDown:
                   newPosition = this.renderEditable.getPositionDown(new TextPosition(_value.selection.start, _value.selection.affinity));
                   return _value.copyWith(selection: TextSelection.fromPosition(newPosition));
                case TextEditOp.MoveLineStart:      
                    newPosition = this.renderEditable.getLineStartPosition(new TextPosition(_value.selection.start, _value.selection.affinity));
                    return _value.copyWith(selection: TextSelection.fromPosition(newPosition));
                case TextEditOp.MoveLineEnd:      
                    newPosition = this.renderEditable.getLineEndPosition(new TextPosition(_value.selection.start, _value.selection.affinity));
                    return _value.copyWith(selection: TextSelection.fromPosition(newPosition));
               case TextEditOp.MoveWordRight:      
                   newPosition = this.renderEditable.getWordRight(new TextPosition(_value.selection.start));
                   return _value.copyWith(selection: TextSelection.fromPosition(newPosition));
                case TextEditOp.MoveWordLeft:      
                    newPosition = this.renderEditable.getWordLeft(new TextPosition(_value.selection.start));
                    return _value.copyWith(selection: TextSelection.fromPosition(newPosition));
//                case TextEditOp.MoveToStartOfNextWord:      MoveToStartOfNextWord(); break;
//                case TextEditOp.MoveToEndOfPreviousWord:        MoveToEndOfPreviousWord(); break;

                case TextEditOp.MoveTextStart:
                    return _value.copyWith(selection: TextSelection.collapsed(0));
                case TextEditOp.MoveTextEnd:        
                    return _value.copyWith(selection: TextSelection.collapsed(_value.text.Length));
//                case TextEditOp.MoveParagraphForward:   MoveParagraphForward(); break;
//                case TextEditOp.MoveParagraphBackward:  MoveParagraphBackward(); break;
//                case TextEditOp.MoveGraphicalLineStart: MoveGraphicalLineStart(); break;
//                case TextEditOp.MoveGraphicalLineEnd: MoveGraphicalLineEnd(); break;
                  case TextEditOp.SelectLeft:
                      return _value.extendLeft();
                  case TextEditOp.SelectRight:
                      return _value.extendRight();
                  case TextEditOp.SelectUp:           
                      newPosition = this.renderEditable.getPositionUp(_value.selection.extendPos);
                      return _value.copyWith(selection: _value.selection.copyWith(extentOffset: newPosition.offset));
                  case TextEditOp.SelectDown:        
                      newPosition = this.renderEditable.getPositionDown(_value.selection.extendPos);
                      return _value.copyWith(selection: _value.selection.copyWith(extentOffset: newPosition.offset));
                  case TextEditOp.SelectWordRight:        
                      newPosition = this.renderEditable.getWordRight(_value.selection.extendPos);
                      return _value.copyWith(selection: _value.selection.copyWith(extentOffset: newPosition.offset));
                  case TextEditOp.SelectWordLeft:     
                      newPosition = this.renderEditable.getWordLeft(_value.selection.extendPos);
                      return _value.copyWith(selection: _value.selection.copyWith(extentOffset: newPosition.offset));
//                case TextEditOp.SelectToEndOfPreviousWord:  SelectToEndOfPreviousWord(); break;
//                case TextEditOp.SelectToStartOfNextWord:    SelectToStartOfNextWord(); break;
//
                 case TextEditOp.SelectTextStart:        
                     return _value.copyWith(selection: _value.selection.copyWith(extentOffset: 0));
               case TextEditOp.SelectTextEnd:      
                   return _value.copyWith(selection: _value.selection.copyWith(extentOffset: _value.text.Length));
//                case TextEditOp.ExpandSelectGraphicalLineStart: ExpandSelectGraphicalLineStart(); break;
//                case TextEditOp.ExpandSelectGraphicalLineEnd: ExpandSelectGraphicalLineEnd(); break;
//                case TextEditOp.SelectParagraphForward:     SelectParagraphForward(); break;
//                case TextEditOp.SelectParagraphBackward:    SelectParagraphBackward(); break;
//                case TextEditOp.SelectGraphicalLineStart: SelectGraphicalLineStart(); break;
//                case TextEditOp.SelectGraphicalLineEnd: SelectGraphicalLineEnd(); break;
                 case TextEditOp.Delete: return _value.deleteSelection(false);
                case TextEditOp.Backspace:
                    return _value.deleteSelection();
            }

            return _value;
        }

        void _updateRemoteEditingValueIfNeeded()
        {
            if (!_hasInputConnection)
                return;
            var localValue = _value;
            if (localValue == _lastKnownRemoteTextEditingValue)
                return;
            _lastKnownRemoteTextEditingValue = localValue;
            _textInputConnection.setEditingState(localValue);
        }


        bool _hasInputConnection
        {
            get
            {
                return  _textInputConnection != null && _textInputConnection.attached;
            }
        }
       

        void _openInputConnection() {
            if (!_hasInputConnection) {
                TextEditingValue localValue = _value;
                _lastKnownRemoteTextEditingValue = localValue;
                _textInputConnection = Window.instance.textInput.attach(this);
                _textInputConnection.setEditingState(localValue);
            }
        }

        void _closeInputConnectionIfNeeded() {
            if (_hasInputConnection) {
                _textInputConnection.close();
                _textInputConnection = null;
                _lastKnownRemoteTextEditingValue = null;
            }
        }

        void _openOrCloseInputConnectionIfNeeded() {
            if (_hasFocus && widget.focusNode.consumeKeyboardToken()) {
                _openInputConnection();
            } else if (!_hasFocus) {
                _closeInputConnectionIfNeeded();
                widget.controller.clearComposing();
            }
        }

        public void requestKeyboard()
        {
            if (_hasFocus)
            {
                _openInputConnection();
            }
            else
            {
                FocusScope.of(context).requestFocus(widget.focusNode);
            }
        }
        
        private void _handleSelectionChanged(TextSelection selection, RenderEditable renderObject, SelectionChangedCause cause) {
            widget.controller.selection = selection;
            requestKeyboard();
            
            if (widget.onSelectionChanged != null)
            {
                widget.onSelectionChanged(selection, cause);
            }
        }

        void _handleCaretChanged(Rect caretRect) {
            if (_textChangedSinceLastCaretUpdate) {
                _textChangedSinceLastCaretUpdate = false; 
//                scheduleMicrotask(() { // todo
//                    _scrollController.animateTo(
//                        _getScrollOffsetForCaret(caretRect),
//                        curve: Curves.fastOutSlowIn,
//                        duration: const Duration(milliseconds: 50),
//                        );
//                });
            }
        }
        private void _formatAndSetValue(TextEditingValue value) {
            var textChanged = (_value == null ? null : _value.text) != (value == null ? null : value.text);
            if (widget.inputFormatters != null && widget.inputFormatters.isNotEmpty()) {
                foreach (var formatter in widget.inputFormatters)
                {
                    value = formatter.formatEditUpdate(_value, value);
                }
                _value = value;
                _updateRemoteEditingValueIfNeeded();
            } else {
                _value = value;
            }

            if (textChanged && widget.onChanged != null)
            {
                widget.onChanged(value.text);    
            }
        }
        
        private TextEditingValue _value
        {
            get { return widget.controller.value; }
            set
            {
                widget.controller.value = value;
            }
        }

        private bool _hasFocus
        {
            get { return widget.focusNode.hasFocus; }
        }

        private bool _isMultiline
        {
            get { return widget.maxLines !=  1; }
        }

        private void _didChangeTextEditingValue()
        {
            _updateRemoteEditingValueIfNeeded();
            _textChangedSinceLastCaretUpdate = true;
            setState(() => {});
        }

        private void _handleFocusChanged()
        {
            _openOrCloseInputConnectionIfNeeded();
            if (!_hasFocus) {
                _value = new TextEditingValue(text: _value.text);
            } else if (!_value.selection.isValid) {
                widget.controller.selection = TextSelection.collapsed(offset: _value.text.Length);
            }
        }


        private TextDirection? _textDirection
        {
            get
            {
                TextDirection? result = widget.textDirection ?? Directionality.of(context);
                D.assert(result != null, 
                    string.Format("{0} created without a textDirection and with no ambient Directionality.", GetType().FullName));
                return result;
            }
        }

        public RenderEditable renderEditable
        {
            get { return (RenderEditable)_editableKey.currentContext.findRenderObject(); }
        }

        public override  Widget build(BuildContext context)
        {
            FocusScope.of(context).reparentIfNeeded(widget.focusNode);
            // todo base.build(context);  See AutomaticKeepAliveClientMixin.
            return new _Editable(
                key: _editableKey,
                textSpan: buildTextSpan(),
                value: _value,
                cursorColor: widget.cursorColor,
                showCursor: _showCursor,
                hasFocus: _hasFocus,
                maxLines: widget.maxLines,
                selectionColor: widget.selectionColor,
                textScaleFactor: 1.0, // todo widget.textScaleFactor ?? MediaQuery.textScaleFactorOf(context),
                textAlign: widget.textAlign,
                textDirection: _textDirection,
                obscureText: widget.obscureText,
                autocorrect: widget.autocorrect,
                offset: new _FixedViewportOffset(0.0),
                onSelectionChanged: _handleSelectionChanged,
                onCaretChanged: _handleCaretChanged,
                rendererIgnoresPointer: widget.rendererIgnoresPointer
                );
            
        }
        
        public TextSpan buildTextSpan() {
            if (!widget.obscureText && _value.composing.isValid) {
                TextStyle composingStyle = widget.style.merge(
                        new TextStyle(decoration: TextDecoration.underline)
                    );

                return new TextSpan(
                    style: widget.style,
                    children: new List<TextSpan>
                    {
                        new TextSpan(text: _value.composing.textBefore(_value.text)),
                        new TextSpan(
                            style: composingStyle,
                            text: _value.composing.textInside(_value.text)
                        ),
                        new TextSpan(text: _value.composing.textAfter(_value.text)),
                    });
            }

            var text = _value.text;
            if (widget.obscureText) {
                text = new string(RenderEditable.obscuringCharacter, text.Length);
                int o =
                    _obscureShowCharTicksPending > 0 ? _obscureLatestCharIndex : -1;
                if (o >= 0 && o < text.Length)
                    text = text.Substring(0, o) + _value.text.Substring(o, 1) + text.Substring(o + 1);
            }
            return new TextSpan(style: widget.style, text: text);
        }
    }



    internal class _Editable : LeafRenderObjectWidget
    {
        public readonly TextSpan textSpan;
        public readonly TextEditingValue value;
        public readonly Color cursorColor;
        public readonly ValueNotifier<bool> showCursor;
        public readonly bool hasFocus;
        public readonly int maxLines;
        public readonly Color selectionColor;
        public readonly double textScaleFactor;
        public readonly TextAlign textAlign;
        public readonly TextDirection? textDirection;
        public readonly bool obscureText;
        public readonly bool autocorrect;
        public readonly ViewportOffset offset;
        public readonly SelectionChangedHandler onSelectionChanged;
        public readonly CaretChangedHandler onCaretChanged;
        public readonly bool rendererIgnoresPointer;


        public _Editable(TextSpan textSpan = null, TextEditingValue value = null,
            Color cursorColor = null, ValueNotifier<bool> showCursor = null, bool hasFocus = false,
            int maxLines = 0, Color selectionColor = null, double textScaleFactor = 1.0,
            TextDirection? textDirection = null, bool obscureText = false, TextAlign textAlign = TextAlign.left,
            bool autocorrect = false, ViewportOffset offset = null, SelectionChangedHandler onSelectionChanged = null,
            CaretChangedHandler onCaretChanged = null, bool rendererIgnoresPointer = false, Key key = null) : base(key)
        {
            
            this.textSpan = textSpan;
            this.value = value;
            this.cursorColor = cursorColor;
            this.showCursor = showCursor;
            this.hasFocus = hasFocus;
            this.maxLines = maxLines;
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
        }

        public override RenderObject createRenderObject(BuildContext context)
        {
            return new RenderEditable(
                text: textSpan,
                textDirection: textDirection??TextDirection.ltr,
                offset: offset,
                showCursor: showCursor,
                cursorColor: cursorColor,
                hasFocus: hasFocus,
                maxLines: maxLines,
                selectionColor: selectionColor,
                textScaleFactor: textScaleFactor,
                textAlign: textAlign,
                selection: value.selection, 
                obscureText: obscureText,
                onSelectionChanged: onSelectionChanged,
                onCaretChanged: onCaretChanged,
                ignorePointer: rendererIgnoresPointer          
            ); 
        }   

        public override void updateRenderObject(BuildContext context, RenderObject renderObject)
        {
            var edit = (RenderEditable) renderObject;
            edit.text = textSpan;
            edit.cursorColor = cursorColor;
            edit.showCursor = showCursor;
            edit.hasFocus = hasFocus;
            edit.maxLines = maxLines;
            edit.selectionColor = selectionColor;
            edit.textScaleFactor = textScaleFactor;
            edit.textAlign = textAlign;
            edit.textDirection = textDirection;
            edit.selection = value.selection;
            edit.offset = offset;
            edit.onSelectionChanged = onSelectionChanged;
            edit.onCaretChanged = onCaretChanged;
            edit.ignorePointer = rendererIgnoresPointer;
            edit.obscureText = obscureText;     
        }
    }
    
    
    class _FixedViewportOffset : ViewportOffset {
        internal _FixedViewportOffset(double _pixels) {
            this._pixels = _pixels;
        }

        internal new static _FixedViewportOffset zero() {
            return new _FixedViewportOffset(0.0);
        }

        double _pixels;

        public override double pixels {
            get { return this._pixels; }
        }

        public override bool applyViewportDimension(double viewportDimension) {
            return true;
        }

        public override bool applyContentDimensions(double minScrollExtent, double maxScrollExtent) {
            return true;
        }

        public override void correctBy(double correction) {
            this._pixels += correction;
        }

        public override void jumpTo(double pixels) {
        }

        public override IPromise animateTo(double to, TimeSpan duration, Curve curve) {
            return Promise.Resolved();
        }

        public override ScrollDirection userScrollDirection {
            get { return ScrollDirection.idle; }
        }

        public override bool allowImplicitScrolling {
            get { return false; }
        }
    }
}