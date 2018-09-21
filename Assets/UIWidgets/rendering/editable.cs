using System;
using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.gestures;
using UIWidgets.painting;
using UIWidgets.service;
using UIWidgets.ui;
using UnityEngine;
using Canvas = UIWidgets.ui.Canvas;
using Color = UIWidgets.ui.Color;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.rendering
{
    public delegate void SelectionChangedHandler(TextSelection selection, RenderEditable renderObject,
        SelectionChangedCause cause);

    public delegate void CaretChangedHandler(Rect caretRect);

    public enum SelectionChangedCause
    {
        tap,
        doubleTap,
        longPress,
        keyboard,
    }

    public class TextSelectionPoint
    {
        public readonly Offset point;
        public readonly TextDirection? direction;

        public TextSelectionPoint(Offset point, TextDirection? direction)
        {
            D.assert(point != null);
            this.point = point;
            this.direction = direction;
        }

        public override string ToString()
        {
            return string.Format("Point: {0}, Direction: {1}", point, direction);
        }
    }
/*
    this._doubleTapGesture = new DoubleTapGestureRecognizer(this.rendererBindings.rendererBinding);
    this._doubleTapGesture.onDoubleTap = () => { Debug.Log("onDoubleTap"); };*/

    public class RenderEditable : RenderBox
    {
        public static readonly char obscuringCharacter = '•';
        private static readonly double _kCaretGap = 1.0;
        private static readonly double _kCaretHeightOffset = 2.0;
        private static readonly double _kCaretWidth = 1.0;

        private TextPainter _textPainter;
        private Color _cursorColor;
        private bool _hasFocus;
        private int _maxLines;
        private Color _selectionColor;
        private ViewportOffset _offset;
        private ValueNotifier<bool> _showCursor;
        private TextSelection _selection;
        private bool _obscureText;
        private TapGestureRecognizer _tap;
        private DoubleTapGestureRecognizer _doubleTap;
        public bool ignorePointer;
        public SelectionChangedHandler onSelectionChanged;
        public CaretChangedHandler onCaretChanged;
        private Rect _lastCaretRect;
        private double? _textLayoutLastWidth;
        private List<TextBox> _selectionRects;
        private Rect _caretPrototype;
        private bool _hasVisualOverflow = false;
        private Offset _lastTapDownPosition;

        public RenderEditable(TextSpan text, TextDirection textDirection, ViewportOffset offset,
            ValueNotifier<bool> showCursor,
            TextAlign textAlign = TextAlign.left, double textScaleFactor = 1.0, Color cursorColor = null,
            bool? hasFocus = null, int maxLines = 1, Color selectionColor = null,
            TextSelection selection = null, bool obscureText = false, SelectionChangedHandler onSelectionChanged = null,
            CaretChangedHandler onCaretChanged = null, bool ignorePointer = false)
        {
            _textPainter = new TextPainter(text: text, textAlign: textAlign, textDirection: textDirection,
                textScaleFactor: textScaleFactor);
            _cursorColor = cursorColor;
            _showCursor = showCursor ?? new ValueNotifier<bool>(false);
            _hasFocus = hasFocus ?? false;
            _maxLines = maxLines;
            _selectionColor = selectionColor;
            _selection = selection;
            _obscureText = obscureText;
            _offset = offset;
            this.ignorePointer = ignorePointer;
            this.onCaretChanged = onCaretChanged;
            this.onSelectionChanged = onSelectionChanged;

            D.assert(_showCursor != null);
            D.assert(!_showCursor.value || cursorColor != null);

//            _tap = new TapGestureRecognizer(owner.binding, this);
//            _doubleTap = new DoubleTapGestureRecognizer(owner.binding, this);
//            _tap.onTapDown = this._handleTapDown;
//            _tap.onTap = this._handleTap;
//            _doubleTap.onDoubleTap = this._handleDoubleTap;
            _tap = new TapGestureRecognizer(this);
            _doubleTap = new DoubleTapGestureRecognizer(this);
            _tap.onTapDown = this._handleTapDown;
            _tap.onTap = this._handleTap;
            _doubleTap.onDoubleTap = this._handleDoubleTap;
        }

        public bool obscureText
        {
            get { return _obscureText; }
            set
            {
                if (_obscureText == value)
                    return;
                _obscureText = value;
                markNeedsSemanticsUpdate();
            }
        }

        public TextSpan text
        {
            get { return _textPainter.text; }
            set
            {
                if (_textPainter.text == value)
                {
                    return;
                }

                _textPainter.text = value;
                markNeedsTextLayout();
                markNeedsSemanticsUpdate();
            }
        }

        public TextAlign textAlign
        {
            get { return _textPainter.textAlign; }
            set
            {
                if (_textPainter.textAlign == value)
                {
                    return;
                }

                _textPainter.textAlign = value;
                markNeedsPaint();
            }
        }

        public TextDirection? textDirection
        {
            get { return _textPainter.textDirection; }
            set
            {
                if (_textPainter.textDirection == value)
                {
                    return;
                }

                _textPainter.textDirection = value;
                markNeedsTextLayout();
                markNeedsSemanticsUpdate();
            }
        }

        public Color cursorColor
        {
            get { return _cursorColor; }
            set
            {
                if (_cursorColor == value)
                {
                    return;
                }

                _cursorColor = value;
                markNeedsPaint();
            }
        }

        public ValueNotifier<bool> ShowCursor
        {
            get { return _showCursor; }
            set
            {
                D.assert(value != null);
                if (_showCursor == value)
                {
                    return;
                }

                if (attached)
                {
                    _showCursor.removeListener(markNeedsPaint);
                }

                _showCursor = value;
                if (attached)
                {
                    _showCursor.addListener(markNeedsPaint);
                }

                markNeedsPaint();
            }
        }

        public bool hasFocus
        {
            get { return _hasFocus; }
            set
            {
                if (_hasFocus == value)
                {
                    return;
                }

                _hasFocus = value;
                markNeedsSemanticsUpdate();
            }
        }

        public int maxLines
        {
            get { return _maxLines; }
            set
            {
                D.assert(value > 0);
                if (_maxLines == value)
                {
                    return;
                }

                _maxLines = value;
                markNeedsTextLayout();
            }
        }

        public Color selectionColor
        {
            get { return _selectionColor; }
            set
            {
                if (_selectionColor == value)
                {
                    return;
                }

                _selectionColor = value;
                markNeedsPaint();
            }
        }

        public double textScaleFactor
        {
            get { return _textPainter.textScaleFactor; }
            set
            {
                if (_textPainter.textScaleFactor == value)
                {
                    return;
                }

                _textPainter.textScaleFactor = value;
                markNeedsTextLayout();
            }
        }

        public TextSelection selection
        {
            get { return _selection; }
            set
            {
                if (_selection == value)
                {
                    return;
                }

                _selection = value;
                _selectionRects = null;
                markNeedsPaint();
                markNeedsSemanticsUpdate();
            }
        }

        public ViewportOffset offset
        {
            get { return _offset; }
            set
            {
                D.assert(offset != null);
                if (_offset == value)
                {
                    return;
                }

                if (attached)
                {
                    _offset.removeListener(markNeedsPaint);
                }

                _offset = value;
                if (attached)
                {
                    _offset.addListener(markNeedsPaint);
                }

                markNeedsLayout();
            }
        }

        public ValueNotifier<bool> showCursor
        {
            get { return _showCursor; }
            set
            {
                D.assert(value != null);
                if (_showCursor == value)
                {
                    return;
                }

                if (attached)
                {
                    _showCursor.removeListener(markNeedsPaint);
                }
                _showCursor = value;
                if (attached)
                {
                    _showCursor.addListener(markNeedsPaint);
                }

                markNeedsPaint();
            }
        }

        public double preferredLineHeight
        {
            get { return _textPainter.preferredLineHeight; }
        }


        public override void attach(object ownerObject)
        {
            base.attach(ownerObject);
            _tap = new TapGestureRecognizer(owner.binding, this);
            _doubleTap = new DoubleTapGestureRecognizer(owner.binding, this);
            _tap.onTapDown = this._handleTapDown;
            _tap.onTap = this._handleTap;
            _doubleTap.onDoubleTap = this._handleDoubleTap;
            _offset.addListener(markNeedsLayout);
            _showCursor.addListener(markNeedsPaint);
        }

        public override void detach()
        {
            _tap.dispose();
            _tap = null;
            _doubleTap.dispose();
            _doubleTap = null;
            _offset.removeListener(markNeedsLayout);
            _showCursor.removeListener(markNeedsPaint);
            base.detach();
        }

        /// Returns the local coordinates of the endpoints of the given selection.
        ///
        /// If the selection is collapsed (and therefore occupies a single point), the
        /// returned list is of length one. Otherwise, the selection is not collapsed
        /// and the returned list is of length two. In this case, however, the two
        /// points might actually be co-located (e.g., because of a bidirectional
        /// selection that contains some text but whose ends meet in the middle).
        ///
        public List<TextSelectionPoint> getEndpointsForSelection(TextSelection selection)
        {
            D.assert(constraints != null);
            _layoutText(constraints.maxWidth);
            var paintOffset = _paintOffset;
            if (selection.isCollapsed)
            {
                var caretOffset = _textPainter.getOffsetForCaret(selection.extendPos, _caretPrototype);
                var start = new Offset(0.0, preferredLineHeight) + caretOffset + paintOffset;
                return new List<TextSelectionPoint>{new TextSelectionPoint(start, null)};
            }
            else
            {
                var boxes = _textPainter.getBoxesForSelection(selection);
                var start = new Offset(boxes[0].start, boxes[0].bottom) + paintOffset;
                var last = boxes.Count - 1;
                var end = new Offset(boxes[last].end, boxes[last].bottom) + paintOffset;
                return new List<TextSelectionPoint>
                    {
                        new TextSelectionPoint(start, boxes[0].direction),
                        new TextSelectionPoint(end, boxes[last].direction),
                    };
            }
        }

        public TextPosition getPositionForPoint(Offset globalPosition)
        {
            _layoutText(constraints.maxWidth);
            globalPosition -= _paintOffset;
            return _textPainter.getPositionForOffset(globalPosition);
        }

        public Rect getLocalRectForCaret(TextPosition caretPosition)
        {
            _layoutText(constraints.maxWidth);
            var caretOffset = _textPainter.getOffsetForCaret(caretPosition, _caretPrototype);
            return Rect.fromLTWH(0.0, 0.0, _kCaretWidth, preferredLineHeight).shift(caretOffset + _paintOffset);
        }
        
        public override double computeMinIntrinsicWidth(double height) {
            _layoutText(double.PositiveInfinity);
            return _textPainter.minIntrinsicWidth;
        }
        
        public override double computeMaxIntrinsicWidth(double height) {
            _layoutText(double.PositiveInfinity);
            return _textPainter.maxIntrinsicWidth;
        }

        public override double computeMinIntrinsicHeight(double width) {
            return _preferredHeight(width);
        }

        public override double computeMaxIntrinsicHeight(double width) {
            return _preferredHeight(width);
        }

        public override double? computeDistanceToActualBaseline(TextBaseline baseline) {
            _layoutText(constraints.maxWidth);
            return _textPainter.computeDistanceToActualBaseline(baseline);
        }
        
        public override void handleEvent(PointerEvent evt, HitTestEntry entry) {
            if (ignorePointer)
                return;
            D.assert(debugHandleEvent(evt, entry));
            if (evt is PointerDownEvent && onSelectionChanged != null) {
                _tap.addPointer((PointerDownEvent)evt);
                _doubleTap.addPointer((PointerDownEvent)evt);
                // todo long press
            }
        }

        public void handleTapDown(TapDownDetails details)
        {
            _lastTapDownPosition = details.globalPosition - _paintOffset;
        }

        public void handleTap()
        {
            _layoutText(constraints.maxWidth);
            D.assert(_lastTapDownPosition != null);
            if (onSelectionChanged != null)
            {
                var position = _textPainter.getPositionForOffset(globalToLocal(_lastTapDownPosition));
                onSelectionChanged(TextSelection.fromPosition(position), this, SelectionChangedCause.tap);
            }
        }
        
        public void handleDoubleTap()
        {
            _layoutText(constraints.maxWidth);
            D.assert(_lastTapDownPosition != null);
            if (onSelectionChanged != null)
            {
                var position = _textPainter.getPositionForOffset(globalToLocal(_lastTapDownPosition));
                onSelectionChanged(_selectWordAtOffset(position), this, SelectionChangedCause.doubleTap);
            }
        }
        
        
        public override void performLayout() {
            _layoutText(constraints.maxWidth);
            _caretPrototype = Rect.fromLTWH(0.0, _kCaretHeightOffset, _kCaretWidth,
                preferredLineHeight - 2.0 * _kCaretHeightOffset);
            _selectionRects = null;

            var textPainterSize = _textPainter.size;
            size = new Size(constraints.maxWidth, constraints.constrainHeight(_preferredHeight(constraints.maxWidth)));
            var contentSize = new Size(textPainterSize.width + _kCaretGap + _kCaretWidth, textPainterSize.height);
            var _maxScrollExtent = _getMaxScrollExtend(contentSize);
            _hasVisualOverflow = _maxScrollExtent > 0.0;
            offset.applyViewportDimension(_viewportExtend);
            offset.applyContentDimensions(0.0, _maxScrollExtent);
        }

        public override void paint(PaintingContext context, Offset offset)
        {
            _layoutText(constraints.maxWidth);
            if (_hasVisualOverflow)
            {
                context.pushClipRect(needsCompositing, offset, Offset.zero & size, _paintContents);
            }
            else
            {
                _paintContents(context, offset);
            }
        }
        
        protected override bool hitTestSelf(Offset position)
        {
            return true;
        }

        protected void markNeedsTextLayout()
        {
            _textLayoutLastWidth = null;
            markNeedsLayout();
        }

        // describeSemanticsConfiguration todo
        
        
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Color>("cursorColor", cursorColor));
            properties.add(new DiagnosticsProperty<ValueNotifier<bool>>("showCursor", showCursor));
            properties.add(new DiagnosticsProperty<int>("maxLines", maxLines));
            properties.add(new DiagnosticsProperty<Color>("selectionColor", selectionColor));
            properties.add(new DiagnosticsProperty<double>("textScaleFactor", textScaleFactor));
            properties.add(new DiagnosticsProperty<TextSelection>("selection", selection));
            properties.add(new DiagnosticsProperty<ViewportOffset>("offset", offset));
        }

        private void _paintCaret(Canvas canvas, Offset effectiveOffset)
        {
            D.assert(_textLayoutLastWidth == constraints.maxWidth);
            var caretOffset = _textPainter.getOffsetForCaret(_selection.extendPos, _caretPrototype);
            var paint = new Paint() {color = _cursorColor};
            var caretRec = _caretPrototype.shift(caretOffset + effectiveOffset);
            canvas.drawRect(caretRec, BorderWidth.zero, BorderRadius.zero, paint);
            if (!caretRec.Equals(_lastCaretRect))
            {
                _lastCaretRect = caretRec;
                if (onCaretChanged != null)
                {
                    onCaretChanged(caretRec);
                }
            }
        }

        void _paintSelection(Canvas canvas, Offset effectiveOffset)
        {
            D.assert(_textLayoutLastWidth == constraints.maxWidth);
            D.assert(_selectionRects != null);
            var paint = new Paint() {color = _selectionColor};
            
            foreach (var box in _selectionRects)
            {
                canvas.drawRect(box.toRect().shift(effectiveOffset), BorderWidth.zero, BorderRadius.zero, paint);
            }
        }

        void _paintContents(PaintingContext context, Offset offset)
        {
            D.assert(_textLayoutLastWidth == constraints.maxWidth);
            var effectiveOffset = offset + _paintOffset;
            
            if (_selection != null) {
                if (_selection.isCollapsed && _showCursor.value && cursorColor != null) {
                    _paintCaret(context.canvas, effectiveOffset);
                } else if (!_selection.isCollapsed && _selectionColor != null) {
                    _selectionRects = _selectionRects??_textPainter.getBoxesForSelection(_selection);
                    _paintSelection(context.canvas, effectiveOffset);
                }
            }

            if (_hasFocus) {
                var caretOffset = _textPainter.getOffsetForCaret(_selection.extendPos, Rect.fromLTWH(0, 0, 1, preferredLineHeight));
                var caretRec = _caretPrototype.shift(caretOffset + effectiveOffset);
                Input.compositionCursorPos = new Vector2((float)caretRec.left, (float)caretRec.bottom);
            }
            _textPainter.paint(context.canvas, effectiveOffset);
        }
        
        private void _handleSetSelection(TextSelection selection)
        {
            onSelectionChanged(selection, this, SelectionChangedCause.keyboard);
        }

        private void _handleTapDown(TapDownDetails details)
        {
            D.assert(!ignorePointer);
            handleTapDown(details);
        }

        private void _handleTap()
        {
            D.assert(!ignorePointer);
            handleTap();
        }

        private void _handleDoubleTap()
        {
            D.assert(!ignorePointer);
            handleDoubleTap();
        }

        private void markNeedsSemanticsUpdate()
        {
            // todo
        }

        private double _preferredHeight(double width)
        {
            if (maxLines <= 0)
            {
                return preferredLineHeight * maxLines;
            }

            if (double.IsInfinity(width))
            {
                var text = _textPainter.text.text;
                int lines = 1;
                for (int index = 0; index < text.Length; ++index)
                {
                    if (text[index] == 0x0A)
                    {
                        lines += 1;
                    }
                }

                return preferredLineHeight * lines;
            }
            
            _layoutText(width);
            return Math.Max(preferredLineHeight, _textPainter.height);
        }

        private void _layoutText(double constraintWidth)
        {
            if (_textLayoutLastWidth == constraintWidth)
            {
                return;
            }

            var caretMargin = _kCaretGap + _kCaretWidth;
            var avialableWidth = Math.Max(0.0, constraintWidth - caretMargin);
            var maxWidth = _isMultiline ? avialableWidth : double.PositiveInfinity;
            _textPainter.layout(minWidth: avialableWidth, maxWidth: maxWidth);
            _textLayoutLastWidth = constraintWidth;
        }

        TextSelection _selectWordAtOffset(TextPosition position)
        {
            D.assert(_textLayoutLastWidth == constraints.maxWidth);
            var word = _textPainter.getWordBoundary(position);
            if (position.offset >= word.end)
            {
                return TextSelection.fromPosition(position);
            }
            return new TextSelection(baseOffset: word.start, extentOffset: word.end);
        }
        
        private bool _isMultiline
        {
            get { return _maxLines != 1; }
        }

        private Axis _viewportAxis
        {
            get { return _isMultiline ? Axis.vertical : Axis.horizontal; }
        }

        private Offset _paintOffset
        {
            get
            {
                switch (_viewportAxis)
                {
                    case Axis.horizontal:
                        return new Offset(-offset.pixels, 0.0);
                    case Axis.vertical:
                        return new Offset(0.0, -offset.pixels);
                }

                return null;
            }
        }

        private double _viewportExtend
        {
            get
            {
                D.assert(hasSize);
                switch (_viewportAxis)
                {
                    case Axis.horizontal:
                        return size.width;
                    case Axis.vertical:
                        return size.height;
                }

                return 0.0;
            }
        }

        private double _getMaxScrollExtend(Size contentSize)
        {
            D.assert(hasSize);
            switch (_viewportAxis)
            {
                case Axis.horizontal:
                    return Math.Max(0.0, contentSize.width - size.width);
                case Axis.vertical:
                    return Math.Max(0.0, contentSize.height - size.height);
            }

            return 0.0;
        }
    }
}