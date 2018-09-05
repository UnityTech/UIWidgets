using System;
using UIWidgets.painting;
using UIWidgets.ui;
using UnityEngine;

namespace UIWidgets.rendering
{
    public enum TextOverflow {
        /// Clip the overflowing text to fix its container.
        clip,

        /// Fade the overflowing text to transparent.
        fade,

        /// Use an ellipsis to indicate that the text has overflowed.
        ellipsis,
    }

    
    public class RenderParagraph: RenderBox
    {
        
        private static readonly string _kEllipsis = "\u2026";
        
        private bool _softWrap;

        private TextOverflow _overflow;
        private readonly TextPainter _textPainter;
        private bool _hasVisualOverflow = false;
        
        public RenderParagraph(TextSpan text, 
            TextAlign textAlign = TextAlign.left,
            TextDirection textDirection = TextDirection.ltr,
            bool softWrap = true,
            TextOverflow overflow = TextOverflow.clip,
            double textScaleFactor = 1.0,
            int maxLines = 0
            )
        {
            _softWrap = softWrap;
            _overflow = overflow;
            _textPainter = new TextPainter(
                text,
                textAlign,
                textDirection,
                textScaleFactor,
                maxLines,
                overflow == TextOverflow.ellipsis ? _kEllipsis : ""
                );
        }

        public TextSpan text
        {
            get
            {
                return _textPainter.text;
            }

            set
            {
                Debug.Assert(value != null);
                switch (_textPainter.text.compareTo(value))
                {
                        case RenderComparison.identical:
                        case RenderComparison.metadata:
                            return;
                        case RenderComparison.paint:
                            _textPainter.text = value;
                            markNeedsPaint();
                            break;
                        case RenderComparison.layout:
                            _textPainter.text = value;
                            markNeedsLayout();
                            break;
                }
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
                _textPainter.textDirection = textDirection;
                markNeedsLayout();
            }
        }
        
        public bool softWrap
        {
            get { return _softWrap; }
            set
            {
                if (_softWrap == value)
                {
                    return;
                }
                _softWrap = value;
                markNeedsLayout();
            }
        }
        
        public TextOverflow overflow
        {
            get { return _overflow; }
            set
            {
                if (_overflow == value)
                {
                    return;
                }
                _overflow = value;
                _textPainter.ellipsis = value == TextOverflow.ellipsis ? _kEllipsis : null;
                // _textPainter.e
                markNeedsLayout();
            }
        }
        
        public double textScaleFactor
        {
            get { return _textPainter.textScaleFactor; }
            set
            {
                if (Math.Abs(_textPainter.textScaleFactor - value) < 0.00000001)
                {
                    return;
                }
                _textPainter.textScaleFactor = value;
                markNeedsLayout();
            }
        }

        public int maxLines
        {
            get { return _textPainter.maxLines; }
            set
            {
                if (_textPainter.maxLines == value)
                {
                    return;
                }

                _textPainter.maxLines = value;
                markNeedsLayout();
            }
        }

        public Size textSize
        {
            get { return _textPainter.size; }
        }

        public override double computeMinIntrinsicWidth(double height) {
            _layoutText();
            return _textPainter.minIntrinsicWidth;
        }
        
        public override double computeMaxIntrinsicWidth(double height) {
            _layoutText();
            return _textPainter.maxIntrinsicWidth;
        }
        
        double _computeIntrinsicHeight(double width) {
            _layoutText(minWidth: width, maxWidth: width);
            return _textPainter.height;
        }

        public override double  computeMinIntrinsicHeight(double width) {
            return _computeIntrinsicHeight(width);
        }

        public override double  computeMaxIntrinsicHeight(double width) {
            return _computeIntrinsicHeight(width);
        }

        public override double?  computeDistanceToActualBaseline(TextBaseline baseline) {
            _layoutTextWithConstraints(constraints);
            return _textPainter.computeDistanceToActualBaseline(baseline);
        }
        
        
        public override void performLayout() {
            _layoutTextWithConstraints(constraints);
            var textSize = _textPainter.size;
            var didOverflowHeight = _textPainter.didExceedMaxLines;
            size = constraints.constrain(textSize);
            
            var didOverflowWidth = size.width < textSize.width;
            _hasVisualOverflow = didOverflowWidth || didOverflowHeight;
        }
 
        public override void paint(PaintingContext context, Offset offset) {
            _layoutTextWithConstraints(constraints);
            var canvas = context.canvas;
            
            if (_hasVisualOverflow) {
                var bounds = offset & size;
                canvas.save();
                canvas.clipRect(bounds);
            }
            _textPainter.paint(canvas, offset);
            if (_hasVisualOverflow) {
                canvas.restore();
            }
        }
        
        private void _layoutText(double minWidth = 0.0, double maxWidth = double.PositiveInfinity)
        {
            var widthMatters = softWrap || overflow == TextOverflow.ellipsis;
            _textPainter.layout(minWidth, widthMatters ? maxWidth : double.PositiveInfinity);
        }
        
        private void _layoutTextWithConstraints(BoxConstraints constraints) {
            _layoutText(minWidth: constraints.minWidth, maxWidth: constraints.maxWidth);
        }
    }
}