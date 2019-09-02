using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    class _CaretMetrics {
        public _CaretMetrics(Offset offset, float? fullHeight) {
            this.offset = offset;
            this.fullHeight = fullHeight;
        }

        public Offset offset;
        public float? fullHeight;
    }

    public class TextPainter {
        TextSpan _text;
        TextAlign _textAlign;
        TextDirection? _textDirection;
        float _textScaleFactor;
        Paragraph _layoutTemplate;
        Paragraph _paragraph;
        bool _needsLayout = true;
        int? _maxLines;
        string _ellipsis;
        float _lastMinWidth;
        float _lastMaxWidth;

        public TextPainter(TextSpan text = null,
            TextAlign textAlign = TextAlign.left,
            TextDirection textDirection = TextDirection.ltr,
            float textScaleFactor = 1.0f,
            int? maxLines = null,
            string ellipsis = "",
            StrutStyle strutStyle = null) {
            this._text = text;
            this._textAlign = textAlign;
            this._textDirection = textDirection;
            this._textScaleFactor = textScaleFactor;
            this._maxLines = maxLines;
            this._ellipsis = ellipsis;
            this._strutStyle = strutStyle;
        }

        public float textScaleFactor {
            get { return this._textScaleFactor; }
            set {
                if (this._textScaleFactor == value) {
                    return;
                }

                this._textScaleFactor = value;
                this._layoutTemplate = null;
                this._needsLayout = true;
                Paragraph.release(ref this._paragraph);
            }
        }

        public string ellipsis {
            get { return this._ellipsis; }
            set {
                if (this._ellipsis == value) {
                    return;
                }

                this._ellipsis = value;
                Paragraph.release(ref this._paragraph);
                this._needsLayout = true;
            }
        }

        public TextSpan text {
            get { return this._text; }
            set {
                if ((this._text == null && value == null) || (this._text != null && this.text.Equals(value))) {
                    return;
                }

                if (!Equals(this._text == null ? null : this._text.style, value == null ? null : value.style)) {
                    this._layoutTemplate = null;
                }

                this._text = value;
                Paragraph.release(ref this._paragraph);
                this._needsLayout = true;
            }
        }

        public Size size {
            get {
                Debug.Assert(!this._needsLayout);
                return new Size(this.width, this.height);
            }
        }

        public TextDirection? textDirection {
            get { return this._textDirection; }
            set {
                if (this.textDirection == value) {
                    return;
                }

                this._textDirection = value;
                Paragraph.release(ref this._paragraph);
                this._layoutTemplate = null;
                this._needsLayout = true;
            }
        }

        public TextAlign textAlign {
            get { return this._textAlign; }
            set {
                if (this._textAlign == value) {
                    return;
                }

                this._textAlign = value;
                Paragraph.release(ref this._paragraph);
                this._needsLayout = true;
            }
        }

        public bool didExceedMaxLines {
            get {
                Debug.Assert(!this._needsLayout);
                return this._paragraph.didExceedMaxLines;
            }
        }

        public int? maxLines {
            get { return this._maxLines; }
            set {
                if (this._maxLines == value) {
                    return;
                }

                this._maxLines = value;
                Paragraph.release(ref this._paragraph);
                this._needsLayout = true;
            }
        }

        public StrutStyle strutStyle {
            get { return this._strutStyle; }
            set {
                if (this._strutStyle == value) {
                    return;
                }

                this._strutStyle = value;
                this._paragraph = null;
                this._needsLayout = true;
            }
        }

        StrutStyle _strutStyle;

        public float minIntrinsicWidth {
            get {
                Debug.Assert(!this._needsLayout);
                return this._applyFloatingPointHack(this._paragraph.minIntrinsicWidth);
            }
        }

        public float maxIntrinsicWidth {
            get {
                Debug.Assert(!this._needsLayout);
                return this._applyFloatingPointHack(this._paragraph.maxIntrinsicWidth);
            }
        }

        public float height {
            get {
                Debug.Assert(!this._needsLayout);
                return this._applyFloatingPointHack(this._paragraph.height);
            }
        }

        public float width {
            get {
                Debug.Assert(!this._needsLayout);
                return this._applyFloatingPointHack(this._paragraph.width);
            }
        }

        public float computeDistanceToActualBaseline(TextBaseline baseline) {
            Debug.Assert(!this._needsLayout);
            switch (baseline) {
                case TextBaseline.alphabetic:
                    return this._paragraph.alphabeticBaseline;
                case TextBaseline.ideographic:
                    return this._paragraph.ideographicBaseline;
            }

            return 0.0f;
        }

        public void layout(float minWidth = 0.0f, float maxWidth = float.PositiveInfinity) {
            Debug.Assert(this.text != null,
                "TextPainter.text must be set to a non-null value before using the TextPainter.");
            Debug.Assert(this.textDirection != null,
                "TextPainter.textDirection must be set to a non-null value before using the TextPainter.");
            if (!this._needsLayout && minWidth == this._lastMinWidth && maxWidth == this._lastMaxWidth) {
                return;
            }

            this._needsLayout = false;
            if (this._paragraph == null) {
                var builder = new ParagraphBuilder(this._createParagraphStyle());
                this._text.build(builder, this.textScaleFactor);
                this._paragraph = builder.build();
            }

            this._lastMinWidth = minWidth;
            this._lastMaxWidth = maxWidth;
            this._paragraph.layout(new ParagraphConstraints(maxWidth));

            if (minWidth != maxWidth) {
                var newWidth = MathUtils.clamp(this.maxIntrinsicWidth, minWidth, maxWidth);
                if (newWidth != this.width) {
                    this._paragraph.layout(new ParagraphConstraints(newWidth));
                }
            }
        }

        public void paint(Canvas canvas, Offset offset) {
            Debug.Assert(!this._needsLayout);
            this._paragraph.paint(canvas, offset);
        }

        public Offset getOffsetForCaret(TextPosition position, Rect caretPrototype) {
            this._computeCaretMetrics(position, caretPrototype);
            return this._caretMetrics.offset;
        }

        public float? getFullHeightForCaret(TextPosition position, Rect caretPrototype) {
            this._computeCaretMetrics(position, caretPrototype);
            return this._caretMetrics.fullHeight;
        }

        _CaretMetrics _caretMetrics;

        TextPosition _previousCaretPosition;
        Rect _previousCaretPrototype;

        void _computeCaretMetrics(TextPosition position, Rect caretPrototype) {
            D.assert(!this._needsLayout);
            if (position == this._previousCaretPosition && caretPrototype == this._previousCaretPrototype) {
                return;
            }

            var offset = position.offset;
            Rect rect;
            switch (position.affinity) {
                case TextAffinity.upstream:
                    rect = this._getRectFromUpstream(offset, caretPrototype) ??
                           this._getRectFromDownStream(offset, caretPrototype);
                    break;
                case TextAffinity.downstream:
                    rect = this._getRectFromDownStream(offset, caretPrototype) ??
                           this._getRectFromUpstream(offset, caretPrototype);
                    break;
                default:
                    throw new UIWidgetsError("Unknown Position Affinity");
            }

            this._caretMetrics = new _CaretMetrics(
                offset: rect != null ? new Offset(rect.left, rect.top) : this._emptyOffset,
                fullHeight: rect != null ? (float?) (rect.bottom - rect.top) : null);
            
            // Cache the caret position. This was forgot in flutter until https://github.com/flutter/flutter/pull/38821
            this._previousCaretPosition = position;
            this._previousCaretPrototype = caretPrototype;
        }

        public Paragraph.LineRange getLineRange(int lineNumber) {
            D.assert(!this._needsLayout);
            return this._paragraph.getLineRange(lineNumber);
        }

        public Paragraph.LineRange getLineRange(TextPosition textPosition) {
            return this.getLineRange(this.getLineIndex(textPosition));
        }

        public List<TextBox> getBoxesForSelection(TextSelection selection) {
            D.assert(!this._needsLayout);
            var results = this._paragraph.getRectsForRange(selection.start, selection.end);
            return results;
        }

        public TextPosition getPositionForOffset(Offset offset) {
            D.assert(!this._needsLayout);
            var result = this._paragraph.getGlyphPositionAtCoordinate(offset.dx, offset.dy);
            return new TextPosition(result.position, result.affinity);
        }

        public TextRange getWordBoundary(TextPosition position) {
            D.assert(!this._needsLayout);
            var range = this._paragraph.getWordBoundary(position.offset);
            return new TextRange(range.start, range.end);
        }

        public TextPosition getPositionVerticalMove(TextPosition position, int move) {
            D.assert(!this._needsLayout);
            var offset = this.getOffsetForCaret(position, Rect.zero);
            var lineIndex = Mathf.Min(Mathf.Max(this._paragraph.getLine(position) + move, 0),
                this._paragraph.getLineCount() - 1);
            var targetLineStart = this._paragraph.getLineRange(lineIndex).start;
            var newLineOffset = this.getOffsetForCaret(new TextPosition(targetLineStart), Rect.zero);
            return this.getPositionForOffset(new Offset(offset.dx, newLineOffset.dy));
        }

        public int getLineIndex(TextPosition position) {
            D.assert(!this._needsLayout);
            return this._paragraph.getLine(position);
        }

        public int getLineCount() {
            D.assert(!this._needsLayout);
            return this._paragraph.getLineCount();
        }

        public TextPosition getWordRight(TextPosition position) {
            D.assert(!this._needsLayout);
            var offset = position.offset;
            while (true) {
                var range = this._paragraph.getWordBoundary(offset);
                if (range.end == range.start) {
                    break;
                }

                if (!char.IsWhiteSpace((char) (this.text.codeUnitAt(range.start) ?? 0))) {
                    return new TextPosition(range.end);
                }

                offset = range.end;
            }

            return new TextPosition(offset, position.affinity);
        }

        public TextPosition getWordLeft(TextPosition position) {
            D.assert(!this._needsLayout);
            var offset = Mathf.Max(position.offset - 1, 0);
            while (true) {
                var range = this._paragraph.getWordBoundary(offset);
                if (!char.IsWhiteSpace((char) (this.text.codeUnitAt(range.start) ?? 0))) {
                    return new TextPosition(range.start);
                }

                offset = Mathf.Max(range.start - 1, 0);
                if (offset == 0) {
                    break;
                }
            }

            return new TextPosition(offset, position.affinity);
        }

        ParagraphStyle _createParagraphStyle(TextDirection defaultTextDirection = TextDirection.ltr) {
            if (this._text.style == null) {
                return new ParagraphStyle(
                    textAlign: this.textAlign,
                    textDirection: this.textDirection ?? defaultTextDirection,
                    maxLines: this.maxLines,
                    ellipsis: this.ellipsis,
                    strutStyle: this._strutStyle
                );
            }

            return this._text.style.getParagraphStyle(this.textAlign, this.textDirection ?? defaultTextDirection,
                this.ellipsis, this.maxLines, this.textScaleFactor);
        }

        public float preferredLineHeight {
            get {
                if (this._layoutTemplate == null) {
                    var builder = new ParagraphBuilder(this._createParagraphStyle(TextDirection.ltr)
                    ); // direction doesn't matter, text is just a space
                    if (this.text != null && this.text.style != null) {
                        builder.pushStyle(this.text.style, this.textScaleFactor);
                    }

                    builder.addText(" ");
                    this._layoutTemplate = builder.build();
                    this._layoutTemplate.layout(new ParagraphConstraints(float.PositiveInfinity));
                }

                return this._layoutTemplate.height;
            }
        }

        float _applyFloatingPointHack(float layoutValue) {
            return Mathf.Ceil(layoutValue);
        }

        const int _zwjUtf16 = 0x200d;

        Rect _getRectFromUpstream(int offset, Rect caretPrototype) {
            string flattenedText = this._text.toPlainText();
            var prevCodeUnit = this._text.codeUnitAt(Mathf.Max(0, offset - 1));
            if (prevCodeUnit == null) {
                return null;
            }

            bool needsSearch = _isUtf16Surrogate(prevCodeUnit.Value) || this._text.codeUnitAt(offset) == _zwjUtf16;
            int graphemeClusterLength = needsSearch ? 2 : 1;
            List<TextBox> boxes = null;
            while ((boxes == null || boxes.isEmpty()) && flattenedText != null) {
                int prevRuneOffset = offset - graphemeClusterLength;
                boxes = this._paragraph.getRectsForRange(prevRuneOffset, offset);
                if (boxes.isEmpty()) {
                    if (!needsSearch) {
                        break;
                    }

                    if (prevRuneOffset < -flattenedText.Length) {
                        break;
                    }

                    graphemeClusterLength *= 2;
                    continue;
                }

                TextBox box = boxes[0];
                const int NEWLINE_CODE_UNIT = 10;
                if (prevCodeUnit == NEWLINE_CODE_UNIT) {
                    return Rect.fromLTRB(this._emptyOffset.dx, box.bottom,
                        this._emptyOffset.dx, box.bottom + box.bottom - box.top);
                }

                float caretEnd = box.end;
                float dx = box.direction == TextDirection.rtl ? caretEnd - caretPrototype.width : caretEnd;
                return Rect.fromLTRB(Mathf.Min(dx, this.width), box.top,
                    Mathf.Min(dx, this.width), box.bottom);
            }

            return null;
        }

        Rect _getRectFromDownStream(int offset, Rect caretPrototype) {
            string flattenedText = this._text.toPlainText();
            var nextCodeUnit =
                this._text.codeUnitAt(Mathf.Min(offset, flattenedText == null ? 0 : flattenedText.Length - 1));
            if (nextCodeUnit == null) {
                return null;
            }

            bool needsSearch = _isUtf16Surrogate(nextCodeUnit.Value) || nextCodeUnit == _zwjUtf16;
            int graphemeClusterLength = needsSearch ? 2 : 1;
            List<TextBox> boxes = null;
            while ((boxes == null || boxes.isEmpty()) && flattenedText != null) {
                int nextRuneOffset = offset + graphemeClusterLength;
                boxes = this._paragraph.getRectsForRange(offset, nextRuneOffset);
                if (boxes.isEmpty()) {
                    if (!needsSearch) {
                        break;
                    }

                    if (nextRuneOffset >= flattenedText.Length << 1) {
                        break;
                    }

                    graphemeClusterLength *= 2;
                    continue;
                }

                TextBox box = boxes[boxes.Count - 1];
                float caretStart = box.start;
                float dx = box.direction == TextDirection.rtl ? caretStart - caretPrototype.width : caretStart;
                return Rect.fromLTRB(Mathf.Min(dx, this.width), box.top,
                    Mathf.Min(dx, this.width), box.bottom);
            }

            return null;
        }

        Offset _emptyOffset {
            get {
                D.assert(!this._needsLayout);
                switch (this.textAlign) {
                    case TextAlign.left:
                        return Offset.zero;
                    case TextAlign.right:
                        return new Offset(this.width, 0.0f);
                    case TextAlign.center:
                        return new Offset(this.width / 2.0f, 0.0f);
                    case TextAlign.justify:
                        if (this.textDirection == TextDirection.rtl) {
                            return new Offset(this.width, 0.0f);
                        }

                        return Offset.zero;
                }

                return null;
            }
        }

        static bool _isUtf16Surrogate(int value) {
            return (value & 0xF800) == 0xD800;
        }
    }
}