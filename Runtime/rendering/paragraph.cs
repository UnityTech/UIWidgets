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
    public enum TextOverflow {
        /// Clip the overflowing text to fix its container.
        clip,

        /// Fade the overflowing text to transparent.
        fade,

        /// Use an ellipsis to indicate that the text has overflowed.
        ellipsis,
        
        /// Render overflowing text outside of its container.
        visible,
    }


    public class RenderParagraph : RenderBox {
        static readonly string _kEllipsis = "\u2026";

        bool _softWrap;

        TextOverflow _overflow;
        readonly TextPainter _textPainter;
        bool _needsClipping = false;

        List<TextBox> _selectionRects;

        public RenderParagraph(TextSpan text,
            TextAlign textAlign = TextAlign.left,
            TextDirection textDirection = TextDirection.ltr,
            bool softWrap = true,
            TextOverflow overflow = TextOverflow.clip,
            float textScaleFactor = 1.0f,
            int? maxLines = null,
            StrutStyle strutStyle = null,
            Action onSelectionChanged = null,
            Color selectionColor = null
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
                overflow == TextOverflow.ellipsis ? _kEllipsis : "",
                strutStyle: strutStyle
            );

            this._selection = null;
            this.onSelectionChanged = onSelectionChanged;
            this.selectionColor = selectionColor;

            this._resetHoverHandler();
        }

        public Action onSelectionChanged;
        public Color selectionColor;

        public TextSelection selection {
            get { return this._selection; }
            set {
                if (this._selection == value) {
                    return;
                }

                this._selection = value;
                this._selectionRects = null;
                this.markNeedsPaint();
            }
        }

        public TextSpan text {
            get { return this._textPainter.text; }

            set {
                Debug.Assert(value != null);
                switch (this._textPainter.text.compareTo(value)) {
                    case RenderComparison.identical:
                    case RenderComparison.metadata:
                        return;
                    case RenderComparison.function:
                        this._textPainter.text = value;
                        this.markNeedsPaint();
                        break;
                    case RenderComparison.paint:
                        this._textPainter.text = value;
                        this.markNeedsPaint();
                        break;
                    case RenderComparison.layout:
                        this._textPainter.text = value;
                        this.markNeedsLayout();
                        break;
                }

                this._resetHoverHandler();
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

        protected Offset getOffsetForCaret(TextPosition position, Rect caretPrototype) {
            D.assert(this._textPainter != null);
            return this._textPainter.getOffsetForCaret(position, caretPrototype);
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

        protected internal override float computeMaxIntrinsicHeight(float width) {
            return this._computeIntrinsicHeight(width);
        }

        protected override float? computeDistanceToActualBaseline(TextBaseline baseline) {
            this._layoutTextWithConstraints(this.constraints);
            return this._textPainter.computeDistanceToActualBaseline(baseline);
        }


        protected override bool hitTestSelf(Offset position) {
            return true;
        }

        bool _hasFocus = false;
        bool _listenerAttached = false;

        public bool hasFocus {
            get { return this._hasFocus; }
            set {
                if (this._hasFocus == value) {
                    return;
                }

                this._hasFocus = value;

                if (this._hasFocus) {
                    D.assert(!this._listenerAttached);
                    RawKeyboard.instance.addListener(this._handleKeyEvent);
                    this._listenerAttached = true;
                }
                else {
                    this.selection = null;
                    D.assert(this._listenerAttached);
                    RawKeyboard.instance.removeListener(this._handleKeyEvent);
                    this._listenerAttached = false;
                }
            }
        }

        TextSpan _previousHoverSpan;

#pragma warning disable 0414
        bool _pointerHoverInside;
#pragma warning restore 0414
        bool _hasHoverRecognizer;
        MouseTrackerAnnotation _hoverAnnotation;

        void _resetHoverHandler() {
            this._hasHoverRecognizer = this._textPainter.text.hasHoverRecognizer;
            this._previousHoverSpan = null;
            this._pointerHoverInside = false;

            if (this._hoverAnnotation != null && this.attached) {
                RendererBinding.instance.mouseTracker.detachAnnotation(this._hoverAnnotation);
            }

            if (this._hasHoverRecognizer) {
                this._hoverAnnotation = new MouseTrackerAnnotation(
                    onEnter: this._onPointerEnter,
                    onHover: this._onPointerHover,
                    onExit: this._onPointerExit);

                if (this.attached) {
                    RendererBinding.instance.mouseTracker.attachAnnotation(this._hoverAnnotation);
                }
            }
            else {
                this._hoverAnnotation = null;
            }
        }

        void _handleKeyEvent(RawKeyEvent keyEvent) {
            //only allow KCommand.copy
            if (keyEvent is RawKeyUpEvent) {
                return;
            }

            if (this.selection.isCollapsed) {
                return;
            }

            KeyCode pressedKeyCode = keyEvent.data.unityEvent.keyCode;
            int modifiers = (int) keyEvent.data.unityEvent.modifiers;
            bool ctrl = (modifiers & (int) EventModifiers.Control) > 0;
            bool cmd = (modifiers & (int) EventModifiers.Command) > 0;
            bool cKey = pressedKeyCode == KeyCode.C;
            bool isMac = SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX;

            KeyCommand? kcmd = keyEvent is RawKeyCommandEvent
                ? ((RawKeyCommandEvent) keyEvent).command
                : ((ctrl || (isMac && cmd)) && cKey)
                    ? KeyCommand.Copy
                    : (KeyCommand?) null;

            if (kcmd == KeyCommand.Copy) {
                Clipboard.setData(
                    new ClipboardData(text: this.selection.textInside(this.text.toPlainText()))
                );
            }
        }

        public override void attach(object owner) {
            base.attach(owner);
            if (this._hoverAnnotation != null) {
                RendererBinding.instance.mouseTracker.attachAnnotation(this._hoverAnnotation);
            }
        }

        public override void detach() {
            if (this._listenerAttached) {
                RawKeyboard.instance.removeListener(this._handleKeyEvent);
            }

            base.detach();
            if (this._hoverAnnotation != null) {
                RendererBinding.instance.mouseTracker.detachAnnotation(this._hoverAnnotation);
            }
        }

        TextSelection _selection;

        public void selectPositionAt(Offset from = null, Offset to = null, SelectionChangedCause? cause = null) {
            D.assert(cause != null);
            D.assert(from != null);
            if (true) {
                TextPosition fromPosition =
                    this._textPainter.getPositionForOffset(this.globalToLocal(from));
                TextPosition toPosition = to == null
                    ? null
                    : this._textPainter.getPositionForOffset(this.globalToLocal(to));

                int baseOffset = fromPosition.offset;
                int extentOffset = fromPosition.offset;
                if (toPosition != null) {
                    baseOffset = Mathf.Min(fromPosition.offset, toPosition.offset);
                    extentOffset = Mathf.Max(fromPosition.offset, toPosition.offset);
                }

                TextSelection newSelection = new TextSelection(
                    baseOffset: baseOffset,
                    extentOffset: extentOffset,
                    affinity: fromPosition.affinity);

                if (newSelection != this._selection) {
                    this._handleSelectionChanged(newSelection, cause.Value);
                }
            }
        }


        void _handleSelectionChanged(TextSelection selection,
            SelectionChangedCause cause) {
            this.selection = selection;
            this.onSelectionChanged?.Invoke();
        }

        void _onPointerEnter(PointerEvent evt) {
            this._pointerHoverInside = true;
        }

        void _onPointerExit(PointerEvent evt) {
            this._pointerHoverInside = false;
            this._previousHoverSpan?.hoverRecognizer?.OnPointerLeave?.Invoke();
            this._previousHoverSpan = null;
        }

        void _onPointerHover(PointerEvent evt) {
            this._layoutTextWithConstraints(this.constraints);
            Offset offset = this.globalToLocal(evt.position);
            TextPosition position = this._textPainter.getPositionForOffset(offset);
            TextSpan span = this._textPainter.text.getSpanForPosition(position);

            if (this._previousHoverSpan != span) {
                this._previousHoverSpan?.hoverRecognizer?.OnPointerLeave?.Invoke();
                span?.hoverRecognizer?.OnPointerEnter?.Invoke((PointerHoverEvent) evt);
                this._previousHoverSpan = span;
            }
        }

        public override void handleEvent(PointerEvent evt, HitTestEntry entry) {
            D.assert(this.debugHandleEvent(evt, entry));
            if (!(evt is PointerDownEvent)) {
                return;
            }
            
            this._layoutTextWithConstraints(this.constraints);
            Offset offset = ((BoxHitTestEntry) entry).localPosition;
            TextPosition position = this._textPainter.getPositionForOffset(offset);
            TextSpan span = this._textPainter.text.getSpanForPosition(position);
            span?.recognizer?.addPointer((PointerDownEvent) evt);
        }

        protected override void performLayout() {
            this._layoutTextWithConstraints(this.constraints);
            var textSize = this._textPainter.size;
            var textDidExceedMaxLines = this._textPainter.didExceedMaxLines;
            this.size = this.constraints.constrain(textSize);

            var didOverflowHeight = this.size.height < textSize.height || textDidExceedMaxLines;
            var didOverflowWidth = this.size.width < textSize.width;
            var hasVisualOverflow = didOverflowWidth || didOverflowHeight;
            if (hasVisualOverflow) {
                switch (this._overflow) {
                case TextOverflow.visible:
                    this._needsClipping = false;
                    break;
                case TextOverflow.clip:
                case TextOverflow.ellipsis:
                case TextOverflow.fade:
                    this._needsClipping = true;
                    break;
                }
            }
            else {
                this._needsClipping = false;
            }

            this._selectionRects = null;
        }


        void paintParagraph(PaintingContext context, Offset offset) {
            this._layoutTextWithConstraints(this.constraints);
            var canvas = context.canvas;

            if (this._needsClipping) {
                var bounds = offset & this.size;
                canvas.save();
                canvas.clipRect(bounds);
            }

            if (this._selection != null && this.selectionColor != null && this._selection.isValid) {
                if (!this._selection.isCollapsed) {
                    this._selectionRects =
                        this._selectionRects ?? this._textPainter.getBoxesForSelection(this._selection);
                    this._paintSelection(canvas, offset);
                }
            }

            this._textPainter.paint(canvas, offset);
            if (this._needsClipping) {
                canvas.restore();
            }
        }

        public override void paint(PaintingContext context, Offset offset) {
            if (this._hoverAnnotation != null) {
                AnnotatedRegionLayer<MouseTrackerAnnotation> layer = new AnnotatedRegionLayer<MouseTrackerAnnotation>(
                    this._hoverAnnotation, size: this.size, offset: offset);

                context.pushLayer(layer, this.paintParagraph, offset);
            }
            else {
                this.paintParagraph(context, offset);
            }
        }


        void _paintSelection(Canvas canvas, Offset effectiveOffset) {
            D.assert(this._selectionRects != null);
            D.assert(this.selectionColor != null);
            var paint = new Paint {color = this.selectionColor};

            Path barPath = new Path();
            foreach (var box in this._selectionRects) {
                barPath.addRect(box.toRect().shift(effectiveOffset));
            }

            canvas.drawPath(barPath, paint);
        }

        public StrutStyle strutStyle {
            get { return this._textPainter.strutStyle; }
            set {
                if (this._textPainter.strutStyle == value) {
                    return;
                }

                this._textPainter.strutStyle = value;
                this.markNeedsLayout();
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