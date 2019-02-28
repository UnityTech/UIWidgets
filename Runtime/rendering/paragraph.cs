using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.rendering {
    public enum TextOverflow {
        /// Clip the overflowing text to fix its container.
        clip,

        /// Fade the overflowing text to transparent.
        fade,

        /// Use an ellipsis to indicate that the text has overflowed.
        ellipsis,
    }


    public class RenderParagraph : RenderBox {
        static readonly string _kEllipsis = "\u2026";

        bool _softWrap;

        TextOverflow _overflow;
        readonly TextPainter _textPainter;
        bool _hasVisualOverflow = false;

        public RenderParagraph(TextSpan text,
            TextAlign textAlign = TextAlign.left,
            TextDirection textDirection = TextDirection.ltr,
            bool softWrap = true,
            TextOverflow overflow = TextOverflow.clip,
            float textScaleFactor = 1.0f,
            int? maxLines = null
        ) {
            D.assert(maxLines == null || maxLines > 0);
            this._softWrap = softWrap;
            this._overflow = overflow;
            this._textPainter = new TextPainter(
                text,
                textAlign,
                textDirection,
                textScaleFactor,
                maxLines,
                overflow == TextOverflow.ellipsis ? _kEllipsis : ""
            );
        }

        public TextSpan text {
            get { return this._textPainter.text; }

            set {
                Debug.Assert(value != null);
                switch (this._textPainter.text.compareTo(value)) {
                    case RenderComparison.identical:
                    case RenderComparison.metadata:
                        return;
                    case RenderComparison.paint:
                        this._textPainter.text = value;
                        this.markNeedsPaint();
                        break;
                    case RenderComparison.layout:
                        this._textPainter.text = value;
                        this.markNeedsLayout();
                        break;
                }
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

                this._textPainter.textDirection = this.textDirection;
                this.markNeedsLayout();
            }
        }

        public bool softWrap {
            get { return this._softWrap; }
            set {
                if (this._softWrap == value) {
                    return;
                }

                this._softWrap = value;
                this.markNeedsLayout();
            }
        }

        public TextOverflow overflow {
            get { return this._overflow; }
            set {
                if (this._overflow == value) {
                    return;
                }

                this._overflow = value;
                this._textPainter.ellipsis = value == TextOverflow.ellipsis ? _kEllipsis : null;
                // _textPainter.e
                this.markNeedsLayout();
            }
        }

        public float textScaleFactor {
            get { return this._textPainter.textScaleFactor; }
            set {
                if (Mathf.Abs(this._textPainter.textScaleFactor - value) < 0.00000001) {
                    return;
                }

                this._textPainter.textScaleFactor = value;
                this.markNeedsLayout();
            }
        }

        public int? maxLines {
            get { return this._textPainter.maxLines; }
            set {
                D.assert(this.maxLines == null || this.maxLines > 0);
                if (this._textPainter.maxLines == value) {
                    return;
                }

                this._textPainter.maxLines = value;
                this.markNeedsLayout();
            }
        }

        public Size textSize {
            get { return this._textPainter.size; }
        }

        protected override float computeMinIntrinsicWidth(float height) {
            this._layoutText();
            return this._textPainter.minIntrinsicWidth;
        }

        protected override float computeMaxIntrinsicWidth(float height) {
            this._layoutText();
            return this._textPainter.maxIntrinsicWidth;
        }

        float _computeIntrinsicHeight(float width) {
            this._layoutText(minWidth: width, maxWidth: width);
            return this._textPainter.height;
        }

        protected override float computeMinIntrinsicHeight(float width) {
            return this._computeIntrinsicHeight(width);
        }

        protected override float computeMaxIntrinsicHeight(float width) {
            return this._computeIntrinsicHeight(width);
        }

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            this._layoutTextWithConstraints(this.constraints);
            return this._textPainter.computeDistanceToActualBaseline(baseline);
        }


        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        protected override void performLayout() {
            this._layoutTextWithConstraints(this.constraints);
            var textSize = this._textPainter.size;
            var didOverflowHeight = this._textPainter.didExceedMaxLines;
            this.size = this.constraints.constrain(textSize);

            var didOverflowWidth = this.size.width < textSize.width;
            this._hasVisualOverflow = didOverflowWidth || didOverflowHeight;
        }

        public override void paint(PaintingContext context, Offset offset) {
            this._layoutTextWithConstraints(this.constraints);
            var canvas = context.canvas;

            if (this._hasVisualOverflow) {
                var bounds = offset & this.size;
                canvas.save();
                canvas.clipRect(bounds);
            }

            this._textPainter.paint(canvas, offset);
            if (this._hasVisualOverflow) {
                canvas.restore();
            }
        }

        void _layoutText(float minWidth = 0.0f, float maxWidth = float.PositiveInfinity) {
            var widthMatters = this.softWrap || this.overflow == TextOverflow.ellipsis;
            this._textPainter.layout(minWidth, widthMatters ? maxWidth : float.PositiveInfinity);
        }

        void _layoutTextWithConstraints(BoxConstraints constraints) {
            this._layoutText(minWidth: constraints.minWidth, maxWidth: constraints.maxWidth);
        }

        public override List<DiagnosticsNode> debugDescribeChildren() {
            return new List<DiagnosticsNode> {
                this.text.toDiagnosticsNode(name: "text", style: DiagnosticsTreeStyle.transition)
            };
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new EnumProperty<TextAlign>("textAlign", this.textAlign));
            properties.add(new EnumProperty<TextDirection?>("textDirection", this.textDirection));
            properties.add(new FlagProperty("softWrap", value: this.softWrap, ifTrue: "wrapping at box width",
                ifFalse: "no wrapping except at line break characters", showName: true));
            properties.add(new EnumProperty<TextOverflow>("overflow", this.overflow));
            properties.add(new FloatProperty("textScaleFactor", this.textScaleFactor, defaultValue: 1.0f));
            properties.add(new IntProperty("maxLines", this.maxLines, ifNull: "unlimited"));
        }
    }
}