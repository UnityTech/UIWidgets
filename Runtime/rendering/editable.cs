using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.rendering {
    public delegate void SelectionChangedHandler(TextSelection selection, RenderEditable renderObject,
        SelectionChangedCause cause);

    public delegate void CaretChangedHandler(Rect caretRect);

    public enum SelectionChangedCause {
        tap,
        doubleTap,
        longPress,
        keyboard,
    }

    public class TextSelectionPoint {
        public readonly Offset point;
        public readonly TextDirection? direction;

        public TextSelectionPoint(Offset point, TextDirection? direction) {
            D.assert(point != null);
            this.point = point;
            this.direction = direction;
        }

        public override string ToString() {
            return $"Point: {this.point}, Direction: {this.direction}";
        }
    }
/*
    this._doubleTapGesture = new DoubleTapGestureRecognizer(this.rendererBindings.rendererBinding);
    this._doubleTapGesture.onDoubleTap = () => { Debug.Log("onDoubleTap"); };*/

    public class RenderEditable : RenderBox {
        public static readonly char obscuringCharacter = '•';
        static readonly double _kCaretGap = 1.0;
        static readonly double _kCaretHeightOffset = 2.0;
        static readonly double _kCaretWidth = 1.0;

        TextPainter _textPainter;
        Color _cursorColor;
        bool _hasFocus;
        int? _maxLines;
        Color _selectionColor;
        ViewportOffset _offset;
        ValueNotifier<bool> _showCursor;
        TextSelection _selection;
        bool _obscureText;
        TapGestureRecognizer _tap;
        LongPressGestureRecognizer _longPress;
        DoubleTapGestureRecognizer _doubleTap;
        public bool ignorePointer;
        public SelectionChangedHandler onSelectionChanged;
        public CaretChangedHandler onCaretChanged;
        Rect _lastCaretRect;
        double? _textLayoutLastWidth;
        List<TextBox> _selectionRects;
        Rect _caretPrototype;
        bool _hasVisualOverflow = false;
        Offset _lastTapDownPosition;

        public RenderEditable(TextSpan text, TextDirection textDirection, ViewportOffset offset,
            ValueNotifier<bool> showCursor,
            TextAlign textAlign = TextAlign.left, double textScaleFactor = 1.0, Color cursorColor = null,
            bool? hasFocus = null, int? maxLines = 1, Color selectionColor = null,
            TextSelection selection = null, bool obscureText = false, SelectionChangedHandler onSelectionChanged = null,
            CaretChangedHandler onCaretChanged = null, bool ignorePointer = false) {
            this._textPainter = new TextPainter(text: text, textAlign: textAlign, textDirection: textDirection,
                textScaleFactor: textScaleFactor);
            this._cursorColor = cursorColor;
            this._showCursor = showCursor ?? new ValueNotifier<bool>(false);
            this._hasFocus = hasFocus ?? false;
            this._maxLines = maxLines;
            this._selectionColor = selectionColor;
            this._selection = selection;
            this._obscureText = obscureText;
            this._offset = offset;
            this.ignorePointer = ignorePointer;
            this.onCaretChanged = onCaretChanged;
            this.onSelectionChanged = onSelectionChanged;

            D.assert(this._maxLines == null || this._maxLines > 0);
            D.assert(this._showCursor != null);
            D.assert(!this._showCursor.value || cursorColor != null);

            this._tap = new TapGestureRecognizer(this);
            this._doubleTap = new DoubleTapGestureRecognizer(this);
            this._tap.onTapDown = this._handleTapDown;
            this._tap.onTap = this._handleTap;
            this._doubleTap.onDoubleTap = this._handleDoubleTap;
            this._longPress = new LongPressGestureRecognizer(debugOwner: this);
            this._longPress.onLongPress = this._handleLongPress;
        }

        public bool obscureText {
            get { return this._obscureText; }
            set {
                if (this._obscureText == value) {
                    return;
                }

                this._obscureText = value;
                this.markNeedsSemanticsUpdate();
            }
        }

        public TextSpan text {
            get { return this._textPainter.text; }
            set {
                if (this._textPainter.text == value) {
                    return;
                }

                this._textPainter.text = value;
                this.markNeedsTextLayout();
                this.markNeedsSemanticsUpdate();
            }
        }

        public TextAlign textAlign {
            get { return this._textPainter.textAlign; }
            set {
                if (this._textPainter.textAlign == value) {
                    return;
                }

                this._textPainter.textAlign = value;
                this.markNeedsPaint();
            }
        }

        public TextDirection? textDirection {
            get { return this._textPainter.textDirection; }
            set {
                if (this._textPainter.textDirection == value) {
                    return;
                }

                this._textPainter.textDirection = value;
                this.markNeedsTextLayout();
                this.markNeedsSemanticsUpdate();
            }
        }

        public Color cursorColor {
            get { return this._cursorColor; }
            set {
                if (this._cursorColor == value) {
                    return;
                }

                this._cursorColor = value;
                this.markNeedsPaint();
            }
        }

        public ValueNotifier<bool> ShowCursor {
            get { return this._showCursor; }
            set {
                D.assert(value != null);
                if (this._showCursor == value) {
                    return;
                }

                if (this.attached) {
                    this._showCursor.removeListener(this.markNeedsPaint);
                }

                this._showCursor = value;
                if (this.attached) {
                    this._showCursor.addListener(this.markNeedsPaint);
                }

                this.markNeedsPaint();
            }
        }

        public bool hasFocus {
            get { return this._hasFocus; }
            set {
                if (this._hasFocus == value) {
                    return;
                }

                this._hasFocus = value;
                this.markNeedsSemanticsUpdate();
            }
        }

        public int? maxLines {
            get { return this._maxLines; }
            set {
                D.assert(value == null || value > 0);
                if (this._maxLines == value) {
                    return;
                }

                this._maxLines = value;
                this.markNeedsTextLayout();
            }
        }

        public Color selectionColor {
            get { return this._selectionColor; }
            set {
                if (this._selectionColor == value) {
                    return;
                }

                this._selectionColor = value;
                this.markNeedsPaint();
            }
        }

        public double textScaleFactor {
            get { return this._textPainter.textScaleFactor; }
            set {
                if (this._textPainter.textScaleFactor == value) {
                    return;
                }

                this._textPainter.textScaleFactor = value;
                this.markNeedsTextLayout();
            }
        }

        public TextSelection selection {
            get { return this._selection; }
            set {
                if (this._selection == value) {
                    return;
                }

                this._selection = value;
                this._selectionRects = null;
                this.markNeedsPaint();
                this.markNeedsSemanticsUpdate();
            }
        }

        public ViewportOffset offset {
            get { return this._offset; }
            set {
                D.assert(this.offset != null);
                if (this._offset == value) {
                    return;
                }

                if (this.attached) {
                    this._offset.removeListener(this.markNeedsPaint);
                }

                this._offset = value;
                if (this.attached) {
                    this._offset.addListener(this.markNeedsPaint);
                }

                this.markNeedsLayout();
            }
        }

        public ValueNotifier<bool> showCursor {
            get { return this._showCursor; }
            set {
                D.assert(value != null);
                if (this._showCursor == value) {
                    return;
                }

                if (this.attached) {
                    this._showCursor.removeListener(this.markNeedsPaint);
                }

                this._showCursor = value;
                if (this.attached) {
                    this._showCursor.addListener(this.markNeedsPaint);
                }

                this.markNeedsPaint();
            }
        }

        public double preferredLineHeight {
            get { return this._textPainter.preferredLineHeight; }
        }


        public override void attach(object ownerObject) {
            base.attach(ownerObject);
            this._offset.addListener(this.markNeedsLayout);
            this._showCursor.addListener(this.markNeedsPaint);
        }

        public override void detach() {
            this._offset.removeListener(this.markNeedsLayout);
            this._showCursor.removeListener(this.markNeedsPaint);
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
        public List<TextSelectionPoint> getEndpointsForSelection(TextSelection selection) {
            D.assert(this.constraints != null);
            this._layoutText(this.constraints.maxWidth);
            var paintOffset = this._paintOffset;
            if (selection.isCollapsed) {
                var caretOffset = this._textPainter.getOffsetForCaret(selection.extendPos, this._caretPrototype);
                var start = new Offset(0.0, this.preferredLineHeight) + caretOffset + paintOffset;
                return new List<TextSelectionPoint> {new TextSelectionPoint(start, null)};
            }
            else {
                var boxes = this._textPainter.getBoxesForSelection(selection);
                var start = new Offset(boxes[0].start, boxes[0].bottom) + paintOffset;
                var last = boxes.Count - 1;
                var end = new Offset(boxes[last].end, boxes[last].bottom) + paintOffset;
                return new List<TextSelectionPoint> {
                    new TextSelectionPoint(start, boxes[0].direction),
                    new TextSelectionPoint(end, boxes[last].direction),
                };
            }
        }

        public TextPosition getPositionForPoint(Offset globalPosition) {
            this._layoutText(this.constraints.maxWidth);
            globalPosition -= this._paintOffset;
            return this._textPainter.getPositionForOffset(this.globalToLocal(globalPosition));
        }

        public Rect getLocalRectForCaret(TextPosition caretPosition) {
            this._layoutText(this.constraints.maxWidth);
            var caretOffset = this._textPainter.getOffsetForCaret(caretPosition, this._caretPrototype);
            return Rect.fromLTWH(0.0, 0.0, _kCaretWidth, this.preferredLineHeight)
                .shift(caretOffset + this._paintOffset);
        }

        public TextPosition getPositionDown(TextPosition position) {
            return this._textPainter.getPositionVerticalMove(position, 1);
        }

        public TextPosition getPositionUp(TextPosition position) {
            return this._textPainter.getPositionVerticalMove(position, -1);
        }

        public TextPosition getLineStartPosition(TextPosition position, TextAffinity? affinity = null) {
            var line = this._textPainter.getLineRange(position);
            return new TextPosition(offset: line.start, affinity: affinity ?? position.affinity);
        }

        public bool isLineEndOrStart(int offset) {
            int lineCount = this._textPainter.getLineCount();
            for (int i = 0; i < lineCount; i++) {
                var line = this._textPainter.getLineRange(i);
                if (line.start == offset || line.endIncludingNewLine == offset) {
                    return true;
                }
            }

            return false;
        }

        public TextPosition getLineEndPosition(TextPosition position, TextAffinity? affinity = null) {
            var line = this._textPainter.getLineRange(position);
            return new TextPosition(offset: line.endIncludingNewLine, affinity: affinity ?? position.affinity);
        }

        public TextPosition getWordRight(TextPosition position) {
            return this._textPainter.getWordRight(position);
        }

        public TextPosition getWordLeft(TextPosition position) {
            return this._textPainter.getWordLeft(position);
        }

        public TextPosition getParagraphStart(TextPosition position, TextAffinity? affinity = null) {
            D.assert(!this._needsLayout);
            int lineIndex = this._textPainter.getLineIndex(position);
            while (lineIndex - 1 >= 0) {
                var preLine = this._textPainter.getLineRange(lineIndex - 1);
                if (preLine.hardBreak) {
                    break;
                }

                lineIndex--;
            }

            var line = this._textPainter.getLineRange(lineIndex);
            return new TextPosition(offset: line.start, affinity: affinity ?? position.affinity);
        }

        public TextPosition getParagraphEnd(TextPosition position, TextAffinity? affinity = null) {
            D.assert(!this._needsLayout);
            int lineIndex = this._textPainter.getLineIndex(position);
            int maxLine = this._textPainter.getLineCount();
            while (lineIndex < maxLine) {
                var line = this._textPainter.getLineRange(lineIndex);
                if (line.hardBreak) {
                    break;
                }

                lineIndex++;
            }

            return new TextPosition(offset: this._textPainter.getLineRange(lineIndex).endIncludingNewLine,
                affinity: affinity ?? position.affinity);
        }

        public TextPosition getParagraphForward(TextPosition position, TextAffinity? affinity = null) {
            var lineCount = this._textPainter.getLineCount();
            Paragraph.LineRange line = null;
            for (int i = 0; i < lineCount; ++i) {
                line = this._textPainter.getLineRange(i);
                if (!line.hardBreak) {
                    continue;
                }

                if (line.end > position.offset) {
                    break;
                }
            }

            if (line == null) {
                return new TextPosition(position.offset, affinity ?? position.affinity);
            }

            return new TextPosition(line.end, affinity ?? position.affinity);
        }


        public TextPosition getParagraphBackward(TextPosition position, TextAffinity? affinity = null) {
            var lineCount = this._textPainter.getLineCount();
            Paragraph.LineRange line = null;
            for (int i = lineCount - 1; i >= 0; --i) {
                line = this._textPainter.getLineRange(i);
                if (i != 0 && !this._textPainter.getLineRange(i - 1).hardBreak) {
                    continue;
                }

                if (line.start < position.offset) {
                    break;
                }
            }

            if (line == null) {
                return new TextPosition(position.offset, affinity ?? position.affinity);
            }

            return new TextPosition(line.start, affinity ?? position.affinity);
        }

        protected override double computeMinIntrinsicWidth(double height) {
            this._layoutText(double.PositiveInfinity);
            return this._textPainter.minIntrinsicWidth;
        }

        protected override double computeMaxIntrinsicWidth(double height) {
            this._layoutText(double.PositiveInfinity);
            return this._textPainter.maxIntrinsicWidth;
        }

        protected override double computeMinIntrinsicHeight(double width) {
            return this._preferredHeight(width);
        }

        protected override double computeMaxIntrinsicHeight(double width) {
            return this._preferredHeight(width);
        }

        protected override double? computeDistanceToActualBaseline(TextBaseline baseline) {
            this._layoutText(this.constraints.maxWidth);
            return this._textPainter.computeDistanceToActualBaseline(baseline);
        }

        public override void handleEvent(PointerEvent evt, HitTestEntry entry) {
            if (this.ignorePointer) {
                return;
            }

            D.assert(this.debugHandleEvent(evt, entry));
            if (evt is PointerDownEvent && this.onSelectionChanged != null) {
                this._tap.addPointer((PointerDownEvent) evt);
                this._doubleTap.addPointer((PointerDownEvent) evt);
                this._longPress.addPointer((PointerDownEvent) evt);
            }
        }

        public void handleTapDown(TapDownDetails details) {
            this._lastTapDownPosition = details.globalPosition - this._paintOffset;
        }

        public void handleTap() {
            this._layoutText(this.constraints.maxWidth);
            D.assert(this._lastTapDownPosition != null);
            if (this.onSelectionChanged != null) {
                var position = this._textPainter.getPositionForOffset(this.globalToLocal(this._lastTapDownPosition));
                this.onSelectionChanged(TextSelection.fromPosition(position), this, SelectionChangedCause.tap);
            }
        }

        public void handleDoubleTap(DoubleTapDetails details) {
            this._lastTapDownPosition = details.firstGlobalPosition - this._paintOffset;
            this.selectWord(cause: SelectionChangedCause.doubleTap);
        }

        public void handleLongPress() {
            this.selectWord(cause: SelectionChangedCause.longPress);
        }

        void selectWord(SelectionChangedCause? cause = null) {
            this._layoutText(this.constraints.maxWidth);
            D.assert(this._lastTapDownPosition != null);
            if (this.onSelectionChanged != null) {
                TextPosition position =
                    this._textPainter.getPositionForOffset(this.globalToLocal(this._lastTapDownPosition));
                this.onSelectionChanged(this._selectWordAtOffset(position), this, cause.Value);
            }
        }

        protected override void performLayout() {
            this._layoutText(this.constraints.maxWidth);
            this._caretPrototype = Rect.fromLTWH(0.0, _kCaretHeightOffset, _kCaretWidth,
                this.preferredLineHeight - 2.0 * _kCaretHeightOffset);
            this._selectionRects = null;

            var textPainterSize = this._textPainter.size;
            this.size = new Size(this.constraints.maxWidth,
                this.constraints.constrainHeight(this._preferredHeight(this.constraints.maxWidth)));
            var contentSize = new Size(textPainterSize.width + _kCaretGap + _kCaretWidth,
                textPainterSize.height);
            var _maxScrollExtent = this._getMaxScrollExtend(contentSize);
            this._hasVisualOverflow = _maxScrollExtent > 0.0;
            this.offset.applyViewportDimension(this._viewportExtend);
            this.offset.applyContentDimensions(0.0, _maxScrollExtent);
        }

        public override void paint(PaintingContext context, Offset offset) {
            this._layoutText(this.constraints.maxWidth);
            if (this._hasVisualOverflow) {
                context.pushClipRect(this.needsCompositing, offset, Offset.zero & this.size, this._paintContents);
            }
            else {
                this._paintContents(context, offset);
            }
        }

        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        protected void markNeedsTextLayout() {
            this._textLayoutLastWidth = null;
            this.markNeedsLayout();
        }

        // describeSemanticsConfiguration todo


        void _paintCaret(Canvas canvas, Offset effectiveOffset) {
            D.assert(this._textLayoutLastWidth == this.constraints.maxWidth);
            var caretOffset = this._textPainter.getOffsetForCaret(this._selection.extendPos, this._caretPrototype);
            var paint = new Paint() {color = this._cursorColor};
            var caretRec = this._caretPrototype.shift(caretOffset + effectiveOffset);
            canvas.drawRect(caretRec, paint);
            if (!caretRec.Equals(this._lastCaretRect)) {
                this._lastCaretRect = caretRec;
                if (this.onCaretChanged != null) {
                    this.onCaretChanged(caretRec);
                }
            }
        }

        void _paintSelection(Canvas canvas, Offset effectiveOffset) {
            D.assert(this._textLayoutLastWidth == this.constraints.maxWidth);
            D.assert(this._selectionRects != null);
            var paint = new Paint() {color = this._selectionColor};

            foreach (var box in this._selectionRects) {
                canvas.drawRect(box.toRect().shift(effectiveOffset), paint);
            }
        }

        void _paintContents(PaintingContext context, Offset offset) {
            D.assert(this._textLayoutLastWidth == this.constraints.maxWidth);
            var effectiveOffset = offset + this._paintOffset;

            if (this._selection != null && this._selection.isValid) {
                if (this._selection.isCollapsed && this._showCursor.value && this.cursorColor != null) {
                    this._paintCaret(context.canvas, effectiveOffset);
                }
                else if (!this._selection.isCollapsed && this._selectionColor != null) {
                    this._selectionRects =
                        this._selectionRects ?? this._textPainter.getBoxesForSelection(this._selection);
                    this._paintSelection(context.canvas, effectiveOffset);
                }
            }

            if (this._hasFocus) {
                var caretOffset = this._textPainter.getOffsetForCaret(this._selection.extendPos,
                    Rect.fromLTWH(0, 0, 1, this.preferredLineHeight));
                var caretRec = this._caretPrototype.shift(caretOffset + effectiveOffset);
                Input.compositionCursorPos = new Vector2((float) caretRec.left, (float) caretRec.bottom);
            }

            this._textPainter.paint(context.canvas, effectiveOffset);
        }

        void _handleSetSelection(TextSelection selection) {
            this.onSelectionChanged(selection, this, SelectionChangedCause.keyboard);
        }

        void _handleTapDown(TapDownDetails details) {
            D.assert(!this.ignorePointer);
            this.handleTapDown(details);
        }

        void _handleTap() {
            D.assert(!this.ignorePointer);
            this.handleTap();
        }

        void _handleDoubleTap(DoubleTapDetails details) {
            D.assert(!this.ignorePointer);
            this.handleDoubleTap(details);
        }

        void _handleLongPress() {
            D.assert(!this.ignorePointer);
            this.handleLongPress();
        }

        void markNeedsSemanticsUpdate() {
            // todo
        }

        double _preferredHeight(double width) {
            if (this.maxLines != null) {
                return this.preferredLineHeight * this.maxLines.Value;
            }

            if (double.IsInfinity(width)) {
                var text = this._textPainter.text.text;
                int lines = 1;
                for (int index = 0; index < text.Length; ++index) {
                    if (text[index] == 0x0A) {
                        lines += 1;
                    }
                }

                return this.preferredLineHeight * lines;
            }

            this._layoutText(width);
            return Math.Max(this.preferredLineHeight, this._textPainter.height);
        }

        void _layoutText(double constraintWidth) {
            if (this._textLayoutLastWidth == constraintWidth) {
                return;
            }

            var caretMargin = _kCaretGap + _kCaretWidth;
            var avialableWidth = Math.Max(0.0, constraintWidth - caretMargin);
            var maxWidth = this._isMultiline ? avialableWidth : double.PositiveInfinity;
            this._textPainter.layout(minWidth: avialableWidth, maxWidth: maxWidth);
            this._textLayoutLastWidth = constraintWidth;
        }

        TextSelection _selectWordAtOffset(TextPosition position) {
            D.assert(this._textLayoutLastWidth == this.constraints.maxWidth);
            var word = this._textPainter.getWordBoundary(position);
            if (position.offset >= word.end) {
                return TextSelection.fromPosition(position);
            }

            return new TextSelection(baseOffset: word.start, extentOffset: word.end);
        }

        bool _isMultiline {
            get { return this._maxLines != 1; }
        }

        Axis _viewportAxis {
            get { return this._isMultiline ? Axis.vertical : Axis.horizontal; }
        }

        Offset _paintOffset {
            get {
                switch (this._viewportAxis) {
                    case Axis.horizontal:
                        return new Offset(-this.offset.pixels, 0.0);
                    case Axis.vertical:
                        return new Offset(0.0, -this.offset.pixels);
                }

                return null;
            }
        }

        double _viewportExtend {
            get {
                D.assert(this.hasSize);
                switch (this._viewportAxis) {
                    case Axis.horizontal:
                        return this.size.width;
                    case Axis.vertical:
                        return this.size.height;
                }

                return 0.0;
            }
        }

        double _getMaxScrollExtend(Size contentSize) {
            D.assert(this.hasSize);
            switch (this._viewportAxis) {
                case Axis.horizontal:
                    return Math.Max(0.0, contentSize.width - this.size.width);
                case Axis.vertical:
                    return Math.Max(0.0, contentSize.height - this.size.height);
            }

            return 0.0;
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Color>("cursorColor", this.cursorColor));
            properties.add(new DiagnosticsProperty<ValueNotifier<bool>>("showCursor", this.showCursor));
            properties.add(new DiagnosticsProperty<int?>("maxLines", this.maxLines));
            properties.add(new DiagnosticsProperty<Color>("selectionColor", this.selectionColor));
            properties.add(new DiagnosticsProperty<double>("textScaleFactor", this.textScaleFactor));
            properties.add(new DiagnosticsProperty<TextSelection>("selection", this.selection));
            properties.add(new DiagnosticsProperty<ViewportOffset>("offset", this.offset));
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            return new List<DiagnosticsNode> {
                this.text.toDiagnosticsNode(
                    name: "text",
                    style: DiagnosticsTreeStyle.transition
                ),
            };
        }
    }
}