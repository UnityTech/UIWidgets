using System;
using System.Collections.Generic;
using UIWidgets.foundation;
using UIWidgets.service;
using UIWidgets.ui;
using UnityEngine;
using Canvas = UIWidgets.ui.Canvas;
using Rect = UIWidgets.ui.Rect;

namespace UIWidgets.painting
{
    public class TextPainter
    {
        private TextSpan _text;
        private TextAlign _textAlign;
        private TextDirection? _textDirection;
        private double _textScaleFactor;
        private Paragraph _layoutTemplate;
        private Paragraph _paragraph;
        private bool _needsLayout = true;
        private int _maxLines;
        private string _ellipsis;
        private double _lastMinWidth;
        private double _lastMaxWidth;

        public TextPainter(TextSpan text,
            TextAlign textAlign = TextAlign.left,
            TextDirection textDirection = TextDirection.ltr,
            double textScaleFactor = 1.0,
            int maxLines = 0,
            string ellipsis = "")
        {
            _text = text;
            _textAlign = textAlign;
            _textDirection = textDirection;
            _textScaleFactor = textScaleFactor;
            _maxLines = maxLines;
            _ellipsis = ellipsis;
        }


        public double textScaleFactor
        {
            get { return _textScaleFactor; }
            set
            {
                if (_textScaleFactor == value)
                    return;
                _textScaleFactor = value;
                _paragraph = null;
                _layoutTemplate = null;
                _needsLayout = true;
            }
        }

        public string ellipsis
        {
            get { return _ellipsis; }
            set
            {
                if (_ellipsis == value)
                {
                    return;
                }

                _ellipsis = value;
                _paragraph = null;
                _needsLayout = true;
            }
        }

        public TextSpan text
        {
            get { return _text; }
            set
            {
                if (text.Equals(value))
                {
                    return;
                }

                if (!Equals(_text == null ? null : _text.style, value == null ? null : value.style))
                {
                    _layoutTemplate = null;
                }

                _text = value;
                _paragraph = null;
                _needsLayout = true;
            }
        }

        public Size size
        {
            get
            {
                Debug.Assert(!_needsLayout);
                return new Size(width, height);
            }
        }

        public TextDirection? textDirection
        {
            get { return _textDirection; }
            set
            {
                if (textDirection == value)
                {
                    return;
                }

                _textDirection = value;
                _paragraph = null;
                _layoutTemplate = null;
                _needsLayout = true;
            }
        }

        public TextAlign textAlign
        {
            get { return _textAlign; }
            set
            {
                if (_textAlign == value)
                {
                    return;
                }

                _textAlign = value;
                _paragraph = null;
                _needsLayout = true;
            }
        }

        public bool didExceedMaxLines
        {
            get
            {
                Debug.Assert(!_needsLayout);
                return _paragraph.didExceedMaxLines;
            }
        }

        public int maxLines
        {
            get { return _maxLines; }
            set
            {
                if (_maxLines == value)
                {
                    return;
                }

                _maxLines = value;
                _paragraph = null;
                _needsLayout = true;
            }
        }

        public double minIntrinsicWidth
        {
            get
            {
                Debug.Assert(!_needsLayout);
                return _applyFloatingPointHack(_paragraph.minIntrinsicWidth);
            }
        }

        public double maxIntrinsicWidth
        {
            get
            {
                Debug.Assert(!_needsLayout);
                return _applyFloatingPointHack(_paragraph.maxIntrinsicWidth);
            }
        }

        public double height
        {
            get
            {
                Debug.Assert(!_needsLayout);
                return _applyFloatingPointHack(_paragraph.height);
            }
        }

        public double width
        {
            get
            {
                Debug.Assert(!_needsLayout);
                return _applyFloatingPointHack(_paragraph.width);
            }
        }

        public double computeDistanceToActualBaseline(TextBaseline baseline)
        {
            Debug.Assert(!_needsLayout);
            switch (baseline)
            {
                    case TextBaseline.alphabetic:
                        return _paragraph.alphabeticBaseline;
                    case TextBaseline.ideographic:
                        return _paragraph.ideographicBaseline;
            }
            return 0.0;
        }

        public void layout(double minWidth = 0.0, double maxWidth = double.PositiveInfinity)
        {
            Debug.Assert(text != null, "TextPainter.text must be set to a non-null value before using the TextPainter.");
            Debug.Assert(textDirection != null, "TextPainter.textDirection must be set to a non-null value before using the TextPainter.");
            if (!_needsLayout && minWidth == _lastMinWidth && maxWidth == _lastMaxWidth)
            {
                return;
            }
            
            _needsLayout = false;
            if (_paragraph == null)
            {
                var builder = new ParagraphBuilder(_createParagraphStyle());
                _text.build(builder, textScaleFactor);
                _paragraph = builder.build();
            }

            _lastMinWidth = minWidth;
            _lastMaxWidth = maxWidth;
            _paragraph.layout(new ParagraphConstraints(maxWidth));
  
            if (minWidth != maxWidth)
            {
                var newWidth = MathUtils.clamp(maxIntrinsicWidth, minWidth, maxWidth);
                if (newWidth != width)
                {
                    _paragraph.layout(new ParagraphConstraints(newWidth));
                }
            }
        }

        public void paint(Canvas canvas, Offset offset)
        {
            Debug.Assert(!_needsLayout);
            _paragraph.paint(canvas, offset.dx, offset.dy);
        }

        public Offset getOffsetForCaret(TextPosition position, Rect caretPrototype)
        {
            D.assert(!_needsLayout);
            var offset = position.offset;
            if (offset > 0)
            {
                var prevCodeUnit = _text.codeUnitAt(offset);
                if (prevCodeUnit == null) // out of upper bounds
                {
                    var rectNextLine = _paragraph.getNextLineStartRect();
                    if (rectNextLine != null)
                    {
                        return new Offset(rectNextLine.start, rectNextLine.top);
                    }
                }
            }
            
            switch (position.affinity)
            {
                    case TextAffinity.upstream:
                        return _getOffsetFromUpstream(offset, caretPrototype) ??
                               _getOffsetFromDownstream(offset, caretPrototype) ?? _emptyOffset;  
                    case TextAffinity.downstream:
                        return _getOffsetFromDownstream(offset, caretPrototype) ??
                               _getOffsetFromUpstream(offset, caretPrototype) ?? _emptyOffset;
            }

            return null;
        }

        public Paragraph.LineRange getLineRange(int lineNumber)
        {
            D.assert(!_needsLayout);
            return _paragraph.getLineRange(lineNumber);
        }
        
        public Paragraph.LineRange getLineRange(TextPosition textPosition)
        {
            return getLineRange(getLineIndex(textPosition));
        }
        
        public List<TextBox> getBoxesForSelection(TextSelection selection)
        {
            D.assert(!_needsLayout);
            var results =  _paragraph.getRectsForRange(selection.start, selection.end);
            return results;
        }
        
        public TextPosition getPositionForOffset(Offset offset) {
            D.assert(!_needsLayout);
            var result = _paragraph.getGlyphPositionAtCoordinate(offset.dx, offset.dy);
            return new TextPosition(result.position, result.affinity);
        }

        public TextRange getWordBoundary(TextPosition position)
        {
            D.assert(!_needsLayout);
            var range = _paragraph.getWordBoundary(position.offset);
            return new TextRange(range.start, range.end);
        }
        
        public TextPosition getPositionVerticalMove(TextPosition position, int move)
        {
            D.assert(!_needsLayout);
            var offset = getOffsetForCaret(position, Rect.zero);
            var lineIndex = Math.Min(Math.Max(_paragraph.getLine(position) + move, 0), _paragraph.getLineCount());
            var targetLineStart = _paragraph.getLineRange(lineIndex).start;
            var newLineOffset = getOffsetForCaret(new TextPosition(targetLineStart), Rect.zero);
            return getPositionForOffset(new Offset(offset.dx, newLineOffset.dy));
        }

        public int getLineIndex(TextPosition position)
        {
            D.assert(!_needsLayout);
            return _paragraph.getLine(position);
        }

        public int getLineCount()
        {
            D.assert(!_needsLayout);
            return _paragraph.getLineCount();
        }

        public TextPosition getWordRight(TextPosition position)
        {
            D.assert(!_needsLayout);
            var offset = position.offset;
            while(true)
            {
                var range = _paragraph.getWordBoundary(offset);
                if (range.end == range.start)
                {
                    break;
                }
                if (!char.IsWhiteSpace((char)(text.codeUnitAt(range.start)??0)))
                {
                    return new TextPosition(range.end);
                }
                offset = range.end;
            }
            
            return new TextPosition(offset, position.affinity);
        }
        
        public TextPosition getWordLeft(TextPosition position)
        {
            D.assert(!_needsLayout);
            var offset = Math.Max(position.offset - 1, 0);
            while(true)
            {
                var range = _paragraph.getWordBoundary(offset);
                if (!char.IsWhiteSpace((char)(text.codeUnitAt(range.start)??0)))
                {
                    return new TextPosition(range.start);
                }
                offset = Math.Max(range.start - 1, 0);
                if (offset == 0)
                {
                    break;
                }
            }
            
            return new TextPosition(offset, position.affinity);
        }
        
        private ParagraphStyle _createParagraphStyle(TextDirection defaultTextDirection = TextDirection.ltr)
        {
            if (_text.style == null)
            {
                return new ParagraphStyle(
                    textAlign: textAlign,
                    textDirection: textDirection ?? defaultTextDirection,
                    maxLines: maxLines,
                    ellipsis: ellipsis
                );
            }

            return _text.style.getParagraphStyle(textAlign, textDirection ?? defaultTextDirection,
                ellipsis, maxLines, textScaleFactor);
        }

        public double preferredLineHeight
        {
            get
            {
                if (_layoutTemplate == null)
                {
                    var builder = new ParagraphBuilder(
                        _createParagraphStyle(TextDirection.ltr)
                    ); // direction doesn't matter, text is just a space
                    if (text != null && text.style != null)
                    {
                        builder.pushStyle(text.style);
                    }

                    builder.addText(" ");
                    _layoutTemplate = builder.build();
                    _layoutTemplate.layout(new ParagraphConstraints(double.PositiveInfinity));
                }

                return _layoutTemplate.height;
            }
        }

        private double _applyFloatingPointHack(double layoutValue)
        {
            return Math.Ceiling(layoutValue);
        }
        
        
        private Offset _getOffsetFromUpstream(int offset, Rect caretPrototype) {
            var prevCodeUnit = _text.codeUnitAt(offset - 1);
            if (prevCodeUnit == null)
                return null;
            var  prevRuneOffset = _isUtf16Surrogate((int)prevCodeUnit) ? offset - 2 : offset - 1;
            var boxes = _paragraph.getRectsForRange(prevRuneOffset, offset);
            if (boxes.Count == 0)
                return null;
            var box = boxes[0];
            var caretEnd = box.end;
            var dx = box.direction == TextDirection.rtl ? caretEnd : caretEnd - caretPrototype.width;
            return new Offset(dx, box.top);
        }

        private Offset _getOffsetFromDownstream(int offset, Rect caretPrototype) {
            var nextCodeUnit = _text.codeUnitAt(offset);
            if (nextCodeUnit == null)
                return null;
            var nextRuneOffset = _isUtf16Surrogate((int)nextCodeUnit) ? offset + 2 : offset + 1;
            var boxes = _paragraph.getRectsForRange(offset, nextRuneOffset);
            if (boxes.Count == 0)
                return null;
            var box = boxes[0];
            var caretStart = box.start;
            var dx = box.direction == TextDirection.rtl ? caretStart - caretPrototype.width : caretStart;
            return new Offset(dx, box.top);
        }

        private Offset _emptyOffset
        {
            get
            {
                D.assert(!_needsLayout);
                switch (textAlign)
                {
                        case TextAlign.left:
                            return Offset.zero;
                        case TextAlign.right:
                            return new Offset(width, 0.0);
                        case TextAlign.center:
                            return new Offset(width / 2.0, 0.0);
                        case TextAlign.justify:
                            if (textDirection == TextDirection.rtl)
                            {
                                return new Offset(width, 0.0);
                            }
                            return Offset.zero;
                }
                return null;
            }
        }
        private static bool _isUtf16Surrogate(int value)
        {
            return (value & 0xF800) == 0xD800;
        }

    }
}