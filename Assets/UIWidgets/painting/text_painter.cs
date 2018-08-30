using System;
using System.Runtime.ConstrainedExecution;
using UIWidgets.math;
using UIWidgets.ui;
using UnityEngine;
using Canvas = UIWidgets.ui.Canvas;

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
            if (_needsLayout && minWidth == _lastMaxWidth && maxWidth == _lastMaxWidth)
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
            _lastMaxWidth = minWidth;
            _paragraph.layout(new ParagraphConstraints(maxWidth));
  
            if (minWidth != maxWidth)
            {
                var newWidth = MathUtil.Clamp(maxIntrinsicWidth, minWidth, maxWidth);
                if (newWidth != width)
                {
                    _paragraph.layout(new ParagraphConstraints(newWidth));
                }
            }
        }

        public void paint(Canvas canvas, Offset offset)
        {
            Debug.Assert(_needsLayout);
            _paragraph.paint(canvas, offset.dx, offset.dy);
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
                        builder.pushStyle(text.style.getTextStyle(textScaleFactor));
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
    }
}